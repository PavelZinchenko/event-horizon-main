using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    internal static class ReUIShipEditorStyler
    {
        private const string SceneName = "ShipEditorScene";

        internal static void Apply(Canvas canvas)
        {
            if (canvas == null || canvas.gameObject.scene.name != SceneName) return;

            Transform window = ReUISpecializedVisuals.FindPath(canvas.transform, "ShipEditorWindow");
            if (window == null) return;

            Transform rightPanel = window.Find("RightPanel");
            if (rightPanel == null) return;

            Transform componentList = rightPanel.Find("ComponentList");
            if (componentList == null) return;

            StylePanel(componentList);
            NormalizeListRows(componentList);
        }

        private static void StylePanel(Transform transform)
        {
            Image image = transform.GetComponent<Image>();
            if (image != null)
                ReUISpecializedVisuals.StyleGlassPanel(image, 0.62f);
        }

        private static void NormalizeListRows(Transform componentList)
        {
            Button[] buttons = componentList.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (!IsListRow(button.transform)) continue;

                if (button.targetGraphic is Image focus)
                {
                    focus.material = null;
                    focus.color = ReUIPalette.WithAlpha(ReUIPalette.AccentCyan, 0.055f);
                    ReUISpecializedVisuals.DisableEffects(focus.gameObject);
                }

                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.Lerp(ReUIPalette.TextPrimary, ReUIPalette.AccentCyan, 0.16f);
                colors.selectedColor = Color.Lerp(ReUIPalette.TextPrimary, ReUIPalette.AccentCyan, 0.30f);
                colors.pressedColor = Color.Lerp(ReUIPalette.TextPrimary, ReUIPalette.AccentCyan, 0.40f);
                colors.disabledColor = ReUIPalette.WithAlpha(ReUIPalette.TextMuted, 0.42f);
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.08f;
                button.colors = colors;

                ReUIButtonMotion motion = button.GetComponent<ReUIButtonMotion>();
                if (motion != null) motion.enabled = false;
            }
        }

        private static bool IsListRow(Transform transform)
        {
            int depth = 0;
            while (transform != null && depth++ < 9)
            {
                string name = transform.name.ToLowerInvariant();
                if (name.Contains("componentitem") || name.Contains("groupitem"))
                    return true;
                if (name == "componentlist") break;
                transform = transform.parent;
            }
            return false;
        }
    }
}
