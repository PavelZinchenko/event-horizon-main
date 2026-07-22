using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    internal static class ReUICaptainStyler
    {
        private const string SceneName = "StarMapScene";

        internal static void Apply(Canvas canvas)
        {
            if (canvas == null || canvas.gameObject.scene.name != SceneName) return;

            Transform panelTransform = ReUISpecializedVisuals.FindByName(canvas.transform, "CaptainPanel");
            if (panelTransform == null) return;

            RectTransform panelRect = panelTransform as RectTransform;
            if (panelRect != null)
            {
                // Leave the permanent right-side star information panel and the
                // bottom navigation unobstructed. The old fixed 1020x660 panel
                // overlapped both on wide Android screens.
                panelRect.anchorMin = new Vector2(0.14f, 0.13f);
                panelRect.anchorMax = new Vector2(0.69f, 0.89f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                panelRect.localScale = Vector3.one;
                panelRect.SetAsLastSibling();
            }

            Image panelImage = panelTransform.GetComponent<Image>();
            if (panelImage != null)
                ReUISpecializedVisuals.StyleGlassPanel(panelImage, 0.84f);

            Text title = panelTransform.Find("Title")?.GetComponent<Text>();
            if (title != null)
            {
                title.fontSize = Mathf.Max(title.fontSize, 30);
                ReUISpecializedVisuals.StyleReadableText(title);
            }

            Text subtitle = panelTransform.Find("Subtitle")?.GetComponent<Text>();
            if (subtitle != null)
                ReUISpecializedVisuals.StyleReadableText(subtitle, true);

            Transform list = panelTransform.Find("CaptainList");
            if (list != null)
            {
                VerticalLayoutGroup layout = list.GetComponent<VerticalLayoutGroup>();
                if (layout != null) layout.spacing = 12f;

                for (int i = 0; i < list.childCount; i++)
                    StyleCaptainCard(list.GetChild(i));
            }

            Transform closeTransform = panelTransform.Find("Close");
            Button close = closeTransform != null ? closeTransform.GetComponent<Button>() : null;
            if (close != null)
            {
                ReUISpecializedVisuals.StyleGlassButton(close, ReUIIconKind.Close, true, false, 0.72f);
                Text label = close.GetComponentInChildren<Text>(true);
                if (label != null) label.text = "关闭";
            }
        }

        private static void StyleCaptainCard(Transform cardTransform)
        {
            if (cardTransform == null) return;
            Button card = cardTransform.GetComponent<Button>();
            Image image = cardTransform.GetComponent<Image>();
            if (card == null || image == null) return;

            Text state = cardTransform.Find("State")?.GetComponent<Text>();
            bool selected = state != null && (state.text ?? string.Empty).Contains("已选择");

            card.targetGraphic = image;
            image.enabled = true;
            image.sprite = ReUICanvasStyler.SurfaceSprite;
            image.type = Image.Type.Sliced;
            image.color = selected
                ? new Color(0.55f, 0.91f, 1.00f, 0.84f)
                : new Color(0.72f, 0.86f, 0.96f, 0.62f);
            ReUIEffectStyler.ApplyButton(card, selected
                ? ReUIEffectRole.SelectedButton
                : ReUIEffectRole.SecondaryButton);

            Outline outline = image.GetComponent<Outline>();
            if (outline == null) outline = image.gameObject.AddComponent<Outline>();
            outline.enabled = selected;
            outline.effectColor = new Color(0.24f, 0.92f, 1f, 0.92f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            // ReUI3's generic semantic detector interpreted the word "战斗" in a
            // captain description as a battle button and covered the portrait with
            // a red crossed icon. Captain cards now preserve their actual artwork.
            ReUIIconGraphic[] generated = card.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < generated.Length; i++)
                generated[i].gameObject.SetActive(false);

            Transform portraitTransform = cardTransform.Find("Portrait");
            Image portrait = portraitTransform != null ? portraitTransform.GetComponent<Image>() : null;
            if (portrait != null)
            {
                portrait.enabled = true;
                portrait.material = null;
                portrait.preserveAspect = true;
                if (portrait.sprite != null) portrait.color = Color.white;
                portrait.raycastTarget = false;
            }

            Text[] labels = card.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                bool muted = labels[i].name == "Description";
                ReUISpecializedVisuals.StyleReadableText(labels[i], muted);
            }

            if (state != null)
                state.color = selected
                    ? new Color(0.38f, 1.00f, 0.72f, 1f)
                    : new Color(0.76f, 0.84f, 0.90f, 1f);
        }
    }
}
