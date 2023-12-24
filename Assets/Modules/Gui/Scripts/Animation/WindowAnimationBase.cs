using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Gui.Presenter;

namespace Gui.Animation
{
    [RequireComponent(typeof(PresenterBase)), DisallowMultipleComponent]
    public abstract class WindowAnimationBase : MonoBehaviour
    {
		private const string PropertyNameDisplay = "display";
		private const string PropertyNameAll = "all";
		private const float DisplayTransitionDuration = 0.001f;

		[SerializeField] private float _duration = 0.5f;
        [SerializeField] private EasingMode _easing = EasingMode.EaseInOutSine;
        [SerializeField] private bool _initiallyVisible;

        [SerializeField] private UnityEngine.Events.UnityEvent _animationFinished;
        [SerializeField] private UnityEngine.Events.UnityEvent<bool> _valueChanged;

        private PresenterBase _presenter;
        private bool _visible;

		private void OnEnable()
		{
			RootElement.RegisterCallback<TransitionStartEvent>(OnTransitionStarted, TrickleDown.TrickleDown);
			RootElement.RegisterCallback<TransitionEndEvent>(OnTransitionEnded, TrickleDown.TrickleDown);
		}

		private void OnDisable()
		{
			RootElement.UnregisterCallback<TransitionStartEvent>(OnTransitionStarted, TrickleDown.TrickleDown);
			RootElement.UnregisterCallback<TransitionEndEvent>(OnTransitionEnded, TrickleDown.TrickleDown);
		}

		private void OnTransitionStarted(TransitionStartEvent e)
		{
			//var names = string.Join(',', e.stylePropertyNames.ToArray());
			//Debug.LogError($"OnTransitionStarted {RootElement.name} - {names}");
		}

		private void OnTransitionEnded(TransitionEndEvent e)
		{
			if (e.stylePropertyNames.Contains(PropertyNameDisplay))
				return;

			//var names = string.Join(',', e.stylePropertyNames.ToArray());
			//Debug.LogError($"OnTransitionEnded {RootElement.name} - {names}");

			if (!_visible) 
				ShowElement(false);

			_animationFinished?.Invoke();
		}

		public event UnityEngine.Events.UnityAction AnimationFinished
        {
            add => (_animationFinished ??= new()).AddListener(value);
            remove => (_animationFinished ??= new()).RemoveListener(value);
        }

        public event UnityEngine.Events.UnityAction<bool> ValueChanged
        {
            add => (_valueChanged ??= new()).AddListener(value);
            remove => (_valueChanged ??= new()).RemoveListener(value);
        }

        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible == value) return;
				_visible = value;
				
				if (_visible) ShowElement(true);
				SetElementState(_visible);
                _valueChanged?.Invoke(_visible);
			}
		}

        protected VisualElement RootElement => _presenter.RootElement;

        protected abstract void SetElementState(bool visible);

		private void Awake()
        {
            _presenter = GetComponent<PresenterBase>();

			var element = _presenter.RootElement;
			element.style.transitionProperty = new List<StylePropertyName> { PropertyNameAll, PropertyNameDisplay };
			element.style.transitionDuration = new List<TimeValue> { _duration, DisplayTransitionDuration };
			element.style.transitionTimingFunction = new(new List<EasingFunction> { _easing });

			_visible = _initiallyVisible;
		}

		private void Start()
		{
			SetElementState(_visible);
			ShowElement(_visible);
		}

		private void ShowElement(bool visible)
		{
			//Debug.LogError($"ShowElement {RootElement.name} - {visible}");
			RootElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
		}
	}
}
