using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ReUI
{
    /// <summary>
    /// Semantic presentation roles retained by the ReUI stylers.  UITest1 used
    /// a third-party effect package here; Beta5 intentionally uses stock UGUI
    /// graphics so authored controls keep their own size, masks and state art.
    /// </summary>
    public enum ReUIEffectRole
    {
        Panel,
        Popup,
        PrimaryButton,
        SecondaryButton,
        NavigationButton,
        SelectedButton,
        DisabledButton,
        Icon,
        SelectedIcon,
        HudBar,
        DangerButton,
    }

    [DisallowMultipleComponent]
    public sealed class ReUIEffectMarker : MonoBehaviour
    {
        public ReUIEffectRole Role;
    }

    /// <summary>
    /// A small stock-UGUI hover treatment. It only updates an outline that was
    /// explicitly created by an existing ReUI styler and never alters layout,
    /// alpha, button transitions or child graphics.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    internal sealed class ReUIEffectInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Selectable _selectable;
        private ReUIEffectMarker _marker;
        private Outline _outline;
        private Color _restingColor;
        private bool _hasRestingColor;

        private void Awake()
        {
            Cache();
        }

        private void OnEnable()
        {
            Cache();
            Restore();
        }

        internal void Configure(ReUIEffectMarker marker, Image image)
        {
            _marker = marker;
            _outline = image != null ? image.GetComponent<Outline>() : null;
            if (_outline != null)
            {
                _restingColor = _outline.effectColor;
                _hasRestingColor = true;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_selectable == null || !_selectable.IsInteractable() || _outline == null)
                return;

            Color accent = ReUIEffectStyler.AccentFor(_marker != null ? _marker.Role : ReUIEffectRole.SecondaryButton);
            _outline.effectColor = ReUIPalette.WithAlpha(accent, Mathf.Max(_restingColor.a, 0.72f));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Restore();
        }

        private void Cache()
        {
            if (_selectable == null) _selectable = GetComponent<Selectable>();
            if (_marker == null) _marker = GetComponent<ReUIEffectMarker>();
            if (_outline == null)
            {
                Image image = _selectable != null ? _selectable.targetGraphic as Image : null;
                if (image == null) image = GetComponent<Image>();
                if (image != null)
                {
                    _outline = image.GetComponent<Outline>();
                    if (_outline != null)
                    {
                        _restingColor = _outline.effectColor;
                        _hasRestingColor = true;
                    }
                }
            }
        }

        private void Restore()
        {
            if (_outline != null && _hasRestingColor)
                _outline.effectColor = _restingColor;
        }
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Toggle))]
    internal sealed class ReUIToggleGlassState : MonoBehaviour
    {
        private Toggle _toggle;
        private bool _subscribed;

        private void OnEnable()
        {
            Cache();
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            if (_subscribed && _toggle != null)
                _toggle.onValueChanged.RemoveListener(OnValueChanged);
            _subscribed = false;
        }

        internal void Configure()
        {
            Cache();
            Subscribe();
            Refresh();
        }

        private void Cache()
        {
            if (_toggle == null) _toggle = GetComponent<Toggle>();
        }

        private void Subscribe()
        {
            if (_subscribed || _toggle == null) return;
            _toggle.onValueChanged.AddListener(OnValueChanged);
            _subscribed = true;
        }

        private void OnValueChanged(bool _)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_toggle == null) return;
            ReUIEffectStyler.ApplySelectable(_toggle,
                _toggle.isOn ? ReUIEffectRole.SelectedButton : ReUIEffectRole.SecondaryButton);
        }
    }

    /// <summary>
    /// Compatibility façade for the existing ReUI stylers. It deliberately has
    /// no package dependency, no custom material and no layout side effects.
    /// </summary>
    public static class ReUIEffectStyler
    {
        public static ReUIEffectMarker ApplyPanel(Image image, bool elevated = false)
        {
            return Apply(image, elevated ? ReUIEffectRole.Popup : ReUIEffectRole.Panel);
        }

        public static ReUIEffectMarker ApplyButton(Button button, ReUIEffectRole role)
        {
            return ApplySelectable(button, role);
        }

        public static ReUIEffectMarker ApplyToggle(Toggle toggle)
        {
            if (toggle == null) return null;
            ReUIEffectMarker marker = ApplySelectable(toggle,
                toggle.isOn ? ReUIEffectRole.SelectedButton : ReUIEffectRole.SecondaryButton);
            if (marker == null) return null;

            ReUIToggleGlassState state = toggle.GetComponent<ReUIToggleGlassState>();
            if (state == null) state = toggle.gameObject.AddComponent<ReUIToggleGlassState>();
            state.Configure();
            return marker;
        }

        public static ReUIEffectMarker ApplySelectable(Selectable selectable, ReUIEffectRole role)
        {
            if (selectable == null) return null;
            Image image = selectable.targetGraphic as Image;
            if (image == null) image = selectable.GetComponent<Image>();
            ReUIEffectMarker marker = Apply(image, role);
            if (marker == null) return null;

            ReUIEffectInteraction interaction = selectable.GetComponent<ReUIEffectInteraction>();
            if (interaction == null) interaction = selectable.gameObject.AddComponent<ReUIEffectInteraction>();
            interaction.Configure(marker, image);
            return marker;
        }

        public static ReUIEffectMarker Apply(Image image, ReUIEffectRole role)
        {
            if (image == null || image.GetComponent<Mask>() != null || image.GetComponent<RectMask2D>() != null)
                return null;

            // Resetting a graphic material returns the target to Unity's normal
            // UGUI path without replacing its sprite, dimensions or colour.
            image.material = null;

            ReUIEffectMarker marker = image.GetComponent<ReUIEffectMarker>();
            if (marker == null) marker = image.gameObject.AddComponent<ReUIEffectMarker>();
            marker.Role = role;

            Outline outline = image.GetComponent<Outline>();
            if (outline != null)
            {
                Color accent = AccentFor(role);
                float alpha = DefaultOutlineAlpha(role, outline.effectColor.a);
                outline.effectColor = ReUIPalette.WithAlpha(accent, alpha);
            }

            return marker;
        }

        internal static Color AccentFor(ReUIEffectRole role)
        {
            return role == ReUIEffectRole.DangerButton ? ReUIPalette.AccentRed :
                role == ReUIEffectRole.DisabledButton ? ReUIPalette.TextMuted :
                role == ReUIEffectRole.Icon || role == ReUIEffectRole.SelectedIcon
                    ? ReUIPalette.TextSecondary
                    : ReUIPalette.AccentCyan;
        }

        private static float DefaultOutlineAlpha(ReUIEffectRole role, float existing)
        {
            float fallback = role switch
            {
                ReUIEffectRole.Popup => 0.56f,
                ReUIEffectRole.PrimaryButton => 0.62f,
                ReUIEffectRole.SelectedButton => 0.72f,
                ReUIEffectRole.DangerButton => 0.70f,
                ReUIEffectRole.DisabledButton => 0.24f,
                _ => 0.42f,
            };
            return Mathf.Max(existing, fallback);
        }
    }
}
