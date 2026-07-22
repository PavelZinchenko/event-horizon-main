using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    internal static class ReUIStarMapStyler
    {
        private const string SceneName = "StarMapScene";

        internal static void Apply(Canvas canvas)
        {
            if (canvas == null || canvas.gameObject.scene.name != SceneName) return;

            Transform gameMenu = ReUISpecializedVisuals.FindPath(canvas.transform, "GameMenu");
            if (gameMenu != null)
            {
                StyleButton(gameMenu, "Buttons/Fleet", ReUIIconKind.Fleet, false, 72f);
                StyleButton(gameMenu, "Buttons/Skills", ReUIIconKind.Skills, false, 72f);
                StyleButton(gameMenu, "Buttons/Quests", ReUIIconKind.Missions, false, 72f);
                StyleButton(gameMenu, "Buttons/Research", ReUIIconKind.Technology, false, 72f);
                StyleButton(gameMenu, "Buttons/CargoHold", ReUIIconKind.Equipment, false, 72f);
                StyleButton(gameMenu, "Buttons/Exit", ReUIIconKind.Close, true, 72f);

                // Filters/Shop is a functional StarMap filter referenced directly by
                // GameMenu.OnFiltersChanged(). Never disable or remove it. Only the
                // separate premium-currency BuyButton is intentionally hidden.
                Transform iapBuyButton = ReUISpecializedVisuals.FindByName(canvas.transform, "BuyButton");
                if (iapBuyButton != null) iapBuyButton.gameObject.SetActive(false);
            }

            StyleDynamicShortcut(canvas.transform, "Preview5RelationsButton", ReUIIconKind.Faction, 72f);

            StyleMapViewButton(canvas.transform, "StarViewButton");
            StyleMapViewButton(canvas.transform, "GalaxyViewButton");

            // StarSystemObjectItem instances are populated and coloured by the
            // map model at runtime.  They are not ordinary buttons: their child
            // graphics encode object type, ownership and availability.  A prior
            // glass conversion replaced their target graphic and injected a new
            // surface, which left the authored child backgrounds exposed as giant
            // solid blocks.  Preserve those dynamic cards completely.

            Transform statusPanel = ReUISpecializedVisuals.FindPath(canvas.transform, "StatusPanel");
            if (statusPanel != null)
            {
                Image statusImage = statusPanel.GetComponent<Image>();
                if (statusImage != null)
                    ReUISpecializedVisuals.StyleGlassPanel(statusImage, 0.58f);
            }
        }

        private static void StyleButton(Transform root, string path, ReUIIconKind kind, bool danger, float iconSize)
        {
            Transform transform = ReUISpecializedVisuals.FindPath(root, path);
            Button button = transform != null ? transform.GetComponent<Button>() : null;
            if (button == null) return;

            ReUISpecializedVisuals.StyleGlassButton(button, kind, danger, false, 0.065f);
            NormalizeButtonBrightness(button, danger);
            NormalizeButtonFrame(button);
            NormalizeIcon(button, kind, iconSize);
        }

        private static void StyleDynamicShortcut(
            Transform root,
            string objectName,
            ReUIIconKind kind,
            float iconSize)
        {
            Transform transform = ReUISpecializedVisuals.FindByName(root, objectName);
            Button button = transform != null ? transform.GetComponent<Button>() : null;
            RectTransform rect = transform as RectTransform;
            if (button == null) return;

            ReUISpecializedVisuals.StyleGlassButton(button, kind, false, true, 0.065f);
            NormalizeButtonBrightness(button, false);
            NormalizeButtonFrame(button);

            NormalizeIcon(button, kind, iconSize);
        }

        private static void NormalizeIcon(Button button, ReUIIconKind kind, float iconSize)
        {
            const string hostName = "ReUI Icon Host";
            const string iconName = "ReUI Vector Icon";

            Transform hostTransform = button.transform.Find(hostName);
            RectTransform host;
            if (hostTransform == null)
            {
                GameObject hostObject = new(hostName, typeof(RectTransform));
                hostObject.layer = button.gameObject.layer;
                host = (RectTransform)hostObject.transform;
                host.SetParent(button.transform, false);
            }
            else
            {
                host = hostTransform as RectTransform;
            }

            Transform iconTransform = host.Find(iconName);
            ReUIIconGraphic icon;
            if (iconTransform == null)
            {
                GameObject iconObject = new(iconName, typeof(RectTransform), typeof(CanvasRenderer), typeof(ReUIIconGraphic));
                iconObject.layer = button.gameObject.layer;
                iconTransform = iconObject.transform;
                iconTransform.SetParent(host, false);
                icon = iconObject.GetComponent<ReUIIconGraphic>();
            }
            else
            {
                icon = iconTransform.GetComponent<ReUIIconGraphic>();
                if (icon == null) icon = iconTransform.gameObject.AddComponent<ReUIIconGraphic>();
            }

            // Disable every stale generated icon. The old implementation selected
            // the first inactive descendant, which could live under a disabled
            // legacy Image and therefore pass validation without rendering.
            ReUIIconGraphic[] generated = button.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < generated.Length; i++)
            {
                if (generated[i] == icon) continue;
                generated[i].enabled = false;
                generated[i].gameObject.SetActive(false);
            }

            Transform legacyIcon = button.transform.Find("Icon");
            if (legacyIcon != null && legacyIcon != host)
            {
                Image legacyImage = legacyIcon.GetComponent<Image>();
                if (legacyImage != null && legacyImage != button.targetGraphic)
                    legacyImage.enabled = false;
            }

            icon.Kind = kind;
            icon.gameObject.SetActive(true);
            icon.enabled = true;
            icon.color = Color.white;
            icon.raycastTarget = false;
            icon.maskable = false;
            icon.canvasRenderer.SetAlpha(1f);
            icon.canvasRenderer.cullTransparentMesh = false;
            RectTransform iconRect = icon.rectTransform;
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            iconRect.localScale = Vector3.one;

            host.gameObject.SetActive(true);
            host.anchorMin = host.anchorMax = new Vector2(0.5f, 0.5f);
            host.pivot = new Vector2(0.5f, 0.5f);
            host.sizeDelta = new Vector2(iconSize, iconSize);
            host.anchoredPosition = Vector2.zero;
            host.localScale = Vector3.one;
            host.SetAsLastSibling();

            // Keep badges above the icon, notably the skill-points bubble.
            Transform points = button.transform.Find("Points");
            if (points != null) points.SetAsLastSibling();
        }

        private static void NormalizeButtonFrame(Button button)
        {
            RectTransform rect = button != null ? button.transform as RectTransform : null;
            if (rect == null) return;

            // Keep each authored RectTransform and any LayoutGroup constraints.
            // Forcing every toolbar control to a square and adding a LayoutElement
            // broke responsive pages at non-menu resolutions.
            rect.localScale = Vector3.one;
        }

        private static void NormalizeButtonBrightness(Button button, bool danger)
        {
            if (button == null) return;

            Image surface = button.targetGraphic as Image;
            if (surface == null) surface = button.GetComponent<Image>();
            if (surface != null)
            {
                surface.enabled = true;
                surface.color = danger
                    ? new Color(1.00f, 0.96f, 0.97f, 0.060f)
                    : new Color(0.90f, 0.98f, 1.00f, 0.065f);
                ReUIEffectStyler.ApplyButton(button, danger
                    ? ReUIEffectRole.DangerButton
                    : ReUIEffectRole.NavigationButton);
                Outline outline = surface.GetComponent<Outline>();
                if (outline == null) outline = surface.gameObject.AddComponent<Outline>();
                outline.enabled = true;
                outline.effectColor = danger
                    ? new Color(1.00f, 0.34f, 0.42f, 0.72f)
                    : new Color(0.16f, 0.86f, 1.00f, 0.62f);
                outline.effectDistance = new Vector2(1f, -1f);
                outline.useGraphicAlpha = false;
            }

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.82f, 0.94f, 1f, 1f);
            colors.disabledColor = new Color(0.82f, 0.90f, 0.96f, 1f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }

        private static void StyleMapViewButton(Transform root, string objectName)
        {
            Transform transform = ReUISpecializedVisuals.FindByName(root, objectName);
            Button button = transform != null ? transform.GetComponent<Button>() : null;
            RectTransform rect = transform as RectTransform;
            if (button == null) return;

            ReUISpecializedVisuals.StyleGlassButton(button, ReUIIconKind.StarMap, false, true, 0.065f);
            NormalizeButtonBrightness(button, false);
            NormalizeButtonFrame(button);
            NormalizeIcon(button, ReUIIconKind.StarMap, 72f);
            if (rect != null)
            {
                rect.localScale = Vector3.one;
            }
        }

        private static void StyleStarSystemObjectButtons(Canvas canvas)
        {
            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                MonoBehaviour[] behaviours = button.GetComponents<MonoBehaviour>();
                bool isStarObject = false;
                for (int j = 0; j < behaviours.Length; j++)
                {
                    if (behaviours[j] != null &&
                        behaviours[j].GetType().FullName == "Gui.StarMap.StarSystemObjectItem")
                    {
                        isStarObject = true;
                        break;
                    }
                }
                if (!isStarObject) continue;

                StyleStarSystemObjectButton(button);
            }
        }

        private static void StyleStarSystemObjectButton(Button button)
        {
            bool interactable = button.interactable;
            Transform surfaceTransform = button.transform.Find("ReUI Object Surface");
            Image surface;
            if (surfaceTransform == null)
            {
                GameObject surfaceObject = new("ReUI Object Surface", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                surfaceObject.layer = button.gameObject.layer;
                surfaceTransform = surfaceObject.transform;
                surfaceTransform.SetParent(button.transform, false);
                surface = surfaceObject.GetComponent<Image>();
            }
            else
            {
                surface = surfaceTransform.GetComponent<Image>();
                if (surface == null) surface = surfaceTransform.gameObject.AddComponent<Image>();
            }

            RectTransform surfaceRect = surfaceTransform as RectTransform;
            surfaceRect.anchorMin = Vector2.zero;
            surfaceRect.anchorMax = Vector2.one;
            surfaceRect.offsetMin = new Vector2(12f, 12f);
            surfaceRect.offsetMax = new Vector2(-12f, -12f);
            surfaceRect.localScale = Vector3.one;
            surfaceTransform.gameObject.SetActive(true);
            surfaceTransform.SetAsFirstSibling();

            surface.enabled = true;
            surface.sprite = ReUICanvasStyler.SurfaceSprite;
            surface.type = Image.Type.Sliced;
            surface.preserveAspect = false;
            surface.color = interactable
                ? new Color(0.84f, 0.96f, 1f, 0.10f)
                : new Color(0.68f, 0.78f, 0.86f, 0.12f);
            surface.raycastTarget = true;
            surface.maskable = false;
            surface.canvasRenderer.SetAlpha(1f);
            surface.canvasRenderer.cullTransparentMesh = false;
            button.targetGraphic = surface;

            Image rootImage = button.GetComponent<Image>();
            if (rootImage != null && rootImage != surface)
                rootImage.enabled = false;
            Transform oldFocus = button.transform.Find("Focus");
            if (oldFocus != null)
            {
                Image focusImage = oldFocus.GetComponent<Image>();
                if (focusImage != null && focusImage != surface)
                    focusImage.enabled = false;
            }

            ReUISpecializedVisuals.DisableEffects(surface.gameObject);
            ReUIEffectStyler.ApplyButton(button, interactable
                ? ReUIEffectRole.SecondaryButton
                : ReUIEffectRole.DisabledButton);
            Outline outline = surface.GetComponent<Outline>();
            if (outline == null) outline = surface.gameObject.AddComponent<Outline>();
            outline.enabled = true;
            outline.effectColor = interactable
                ? new Color(0.18f, 0.82f, 1f, 0.52f)
                : new Color(0.58f, 0.68f, 0.76f, 0.38f);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = false;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.90f, 0.98f, 1f, 1f);
            colors.selectedColor = new Color(0.86f, 0.96f, 1f, 1f);
            colors.pressedColor = new Color(0.76f, 0.90f, 0.98f, 1f);
            colors.disabledColor = new Color(0.88f, 0.92f, 0.96f, 1f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.transition = Selectable.Transition.ColorTint;

            // Only the button's own CanvasGroup is normalized. Ancestor groups are
            // owned by AnimatedWindow and must retain their hide/show animation.
            Text[] labels = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].gameObject.SetActive(true);
                labels[i].enabled = true;
                labels[i].color = new Color(0.88f, 0.96f, 1f, 1f);
                labels[i].canvasRenderer.SetAlpha(1f);
                labels[i].fontStyle = FontStyle.Bold;
            }

            Transform iconTransform = button.transform.Find("Image");
            Image icon = iconTransform != null ? iconTransform.GetComponent<Image>() : null;
            if (icon != null)
            {
                icon.gameObject.SetActive(true);
                icon.enabled = true;
                icon.color = EnsureReadableIconColor(icon.color);
                icon.canvasRenderer.SetAlpha(1f);
                icon.raycastTarget = false;
            }

            // Reassert the gameplay state after styling; visual normalization must
            // never make an unavailable star object clickable.
            button.interactable = interactable;
        }

        private static Color EnsureReadableIconColor(Color color)
        {
            color.a = 1f;
            float luminance = 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
            if (luminance >= 0.58f) return color;

            float blend = Mathf.Clamp01((0.58f - luminance) / 0.58f);
            Color readable = Color.Lerp(color, Color.white, blend * 0.72f);
            readable.a = 1f;
            return readable;
        }
    }
}
