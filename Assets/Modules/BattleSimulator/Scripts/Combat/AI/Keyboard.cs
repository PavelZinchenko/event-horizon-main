using UnityEngine;
using Services.Settings;
using Services.Input;
using Zenject;
using System;
using UnityEngine.InputSystem;

namespace Combat.Ai
{
    public interface IKeyboard
    {
        bool Throttle { get; }
        bool Left { get; }
        bool Right { get; }
        float JoystickX { get; }
        float JoystickY { get; }
        bool Action(int index);
    }

    public class NullKeyboard : IKeyboard
    {
        public bool Throttle => false;
        public bool Left => false;
        public bool Right => false;
        public float JoystickX => 0f;
        public float JoystickY => 0f;
        public bool Action(int index) { return false; }
    }

    public class InputSystemKeyboard : IKeyboard, IInitializable, IDisposable
	{
        [Inject]
        public InputSystemKeyboard(IGameSettings settings, KeyBindingsChangedSignal keyBindingsChangedSignal)
        {
            var configuration = new Configuration();

            try
            {
                configuration = Configuration.FromJson(settings.KeyBindings);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"InputSystemKeyboard: failed to parse {settings.KeyBindings}");
            }

            _combatActions = new InputActions().Combat;
            configuration.Apply(_combatActions);

            _keyBindingsChangedSignal = keyBindingsChangedSignal;
            _keyBindingsChangedSignal.Event += OnKeyBindingsChanged;
        }

        private void OnKeyBindingsChanged(string config)
        {
            _combatActions.Disable();
            var configuration = Configuration.FromJson(config);
            configuration.Apply(_combatActions);
            _combatActions.Enable();
        }

        public void Initialize()
        {
            Subscribe(_combatActions.Move, OnMoveAction);
            Subscribe(_combatActions.Horizontal, OnHorizontalAction);
            Subscribe(_combatActions.Vertical, OnVerticallAction);
            Subscribe(_combatActions.Fire1, OnFire1Action);
            Subscribe(_combatActions.Fire2, OnFire2Action);
            Subscribe(_combatActions.Fire3, OnFire3Action);
            Subscribe(_combatActions.Fire4, OnFire4Action);
            Subscribe(_combatActions.Fire5, OnFire5Action);
            Subscribe(_combatActions.Fire6, OnFire6Action);

            _combatActions.Enable();
        }

        public void Dispose()
        {
            _combatActions.Disable();

            Unsubscribe(_combatActions.Move, OnMoveAction);
            Unsubscribe(_combatActions.Horizontal, OnHorizontalAction);
            Unsubscribe(_combatActions.Vertical, OnVerticallAction);
            Unsubscribe(_combatActions.Fire1, OnFire1Action);
            Unsubscribe(_combatActions.Fire2, OnFire2Action);
            Unsubscribe(_combatActions.Fire3, OnFire3Action);
            Unsubscribe(_combatActions.Fire4, OnFire4Action);
            Unsubscribe(_combatActions.Fire5, OnFire5Action);
            Unsubscribe(_combatActions.Fire6, OnFire6Action);
        }

        private static void Subscribe(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            action.performed += callback;
            action.canceled += callback;
            action.started += callback;
        }

        private static void Unsubscribe(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            action.performed -= callback;
            action.canceled -= callback;
            action.started -= callback;
        }

        public bool Throttle { get; private set; }
        public bool Left { get; private set; }
		public bool Right { get; private set; }
        public float JoystickX { get; private set; }
        public float JoystickY { get; private set; }
        public bool Action(int index) { return index >= 0 && index < _actions.Length && _actions[index]; }

		private void OnHorizontalAction(UnityEngine.InputSystem.InputAction.CallbackContext context)
		{
            Left = context.ReadValue<float>() < 0;
            Right = context.ReadValue<float>() > 0;
        }

        private void OnVerticallAction(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            Throttle = context.ReadValue<float>() > 0;
        }

        private void OnMoveAction(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            var value = context.ReadValue<Vector2>();
            JoystickX = value.x;
            JoystickY = value.y;
        }

        private void OnFire1Action(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            _actions[0] = context.ReadValueAsButton();
        }

        private void OnFire2Action(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            _actions[1] = context.ReadValueAsButton();
        }

        private void OnFire3Action(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            _actions[2] = context.ReadValueAsButton();
        }

        private void OnFire4Action(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            _actions[3] = context.ReadValueAsButton();
        }

        private void OnFire5Action(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            _actions[4] = context.ReadValueAsButton();
        }

        private void OnFire6Action(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            _actions[5] = context.ReadValueAsButton();
        }

        private InputActions.CombatActions _combatActions;
        private readonly KeyBindingsChangedSignal _keyBindingsChangedSignal;
        private readonly bool[] _actions = { false, false, false, false, false, false };
	}
}
