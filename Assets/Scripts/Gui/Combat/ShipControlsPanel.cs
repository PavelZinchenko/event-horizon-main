using System.Collections.Generic;
using Combat.Component.Ship;
using Combat.Integration;
using Combat.Integration.Legacy;
using Services.Settings;
using Gui.Windows;
using Services.Gui;
using Services.Resources;
using UnityEngine;
using Zenject;
using Combat.Ai;

namespace Gui.Combat
{
    public class ShipControlsPanel : MonoBehaviour
    {
        [SerializeField] private ShipNavigationPanel _navigationPanel;

        [Inject] private readonly PlayerInputSignal.Trigger _playerInputTrigger;
        [Inject] private readonly IGameSettings _gameSettings;
        [Inject] private readonly IResourceLocator _resourceLocator;

        // Temporary entry for exploration and the legacy controls scene until phase 6.
        public void Load(IShip ship)
        {
            var controls = ship != null ? new LegacyShipControls(ship) : null;
            Load(LegacyShipView.Of(ship), controls, controls);
        }

        public void Load(IShipView ship, IShipControlDescription description, IShipCommandSink commands)
        {
            if (ship == null || !ship.IsActive)
            {
                Window.Close(WindowExitCode.Ok);
                return;
            }

            Window.Open();
            Initialize(ship, description, commands);
        }

        public void Reload()
        {
            if (IsShipActive && GetComponent<IWindow>().IsVisible)
                Initialize(_ship, _description, _commands);
        }

        public void OnJoystickPressed(Vector2 direction)
        {
            _joystickDirection = direction;
        }

        public void OnJoystickReleased()
        {
            _joystickDirection = Vector2.zero;
            if (_thrustWithJoystick && _commands != null)
                _commands.SetThrottle(0f);
        }

        public void OnThrustPressed()
        {
            if (IsShipActive)
                _commands.SetThrottle(1);
        }

        public void OnThrustReleased()
        {
            if (IsShipActive)
                _commands.SetThrottle(0);
        }

        public void OnLeftPressed()
        {
            _leftPressed = true;
        }

        public void OnRightPressed()
        {
            _rightPressed = true;
        }

        public void OnLeftRightReleased()
        {
            _rightPressed = _leftPressed = false;
            if (IsShipActive)
                _commands.SetCourse(null);
        }

        public void ActivateSystem(int id)
        {
            ActiveButtons++;
            if (IsShipActive)
                _commands.SetSystemState(id, true);
        }

        public void DeactivateSystem(int id)
        {
            ActiveButtons--;
            if (IsShipActive)
                _commands.SetSystemState(id, false);
        }

        public void ActivateDroneBays()
        {
            ActiveButtons++;
            if (IsShipActive)
                foreach (var id in _description.DroneBays)
                    _commands.SetSystemState(id, true);
        }

        public void DeactivateDroneBays()
        {
            ActiveButtons--;
            if (IsShipActive)
                foreach (var id in _description.DroneBays)
                    _commands.SetSystemState(id, false);
        }

        public void OnKeyPressed(int key)
        {
            if (key < _keyBindings.Count)
                foreach (var id in _keyBindings[key])
                    ActivateSystem(id);
        }

        public void OnKeyReleased(int key)
        {
            if (key < _keyBindings.Count)
                foreach (var id in _keyBindings[key])
                    DeactivateSystem(id);
        }

        private void Initialize(IShipView ship, IShipControlDescription description, IShipCommandSink commands)
        {
            _ship = ship;
            _description = description;
            _commands = commands;
            _joystickDirection = Vector2.zero;
            _leftPressed = false;
            _rightPressed = false;
            _activeButtons = 0;

            var buttons = new Dictionary<string, RectTransform>();
            foreach (Transform item in transform)
            {
                if (item.gameObject == _navigationPanel.gameObject)
                    continue;

                item.gameObject.SetActive(false);
                buttons.Add(item.name, item.GetComponent<RectTransform>());
            }

            var layout = _gameSettings.ControlsLayout;
            if (string.IsNullOrEmpty(layout))
            {
                layout = "Action0,1430,100,180 Action1,1240,100,180 Action2,1050,100,180 Action3,860,100,180 Action4,670,100,180 Action5,480,100,180 Joystick,220,160,300";
                _slideToMove = false;
                _thrustWithJoystick = true;
                _stopWhenWeaponActive = false;
            }
            else
            {
                _slideToMove = _gameSettings.SlideToMove;
                _thrustWithJoystick = _gameSettings.ThrustWidthJoystick;
                _stopWhenWeaponActive = _gameSettings.StopWhenWeaponActive;
            }

#if UNITY_STANDALONE
            _navigationPanel.gameObject.SetActive(false);
            UpdateLayout(buttons, "Action0,1470,70,90 Action1,1370,70,90 Action2,1270,70,90 Action3,1170,70,90 Action4,1070,70,90 Action5,970,70,90");
#else
            _navigationPanel.gameObject.SetActive(_slideToMove);
            _navigationPanel.Initialize(ship, commands);
            _navigationPanel.AllowThrust = _thrustWithJoystick;
            UpdateLayout(buttons, layout);
#endif
            UpdateActionButtons(buttons);
        }

