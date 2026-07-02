using UnityEngine;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using GameDatabase.Enums;
using Services.Resources;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Combat.Ai.Calculations;
using System.Linq;

namespace Gui.Combat
{
    public class Radar : UIBehaviour
    {
        [SerializeField] private Image ShipIcon;
        [SerializeField] private Image Background;
        [SerializeField] private float Size = 24;
        [SerializeField] private Color AllyColor;
        [SerializeField] private Color NormalColor;
        [SerializeField] private Color BossColor;
        [SerializeField] private Color StarbaseColor;
        [SerializeField] private Color DangerColor = Color.red;

        public void Open(IShip ship, IScene scene, IResourceLocator resourceLocator)
        {
            _scene = scene;
            _ship = ship;

            Initialize(resourceLocator);
            Update();
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!_ship.IsActive())
            {
                Close();
                return;
            }

            var camera = Camera.main;
            if (!camera) return;

            var observer = _scene.PlayerShip;
            var radarRange = observer.IsActive() ? CombatMinimap.GetRadarRange(observer) : 0f;
            var detected = _ship.Type.Side == UnitSide.Ally ||
                           (observer.IsActive() && Vector2.Distance(observer.Body.Position, _ship.Body.Position) <= radarRange);
            if (!detected)
            {
                ShipIcon.enabled = false;
                Background.enabled = false;
                return;
            }

            var itemPosition = _ship.Body.VisualPosition;
            var position = _scene.ViewPoint.Direction(itemPosition);
            var cameraHeight = camera.orthographicSize;
            var cameraWidth = cameraHeight* camera.aspect;

            var x = position.x/cameraWidth;
            var y = position.y/cameraHeight;

            var outOfBounds = x < -1 || x > 1 || y < -1 || y > 1;
            if (ShipIcon.enabled != outOfBounds) ShipIcon.enabled = outOfBounds;
            if (Background.enabled != outOfBounds) Background.enabled = outOfBounds;

            if (!outOfBounds)
                return;

            var dx = ((position.x > 0 ? position.x : -position.x) - cameraWidth)/(_scene.Settings.AreaWidth/2 - cameraWidth);
            var dy = ((position.y > 0 ? position.y : -position.y) - cameraHeight)/(_scene.Settings.AreaHeight/2 - cameraHeight);
            var scale = Mathf.Max(1 - 0.5f*Mathf.Max(dx, dy), 0.25f);

            var max = Mathf.Max(x > 0 ? x : -x, y > 0 ? y : -y);
            var offset = scale*_offset;

            x = offset + 0.5f*(x/max + 1)*(_screenSize.x - 2*offset);
            y = offset + 0.5f*(y/max + 1)*(_screenSize.y - 2*offset);

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            RectTransform.anchoredPosition = new Vector2(x, y);
            var locked = _scene.LockedEnemyShip == _ship;
            RectTransform.localScale = Vector3.one * scale * (locked ? 1.35f : 1f);
            if (locked)
                Background.color = CombatTargetLine.HighlightedTargetColor(_ship);
            else
                ApplyBackgroundColor();
            ShipIcon.transform.localEulerAngles = new Vector3(0, 0, _ship.Body.VisualRotation);
        }

        public void Close()
        {
            _ship = null;

            if (this)
                gameObject.SetActive(false);
        }

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        private void Initialize(IResourceLocator resourceLocator)
        {
            var model = _ship.Specification.Stats;
            var isAlly = _ship.Type.Side == UnitSide.Ally;
            var isDangerous = _ship.Specification.Info.Class >= DifficultyClass.Class3;

            ApplyBackgroundColor();

            switch (model.ShipModel.SizeClass)
            {
                case SizeClass.Starbase:
                    _offset = Size*1.8f;
                    break;
                case SizeClass.Titan:
                    _offset = Size*1.5f;
                    break;
                default:
                    _offset = Size;
                    break;
            }

            ShipIcon.sprite = resourceLocator.GetSprite(model.ShipModel.ModelImage);
            UpdateAllyMarker(isAlly && _ship != _scene.PlayerShip);

            UpdateScreenSize();
        }

        private void ApplyBackgroundColor()
        {
            if (_ship == null) return;
            var size = _ship.Specification.Stats.ShipModel.SizeClass;
            var isAlly = _ship.Type.Side == UnitSide.Ally;
            var isDangerous = _ship.Specification.Info.Class >= DifficultyClass.Class3;
            if (isAlly) Background.color = AllyColor;
            else if (size == SizeClass.Starbase) Background.color = StarbaseColor;
            else if (size == SizeClass.Titan) Background.color = isDangerous ? DangerColor : BossColor;
            else if (size == SizeClass.Cruiser || size == SizeClass.Battleship) Background.color = new Color(1f, 0.45f, 0.05f, 1f);
            else Background.color = NormalColor;
        }

        private void UpdateAllyMarker(bool visible)
        {
            if (_allyMarker == null)
            {
                var marker = new GameObject("AllyMarker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                var rect = marker.GetComponent<RectTransform>();
                rect.SetParent(RectTransform, false);
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, 2f);
                rect.sizeDelta = new Vector2(22f, 18f);
                _allyMarker = marker.GetComponent<Text>();
                _allyMarker.text = "▲";
                _allyMarker.alignment = TextAnchor.LowerCenter;
                _allyMarker.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                _allyMarker.fontSize = 18;
                _allyMarker.color = new Color(0.15f, 0.55f, 1f, 1f);
                _allyMarker.raycastTarget = false;
            }

            _allyMarker.gameObject.SetActive(visible);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            UpdateScreenSize();
        }

        private void UpdateScreenSize()
        {
            _screenSize = RectTransform.parent.GetComponent<RectTransform>().rect.size;

            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _offset*2);
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _offset*2);
        }

        private float _offset;
        private Vector2 _screenSize;
        private RectTransform _rectTransform;
        private IShip _ship;
        private IScene _scene;
        private Text _allyMarker;
    }
}
