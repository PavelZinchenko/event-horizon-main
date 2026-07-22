using UnityEngine;
using UnityEngine.UI;

namespace Gui.DebugConsole
{
    public class DebugConsoleLogEntry : MonoBehaviour
    {
        [SerializeField] private Text _message;
        [SerializeField] private Text _stackTrace;
        [SerializeField] private GameObject _icon;
        [SerializeField] private Toggle _toggle;

        public void Initialize(LogEntry entry)
        {
            if (entry.Type == LogType.Log)
                _message.text = entry.Message;
            else
                _message.text = $"[{entry.Type}] {entry.Message}";

            bool hasStackTraces = !string.IsNullOrEmpty(entry.StackTrace);
            _icon.SetActive(hasStackTraces);
            _toggle.interactable = hasStackTraces;

            _stackTrace.gameObject.SetActive(false);
            _stackTrace.text = entry.StackTrace;
        }

        public void OnValueChanged(bool value)
        {
            _stackTrace.gameObject.SetActive(value);
        }
    }
}
