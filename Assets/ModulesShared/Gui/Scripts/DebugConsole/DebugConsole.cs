using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gui.DebugConsole
{
    public class DebugConsole : MonoBehaviour
    {
        [SerializeField] private LayoutGroup _content;
        [SerializeField] private GameObject _button;
        [SerializeField] private int _maxMessageCount = 100;

        private readonly List<LogEntry> _messages = new();
        private IDebugConsoleLogger _debugLogger;

        [Inject]
        private void Initialize(IDebugConsoleLogger debugLogger)
        {
            _debugLogger = debugLogger;
            _debugLogger.MessageReceived += OnDebugMessageReceived;
        }

        private void OnDestroy()
        {
            _debugLogger.MessageReceived -= OnDebugMessageReceived;
        }

        public void OnDebugMessageReceived()
        {
            if (!this) return;
            _button.SetActive(true);
        }

        private void OnEnable()
        {
            _debugLogger.GetMessages(_messages, _maxMessageCount);
            _content.transform.InitializeElements<DebugConsoleLogEntry, LogEntry>(_messages, UpdateLine);
            _messages.Clear();
        }

        private void UpdateLine(DebugConsoleLogEntry logEntry, LogEntry entry)
        {
            logEntry.Initialize(entry);
        }        
    }
}
