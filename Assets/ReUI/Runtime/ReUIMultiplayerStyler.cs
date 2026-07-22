using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    internal static class ReUIMultiplayerStyler
    {
        private static readonly Dictionary<int, int> SelectedChoiceByGroup = new();
        private static readonly HashSet<int> WiredFleetButtons = new();
        private static readonly HashSet<int> WiredFleetToggles = new();

        private static readonly string[] ContextTokens =
        {
            "联机战斗测试", "选择舰队", "选择联合进攻舰队", "联合进攻",
            "舰队联机", "等待大厅", "等待连接"
        };

        private static readonly string[] ActionTokens =
        {
            "创建房间", "加入战斗", "作为主机", "作为客机", "准备",
            "关闭", "取消", "确认选择"
        };

        internal static void Apply(Canvas canvas)
        {
            if (canvas == null) return;

            StyleKnownPanels(canvas.transform);
            if (!HasContext(canvas.transform)) return;

            StyleActionButtons(canvas.transform);
            StyleFleetToggles(canvas.transform);
            StyleFleetButtons(canvas.transform);
            UpdateJointAttackIndicator(canvas.transform);
        }

        private static void StyleKnownPanels(Transform root)
        {
            Transform multiplayer = ReUISpecializedVisuals.FindByName(root, "MultiplayerPanel");
            Image multiplayerImage = multiplayer != null ? multiplayer.GetComponent<Image>() : null;
            if (multiplayerImage != null)
                ReUISpecializedVisuals.StyleGlassPanel(multiplayerImage, 0.76f);

            Transform alliedDialog = ReUISpecializedVisuals.FindByName(root, "Preview7AlliedAttackDialog");
            if (alliedDialog == null) return;

            Transform card = alliedDialog.Find("Card");
            Image cardImage = card != null ? card.GetComponent<Image>() : null;
            if (cardImage != null)
                ReUISpecializedVisuals.StyleGlassPanel(cardImage, 0.80f);
        }

        private static bool HasContext(Transform root)
        {
            Text[] texts = root.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                string value = texts[i].text ?? string.Empty;
                if (ContainsAny(value, ContextTokens)) return true;
            }
            return ReUISpecializedVisuals.FindByName(root, "Preview7AlliedAttackDialog") != null;
        }

        private static void StyleActionButtons(Transform root)
        {
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                string label = GetLabel(buttons[i]);
                if (!ContainsAny(label, ActionTokens)) continue;

                bool danger = label.Contains("关闭") || label.Contains("取消");
                ReUISpecializedVisuals.StyleGlassButton(
                    buttons[i],
                    danger ? ReUIIconKind.Close : ReUIIconKind.None,
                    danger,
                    false,
                    danger ? 0.72f : 0.66f);
            }
        }

        private static void StyleFleetToggles(Transform root)
        {
            Toggle[] toggles = root.GetComponentsInChildren<Toggle>(true);
            for (int i = 0; i < toggles.Length; i++)
            {
                Toggle toggle = toggles[i];
                string label = GetLabel(toggle);
                bool alliedChoice = IsUnderNamedAncestor(toggle.transform, "Preview7AlliedAttackDialog");
                if (!alliedChoice && !label.Contains("舰队")) continue;

                // This is a single optional support fleet, not a mandatory radio
                // selection. Detaching it from a ToggleGroup guarantees that tapping
                // the selected row again can cancel joint attack.
                if (alliedChoice) toggle.group = null;

                ApplyFleetToggleVisual(toggle);
                int id = toggle.GetInstanceID();
                if (WiredFleetToggles.Add(id))
                {
                    Toggle captured = toggle;
                    Transform capturedRoot = root;
                    toggle.onValueChanged.AddListener(_ =>
                    {
                        ApplyFleetToggleVisual(captured);
                        UpdateJointAttackIndicator(capturedRoot);
                    });
                }
            }
        }

        private static void ApplyFleetToggleVisual(Toggle toggle)
        {
            if (toggle == null) return;
            Image image = toggle.targetGraphic as Image;
            if (image == null) image = toggle.GetComponent<Image>();
            if (image == null) return;

            image.enabled = true;
            image.sprite = ReUICanvasStyler.SurfaceSprite;
            image.type = Image.Type.Sliced;
            image.color = toggle.isOn
                ? new Color(0.92f, 0.98f, 1.00f, 0.28f)
                : new Color(0.88f, 0.94f, 1.00f, 0.16f);
            ReUIEffectStyler.ApplySelectable(toggle, toggle.isOn
                ? ReUIEffectRole.SelectedButton
                : ReUIEffectRole.SecondaryButton);

            Outline outline = image.GetComponent<Outline>();
            if (outline == null) outline = image.gameObject.AddComponent<Outline>();
            outline.enabled = toggle.isOn;
            outline.effectColor = new Color(0.20f, 0.94f, 1f, 0.98f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            if (toggle.graphic is Image check)
            {
                check.color = toggle.isOn
                    ? new Color(0.12f, 0.95f, 1f, 1f)
                    : new Color(0.45f, 0.58f, 0.66f, 0.55f);
            }

            Text[] labels = toggle.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].color = toggle.isOn
                    ? Color.white
                    : new Color(0.78f, 0.86f, 0.92f, 0.96f);
                labels[i].fontStyle = FontStyle.Bold;
            }
        }

        private static void StyleFleetButtons(Transform root)
        {
            List<Button> candidates = CollectFleetButtons(root);
            if (candidates.Count == 0) return;

            Transform groupHost = candidates[0].transform.parent != null
                ? candidates[0].transform.parent
                : root;
            int groupId = groupHost.GetInstanceID();

            int selectedId;
            if (!SelectedChoiceByGroup.TryGetValue(groupId, out selectedId) ||
                !ContainsInstance(candidates, selectedId))
            {
                selectedId = candidates[0].GetInstanceID();
                SelectedChoiceByGroup[groupId] = selectedId;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                Button button = candidates[i];
                int buttonId = button.GetInstanceID();
                if (WiredFleetButtons.Add(buttonId))
                {
                    Button capturedButton = button;
                    Transform capturedGroup = groupHost;
                    int capturedGroupId = groupId;
                    button.onClick.AddListener(() =>
                    {
                        SelectedChoiceByGroup[capturedGroupId] = capturedButton.GetInstanceID();
                        RefreshFleetButtonGroup(capturedGroup, capturedGroupId);
                    });
                }
                ApplyFleetButtonVisual(button, buttonId == selectedId);
            }
        }

        private static void RefreshFleetButtonGroup(Transform groupRoot, int groupId)
        {
            if (groupRoot == null) return;
            if (!SelectedChoiceByGroup.TryGetValue(groupId, out int selectedId)) return;

            List<Button> candidates = CollectFleetButtons(groupRoot);
            for (int i = 0; i < candidates.Count; i++)
                ApplyFleetButtonVisual(candidates[i], candidates[i].GetInstanceID() == selectedId);
        }

        private static void ApplyFleetButtonVisual(Button button, bool selected)
        {
            if (button == null) return;
            Image image = button.targetGraphic as Image;
            if (image == null) image = button.GetComponent<Image>();
            if (image == null) return;

            image.sprite = ReUICanvasStyler.SurfaceSprite;
            image.type = Image.Type.Sliced;
            image.color = selected
                ? new Color(0.92f, 0.98f, 1.00f, 0.28f)
                : new Color(0.88f, 0.94f, 1.00f, 0.16f);
            ReUIEffectStyler.ApplyButton(button, selected
                ? ReUIEffectRole.SelectedButton
                : ReUIEffectRole.SecondaryButton);

            Outline outline = image.GetComponent<Outline>();
            if (outline == null) outline = image.gameObject.AddComponent<Outline>();
            outline.enabled = selected;
            outline.effectColor = new Color(0.22f, 0.92f, 1f, 0.96f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            Text[] labels = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].color = selected ? Color.white : new Color(0.78f, 0.86f, 0.92f, 0.95f);
                labels[i].fontStyle = FontStyle.Bold;
            }
        }

        private static List<Button> CollectFleetButtons(Transform root)
        {
            var candidates = new List<Button>();
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                string label = GetLabel(buttons[i]);
                if (!label.Contains("舰队")) continue;
                if (label.Contains("选择舰队") || label.Contains("联合进攻") ||
                    label.Contains("出战舰队") || label.Contains("配置本地"))
                    continue;
                candidates.Add(buttons[i]);
            }
            return candidates;
        }

        private static void UpdateJointAttackIndicator(Transform root)
        {
            Transform jointTransform = ReUISpecializedVisuals.FindByName(root, "Preview5JointAttackButton");
            if (jointTransform == null) return;

            Text label = jointTransform.GetComponentInChildren<Text>(true);
            if (label == null) return;

            Transform dialog = ReUISpecializedVisuals.FindByName(root, "Preview7AlliedAttackDialog");
            Toggle choice = dialog != null ? dialog.GetComponentInChildren<Toggle>(true) : null;
            bool enabled = choice != null && choice.isOn;

            label.text = enabled ? "■  联合进攻" : "□  联合进攻";
            label.color = enabled
                ? Color.white
                : new Color(0.78f, 0.86f, 0.92f, 0.96f);
            label.fontStyle = FontStyle.Bold;
        }

        private static bool ContainsInstance(List<Button> buttons, int instanceId)
        {
            for (int i = 0; i < buttons.Count; i++)
                if (buttons[i] != null && buttons[i].GetInstanceID() == instanceId) return true;
            return false;
        }

        private static bool IsUnderNamedAncestor(Transform transform, string name)
        {
            Transform current = transform;
            while (current != null)
            {
                if (current.name == name) return true;
                current = current.parent;
            }
            return false;
        }

        private static string GetLabel(Button button)
        {
            Text[] labels = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
                if (!string.IsNullOrWhiteSpace(labels[i].text)) return labels[i].text;
            return string.Empty;
        }

        private static string GetLabel(Toggle toggle)
        {
            Text[] labels = toggle.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
                if (!string.IsNullOrWhiteSpace(labels[i].text)) return labels[i].text;
            return string.Empty;
        }

        private static bool ContainsAny(string value, string[] tokens)
        {
            for (int i = 0; i < tokens.Length; i++)
                if (value.Contains(tokens[i])) return true;
            return false;
        }
    }
}
