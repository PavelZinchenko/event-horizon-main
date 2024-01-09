using Services.Gui;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Gui.Utils
{
    public class BackButtonEvent : MonoBehaviour
    {
        [SerializeField] private UnityEvent _backButtonPressed = new UnityEvent();

        [SerializeField] private bool _ignoreIfStandaloneBuild = false;

		private EscapeKeyPressedSignal _escapeKeyPressedSignal;

		[Inject]
        private void Initialize(EscapeKeyPressedSignal escapeKeyPressedSignal)
        {
#if UNITY_STANDALONE
            if (_ignoreIfStandaloneBuild) return;
#endif
			_escapeKeyPressedSignal = escapeKeyPressedSignal;
			_escapeKeyPressedSignal.Event += OnCancel;
        }

        private void OnCancel()
        {
            if (this) _backButtonPressed.Invoke();
        }
    }
}
