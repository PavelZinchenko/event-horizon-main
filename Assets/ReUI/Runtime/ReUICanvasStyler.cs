using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    [Flags]
    internal enum ReUIStyleFlags
    {
        None = 0,
        Surface = 1 << 0,
        Button = 1 << 1,
        Text = 1 << 2,
        Control = 1 << 3,
        Icon = 1 << 4,
    }

    [DisallowMultipleComponent]
    internal sealed class ReUIStyledElement : MonoBehaviour
    {
        public ReUIStyleFlags Flags;
    }

    public static class ReUICanvasStyler
    {
        private const string MainMenuSceneName = "MainMenuScene";
        private static Sprite _roundedSprite;

        private static readonly string[] ArtworkTokens =
        {
            "icon", "logo", "sprite", "portrait", "avatar", "character", "faction",
            "planet", "shipimage", "ship image", "preview", "thumbnail", "minimap",
            "radar", "galaxy", "starmap", "star map", "grid", "weapon", "bullet",
            "componentimage", "component image", "resource", "currency", "backgroundimage",
            "background image", "checkmark", "arrow", "line", "mask", "viewport",
            "fill", "handle", "glow", "effect", "illustration", "photo"
        };

        private static readonly string[] SurfaceTokens =
        {
            "window", "dialog", "popup", "modal", "panel", "menu", "frame", "drawer",
            "sheet", "toolbar", "header", "footer", "navigation", "navbar", "tabbar",
            "tab bar", "container", "content background", "contentbackground", "hud",
            "settings background", "settingsbackground", "overlay panel", "overlaypanel"
        };

        private static readonly string[] ElevatedTokens =
        {
            "window", "dialog", "popup", "modal", "sheet", "confirmation", "message"
        };

        private static readonly string[] NavigationTokens =
        {
            "menu", "toolbar", "navigation", "navbar", "tabbar", "tab bar", "header", "footer", "hud"
        };

        private static readonly string[] ListElementTokens =
        {
            "item", "cell", "slot", "entry", "list element", "listelement", "shipitem",
            "hangaritem", "product", "reward", "toggle", "keysetup", "componentitem",
            "groupitem", "ship prefab", "shipprefab", "questitem", "shopitem", "preset"
        };

        private static readonly string[] ContentListAncestorTokens =
        {
            "componentitem", "groupitem", "shiplist", "satellitelist", "buildlist",
            "listscrollrect", "questitem", "shopitem", "shipprefab", "scrollrect/content"
        };

        private static readonly string[] ProtectedGameplayAncestorTokens =
        {
            "captainpanel", "captainlist", "researchpanel", "techtreepanel", "techtree", "techitem",
            "factions", "factionslayout", "preview7threebodytree", "preview7skilltabs",
            // Combat radar/minimap dots deliberately encode target type, faction,
            // danger and projectile class through their authored colors. Protect
            // the complete hierarchy from generic Image/Button/Text restyling.
            "radarpanel", "preview5combatminimap", "combatminimap"
        };

        public static void Apply(Canvas canvas)
        {
            if (canvas == null || !canvas.gameObject.scene.IsValid()) return;

            // Skill-tree nodes and technology-tree entries encode availability,
            // ownership and faction state through their original graphics. Generic
            // restyling destroys those semantics, so SkillTreeScene is handled only
            // by its dedicated, non-invasive styler.
            if (canvas.gameObject.scene.name == "SkillTreeScene")
            {
                ReUISkillTreeStyler.Apply(canvas);
                return;
            }

            // The main menu uses a small, stable set of authored controls, so its
            // complete replacement treatment is safe.  The gameplay menus reuse
            // the same Button/Image components for maps, list cards, selectors and
            // stateful widgets.  Replacing their target sprite, layout or colour in
            // a generic hierarchy scan caused the oversized cyan/orange/purple
            // controls seen after entering the game.  Those scenes are now handled
            // only by their explicit, layout-aware stylers below.
            bool replaceAuthoredPresentation = canvas.gameObject.scene.name == MainMenuSceneName;
            if (replaceAuthoredPresentation)
            {
                AddAmbientLayer(canvas);

                foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
                    if (!IsProtectedGameplayContent(image.transform)) StyleSurface(image);

                foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
                    if (!IsProtectedGameplayContent(button.transform)) StyleButton(button);

                foreach (Text text in canvas.GetComponentsInChildren<Text>(true))
                    if (!IsProtectedGameplayContent(text.transform)) StyleText(text);

                foreach (Toggle toggle in canvas.GetComponentsInChildren<Toggle>(true))
                    if (!IsProtectedGameplayContent(toggle.transform)) StyleToggle(toggle);

                foreach (Slider slider in canvas.GetComponentsInChildren<Slider>(true))
                    if (!IsProtectedGameplayContent(slider.transform)) StyleSlider(slider);

                foreach (Scrollbar scrollbar in canvas.GetComponentsInChildren<Scrollbar>(true))
                    if (!IsProtectedGameplayContent(scrollbar.transform)) StyleScrollbar(scrollbar);

                foreach (InputField input in canvas.GetComponentsInChildren<InputField>(true))
                    if (!IsProtectedGameplayContent(input.transform)) StyleInputField(input);
            }

            ReUIMainMenuStyler.Apply(canvas);
            ReUISettingsStyler.Apply(canvas);
            ReUIStarMapStyler.Apply(canvas);
            ReUIFactionPanelStyler.Apply(canvas);
            ReUIMarketStyler.Apply(canvas);
            ReUICaptainStyler.Apply(canvas);
            ReUITechTreeStyler.Apply(canvas);
            ReUIShipEditorStyler.Apply(canvas);
            ReUIShipServiceStyler.Apply(canvas);
            ReUISkillTreeStyler.Apply(canvas);
            ReUIMultiplayerStyler.Apply(canvas);
            ReUICombatStyler.Apply(canvas);
            ReUIDialogStyler.Apply(canvas);
            ReUIArenaStyler.Apply(canvas);
            ReUIHudStyler.Apply(canvas);
            if (replaceAuthoredPresentation)
                NormalizeButtonPresentation(canvas);
        }

        internal static void ResetStyleFlags(Canvas canvas)
        {
            if (canvas == null) return;

            ReUIStyledElement[] styled = canvas.GetComponentsInChildren<ReUIStyledElement>(true);
            for (int i = 0; i < styled.Length; i++)
            {
                if (styled[i] != null)
                    styled[i].Flags = ReUIStyleFlags.None;
            }
        }

        private static void AddAmbientLayer(Canvas canvas)
        {
            if (!canvas.isRootCanvas || canvas.renderMode == RenderMode.WorldSpace) return;

            // A full-screen translucent overlay is not liquid glass: it simply tints
            // the entire artwork and creates the foggy/plastic look reported on device.
            // Individual surfaces provide their own stock-UGUI outline treatment;
            // they intentionally do not sample or blur the game framebuffer.
            Transform existing = canvas.transform.Find("ReUI Ambient Layer");
            if (existing != null) UnityEngine.Object.Destroy(existing.gameObject);
        }

        private static bool LooksLikeFullscreenBackground(Transform transform)
        {
            if (transform is not RectTransform rect) return false;
            string name = Normalize(transform.name);
            bool stretched = rect.anchorMin.x <= 0.01f && rect.anchorMin.y <= 0.01f &&
                             rect.anchorMax.x >= 0.99f && rect.anchorMax.y >= 0.99f;
            return stretched && (name.Contains("background") || name.Contains("backdrop") || name.Contains("wallpaper"));
        }

        private static void StyleSurface(Image image)
        {
            if (image == null || image.GetComponent<Button>() != null) return;
            if (HasFlag(image.gameObject, ReUIStyleFlags.Surface)) return;

            string name = Normalize(image.gameObject.name);
            if (!ContainsAny(name, SurfaceTokens) || ContainsAny(name, ArtworkTokens)) return;
            if (image.color.a < 0.06f) return;

            bool elevated = ContainsAny(name, ElevatedTokens);
            bool navigation = ContainsAny(name, NavigationTokens);

            image.sprite = RoundedSprite;
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            image.color = elevated
                ? ReUIPalette.GlassElevated
                : navigation
                    ? ReUIPalette.GlassPrimary
                    : ReUIPalette.GlassSecondary;

            ReUIEffectStyler.ApplyPanel(image, elevated);

            if (image.GetComponent<Mask>() == null && image.GetComponent<RectMask2D>() == null)
            {
                Outline outline = image.GetComponent<Outline>();
                if (outline == null) outline = image.gameObject.AddComponent<Outline>();
                outline.effectColor = elevated ? ReUIPalette.OutlineStrong : ReUIPalette.Outline;
                outline.effectDistance = new Vector2(1f, -1f);
                outline.useGraphicAlpha = true;

                // Keep popup depth light: a heavy opaque shadow would obscure
                // the authored menu artwork behind the surface.
                Shadow shadow = image.GetComponent<Shadow>();
                if (shadow != null) shadow.enabled = false;
            }

            AddFlag(image.gameObject, ReUIStyleFlags.Surface);
        }

        private static void StyleButton(Button button)
        {
            if (button == null) return;

            if (IsContentListButton(button))
            {
                NormalizeContentListButton(button);
                return;
            }

            string descriptor = BuildDescriptor(button.gameObject);
            if (!HasFlag(button.gameObject, ReUIStyleFlags.Button))
            {
                bool danger = ContainsAny(descriptor, new[] { "delete", "remove", "danger", "quit", "exit", "destroy", "放弃", "删除", "退出" });
                bool premium = ContainsAny(descriptor, new[] { "premium", "purchase", "buy", "store", "shop", "market", "购买", "商店" });

                if (button.targetGraphic is Image image)
                {
                    image.sprite = RoundedSprite;
                    image.type = Image.Type.Sliced;
                    image.preserveAspect = false;
                    // ReUI uses glass as a surface, not a solid faction/status fill.
                    // Danger and premium states are expressed through outline and icon color.
                    image.color = ReUIPalette.WithAlpha(ReUIPalette.GlassSoft, 0.10f);

                    Outline outline = image.GetComponent<Outline>();
                    if (outline == null) outline = image.gameObject.AddComponent<Outline>();
                    outline.effectColor = danger
                        ? ReUIPalette.WithAlpha(ReUIPalette.AccentRed, 0.65f)
                        : premium
                            ? ReUIPalette.WithAlpha(ReUIPalette.AccentGold, 0.55f)
                            : ReUIPalette.Outline;
                    outline.effectDistance = new Vector2(1f, -1f);
                    outline.useGraphicAlpha = false;
                }

                Color accent = danger ? ReUIPalette.AccentRed : premium ? ReUIPalette.AccentGold : ReUIPalette.AccentCyan;
                ColorBlock colors = button.colors;
                Color enabledColor = Color.Lerp(Color.white, accent, 0.24f);
                colors.normalColor = enabledColor;
                colors.highlightedColor = enabledColor;
                colors.selectedColor = enabledColor;
                colors.pressedColor = Color.Lerp(enabledColor, accent, 0.10f);
                colors.disabledColor = new Color(0.42f, 0.47f, 0.53f, 0.72f);
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.10f;
                button.colors = colors;
                button.transition = Selectable.Transition.ColorTint;

                if (button.GetComponent<ReUIButtonMotion>() == null)
                    button.gameObject.AddComponent<ReUIButtonMotion>();

                foreach (Text label in button.GetComponentsInChildren<Text>(true))
                {
                    label.color = button.interactable
                        ? ReUIPalette.TextPrimary
                        : new Color(0.84f, 0.90f, 0.96f, 1f);
                    if (label.fontStyle == FontStyle.Normal)
                        label.fontStyle = FontStyle.Bold;
                }

                AddFlag(button.gameObject, ReUIStyleFlags.Button);
            }

            ReapplyTransparentButtonSurface(button, descriptor);
            TryInstallSemanticIcon(button, descriptor);
        }

        private static void ReapplyTransparentButtonSurface(Button button, string descriptor)
        {
            if (button == null) return;
            Image image = button.targetGraphic as Image;
            if (image == null) image = button.GetComponent<Image>();
            if (image == null) return;

            bool danger = ContainsAny(descriptor, new[] { "delete", "remove", "danger", "quit", "exit", "destroy", "放弃", "删除", "退出" });
            bool premium = ContainsAny(descriptor, new[] { "premium", "purchase", "buy", "store", "shop", "market", "购买", "商店" });

            image.enabled = true;
            image.sprite = RoundedSprite;
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            image.color = new Color(0.90f, 0.97f, 1.00f, 0.035f);

            Outline outline = image.GetComponent<Outline>();
            if (outline == null) outline = image.gameObject.AddComponent<Outline>();
            outline.enabled = true;
            outline.effectColor = danger
                ? ReUIPalette.WithAlpha(ReUIPalette.AccentRed, 0.58f)
                : premium
                    ? ReUIPalette.WithAlpha(ReUIPalette.AccentGold, 0.52f)
                    : ReUIPalette.WithAlpha(ReUIPalette.AccentCyan, 0.34f);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = false;

            Outline[] outlines = image.GetComponents<Outline>();
            for (int i = 1; i < outlines.Length; i++) outlines[i].enabled = false;

            ReUIEffectRole role = danger
                ? ReUIEffectRole.DangerButton
                : ContainsAny(descriptor, NavigationTokens)
                    ? ReUIEffectRole.NavigationButton
                    : premium
                        ? ReUIEffectRole.PrimaryButton
                        : ReUIEffectRole.SecondaryButton;
            ReUIEffectStyler.ApplyButton(button, role);
        }

        private static void NormalizeButtonPresentation(Canvas canvas)
        {
            if (canvas == null) return;

            ReUIIconGraphic[] allGeneratedIcons = canvas.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < allGeneratedIcons.Length; i++)
            {
                ReUIIconGraphic icon = allGeneratedIcons[i];
                if (icon == null || IsProtectedGameplayContent(icon.transform)) continue;
                icon.color = Color.white;
                icon.canvasRenderer.SetAlpha(1f);
                icon.canvasRenderer.cullTransparentMesh = false;
            }

            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null || IsProtectedGameplayContent(button.transform)) continue;

                RemoveAccidentalCloseOverlays(button);

                Image target = button.targetGraphic as Image;
                if (target == null) target = button.GetComponent<Image>();
                if (target != null)
                {
                    Outline[] outlines = target.GetComponents<Outline>();
                    for (int outlineIndex = 1; outlineIndex < outlines.Length; outlineIndex++)
                        outlines[outlineIndex].enabled = false;
                }

                Graphic[] graphics = button.GetComponentsInChildren<Graphic>(true);
                for (int graphicIndex = 0; graphicIndex < graphics.Length; graphicIndex++)
                {
                    Graphic graphic = graphics[graphicIndex];
                    if (graphic == null || graphic == target) continue;

                    if (graphic is Image image && IsButtonBackgroundImage(button, image))
                    {
                        Outline[] outlines = image.GetComponents<Outline>();
                        for (int outlineIndex = 1; outlineIndex < outlines.Length; outlineIndex++)
                            outlines[outlineIndex].enabled = false;
                        continue;
                    }

                    Color color = graphic.color;
                    color.a = 1f;
                    graphic.color = color;
                    graphic.canvasRenderer.SetAlpha(1f);
                    graphic.canvasRenderer.cullTransparentMesh = false;
                    DisableOutlines(graphic);
                }

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

                ReUIButtonMotion motion = button.GetComponent<ReUIButtonMotion>();
                if (motion == null) motion = button.gameObject.AddComponent<ReUIButtonMotion>();
                motion.RefreshVisualState();
            }
        }

        private static void RemoveAccidentalCloseOverlays(Button button)
        {
            if (button == null) return;
            string descriptor = BuildDescriptor(button.gameObject);
            bool explicitClose = ContainsAny(descriptor, new[]
            {
                "close", "exit", "cancel", "dismiss", "quit", "surrender",
                "关闭", "退出", "取消", "投降"
            });

            ReUIIconGraphic[] icons = button.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < icons.Length; i++)
                if (icons[i] != null && icons[i].Kind == ReUIIconKind.Close && !explicitClose)
                    icons[i].gameObject.SetActive(false);

            ReUIProhibitGraphic[] prohibitGraphics = button.GetComponentsInChildren<ReUIProhibitGraphic>(true);
            bool isClearButton = Normalize(button.name) == "clearbutton";
            for (int i = 0; i < prohibitGraphics.Length; i++)
                if (prohibitGraphics[i] != null && !isClearButton)
                    prohibitGraphics[i].gameObject.SetActive(false);
        }

        private static Color BrightestColor(params Color[] colors)
        {
            Color result = Color.white;
            float best = -1f;
            for (int i = 0; i < colors.Length; i++)
            {
                Color candidate = colors[i];
                float brightness = (candidate.r * 0.2126f + candidate.g * 0.7152f +
                                    candidate.b * 0.0722f) * candidate.a;
                if (brightness <= best) continue;
                best = brightness;
                result = candidate;
            }
            return result;
        }

        private static bool IsButtonBackgroundImage(Button button, Image image)
        {
            if (button == null || image == null) return false;
            if (image.transform == button.transform) return true;
            if (image.sprite == RoundedSprite) return true;

            Toggle ownerToggle = image.GetComponentInParent<Toggle>();
            if (ownerToggle != null && ownerToggle.targetGraphic == image)
                return true;

            Toggle[] nestedToggles = button.GetComponentsInChildren<Toggle>(true);
            for (int i = 0; i < nestedToggles.Length; i++)
                if (nestedToggles[i] != null && nestedToggles[i].targetGraphic == image)
                    return true;

            string name = Normalize(image.gameObject.name);
            return name == "background" || name == "focus" || name == "left" || name == "right" ||
                   name.Contains("surface") || name.Contains("button background") ||
                   name.Contains("buttonbackground");
        }

        private static void DisableOutlines(Graphic graphic)
        {
            if (graphic == null) return;
            Outline[] outlines = graphic.GetComponents<Outline>();
            for (int i = 0; i < outlines.Length; i++) outlines[i].enabled = false;
        }

        private static bool IsProtectedGameplayContent(Transform transform)
        {
            if (transform == null) return false;
            // Dynamic technology items can be nested more than twelve levels below
            // ResearchPanel. Walking the complete hierarchy prevents the generic pass
            // from disabling their original icon Images before the dedicated styler runs.
            Transform current = transform;
            while (current != null)
            {
                if (ContainsAny(Normalize(current.name), ProtectedGameplayAncestorTokens))
                    return true;

                MonoBehaviour[] behaviours = current.GetComponents<MonoBehaviour>();
                for (int i = 0; i < behaviours.Length; i++)
                {
                    string typeName = behaviours[i] != null ? behaviours[i].GetType().FullName : null;
                    if (typeName == "ViewModel.TechItemViewModel" ||
                        typeName == "ViewModel.TechTreePanelViewModel" ||
                        typeName == "ViewModel.FactionViewModel" ||
                        typeName == "ViewModel.ResearchPanelViewModel" ||
                        typeName == "ViewModel.Skills.SkillTreeNode" ||
                        typeName == "Gui.Combat.RadarPanel" ||
                        typeName == "Gui.Combat.Radar" ||
                        typeName == "Gui.Combat.BeaconRadar" ||
                        typeName == "Gui.Combat.CombatMinimap")
                        return true;
                }

                current = current.parent;
            }
            return false;
        }

        private static bool IsContentListButton(Button button)
        {
            Transform current = button.transform;
            var path = new StringBuilder();
            int depth = 0;
            while (current != null && depth++ < 8)
            {
                if (path.Length > 0) path.Insert(0, '/');
                path.Insert(0, Normalize(current.name));
                current = current.parent;
            }
            return ContainsAny(path.ToString(), ContentListAncestorTokens);
        }

        private static void NormalizeContentListButton(Button button)
        {
            if (button.targetGraphic is Image focus)
            {
                focus.material = null;
                focus.color = new Color(0.32f, 0.66f, 0.82f, 0.08f);

                Outline[] outlines = focus.GetComponents<Outline>();
                for (int i = 0; i < outlines.Length; i++) outlines[i].enabled = false;
                Shadow[] shadows = focus.GetComponents<Shadow>();
                for (int i = 0; i < shadows.Length; i++) shadows[i].enabled = false;
            }

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

            ReUIButtonMotion motion = button.GetComponent<ReUIButtonMotion>();
            if (motion == null) motion = button.gameObject.AddComponent<ReUIButtonMotion>();
            motion.enabled = true;
            motion.RefreshVisualState();

            ReUIIconGraphic[] generatedIcons = button.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < generatedIcons.Length; i++)
                generatedIcons[i].gameObject.SetActive(false);
        }

        private static void TryInstallSemanticIcon(Button button, string descriptor)
        {
            // The captain shortcut owns an authored Sprite Image.  It must not
            // be replaced by the generic vector-icon path.
            if (button.name == "ThreeBodyCaptainButton") return;
            if (button.name == "ConfigureEnemyFleet" || button.name == "ConfigureAllyFleet") return;
            if (HasFlag(button.gameObject, ReUIStyleFlags.Icon)) return;
            if (ContainsAny(descriptor, ListElementTokens)) return;

            ReUIIconKind iconKind = DetectIconKind(descriptor);
            if (iconKind == ReUIIconKind.None) return;
            if (InstallSemanticIcon(button, iconKind))
                AddFlag(button.gameObject, ReUIStyleFlags.Icon);
        }

        internal static bool ForceSemanticIcon(Button button, ReUIIconKind kind)
        {
            if (button == null) return false;
            if (button.name == "ThreeBodyCaptainButton")
            {
                ClearSemanticIcon(button);
                return true;
            }
            if (kind == ReUIIconKind.None)
            {
                ClearSemanticIcon(button);
                return true;
            }
            bool installed = InstallSemanticIcon(button, kind);
            if (installed) AddFlag(button.gameObject, ReUIStyleFlags.Icon);
            return installed;
        }

        internal static void ClearSemanticIcon(Button button)
        {
            if (button == null) return;
            ReUIIconGraphic[] generated = button.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < generated.Length; i++)
                if (generated[i] != null)
                    generated[i].gameObject.SetActive(false);
        }

        private static bool InstallSemanticIcon(Button button, ReUIIconKind kind)
        {
            ReUIIconGraphic existing = button.GetComponentInChildren<ReUIIconGraphic>(true);
            if (existing != null)
            {
                existing.Kind = kind;
                existing.gameObject.SetActive(true);
                existing.enabled = true;
                existing.color = Color.white;
                return true;
            }

            Image target = button.targetGraphic as Image;
            Image iconImage = FindIconImage(button, target);

            RectTransform host;
            if (iconImage != null)
            {
                iconImage.enabled = false;
                host = iconImage.rectTransform;
            }
            else
            {
                RectTransform buttonRect = button.transform as RectTransform;
                if (buttonRect == null) return false;

                bool hasLabel = button.GetComponentInChildren<Text>(true) != null;
                float width = Mathf.Abs(buttonRect.rect.width);
                float height = Mathf.Abs(buttonRect.rect.height);
                bool iconFriendly = !hasLabel || width <= height * 1.65f;
                if (!iconFriendly) return false;

                GameObject hostObject = new("ReUI Icon Host", typeof(RectTransform));
                host = (RectTransform)hostObject.transform;
                host.SetParent(button.transform, false);
                float iconSize = Mathf.Clamp(Mathf.Min(width, height) * 0.68f, 28f, 96f);
                host.anchorMin = host.anchorMax = new Vector2(0.5f, 0.5f);
                host.pivot = new Vector2(0.5f, 0.5f);
                host.sizeDelta = new Vector2(iconSize, iconSize);
                host.anchoredPosition = Vector2.zero;
            }

            GameObject iconObject = new("ReUI Vector Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(ReUIIconGraphic));
            RectTransform iconRect = (RectTransform)iconObject.transform;
            iconRect.SetParent(host, false);
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            ReUIIconGraphic graphic = iconObject.GetComponent<ReUIIconGraphic>();
            graphic.Kind = kind;
            graphic.color = Color.white;
            return true;
        }

        private static Image FindIconImage(Button button, Image target)
        {
            Image[] images = button.GetComponentsInChildren<Image>(true);
            Image fallback = null;

            foreach (Image image in images)
            {
                if (image == target) continue;
                string name = Normalize(image.gameObject.name);
                if (ContainsAny(name, new[] { "background", "frame", "fill", "mask", "viewport", "glow", "selection" }))
                    continue;

                if (name.Contains("icon") || name.Contains("image") || name.Contains("sprite"))
                    return image;

                Rect rect = image.rectTransform.rect;
                float width = Mathf.Abs(rect.width);
                float height = Mathf.Abs(rect.height);
                if (width >= 18f && height >= 18f && width <= 256f && height <= 256f && fallback == null)
                    fallback = image;
            }

            return fallback;
        }

        private static void StyleText(Text text)
        {
            if (text == null || HasFlag(text.gameObject, ReUIStyleFlags.Text)) return;

            string name = Normalize(text.gameObject.name);
            bool header = ContainsAny(name, new[] { "title", "header", "caption", "headline", "window name", "windowname" });
            bool muted = ContainsAny(name, new[] { "hint", "description", "secondary", "placeholder", "details", "small" });
            bool resource = ContainsAny(name, new[]
            {
                "money", "credits", "fuel", "stars", "tokens", "snowflake", "damage",
                "health", "hitpoints", "armor", "shield", "energy", "condition",
                "resourcevalue", "firedefense", "energydefense", "kineticdefense", "corrosive"
            });

            if (!resource)
                text.color = header ? ReUIPalette.TextPrimary : muted ? ReUIPalette.TextSecondary : ReUIPalette.TextPrimary;

            if (header)
            {
                text.fontStyle = FontStyle.Bold;
                text.lineSpacing = Mathf.Max(text.lineSpacing, 1.05f);
                if (text.GetComponent<Shadow>() == null)
                {
                    Shadow shadow = text.gameObject.AddComponent<Shadow>();
                    shadow.effectColor = new Color(0.05f, 0.22f, 0.38f, 0.48f);
                    shadow.effectDistance = new Vector2(0f, -1f);
                    shadow.useGraphicAlpha = true;
                }
            }

            AddFlag(text.gameObject, ReUIStyleFlags.Text);
        }

        private static void StyleToggle(Toggle toggle)
        {
            if (toggle == null || HasFlag(toggle.gameObject, ReUIStyleFlags.Control)) return;

            ColorBlock colors = toggle.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.disabledColor = Color.white;
            colors.fadeDuration = 0f;
            toggle.colors = colors;
            toggle.transition = Selectable.Transition.None;

            if (toggle.targetGraphic is Image background)
            {
                background.sprite = RoundedSprite;
                background.type = Image.Type.Sliced;
                background.color = ReUIPalette.GlassSoft;
            }

            ReUIEffectStyler.ApplyToggle(toggle);

            if (toggle.graphic is Image checkmark)
                checkmark.color = ReUIPalette.AccentCyan;

            ReUIButtonMotion motion = toggle.GetComponent<ReUIButtonMotion>();
            if (motion == null) motion = toggle.gameObject.AddComponent<ReUIButtonMotion>();
            motion.enabled = true;
            motion.RefreshVisualState();

            AddFlag(toggle.gameObject, ReUIStyleFlags.Control);
        }

        private static void StyleSlider(Slider slider)
        {
            if (slider == null || HasFlag(slider.gameObject, ReUIStyleFlags.Control)) return;

            if (slider.fillRect != null && slider.fillRect.TryGetComponent(out Image fill))
                fill.color = ReUIPalette.AccentCyan;

            if (slider.handleRect != null && slider.handleRect.TryGetComponent(out Image handle))
            {
                handle.sprite = RoundedSprite;
                handle.type = Image.Type.Sliced;
                handle.color = ReUIPalette.TextPrimary;
                Outline outline = handle.GetComponent<Outline>();
                if (outline == null) outline = handle.gameObject.AddComponent<Outline>();
                outline.effectColor = ReUIPalette.OutlineStrong;
                outline.effectDistance = new Vector2(1f, -1f);
            }

            AddFlag(slider.gameObject, ReUIStyleFlags.Control);
        }

        private static void StyleScrollbar(Scrollbar scrollbar)
        {
            if (scrollbar == null || HasFlag(scrollbar.gameObject, ReUIStyleFlags.Control)) return;

            if (scrollbar.targetGraphic is Image handle)
            {
                handle.sprite = RoundedSprite;
                handle.type = Image.Type.Sliced;
                handle.color = ReUIPalette.WithAlpha(ReUIPalette.AccentCyan, 0.72f);
            }

            Image background = scrollbar.GetComponent<Image>();
            if (background != null)
            {
                background.sprite = RoundedSprite;
                background.type = Image.Type.Sliced;
                background.color = ReUIPalette.GlassSoft;
            }

            AddFlag(scrollbar.gameObject, ReUIStyleFlags.Control);
        }

        private static void StyleInputField(InputField input)
        {
            if (input == null || HasFlag(input.gameObject, ReUIStyleFlags.Control)) return;

            if (input.targetGraphic is Image background)
            {
                background.sprite = RoundedSprite;
                background.type = Image.Type.Sliced;
                background.color = ReUIPalette.GlassSecondary;
                ReUIEffectStyler.ApplyPanel(background);
                Outline outline = background.GetComponent<Outline>();
                if (outline == null) outline = background.gameObject.AddComponent<Outline>();
                outline.effectColor = ReUIPalette.Outline;
                outline.effectDistance = new Vector2(1f, -1f);
            }

            if (input.textComponent != null)
                input.textComponent.color = ReUIPalette.TextPrimary;
            if (input.placeholder is Text placeholder)
                placeholder.color = ReUIPalette.TextMuted;

            AddFlag(input.gameObject, ReUIStyleFlags.Control);
        }

        private static ReUIIconKind DetectIconKind(string descriptor)
        {
            if (ContainsAny(descriptor, new[] { "newgame", "continue", "startgame", "play", "开始游戏", "继续游戏" })) return ReUIIconKind.StarMap;
            if (ContainsAny(descriptor, new[] { "relations", "faction", "势力", "阵营" })) return ReUIIconKind.StarMap;
            if (ContainsAny(descriptor, new[] { "captain", "commander", "舰长", "指挥官" })) return ReUIIconKind.Multiplayer;
            if (ContainsAny(descriptor, new[] { "undo", "撤销" })) return ReUIIconKind.Undo;
            // Clear/remove-all actions may already carry a purpose-built single
            // slash marker. Do not auto-overlay a second X icon on them.
            if (ContainsAny(descriptor, new[] { "clear", "removeall", "清空", "全部移除" })) return ReUIIconKind.None;
            if (ContainsAny(descriptor, new[] { "close", "dismiss", "关闭" })) return ReUIIconKind.Close;
            if (ContainsAny(descriptor, new[] { "back", "return", "返回" })) return ReUIIconKind.Back;
            if (ContainsAny(descriptor, new[] { "multiplayer", "multi test", "multitest", "联机", "多人" })) return ReUIIconKind.Multiplayer;
            if (ContainsAny(descriptor, new[] { "research", "technology", "tech", "skill tree", "skilltree", "科技", "研究" })) return ReUIIconKind.Technology;
            if (ContainsAny(descriptor, new[] { "ship editor", "shipeditor", "constructor", "shipyard", "layout", "舰船编辑", "造船" })) return ReUIIconKind.ShipEditor;
            if (ContainsAny(descriptor, new[] { "equipment", "component", "craft", "inventory", "cargo", "mods", "装备", "组件", "仓库" })) return ReUIIconKind.Equipment;
            if (ContainsAny(descriptor, new[] { "quest", "journal", "mission", "任务", "日志" })) return ReUIIconKind.Missions;
            if (ContainsAny(descriptor, new[] { "settings", "options", "configure", "controls", "设置", "选项" })) return ReUIIconKind.Settings;
            if (ContainsAny(descriptor, new[] { "combat", "battle", "fight", "arena", "战斗", "竞技场" })) return ReUIIconKind.Battle;
            if (ContainsAny(descriptor, new[] { "star map", "starmap", "galaxy map", "galaxymap", "星图", "银河" })) return ReUIIconKind.StarMap;
            if (ContainsAny(descriptor, new[] { "store", "shop", "market", "purchase", "商店", "市场" })) return ReUIIconKind.Store;
            if (ContainsAny(descriptor, new[] { "ehopedia", "encyclopedia", "database", "图鉴", "百科" })) return ReUIIconKind.Encyclopedia;
            if (ContainsAny(descriptor, new[] { "fleet", "ships", "hangar", "舰队", "机库" })) return ReUIIconKind.Fleet;
            return ReUIIconKind.None;
        }

        private static string BuildDescriptor(GameObject gameObject)
        {
            StringBuilder builder = new(Normalize(gameObject.name));
            Text[] labels = gameObject.GetComponentsInChildren<Text>(true);
            int count = Mathf.Min(labels.Length, 4);
            for (int i = 0; i < count; i++)
            {
                if (string.IsNullOrWhiteSpace(labels[i].text)) continue;
                builder.Append(' ').Append(Normalize(labels[i].text));
            }
            return builder.ToString();
        }

        private static bool ContainsAny(string value, IReadOnlyList<string> tokens)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (value.Contains(tokens[i])) return true;
            }
            return false;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }

        internal static bool HasStyleFlag(GameObject gameObject, ReUIStyleFlags flag)
        {
            return HasFlag(gameObject, flag);
        }

        internal static void AddStyleFlag(GameObject gameObject, ReUIStyleFlags flag)
        {
            AddFlag(gameObject, flag);
        }

        private static bool HasFlag(GameObject gameObject, ReUIStyleFlags flag)
        {
            ReUIStyledElement marker = gameObject.GetComponent<ReUIStyledElement>();
            return marker != null && (marker.Flags & flag) != 0;
        }

        private static void AddFlag(GameObject gameObject, ReUIStyleFlags flag)
        {
            ReUIStyledElement marker = gameObject.GetComponent<ReUIStyledElement>();
            if (marker == null) marker = gameObject.AddComponent<ReUIStyledElement>();
            marker.Flags |= flag;
        }

        internal static Sprite SurfaceSprite => RoundedSprite;

        private static Sprite RoundedSprite
        {
            get
            {
                if (_roundedSprite != null) return _roundedSprite;

                const int size = 64;
                const float radius = 15f;
                Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
                {
                    name = "ReUI Rounded Surface",
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    hideFlags = HideFlags.HideAndDontSave,
                };

                Color32[] pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = Mathf.Max(Mathf.Abs(x + 0.5f - size * 0.5f) - (size * 0.5f - radius), 0f);
                        float dy = Mathf.Max(Mathf.Abs(y + 0.5f - size * 0.5f) - (size * 0.5f - radius), 0f);
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);
                        byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(radius + 0.5f - distance) * 255f);
                        pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply(false, true);
                _roundedSprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.FullRect,
                    new Vector4(18f, 18f, 18f, 18f));
                _roundedSprite.name = "ReUI Rounded Surface";
                _roundedSprite.hideFlags = HideFlags.HideAndDontSave;
                return _roundedSprite;
            }
        }
    }
}
