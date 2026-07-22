using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    internal static class ReUIArenaStyler
    {
        private const string SurfaceName = "ReUI Arena Surface";
        private const string IconName = "ReUI Arena Icon";
        private const string FightIconName = "ReUI Fight Emblem";
        private const string LabelName = "ReUI Arena Label";

        internal static void Apply(Canvas canvas)
        {
            if (canvas == null || canvas.gameObject.scene.name != "StarMapScene") return;

            Transform[] all = canvas.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == null || all[i].name != "ArenaFight") continue;
                StyleArenaFight(all[i]);
            }
        }

        private static void StyleArenaFight(Transform arenaFight)
        {
            Transform buttons = arenaFight.Find("Buttons") ?? ReUISpecializedVisuals.FindByName(arenaFight, "Buttons");
            if (buttons == null) return;

            // The original prefab uses a HorizontalLayoutGroup. It rewrites the
            // absolute positions after the ReUI pass and was the main source of the
            // device-only discrepancy. The action buttons are now deliberately
            // positioned, so the legacy child layout must no longer drive them.
            LayoutGroup[] childLayouts = buttons.GetComponents<LayoutGroup>();
            for (int i = 0; i < childLayouts.Length; i++)
                childLayouts[i].enabled = false;

            RectTransform buttonsRect = buttons as RectTransform;
            if (buttonsRect != null)
            {
                buttonsRect.anchorMin = buttonsRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonsRect.pivot = new Vector2(0.5f, 0.5f);
                buttonsRect.sizeDelta = new Vector2(480f, 190f);
                buttonsRect.anchoredPosition = new Vector2(0f, 6f);

                LayoutElement containerLayout = buttons.GetComponent<LayoutElement>();
                if (containerLayout == null) containerLayout = buttons.gameObject.AddComponent<LayoutElement>();
                containerLayout.minWidth = 480f;
                containerLayout.preferredWidth = 480f;
                containerLayout.minHeight = 190f;
                containerLayout.preferredHeight = 190f;
                containerLayout.flexibleWidth = 0f;
                containerLayout.flexibleHeight = 0f;
            }

            StyleFightButton(arenaFight, "FightButton", ReUIIconKind.Battle, false,
                new Vector2(-55f, 0f), 176f, 124f, "战斗");
            StyleFightButton(arenaFight, "CancelButton", ReUIIconKind.Close, true,
                new Vector2(130f, 0f), 118f, 78f, "取消");
        }

        private static void StyleFightButton(
            Transform arenaFight,
            string buttonName,
            ReUIIconKind iconKind,
            bool danger,
            Vector2 anchoredPosition,
            float buttonSize,
            float iconSize,
            string labelValue)
        {
            Transform target = ReUISpecializedVisuals.FindByName(arenaFight, buttonName);
            Button button = target != null ? target.GetComponent<Button>() : null;
            RectTransform rect = target as RectTransform;
            if (button == null || rect == null) return;

            // Preserve the original Button and its persistent onClick event. Only
            // replace its visual target with a plain Image that cannot be repainted
            // by ThemedImage.Start or hidden together with the legacy icon layers.
            target.gameObject.SetActive(true);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(buttonSize, buttonSize);
            rect.anchoredPosition = anchoredPosition;
            rect.localScale = Vector3.one;

            LayoutElement layout = target.GetComponent<LayoutElement>();
            if (layout == null) layout = target.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
            layout.minWidth = buttonSize;
            layout.preferredWidth = buttonSize;
            layout.minHeight = buttonSize;
            layout.preferredHeight = buttonSize;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;

            Image surface = EnsureSurface(button, danger);
            DisableLegacyImages(button, surface);
            button.targetGraphic = surface;

            Color accent = danger ? ReUIPalette.AccentRed : ReUIPalette.AccentCyan;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.Lerp(Color.white, accent, 0.10f);
            colors.selectedColor = Color.Lerp(Color.white, accent, 0.16f);
            colors.pressedColor = Color.Lerp(Color.white, accent, 0.24f);
            colors.disabledColor = new Color(0.82f, 0.88f, 0.94f, 1f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.transition = Selectable.Transition.ColorTint;

            CanvasGroup ownGroup = button.GetComponent<CanvasGroup>();
            if (ownGroup != null)
            {
                ownGroup.alpha = 1f;
                ownGroup.interactable = true;
                ownGroup.blocksRaycasts = true;
            }

            Graphic icon = iconKind == ReUIIconKind.Battle
                ? EnsureFightIcon(button, iconSize)
                : EnsureIcon(button, iconKind, iconSize);
            Text label = EnsureLabel(button, labelValue, danger, buttonSize);
            ReUISpecializedVisuals.DisableEffects(icon.gameObject);
            ReUISpecializedVisuals.DisableEffects(label.gameObject);

            // Legacy text and generated icons are explicitly disabled so only the
            // deterministic Surface/Icon/Label stack can render.
            Text[] labels = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] == label) continue;
                labels[i].enabled = false;
            }

            ReUIIconGraphic[] generated = button.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < generated.Length; i++)
            {
                if (generated[i] == icon) continue;
                generated[i].enabled = false;
                generated[i].gameObject.SetActive(false);
            }

            surface.transform.SetAsFirstSibling();
            icon.transform.SetAsLastSibling();
            label.transform.SetAsLastSibling();
        }

        private static Image EnsureSurface(Button button, bool danger)
        {
            Transform existing = button.transform.Find(SurfaceName);
            Image surface;
            if (existing == null)
            {
                GameObject surfaceObject = new(SurfaceName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                surfaceObject.layer = button.gameObject.layer;
                existing = surfaceObject.transform;
                existing.SetParent(button.transform, false);
                surface = surfaceObject.GetComponent<Image>();
            }
            else
            {
                surface = existing.GetComponent<Image>();
                if (surface == null) surface = existing.gameObject.AddComponent<Image>();
            }

            existing.gameObject.SetActive(true);
            RectTransform rect = existing as RectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            surface.enabled = true;
            surface.sprite = ReUICanvasStyler.SurfaceSprite;
            surface.type = Image.Type.Sliced;
            surface.preserveAspect = false;
            // Keep the original low-opacity button surface. Only the emblem and
            // label are forced fully opaque; the button background remains glassy.
            surface.material = null;
            surface.color = danger
                ? new Color(1f, 0.72f, 0.72f, 0.07f)
                : new Color(0.78f, 0.94f, 1f, 0.10f);
            surface.raycastTarget = true;
            surface.maskable = false;
            surface.canvasRenderer.SetAlpha(1f);
            surface.canvasRenderer.cullTransparentMesh = false;

            ReUISpecializedVisuals.DisableEffects(surface.gameObject);
            Outline outline = surface.GetComponent<Outline>();
            if (outline == null) outline = surface.gameObject.AddComponent<Outline>();
            outline.enabled = true;
            outline.effectColor = ReUIPalette.WithAlpha(
                danger ? ReUIPalette.AccentRed : ReUIPalette.AccentCyan,
                danger ? 0.72f : 0.82f);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = false;
            return surface;
        }

        private static void DisableLegacyImages(Button button, Image surface)
        {
            Image[] images = button.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == surface) continue;
                images[i].enabled = false;
                images[i].raycastTarget = false;
            }
        }

        private static ReUIIconGraphic EnsureIcon(Button button, ReUIIconKind kind, float iconSize)
        {
            Transform iconTransform = button.transform.Find(IconName);
            ReUIIconGraphic icon;
            if (iconTransform == null)
            {
                GameObject iconObject = new(IconName, typeof(RectTransform), typeof(CanvasRenderer), typeof(ReUIIconGraphic));
                iconObject.layer = button.gameObject.layer;
                iconTransform = iconObject.transform;
                iconTransform.SetParent(button.transform, false);
                icon = iconObject.GetComponent<ReUIIconGraphic>();
            }
            else
            {
                icon = iconTransform.GetComponent<ReUIIconGraphic>();
                if (icon == null) icon = iconTransform.gameObject.AddComponent<ReUIIconGraphic>();
            }

            iconTransform.gameObject.SetActive(true);
            RectTransform rect = iconTransform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(iconSize, iconSize);
            rect.anchoredPosition = new Vector2(0f, 8f);
            rect.localScale = Vector3.one;

            icon.enabled = true;
            icon.Kind = kind;
            icon.color = Color.white;
            icon.raycastTarget = false;
            icon.maskable = false;
            icon.canvasRenderer.SetAlpha(1f);
            icon.canvasRenderer.cullTransparentMesh = false;
            return icon;
        }

        private static ReUIFightIconGraphic EnsureFightIcon(Button button, float iconSize)
        {
            Transform iconTransform = button.transform.Find(FightIconName);
            ReUIFightIconGraphic icon;
            if (iconTransform == null)
            {
                GameObject iconObject = new(
                    FightIconName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(ReUIFightIconGraphic));
                iconObject.layer = button.gameObject.layer;
                iconTransform = iconObject.transform;
                iconTransform.SetParent(button.transform, false);
                icon = iconObject.GetComponent<ReUIFightIconGraphic>();
            }
            else
            {
                icon = iconTransform.GetComponent<ReUIFightIconGraphic>();
                if (icon == null) icon = iconTransform.gameObject.AddComponent<ReUIFightIconGraphic>();
            }

            Transform oldArenaIcon = button.transform.Find(IconName);
            if (oldArenaIcon != null)
            {
                ReUIIconGraphic oldGraphic = oldArenaIcon.GetComponent<ReUIIconGraphic>();
                if (oldGraphic != null) oldGraphic.enabled = false;
                oldArenaIcon.gameObject.SetActive(false);
            }

            iconTransform.gameObject.SetActive(true);
            RectTransform rect = iconTransform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(iconSize, iconSize);
            rect.anchoredPosition = new Vector2(0f, 8f);
            rect.localScale = Vector3.one;

            icon.enabled = true;
            icon.color = Color.white;
            icon.raycastTarget = false;
            icon.maskable = false;
            icon.material = null;
            icon.canvasRenderer.SetAlpha(1f);
            icon.canvasRenderer.cullTransparentMesh = false;
            icon.SetVerticesDirty();
            return icon;
        }

        private static Text EnsureLabel(Button button, string value, bool danger, float buttonSize)
        {
            Transform labelTransform = button.transform.Find(LabelName);
            Text label;
            if (labelTransform == null)
            {
                GameObject labelObject = new(LabelName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                labelObject.layer = button.gameObject.layer;
                labelTransform = labelObject.transform;
                labelTransform.SetParent(button.transform, false);
                label = labelObject.GetComponent<Text>();
            }
            else
            {
                label = labelTransform.GetComponent<Text>();
                if (label == null) label = labelTransform.gameObject.AddComponent<Text>();
            }

            labelTransform.gameObject.SetActive(true);
            RectTransform rect = labelTransform as RectTransform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, Mathf.Max(34f, buttonSize * 0.25f));
            rect.anchoredPosition = new Vector2(0f, 9f);
            rect.localScale = Vector3.one;

            label.enabled = true;
            label.text = value;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = danger ? 18 : 22;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = danger ? new Color(1f, 0.86f, 0.86f, 1f) : Color.white;
            label.raycastTarget = false;
            label.maskable = false;
            label.canvasRenderer.SetAlpha(1f);
            label.canvasRenderer.cullTransparentMesh = false;
            return label;
        }
    }
}
