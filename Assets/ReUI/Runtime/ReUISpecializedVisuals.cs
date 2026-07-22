using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    internal static class ReUISpecializedVisuals
    {
        internal static Transform FindPath(Transform root, string path)
        {
            if (root == null || string.IsNullOrEmpty(path)) return null;
            string[] parts = path.Split('/');
            Transform current = root;
            int start = 0;
            if (parts.Length > 0 && parts[0] == root.name) start = 1;
            for (int i = start; i < parts.Length; i++)
            {
                current = current.Find(parts[i]);
                if (current == null) return null;
            }
            return current;
        }

        internal static Transform FindByName(Transform root, string objectName)
        {
            if (root == null) return null;
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
                if (all[i].name == objectName) return all[i];
            return null;
        }

        internal static void StyleGlassButton(
            Button button,
            ReUIIconKind iconKind,
            bool danger = false,
            bool hideLabels = false,
            float opacity = 0.72f)
        {
            if (button == null) return;

            Image target = button.targetGraphic as Image;
            if (target == null) target = button.GetComponent<Image>();
            if (target == null) return;
            button.targetGraphic = target;

            Image rootImage = button.GetComponent<Image>();
            if (rootImage != null && rootImage != target)
                rootImage.enabled = false;

            Transform oldBackground = button.transform.Find("Background");
            if (oldBackground != null && oldBackground != target.transform)
            {
                Image backgroundImage = oldBackground.GetComponent<Image>();
                if (backgroundImage != null) backgroundImage.enabled = false;
            }

            Transform oldIcon = button.transform.Find("Icon");
            if (oldIcon != null)
            {
                Image oldIconImage = oldIcon.GetComponent<Image>();
                if (oldIconImage != null) oldIconImage.enabled = false;
            }

            target.enabled = true;
            target.sprite = ReUICanvasStyler.SurfaceSprite;
            target.type = Image.Type.Sliced;
            target.preserveAspect = false;
            float transparentOpacity = Mathf.Min(Mathf.Clamp01(opacity), 0.28f);
            target.color = ReUIPalette.WithAlpha(ReUIPalette.GlassSoft, transparentOpacity);
            PlaceSurfaceBehindContent(button, target);

            DisableEffects(target.gameObject);

            Color accent = danger ? ReUIPalette.AccentRed : ReUIPalette.AccentCyan;
            Outline outline = target.GetComponent<Outline>();
            if (outline == null) outline = target.gameObject.AddComponent<Outline>();
            outline.enabled = true;
            outline.effectColor = ReUIPalette.WithAlpha(accent, danger ? 0.82f : 0.68f);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = false;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.disabledColor = Color.white;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0f;
            button.colors = colors;
            // Preserve the original button transition. Different pages use
            // state sprites, colour tint and animation for gameplay feedback.

            ReUIEffectStyler.ApplyButton(button, danger
                ? ReUIEffectRole.DangerButton
                : ReUIEffectRole.PrimaryButton);

            // Passing None is also meaningful: it removes a generic semantic icon
            // that may have been installed earlier in the same canvas scan.
            ReUICanvasStyler.ForceSemanticIcon(button, iconKind);

            Text[] labels = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].gameObject.SetActive(!hideLabels);
                if (hideLabels) continue;
                labels[i].color = ReUIPalette.TextPrimary;
                labels[i].canvasRenderer.SetAlpha(1f);
                labels[i].fontStyle = FontStyle.Bold;
                Outline[] outlines = labels[i].GetComponents<Outline>();
                for (int outlineIndex = 0; outlineIndex < outlines.Length; outlineIndex++)
                    outlines[outlineIndex].enabled = false;
                AddDarkShadow(labels[i]);
            }

            ReUIButtonMotion motion = button.GetComponent<ReUIButtonMotion>();
            if (motion == null) motion = button.gameObject.AddComponent<ReUIButtonMotion>();
            motion.RefreshVisualState();
        }

        private static void PlaceSurfaceBehindContent(Button button, Image target)
        {
            if (button == null || target == null || target.transform.parent != button.transform)
                return;

            RectTransform rect = target.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            LayoutElement layout = target.GetComponent<LayoutElement>();
            if (layout == null) layout = target.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
            target.transform.SetAsFirstSibling();
        }

        internal static void StyleGlassPanel(Image image, float opacity = 0.68f)
        {
            if (image == null) return;
            image.enabled = true;
            image.sprite = ReUICanvasStyler.SurfaceSprite;
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            image.color = ReUIPalette.WithAlpha(ReUIPalette.GlassElevated, opacity);
            DisableEffects(image.gameObject);
            ReUIEffectStyler.ApplyPanel(image, opacity >= 0.76f);
        }

        internal static void StyleReadableText(Text text, bool muted = false)
        {
            if (text == null) return;
            text.color = muted
                ? ReUIPalette.WithAlpha(ReUIPalette.TextSecondary, 0.96f)
                : ReUIPalette.TextPrimary;
            text.fontStyle = FontStyle.Bold;
            AddDarkShadow(text);
        }

        internal static void DisableEffects(GameObject gameObject)
        {
            if (gameObject == null) return;
            Outline[] outlines = gameObject.GetComponents<Outline>();
            for (int i = 0; i < outlines.Length; i++) outlines[i].enabled = false;
            Shadow[] shadows = gameObject.GetComponents<Shadow>();
            for (int i = 0; i < shadows.Length; i++)
            {
                if (shadows[i].GetType() == typeof(Shadow)) shadows[i].enabled = false;
            }
        }

        private static void AddDarkShadow(Text text)
        {
            Shadow[] effects = text.GetComponents<Shadow>();
            Shadow shadow = null;
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i].GetType() == typeof(Shadow))
                {
                    shadow = effects[i];
                    break;
                }
            }
            if (shadow == null) shadow = text.gameObject.AddComponent<Shadow>();
            shadow.enabled = true;
            shadow.effectColor = new Color(0f, 0.01f, 0.035f, 0.86f);
            shadow.effectDistance = new Vector2(1.25f, -1.25f);
            shadow.useGraphicAlpha = true;
        }
    }
}
