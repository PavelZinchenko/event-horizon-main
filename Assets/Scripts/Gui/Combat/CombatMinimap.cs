using System.Collections.Generic;
using System.Linq;
using Combat.Component.Bullet;
using Combat.Component.Ship;
using Combat.Component.Ship.Effects;
using Combat.Component.Collider;
using Combat.Component.Controller;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using GameDatabase.Enums;
using UnityEngine;
using UnityEngine.UI;
using GameServices.Player;

namespace Gui.Combat
{
    public sealed class CombatMinimap : MonoBehaviour
    {
        private const float BaseRadarRange = 300f;
        private readonly Dictionary<IShip, TargetMarker> _markers = new();
        private readonly Dictionary<IShip, Text> _allyMarkers = new();
        private readonly Dictionary<IUnit, UnitMarker> _projectileMarkers = new();
        private readonly Dictionary<IUnit, Vector2> _lastProjectilePositions = new();
        private readonly HashSet<IUnit> _seenAreaEffects = new();
        private readonly List<TransientMarker> _transientMarkers = new();
        private IScene _scene;
        private RectTransform _map;
        private Text _status;
        private Text _rangeText;
        private RectTransform _root;
        private Text _expandLabel;
        private Toggle _engineThrottleToggle;
        private Slider _speedLimitSlider;
        private Text _speedLimitText;
        private CanvasGroup _canvasGroup;
        private RawImage _staticOverlay;
        private Texture2D _staticTexture;
        private Color32[] _staticPixels;
        private float _nextStaticUpdateTime;
        private bool _expanded;

