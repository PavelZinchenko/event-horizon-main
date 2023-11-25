using UnityEngine;
using Zenject;
using Services.Gui;
using Gui.Presenter;

namespace Gui.Window
{
    [RequireComponent(typeof(Animation.WindowAnimationBase), typeof(PresenterBase))]
    public class UiDocumentWindow : MonoBehaviour, IWindow
    {
        [Inject] WindowOpenedSignal.Trigger _windowOpenedTrigger;
        [Inject] WindowClosedSignal.Trigger _windowClosedTrigger;

        [SerializeField] private WindowClass _class = WindowClass.Singleton;
        [SerializeField] private EscapeKeyAction _escapeKeyAction = EscapeKeyAction.None;

        private Animation.WindowAnimationBase _animation;
        private PresenterBase _presenter;
        private bool _isEnabled = true;

        public string Id => name;
        public WindowClass Class => _class;

        public bool IsVisible => _animation != null ? _animation.Visible : false;
        public EscapeKeyAction EscapeAction => _escapeKeyAction;

        public bool Enabled 
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                UpdateWindowState();
            }
        }

        private void Awake()
        {
            _presenter = GetComponent<PresenterBase>();
            _animation = GetComponent<Animation.WindowAnimationBase>();
        }

        private void OnEnable()
        {
            _animation.AnimationFinished += OnAnimationFinished;
            _animation.ValueChanged += OnVisibilityChanged;
        }

        private void OnDisable()
        {
            _animation.AnimationFinished -= OnAnimationFinished;
            _animation.ValueChanged -= OnVisibilityChanged;
        }

        private void Start()
        {
            Open();
        }

        public void Open(WindowArgs args)
        {
            gameObject.SetActive(true);
            _animation.Visible = true;
            _windowOpenedTrigger.Fire(Id);
        }

        public void Open()
        {
            Open(null);
        }

        public void Close()
        {
            _animation.Visible = false;
            _windowClosedTrigger.Fire(Id, WindowExitCode.Cancel);
        }

        private void OnAnimationFinished()
        {
            if (!_animation.Visible)
                gameObject.SetActive(false);
        }

        private void OnVisibilityChanged(bool visible)
        {
            UpdateWindowState();
        }

        private void UpdateWindowState()
        {
            _presenter.RootElement.SetEnabled(_isEnabled && _animation.Visible);
        }
    }
}
