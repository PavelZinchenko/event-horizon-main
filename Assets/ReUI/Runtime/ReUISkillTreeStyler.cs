using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    internal static class ReUISkillTreeStyler
    {
        private const string SceneName = "SkillTreeScene";
        private const string LabelName = "ReUI Exit Label";

        internal static void Apply(Canvas canvas)
        {
            if (canvas == null || canvas.gameObject.scene.name != SceneName) return;

            Transform exitTransform = ReUISpecializedVisuals.FindByName(canvas.transform, "ExitButton");
            Button exitButton = exitTransform != null ? exitTransform.GetComponent<Button>() : null;
            RectTransform exitRect = exitTransform as RectTransform;
            if (exitButton == null || exitRect == null) return;

            exitTransform.gameObject.SetActive(true);

            // The original prefab places a 128x128 X inside the central layout.
            // Keep its existing onClick binding, but move the same Button instance
            // to a predictable bottom-right safe position.
            if (exitRect.parent != canvas.transform)
                exitRect.SetParent(canvas.transform, false);
            exitRect.anchorMin = exitRect.anchorMax = new Vector2(1f, 0f);
            exitRect.pivot = new Vector2(1f, 0f);
            exitRect.anchoredPosition = new Vector2(-24f, 22f);
            exitRect.sizeDelta = new Vector2(168f, 72f);
            exitRect.localScale = Vector3.one;
            exitRect.SetAsLastSibling();

            LayoutElement layout = exitButton.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.ignoreLayout = true;
                layout.minWidth = 168f;
                layout.minHeight = 72f;
                layout.preferredWidth = 168f;
                layout.preferredHeight = 72f;
            }

            ReUISpecializedVisuals.StyleGlassButton(
                exitButton,
                ReUIIconKind.Close,
                true,
                false,
                0.72f);

            Text label = exitTransform.Find(LabelName)?.GetComponent<Text>();
            if (label == null)
            {
                GameObject labelObject = new(LabelName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform labelRect = (RectTransform)labelObject.transform;
                labelRect.SetParent(exitTransform, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(58f, 4f);
                labelRect.offsetMax = new Vector2(-12f, -4f);
                label = labelObject.GetComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.alignment = TextAnchor.MiddleCenter;
                label.fontSize = 24;
                label.raycastTarget = false;
            }
            label.text = "退出";
            ReUISpecializedVisuals.StyleReadableText(label);

            // The scene's skill nodes, links, faction technology states and runtime
            // Three-Body tab are intentionally left on their original materials.
            // Their colors represent lock/ownership state and must not be flattened
            // by the generic ReUI button/text pass.
        }
    }
}