        private void UpdateLayout(IDictionary<string, RectTransform> items, string layoutData)
        {
            foreach (var layout in layoutData.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                var data = layout.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                var id = data[0];
                var x = System.Convert.ToSingle(data[1]);
                var y = System.Convert.ToSingle(data[2]);
                var size = System.Convert.ToSingle(data[3]);
                var doubleTaps = data.Length <= 4 || System.Convert.ToInt32(data[4]) == 0;

                RectTransform item;
                if (!items.TryGetValue(id, out item))
                {
                    UnityEngine.Debug.Log("Button not found: " + id);
                    continue;
                }

                var button = item.GetComponent<ActionButton>();
                if (button != null)
                    button.EnableDoubleTaps = doubleTaps;

                item.gameObject.SetActive(true);
                item.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                item.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
                item.anchoredPosition = new Vector2(x, y);
            }
        }

        private void UpdateActionButtons(IDictionary<string, RectTransform> buttons)
        {
            _keyBindings = _description.KeyBindings;
            _buttons = new List<ActionButton>();

            RectTransform item;
            for (int i = 0; buttons.TryGetValue("Action" + i, out item); i++)
            {
                if (i < _keyBindings.Count)
                {
                    item.gameObject.SetActive(true);
                    var button = item.GetComponent<ActionButton>();
                    button.Image.sprite = _resourceLocator.GetSprite(_description.GetButtonIcon(_keyBindings[i][0]));
                    button.ReleaseImmediately = _description.ReleaseImmediately(i);
                    _buttons.Add(button);
                }
                else
                {
                    item.gameObject.SetActive(false);
                }
            }
        }

        public void Update()
        {
            if (!IsShipActive)
            {
                Window.Close(WindowExitCode.Ok);
                return;
            }

            bool isActive = ActiveButtons > 0;
            if (_joystickDirection != Vector2.zero)
            {
                var rotation = RotationHelpers.Angle(_joystickDirection);
                _commands.SetCourse(rotation);
                if (_thrustWithJoystick && !(_stopWhenWeaponActive && _activeButtons > 0))
                    _commands.SetThrottle(Mathf.Abs(Mathf.DeltaAngle(_ship.Rotation, rotation)) < 30 ? Mathf.Clamp01(_joystickDirection.magnitude * 2) : 0f);

                isActive = true;
            }

            if (_leftPressed)
            {
                _commands.SetCourse(_ship.Rotation + 175);
                isActive = true;
            }

            if (_rightPressed)
            {
                _commands.SetCourse(_ship.Rotation - 175);
                isActive = true;
            }

            for (int i = 0; i < _keyBindings.Count; ++i)
            {
                var item = _buttons[i];
                if (item == null)
                    continue;

                var systems = _keyBindings[i];
                var minCooldown = 1.0f;
                var maxCooldown = 0.0f;
                for (int j = 0; j < systems.Count; ++j)
                {
                    var cooldown = _description.GetCooldown(systems[j]);
                    if (cooldown < minCooldown)
                        minCooldown = cooldown;
                    if (cooldown > maxCooldown)
                        maxCooldown = cooldown;
                }

                item.SetCooldown(minCooldown/*, maxCooldown*/);
            }

            if (isActive)
                _playerInputTrigger.Fire();
        }

        private int ActiveButtons
        {
            get
            {
                return _activeButtons;
            }
            set
            {
                _activeButtons = value;
                if (_activeButtons > 0 && _stopWhenWeaponActive && IsShipActive)
                {
                    _commands.SetThrottle(0);
                }

                _navigationPanel.AllowThrust = _thrustWithJoystick && !(_stopWhenWeaponActive && _activeButtons > 0);
            }
        }

        private AnimatedWindow Window { get { return _window ?? (_window = GetComponent<AnimatedWindow>()); } }

        private bool IsShipActive => _ship != null && _ship.IsActive;

        private IReadOnlyList<IReadOnlyList<int>> _keyBindings;
        private List<ActionButton> _buttons;

        private int _activeButtons;
        private bool _leftPressed;
        private bool _rightPressed;
        private Vector2 _joystickDirection;

        private bool _slideToMove;
        private bool _thrustWithJoystick;
        private bool _stopWhenWeaponActive;

        private IShipView _ship;
        private IShipControlDescription _description;
        private IShipCommandSink _commands;

        private AnimatedWindow _window;
    }
}
