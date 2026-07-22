using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    [DisallowMultipleComponent]
    internal sealed class ReUIMainMenuStyled : MonoBehaviour
    {
    }

    internal static class ReUIMainMenuStyler
    {
        private const string MainMenuSceneName = "MainMenuScene";
        private const string MenuRootName = "MainMenu";
        private const string LegacyAccentRailName = "ReUI Menu Accent";

        public static void Apply(Canvas canvas)
        {
            if (canvas == null || canvas.gameObject.scene.name != MainMenuSceneName) return;

            RectTransform menuRoot = FindRect(canvas.transform, MenuRootName);
            if (menuRoot == null) return;

            if (menuRoot.GetComponent<ReUIMainMenuStyled>() == null)
            {
                RemoveLegacyOverlay(menuRoot);
                menuRoot.gameObject.AddComponent<ReUIMainMenuStyled>();
            }

            // Theme/localization scripts can rewrite colors after scene startup. Reapply
            // lightweight visual properties on every dynamic scan so labels never fade
            // back into the sampled background.
            StyleMenuButtons(menuRoot);
            CleanupConfigurationButtons(canvas.transform);
            StyleNamedText(canvas.transform, "ProgramTitle", true);
            StyleNamedText(canvas.transform, "VerstionInfo", false);
        }

        private static void RemoveLegacyOverlay(RectTransform menuRoot)
        {
            // Earlier ReUI builds added a full-height Image, Outline and Shadow to the
            // menu root. The menu root is a layout container, not a visual surface;
            // styling it produced a second opaque panel over the original interface.
            Image rootImage = menuRoot.GetComponent<Image>();
            if (rootImage != null)
                Object.Destroy(rootImage);

            Outline rootOutline = menuRoot.GetComponent<Outline>();
            if (rootOutline != null)
                Object.Destroy(rootOutline);

            Shadow[] rootShadows = menuRoot.GetComponents<Shadow>();
            for (int i = 0; i < rootShadows.Length; i++)
            {
                if (rootShadows[i].GetType() == typeof(Shadow))
                    Object.Destroy(rootShadows[i]);
            }

            // Do not alter the original VerticalLayoutGroup. The original menu already
            // owns button sizing and placement; overriding it displaced several buttons.
        }

        private static void StyleMenuButtons(RectTransform menuRoot)
        {
            for (int i = 0; i < menuRoot.childCount; i++)
            {
                Transform child = menuRoot.GetChild(i);
                Button button = child.GetComponent<Button>();
                if (button == null) continue;

                RemoveLegacyAccentRail(button.transform);
                DisableOriginalButtonChrome(button);
                StyleOriginalButtonSurface(button);
                StyleOriginalButtonText(button);
                ReUICanvasStyler.ForceSemanticIcon(button, IconForButton(button));
            }
        }

        private static void DisableOriginalButtonChrome(Button button)
        {
            // MainMenuButton.prefab contains mirrored Left and Right background images.
            // Keep these transforms because the HorizontalLayoutGroup uses their sizes,
            // but disable only their old decorative graphics and shadows.
            DisableChromeBranch(button.transform.Find("Left"));
            DisableChromeBranch(button.transform.Find("Right"));
        }

        private static void DisableChromeBranch(Transform branch)
        {
            if (branch == null) return;

            Image branchImage = branch.GetComponent<Image>();
            if (branchImage != null)
                branchImage.enabled = false;

            Shadow[] effects = branch.GetComponentsInChildren<Shadow>(true);
            for (int i = 0; i < effects.Length; i++)
                effects[i].enabled = false;
        }

        private static void StyleOriginalButtonSurface(Button button)
        {
            if (button.targetGraphic is not Image surface) return;

            surface.enabled = true;
            surface.sprite = ReUICanvasStyler.SurfaceSprite;
            surface.type = Image.Type.Sliced;
            surface.preserveAspect = false;
            surface.raycastTarget = true;

            bool combatButton = button.name == "Combat";
            Outline outline = surface.GetComponent<Outline>();
            surface.material = null;
            surface.color = ReUIPalette.WithAlpha(ReUIPalette.GlassSoft, combatButton ? 0.26f : 0.18f);
            if (outline != null) outline.enabled = false;
            ReUIEffectStyler.ApplyButton(button, combatButton
                ? ReUIEffectRole.PrimaryButton
                : ReUIEffectRole.NavigationButton);

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.disabledColor = Color.white;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0f;
            button.colors = colors;
            button.transition = Selectable.Transition.None;
        }

        private static void StyleOriginalButtonText(Button button)
        {
            Text[] labels = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                Text label = labels[i];
                bool combatButton = button.name == "Combat";
                label.color = Color.white;
                label.canvasRenderer.SetAlpha(1f);
                label.fontStyle = FontStyle.Bold;

                Shadow shadow = GetOrAddShadow(label.gameObject);
                shadow.enabled = true;
                shadow.effectColor = combatButton
                    ? new Color(0.08f, 0.005f, 0f, 0.98f)
                    : new Color(0f, 0.015f, 0.04f, 0.88f);
                shadow.effectDistance = combatButton ? new Vector2(2f, -2f) : new Vector2(1.5f, -1.5f);
                shadow.useGraphicAlpha = true;

                // Preserve the prefab's own alignment, anchors, best-fit and offsets.
                // This avoids the displaced/duplicated appearance from the first build.
            }
        }

        private static ReUIIconKind IconForButton(Button button)
        {
            string buttonName = button.name;
            string label = string.Empty;
            Text[] labels = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(labels[i].text))
                {
                    label += labels[i].text;
                    label += " ";
                }
            }

            string descriptor = (buttonName + " " + label).ToLowerInvariant();
            if (buttonName == "Combat" || descriptor.Contains("quick") || descriptor.Contains("快速"))
                return ReUIIconKind.QuickBattle;
            if (descriptor.Contains("combat") || descriptor.Contains("battle") || descriptor.Contains("战斗"))
                return ReUIIconKind.Battle;
            if (descriptor.Contains("continue") || descriptor.Contains("newgame") || descriptor.Contains("start") ||
                descriptor.Contains("继续") || descriptor.Contains("开始") || descriptor.Contains("新游戏"))
                return ReUIIconKind.StarMap;
            if (descriptor.Contains("multiplayer") || descriptor.Contains("联机"))
                return ReUIIconKind.Multiplayer;
            if (descriptor.Contains("encyclopedia") || descriptor.Contains("图鉴") || descriptor.Contains("百科"))
                return ReUIIconKind.Encyclopedia;
            if (descriptor.Contains("settings") || descriptor.Contains("设置"))
                return ReUIIconKind.Settings;
            if (descriptor.Contains("constructor") || descriptor.Contains("editor") || descriptor.Contains("建造") || descriptor.Contains("编辑"))
                return ReUIIconKind.ShipEditor;
            if (descriptor.Contains("purchase") || descriptor.Contains("store") || descriptor.Contains("商店") || descriptor.Contains("购买"))
                return ReUIIconKind.Store;
            if (descriptor.Contains("exit") || descriptor.Contains("退出"))
                return ReUIIconKind.Close;
            return ReUIIconKind.None;
        }

        private static void CleanupConfigurationButtons(Transform root)
        {
            CleanupConfigurationButton(root, "ConfigureEnemyFleet");
            CleanupConfigurationButton(root, "ConfigureAllyFleet");
        }

        private static void CleanupConfigurationButton(Transform root, string objectName)
        {
            RectTransform rect = FindRect(root, objectName);
            if (rect == null) return;

            ReUIIconGraphic[] icons = rect.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < icons.Length; i++)
                Object.Destroy(icons[i].gameObject);

            Transform generatedHost = rect.Find("ReUI Icon Host");
            if (generatedHost != null)
                Object.Destroy(generatedHost.gameObject);

            Button button = rect.GetComponent<Button>();
            if (button?.targetGraphic is Image image)
            {
                image.sprite = ReUICanvasStyler.SurfaceSprite;
                image.type = Image.Type.Sliced;
                image.color = new Color(0.92f, 0.97f, 1f, 0.22f);
                ReUIEffectStyler.ApplyButton(button, ReUIEffectRole.SecondaryButton);
            }
        }

        private static void RemoveLegacyAccentRail(Transform buttonTransform)
        {
            Transform rail = buttonTransform.Find(LegacyAccentRailName);
            if (rail != null)
                Object.Destroy(rail.gameObject);
        }

        private static void StyleNamedText(Transform root, string objectName, bool title)
        {
            RectTransform rect = FindRect(root, objectName);
            if (rect == null) return;

            Text text = rect.GetComponent<Text>();
            if (text == null) text = rect.GetComponentInChildren<Text>(true);
            if (text == null) return;

            text.color = title ? ReUIPalette.TextPrimary : ReUIPalette.TextSecondary;
            if (!title) return;

            text.fontStyle = FontStyle.Bold;
            Shadow shadow = GetOrAddShadow(text.gameObject);
            shadow.effectColor = ReUIPalette.WithAlpha(ReUIPalette.AccentCyan, 0.20f);
            shadow.effectDistance = new Vector2(0f, -1f);
            shadow.useGraphicAlpha = true;
        }

        private static Shadow GetOrAddShadow(GameObject gameObject)
        {
            Shadow[] effects = gameObject.GetComponents<Shadow>();
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i].GetType() == typeof(Shadow)) return effects[i];
            }
            return gameObject.AddComponent<Shadow>();
        }

        private static RectTransform FindRect(Transform root, string objectName)
        {
            RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i].name == objectName) return rects[i];
            }
            return null;
        }
    }
}
