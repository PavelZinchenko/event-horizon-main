using UnityEngine.InputSystem;
using Services.Input;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Services.Audio;
using Services.Settings;
using Services.Localization;

namespace Gui.MainMenu
{
    public class KeyBindingPanel : MonoBehaviour
    {
        [Inject] private readonly ISoundPlayer _soundPlayer;
        [Inject] private readonly IGameSettings _settings;
        [Inject] private readonly IKeyNameLocalizer _localizer;
        [Inject] private readonly KeyBindingsChangedSignal.Trigger _keyBindingsChangedTrigger;
        [Inject] private readonly MouseEnabledSignal.Trigger _mouseEnabledTrigger;

        [SerializeField] private ToggleGroup _toggleGroup;

        [SerializeField] private Toggle _toggleLeft;
        [SerializeField] private Toggle _toggleRight;
        [SerializeField] private Toggle _toggleForward;
        [SerializeField] private Toggle _toggleAction1;
        [SerializeField] private Toggle _toggleAction2;
        [SerializeField] private Toggle _toggleAction3;
        [SerializeField] private Toggle _toggleAction4;
        [SerializeField] private Toggle _toggleAction5;
        [SerializeField] private Toggle _toggleAction6;

        [SerializeField] private Text _leftText;
        [SerializeField] private Text _rightText;
        [SerializeField] private Text _forwardText;
        [SerializeField] private Text _actionText1;
        [SerializeField] private Text _actionText2;
        [SerializeField] private Text _actionText3;
        [SerializeField] private Text _actionText4;
        [SerializeField] private Text _actionText5;
        [SerializeField] private Text _actionText6;

        [SerializeField] private Toggle _useMouseToggle;

        [SerializeField] private AudioClip _audioClip;

        public void Awake()
        {
            _inputActions = new InputActions();
        }

        public void OnEnable()
        {
            var config = Configuration.FromJson(_settings.KeyBindings);
            config.Apply(_inputActions.Combat);

            UpdateButtonsText();
            _toggleGroup.SetAllTogglesOff(false);
            _useMouseToggle.isOn = _settings.UseMouse;
        }

        public void OnDisable()
        {
            CancelOperation();
            var config = Configuration.Create(_inputActions.Combat).ToJson();
            if (config == _settings.KeyBindings) return;

            _settings.KeyBindings = config;
            _keyBindingsChangedTrigger.Fire(_settings.KeyBindings);
        }

        public void SetUseMouse(bool selected)
        {
            if (_settings.UseMouse == selected) return;
            _settings.UseMouse = selected;
            _mouseEnabledTrigger.Fire(selected);
        }

        public void OnButtonClicked(bool selected)
        {
            CancelOperation();

            if (!selected) return;

            var actions = _inputActions.Combat;

            if (_toggleLeft.isOn)
            {
                var left = actions.Horizontal.bindings.IndexOf(b => b.name == "negative");
                _operation = actions.Horizontal.PerformInteractiveRebinding(left);
            }
            else if (_toggleRight.isOn)
            {
                var right = actions.Horizontal.bindings.IndexOf(b => b.name == "positive");
                _operation = actions.Horizontal.PerformInteractiveRebinding(right);
            }
            else if (_toggleForward.isOn)
            {
                var up = actions.Vertical.bindings.IndexOf(b => b.name == "positive");
                _operation = actions.Vertical.PerformInteractiveRebinding(up);
            }
            else if (_toggleAction1.isOn)
                _operation = actions.Fire1.PerformInteractiveRebinding(0);
            else if (_toggleAction2.isOn)
                _operation = actions.Fire2.PerformInteractiveRebinding(0);
            else if (_toggleAction3.isOn)
                _operation = actions.Fire3.PerformInteractiveRebinding(0);
            else if (_toggleAction4.isOn)
                _operation = actions.Fire4.PerformInteractiveRebinding(0);
            else if (_toggleAction5.isOn)
                _operation = actions.Fire5.PerformInteractiveRebinding(0);
            else if (_toggleAction6.isOn)
                _operation = actions.Fire6.PerformInteractiveRebinding(0);
            else
                return;

            _operation.
                WithControlsExcluding("Mouse").
                WithControlsExcluding("<Keyboard>/escape").
                WithControlsExcluding("<Gamepad>/select").
                OnMatchWaitForAnother(0.1f).
                OnComplete(ButtonRebindCompleted).
                Start();
        }

        private InputActionRebindingExtensions.RebindingOperation RebindKey(InputAction actionToRebind)
        {
            var rebindOperation = actionToRebind.PerformInteractiveRebinding(0)
                .WithControlsHavingToMatchPath("<Keyboard>/")
                .WithControlsExcluding("<Keyboard>/escape") //to prevent the player from re binding the pause key
                .WithControlsExcluding("<Gamepad>/select") //to prevent the player from re binding the pause key
                .WithControlsExcluding("<Pointer>/position") // Don't bind to mouse position
                .WithControlsExcluding("<Pointer>/delta"); // Don't bind to mouse movement deltas

            return rebindOperation;
        }

        private void CancelOperation()
        {
            if (_operation == null) return;
            _operation.Cancel();
            _operation.Dispose();
            _operation = null;
            _toggleGroup.SetAllTogglesOff();
        }

        private void ButtonRebindCompleted(InputActionRebindingExtensions.RebindingOperation operation)
        {
            CancelOperation();
            UpdateButtonsText();
            _soundPlayer.Play(_audioClip);
        }

        private void UpdateButtonsText()
        {
            var actions = _inputActions.Combat;

            var left = actions.Horizontal.bindings.IndexOf(b => b.name == "negative");
            var right = actions.Horizontal.bindings.IndexOf(b => b.name == "positive");
            var up = actions.Vertical.bindings.IndexOf(b => b.name == "positive");
            _leftText.text = _localizer.Localize(actions.Horizontal.bindings[left] .effectivePath);
            _rightText.text = _localizer.Localize(actions.Horizontal.bindings[right].effectivePath);
            _forwardText.text = _localizer.Localize(actions.Vertical.bindings[up].effectivePath);
            _actionText1.text = _localizer.Localize(actions.Fire1.bindings[0].effectivePath);
            _actionText2.text = _localizer.Localize(actions.Fire2.bindings[0].effectivePath);
            _actionText3.text = _localizer.Localize(actions.Fire3.bindings[0].effectivePath);
            _actionText4.text = _localizer.Localize(actions.Fire4.bindings[0].effectivePath);
            _actionText5.text = _localizer.Localize(actions.Fire5.bindings[0].effectivePath);
            _actionText6.text = _localizer.Localize(actions.Fire6.bindings[0].effectivePath);
        }

        private InputActions _inputActions;
        private InputActionRebindingExtensions.RebindingOperation _operation;
    }
}
