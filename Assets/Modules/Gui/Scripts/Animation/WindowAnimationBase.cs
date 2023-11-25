using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Gui.Presenter;

namespace Gui.Animation
{
    [RequireComponent(typeof(PresenterBase)), DisallowMultipleComponent]
    public abstract class WindowAnimationBase : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private EasingMode _easing = EasingMode.EaseInOutSine;
        [SerializeField] private bool _initiallyVisible;

        [SerializeField] private UnityEngine.Events.UnityEvent _animationFinished;
        [SerializeField] private UnityEngine.Events.UnityEvent<bool> _valueChanged;

        private PresenterBase _presenter;
        private bool _visible;
        private bool _stateChanged;
        private bool _isAnimationRunning;

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
                _stateChanged = true;

                if (!_isAnimationRunning)
                    StartCoroutine(StartAnimation());

                _valueChanged?.Invoke(_visible);
            }
        }

        protected VisualElement RootElement => _presenter.RootElement;

        protected abstract void ShowElement(bool visible);

        private void Awake()
        {
            _presenter = GetComponent<PresenterBase>();

            var element = _presenter.RootElement;
            element.style.transitionDuration = new List<TimeValue>() { new TimeValue(_duration, TimeUnit.Second) };
            element.style.transitionTimingFunction = new(new List<EasingFunction>() { new EasingFunction(_easing) });

            _visible = _initiallyVisible;
            ShowElement(_visible);
        }

        private IEnumerator StartAnimation()
        {
            _isAnimationRunning = true;

            float elapsedTime = 0f;
            yield return null;

            while (elapsedTime < _duration)
            {
                if (_stateChanged)
                {
                    elapsedTime = 0f;
                    _stateChanged = false;

                    ShowElement(_visible);
                }

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            _isAnimationRunning = false;
            _animationFinished?.Invoke();
        }
    }
}
