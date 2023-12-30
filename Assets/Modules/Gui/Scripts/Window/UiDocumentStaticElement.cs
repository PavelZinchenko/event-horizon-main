using UnityEngine;
using UnityEngine.Events;
using Zenject;
using Services.Gui;
using Gui.Presenter;

namespace Gui.Window
{
    [RequireComponent(typeof(PresenterBase))]
    public class UiDocumentStaticElement : MonoBehaviour//, IWindow
    {
        [Inject] WindowOpenedSignal.Trigger _windowOpenedTrigger;
        [Inject] WindowClosedSignal.Trigger _windowClosedTrigger;

		[SerializeField] private UnityEvent WindowEnabled;
		[SerializeField] private UnityEvent WindowDisabled;

		private PresenterBase _presenter;
        private bool _isEnabled = true;

        public string Id => name;
        public WindowClass Class => WindowClass.HudElement;

        public bool IsVisible { get; private set; }
        public EscapeKeyAction EscapeAction => EscapeKeyAction.None;

        public bool Enabled 
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
				_presenter.RootElement.SetEnabled(_isEnabled);
				
				if (_isEnabled)
					WindowEnabled?.Invoke();
				else
					WindowDisabled?.Invoke();
			}
		}

        private void Awake()
        {
            _presenter = GetComponent<PresenterBase>();
        }

        private void Start()
        {
            Open();
        }

        public void Open(WindowArgs args)
        {
			IsVisible = true;
			_windowOpenedTrigger.Fire(Id);
        }

        public void Open()
        {
            Open(null);
        }

        public void Close()
        {
			IsVisible = false;
			_windowClosedTrigger.Fire(Id, WindowExitCode.Cancel);
        }
    }
}
