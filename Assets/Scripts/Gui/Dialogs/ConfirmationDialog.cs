using Gui.Windows;
using Services.Gui;
using UnityEngine;
using UnityEngine.UI;

namespace Gui.Dialogs
{
    public sealed class ConfirmationDialogOptions
    {
        public ConfirmationDialogOptions(
            string message,
            string confirmLabel = null,
            string cancelLabel = null,
            string extraLabel = null,
            WindowExitCode confirmResult = WindowExitCode.Ok,
            WindowExitCode cancelResult = WindowExitCode.Cancel,
            WindowExitCode extraResult = WindowExitCode.Option1)
        {
            Message = message;
            ConfirmLabel = confirmLabel;
            CancelLabel = cancelLabel;
            ExtraLabel = extraLabel;
            ConfirmResult = confirmResult;
            CancelResult = cancelResult;
            ExtraResult = extraResult;
        }

        public string Message { get; }
        public string ConfirmLabel { get; }
        public string CancelLabel { get; }
        public string ExtraLabel { get; }
        public WindowExitCode ConfirmResult { get; }
        public WindowExitCode CancelResult { get; }
        public WindowExitCode ExtraResult { get; }
    }

    public class ConfirmationDialog : MonoBehaviour
    {
        [SerializeField] private Text _text;

        public void InitializeWindow(WindowArgs args)
        {
            var options = args.TryGet<ConfirmationDialogOptions>(0, out var dialogOptions)
                ? dialogOptions
                : new ConfirmationDialogOptions(args.Get<string>());

            _text.text = options.Message;
            EnsureButtons();

            ConfigureButton(_confirmButton, options.ConfirmLabel ?? "确定", options.ConfirmResult);
            ConfigureButton(_cancelButton, options.CancelLabel ?? "取消", options.CancelResult);

            if (string.IsNullOrWhiteSpace(options.ExtraLabel))
            {
                if (_extraButton != null)
                    _extraButton.gameObject.SetActive(false);
            }
            else
            {
                EnsureExtraButton();
                ConfigureButton(_extraButton, options.ExtraLabel, options.ExtraResult);
                _extraButton.gameObject.SetActive(true);
            }
        }

        public void ConfirmButtonClicked()
        {
            GetComponent<AnimatedWindow>().Close(WindowExitCode.Ok);
        }

        private void EnsureButtons()
        {
            if (_confirmButton != null && _cancelButton != null)
                return;

            var footer = transform.Find("Footer");
            if (footer == null)
                return;

            var buttons = footer.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0)
                _confirmButton = buttons[0];
            if (buttons.Length > 1)
                _cancelButton = buttons[1];
        }

        private void EnsureExtraButton()
        {
            if (_extraButton != null || _cancelButton == null)
                return;

            var clone = Instantiate(_cancelButton.gameObject, _cancelButton.transform.parent, false);
            clone.name = "ExtraButton";
            clone.transform.SetSiblingIndex(_cancelButton.transform.GetSiblingIndex() + 1);
            _extraButton = clone.GetComponent<Button>();
        }

        private void ConfigureButton(Button button, string caption, WindowExitCode result)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => GetComponent<AnimatedWindow>().Close(result));

            var label = button.GetComponentInChildren<Text>(true);
            if (label != null)
                label.text = caption;
        }

        private Button _confirmButton;
        private Button _cancelButton;
        private Button _extraButton;
    }
}
