using System.Collections.Generic;
using Combat.Component.Ship;
using Combat.Unit;

namespace Combat.Ai
{
    public class KeyboardController : IController
    {
        public KeyboardController(IShip ship, IKeyboard keyboard, IMouse mouse, PlayerInputSignal.Trigger playerInputTrigger)
        {
            _ship = ship;
            _keyBindings = ship.Systems.All.GetKeyBindings();
            _keyboard = keyboard;
            _mouse = mouse;
            _playerInputTrigger = playerInputTrigger;
        }

        public ControllerStatus Status
        {
            get
            {
                if (!_ship.IsActive()) return ControllerStatus.Dead;
                if (_fire || _left || _right || _joystick || _throttle || _mouse.IsActive) return ControllerStatus.Active;
                return ControllerStatus.Idle;
            }
        }

        public void Update(float deltaTime, in AiManager.Options options)
        {
            var controls = _ship.Controls;
            var currentRotation = _ship.Body.Rotation;

            if (_keyboard.Throttle || _mouse.ThrustButton)
            {
                _throttle = true;
                controls.Throttle = 1.0f;
            }
            else if (_throttle)
            {
                _throttle = false;
                controls.Throttle = 0.0f;
            }

            var dir = new UnityEngine.Vector2(_keyboard.JoystickX, _keyboard.JoystickY);
            if (dir.sqrMagnitude > 0)
            {
                _joystick = true;
                controls.Throttle = dir.sqrMagnitude > 0.5f ? 1 : 0;
                controls.Course = RotationHelpers.Angle(dir);
            }
            else if (_joystick)
            {
                _joystick = false;
                controls.Throttle = 0;
                controls.Course = null;
            }

            if (_keyboard.Left)
            {
                _left = true;
                controls.Course = currentRotation + 175;
            }
            else if (_left)
            {
                _left = false;
                controls.Course = null;
            }

            if (_keyboard.Right)
            {
                _right = true;
                controls.Course = currentRotation - 175;
            }
            else if (_right)
            {
                _right = false;
                controls.Course = null;
            }

            if (!_right && !_left && !_joystick && _mouse.IsActive)
            {
                var direction = _mouse.Position - _ship.Body.Position;
                controls.Course = RotationHelpers.Angle(direction);
            }
            else
            {
                _mouse.IsActive = false;
            }

            var fire = false;
            for (var i = 0; i < _keyBindings.Count; ++i)
            {
                if (options.CeaseFire)
                {
                    controls.Systems.Clear();
                }
                else if (_keyboard.Action(i) || _mouse.FireButton(i))
                {
                    fire = true;
                    foreach (var id in _keyBindings[i])
                        controls.Systems.SetState(id, true);
                }
                else if (_fire)
                {
                    foreach (var id in _keyBindings[i])
                        controls.Systems.SetState(id, false);
                }
            }

            _fire = fire;

            if (_fire || _left || _right || _joystick || _throttle || _mouse.IsActive)
                _playerInputTrigger.Fire();
        }

        private bool _joystick;
        private bool _throttle;
        private bool _left;
        private bool _right;
        private bool _fire;

        private readonly List<List<int>> _keyBindings;

        private readonly IShip _ship;
        private readonly IMouse _mouse;
        private readonly IKeyboard _keyboard;
        private readonly PlayerInputSignal.Trigger _playerInputTrigger;

        public class Factory : IControllerFactory
        {
            public Factory(IKeyboard keyboard, IMouse mouse, PlayerInputSignal.Trigger playerInputTrigger)
            {
                _keyboard = keyboard;
                _mouse = mouse;
                _playerInputTrigger = playerInputTrigger;
            }

            public IController Create(IShip ship)
            {
                return new KeyboardController(ship, _keyboard, _mouse, _playerInputTrigger);
            }

            private readonly PlayerInputSignal.Trigger _playerInputTrigger;
            private readonly IKeyboard _keyboard;
            private readonly IMouse _mouse;
        }
    }
}
