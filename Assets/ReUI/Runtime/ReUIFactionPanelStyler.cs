using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    /// <summary>
    /// Keeps starbase facility buttons readable even when gameplay marks them as
    /// unavailable. Interactivity is never changed; only the excessive disabled
    /// dimming and inconsistent icon geometry are corrected.
    /// </summary>
    internal static class ReUIFactionPanelStyler
    {
        private static readonly string[] FacilityButtonNames =
        {
            "Store",
            "Factory",
            "Shipyard",
            "Mission",
            "Capture",
            "StarbaseDefenseButton",
            "PeacefulTransferButton",
            "Preview5JointAttackButton",
        };

        internal static void Apply(Canvas canvas)
        {
            if (canvas == null) return;

            Transform panel = ReUISpecializedVisuals.FindByName(canvas.transform, "FactionPanel");
            if (panel == null) return;

            for (int i = 0; i < FacilityButtonNames.Length; i++)
            {
                Transform target = ReUISpecializedVisuals.FindByName(panel, FacilityButtonNames[i]);
                Button button = target != null ? target.GetComponent<Button>() : null;
                if (button != null) StyleFacilityButton(button, FacilityButtonNames[i] == "Store");
            }
        }

        private static void StyleFacilityButton(Button button, bool premium)
        {
            ReUIIconGraphic[] generated = button.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < generated.Length; i++)
                generated[i].gameObject.SetActive(false);

            Image target = button.targetGraphic as Image;
            if (target == null) target = button.GetComponent<Image>();
            Color accent = premium ? ReUIPalette.AccentGold : ReUIPalette.AccentCyan;
            if (target != null)
            {
                target.enabled = true;
                target.sprite = ReUICanvasStyler.SurfaceSprite;
                target.type = Image.Type.Sliced;
                target.color = new Color(0.90f, 0.98f, 1.00f, 0.12f);
                ApplyOutline(target, accent, 0.90f);
                ReUIEffectStyler.ApplyButton(button, premium
                    ? ReUIEffectRole.PrimaryButton
                    : ReUIEffectRole.NavigationButton);
            }

            Image[] images = button.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (image == null || image == target) continue;

                string name = image.name.ToLowerInvariant();
                string parentName = image.transform.parent != null
                    ? image.transform.parent.name.ToLowerInvariant()
                    : string.Empty;

                if (name == "background" || name == "focus" || name == "left" || name == "right" ||
                    name == "separator" || parentName == "background")
                {
                    image.material = null;
                    image.color = Color.clear;
                    continue;
                }

                if (name == "icon" || name == "image" || parentName == "left")
                {
                    image.enabled = true;
                    image.material = null;
                    image.preserveAspect = true;
                    image.color = premium
                        ? new Color(1.00f, 0.90f, 0.58f, 1f)
                        : Color.white;
                    image.canvasRenderer.SetAlpha(1f);
                    NormalizeImageSize(image.rectTransform, 58f);
                }
            }

            Text[] labels = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].enabled = true;
                labels[i].color = Color.white;
                labels[i].canvasRenderer.SetAlpha(1f);
                labels[i].fontStyle = FontStyle.Bold;
            }

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.disabledColor = Color.white;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            // The target graphic is the glass surface itself. ColorTint would
            // replace its authored 0.06 alpha with the disabled state's alpha=1
            // whenever gameplay locks a facility, producing an opaque block.
            // Interactivity is still controlled by Button.interactable; only the
            // destructive visual tint transition is disabled.
            button.transition = Selectable.Transition.None;

            ReUIButtonMotion motion = button.GetComponent<ReUIButtonMotion>();
            if (motion == null) motion = button.gameObject.AddComponent<ReUIButtonMotion>();
            motion.enabled = true;
            motion.RefreshVisualState();
        }

        private static void NormalizeImageSize(RectTransform rect, float size)
        {
            if (rect == null) return;
            rect.localScale = Vector3.one;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
        }

        private static void ApplyOutline(Image image, Color accent, float alpha)
        {
            Outline outline = image.GetComponent<Outline>();
            if (outline == null) outline = image.gameObject.AddComponent<Outline>();
            outline.enabled = true;
            outline.effectColor = ReUIPalette.WithAlpha(accent, alpha);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = false;
        }
    }
}
