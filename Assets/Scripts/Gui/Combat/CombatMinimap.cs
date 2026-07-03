using System.Collections.Generic;
using System.Linq;
using Combat.Component.Ship;
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
        private IScene _scene;
        private RectTransform _map;
        private Text _status;
        private Text _rangeText;
        private RectTransform _root;
        private Text _expandLabel;
        private bool _expanded;

        public void Initialize(IScene scene)
        {
            _scene = scene;
            var root = GetComponent<RectTransform>();
            _root = root;
            root.anchorMin = root.anchorMax = new Vector2(1f, 0.5f);
            root.pivot = new Vector2(1f, 0.5f);
            root.anchoredPosition = new Vector2(-115f, 55f);
            root.sizeDelta = new Vector2(190f, 170f);

            var panel = NewImage("Map", root, new Color(0.01f, 0.04f, 0.05f, 0.82f));
            panel.raycastTarget = false;
            _map = panel.rectTransform;
            _map.anchorMin = Vector2.zero;
            _map.anchorMax = Vector2.one;
            _map.offsetMin = Vector2.zero;
            _map.offsetMax = new Vector2(0f, -32f);

            var center = NewImage("Player", _map, new Color(0.1f, 1f, 0.25f, 1f));
            center.raycastTarget = false;
            SetDot(center.rectTransform, Vector2.zero, 7f);

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

        private void ToggleExpanded()
        {
            _expanded = !_expanded;
            _root.sizeDelta = _expanded ? new Vector2(380f, 340f) : new Vector2(190f, 170f);
            _root.anchoredPosition = _expanded ? new Vector2(-85f, 20f) : new Vector2(-115f, 55f);
            if (_expandLabel != null)
                _expandLabel.text = _expanded ? "↘" : "↗";
        }

        private void Update()
        {
            if (_scene == null || !_scene.PlayerShip.IsActive()) return;
            var player = _scene.PlayerShip;
            var radarRange = GetRadarRange(player);
            var enemies = _scene.Ships.Items
                .Where(s => s.IsActive() && CombatRelations.AreEnemies(player.Type, s.Type))
                .ToArray();
            var detected = enemies.Where(s => Vector2.Distance(player.Body.Position, s.Body.Position) <= radarRange).ToArray();
            if (!_scene.LockedEnemyShip.IsActive() || !detected.Contains(_scene.LockedEnemyShip))
            {
                _scene.LockTarget(null);
                var nearestTarget = detected
                    .OrderBy(s => Vector2.SqrMagnitude(s.Body.Position - player.Body.Position))
                    .FirstOrDefault();
                if (nearestTarget != null)
                    Lock(nearestTarget);
            }
            var displayRange = Mathf.Max(100f, detected.Select(s => Vector2.Distance(player.Body.Position, s.Body.Position)).DefaultIfEmpty(100f).Max());

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
                marker.Image.color = ThreeBodySkillState.AdvancedRadarUnlocked
                    ? CombatTargetLine.TargetColor(ship)
                    : Color.red;
                marker.Cross.SetActive(ship == _scene.LockedEnemyShip);
            }

            _status.text = _scene.LockedEnemyShip.IsActive() ? "LOCKED" : "NO LOCK";
            _rangeText.text = $"RADAR {radarRange:0}";
        }

        private void LockNearest()
        {
            var player = _scene.PlayerShip;
            var target = _scene.Ships.Items.Where(s => s.IsActive() && CombatRelations.AreEnemies(player.Type, s.Type))
                .Where(s => Vector2.Distance(player.Body.Position, s.Body.Position) <= GetRadarRange(player))
                .OrderBy(s => Vector2.SqrMagnitude(s.Body.Position - player.Body.Position)).FirstOrDefault();
            Lock(target);
        }

        public static float GetRadarRange(IShip ship)
        {
            var range = BaseRadarRange + ship.Specification.Devices
                .Where(d => d.Device.DeviceClass == DeviceClass.Radar).Sum(d => d.Device.Power);
            return range * ThreeBodySkillState.RadarRangeMultiplier;
        }

        private void Lock(IShip ship)
        {
            if (ship == null || !ship.IsActive())
                return;
            _scene.LockTarget(ship);
            _status.text = "LOCKED";
        }

        private TargetMarker CreateMarker(IShip ship)
        {
            var buttonObject = new GameObject("Target", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(_map, false);
            var image = buttonObject.GetComponent<Image>();
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
    }
}
