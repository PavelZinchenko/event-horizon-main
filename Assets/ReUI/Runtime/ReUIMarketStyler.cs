using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    internal static class ReUIMarketStyler
    {
        private static readonly string[] CategoryTogglePaths =
        {
            "LeftPanel/Resources",
            "LeftPanel/Ships",
            "LeftPanel/Weapon",
            "LeftPanel/Modules",
            "LeftPanel/Other",
            "ItemsPanel/Buttons/Buy",
            "ItemsPanel/Buttons/Sell",
        };

        private static readonly string[] ActionButtonPaths =
        {
            "RightPanel/Buttons/BuyButton",
            "RightPanel/Buttons/SellButton",
            "RightPanel/Buttons/SellTrashButton",
            "RightPanel/Buttons/ExitButton",
        };

        internal static void Apply(Canvas canvas)
        {
            if (canvas == null) return;

            Transform market = FindMarketDialog(canvas.transform);
            if (market == null) return;

            // Theme scripts can repaint store controls after the first ReUI scan.
            // Normalize every interactive surface on every pass so selected tabs,
            // buy buttons and exit buttons never regain solid faction/status fills.
            Button[] allButtons = market.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < allButtons.Length; i++)
                StyleTransparentSurface(allButtons[i]);

            Toggle[] allToggles = market.GetComponentsInChildren<Toggle>(true);
            for (int i = 0; i < allToggles.Length; i++)
                StyleTransparentToggle(allToggles[i]);

            for (int i = 0; i < CategoryTogglePaths.Length; i++)
            {
                Transform target = ReUISpecializedVisuals.FindPath(market, CategoryTogglePaths[i]);
                Toggle toggle = target != null ? target.GetComponent<Toggle>() : null;
                if (toggle != null) StyleTransparentToggle(toggle);
            }

            for (int i = 0; i < ActionButtonPaths.Length; i++)
            {
                Transform target = ReUISpecializedVisuals.FindPath(market, ActionButtonPaths[i]);
                Button button = target != null ? target.GetComponent<Button>() : null;
                if (button == null) continue;

                bool danger = button.name == "ExitButton";
                bool premium = button.name == "BuyButton";
                StyleTransparentActionButton(button, danger, premium);
            }
        }

        private static Transform FindMarketDialog(Transform root)
        {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                string name = all[i].name;
                if (name == "MarketDialog" || name.StartsWith("MarketDialog(") ||
                    name == "IapStoreDialog" || name.StartsWith("IapStoreDialog(") ||
                    name == "SpecialStoreDialog" || name.StartsWith("SpecialStoreDialog("))
                    return all[i];
            }
            return null;
        }

        private static void StyleTransparentToggle(Toggle toggle)
        {
            Image surface = toggle.targetGraphic as Image;
            if (surface == null) surface = toggle.GetComponent<Image>();
            if (surface != null)
            {
                surface.enabled = true;
                surface.sprite = ReUICanvasStyler.SurfaceSprite;
                surface.type = Image.Type.Sliced;
                surface.material = null;
                surface.color = Color.clear;
                ApplyOutline(surface, toggle.isOn, ReUIPalette.AccentCyan);
            }

            if (toggle.graphic is Image selectedFill)
            {
                selectedFill.enabled = true;
                selectedFill.material = null;
                selectedFill.color = Color.clear;
            }

            Image icon = FindPrimaryIcon(toggle.transform);
            if (icon != null)
            {
                icon.gameObject.SetActive(true);
                icon.enabled = true;
                icon.material = null;
                icon.color = toggle.isOn
                    ? new Color(0.76f, 0.97f, 1.00f, 1f)
                    : new Color(0.32f, 0.80f, 0.94f, 0.92f);
            }

            Text[] labels = toggle.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].color = toggle.isOn
                    ? new Color(0.76f, 0.97f, 1.00f, 1f)
                    : Color.white;
                labels[i].fontStyle = FontStyle.Bold;
            }

            ColorBlock colors = toggle.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.82f, 0.93f, 1f, 1f);
            colors.disabledColor = new Color(0.48f, 0.54f, 0.62f, 0.45f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            toggle.colors = colors;
        }

        private static void StyleTransparentActionButton(Button button, bool danger, bool premium)
        {
            ReUIIconGraphic[] generatedIcons = button.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < generatedIcons.Length; i++)
            {
                generatedIcons[i].gameObject.SetActive(true);
                generatedIcons[i].enabled = true;
                generatedIcons[i].color = Color.white;
            }

            Image[] images = button.GetComponentsInChildren<Image>(true);
            Image outlineHost = button.targetGraphic as Image;
            if (outlineHost == null) outlineHost = button.GetComponent<Image>();

            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (IsActionIcon(image, button.transform))
                {
                    image.gameObject.SetActive(true);
                    image.enabled = true;
                    image.material = null;
                    image.color = danger
                        ? new Color(1.00f, 0.48f, 0.54f, 1f)
                        : premium
                            ? new Color(1.00f, 0.78f, 0.34f, 1f)
                            : new Color(0.38f, 0.84f, 0.96f, 1f);
                    continue;
                }

                if (!IsChromeImage(image, button.transform)) continue;
                image.enabled = true;
                image.material = null;
                image.color = Color.clear;
            }

            if (outlineHost != null)
            {
                outlineHost.enabled = true;
                outlineHost.sprite = ReUICanvasStyler.SurfaceSprite;
                outlineHost.type = Image.Type.Sliced;
                outlineHost.material = null;
                outlineHost.color = Color.clear;
                Color accent = danger
                    ? ReUIPalette.AccentRed
                    : premium
                        ? ReUIPalette.AccentGold
                        : ReUIPalette.AccentCyan;
                ApplyOutline(outlineHost, true, accent);
            }

            Text[] labels = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].color = Color.white;
                labels[i].fontStyle = FontStyle.Bold;
            }

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.82f, 0.93f, 1f, 1f);
            colors.disabledColor = new Color(0.48f, 0.54f, 0.62f, 0.45f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }

        private static void StyleTransparentSurface(Button button)
        {
            if (button == null) return;
            Image surface = button.targetGraphic as Image;
            if (surface == null) surface = button.GetComponent<Image>();
            if (surface == null) return;

            surface.enabled = true;
            surface.sprite = ReUICanvasStyler.SurfaceSprite;
            surface.type = Image.Type.Sliced;
            surface.material = null;
            surface.color = Color.clear;
            ApplyOutline(surface, true, IsDanger(button)
                ? ReUIPalette.AccentRed
                : IsPremium(button)
                    ? ReUIPalette.AccentGold
                    : ReUIPalette.AccentCyan);
        }

        private static bool IsDanger(Button button)
        {
            string value = button != null ? button.name.ToLowerInvariant() : string.Empty;
            return value.Contains("exit") || value.Contains("close") || value.Contains("remove") || value.Contains("trash");
        }

        private static bool IsPremium(Button button)
        {
            string value = button != null ? button.name.ToLowerInvariant() : string.Empty;
            return value.Contains("buy") || value.Contains("shop") || value.Contains("purchase");
        }

        private static Image FindPrimaryIcon(Transform root)
        {
            Image direct = root.Find("Image")?.GetComponent<Image>();
            if (direct != null && !IsChromeSprite(direct.sprite)) return direct;

            Image[] images = root.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (IsActionIcon(images[i], root)) return images[i];
            }
            return null;
        }

        private static bool IsActionIcon(Image image, Transform buttonRoot)
        {
            if (image == null || image.transform == buttonRoot) return false;
            string spriteName = image.sprite != null ? image.sprite.name.ToLowerInvariant() : string.Empty;
            string name = image.name.ToLowerInvariant();
            string parentName = image.transform.parent != null ? image.transform.parent.name.ToLowerInvariant() : string.Empty;

            if (parentName == "left" || name == "icon")
                return !IsChromeSprite(image.sprite);

            return spriteName.Contains("shop_icon") || spriteName.Contains("icon_exit") ||
                   spriteName.Contains("scrap_icon") || spriteName.Contains("icon_remove");
        }

        private static bool IsChromeImage(Image image, Transform buttonRoot)
        {
            if (image == null) return false;
            if (image.transform == buttonRoot) return true;

            string name = image.name.ToLowerInvariant();
            string spriteName = image.sprite != null ? image.sprite.name.ToLowerInvariant() : string.Empty;
            return name == "left" || name == "right" || name == "background" ||
                   name == "focus" || name == "image" || IsChromeSpriteName(spriteName);
        }

        private static bool IsChromeSprite(Sprite sprite)
        {
            return IsChromeSpriteName(sprite != null ? sprite.name.ToLowerInvariant() : string.Empty);
        }

        private static bool IsChromeSpriteName(string spriteName)
        {
            return spriteName.StartsWith("ui_content") || spriteName.StartsWith("ui_button") ||
                   spriteName == "bar" || spriteName == "cloud";
        }

        private static void ApplyOutline(Image image, bool enabled, Color accent)
        {
            Outline outline = image.GetComponent<Outline>();
            if (outline == null) outline = image.gameObject.AddComponent<Outline>();
            outline.enabled = enabled;
            outline.effectColor = ReUIPalette.WithAlpha(accent, 0.64f);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = false;
        }
    }
}
