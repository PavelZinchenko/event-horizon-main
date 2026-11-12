using Combat.Integration;
using Model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gui.Combat
{
    public class ShipNavigationPanel : MonoBehaviour, IEventSystemHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
    {
        public void Initialize(IShipView ship, IShipCommandSink commands)
        {
            // Release a held drag on the previous ship before switching.
            Pressed = false;
            _ship = ship;
            _commands = commands;
        }

        public void OnDrag(PointerEventData data)
        {
            if (Pressed && _touchId == data.pointerId)
                _currentPosition = data.position;
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (!Pressed)
            {
                Pressed = true;
                _touchId = data.pointerId;
                _startPosition = _currentPosition = data.position;
                _startDirection = _ship.Rotation;
                //_doubleClicked = Time.realtimeSinceStartup - _clickTime < 0.5f;
            }
        }

        public void OnEndDrag(PointerEventData data)
        {
            if (_touchId == data.pointerId)
                Pressed = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //if (!Pressed)
            //	_clickTime = Time.realtimeSinceStartup;
        }

        public bool AllowThrust { get; set; }

        private void Awake()
        {
            _max = Screen.dpi == 0 ? Screen.height / 10 : Screen.dpi / 2;
            _min = _max / 5;
        }

        private void Update()
        {
            if (_ship == null || !_ship.IsActive)
                return;

            if (!Pressed)
                return;

            if (!AllowThrust /*|| _doubleClicked*/)
            {
                _commands.SetCourse(RotationHelpers.Angle(_currentPosition - _startPosition));
            }
            else
            {
                var rotation = _ship.Rotation;
                if (Vector2.Distance(_currentPosition, _startPosition) > _max)
                {
                    _startPosition = _currentPosition + (_startPosition - _currentPosition).normalized * _max;
                    _startDirection = rotation;
                }

                Vector2 direction = _currentPosition - _startPosition;
                var power = direction.magnitude;
                float? course = null;
                if (power > 1)
                    course = Mathf.LerpAngle(_startDirection, RotationHelpers.Angle(direction), Mathf.Clamp01(2 * power / _max));

                _commands.SetCourse(course);
                _commands.SetThrottle(Mathf.Abs(Mathf.DeltaAngle(course.GetValueOrDefault(rotation), rotation)) < 30 ? Mathf.Clamp01((power - _min) / (_max - _min)) : 0);
            }
        }

        private bool Pressed
        {
            get { return _pressed; }
            set
            {
                if (_pressed != value)
                {
                    _pressed = value;
                    if (!_pressed && _commands != null)
                    {
                        _commands.SetCourse(null);
                        if (AllowThrust)
                            _commands.SetThrottle(0.0f);
                    }
                }
            }
        }

        //private float _clickTime;
        //private bool  _doubleClicked;

        private bool _pressed;
        private int _touchId;
        private Vector2 _startPosition;
        private Vector2 _currentPosition;
        private float _startDirection;

        private float _min;
        private float _max;

        private IShipView _ship;
        private IShipCommandSink _commands;
    }
}
