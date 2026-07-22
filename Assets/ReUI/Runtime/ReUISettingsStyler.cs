using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    internal static class ReUISettingsStyler
    {
        private static readonly string[] NavigationToggleNames =
        {
            "General", "Combat", "Controls", "Account", "LoadSave", "Database"
        };

        internal static void Apply(Canvas canvas)
        {
            if (canvas == null) return;

            Transform settings = FindSettingsRoot(canvas.transform);
            Transform buttons = settings != null ? settings.Find("Buttons") : null;
            if (buttons == null) return;

            for (int i = 0; i < NavigationToggleNames.Length; i++)
            {
                Transform target = buttons.Find(NavigationToggleNames[i]);
                Toggle toggle = target != null ? target.GetComponent<Toggle>() : null;
                if (toggle != null) StyleNavigationToggle(toggle);
            }

            Transform exitTransform = buttons.Find("Exit");
            Button exitButton = exitTransform != null ? exitTransform.GetComponent<Button>() : null;
            if (exitButton != null) StyleExitButton(exitButton);

            Toggle[] contentToggles = settings.GetComponentsInChildren<Toggle>(true);
            for (int i = 0; i < contentToggles.Length; i++)
            {
                Toggle toggle = contentToggles[i];
                if (toggle == null || IsDescendantOf(toggle.transform, buttons)) continue;
                bool mapSelector = HasAncestorNamed(toggle.transform, "CombatMapSize");
                StyleContentToggle(toggle, mapSelector);
            }

            Transform combatMapSize = ReUISpecializedVisuals.FindByName(settings, "CombatMapSize");
            if (combatMapSize != null) StyleCombatMapSize(combatMapSize);

            ReUIThemePalettePanel.Ensure(canvas, settings);
        }

        private static Transform FindSettingsRoot(Transform root)
        {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == "Settings" && all[i].Find("Buttons") != null)
                    return all[i];
            }
            return null;
        }

        private static void StyleNavigationToggle(Toggle toggle)
        {
            RemoveGeneratedIcons(toggle.transform);

            Image surface = toggle.targetGraphic as Image;
            if (surface == null) surface = toggle.GetComponent<Image>();
            if (surface != null)
            {
                surface.enabled = true;
                surface.sprite = ReUICanvasStyler.SurfaceSprite;
                surface.type = Image.Type.Sliced;
                surface.color = ReUIPalette.WithAlpha(ReUIPalette.GlassPrimary, 0.18f);
                ApplyOutline(surface, toggle.isOn ? 0.82f : 0.50f);
                ReUIEffectStyler.ApplySelectable(toggle, toggle.isOn
                    ? ReUIEffectRole.SelectedButton
                    : ReUIEffectRole.NavigationButton);
            }

            if (toggle.graphic is Image focus)
            {
                focus.enabled = true;
                focus.sprite = ReUICanvasStyler.SurfaceSprite;
                focus.type = Image.Type.Sliced;
                focus.material = null;
                focus.color = toggle.isOn
                    ? ReUIPalette.WithAlpha(ReUIPalette.AccentCyan, 0.20f)
                    : Color.clear;
            }

            Image icon = toggle.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                icon.gameObject.SetActive(true);
                icon.enabled = true;
                icon.material = null;
                icon.color = toggle.isOn
                    ? ReUIPalette.TextPrimary
                    : ReUIPalette.TextSecondary;
                NormalizeIconRect(icon.rectTransform, 64f);
            }

            ColorBlock colors = toggle.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = ReUIPalette.TextPrimary;
            colors.selectedColor = Color.white;
            colors.pressedColor = ReUIPalette.TextSecondary;
            colors.disabledColor = ReUIPalette.TextMuted;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            toggle.colors = colors;

            CanvasGroup group = toggle.GetComponent<CanvasGroup>();
            if (group != null) group.alpha = 1f;
        }

        private static void StyleExitButton(Button button)
        {
            RemoveGeneratedIcons(button.transform);

            Image icon = button.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                icon.gameObject.SetActive(true);
                icon.enabled = true;
                icon.material = null;
                icon.color = ReUIPalette.TextSecondary;
                NormalizeIconRect(icon.rectTransform, 64f);
            }

            Image target = button.targetGraphic as Image;
            if (target == null) target = button.GetComponent<Image>();
            if (target != null)
            {
                target.enabled = true;
                target.sprite = ReUICanvasStyler.SurfaceSprite;
                target.type = Image.Type.Sliced;
                target.color = ReUIPalette.WithAlpha(ReUIPalette.GlassPrimary, 0.15f);
                ApplyOutline(target, 0.38f);
                ReUIEffectStyler.ApplyButton(button, ReUIEffectRole.DangerButton);
            }

            Image background = button.transform.Find("Background")?.GetComponent<Image>();
            if (background != null && background != target)
            {
                background.enabled = true;
                background.sprite = ReUICanvasStyler.SurfaceSprite;
                background.type = Image.Type.Sliced;
                background.color = ReUIPalette.WithAlpha(ReUIPalette.GlassSecondary, 0.12f);
                ReUIEffectStyler.ApplyPanel(background);
            }

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = ReUIPalette.TextPrimary;
            colors.selectedColor = Color.white;
            colors.pressedColor = ReUIPalette.TextSecondary;
            colors.disabledColor = ReUIPalette.TextMuted;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            CanvasGroup group = button.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;
                group.interactable = true;
                group.blocksRaycasts = true;
            }
        }

        private static void StyleContentToggle(Toggle toggle, bool forceVisible)
        {
            if (toggle == null) return;
            RemoveGeneratedIcons(toggle.transform);

            CanvasGroup group = toggle.GetComponent<CanvasGroup>();
            if (group != null) group.alpha = 1f;

            RectTransform root = toggle.transform as RectTransform;
            if (root != null)
            {
                root.localScale = Vector3.one;
                LayoutElement layout = root.GetComponent<LayoutElement>();
                if (layout != null)
                {
                    layout.minWidth = 80f;
                    layout.minHeight = 80f;
                    layout.preferredWidth = 80f;
                    layout.preferredHeight = 80f;
                }
            }

            Image background = toggle.targetGraphic as Image;
            if (background == null) background = toggle.GetComponent<Image>();
            if (background != null)
            {
                background.gameObject.SetActive(true);
                background.enabled = true;
                background.sprite = ReUICanvasStyler.SurfaceSprite;
                background.type = Image.Type.Sliced;
                background.color = ReUIPalette.WithAlpha(ReUIPalette.GlassSecondary, 0.16f);
                ApplyOutline(background, forceVisible || toggle.isOn ? 0.72f : 0.42f);
                ReUIEffectStyler.ApplySelectable(toggle, forceVisible || toggle.isOn
                    ? ReUIEffectRole.SelectedButton
                    : ReUIEffectRole.SecondaryButton);
            }

            if (toggle.graphic is Image marker)
            {
                marker.gameObject.SetActive(true);
                marker.enabled = true;
                marker.sprite = ReUICanvasStyler.SurfaceSprite;
                marker.type = Image.Type.Sliced;
                marker.material = null;
                marker.color = forceVisible || toggle.isOn
                    ? ReUIPalette.WithAlpha(ReUIPalette.AccentCyan, 0.96f)
                    : ReUIPalette.WithAlpha(ReUIPalette.AccentCyan, 0.18f);
                NormalizeIconRect(marker.rectTransform, 34f);
            }

            ColorBlock colors = toggle.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = ReUIPalette.TextPrimary;
            colors.selectedColor = Color.white;
            colors.pressedColor = ReUIPalette.TextSecondary;
            colors.disabledColor = Color.white;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            toggle.colors = colors;
        }

        private static void StyleCombatMapSize(Transform selector)
        {
            RemoveGeneratedIcons(selector);

            Button button = selector.GetComponent<Button>();
            if (button != null)
            {
                Image surface = button.targetGraphic as Image;
                if (surface == null) surface = selector.GetComponent<Image>();
                if (surface != null)
                {
                    surface.enabled = true;
                    surface.sprite = ReUICanvasStyler.SurfaceSprite;
                    surface.type = Image.Type.Sliced;
                    surface.color = ReUIPalette.WithAlpha(ReUIPalette.GlassSecondary, 0.14f);
                    ApplyOutline(surface, 0.34f);
                    button.targetGraphic = surface;
                    ReUIEffectStyler.ApplyButton(button, ReUIEffectRole.SecondaryButton);
                }
            }

            Toggle toggle = selector.GetComponentInChildren<Toggle>(true);
            if (toggle != null) StyleContentToggle(toggle, true);

            Text[] labels = selector.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].color = ReUIPalette.TextPrimary;
                labels[i].fontStyle = FontStyle.Bold;
            }
        }

        private static void NormalizeIconRect(RectTransform rect, float size)
        {
            if (rect == null) return;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(size, size);
            rect.localScale = Vector3.one;
        }

        private static bool IsDescendantOf(Transform child, Transform ancestor)
        {
            Transform current = child;
            while (current != null)
            {
                if (current == ancestor) return true;
                current = current.parent;
            }
            return false;
        }

        private static bool HasAncestorNamed(Transform child, string name)
        {
            Transform current = child;
            while (current != null)
            {
                if (current.name == name) return true;
                current = current.parent;
            }
            return false;
        }

        private static void ApplyOutline(Image image, float alpha)
        {
            Outline outline = image.GetComponent<Outline>();
            if (outline == null) outline = image.gameObject.AddComponent<Outline>();
            outline.enabled = true;
            outline.effectColor = ReUIPalette.WithAlpha(ReUIPalette.AccentCyan, alpha);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = false;
        }

        private static void RemoveGeneratedIcons(Transform root)
        {
            ReUIIconGraphic[] icons = root.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i].gameObject.SetActive(false);
                if (Application.isPlaying)
                    Object.Destroy(icons[i].gameObject);
                else
                    Object.DestroyImmediate(icons[i].gameObject);
            }
        }
    }
}