        public void Initialize(IScene scene)
        {
            _scene = scene;
            var root = GetComponent<RectTransform>();
            _root = root;
            _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            root.anchorMin = root.anchorMax = new Vector2(1f, 0.5f);
            root.pivot = new Vector2(1f, 0.5f);
            root.anchoredPosition = new Vector2(-115f, 55f);
            root.sizeDelta = new Vector2(190f, 196f);

            CreateSpeedLimitSlider(root);

            var panel = NewImage("Map", root, new Color(0.01f, 0.04f, 0.05f, 0.82f));
            panel.raycastTarget = false;
            _map = panel.rectTransform;
            _map.anchorMin = Vector2.zero;
            _map.anchorMax = Vector2.one;
            _map.offsetMin = new Vector2(0f, 26f);
            _map.offsetMax = new Vector2(0f, -32f);

            var throttleRow = new GameObject("EngineThrottle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
            var throttleRect = throttleRow.GetComponent<RectTransform>();
            throttleRect.SetParent(root, false);
            throttleRect.anchorMin = new Vector2(0f, 1f);
            throttleRect.anchorMax = new Vector2(1f, 1f);
            throttleRect.pivot = new Vector2(0.5f, 1f);
            throttleRect.anchoredPosition = new Vector2(0f, 0f);
            throttleRect.sizeDelta = new Vector2(0f, 24f);
            throttleRow.GetComponent<Image>().color = new Color(0.06f, 0.16f, 0.2f, 0.92f);
            _engineThrottleToggle = throttleRow.GetComponent<Toggle>();
            _engineThrottleToggle.isOn = ThreeBodySkillState.EngineThrottleEnabled;
            _engineThrottleToggle.targetGraphic = throttleRow.GetComponent<Image>();

            var throttleCheck = NewImage("Checkmark", throttleRect, new Color(0.2f, 0.95f, 1f, 0.96f));
            throttleCheck.rectTransform.anchorMin = throttleCheck.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            throttleCheck.rectTransform.pivot = new Vector2(0f, 0.5f);
            throttleCheck.rectTransform.anchoredPosition = new Vector2(6f, 0f);
            throttleCheck.rectTransform.sizeDelta = new Vector2(14f, 14f);
            throttleCheck.raycastTarget = false;
            _engineThrottleToggle.graphic = throttleCheck;
            throttleCheck.enabled = _engineThrottleToggle.isOn;
            _engineThrottleToggle.onValueChanged.AddListener(value =>
            {
                ThreeBodySkillState.SetEngineThrottle(value);
                throttleCheck.enabled = value;
            });

            var throttleLabel = AddText(throttleRect, "引擎节流", 13);
            throttleLabel.alignment = TextAnchor.MiddleLeft;
            throttleLabel.rectTransform.offsetMin = new Vector2(26f, 0f);
            throttleLabel.rectTransform.offsetMax = new Vector2(-6f, 0f);

            var center = NewImage("Player", _map, new Color(0.1f, 1f, 0.25f, 1f));
            center.sprite = MarkerSprite;
            center.raycastTarget = false;
            SetDot(center.rectTransform, Vector2.zero, 7f);
            CreateRadarStaticOverlay();

            var nearest = new GameObject("LockNearest", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var nearestRect = nearest.GetComponent<RectTransform>();
            nearestRect.SetParent(root, false);
            nearestRect.anchorMin = new Vector2(0f, 0f);
            nearestRect.anchorMax = new Vector2(0.4f, 0f);
            nearestRect.offsetMin = Vector2.zero;
            nearestRect.offsetMax = new Vector2(0f, 30f);
            nearest.GetComponent<Image>().color = new Color(0.08f, 0.32f, 0.2f, 0.95f);
            nearest.GetComponent<Button>().onClick.AddListener(LockNearest);
            AddText(nearestRect, "锁定最近", 14);

            var statusObject = new GameObject("Status", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var statusRect = statusObject.GetComponent<RectTransform>();
            statusRect.SetParent(root, false);
            statusRect.anchorMin = new Vector2(0.72f, 0f);
            statusRect.anchorMax = new Vector2(0.88f, 1f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = new Vector2(0f, 30f);
            _status = statusObject.GetComponent<Text>();
            _status.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _status.alignment = TextAnchor.MiddleCenter;
            _status.color = Color.white;
            _status.raycastTarget = false;

            var rangeObject = new GameObject("Range", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var rangeRect = rangeObject.GetComponent<RectTransform>();
            rangeRect.SetParent(root, false);
            rangeRect.anchorMin = new Vector2(0.4f, 0f);
            rangeRect.anchorMax = new Vector2(0.72f, 0f);
            rangeRect.offsetMin = Vector2.zero;
            rangeRect.offsetMax = new Vector2(0f, 30f);
            _rangeText = rangeObject.GetComponent<Text>();
            _rangeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _rangeText.fontSize = 13;
            _rangeText.alignment = TextAnchor.MiddleCenter;
            _rangeText.color = Color.white;
            _rangeText.raycastTarget = false;

            var expand = new GameObject("ExpandRadar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var expandRect = expand.GetComponent<RectTransform>();
            expandRect.SetParent(root, false);
            expandRect.anchorMin = expandRect.anchorMax = new Vector2(1f, 0f);
            expandRect.pivot = new Vector2(1f, 0f);
            expandRect.anchoredPosition = new Vector2(-2f, 2f);
            expandRect.sizeDelta = new Vector2(30f, 28f);
            expand.GetComponent<Image>().color = new Color(0.08f, 0.32f, 0.42f, 0.98f);
            expand.GetComponent<Button>().onClick.AddListener(ToggleExpanded);
            _expandLabel = AddText(expandRect, "↗", 18);
        }

        private void CreateSpeedLimitSlider(RectTransform root)
        {
            var row = new GameObject("EngineSpeedLimit", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.SetParent(root, false);
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0f, 28f);
            rowRect.sizeDelta = new Vector2(0f, 28f);
            row.GetComponent<Image>().color = new Color(0.04f, 0.12f, 0.16f, 0.96f);

            var caption = AddText(rowRect, "限速", 12);
            caption.alignment = TextAnchor.MiddleLeft;
            caption.rectTransform.anchorMax = new Vector2(0.2f, 1f);
            caption.rectTransform.offsetMin = new Vector2(5f, 0f);

            var value = AddText(rowRect, string.Empty, 12);
            value.alignment = TextAnchor.MiddleRight;
            value.rectTransform.anchorMin = new Vector2(0.78f, 0f);
            value.rectTransform.offsetMax = new Vector2(-5f, 0f);
            _speedLimitText = value;

            var sliderObject = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            var sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.SetParent(rowRect, false);
            sliderRect.anchorMin = new Vector2(0.2f, 0.18f);
            sliderRect.anchorMax = new Vector2(0.78f, 0.82f);
            sliderRect.offsetMin = sliderRect.offsetMax = Vector2.zero;

            var background = NewImage("Background", sliderRect, new Color(0.12f, 0.28f, 0.34f, 1f));
            background.rectTransform.anchorMin = new Vector2(0f, 0.4f);
            background.rectTransform.anchorMax = new Vector2(1f, 0.6f);
            background.rectTransform.offsetMin = background.rectTransform.offsetMax = Vector2.zero;

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.SetParent(sliderRect, false);
            fillAreaRect.anchorMin = new Vector2(0f, 0.35f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.65f);
            fillAreaRect.offsetMin = new Vector2(5f, 0f);
            fillAreaRect.offsetMax = new Vector2(-5f, 0f);
            var fill = NewImage("Fill", fillAreaRect, new Color(0.15f, 0.9f, 1f, 1f));
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.SetParent(sliderRect, false);
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(7f, 0f);
            handleAreaRect.offsetMax = new Vector2(-7f, 0f);
            var handle = NewImage("Handle", handleAreaRect, new Color(0.8f, 1f, 1f, 1f));
            handle.rectTransform.sizeDelta = new Vector2(14f, 18f);

            _speedLimitSlider = sliderObject.GetComponent<Slider>();
            _speedLimitSlider.minValue = 20f;
            _speedLimitSlider.maxValue = 120f;
            _speedLimitSlider.wholeNumbers = true;
            _speedLimitSlider.fillRect = fill.rectTransform;
            _speedLimitSlider.handleRect = handle.rectTransform;
            _speedLimitSlider.targetGraphic = handle;
            _speedLimitSlider.value = ThreeBodySkillState.EngineThrottleLimit;
            UpdateSpeedLimitText(_speedLimitSlider.value);
            _speedLimitSlider.onValueChanged.AddListener(speed =>
            {
                ThreeBodySkillState.SetEngineThrottleLimit(speed);
                UpdateSpeedLimitText(speed);
            });
        }

        private void UpdateSpeedLimitText(float speed)
        {
            if (_speedLimitText != null)
                _speedLimitText.text = Mathf.RoundToInt(speed).ToString();
        }

        private void SetRadarVisible(bool visible)
        {
            if (_canvasGroup == null)
                return;

            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        private void ToggleExpanded()
        {
            _expanded = !_expanded;
            _root.sizeDelta = _expanded ? new Vector2(380f, 366f) : new Vector2(190f, 196f);
            _root.anchoredPosition = _expanded ? new Vector2(-85f, 20f) : new Vector2(-115f, 55f);
            if (_expandLabel != null)
                _expandLabel.text = _expanded ? "↘" : "↗";
        }

        private void Update()
        {
            if (_scene == null || !_scene.PlayerShip.IsActive()) return;
            var player = _scene.PlayerShip;
            if (RadarStatus.IsJammed(player))
            {
                SetRadarStatic(true);
                _scene.LockTarget(null);
                _status.text = "JAMMED";
                _rangeText.text = "SNOW";
                return;
            }

            SetRadarVisible(true);
            SetRadarStatic(false);
            var radarRange = GetRadarRange(player);
            var enemies = _scene.Ships.Items
                .Where(s => s.IsActive() && !RadarStatus.IsStealthedFrom(s, player) && CombatRelations.AreEnemies(player.Type, s.Type))
                .ToArray();
            var detected = enemies.Where(s => Vector2.Distance(player.Body.Position, s.Body.Position) <= radarRange).ToArray();
            var allies = _scene.Ships.Items
                .Where(s => s.IsActive() && s != player && !RadarStatus.IsStealthedFrom(s, player) && s.Type.Side == UnitSide.Ally)
                .Where(s => Vector2.Distance(player.Body.Position, s.Body.Position) <= radarRange)
                .ToArray();
            var detectedLockableProjectiles = GetDetectedLockableProjectiles(player, radarRange);
            var lockedTarget = _scene.LockedTarget;
            var lockedShip = _scene.LockedEnemyShip;
            var lockedProjectile = IsLockableProjectile(lockedTarget);
            var lockedTargetVisible = lockedShip != null && detected.Contains(lockedShip) ||
                                      lockedProjectile && IsProjectileVisible(lockedTarget, player, radarRange);
            if (!lockedTargetVisible)
            {
                _scene.LockTarget(null);
                IUnit nearestTarget = detected
                    .Cast<IUnit>()
                    .Concat(detectedLockableProjectiles)
                    .OrderBy(s => Vector2.SqrMagnitude(s.Body.Position - player.Body.Position))
                    .FirstOrDefault();
                if (nearestTarget != null)
                    Lock(nearestTarget);
            }
            var displayRange = Mathf.Max(100f, detected.Concat(allies)
                .Select(s => Vector2.Distance(player.Body.Position, s.Body.Position))
                .Concat(detectedLockableProjectiles.Select(m => Vector2.Distance(player.Body.Position, m.Body.WorldPosition())))
                .DefaultIfEmpty(100f).Max());

            var detectedSet = new HashSet<IShip>(detected);
            foreach (var stale in _markers.Keys.Where(ship => !detectedSet.Contains(ship)).ToArray())
            {
                Destroy(_markers[stale].Root);
                _markers.Remove(stale);
            }

            foreach (var ship in detected)
            {
                if (!_markers.TryGetValue(ship, out var marker))
                {
                    marker = CreateMarker(ship);
                    _markers.Add(ship, marker);
                }

                var rect = marker.Rect;
                var relative = (ship.Body.Position - player.Body.Position) / displayRange;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f + relative.x * 0.47f, 0.5f + relative.y * 0.47f);
                SetDot(rect, Vector2.zero, ship == _scene.LockedEnemyShip ? 12f : 7f);
                marker.Image.color = ship.Specification.Stats.ShipModel.SizeClass == GameDatabase.Enums.SizeClass.Starbase
                    ? CombatTargetLine.TargetColor(ship)
                    : ThreeBodySkillState.AdvancedRadarUnlocked
                        ? CombatTargetLine.TargetColor(ship)
                        : Color.red;
                marker.Cross.SetActive(ship == _scene.LockedEnemyShip);
            }

            UpdateAllyMarkers(player, allies, displayRange);

            UpdateProjectileLayer(player, radarRange, displayRange);
            UpdateTransientMarkers(player, displayRange);
            _status.text = _scene.LockedTarget != null && _scene.LockedTarget.IsActive() ? "LOCKED" : "NO LOCK";
            _rangeText.text = $"RADAR {radarRange:0}";
        }

        private void UpdateAllyMarkers(IShip player, IEnumerable<IShip> allies, float displayRange)
        {
            var visible = new HashSet<IShip>(allies);
            foreach (var stale in _allyMarkers.Keys.Where(ship => !visible.Contains(ship)).ToArray())
            {
                Destroy(_allyMarkers[stale].gameObject);
                _allyMarkers.Remove(stale);
            }

            foreach (var ally in visible)
            {
                if (!_allyMarkers.TryGetValue(ally, out var marker))
                {
                    var go = new GameObject("AllyRadarTriangle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                    var markerRect = go.GetComponent<RectTransform>();
                    markerRect.SetParent(_map, false);
                    marker = go.GetComponent<Text>();
                    marker.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    marker.fontSize = 10;
                    marker.alignment = TextAnchor.MiddleCenter;
                    marker.text = "●";
                    marker.color = new Color(0.2f, 0.62f, 1f, 1f);
                    marker.raycastTarget = false;
                    _allyMarkers.Add(ally, marker);
                }

                var relative = (ally.Body.Position - player.Body.Position) / displayRange;
                var rect = marker.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f + relative.x * 0.47f, 0.5f + relative.y * 0.47f);
                SetDot(rect, Vector2.zero, 8f);
            }
        }

        private void UpdateProjectileLayer(IShip player, float radarRange, float displayRange)
        {
            var visible = new HashSet<IUnit>();
            lock (_scene.Units.LockObject)
            {
                foreach (var unit in _scene.Units.Items)
                {
                    if (!IsProjectileVisible(unit, player, radarRange))
                    {
                        if (unit.IsActive() && unit.Type.Class == UnitClass.AreaOfEffect &&
                            Vector2.Distance(player.Body.Position, unit.Body.WorldPosition()) <= radarRange &&
                            _seenAreaEffects.Add(unit))
                        {
                            SpawnExplosion(unit.Body.WorldPosition(), !CombatRelations.AreEnemies(player.Type, unit.Type));
                        }

                        continue;
                    }

                    visible.Add(unit);
                    _lastProjectilePositions[unit] = unit.Body.WorldPosition();
                    if (!_projectileMarkers.TryGetValue(unit, out var marker))
                    {
                        marker = CreateProjectileMarker(unit);
                        _projectileMarkers.Add(unit, marker);
                    }

                    UpdateProjectileMarker(marker, unit, player, displayRange);
                }
            }

            foreach (var stale in _projectileMarkers.Keys.Where(unit => !visible.Contains(unit)).ToArray())
            {
                if (_lastProjectilePositions.TryGetValue(stale, out var lastPosition) &&
                    (stale.Type.Class == UnitClass.Missile || IsMacroElectron(stale)))
                    SpawnExplosion(lastPosition, !CombatRelations.AreEnemies(player.Type, stale.Type));

                Destroy(_projectileMarkers[stale].Root);
                _projectileMarkers.Remove(stale);
                _lastProjectilePositions.Remove(stale);
            }

            _seenAreaEffects.RemoveWhere(unit => !unit.IsActive());
        }

        private static bool IsProjectileVisible(IUnit unit, IShip player, float radarRange)
        {
            if (!unit.IsActive())
                return false;

            if (!IsLockableProjectile(unit) && unit.Type.Class != UnitClass.Missile &&
                unit.Type.Class != UnitClass.EnergyBolt)
                return false;

            return Vector2.Distance(player.Body.Position, unit.Body.WorldPosition()) <= radarRange;
        }

        private UnitMarker CreateProjectileMarker(IUnit unit)
        {
            var isLockable = IsLockableProjectile(unit);
            var go = new GameObject(isLockable ? "LockableProjectileBlip" : unit.Type.Class == UnitClass.Missile ? "MissileBlip" : "LaserTrace", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(_map, false);
            var image = go.GetComponent<Image>();
            image.sprite = MarkerSprite;
            image.raycastTarget = isLockable;

            GameObject cross = null;
            Text symbol = null;
            if (isLockable)
            {
                var button = go.AddComponent<Button>();
                button.onClick.AddListener(() => _scene.LockUnit(unit));
                symbol = AddText(rect, "●", 14);
                symbol.color = Color.white;
                cross = new GameObject("LockedCross", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                var crossRect = cross.GetComponent<RectTransform>();
                crossRect.SetParent(rect, false);
                crossRect.anchorMin = Vector2.zero;
                crossRect.anchorMax = Vector2.one;
                crossRect.offsetMin = crossRect.offsetMax = Vector2.zero;
                var crossText = cross.GetComponent<Text>();
                crossText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                crossText.fontSize = 16;
                crossText.alignment = TextAnchor.MiddleCenter;
                crossText.color = Color.white;
                crossText.text = "+";
                crossText.raycastTarget = false;
                cross.SetActive(false);
            }

            return new UnitMarker(go, rect, image, cross, symbol);
        }

        private void UpdateProjectileMarker(UnitMarker marker, IUnit unit, IShip player, float displayRange)
        {
            var friendly = !CombatRelations.AreEnemies(player.Type, unit.Type);
            marker.Image.color = friendly ? new Color(0.25f, 0.65f, 1f, 0.95f) : new Color(1f, 0.15f, 0.1f, 0.95f);

            var worldPosition = unit.Body.WorldPosition();
            var relative = (worldPosition - player.Body.Position) / displayRange;
            marker.Rect.anchorMin = marker.Rect.anchorMax = new Vector2(0.5f + relative.x * 0.47f, 0.5f + relative.y * 0.47f);

            if (IsLockableProjectile(unit))
            {
                var macroColor = IsDualVectorFoil(unit)
                    ? Color.white
                    : friendly
                    ? new Color(0.2f, 0.65f, 1f, 1f)
                    : new Color(1f, 0.16f, 0.08f, 1f);
                marker.Image.color = new Color(macroColor.r, macroColor.g, macroColor.b, 0.22f);
                if (marker.Symbol != null)
                    marker.Symbol.color = macroColor;
                SetDot(marker.Rect, Vector2.zero, _scene.LockedTarget == unit ? 12f : 9f);
                marker.Cross?.SetActive(_scene.LockedTarget == unit);
                marker.Rect.localEulerAngles = Vector3.zero;
                return;
            }

            if (unit.Type.Class == UnitClass.Missile)
            {
                SetDot(marker.Rect, Vector2.zero, 4f);
                marker.Rect.localEulerAngles = Vector3.zero;
                return;
            }

            var lineStart = unit.Body.WorldPositionNoOffset();
            var direction = RotationHelpers.Direction(unit.Body.WorldRotation());
            var end = lineStart + direction * Mathf.Max(unit.Body.Scale * 1.8f, 12f);
            if (unit.Collider is RayCastCollider ray)
                end = ray.ActiveCollision != null ? ray.LastContactPoint : lineStart + direction * ray.MaxRange;
            else if (unit.Body.Parent == null)
                lineStart = worldPosition - direction * Mathf.Max(unit.Body.Scale * 1.8f, 12f);
            var startRelative = (lineStart - player.Body.Position) / displayRange;
            var endRelative = (end - player.Body.Position) / displayRange;
            var startPoint = new Vector2(startRelative.x * _map.rect.width * 0.47f, startRelative.y * _map.rect.height * 0.47f);
            var endPoint = new Vector2(endRelative.x * _map.rect.width * 0.47f, endRelative.y * _map.rect.height * 0.47f);
            var delta = endPoint - startPoint;
            var radarLength = Mathf.Max(10f, delta.magnitude);
            marker.Rect.anchorMin = marker.Rect.anchorMax = new Vector2(0.5f, 0.5f);
            marker.Rect.anchoredPosition = startPoint;
            marker.Rect.sizeDelta = new Vector2(radarLength, 2.5f);
            marker.Rect.pivot = new Vector2(0f, 0.5f);
            marker.Rect.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        private void LockNearest()
        {
            var player = _scene.PlayerShip;
            var target = _scene.Ships.Items.Where(s => s.IsActive() && CombatRelations.AreEnemies(player.Type, s.Type))
                .Where(s => !RadarStatus.IsStealthedFrom(s, player))
                .Where(s => Vector2.Distance(player.Body.Position, s.Body.Position) <= GetRadarRange(player))
                .Cast<IUnit>()
                .Concat(GetDetectedLockableProjectiles(player, GetRadarRange(player)))
                .OrderBy(s => Vector2.SqrMagnitude(s.Body.WorldPosition() - player.Body.Position)).FirstOrDefault();
            Lock(target);
        }

        private IUnit[] GetDetectedLockableProjectiles(IShip player, float radarRange)
        {
            lock (_scene.Units.LockObject)
            {
                return _scene.Units.Items
                    .Where(unit => IsLockableProjectile(unit) &&
                                   CombatRelations.AreEnemies(player.Type, unit.Type) &&
                                   IsProjectileVisible(unit, player, radarRange))
                    .ToArray();
            }
        }

        public static float GetRadarRange(IShip ship)
        {
            var range = BaseRadarRange + ship.Specification.Devices
                .Where(d => d.Device.DeviceClass == DeviceClass.Radar).Sum(d => d.Device.Power);
            return range * ThreeBodySkillState.RadarRangeMultiplier;
        }

        private void Lock(IShip ship)
        {
            Lock((IUnit)ship);
        }

        private void Lock(IUnit unit)
        {
            if (unit == null || !unit.IsActive())
                return;
            _scene.LockUnit(unit);
            _status.text = "LOCKED";
        }

        private TargetMarker CreateMarker(IShip ship)
        {
            var buttonObject = new GameObject("Target", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(_map, false);
            var image = buttonObject.GetComponent<Image>();
            image.sprite = MarkerSprite;
            image.raycastTarget = true;
            buttonObject.GetComponent<Button>().onClick.AddListener(() => Lock(ship));

            var cross = new GameObject("LockedCross", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var crossRect = cross.GetComponent<RectTransform>();
            crossRect.SetParent(rect, false);
            crossRect.anchorMin = Vector2.zero;
            crossRect.anchorMax = Vector2.one;
            crossRect.offsetMin = crossRect.offsetMax = Vector2.zero;
            var crossText = cross.GetComponent<Text>();
            crossText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            crossText.fontSize = 18;
            crossText.alignment = TextAnchor.MiddleCenter;
            crossText.color = Color.white;
            crossText.text = "+";
            crossText.raycastTarget = false;
            cross.SetActive(false);
            return new TargetMarker(buttonObject, rect, image, cross);
        }

        private static Image NewImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private void CreateRadarStaticOverlay()
        {
            _staticTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
            };
            _staticPixels = new Color32[64 * 64];

            var go = new GameObject("RadarStatic", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(_map, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            _staticOverlay = go.GetComponent<RawImage>();
            _staticOverlay.texture = _staticTexture;
            _staticOverlay.color = new Color(0.65f, 1f, 1f, 0.86f);
            _staticOverlay.raycastTarget = false;
            _staticOverlay.enabled = false;
        }

        private void SetRadarStatic(bool visible)
        {
            if (_staticOverlay == null)
                return;

            _staticOverlay.enabled = visible;
            if (!visible || Time.unscaledTime < _nextStaticUpdateTime)
                return;

            _nextStaticUpdateTime = Time.unscaledTime + 0.045f;
            for (var i = 0; i < _staticPixels.Length; ++i)
            {
                var value = (byte)Random.Range(55, 255);
                var alpha = (byte)Random.Range(150, 245);
                _staticPixels[i] = new Color32(value, (byte)Mathf.Min(255, value + 25), 255, alpha);
            }

            _staticTexture.SetPixels32(_staticPixels);
            _staticTexture.Apply(false);
        }

        private static bool IsMacroElectron(IUnit unit)
        {
            return unit is Bullet bullet && bullet.Controller is BallLightningController;
        }

        private static bool IsLockableProjectile(IUnit unit)
        {
            return IsMacroElectron(unit) || unit is Bullet bullet &&
                bullet.Controller is StrategicWeaponController controller &&
                controller.Kind == StrategicWeaponController.WeaponKind.DualVectorFoil;
        }

        private static bool IsDualVectorFoil(IUnit unit)
        {
            return unit is Bullet bullet && bullet.Controller is StrategicWeaponController controller &&
                controller.Kind == StrategicWeaponController.WeaponKind.DualVectorFoil;
        }

        private static Sprite MarkerSprite
        {
            get
            {
                if (_markerSprite == null)
                {
                    const int size = 32;
                    var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                    {
                        name = "Combat Minimap Circular Marker",
                        filterMode = FilterMode.Bilinear,
                        wrapMode = TextureWrapMode.Clamp,
                        hideFlags = HideFlags.HideAndDontSave,
                    };
                    var pixels = new Color32[size * size];
                    float center = (size - 1) * 0.5f;
                    float radius = size * 0.47f;
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dx = x - center;
                            float dy = y - center;
                            float distance = Mathf.Sqrt(dx * dx + dy * dy);
                            float alpha = Mathf.Clamp01(radius + 0.75f - distance);
                            pixels[y * size + x] = new Color32(255, 255, 255,
                                (byte)Mathf.RoundToInt(alpha * 255f));
                        }
                    }
                    texture.SetPixels32(pixels);
                    texture.Apply(false, false);
                    _markerSprite = Sprite.Create(texture, new Rect(0, 0, size, size),
                        new Vector2(0.5f, 0.5f), size);
                    _markerSprite.name = "Combat Minimap Circular Marker";
                    _markerSprite.hideFlags = HideFlags.HideAndDontSave;
                }
                return _markerSprite;
            }
        }

        private static void SetDot(RectTransform rect, Vector2 position, float size)
        {
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(size, size);
        }

        private static Text AddText(RectTransform parent, string value, int size)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }

        private void SpawnExplosion(Vector2 worldPosition, bool friendly)
        {
            var go = new GameObject("ExplosionBlip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(_map, false);
            var image = go.GetComponent<Image>();
            image.sprite = MarkerSprite;
            image.raycastTarget = false;
            image.color = friendly ? new Color(0.45f, 0.85f, 1f, 0.9f) : new Color(1f, 0.55f, 0.2f, 0.95f);
            _transientMarkers.Add(new TransientMarker(go, rect, image, worldPosition, 0.45f));
        }

        private void UpdateTransientMarkers(IShip player, float displayRange)
        {
            for (var i = _transientMarkers.Count - 1; i >= 0; i--)
            {
                var marker = _transientMarkers[i];
                marker.TimeLeft -= Time.deltaTime;
                if (marker.TimeLeft <= 0f)
                {
                    Destroy(marker.Root);
                    _transientMarkers.RemoveAt(i);
                    continue;
                }

                var relative = (marker.WorldPosition - player.Body.Position) / displayRange;
                marker.Rect.anchorMin = marker.Rect.anchorMax = new Vector2(0.5f + relative.x * 0.47f, 0.5f + relative.y * 0.47f);
                var size = Mathf.Lerp(14f, 6f, 1f - marker.TimeLeft / marker.Duration);
                SetDot(marker.Rect, Vector2.zero, size);
                var color = marker.Image.color;
                color.a = Mathf.Clamp01(marker.TimeLeft / marker.Duration);
                marker.Image.color = color;
            }
        }

        private sealed class TargetMarker
        {
            public TargetMarker(GameObject root, RectTransform rect, Image image, GameObject cross)
            {
                Root = root;
                Rect = rect;
                Image = image;
                Cross = cross;
            }

            public readonly GameObject Root;
            public readonly RectTransform Rect;
            public readonly Image Image;
            public readonly GameObject Cross;
        }

        private sealed class UnitMarker
        {
            public UnitMarker(GameObject root, RectTransform rect, Image image, GameObject cross = null, Text symbol = null)
            {
                Root = root;
                Rect = rect;
                Image = image;
                Cross = cross;
                Symbol = symbol;
            }

            public readonly GameObject Root;
            public readonly RectTransform Rect;
            public readonly Image Image;
            public readonly GameObject Cross;
            public readonly Text Symbol;
        }

        private sealed class TransientMarker
        {
            public TransientMarker(GameObject root, RectTransform rect, Image image, Vector2 worldPosition, float duration)
            {
                Root = root;
                Rect = rect;
                Image = image;
                WorldPosition = worldPosition;
                Duration = duration;
                TimeLeft = duration;
            }

            public readonly GameObject Root;
            public readonly RectTransform Rect;
            public readonly Image Image;
            public readonly Vector2 WorldPosition;
            public readonly float Duration;
            public float TimeLeft;
        }

        private static Sprite _markerSprite;
    }
}
