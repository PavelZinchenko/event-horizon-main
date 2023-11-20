using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using Services.Gui;

namespace Gui.Window
{
    [RequireComponent(typeof(Animation.WindowAnimationBase), typeof(UIDocument))]
    public class UiDocumentWindow : MonoBehaviour, IWindow
    {
        [Inject] WindowOpenedSignal.Trigger _windowOpenedTrigger;
        [Inject] WindowClosedSignal.Trigger _windowClosedTrigger;

        [SerializeField] private WindowClass _class = WindowClass.Singleton;
        [SerializeField] private EscapeKeyAction _escapeKeyAction = EscapeKeyAction.None;

        private Animation.WindowAnimationBase _animation;
        private UIDocument _uiDocument;
        private bool _isEnabled = true;

        public string Id => name;
        public WindowClass Class => _class;

        public bool IsVisible => _animation.Visible;
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
            _uiDocument = GetComponent<UIDocument>();
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
        }

        private void OnVisibilityChanged(bool visible)
        {
            UpdateWindowState();
        }

        private void UpdateWindowState()
        {
            _uiDocument.rootVisualElement.SetEnabled(_isEnabled && _animation.Visible);
        }
    }
}
