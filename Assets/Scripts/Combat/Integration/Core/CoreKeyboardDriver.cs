using System.Collections.Generic;
using Combat.Ai;
using UnityEngine;
using Zenject;

namespace Combat.Integration.Core
{
    // Keyboard and mouse input for the controlled core ship (migration phase 3c, step 5). The legacy
    // KeyboardController rules, written to the ship's command sink. The sink gets a value only when the driver's
    // own value changes, so the HUD buttons keep theirs. A key held across a ship change acts on the new ship
    // (owner decision, legacy behavior); the new pilot itself starts with empty controls.
    public sealed class CoreKeyboardDriver : ITickable
    {
        [Inject]
        public CoreKeyboardDriver(IKeyboard keyboard, IMouse mouse, IBattleView battle, IShipViews ships, IShipControlSource controls)
        {
            _keyboard = keyboard;
            _mouse = mouse;
            _battle = battle;
            _ships = ships;
            _controls = controls;
        }

        public void Tick()
        {
            var id = _battle.ControlledShip;
            if (id != _shipId)
                Select(id);

            if (_commands == null || _ship == null || !_ship.IsActive)
                return;

            UpdateThrottle();
            UpdateJoystick();
            UpdateTurn();
            UpdateMouse();
            UpdateActions();
        }

        // The previous ship is gone or no longer controlled, so its sink gets no releases.
        private void Select(EntityId id)
        {
            _shipId = id;
            _ship = null;
            _commands = null;
            _keyBindings = null;
            _throttle = _joystick = _left = _right = false;
            _actions.Clear();

            if (!id.IsValid || !_ships.TryGetShip(id, out var ship))
                return;
            if (!_controls.TryGetControls(id, out var description, out var commands))
                return;

            _ship = ship;
            _commands = commands;
            _keyBindings = description.KeyBindings;
            for (var i = 0; i < _keyBindings.Count; ++i)
                _actions.Add(false);
        }

        private void UpdateThrottle()
        {
            if (_keyboard.Throttle || _mouse.ThrustButton)
            {
                _throttle = true;
                _commands.SetThrottle(1f);
            }
            else if (_throttle)
            {
                _throttle = false;
                _commands.SetThrottle(0f);
            }
        }

        private void UpdateJoystick()
        {
            var direction = new Vector2(_keyboard.JoystickX, _keyboard.JoystickY);
            if (direction.sqrMagnitude > 0)
            {
                _joystick = true;
                _commands.SetThrottle(direction.sqrMagnitude > 0.5f ? 1f : 0f);
                _commands.SetCourse(RotationHelpers.Angle(direction));
            }
            else if (_joystick)
            {
                _joystick = false;
                _commands.SetThrottle(0f);
                _commands.SetCourse(null);
            }
        }

        // The course is a target heading, so a turn key keeps it almost half a turn ahead of the ship.
        private void UpdateTurn()
        {
            if (_keyboard.Left)
            {
                _left = true;
                _commands.SetCourse(_ship.Rotation + TurnAngle);
            }
            else if (_left)
            {
                _left = false;
                _commands.SetCourse(null);
            }

            if (_keyboard.Right)
            {
                _right = true;
                _commands.SetCourse(_ship.Rotation - TurnAngle);
            }
            else if (_right)
            {
                _right = false;
                _commands.SetCourse(null);
            }
        }

        // The mouse position is in camera space, which is the space of the drawn ship, so aim works across the wrap.
        private void UpdateMouse()
        {
            if (!_left && !_right && !_joystick && _mouse.IsActive)
                _commands.SetCourse(RotationHelpers.Angle(_mouse.Position - _ship.VisualPosition));
            else
                _mouse.IsActive = false;
        }

        // Per key group, so releasing one key does not touch another held key (legacy released all groups at once).
        private void UpdateActions()
        {
            for (var i = 0; i < _keyBindings.Count; ++i)
            {
                var pressed = _keyboard.Action(i) || _mouse.FireButton(i);
                if (pressed == _actions[i])
                    continue;

                _actions[i] = pressed;
                foreach (var system in _keyBindings[i])
                    _commands.SetSystemState(system, pressed);
            }
        }

        private const float TurnAngle = 175f;

        private EntityId _shipId;
        private IShipView _ship;
        private IShipCommandSink _commands;
        private IReadOnlyList<IReadOnlyList<int>> _keyBindings;

        private bool _throttle;
        private bool _joystick;
        private bool _left;
        private bool _right;
        private readonly List<bool> _actions = new List<bool>();

        private readonly IKeyboard _keyboard;
        private readonly IMouse _mouse;
        private readonly IBattleView _battle;
        private readonly IShipViews _ships;
        private readonly IShipControlSource _controls;
    }
}
