using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ActionButton : UIBehaviour, ICanvasRaycastFilter, IPointerDownHandler, IPointerUpHandler
{
    private enum ButtonState
    {
        Normal,
        Locked,
        CanBeLocked,
        Disabled,
    }

    [SerializeField] private Image _image;
    [SerializeField] private Image _cooldownImage;

    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _pressedColor;
    [SerializeField] private Color _lockedColor;
    [SerializeField] private Color _disabledColor;

    [SerializeField] private float _doubleClickInterval = 0.3f;
    [SerializeField] private float _unlockIfOnCooldownAfter = 2.0f;

    [SerializeField] public UnityEvent _buttonPressed = new UnityEvent();
    [SerializeField] public UnityEvent _buttonReleased = new UnityEvent();

    private RectTransform _rectTransform;
    private double _lastPressTime;
    private float _timeSinceOnCooldown;
    private ButtonState _state;
    private bool _pressed;
    private bool _active;

    public Image Image => _image;
    public bool EnableDoubleTaps { get; set; }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateState(ButtonState.Normal, false);
        _cooldownImage.enabled = false;
    }

    private void Update()
    {
        if (_state != ButtonState.CanBeLocked) return;
        var time = Time.unscaledTimeAsDouble;
        if (time - _lastPressTime < _doubleClickInterval) return;
        UpdateState(ButtonState.Normal, _pressed);
    }

    public bool ReleaseImmediately { get; set; }

    public bool Enabled
    {
        get => _state != ButtonState.Disabled;
        set
        {
            if (value == (_state != ButtonState.Disabled)) return;
            UpdateState(value ? ButtonState.Normal : ButtonState.Disabled, _pressed);
        }
    }

    public void SetCooldown(float cooldown)
    {
        if (cooldown > 0.01f)
        {
            _cooldownImage.enabled = true;
            _cooldownImage.fillAmount = cooldown;
            _timeSinceOnCooldown += Time.fixedUnscaledDeltaTime;

            if (_state == ButtonState.Locked)
                if (_timeSinceOnCooldown >= _unlockIfOnCooldownAfter)
                    UpdateState(ButtonState.Disabled, _pressed);
        }
        else
        {
            _timeSinceOnCooldown = 0;
            _cooldownImage.enabled = false;
            if (_state == ButtonState.Disabled)
                UpdateState(ButtonState.Normal, _pressed);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var newState = _state;

        if (_state == ButtonState.Locked && Time.unscaledTimeAsDouble - _lastPressTime > _doubleClickInterval)
            newState = ButtonState.Normal;

        UpdateState(newState, true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        var newState = _state;
        var time = Time.unscaledTimeAsDouble;

        if (EnableDoubleTaps)
        {
            if (_state == ButtonState.Normal)
            {
                if (time - _lastPressTime < _doubleClickInterval)
                    newState = ButtonState.Locked;
                else if (!ReleaseImmediately)
                    newState = ButtonState.CanBeLocked;
            }
            else if (_state == ButtonState.CanBeLocked)
            {
                if (time - _lastPressTime < _doubleClickInterval)
                    newState = ButtonState.Locked;
            }
        }

        _lastPressTime = time;
        UpdateState(newState, false);
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			RectTransform, screenPoint, eventCamera, out var pivotToCursorVector);		
		return pivotToCursorVector.magnitude < RectTransform.rect.width/2;
	}

    private void UpdateState(ButtonState state, bool pressed)
    {
        if (state == _state && _pressed == pressed) return;
        _state = state;
        _pressed = pressed;

        bool active = false;
        switch (_state)
        {
            case ButtonState.Normal:
                _image.color = _pressed ? _pressedColor : _normalColor;
                active = _pressed;
                break;
            case ButtonState.CanBeLocked:
                _image.color = _pressedColor;
                active = true;
                break;
            case ButtonState.Locked:
                _image.color = _lockedColor;
                active = true;
                break;
            case ButtonState.Disabled:
                _image.color = _disabledColor;
                active = false;
                break;
        }

        if (_active != active)
        {
            _active = active;

            if (_active)
                _buttonPressed.Invoke();
            else
                _buttonReleased.Invoke();
        }
    }

    private RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
				_rectTransform = GetComponent<RectTransform>();
			return _rectTransform;
		}
	}
}
