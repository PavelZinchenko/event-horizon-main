using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    internal static class ReUICombatStyler
    {
        private const string SceneName = "CombatScene";

        internal static void Apply(Canvas canvas)
        {
            if (canvas == null || canvas.gameObject.scene.name != SceneName) return;

            Transform root = canvas.transform;
            StyleCombatMenuButton(root, "CombatMenu/Panel/Panel/Resume", ReUIIconKind.Back);
            StyleCombatMenuButton(root, "CombatMenu/Panel/Panel/ChangeShip", ReUIIconKind.Fleet);
            StyleCombatMenuButton(root, "CombatMenu/Panel/Panel/NextEnemy", ReUIIconKind.NextEnemy);
            StyleCombatMenuButton(root, "CombatMenu/Panel/Panel/Settings", ReUIIconKind.Settings);
            StyleCombatMenuButton(root, "CombatMenu/Panel/Panel/KeySettings", ReUIIconKind.Settings);
            StyleCombatMenuButton(root, "CombatMenu/Panel/Panel/KillThemAll", ReUIIconKind.Battle, true);
            StyleCombatMenuButton(root, "CombatMenu/Panel/Panel/Surrender", ReUIIconKind.Close, true);

            StyleCombatMenuButton(root, "ShipSelectionPanel/Panel/Button", ReUIIconKind.Battle);
            StyleCombatMenuButton(root, "CombatRewardWindow/ExitButton", ReUIIconKind.Close, true);
            StyleCombatMenuButton(root, "SettingsPanel/Settings/Panel/ExitButton", ReUIIconKind.Close, true);
            StyleCombatMenuButton(root, "KeySettingsPanel/KeySettings/ExitButton", ReUIIconKind.Close, true);
            NormalizeCombatRewardFills(root);
        }

        private static void NormalizeCombatRewardFills(Transform root)
        {
            Transform window = ReUISpecializedVisuals.FindByName(root, "CombatRewardWindow");
            if (window == null) return;

            Image[] images = window.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                string objectName = image.gameObject.name;
                bool rewardSurface = objectName.StartsWith("ExpItem") ||
                                     objectName.StartsWith("PlayerExpItem") ||
                                     objectName.StartsWith("RewardItem") ||
                                     objectName == "Focus";
                if (!rewardSurface) continue;

                // Reward rarity/status colors belong to the item artwork and text,
                // not to opaque card fills. Keep every reward slot background fully
                // transparent so purple, gray and cyan cards no longer form blocks.
                image.enabled = true;
                image.sprite = ReUICanvasStyler.SurfaceSprite;
                image.type = Image.Type.Sliced;
                image.preserveAspect = false;
                image.material = null;
                image.color = Color.clear;
                ReUISpecializedVisuals.DisableEffects(image.gameObject);

                // Selection remains visible through an outline only; the fill stays
                // completely transparent as requested.
                if (objectName == "Focus")
                {
                    Outline outline = image.GetComponent<Outline>();
                    if (outline == null) outline = image.gameObject.AddComponent<Outline>();
                    outline.enabled = true;
                    outline.effectColor = new Color(0.16f, 0.86f, 1.00f, 0.72f);
                    outline.effectDistance = new Vector2(1.5f, -1.5f);
                    outline.useGraphicAlpha = false;
                }
            }
        }

        private static void StyleCombatMenuButton(
            Transform root,
            string path,
            ReUIIconKind iconKind,
            bool danger = false)
        {
            Transform target = ReUISpecializedVisuals.FindPath(root, path);
            Button button = target != null ? target.GetComponent<Button>() : null;
            if (button == null) return;

            ReUISpecializedVisuals.StyleGlassButton(button, iconKind, danger, false, danger ? 0.72f : 0.64f);

            // These legacy buttons store their artwork at Left/Image rather than
            // a direct Icon child. ForceSemanticIcon uses that exact image as the
            // host, so the original sprite is hidden while the event binding and
            // layout remain untouched.
            Transform oldLeftIcon = target.Find("Left/Image");
            if (oldLeftIcon != null)
            {
                Image oldImage = oldLeftIcon.GetComponent<Image>();
                if (oldImage != null) oldImage.enabled = false;
            }

            Text[] labels = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].color = Color.white;
                labels[i].fontStyle = FontStyle.Bold;
            }
        }
    }
}
