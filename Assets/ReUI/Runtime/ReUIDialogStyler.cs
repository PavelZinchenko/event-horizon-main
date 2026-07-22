using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    /// <summary>
    /// Normalizes confirmation-dialog action buttons. Generic semantic detection used
    /// to insert a small close glyph beside localized "Cancel" labels, while the
    /// button surface itself stayed invisible. Dialog actions remain text buttons and
    /// preserve all original Window result bindings.
    /// </summary>
    internal static class ReUIDialogStyler
    {
        internal static void Apply(Canvas canvas)
        {
            if (canvas == null) return;

            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null || HasAncestorNamed(button.transform, "ArenaFight")) continue;

                bool option1 = HasPersistentMethod(button, "CloseWithResultOption1");
                bool option2 = HasPersistentMethod(button, "CloseWithResultOption2");
                bool namedCancel = IsTextCancelButton(button);
                if (!option1 && !option2 && !namedCancel) continue;

                StyleDialogAction(button, option1 && !option2);
            }
        }

        private static void StyleDialogAction(Button button, bool primary)
        {
            RemoveGeneratedIcons(button.transform);

            CanvasGroup group = button.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;
                group.blocksRaycasts = true;
            }

            RectTransform rect = button.transform as RectTransform;
            if (rect != null)
            {
                rect.localScale = Vector3.one;
                LayoutElement layout = rect.GetComponent<LayoutElement>();
                if (layout != null)
                {
                    layout.minHeight = Mathf.Max(layout.minHeight, 68f);
                    layout.preferredHeight = Mathf.Max(layout.preferredHeight, 72f);
                }
            }

            Image target = button.targetGraphic as Image;
            if (target == null) target = button.GetComponent<Image>();
            if (target != null)
            {
                target.gameObject.SetActive(true);
                target.enabled = true;
                target.sprite = ReUICanvasStyler.SurfaceSprite;
                target.type = Image.Type.Sliced;
                target.color = new Color(0.90f, 0.98f, 1.00f, primary ? 0.070f : 0.050f);
                ApplyOutline(target, primary ? 0.76f : 0.52f);
                button.targetGraphic = target;
                ReUIEffectStyler.ApplyButton(button, primary
                    ? ReUIEffectRole.PrimaryButton
                    : ReUIEffectRole.SecondaryButton);
            }

            Image[] images = button.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (image == null || image == target) continue;
                string name = image.name.ToLowerInvariant();
                if (name == "left" || name == "right" || name == "background" ||
                    name == "focus" || name == "icon" || name == "image")
                {
                    image.material = null;
                    image.color = Color.clear;
                }
            }

            Text[] labels = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].gameObject.SetActive(true);
                labels[i].enabled = true;
                labels[i].color = Color.white;
                labels[i].fontStyle = FontStyle.Bold;
                labels[i].alignment = TextAnchor.MiddleCenter;
            }

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.82f, 0.94f, 1f, 1f);
            colors.disabledColor = new Color(0.76f, 0.82f, 0.88f, 1f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }

        private static bool HasPersistentMethod(Button button, string methodName)
        {
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (button.onClick.GetPersistentMethodName(i) == methodName)
                    return true;
            }
            return false;
        }

        private static bool IsTextCancelButton(Button button)
        {
            string name = button.name.ToLowerInvariant();
            if (name == "cancel" || name == "cancelbutton" || name == "nobutton" || name == "decline")
                return button.GetComponentInChildren<Text>(true) != null;

            var descriptor = new StringBuilder();
            Text[] labels = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
                descriptor.Append(labels[i].text).Append(' ');
            string text = descriptor.ToString().ToLowerInvariant();
            return text.Contains("取消") || text.Contains("cancel") || text.Trim() == "no";
        }

        private static bool HasAncestorNamed(Transform transform, string name)
        {
            Transform current = transform;
            while (current != null)
            {
                if (current.name == name) return true;
                current = current.parent;
            }
            return false;
        }

        private static void RemoveGeneratedIcons(Transform root)
        {
            ReUIIconGraphic[] icons = root.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i].enabled = false;
                icons[i].gameObject.SetActive(false);
            }
        }

        private static void ApplyOutline(Image image, float alpha)
        {
            Outline outline = image.GetComponent<Outline>();
            if (outline == null) outline = image.gameObject.AddComponent<Outline>();
            outline.enabled = true;
            outline.effectColor = new Color(0.16f, 0.86f, 1.00f, alpha);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = false;
        }
    }
}
