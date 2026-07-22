using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameDatabase.Model;
using DbAmmunition = GameDatabase.DataModel.Ammunition;
using DbComponent = GameDatabase.DataModel.Component;
using DbTechnology = GameDatabase.DataModel.Technology;
using Services.Resources;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReUI.Editor
{
    public static class ReUIValidation
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenuScene.unity";

        public static void ValidateClassicPresentation()
        {
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            string manifest = File.ReadAllText(manifestPath);
            if (manifest.Contains("com.coffee.ui-effect"))
                throw new InvalidOperationException("The abandoned UIEffect package is still referenced by Packages/manifest.json.");

            GameObject canvasObject = new("ReUI Classic Presentation Validation Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            try
            {
                Canvas canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                ReUIEffectRole[] roles =
                {
                    ReUIEffectRole.Panel,
                    ReUIEffectRole.Popup,
                    ReUIEffectRole.PrimaryButton,
                    ReUIEffectRole.SecondaryButton,
                    ReUIEffectRole.NavigationButton,
                    ReUIEffectRole.SelectedButton,
                    ReUIEffectRole.DisabledButton,
                    ReUIEffectRole.DangerButton,
                };

                for (int i = 0; i < roles.Length; i++)
                {
                    GameObject sample = new("ReUI Classic " + roles[i], typeof(RectTransform),
                        typeof(CanvasRenderer), typeof(Image));
                    sample.transform.SetParent(canvasObject.transform, false);
                    Image image = sample.GetComponent<Image>();
                    ReUIEffectMarker marker = ReUIEffectStyler.Apply(image, roles[i]);
                    if (marker == null || image.GetComponent<ReUIEffectMarker>() == null)
                        throw new InvalidOperationException("ReUI classic presentation surface was not configured correctly.");
                }

                GameObject toggleObject = new("ReUI Classic Toggle", typeof(RectTransform),
                    typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
                toggleObject.transform.SetParent(canvasObject.transform, false);
                Toggle toggle = toggleObject.GetComponent<Toggle>();
                toggle.targetGraphic = toggleObject.GetComponent<Image>();
                ReUIEffectStyler.ApplyToggle(toggle);
                toggle.isOn = true;
                ReUIEffectMarker toggleMarker = toggleObject.GetComponent<ReUIEffectMarker>();
                if (toggleMarker == null || toggleMarker.Role != ReUIEffectRole.SelectedButton)
                    throw new InvalidOperationException("ReUI toggle glass state did not follow its selected value.");

                Color original = ReUIPalette.ThemeColor;
                bool hadCustomTheme = ReUIPalette.HasCustomThemeColor;
                Color testColor = new(0.32f, 0.78f, 0.96f, 1f);
                ReUIPalette.SetThemeColor(testColor);
                if (Vector4.Distance(ReUIPalette.ThemeColor, testColor) > 0.001f)
                    throw new InvalidOperationException("ReUI theme palette did not retain a selected colour.");
                if (hadCustomTheme) ReUIPalette.SetThemeColor(original);
                else ReUIPalette.ResetThemeColor();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(canvasObject);
            }

            Debug.Log("[ReUI Validation] presentation=classic-ugui, thirdPartyUiEffect=false, themePalette=persistent");
        }

        public static void ValidateReUI5Data()
        {
            var database = new GameDatabase.Database();
            database.LoadDefault();

            var duplicateTechIds = database.TechnologyList
                .GroupBy(item => item.Id.Value)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();
            if (duplicateTechIds.Length > 0)
                throw new InvalidOperationException("Duplicate technology IDs: " + string.Join(",", duplicateTechIds));

            int[] starshipEarthShipTechs = { 375, 376, 377, 378, 379 };
            for (int i = 0; i < starshipEarthShipTechs.Length; ++i)
            {
                DbTechnology technology = database.GetTechnology(new ItemId<DbTechnology>(starshipEarthShipTechs[i]));
                if (technology == null || technology.Id.IsNull || GetTechnologyFactionId(technology) != 21)
                    throw new InvalidOperationException($"Technology {starshipEarthShipTechs[i]} is not assigned to Starship Earth.");
            }

            DbTechnology trisolarisTitan = database.GetTechnology(new ItemId<DbTechnology>(415));
            DbTechnology antigravityCore = database.GetTechnology(new ItemId<DbTechnology>(416));
            if (trisolarisTitan == null || trisolarisTitan.Id.IsNull || GetTechnologyFactionId(trisolarisTitan) != 22)
                throw new InvalidOperationException("Trisolaris Titan technology 415 is missing.");
            if (antigravityCore == null || antigravityCore.Id.IsNull || GetTechnologyFactionId(antigravityCore) != 22)
                throw new InvalidOperationException("Trisolaris antigravity technology 416 is missing.");

            int[] missileComponents = { 901, 902, 934, 951 };
            for (int i = 0; i < missileComponents.Length; ++i)
            {
                DbComponent component = database.GetComponent(new ItemId<DbComponent>(missileComponents[i]));
                if (component == null || component.Id.IsNull || component.PossibleModifications.Count < 10)
                    throw new InvalidOperationException($"Missile component {missileComponents[i]} has no complete modification pool.");
            }

            DbAmmunition wraith = database.GetAmmunition(new ItemId<DbAmmunition>(150));
            DbAmmunition wraithBig = database.GetAmmunition(new ItemId<DbAmmunition>(152));
            DbAmmunition dualVectorFoil = database.GetAmmunition(new ItemId<DbAmmunition>(168));
            if (wraith.Body.BulletPrefab.Id.Value != 21 || wraithBig.Body.BulletPrefab.Id.Value != 21)
                throw new InvalidOperationException("Wraith missiles no longer reference the standard rocket projectile.");
            if (dualVectorFoil.Body.BulletPrefab.Id.Value != 23)
                throw new InvalidOperationException("Dual-vector foil does not reference its dedicated projectile prefab.");

            ResourceLocator locatorPrefab = Resources.Load<ResourceLocator>("ResourceLocator");
            if (locatorPrefab == null)
                throw new InvalidOperationException("ResourceLocator prefab was not found.");
            ResourceLocator locator = UnityEngine.Object.Instantiate(locatorPrefab);
            try
            {
                Sprite sophon = locator.GetSprite(new SpriteId("sophon_launcher", SpriteId.Type.Ship));
                if (sophon == null || !sophon.name.EndsWith("_Clockwise90", StringComparison.Ordinal))
                    throw new InvalidOperationException("Sophon launcher sprite rotation correction was not applied.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(locator.gameObject);
            }

            Debug.Log(
                $"[ReUI5 Validation] tech21={database.TechnologyList.Count(item => GetTechnologyFactionId(item) == 21)}, " +
                $"tech22={database.TechnologyList.Count(item => GetTechnologyFactionId(item) == 22)}, " +
                $"missileMods={missileComponents.Length}, foilPrefab={dualVectorFoil.Body.BulletPrefab.Id.Value}, " +
                "sophonRotation=Clockwise90");
        }

        public static void ValidateReUI8Presentation()
        {
            ValidateSettingsPresentation();
            ValidateStarMapPresentation();
            ValidateFactionPanelPresentation();
            ValidateDialogPresentation();
            ValidateMarketPresentation();
            Debug.Log("[ReUI8 Validation] arenaFight=primary-emblem, settingsToggles=uniform, " +
                      "shopFilter=preserved, buyButton=hidden, bottomIcons=uniform, " +
                      "facilityBrightness=normal, dialogCancel=rectangular, marketFills=transparent");
        }

        public static void ValidateReUI9Presentation()
        {
            ValidateSettingsPresentation();
            ValidateStarMapPresentation();
            ValidateStarSystemObjectPresentation();
            ValidateFactionPanelPresentation();
            ValidateDialogPresentation();
            ValidateMarketPresentation();
            Debug.Log("[ReUI9 Validation] arenaFight=runtime-visible-stack, arenaLayout=deterministic, " +
                      "starObjectDisabled=readable, shopFilter=preserved, buyButton=hidden, " +
                      "bottomButtonHosts=uniform, mapSize=uniform, dialogCancel=rectangular, " +
                      "marketFills=transparent");
        }

        public static void ValidateReUI10Presentation()
        {
            ValidateSettingsPresentation();
            ValidateStarMapPresentation();
            ValidateStarSystemObjectPresentation();
            ValidateFactionPanelPresentation();
            ValidateDialogPresentation();
            ValidateMarketPresentation();
            ValidateCombatRewardTransparency();
            ValidateRadarColorProtection();
            Debug.Log("[ReUI10 Validation] buttonSurfaces=preserved, buttonTextIcons=opaque, glow=removed, " +
                      "arenaFight=dedicated-opaque-emblem, rewardCards=transparent, radarColors=preserved, " +
                      "radarMarkers=circular, shopFilter=preserved, buyButton=hidden, regressions=checked");
        }

        public static void ValidateReUI11Presentation()
        {
            ValidateReUI10Presentation();
            ValidateUniformEnabledButtonBrightness();
            ValidateShipEditorUndoAndCloseIcons();
            ValidateReUI11GameplayConfiguration();
            Debug.Log("[ReUI11 Validation] icons=uniform-full-brightness, accidentalCloseOverlays=removed, " +
                      "shipEditorUndo=180-degree-arrow, dualVectorFoil=small-white-paper, " +
                      "stellarHydrogenBomb=battlewide-30s-emp, sophon=player-and-ai-activation");
        }

        public static void ValidateReUI12Presentation()
        {
            ValidateReUI10Presentation();
            ValidateUniformEnabledButtonBrightness();
            ValidateShipEditorUndoAndCloseIcons();
            ValidateReUI11GameplayConfiguration();
            ValidateReUI12StableButtonsAndIcons();
            ValidateReUI12SophonPulseAndManualTitans();
            Debug.Log("[ReUI12 Validation] sophon=fixed-update-pulse, buttons=state-invariant, " +
                      "quickBattleIcon=removed, nextEnemyIcon=removed, configurableTitans=listed-and-parsed");
        }

        public static void ValidateReUI13Presentation()
        {
            ValidateReUI10Presentation();
            ValidateShipEditorUndoAndCloseIcons();
            ValidateReUI13StableButtonsAndIcons();
            ValidateReUI13SophonActivationAndManualTitans();
            Debug.Log("[ReUI13 Validation] sophonRequest=runtime-latched, jammedWeaponGuard=present, " +
                      "buttonBlink=executed-and-suppressed, quickBattleIcon=lightning, " +
                      "nextEnemyIcon=next-marker, configurableTitans=listed-and-parsed");
        }

        public static void ValidateReUI14Presentation()
        {
            ValidateReUI13Presentation();
            ValidateReUI14SophonProjectilePipeline();
            ValidateFactionVisibilityPreservation();
            ValidateUnavailableShipOverlay();
            ValidateSelectableRuntimeStability();
            Debug.Log("[ReUI14 Validation] sophon=invisible-short-range-expiring-projectile, " +
                      "enemyEmp=legacy-trigger-pipeline, buttons=button-toggle-actionbutton-stable, " +
                      "starbaseVisibility=original-logic-preserved, starbaseButtons=readable, " +
                      "unavailableShips=light-overlay-not-black-block");
        }

        public static void ValidateScopedUiRollback()
        {
            ValidateUiRuntimeSource();
            ValidateShipEditorDeviceListOnly();
            ValidateCombatHudOnly();
            ValidateRadarColorProtection();
            Debug.Log("[UI Runtime Validation] menus=authored, runtimeScan=removed, " +
                      "shipEditor=device-list-only, shipEditorIcons=original, " +
                      "combat=radar-and-resource-bars-only, settings=palette-only");
        }

        public static void ValidateAllEnabledScenesScopedSmoke()
        {
            string[] scenePaths = EditorBuildSettings.scenes
                .Where(item => item.enabled)
                .Select(item => item.path)
                .ToArray();

            int sceneCount = 0;
            int canvasCount = 0;
            for (int i = 0; i < scenePaths.Length; i++)
            {
                Scene scene = EditorSceneManager.OpenScene(scenePaths[i], OpenSceneMode.Single);
                if (!scene.IsValid() || !scene.isLoaded)
                    throw new InvalidOperationException("Scene could not be opened: " + scenePaths[i]);

                Canvas[] canvases = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Canvas>(true))
                    .ToArray();

                ApplyScopedRuntimeOnce();
                sceneCount++;
                canvasCount += canvases.Length;
                Debug.Log($"[Scoped UI Smoke] scene={scene.name}, canvases={canvases.Length}");
            }

            Debug.Log($"[Scoped UI Smoke] completed scenes={sceneCount}, canvases={canvasCount}");
        }

        public static void ValidateBeta4()
        {
            ValidateBeta5();
        }

        [MenuItem("Tools/ReUI/Validate Beta5")]
        public static void ValidateBeta5()
        {
            ValidateClassicPresentation();
            ValidateUiRuntimeSource();
            ValidateThemePaletteUi();
            ValidateStarMapIsUntouched();
            ValidateBeta5ThemeAndAssets();
            Debug.Log("[Beta5 Validation] uiTheme=player-selectable, presentation=classic-ugui, settings=serialized-controls, " +
                      "shipEditor=ui-settings, captain=authored-png, factionIcons=21-28-authored-png, " +
                      "runtimeScope=settings-palette-plus-specialized-ship-editor-and-combat");
        }

        private static void ValidateBeta5ThemeAndAssets()
        {
            var database = new GameDatabase.Database();
            database.LoadDefault();

            Color[] purpleTheme =
            {
                database.UiSettings.WindowColor,
                database.UiSettings.ScrollBarColor,
                database.UiSettings.IconColor,
                database.UiSettings.SelectionColor,
                database.UiSettings.ButtonColor,
                database.UiSettings.ButtonFocusColor,
                database.UiSettings.ButtonTextColor,
                database.UiSettings.ButtonIconColor,
                database.UiSettings.TextColor,
                database.UiSettings.HeaderTextColor,
                database.UiSettings.PaleTextColor,
                database.UiSettings.BrightTextColor,
                database.UiSettings.BackgroundDark,
                database.UiSettings.CommonQualityItemColor,
                database.UiSettings.AvailableTechColor,
                database.UiSettings.ObtainedTechColor,
                database.UiSettings.HiddenTechColor,
                database.UiSettings.CreditsColor,
                database.UiSettings.TokensColor,
            };

            if (purpleTheme.Any(color => color.b <= color.g || color.r <= color.g))
                throw new InvalidOperationException("UiTheme.json does not define a fully purple Beta5 presentation palette.");

            ValidateGeneratedSprite("Textures/UI/captain", "captain");
            for (int factionId = 21; factionId <= 28; factionId++)
                ValidateGeneratedSprite($"Textures/Factions/faction_{factionId}", $"faction_{factionId}");

            string faction28Path = Path.Combine(Application.dataPath, "Modules/Database/Resources/Database/Faction/28.json");
            if (!File.ReadAllText(faction28Path).Contains("\"Icon\": \"faction_28\""))
                throw new InvalidOperationException("Developer faction 28 does not reference its own Beta5 faction icon.");

            string guiRoot = Path.Combine(Application.dataPath, "Scripts/Gui");
            string paletteSource = File.ReadAllText(Path.Combine(guiRoot, "Common/ThreeBodyUiPalette.cs"));
            string gameMenuSource = File.ReadAllText(Path.Combine(guiRoot, "StarMap/GameMenu.cs"));
            string settingsGeneralSource = File.ReadAllText(Path.Combine(guiRoot, "MainMenu/SettingsGeneral.cs"));
            string settingsProgressSource = File.ReadAllText(Path.Combine(guiRoot, "MainMenu/SettingsProgress.cs"));
            string componentPanelSource = File.ReadAllText(Path.Combine(Application.dataPath,
                "ModulesShared/ShipEditor/Scripts/UI/ComponentPanel.cs"));
            string shipEditorWindowSource = File.ReadAllText(Path.Combine(Application.dataPath,
                "ModulesShared/ShipEditor/Scripts/UI/ShipEditorWindow.cs"));
            string texturePanelSource = File.ReadAllText(Path.Combine(Application.dataPath,
                "ModulesShared/ShipEditor/Scripts/UI/ShipTextureCustomizationPanel.cs"));
            string textureDisclaimerSource = File.ReadAllText(Path.Combine(Application.dataPath,
                "ModulesShared/ShipEditor/Scripts/UI/ShipTextureDisclaimerPanel.cs"));
            string planetBackgroundSource = File.ReadAllText(Path.Combine(Application.dataPath,
                "Scripts/Combat/Background/PlanetBackground.cs"));
            string factionImporterSource = File.ReadAllText(Path.Combine(Application.dataPath,
                "Editor/ThreeBodyGeneratedIconImporter.cs"));
            string starMapStylerSource = File.ReadAllText(Path.Combine(Application.dataPath,
                "ReUI/Runtime/ReUIStarMapStyler.cs"));

            if (!paletteSource.Contains("Resources.Load<Sprite>(\"Textures/UI/captain\")") ||
                paletteSource.Contains("captain_generated_base64") ||
                paletteSource.Contains("Convert.FromBase64String"))
                throw new InvalidOperationException("Captain shortcut still relies on a generated Base64 or temporary bitmap path.");

            int captainMethod = gameMenuSource.IndexOf("private static void EnsureCaptainIcon", StringComparison.Ordinal);
            int nextMethod = captainMethod >= 0
                ? gameMenuSource.IndexOf("\n        private ", captainMethod + 1, StringComparison.Ordinal)
                : -1;
            string captainSource = captainMethod >= 0
                ? gameMenuSource.Substring(captainMethod, (nextMethod >= 0 ? nextMethod : gameMenuSource.Length) - captainMethod)
                : string.Empty;
            if (!captainSource.Contains("ThreeBodyUiPalette.LoadCaptainIcon()") ||
                captainSource.Contains("faction_relations_preview4") ||
                !captainSource.Contains("ReUIIconGraphic"))
                throw new InvalidOperationException("Captain shortcut does not exclusively use the authored captain icon.");

            if (!settingsGeneralSource.Contains("ApplyThreeBodySettingsTheme()") ||
                !settingsGeneralSource.Contains("_languagesDropdown") ||
                !settingsGeneralSource.Contains("_soundVolumeSlider") ||
                !settingsGeneralSource.Contains("_lowQualityToggle") ||
                !settingsProgressSource.Contains("_database.UiSettings") ||
                !componentPanelSource.Contains("_database.UiSettings.ButtonColor") ||
                !shipEditorWindowSource.Contains("_database.UiSettings.ButtonColor") ||
                !texturePanelSource.Contains("UiSettings") ||
                !textureDisclaimerSource.Contains("UiSettings"))
                throw new InvalidOperationException("Beta5 still has an unthemed dynamic settings or ship-editor surface.");

            if (!planetBackgroundSource.Contains("UpdateViewSize()") ||
                planetBackgroundSource.Contains("_width = _height =") ||
                !planetBackgroundSource.Contains("camera.aspect"))
                throw new InvalidOperationException("Exploration planet background does not size itself to the full camera viewport.");

            if (!factionImporterSource.Contains("factionId >= 21") ||
                !factionImporterSource.Contains("factionId <= 28") ||
                starMapStylerSource.Contains("StyleDynamicShortcut(canvas, \"ThreeBodyCaptainButton\""))
                throw new InvalidOperationException("Beta5 generated icon import or captain shortcut isolation is incomplete.");
        }

        private static void ValidateGeneratedSprite(string resourcePath, string expectedName)
        {
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null || sprite.texture == null || sprite.texture.width != 512 || sprite.texture.height != 512 ||
                sprite.rect.width < 500f || sprite.rect.height < 500f)
                throw new InvalidOperationException($"Generated Beta5 sprite '{resourcePath}' is missing or was not imported as a 512x512 Sprite.");
        }

        private static void ValidateBeta4ControlIconsAndPurpleTheme()
        {
            var database = new GameDatabase.Database();
            database.LoadDefault();

            var guidance = database.GetDevice(new ItemId<GameDatabase.DataModel.Device>(39));
            var emp = database.GetWeapon(new ItemId<GameDatabase.DataModel.Weapon>(125));
            var threeBodyEmp = database.GetWeapon(new ItemId<GameDatabase.DataModel.Weapon>(147));
            if (guidance == null || guidance == GameDatabase.DataModel.Device.DefaultValue ||
                guidance.Stats.ControlButtonIcon.Id != "controls_sophon_guidance")
                throw new InvalidOperationException("Waterdrop Sophon guidance device does not reference its dedicated control icon.");
            if (emp == null || emp == GameDatabase.DataModel.Weapon.DefaultValue ||
                emp.Stats.ControlButtonIcon.Id != "controls_missile" ||
                threeBodyEmp == null || threeBodyEmp == GameDatabase.DataModel.Weapon.DefaultValue ||
                threeBodyEmp.Stats.ControlButtonIcon.Id != "controls_missile")
                throw new InvalidOperationException("EMP missile weapons do not use the missile control icon.");

            Sprite guidanceSprite = Resources.Load<Sprite>(
                "Textures/GUI/Controls/controls_sophon_guidance");
            if (guidanceSprite == null || guidanceSprite.rect.width < 32f || guidanceSprite.rect.height < 32f)
                throw new InvalidOperationException("Waterdrop Sophon guidance control texture was not imported as a usable sprite.");

            var theme = database.UiSettings;
            Color window = theme.WindowColor;
            Color button = theme.ButtonColor;
            Color icon = theme.IconColor;
            Color header = theme.HeaderTextColor;
            Color background = theme.BackgroundDark;
            Color credits = theme.CreditsColor;
            if (!(window.b > window.g && window.r > window.g) ||
                !(button.b > button.g && button.r > button.g) ||
                !(icon.b > icon.g && icon.r > icon.g) ||
                !(header.b > header.g && header.r > header.g) ||
                !(background.b > background.g && background.r > background.g) ||
                !(credits.b > credits.g && credits.r > credits.g))
                throw new InvalidOperationException(
                    $"UISettings main palette is not fully purple: window={window}, button={button}, " +
                    $"icon={icon}, header={header}, background={background}, credits={credits}.");
        }

        private static void ValidateBeta4StarMapControls()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/StarMapScene.unity", OpenSceneMode.Single);
            Gui.StarMap.GameMenu menu = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Gui.StarMap.GameMenu>(true))
                .FirstOrDefault();
            if (menu == null)
                throw new InvalidOperationException("Star-map GameMenu was not found.");

            InvokePrivate(menu, "CreateRelationsButton");
            InvokePrivate(menu, "CreateCaptainButton");
            InvokePrivate(menu, "HidePremiumBuyButton");
            Canvas.ForceUpdateCanvases();

            Button captain = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Button>(true))
                .FirstOrDefault(button => button.name == "ThreeBodyCaptainButton");
            Image captainIcon = captain != null
                ? captain.GetComponentsInChildren<Image>(true)
                    .FirstOrDefault(image => image.gameObject.name == "Icon" && image.sprite != null)
                : null;
            ReUIIconGraphic oldVectorIcon = captain != null
                ? captain.GetComponentsInChildren<ReUIIconGraphic>(true)
                    .FirstOrDefault(icon => icon.enabled && icon.gameObject.activeSelf)
                : null;
            Transform captainLabel = captain != null ? captain.transform.Find("Label") : null;
            if (captain == null || captainIcon == null || captainIcon.sprite.name != "captain" ||
                oldVectorIcon != null || captainIcon.color.a < 0.99f || captainIcon.canvasRenderer.GetAlpha() < 0.99f ||
                (captainLabel != null && captainLabel.gameObject.activeSelf))
                throw new InvalidOperationException("Captain shortcut does not use the generated bitmap icon exclusively.");

            Button buyButton = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Button>(true))
                .FirstOrDefault(button => button.name == "BuyButton" &&
                                          button.transform.parent != null &&
                                          button.transform.parent.name == "StatusPanel");
            if (buyButton == null || buyButton.gameObject.activeSelf)
                throw new InvalidOperationException("Star-map StatusPanel BuyButton was not hidden.");
        }

        private static void ValidateBeta4ThemeIntegrationAndLayout()
        {
            var database = new GameDatabase.Database();
            database.LoadDefault();
            Gui.Common.ThreeBodyUiPalette.Configure(database.UiSettings);

            Color configuredButton = Gui.Common.ThreeBodyUiPalette.Button;
            Color databaseButton = database.UiSettings.ButtonColor;
            if (!Approximately(configuredButton, databaseButton, 0.002f))
                throw new InvalidOperationException(
                    $"Dynamic UI palette does not read ButtonColor from UiSettings: " +
                    $"configured={configuredButton}, database={databaseButton}.");

            Sprite captain = Gui.Common.ThreeBodyUiPalette.LoadCaptainIcon();
            if (captain == null || captain.texture == null || captain.texture.width != 512 || captain.texture.height != 512)
                throw new InvalidOperationException("Generated captain bitmap resource is missing or has the wrong dimensions.");

            string guiRoot = Path.Combine(Application.dataPath, "Scripts/Gui");
            string paletteSource = File.ReadAllText(Path.Combine(guiRoot, "Common/ThreeBodyUiPalette.cs"));
            string mainMenuSource = File.ReadAllText(Path.Combine(guiRoot, "MainMenu/MainMenu.cs"));
            string starMapSource = File.ReadAllText(Path.Combine(guiRoot, "StarMap/GameMenu.cs"));
            string settingsSource = File.ReadAllText(Path.Combine(guiRoot, "MainMenu/SettingsProgress.cs"));
            string modificationsSource = File.ReadAllText(Path.Combine(guiRoot, "Craft/ModificationsPanel.cs"));
            string prologueSource = File.ReadAllText(Path.Combine(guiRoot, "Quests/ThreeBodyPrologueOverlay.cs"));
            string statsSource = File.ReadAllText(Path.Combine(guiRoot, "Combat/ShipStatsPanel.cs"));
            string componentPanelSource = File.ReadAllText(Path.Combine(Application.dataPath,
                "ModulesShared/ShipEditor/Scripts/UI/ComponentPanel.cs"));

            if (paletteSource.Contains("GetComponentsInChildren<Graphic>") ||
                paletteSource.Contains("GetComponentsInChildren<Selectable>") ||
                paletteSource.Contains("ApplyPurpleTheme"))
                throw new InvalidOperationException("Dynamic theme still uses recursive runtime color replacement.");

            string[] themeConsumers = { mainMenuSource, starMapSource, settingsSource };
            if (themeConsumers.Any(source => !source.Contains("ThreeBodyUiPalette.Configure(_database.UiSettings)")))
                throw new InvalidOperationException("A dynamic UI entry point does not configure its palette from UiSettings.");

            if (mainMenuSource.Contains("CreateMultiplayerButton(canvas);") ||
                !mainMenuSource.Contains("HideMultiplayerEntry();"))
                throw new InvalidOperationException("Main-menu multiplayer entry is still being created or is not hidden.");

            if (!componentPanelSource.Contains("new Vector2(0.86f, 0.86f)") ||
                !componentPanelSource.Contains("new Vector2(0.97f, 0.96f)") ||
                !componentPanelSource.Contains("optionHeight = options.Count > 0") ||
                !componentPanelSource.Contains("_database.UiSettings.ButtonColor") ||
                componentPanelSource.Contains("new Vector2(0.33f, 0.04f), new Vector2(0.67f, 0.13f)"))
                throw new InvalidOperationException(
                    "The actual component modification selector does not use a top-right close button and UiSettings colors.");

            if (!prologueSource.Contains("frame.sprite = LoadPurpleFrameSprite();") ||
                !prologueSource.Contains("prologue_frame_purple") ||
                !prologueSource.Contains("Color.HSVToRGB(targetHue"))
                throw new InvalidOperationException("Opening prologue frame is not converted to a cached purple texture.");

            if (!statsSource.Contains("ConfigureContinuousBars();") ||
                !statsSource.Contains("bar.UseSolidTexture();"))
                throw new InvalidOperationException("Pooled combat status panels do not force solid continuous bars.");
        }

        private static bool Approximately(Color left, Color right, float tolerance)
        {
            return Mathf.Abs(left.r - right.r) <= tolerance &&
                   Mathf.Abs(left.g - right.g) <= tolerance &&
                   Mathf.Abs(left.b - right.b) <= tolerance &&
                   Mathf.Abs(left.a - right.a) <= tolerance;
        }

        private static void ValidateBeta4CombatInt64Formatting()
        {
            MethodInfo formatter = typeof(Gui.Combat.ShipStatsPanel).GetMethod(
                "FormatResource", BindingFlags.Static | BindingFlags.NonPublic);
            if (formatter == null)
                throw new InvalidOperationException("Combat resource formatter was not found.");

            string formatted = formatter.Invoke(null, new object[] { 3000000000f, 6000000000f }) as string;
            string[] values = formatted?.Split('/');
            if (values == null || values.Length != 2 ||
                !long.TryParse(values[0], System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out long current) ||
                !long.TryParse(values[1], System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out long maximum) ||
                current <= int.MaxValue || maximum <= current)
                throw new InvalidOperationException("Combat resource values still overflow 32-bit integer display.");
        }

        private static void ValidateBeta4SophonProjectile()
        {
            ValidateReUI14SophonProjectilePipeline();

            var database = new GameDatabase.Database();
            database.LoadDefault();
            var build = database.GetShipBuild(new ItemId<GameDatabase.DataModel.ShipBuild>(1145148));
            DbComponent sophon = database.GetComponent(new ItemId<DbComponent>(952));
            var runtimeComponent = new Constructor.Component.CommonComponent(
                sophon, build.Ship.Layout.CellCount);
            float activationCost = runtimeComponent.Devices.Single().EnergyConsumption;
            if (sophon.Device.Stats.ScaleEnergyWithShipSize ||
                Mathf.Abs(sophon.Device.Stats.EnergyConsumption - 2000f) > 0.001f ||
                Mathf.Abs(activationCost - 2000f) > 0.001f)
                throw new InvalidOperationException(
                    $"Sophon actual activation cost is not fixed at 2000: " +
                    $"base={sophon.Device.Stats.EnergyConsumption}, " +
                    $"scale={sophon.Device.Stats.ScaleEnergyWithShipSize}, actual={activationCost}.");

            string source = File.ReadAllText(Path.Combine(Application.dataPath,
                "Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/SophonJammerDevice.cs"));
            if (!source.Contains("RequestActivation()") ||
                !source.Contains("TryFireEmpProjectile();") ||
                source.Contains("_activationRequested"))
                throw new InvalidOperationException("Player Sophon activation still waits on a fragile queued physics flag.");

            GameObject emptyBullet = Resources.Load<GameObject>("Combat/Bullets/Empty");
            UnityEngine.Component[] components = emptyBullet != null
                ? emptyBullet.GetComponents<UnityEngine.Component>()
                : Array.Empty<UnityEngine.Component>();
            if (emptyBullet == null ||
                !components.Any(item => item is Combat.Component.Body.IBodyComponent) ||
                !components.Any(item => item is Combat.Component.View.IView) ||
                !components.Any(item => item is Combat.Component.Collider.ICollider) ||
                !components.Any(item => item is Combat.Component.Helpers.IDependencyInjector))
                throw new InvalidOperationException("Invisible Sophon carrier prefab lacks required bullet runtime components.");
        }

        private static void ValidateBeta4EarthTitanWarpBinding()
        {
            var database = new GameDatabase.Database();
            database.LoadDefault();
            var build = database.GetShipBuild(new ItemId<GameDatabase.DataModel.ShipBuild>(94008));
            var warp = build.Components.FirstOrDefault(item => item.Component.Id.Value == 305);
            if (warp == null || warp == GameDatabase.DataModel.InstalledComponent.DefaultValue || warp.KeyBinding != 6)
                throw new InvalidOperationException(
                    "Earth Titan warp drive is not assigned to key binding 6.");
        }

        private static void InvokePrivate(object instance, string methodName)
        {
            MethodInfo method = instance?.GetType().GetMethod(
                methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
                throw new InvalidOperationException("Private validation target was not found: " + methodName);
            method.Invoke(instance, null);
        }

        private static void ValidateUiRuntimeSource()
        {
            string runtime = Path.Combine(Application.dataPath, "ReUI/Runtime");
            string bootstrap = File.ReadAllText(Path.Combine(runtime, "ReUIBootstrap.cs"));
            string shipEditor = File.ReadAllText(Path.Combine(runtime, "ReUIShipEditorStyler.cs"));
            string hud = File.ReadAllText(Path.Combine(runtime, "ReUIHudStyler.cs"));
            string buttonMotion = File.ReadAllText(Path.Combine(runtime, "ReUIButtonMotion.cs"));
            string effectStyler = File.ReadAllText(Path.Combine(runtime, "ReUIEffectStyler.cs"));
            string canvasStyler = File.ReadAllText(Path.Combine(runtime, "ReUICanvasStyler.cs"));
            string starMapStyler = File.ReadAllText(Path.Combine(runtime, "ReUIStarMapStyler.cs"));

            if (bootstrap.Contains("DynamicUiScanInterval") ||
                bootstrap.Contains("ScanForDynamicUi") ||
                bootstrap.Contains("private void LateUpdate()"))
                throw new InvalidOperationException("Global or per-frame ReUI scanning is still active.");

            if (!bootstrap.Contains("ReUIShipEditorStyler.Apply") ||
                !bootstrap.Contains("ReUIHudStyler.Apply") ||
                !bootstrap.Contains("ReUIThemePalettePanel.EnsureForSettings") ||
                bootstrap.Contains("ReUICanvasStyler.Apply(canvas)"))
                throw new InvalidOperationException("ReUI runtime scope is not restored to the Beta5 scene set.");

            if (!bootstrap.Contains("return sceneName == ShipEditorSceneName") ||
                !bootstrap.Contains("sceneName == CombatSceneName") ||
                !bootstrap.Contains("sceneName == SettingsSceneName"))
                throw new InvalidOperationException("ReUI bootstrap no longer limits itself to the Beta5 scene set.");

            if (buttonMotion.Contains("void Update(") || buttonMotion.Contains("void LateUpdate(") ||
                effectStyler.Contains("void Update(") || effectStyler.Contains("void LateUpdate("))
                throw new InvalidOperationException("ReUI glass presentation still uses a per-frame writer.");

            if (canvasStyler.Contains("Every player-facing") ||
                canvasStyler.Contains("menu scenes is styled at creation time"))
                throw new InvalidOperationException("Generic ReUI styling still claims ownership of player-facing menu scenes.");

            if (starMapStyler.Contains("StyleStarSystemObjectButtons(canvas);") ||
                starMapStyler.Contains("layout.minWidth =") ||
                starMapStyler.Contains("layout.preferredWidth ="))
                throw new InvalidOperationException("Star-map styling still overrides authored dynamic-card or toolbar layout.");

            if (shipEditor.Contains("StyleButton(") ||
                shipEditor.Contains("StyleRemoveAllButton") ||
                shipEditor.Contains("\"ShipList\"") ||
                shipEditor.Contains("\"SatelliteList\"") ||
                shipEditor.Contains("\"BuildList\""))
                throw new InvalidOperationException("Ship editor styling still changes navigation or non-device panels.");

            if (hud.Contains("StyleShipSelectionBars") ||
                hud.Contains("StyleShipEditorMiniStats"))
                throw new InvalidOperationException("HUD styling still changes UI outside combat resource bars.");

            if (typeof(ActionButton).GetMethod("LateUpdate",
                    BindingFlags.Instance | BindingFlags.NonPublic) != null)
                throw new InvalidOperationException("ActionButton still contains the ReUI per-frame brightness writer.");
            if (typeof(Gui.StarMap.ShipListItem).GetMethod("LateUpdate",
                    BindingFlags.Instance | BindingFlags.NonPublic) != null)
                throw new InvalidOperationException("ShipListItem still contains the ReUI per-frame disabled overlay writer.");
            if (typeof(ViewModel.FactionPanelViewModel).GetMethod("RefreshReUIPresentation",
                    BindingFlags.Instance | BindingFlags.NonPublic) != null)
                throw new InvalidOperationException("FactionPanel still invokes ReUI after its original visibility logic.");
        }

        private static void ValidateThemePaletteUi()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/SettingsScene.unity", OpenSceneMode.Single);
            Canvas canvas = FindCanvas(scene);
            if (canvas == null)
                throw new InvalidOperationException("SettingsScene contains no Canvas for the theme palette.");

            ApplyScopedRuntimeOnce();

            Transform launcher = FindByName(canvas.transform, "ReUI Theme Palette Button");
            Transform panel = FindByName(canvas.transform, "ReUI Theme Palette");
            Button launcherButton = launcher != null ? launcher.GetComponent<Button>() : null;
            if (launcherButton == null || panel == null)
                throw new InvalidOperationException("Settings theme palette launcher or panel was not created.");

            launcherButton.onClick.Invoke();
            if (!panel.gameObject.activeSelf ||
                panel.Find("Color Square")?.GetComponent<ReUIThemeColorSquareGraphic>() == null ||
                panel.Find("Color Square")?.GetComponent<ReUIThemeColorSquareInput>() == null ||
                panel.Find("Hue")?.GetComponent<Slider>() == null ||
                panel.Find("Hue")?.GetComponent<ReUIThemeHueStripGraphic>() == null)
                throw new InvalidOperationException("Settings theme palette controls are incomplete.");

            Color original = ReUIPalette.ThemeColor;
            bool hadCustomTheme = ReUIPalette.HasCustomThemeColor;
            Button preset = panel.Find("Preset 1")?.GetComponent<Button>();
            if (preset == null)
                throw new InvalidOperationException("Settings theme palette has no selectable preset.");
            preset.onClick.Invoke();
            if (Vector4.Distance(ReUIPalette.ThemeColor, original) < 0.001f)
                throw new InvalidOperationException("Settings theme palette selection did not update the active colour.");
            if (hadCustomTheme) ReUIPalette.SetThemeColor(original);
            else ReUIPalette.ResetThemeColor();
        }

        private static void ValidateStarMapIsUntouched()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/StarMapScene.unity", OpenSceneMode.Single);
            Canvas canvas = FindCanvas(scene);
            if (canvas == null)
                throw new InvalidOperationException("StarMapScene contains no Canvas.");

            string[] before = CaptureImageSignatures(canvas.transform, null);
            int motionBefore = canvas.GetComponentsInChildren<ReUIButtonMotion>(true).Length;
            int generatedBefore = canvas.GetComponentsInChildren<ReUIIconGraphic>(true).Length;

            ApplyScopedRuntimeOnce();

            string[] after = CaptureImageSignatures(canvas.transform, null);
            int motionAfter = canvas.GetComponentsInChildren<ReUIButtonMotion>(true).Length;
            int generatedAfter = canvas.GetComponentsInChildren<ReUIIconGraphic>(true).Length;
            if (!before.SequenceEqual(after) || motionBefore != motionAfter || generatedBefore != generatedAfter)
                throw new InvalidOperationException("Star map UI was modified by the scoped ReUI runtime.");
        }

        private static void ValidateShipEditorDeviceListOnly()
        {
            Scene scene = EditorSceneManager.OpenScene(
                "Assets/ModulesShared/ShipEditor/Scenes/ShipEditorScene.unity", OpenSceneMode.Single);
            Canvas canvas = FindCanvas(scene);
            if (canvas == null)
                throw new InvalidOperationException("ShipEditorScene contains no Canvas.");

            Transform window = FindByName(canvas.transform, "ShipEditorWindow");
            Transform buttons = window != null ? window.Find("Buttons") : null;
            Transform rightPanel = window != null ? window.Find("RightPanel") : null;
            Transform componentList = rightPanel != null ? rightPanel.Find("ComponentList") : null;
            Image componentSurface = componentList != null ? componentList.GetComponent<Image>() : null;
            if (buttons == null || rightPanel == null || componentSurface == null)
                throw new InvalidOperationException("Ship editor scoped validation targets were not found.");

            string[] topBefore = CaptureImageSignatures(buttons, null);
            string[] rightBefore = CaptureImageSignatures(rightPanel, image =>
            {
                if (image == componentSurface) return false;
                Button owner = image.GetComponentInParent<Button>();
                return owner == null || owner.targetGraphic != image;
            });

            ApplyScopedRuntimeOnce();

            string[] topAfter = CaptureImageSignatures(buttons, null);
            string[] rightAfter = CaptureImageSignatures(rightPanel, image =>
            {
                if (image == componentSurface) return false;
                Button owner = image.GetComponentInParent<Button>();
                return owner == null || owner.targetGraphic != image;
            });

            if (!topBefore.SequenceEqual(topAfter))
                throw new InvalidOperationException("Ship editor top buttons or their icons were modified.");
            if (!rightBefore.SequenceEqual(rightAfter))
                throw new InvalidOperationException("Ship editor right-side original icons were modified.");
            if (componentSurface.GetComponent<ReUIEffectMarker>() == null || componentSurface.color.a < 0.60f)
                throw new InvalidOperationException("Ship editor device list styling was not retained.");
        }

        private static void ValidateCombatHudOnly()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/CombatScene.unity", OpenSceneMode.Single);
            Canvas canvas = FindCanvas(scene);
            if (canvas == null)
                throw new InvalidOperationException("CombatScene contains no Canvas.");

            Transform combatMenu = FindPath(scene, "Canvas/CombatMenu");
            string[] menuBefore = combatMenu != null
                ? CaptureImageSignatures(combatMenu, null)
                : Array.Empty<string>();

            ApplyScopedRuntimeOnce();

            string[] menuAfter = combatMenu != null
                ? CaptureImageSignatures(combatMenu, null)
                : Array.Empty<string>();
            if (!menuBefore.SequenceEqual(menuAfter))
                throw new InvalidOperationException("Combat menu UI changed outside the retained HUD scope.");

            Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>(true);
            Graphic life = graphics.FirstOrDefault(item =>
                item != null && item.GetType().FullName == "Gui.Controls.ProgressBar" &&
                (item.name == "HitPoints" || item.name == "ArmorPoints"));
            Graphic energy = graphics.FirstOrDefault(item =>
                item != null && item.GetType().FullName == "Gui.Controls.ProgressBar" &&
                item.name == "EnergyPoints");
            if (life == null || energy == null)
                throw new InvalidOperationException("Combat life or energy progress bar was not found.");
            string lifeShader = life.material != null && life.material.shader != null
                ? life.material.shader.name
                : string.Empty;
            string energyShader = energy.material != null && energy.material.shader != null
                ? energy.material.shader.name
                : string.Empty;
            if (life.color.g <= life.color.b || energy.color.r < 0.95f || energy.color.g < 0.80f)
                throw new InvalidOperationException(
                    $"Combat life/energy continuous bar styling was not retained: " +
                    $"lifeColor={life.color}, lifeShader={lifeShader}, " +
                    $"energyColor={energy.color}, energyShader={energyShader}.");
        }

        private static void ApplyScopedRuntimeOnce()
        {
            GameObject host = new("Scoped ReUI Validation Host", typeof(ReUIBootstrap));
            try
            {
                host.GetComponent<ReUIBootstrap>().ApplyNow();
                Canvas.ForceUpdateCanvases();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        private static string[] CaptureImageSignatures(Transform root, Func<Image, bool> predicate)
        {
            if (root == null) return Array.Empty<string>();
            return root.GetComponentsInChildren<Image>(true)
                .Where(image => image != null && (predicate == null || predicate(image)))
                .OrderBy(image => image.GetInstanceID())
                .Select(image => string.Join("|",
                    image.GetInstanceID(),
                    image.sprite != null ? image.sprite.GetInstanceID() : 0,
                    image.material != null ? image.material.GetInstanceID() : 0,
                    image.color.r.ToString("R"), image.color.g.ToString("R"),
                    image.color.b.ToString("R"), image.color.a.ToString("R"),
                    image.enabled, image.gameObject.activeSelf))
                .ToArray();
        }

        private static void ValidateUniformEnabledButtonBrightness()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/CombatScene.unity", OpenSceneMode.Single);
            Canvas canvas = FindCanvas(scene);
            if (canvas == null) throw new InvalidOperationException("CombatScene contains no Canvas.");

            Transform menu = FindPath(scene, "Canvas/CombatMenu");
            if (menu == null) throw new InvalidOperationException("CombatMenu was not found.");
            menu.gameObject.SetActive(true);
            ReUICanvasStyler.Apply(canvas);
            Canvas.ForceUpdateCanvases();

            Button resume = FindPath(scene, "Canvas/CombatMenu/Panel/Panel/Resume")?.GetComponent<Button>();
            Button nextEnemy = FindPath(scene, "Canvas/CombatMenu/Panel/Panel/NextEnemy")?.GetComponent<Button>();
            Button settings = FindPath(scene, "Canvas/CombatMenu/Panel/Panel/Settings")?.GetComponent<Button>();
            if (resume == null || nextEnemy == null || settings == null)
                throw new InvalidOperationException("Combat menu buttons were not found.");

            ValidateEnabledBrightness(resume, "Resume");
            ValidateEnabledBrightness(settings, "Settings");

            nextEnemy.interactable = false;
            ReUIButtonMotion motion = nextEnemy.GetComponent<ReUIButtonMotion>();
            if (motion == null)
                throw new InvalidOperationException("NextEnemy has no runtime visual-state synchronizer.");
            motion.RefreshVisualState();

            Text disabledLabel = nextEnemy.GetComponentInChildren<Text>(true);
            ReUIIconGraphic disabledIcon = nextEnemy.GetComponentsInChildren<ReUIIconGraphic>(true)
                .FirstOrDefault(item => item != null && item.gameObject.activeSelf && item.enabled);
            if (disabledLabel == null || disabledIcon == null || disabledIcon.Kind != ReUIIconKind.NextEnemy ||
                disabledLabel.canvasRenderer.GetAlpha() < 0.99f ||
                disabledIcon.canvasRenderer.GetAlpha() < 0.99f ||
                disabledLabel.color.a < 0.99f)
                throw new InvalidOperationException("Disabled combat button content is not fully bright.");
        }

        private static void ValidateReUI13StableButtonsAndIcons()
        {
            Scene mainMenuScene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            Canvas mainCanvas = FindCanvas(mainMenuScene);
            if (mainCanvas == null)
                throw new InvalidOperationException("MainMenuScene contains no Canvas.");
            ReUICanvasStyler.Apply(mainCanvas);
            Canvas.ForceUpdateCanvases();

            Button quickBattle = FindByName(mainCanvas.transform, "Combat")?.GetComponent<Button>();
            if (quickBattle == null)
                throw new InvalidOperationException("Quick Battle button was not found.");
            ValidateStateInvariantButton(quickBattle, "Quick Battle");
            ReUIIconGraphic quickIcon = GetActiveGeneratedIcon(quickBattle);
            if (quickIcon == null || quickIcon.Kind != ReUIIconKind.QuickBattle)
                throw new InvalidOperationException("Quick Battle does not use the dedicated lightning icon.");

            Scene combatScene = EditorSceneManager.OpenScene("Assets/Scenes/CombatScene.unity", OpenSceneMode.Single);
            Canvas combatCanvas = FindCanvas(combatScene);
            Transform combatMenu = FindPath(combatScene, "Canvas/CombatMenu");
            if (combatCanvas == null || combatMenu == null)
                throw new InvalidOperationException("Combat menu validation target was not found.");
            combatMenu.gameObject.SetActive(true);
            ReUICanvasStyler.Apply(combatCanvas);
            Canvas.ForceUpdateCanvases();

            Button nextEnemy = FindPath(combatScene, "Canvas/CombatMenu/Panel/Panel/NextEnemy")?.GetComponent<Button>();
            if (nextEnemy == null)
                throw new InvalidOperationException("Next Enemy button was not found.");
            ValidateStateInvariantButton(nextEnemy, "Next Enemy");
            ReUIIconGraphic nextIcon = GetActiveGeneratedIcon(nextEnemy);
            if (nextIcon == null || nextIcon.Kind != ReUIIconKind.NextEnemy)
                throw new InvalidOperationException("Next Enemy does not use the dedicated next-marker icon.");

            ValidateBlinkSuppressionRuntime();
        }

        private static void ValidateBlinkSuppressionRuntime()
        {
            MethodInfo blinkStart = typeof(Gui.ImageBlink).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo blinkLateUpdate = typeof(Gui.ImageBlink).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo motionLateUpdate = typeof(ReUIButtonMotion).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            if (blinkStart == null || blinkLateUpdate == null || motionLateUpdate != null)
                throw new InvalidOperationException("Button brightness suppression runtime contract is invalid.");

            GameObject probe = new("ReUI13 Button Blink Probe", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Button), typeof(ReUIButtonMotion));
            GameObject child = new("Focus", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Gui.ImageBlink));
            try
            {
                child.transform.SetParent(probe.transform, false);
                Button button = probe.GetComponent<Button>();
                button.targetGraphic = probe.GetComponent<Image>();
                Gui.ImageBlink blink = child.GetComponent<Gui.ImageBlink>();
                Image childImage = child.GetComponent<Image>();

                blinkStart.Invoke(blink, null);
                childImage.canvasRenderer.SetAlpha(0.5f);
                blinkLateUpdate.Invoke(blink, null);
                if (childImage.canvasRenderer.GetAlpha() < 0.99f)
                    throw new InvalidOperationException("Legacy ImageBlink still changes brightness over time.");

                ReUIButtonMotion motion = probe.GetComponent<ReUIButtonMotion>();
                motion.RefreshVisualState();
                if (blink.enabled)
                    throw new InvalidOperationException("ReUIButtonMotion did not disable a nested ImageBlink.");

                childImage.canvasRenderer.SetAlpha(0.5f);
                motion.RefreshVisualState();
                if (childImage.canvasRenderer.GetAlpha() < 0.99f)
                    throw new InvalidOperationException("Explicit button refresh did not restore stable brightness.");

                DefaultExecutionOrder order = typeof(ReUIButtonMotion)
                    .GetCustomAttribute<DefaultExecutionOrder>();
                if (order == null || order.order < 10000)
                    throw new InvalidOperationException("ReUIButtonMotion does not execute after legacy blink scripts.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(probe);
            }
        }

        private static void ValidateReUI13SophonActivationAndManualTitans()
        {
            var stats = new GameDatabase.DataModel.DeviceStats
            {
                DeviceClass = GameDatabase.Enums.DeviceClass.SophonJammer,
                Cooldown = 90f,
                Lifetime = 60f,
                EnergyConsumption = 1000f,
            };
            var device = new Combat.Component.Systems.Devices.SophonJammerDevice(null, stats, 3, null);
            device.RequestActivation();
            FieldInfo requestField = typeof(Combat.Component.Systems.Devices.SophonJammerDevice)
                .GetField("_activationRequested", BindingFlags.Instance | BindingFlags.NonPublic);
            if (requestField == null || !(bool)requestField.GetValue(device))
                throw new InvalidOperationException("Sophon runtime activation request was not latched.");

            string assets = Application.dataPath;
            string controlsPanel = File.ReadAllText(Path.Combine(assets,
                "Scripts/Gui/Combat/ShipControlsPanel.cs"));
            if (!controlsPanel.Contains("TryRequestSophonActivation") ||
                !controlsPanel.Contains("RequestActivation()") ||
                controlsPanel.Contains("ReleasePulsedSystemAfterPhysics"))
                throw new InvalidOperationException("Touch controls still use the fragile FixedUpdate coroutine pulse.");

            string shipSystems = File.ReadAllText(Path.Combine(assets,
                "Modules/BattleSimulator/Scripts/Combat/Component/Systems/ShipSystems.cs"));
            string radarStatus = File.ReadAllText(Path.Combine(assets,
                "Modules/BattleSimulator/Scripts/Combat/Unit/Ship/Effects/RadarStatusEffect.cs"));
            if (!shipSystems.Contains("SuppressJammedWeapons") ||
                !shipSystems.Contains("RadarStatus.IsJammed(_ship)") ||
                !shipSystems.Contains("weapon.Platform.ActiveTarget = null") ||
                !radarStatus.Contains("ClearTargeting(ship)") ||
                !radarStatus.Contains("ship.Effects == null"))
                throw new InvalidOperationException("Radar jamming is missing its weapon suppression or null-safe application path.");

            var database = new GameDatabase.Database();
            database.LoadDefault();
            int[] titanBuildIds = { 94008, 1145140 };
            for (int i = 0; i < titanBuildIds.Length; i++)
            {
                var build = database.GetShipBuild(
                    new ItemId<GameDatabase.DataModel.ShipBuild>(titanBuildIds[i]));
                if (build == null || build == GameDatabase.DataModel.ShipBuild.DefaultValue ||
                    build.Ship.SizeClass != GameDatabase.Enums.SizeClass.TitanP ||
                    !GameStateMachine.States.QuickCombatState.IsConfigurableQuickBattleBuild(build) ||
                    GameStateMachine.States.QuickCombatState.IsQuickBattleBuild(build))
                    throw new InvalidOperationException(
                        "Manual quick-battle Titan is missing or leaked into the random pool: " + titanBuildIds[i]);
            }
        }

        private static ReUIIconGraphic GetActiveGeneratedIcon(Button button)
        {
            return button.GetComponentsInChildren<ReUIIconGraphic>(true)
                .FirstOrDefault(item => item != null && item.gameObject.activeSelf && item.enabled &&
                                        item.Kind != ReUIIconKind.None);
        }

        private static void ValidateReUI12StableButtonsAndIcons()
        {
            Scene mainMenuScene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            Canvas mainCanvas = FindCanvas(mainMenuScene);
            if (mainCanvas == null)
                throw new InvalidOperationException("MainMenuScene contains no Canvas.");
            ReUICanvasStyler.Apply(mainCanvas);
            Canvas.ForceUpdateCanvases();

            Button quickBattle = FindByName(mainCanvas.transform, "Combat")?.GetComponent<Button>();
            if (quickBattle == null)
                throw new InvalidOperationException("Quick Battle button was not found.");
            ValidateStateInvariantButton(quickBattle, "Quick Battle");
            if (HasActiveGeneratedIcon(quickBattle))
                throw new InvalidOperationException("Quick Battle still contains the crossed Battle/X glyph.");

            Scene combatScene = EditorSceneManager.OpenScene("Assets/Scenes/CombatScene.unity", OpenSceneMode.Single);
            Canvas combatCanvas = FindCanvas(combatScene);
            Transform combatMenu = FindPath(combatScene, "Canvas/CombatMenu");
            if (combatCanvas == null || combatMenu == null)
                throw new InvalidOperationException("Combat menu validation target was not found.");
            combatMenu.gameObject.SetActive(true);
            ReUICanvasStyler.Apply(combatCanvas);
            Canvas.ForceUpdateCanvases();

            Button nextEnemy = FindPath(combatScene, "Canvas/CombatMenu/Panel/Panel/NextEnemy")?.GetComponent<Button>();
            Button settings = FindPath(combatScene, "Canvas/CombatMenu/Panel/Panel/Settings")?.GetComponent<Button>();
            if (nextEnemy == null || settings == null)
                throw new InvalidOperationException("Combat menu state-invariance targets were not found.");
            ValidateStateInvariantButton(nextEnemy, "Next Enemy");
            ValidateStateInvariantButton(settings, "Combat Settings");
            if (HasActiveGeneratedIcon(nextEnemy))
                throw new InvalidOperationException("Next Enemy still contains the crossed Battle/X glyph.");
        }

        private static void ValidateStateInvariantButton(Button button, string description)
        {
            ColorBlock colors = button.colors;
            if (!Approximately(colors.normalColor, Color.white) ||
                !Approximately(colors.highlightedColor, Color.white) ||
                !Approximately(colors.selectedColor, Color.white) ||
                !Approximately(colors.pressedColor, Color.white) ||
                !Approximately(colors.disabledColor, Color.white) ||
                button.transition != Selectable.Transition.None)
                throw new InvalidOperationException(description + " still changes tint between interaction states.");

            bool originalInteractable = button.interactable;
            button.interactable = !originalInteractable;
            button.GetComponent<ReUIButtonMotion>()?.RefreshVisualState();
            Graphic[] graphics = button.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
                if (graphics[i] != null && graphics[i].canvasRenderer.GetAlpha() < 0.99f)
                    throw new InvalidOperationException(description + " contains a state-dimmed graphic: " + graphics[i].name);
            button.interactable = originalInteractable;
        }

        private static bool HasActiveGeneratedIcon(Button button)
        {
            return button.GetComponentsInChildren<ReUIIconGraphic>(true)
                .Any(item => item != null && item.gameObject.activeSelf && item.enabled && item.Kind != ReUIIconKind.None);
        }

        private static void ValidateReUI12SophonPulseAndManualTitans()
        {
            string assets = Application.dataPath;
            string controlsPanel = File.ReadAllText(Path.Combine(assets,
                "Scripts/Gui/Combat/ShipControlsPanel.cs"));
            if (!controlsPanel.Contains("IsPulseSystem(id)") ||
                !controlsPanel.Contains("SophonJammerDevice") ||
                !controlsPanel.Contains("new WaitForFixedUpdate()") ||
                !controlsPanel.Contains("ReleasePulsedSystemAfterPhysics"))
                throw new InvalidOperationException("Sophon input is not retained through a physics update.");

            string sophonDeviceSource = File.ReadAllText(Path.Combine(assets,
                "Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/SophonJammerDevice.cs"));
            if (!sophonDeviceSource.Contains("RadarStatus.ApplyJammed") ||
                !sophonDeviceSource.Contains("RadarStatus.RevealStealthFor"))
                throw new InvalidOperationException("Sophon no longer applies radar jamming and stealth reveal.");

            var database = new GameDatabase.Database();
            database.LoadDefault();
            var sophon = database.GetDevice(new ItemId<GameDatabase.DataModel.Device>(906));
            if (sophon == null || sophon == GameDatabase.DataModel.Device.DefaultValue ||
                sophon.Stats.DeviceClass != GameDatabase.Enums.DeviceClass.SophonJammer ||
                sophon.Stats.Lifetime < 59.9f)
                throw new InvalidOperationException("Sophon device 906 is missing or has invalid runtime data.");

            int[] titanBuildIds = { 94008, 1145140 };
            for (int i = 0; i < titanBuildIds.Length; i++)
            {
                var build = database.GetShipBuild(
                    new ItemId<GameDatabase.DataModel.ShipBuild>(titanBuildIds[i]));
                if (build == null || build == GameDatabase.DataModel.ShipBuild.DefaultValue ||
                    build.Ship.SizeClass != GameDatabase.Enums.SizeClass.TitanP ||
                    !GameStateMachine.States.QuickCombatState.IsConfigurableQuickBattleBuild(build) ||
                    GameStateMachine.States.QuickCombatState.IsQuickBattleBuild(build))
                    throw new InvalidOperationException(
                        "Manual quick-battle Titan is missing or leaked into the random pool: " + titanBuildIds[i]);
            }
        }

        private static void ValidateEnabledBrightness(Button button, string description)
        {
            button.interactable = true;
            ReUIButtonMotion motion = button.GetComponent<ReUIButtonMotion>();
            if (motion == null)
                throw new InvalidOperationException(description + " has no runtime visual-state synchronizer.");
            motion.RefreshVisualState();

            ColorBlock colors = button.colors;
            if (!Approximately(colors.normalColor, colors.highlightedColor) ||
                !Approximately(colors.normalColor, colors.selectedColor))
                throw new InvalidOperationException(description + " changes brightness between enabled states.");

            Image surface = button.targetGraphic as Image;
            if (surface == null || surface.color.a <= 0.01f)
                throw new InvalidOperationException(description + " lost its glass surface.");
            if (surface.transform.parent == button.transform)
            {
                LayoutElement layout = surface.GetComponent<LayoutElement>();
                if (surface.transform.GetSiblingIndex() != 0 || layout == null || !layout.ignoreLayout)
                    throw new InvalidOperationException(description + " glass surface still renders above or participates in content layout.");
            }

            Text label = button.GetComponentInChildren<Text>(true);
            ReUIIconGraphic icon = button.GetComponentInChildren<ReUIIconGraphic>(true);
            if (label == null || icon == null ||
                label.canvasRenderer.GetAlpha() < 0.99f || icon.canvasRenderer.GetAlpha() < 0.99f)
                throw new InvalidOperationException(description + " enabled content is not fully bright.");
        }

        private static void ValidateShipEditorUndoAndCloseIcons()
        {
            Scene scene = EditorSceneManager.OpenScene(
                "Assets/ModulesShared/ShipEditor/Scenes/ShipEditorScene.unity", OpenSceneMode.Single);
            Canvas canvas = FindCanvas(scene);
            if (canvas == null)
                throw new InvalidOperationException("ShipEditorScene contains no Canvas.");

            ReUICanvasStyler.Apply(canvas);
            Canvas.ForceUpdateCanvases();

            Transform undoTransform = FindByName(canvas.transform, "UndoButton");
            Transform backTransform = FindByName(canvas.transform, "BackButton");
            Transform clearTransform = FindByName(canvas.transform, "ClearButton");
            if (undoTransform == null || backTransform == null || clearTransform == null)
                throw new InvalidOperationException("Ship editor navigation buttons were not found.");

            ReUIIconGraphic undo = undoTransform.GetComponentInChildren<ReUIIconGraphic>(true);
            ReUIIconGraphic back = backTransform.GetComponentInChildren<ReUIIconGraphic>(true);
            if (undo == null || undo.Kind != ReUIIconKind.Undo)
                throw new InvalidOperationException("UndoButton does not use the dedicated 180-degree arrow.");
            if (back == null || back.Kind != ReUIIconKind.Back)
                throw new InvalidOperationException("BackButton no longer uses the straight return arrow.");

            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                ReUIIconGraphic[] icons = buttons[i].GetComponentsInChildren<ReUIIconGraphic>(true);
                for (int j = 0; j < icons.Length; j++)
                {
                    if (icons[j] == null || !icons[j].gameObject.activeSelf || icons[j].Kind != ReUIIconKind.Close)
                        continue;
                    string name = buttons[i].name.ToLowerInvariant();
                    if (!name.Contains("close") && !name.Contains("exit") && !name.Contains("cancel"))
                        throw new InvalidOperationException("Accidental Close/X overlay remains on " + buttons[i].name + ".");
                }
            }

            ReUIProhibitGraphic prohibit = clearTransform.GetComponentInChildren<ReUIProhibitGraphic>(true);
            if (prohibit == null || !prohibit.gameObject.activeSelf)
                throw new InvalidOperationException("ClearButton lost its intended single-slash prohibit marker.");
        }

        private static void ValidateReUI11GameplayConfiguration()
        {
            ResourceLocator locatorPrefab = Resources.Load<ResourceLocator>("ResourceLocator");
            if (locatorPrefab == null)
                throw new InvalidOperationException("ResourceLocator prefab was not found.");

            ResourceLocator locator = UnityEngine.Object.Instantiate(locatorPrefab);
            try
            {
                Sprite foil = locator.GetSprite(new SpriteId(
                    "dual_vector_foil_projectile", SpriteId.Type.Ammunition));
                if (foil == null || foil.name != "dual_vector_foil_projectile_Paper" ||
                    foil.rect.width / Mathf.Max(1f, foil.rect.height) < 2.4f)
                    throw new InvalidOperationException("Dual-vector foil projectile is not the generated white paper rectangle.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(locator.gameObject);
            }

            string assets = Application.dataPath;
            string foilJson = File.ReadAllText(Path.Combine(assets,
                "Modules/Database/Resources/Database/Ammunition/Bullets/DualVectorFoil.json"));
            if (!foilJson.Contains("\"Size\":0.38") || !foilJson.Contains("\"Margins\":0.06"))
                throw new InvalidOperationException("Dual-vector foil projectile size was not reduced.");

            string bulletFactory = File.ReadAllText(Path.Combine(assets,
                "Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs"));
            if (!bulletFactory.Contains("IsStellarHydrogenBomb(_stats)") ||
                !bulletFactory.Contains("_scene, 30f, 0.20f, 1f") ||
                !bulletFactory.Contains("AmmunitionClassObsolete.AcidRocket") ||
                !bulletFactory.Contains("Combat/Bullets/SatelliteRocket"))
                throw new InvalidOperationException(
                    "Stellar hydrogen bomb is missing its 30-second EMP action or stable legacy-ammunition identification.");

            string empAction = File.ReadAllText(Path.Combine(assets,
                "Modules/BattleSimulator/Scripts/Combat/Unit/Bullet/Action/CreateEmpAction.cs"));
            if (!empAction.Contains("ship == playerShip") ||
                !empAction.Contains("RadarStatus.ApplyJammed") ||
                !empAction.Contains("ship.Stats.Energy.MaxValue * _initialEnergyDrainFraction"))
                throw new InvalidOperationException("Battlewide EMP no longer excludes only the current player ship or no longer reuses EMP semantics.");

            string specialRules = File.ReadAllText(Path.Combine(assets,
                "Modules/BattleSimulator/Scripts/Combat/AI/Strategy/Factories/SpecialRules.cs"));
            string sophonDevice = File.ReadAllText(Path.Combine(assets,
                "Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/SophonJammerDevice.cs"));
            if (!specialRules.Contains("device is SophonJammerDevice") ||
                !sophonDevice.Contains("_ship.Type.Side == UnitSide.Player"))
                throw new InvalidOperationException("Sophon does not support both AI cooldown activation and player press-edge activation.");
        }

        public static void ValidateAllEnabledScenesSmoke()
        {
            string[] scenePaths = EditorBuildSettings.scenes
                .Where(item => item.enabled)
                .Select(item => item.path)
                .ToArray();

            int sceneCount = 0;
            int canvasCount = 0;
            for (int i = 0; i < scenePaths.Length; i++)
            {
                Scene scene = EditorSceneManager.OpenScene(scenePaths[i], OpenSceneMode.Single);
                if (!scene.IsValid() || !scene.isLoaded)
                    throw new InvalidOperationException("Scene could not be opened: " + scenePaths[i]);

                Canvas[] canvases = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Canvas>(true))
                    .ToArray();
                for (int j = 0; j < canvases.Length; j++)
                    ReUICanvasStyler.Apply(canvases[j]);

                sceneCount++;
                canvasCount += canvases.Length;
                Debug.Log($"[ReUI Smoke] scene={scene.name}, canvases={canvases.Length}");
            }

            Debug.Log($"[ReUI Smoke] completed scenes={sceneCount}, canvases={canvasCount}");
        }

        private static void ValidateSettingsPresentation()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/SettingsScene.unity", OpenSceneMode.Single);
            Canvas canvas = FindCanvas(scene);
            if (canvas == null) throw new InvalidOperationException("SettingsScene contains no Canvas.");

            ReUICanvasStyler.Apply(canvas);

            Transform exitTransform = FindPath(scene, "Canvas/Settings/Buttons/Exit");
            Button exit = exitTransform != null ? exitTransform.GetComponent<Button>() : null;
            if (exit == null) throw new InvalidOperationException("Settings exit button was not found.");

            ReUIIconGraphic[] generated = exit.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < generated.Length; i++)
            {
                if (generated[i] != null && generated[i].enabled && generated[i].gameObject.activeSelf)
                    throw new InvalidOperationException("Settings exit still contains an active generated close icon.");
            }

            Image originalIcon = exit.transform.Find("Icon")?.GetComponent<Image>();
            if (originalIcon == null || !originalIcon.enabled || originalIcon.sprite == null ||
                originalIcon.sprite.name != "icon_exit")
                throw new InvalidOperationException("Settings exit did not restore the original icon_exit sprite.");
            if (originalIcon.color.b <= originalIcon.color.r)
                throw new InvalidOperationException("Settings exit icon is not using the cyan navigation color.");

            Image target = exit.targetGraphic as Image;
            if (target == null || target.color.a > 0.08f)
                throw new InvalidOperationException("Settings exit background is not visually consistent with navigation tabs.");

            string[] navigation = { "General", "Combat", "Controls", "Account", "LoadSave", "Database" };
            for (int i = 0; i < navigation.Length; i++)
            {
                Toggle toggle = FindPath(scene, "Canvas/Settings/Buttons/" + navigation[i])?.GetComponent<Toggle>();
                Image icon = toggle != null ? toggle.transform.Find("Icon")?.GetComponent<Image>() : null;
                if (toggle == null || icon == null || !icon.enabled || icon.color.a < 0.99f ||
                    Mathf.Abs(icon.rectTransform.sizeDelta.x - 64f) > 0.5f)
                    throw new InvalidOperationException("Settings navigation icon is dim or incorrectly sized: " + navigation[i]);
            }

            RectTransform template = FindRect(scene, "EnemyTransmissions");
            if (template == null)
                throw new InvalidOperationException("EnemyTransmissions template was not found.");
            GameObject selector = UnityEngine.Object.Instantiate(template.gameObject, template.parent);
            selector.name = "CombatMapSize";
            Toggle selectorToggle = selector.GetComponentInChildren<Toggle>(true);
            if (selectorToggle == null)
                throw new InvalidOperationException("CombatMapSize test selector contains no Toggle.");
            Button selectorButton = selector.GetComponent<Button>();
            if (selectorButton == null) selectorButton = selector.AddComponent<Button>();
            selectorButton.targetGraphic = selectorToggle.targetGraphic;
            selectorToggle.enabled = false;

            ReUICanvasStyler.Apply(canvas);

            Image mapBackground = selectorToggle.targetGraphic as Image;
            Image mapMarker = selectorToggle.graphic as Image;
            if (mapBackground == null || mapMarker == null || mapBackground.color.a > 0.08f ||
                Mathf.Abs(mapMarker.rectTransform.sizeDelta.x - 34f) > 0.5f || mapMarker.color.a < 0.90f)
                throw new InvalidOperationException("CombatMapSize selector does not match the standard settings-toggle style.");

            UnityEngine.Object.DestroyImmediate(selector);
        }

        private static void ValidateStarMapPresentation()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/StarMapScene.unity", OpenSceneMode.Single);
            Canvas canvas = FindCanvas(scene);
            if (canvas == null) throw new InvalidOperationException("StarMapScene contains no Canvas.");

            Transform buttons = FindPath(scene, "Canvas/GameMenu/Buttons");
            if (buttons == null) throw new InvalidOperationException("Star-map button container was not found.");
            Button relation = CreateValidationShortcut(buttons, "Preview5RelationsButton", true);
            Button captain = CreateValidationCaptainShortcut(buttons);

            Transform arenaFight = FindPath(scene, "Canvas/Panels/ArenaFight");
            if (arenaFight == null) throw new InvalidOperationException("ArenaFight runtime object was not found.");
            arenaFight.gameObject.SetActive(true);

            ReUICanvasStyler.Apply(canvas);
            Canvas.ForceUpdateCanvases();
            RectTransform buttonsRect = buttons as RectTransform;
            if (buttonsRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsRect);
            RectTransform arenaButtonsRect = arenaFight.Find("Buttons") as RectTransform;
            if (arenaButtonsRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(arenaButtonsRect);
            // A second pass simulates the periodic runtime scan after layout and
            // themed controls have completed their own lifecycle work.
            ReUICanvasStyler.Apply(canvas);
            Canvas.ForceUpdateCanvases();

            Transform shopFilter = FindPath(scene, "Canvas/GameMenu/Filters/Shop");
            if (shopFilter == null || !shopFilter.gameObject.activeSelf)
                throw new InvalidOperationException("Functional Shop filter was disabled or removed.");

            Transform buyButton = FindPath(scene, "Canvas/GameMenu/BuyButton");
            if (buyButton == null)
                buyButton = FindRect(scene, "BuyButton");
            if (buyButton == null || buyButton.gameObject.activeSelf)
                throw new InvalidOperationException("Premium-currency BuyButton was not hidden.");

            Button cargo = FindPath(scene, "Canvas/GameMenu/Buttons/CargoHold")?.GetComponent<Button>();
            ValidateBottomIcon(cargo, ReUIIconKind.Equipment, 72f, "CargoHold");
            ValidateBottomIcon(relation, ReUIIconKind.Faction, 72f, "faction");
            ValidateCaptainShortcut(captain);

            string[] fixedButtons = { "Fleet", "Skills", "Quests", "Research", "CargoHold", "Exit" };
            ReUIIconKind[] fixedKinds =
            {
                ReUIIconKind.Fleet,
                ReUIIconKind.Skills,
                ReUIIconKind.Missions,
                ReUIIconKind.Technology,
                ReUIIconKind.Equipment,
                ReUIIconKind.Close,
            };
            for (int i = 0; i < fixedButtons.Length; i++)
            {
                Button fixedButton = FindPath(scene, "Canvas/GameMenu/Buttons/" + fixedButtons[i])?.GetComponent<Button>();
                ValidateBottomIcon(fixedButton, fixedKinds[i], 72f, fixedButtons[i]);
            }

            Transform fightTransform = FindPath(scene, "Canvas/Panels/ArenaFight/Buttons/FightButton");
            Button fight = fightTransform != null ? fightTransform.GetComponent<Button>() : null;
            Image fightSurface = fight != null
                ? fight.transform.Find("ReUI Arena Surface")?.GetComponent<Image>()
                : null;
            ReUIFightIconGraphic fightIcon = fight != null
                ? fight.transform.Find("ReUI Fight Emblem")?.GetComponent<ReUIFightIconGraphic>()
                : null;
            RectTransform fightRect = fightTransform as RectTransform;
            Text fightLabel = fight != null ? fight.transform.Find("ReUI Arena Label")?.GetComponent<Text>() : null;
            LayoutElement fightLayout = fight != null ? fight.GetComponent<LayoutElement>() : null;
            if (fight == null || fightSurface == null || fight.targetGraphic != fightSurface ||
                fightSurface.GetType() != typeof(Image) || !fightSurface.enabled ||
                !fightSurface.gameObject.activeInHierarchy || fightSurface.canvasRenderer.GetAlpha() < 0.99f ||
                GetEffectiveCanvasGroupAlpha(fightSurface.transform) < 0.99f || fightSurface.maskable ||
                fightSurface.color.a < 0.04f ||
                fightSurface.color.a > 0.14f ||
                !HasSingleBorder(fightSurface))
                throw new InvalidOperationException("Arena fight Surface is not independently visible at runtime.");
            if (fightIcon == null || !fightIcon.enabled || !fightIcon.gameObject.activeInHierarchy ||
                fightIcon.canvasRenderer.GetAlpha() < 0.99f || fightIcon.color.a < 0.99f ||
                Mathf.Abs(fightIcon.rectTransform.rect.width - 124f) > 0.5f || fightIcon.maskable ||
                HasEnabledOutline(fightIcon))
                throw new InvalidOperationException("Dedicated arena fight emblem is not visible at the intended runtime size.");
            if (fightLabel == null || !fightLabel.enabled || !fightLabel.gameObject.activeInHierarchy ||
                fightLabel.text != "战斗" || fightLabel.color.a < 0.99f ||
                fightLabel.canvasRenderer.GetAlpha() < 0.99f || fightLabel.maskable ||
                HasEnabledOutline(fightLabel))
                throw new InvalidOperationException("Arena fight Label is not independently visible.");
            if (fightRect == null || Mathf.Abs(fightRect.anchoredPosition.x + 55f) > 0.5f ||
                Mathf.Abs(fightRect.rect.width - 176f) > 0.5f || fightLayout == null || !fightLayout.ignoreLayout)
                throw new InvalidOperationException("Arena fight button layout is still controlled by the legacy LayoutGroup.");
            if (arenaButtonsRect == null || arenaButtonsRect.GetComponents<LayoutGroup>().Any(item => item.enabled))
                throw new InvalidOperationException("Arena action container still has an enabled legacy LayoutGroup.");
            if (!HasPersistentMethod(fight, "OkButtonClicked"))
                throw new InvalidOperationException("Arena fight button lost its original OkButtonClicked event.");

            Transform cancelTransform = FindPath(scene, "Canvas/Panels/ArenaFight/Buttons/CancelButton");
            Button cancel = cancelTransform != null ? cancelTransform.GetComponent<Button>() : null;
            Image cancelSurface = cancel != null
                ? cancel.transform.Find("ReUI Arena Surface")?.GetComponent<Image>()
                : null;
            Text cancelLabel = cancel != null ? cancel.transform.Find("ReUI Arena Label")?.GetComponent<Text>() : null;
            if (cancel == null || cancelSurface == null || cancel.targetGraphic != cancelSurface ||
                !cancelSurface.gameObject.activeInHierarchy || cancelLabel == null || cancelLabel.text != "取消" ||
                !HasPersistentMethod(cancel, "Close"))
                throw new InvalidOperationException("Arena cancel button style or Close event regressed.");
        }

        private static void ValidateFactionPanelPresentation()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject canvasObject = new("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            Canvas canvas = canvasObject.GetComponent<Canvas>();

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Gui/StarMapScene/FactionPanel.prefab");
            if (prefab == null) throw new InvalidOperationException("FactionPanel prefab was not found.");
            GameObject panel = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (panel == null) throw new InvalidOperationException("FactionPanel prefab could not be instantiated.");
            panel.transform.SetParent(canvas.transform, false);
            panel.SetActive(true);

            ReUICanvasStyler.Apply(canvas);

            string[] facilities = { "Store", "Factory", "Shipyard" };
            for (int i = 0; i < facilities.Length; i++)
            {
                Transform target = FindByName(panel.transform, facilities[i]);
                Button button = target != null ? target.GetComponent<Button>() : null;
                Image surface = button != null ? button.targetGraphic as Image : null;
                Text label = button != null ? button.GetComponentInChildren<Text>(true) : null;
                Image icon = null;
                if (button != null)
                {
                    Image[] images = button.GetComponentsInChildren<Image>(true);
                    for (int imageIndex = 0; imageIndex < images.Length; imageIndex++)
                    {
                        Image candidate = images[imageIndex];
                        if (candidate == null || candidate == surface) continue;
                        string candidateName = candidate.name.ToLowerInvariant();
                        string parentName = candidate.transform.parent != null
                            ? candidate.transform.parent.name.ToLowerInvariant()
                            : string.Empty;
                        if (candidateName == "icon" || candidateName == "image" || parentName == "left")
                        {
                            icon = candidate;
                            break;
                        }
                    }
                }
                if (button != null)
                {
                    button.interactable = false;
                    button.GetComponent<ReUIButtonMotion>()?.RefreshVisualState();
                }
                if (button == null || surface == null || surface.color.a < 0.10f || surface.color.a > 0.16f ||
                    label == null || label.color.a < 0.99f ||
                    label.canvasRenderer.GetAlpha() < 0.99f ||
                    icon == null || !icon.enabled || icon.color.a < 0.99f || icon.canvasRenderer.GetAlpha() < 0.99f)
                    throw new InvalidOperationException(
                        $"Faction facility content is not fully readable: {facilities[i]}; " +
                        $"surfaceA={surface?.color.a}, label={label?.name}, labelA={label?.color.a}, " +
                        $"labelRenderer={label?.canvasRenderer.GetAlpha()}, icon={icon?.name}, " +
                        $"iconEnabled={icon?.enabled}, iconA={icon?.color.a}, iconRenderer={icon?.canvasRenderer.GetAlpha()}");
            }

            UnityEngine.Object.DestroyImmediate(canvasObject);
        }

        private static void ValidateReUI14SophonProjectilePipeline()
        {
            MethodInfo statsFactory = typeof(Combat.Factory.DeviceFactory).GetMethod(
                "CreateSophonEmpPulseStats", BindingFlags.Static | BindingFlags.NonPublic);
            if (statsFactory == null)
                throw new InvalidOperationException("Sophon EMP projectile stats factory was not found.");

            var stats = (GameDatabase.DataModel.AmmunitionObsoleteStats)statsFactory.Invoke(null, null);
            if (stats.AmmunitionClass != GameDatabase.Enums.AmmunitionClassObsolete.Bomb ||
                stats.BulletPrefab.ToString() != "Combat/Bullets/Empty" ||
                Mathf.Abs(stats.Range - 0.5f) > 0.001f ||
                Mathf.Abs(stats.Velocity - 20f) > 0.001f ||
                Mathf.Abs(stats.LifeTime - 0.05f) > 0.001f ||
                Mathf.Abs(stats.Size - 0.03f) > 0.001f ||
                stats.Color.A > 0.001f)
                throw new InvalidOperationException("Sophon EMP projectile is not invisible, short-ranged and fast-expiring.");

            var dummySource = (Combat.Component.Ship.IShip)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(Combat.Component.Ship.Ship));
            var factory = new Combat.Factory.BulletFactoryObsolete(
                stats, null, null, null, null, dummySource, false,
                dummySource, 60f, 0.20f, 1f);
            Type factoryType = typeof(Combat.Factory.BulletFactoryObsolete);
            object configuredSource = factoryType.GetField("_enemyFleetEmpSource",
                BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(factory);
            float configuredDuration = (float)(factoryType.GetField("_enemyFleetEmpDuration",
                BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(factory) ?? 0f);
            float configuredFraction = (float)(factoryType.GetField("_enemyFleetEmpInitialEnergyDrainFraction",
                BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(factory) ?? 0f);
            float configuredDrain = (float)(factoryType.GetField("_enemyFleetEmpEnergyDrainPerSecond",
                BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(factory) ?? 0f);
            if (!ReferenceEquals(configuredSource, dummySource) ||
                Mathf.Abs(configuredDuration - 60f) > 0.001f ||
                Mathf.Abs(configuredFraction - 0.20f) > 0.001f ||
                Mathf.Abs(configuredDrain - 1f) > 0.001f)
                throw new InvalidOperationException("Legacy bullet factory did not retain the Sophon enemy-fleet EMP configuration.");

            string sophonSource = File.ReadAllText(Path.Combine(Application.dataPath,
                "Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/SophonJammerDevice.cs"));
            string bulletFactorySource = File.ReadAllText(Path.Combine(Application.dataPath,
                "Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs"));
            string deviceFactorySource = File.ReadAllText(Path.Combine(Application.dataPath,
                "Modules/BattleSimulator/Scripts/Combat/Factory/Systems/DeviceFactory.cs"));
            if (!sophonSource.Contains("_bulletFactory.Create(this") ||
                sophonSource.Contains("RadarStatus.ApplyJammed") ||
                !bulletFactorySource.Contains("new CreateEnemyFleetEmpAction") ||
                !bulletFactorySource.Contains("explodeCondition") ||
                !deviceFactorySource.Contains("stats.Lifetime") ||
                !deviceFactorySource.Contains("0.20f") ||
                !deviceFactorySource.Contains("1f"))
                throw new InvalidOperationException("Sophon still bypasses the legacy projectile detonation pipeline.");
        }

        private static void ValidateFactionVisibilityPreservation()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject canvasObject = new("Canvas", typeof(RectTransform), typeof(Canvas));
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            GameObject panel = new("FactionPanel", typeof(RectTransform));
            panel.transform.SetParent(canvas.transform, false);

            Button store = CreateValidationButton(panel.transform, "Store", true);
            Button capture = CreateValidationButton(panel.transform, "Capture", false);
            Button transfer = CreateValidationButton(panel.transform, "PeacefulTransferButton", false);
            Button joint = CreateValidationButton(panel.transform, "Preview5JointAttackButton", false);

            ReUICanvasStyler.Apply(canvas);

            if (capture.gameObject.activeSelf || transfer.gameObject.activeSelf || joint.gameObject.activeSelf)
                throw new InvalidOperationException("Faction panel styling reactivated gameplay-hidden starbase actions.");

            Image surface = store.targetGraphic as Image;
            Text label = store.GetComponentInChildren<Text>(true);
            Image icon = store.GetComponentsInChildren<Image>(true)
                .FirstOrDefault(item => item != null && item != surface && item.name == "Icon");
            Outline outline = surface != null ? surface.GetComponent<Outline>() : null;
            if (surface == null || surface.color.a < 0.10f || surface.color.a > 0.16f ||
                outline == null || !outline.enabled || outline.effectColor.a < 0.80f ||
                label == null || label.color.a < 0.99f || label.canvasRenderer.GetAlpha() < 0.99f ||
                icon == null || icon.color.a < 0.99f || icon.canvasRenderer.GetAlpha() < 0.99f)
                throw new InvalidOperationException("Owned-starbase facility buttons remain too dark or unreadable.");

            UnityEngine.Object.DestroyImmediate(canvasObject);
        }

        private static Button CreateValidationButton(Transform parent, string name, bool active)
        {
            GameObject root = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            Image surface = root.GetComponent<Image>();
            Button button = root.GetComponent<Button>();
            button.targetGraphic = surface;

            GameObject iconObject = new("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(root.transform, false);
            GameObject labelObject = new("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(root.transform, false);
            Text label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = name;
            root.SetActive(active);
            return button;
        }

        private static void ValidateUnavailableShipOverlay()
        {
            GameObject root = new("ShipListItemProbe", typeof(RectTransform), typeof(Gui.StarMap.ShipListItem));
            GameObject disabled = new("Disabled", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            disabled.transform.SetParent(root.transform, false);
            disabled.SetActive(true);
            var item = root.GetComponent<Gui.StarMap.ShipListItem>();
            typeof(Gui.StarMap.ShipListItem).GetField("_disabledPanel",
                BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(item, disabled);

            MethodInfo apply = typeof(Gui.StarMap.ShipListItem).GetMethod("ApplyUnavailableOverlay",
                BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo lateUpdate = typeof(Gui.StarMap.ShipListItem).GetMethod("LateUpdate",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (apply == null || lateUpdate == null)
                throw new InvalidOperationException("Unavailable ship overlay stabilizer was not found.");

            Image overlay = disabled.GetComponent<Image>();
            overlay.color = Color.black;
            apply.Invoke(item, null);
            overlay.color = Color.black;
            lateUpdate.Invoke(item, null);
            Material overlayMaterial = overlay.material;
            bool hasSpecialMaterial = overlayMaterial != null && overlayMaterial.shader != null &&
                                      overlayMaterial.shader.name != "UI/Default";
            if (overlay.color.a < 0.12f || overlay.color.a > 0.22f ||
                ColorLuminance(overlay.color) < 0.12f || hasSpecialMaterial ||
                overlay.canvasRenderer.GetAlpha() < 0.99f)
                throw new InvalidOperationException(
                    $"Unavailable ship option is still rendered as an opaque black block: " +
                    $"color={overlay.color}, luminance={ColorLuminance(overlay.color)}, " +
                    $"renderer={overlay.canvasRenderer.GetAlpha()}, material={overlayMaterial?.shader?.name}");

            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void ValidateSelectableRuntimeStability()
        {
            GameObject toggleObject = new("ToggleProbe", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Toggle), typeof(ReUIButtonMotion));
            GameObject child = new("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child.transform.SetParent(toggleObject.transform, false);
            try
            {
                Toggle toggle = toggleObject.GetComponent<Toggle>();
                toggle.targetGraphic = toggleObject.GetComponent<Image>();
                ColorBlock colors = toggle.colors;
                colors.normalColor = Color.black;
                colors.highlightedColor = Color.red;
                colors.selectedColor = Color.green;
                colors.pressedColor = Color.blue;
                colors.disabledColor = Color.black;
                toggle.colors = colors;
                toggle.transition = Selectable.Transition.ColorTint;
                child.GetComponent<Image>().canvasRenderer.SetAlpha(0.35f);

                ReUIButtonMotion motion = toggleObject.GetComponent<ReUIButtonMotion>();
                motion.RefreshVisualState();
                typeof(ReUIButtonMotion).GetMethod("LateUpdate",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(motion, null);
                ColorBlock stable = toggle.colors;
                if (toggle.transition != Selectable.Transition.None ||
                    stable.normalColor != Color.white || stable.highlightedColor != Color.white ||
                    stable.selectedColor != Color.white || stable.pressedColor != Color.white ||
                    stable.disabledColor != Color.white || child.GetComponent<Image>().canvasRenderer.GetAlpha() < 0.99f)
                    throw new InvalidOperationException("Toggle states can still change brightness or alpha.");

                GameObject actionObject = new("ActionProbe", typeof(RectTransform), typeof(CanvasRenderer),
                    typeof(Image), typeof(ActionButton));
                try
                {
                    ActionButton action = actionObject.GetComponent<ActionButton>();
                    Image image = actionObject.GetComponent<Image>();
                    typeof(ActionButton).GetField("_image", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.SetValue(action, image);
                    typeof(ActionButton).GetField("_pressedColor", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.SetValue(action, new Color(0.65f, 0.95f, 1f, 1f));
                    image.color = Color.black;
                    image.canvasRenderer.SetAlpha(0.3f);
                    typeof(ActionButton).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.Invoke(action, null);
                    if (image.color.a < 0.99f || image.canvasRenderer.GetAlpha() < 0.99f ||
                        ColorLuminance(image.color) < 0.70f)
                        throw new InvalidOperationException("ActionButton can still flicker or dim after state updates.");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(actionObject);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(toggleObject);
            }
        }

        private static void ValidateStarSystemObjectPresentation()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/StarMapScene.unity", OpenSceneMode.Single);
            Canvas canvas = FindCanvas(scene);
            if (canvas == null) throw new InvalidOperationException("StarMapScene contains no Canvas.");

            Button starObject = null;
            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length && starObject == null; i++)
            {
                MonoBehaviour[] behaviours = buttons[i].GetComponents<MonoBehaviour>();
                for (int j = 0; j < behaviours.Length; j++)
                {
                    if (behaviours[j] != null &&
                        behaviours[j].GetType().FullName == "Gui.StarMap.StarSystemObjectItem")
                    {
                        starObject = buttons[i];
                        break;
                    }
                }
            }
            if (starObject == null)
                throw new InvalidOperationException("Runtime InformationPanel StarSystemObjectItem button was not found.");

            CanvasGroup[] ancestors = starObject.GetComponentsInParent<CanvasGroup>(true)
                .Where(item => item.transform != starObject.transform)
                .ToArray();
            float[] ancestorAlphas = ancestors.Select(item => item.alpha).ToArray();

            starObject.gameObject.SetActive(true);
            starObject.interactable = false;
            ReUICanvasStyler.Apply(canvas);
            Canvas.ForceUpdateCanvases();
            // Force the actual Selectable disabled transition after styling.
            starObject.interactable = true;
            starObject.interactable = false;
            ReUICanvasStyler.Apply(canvas);
            Canvas.ForceUpdateCanvases();

            Image surface = starObject.transform.Find("ReUI Object Surface")?.GetComponent<Image>();
            Text label = starObject.transform.Find("Name")?.GetComponent<Text>();
            Image icon = starObject.transform.Find("Image")?.GetComponent<Image>();
            Color disabled = starObject.colors.disabledColor;
            if (starObject.interactable || surface == null || starObject.targetGraphic != surface ||
                surface.GetType() != typeof(Image) || !surface.enabled || surface.canvasRenderer.GetAlpha() < 0.99f ||
                surface.color.a < 0.04f || surface.color.a > 0.16f || !HasSingleBorder(surface) ||
                disabled.a < 0.99f)
                throw new InvalidOperationException("Disabled StarSystemObjectItem surface or interaction state is invalid.");
            if (label == null || !label.enabled || label.color.a < 0.99f ||
                label.canvasRenderer.GetAlpha() < 0.99f ||
                ColorLuminance(label.color) < 0.75f ||
                HasEnabledOutline(label))
                throw new InvalidOperationException("Disabled StarSystemObjectItem label is not fully readable.");
            if (icon == null || !icon.enabled || icon.color.a < 0.99f ||
                icon.canvasRenderer.GetAlpha() < 0.99f ||
                ColorLuminance(icon.color) < 0.48f)
                throw new InvalidOperationException("Disabled StarSystemObjectItem icon is not fully readable.");

            for (int i = 0; i < ancestors.Length; i++)
            {
                if (Mathf.Abs(ancestors[i].alpha - ancestorAlphas[i]) > 0.0001f)
                    throw new InvalidOperationException(
                        "StarSystemObjectItem styling modified an ancestor CanvasGroup used by the window.");
            }

        }

        private static void ValidateDialogPresentation()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/CommonGuiScene.unity", OpenSceneMode.Single);
            Canvas canvas = FindCanvas(scene);
            if (canvas == null) throw new InvalidOperationException("CommonGuiScene contains no Canvas.");

            ReUICanvasStyler.Apply(canvas);

            Button primary = FindButtonByPersistentMethod(scene, "CloseWithResultOption1");
            Button cancel = FindButtonByPersistentMethod(scene, "CloseWithResultOption2");
            if (primary == null || cancel == null)
                throw new InvalidOperationException("Confirmation-dialog action buttons were not found.");

            ValidateDialogAction(primary, "primary");
            ValidateDialogAction(cancel, "cancel");

            ReUIIconGraphic[] cancelIcons = cancel.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < cancelIcons.Length; i++)
            {
                if (cancelIcons[i].enabled && cancelIcons[i].gameObject.activeSelf)
                    throw new InvalidOperationException("Confirmation cancel button still contains a generated close glyph.");
            }
        }

        private static void ValidateCombatRewardTransparency()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/CombatScene.unity", OpenSceneMode.Single);
            Canvas[] canvases = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Canvas>(true))
                .ToArray();
            if (canvases.Length == 0) throw new InvalidOperationException("CombatScene contains no Canvas.");
            for (int i = 0; i < canvases.Length; i++)
                ReUICanvasStyler.Apply(canvases[i]);

            Transform rewardWindow = FindRect(scene, "CombatRewardWindow");
            if (rewardWindow == null)
                throw new InvalidOperationException("CombatRewardWindow was not found.");

            int checkedSurfaces = 0;
            Image[] images = rewardWindow.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                string objectName = images[i].gameObject.name;
                bool rewardSurface = objectName.StartsWith("ExpItem") ||
                                     objectName.StartsWith("PlayerExpItem") ||
                                     objectName.StartsWith("RewardItem") ||
                                     objectName == "Focus";
                if (!rewardSurface) continue;

                checkedSurfaces++;
                if (images[i].color.a > 0.001f)
                    throw new InvalidOperationException(
                        "Combat reward item retains a colored background: " + objectName);
            }

            if (checkedSurfaces == 0)
                throw new InvalidOperationException("No combat reward card surfaces were checked.");
        }

        private static void ValidateRadarColorProtection()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject canvasObject = new(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            Canvas canvas = canvasObject.GetComponent<Canvas>();

            GameObject minimapObject = new("Preview5CombatMinimap", typeof(RectTransform));
            minimapObject.transform.SetParent(canvasObject.transform, false);

            GameObject markerObject = new(
                "Target",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            markerObject.transform.SetParent(minimapObject.transform, false);
            Image marker = markerObject.GetComponent<Image>();
            Button markerButton = markerObject.GetComponent<Button>();
            markerButton.targetGraphic = marker;

            Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixels(new[]
            {
                Color.red, Color.green,
                Color.blue, Color.yellow,
            });
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f));
            Material material = new(Shader.Find("UI/Default"));
            Color authoredColor = new(0.73f, 0.18f, 0.94f, 0.87f);
            marker.sprite = sprite;
            marker.color = authoredColor;
            marker.material = material;

            GameObject allyTextObject = new(
                "AllyRadarTriangle",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            allyTextObject.transform.SetParent(minimapObject.transform, false);
            Text allyText = allyTextObject.GetComponent<Text>();
            Color authoredTextColor = new(0.20f, 0.62f, 1.00f, 1f);
            allyText.color = authoredTextColor;
            allyText.text = "●";
            allyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            ReUICanvasStyler.Apply(canvas);

            if (marker.sprite != sprite || marker.material != material ||
                !Approximately(marker.color, authoredColor))
                throw new InvalidOperationException(
                    "Combat minimap target marker Sprite, material or authored color was modified.");
            if (!Approximately(allyText.color, authoredTextColor))
                throw new InvalidOperationException("Combat minimap ally marker color was modified.");
            if (HasReUIStyleMarker(marker.gameObject) || HasReUIStyleMarker(allyText.gameObject))
                throw new InvalidOperationException("Combat minimap markers entered the generic ReUI styling pipeline.");

            var markerProperty = typeof(Gui.Combat.CombatMinimap).GetProperty(
                "MarkerSprite",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Sprite circularMarker = markerProperty?.GetValue(null) as Sprite;
            Texture2D markerTexture = circularMarker != null ? circularMarker.texture : null;
            if (markerTexture == null ||
                markerTexture.GetPixel(0, 0).a > 0.01f ||
                markerTexture.GetPixel(markerTexture.width / 2, markerTexture.height / 2).a < 0.99f)
                throw new InvalidOperationException("Combat minimap marker sprite is not circular.");

            UnityEngine.Object.DestroyImmediate(canvasObject);
            UnityEngine.Object.DestroyImmediate(sprite);
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(material);
        }

        private static void ValidateDialogAction(Button button, string description)
        {
            Image surface = button.targetGraphic as Image;
            Text label = button.GetComponentInChildren<Text>(true);
            LayoutElement layout = button.GetComponent<LayoutElement>();
            if (surface == null || !surface.enabled || surface.color.a < 0.02f || surface.color.a > 0.08f)
                throw new InvalidOperationException("Dialog " + description + " surface did not preserve its glass background.");
            if (label == null || !label.enabled || !label.gameObject.activeSelf || label.color.a < 0.99f)
                throw new InvalidOperationException("Dialog " + description + " label is not visible.");
            if (layout != null && layout.preferredHeight < 68f)
                throw new InvalidOperationException("Dialog " + description + " button height is inconsistent.");
        }

        private static void ValidateMarketPresentation()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject canvasObject = new("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            Canvas canvas = canvasObject.GetComponent<Canvas>();

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Gui/StarMapScene/MarketDialog.prefab");
            if (prefab == null) throw new InvalidOperationException("MarketDialog prefab was not found.");
            GameObject market = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (market == null) throw new InvalidOperationException("MarketDialog prefab could not be instantiated.");
            market.transform.SetParent(canvas.transform, false);
            market.SetActive(true);

            ReUICanvasStyler.Apply(canvas);

            Toggle buyTab = market.transform.Find("ItemsPanel/Buttons/Buy")?.GetComponent<Toggle>();
            Toggle sellTab = market.transform.Find("ItemsPanel/Buttons/Sell")?.GetComponent<Toggle>();
            if (buyTab == null || sellTab == null)
                throw new InvalidOperationException("Market buy/sell tabs were not found.");
            AssertTransparent(buyTab.targetGraphic as Image, "Market Buy tab target");
            AssertTransparent(buyTab.graphic as Image, "Market Buy tab selection");
            AssertTransparent(sellTab.targetGraphic as Image, "Market Sell tab target");
            AssertTransparent(sellTab.graphic as Image, "Market Sell tab selection");

            ValidateTransparentMarketButton(market.transform, "RightPanel/Buttons/BuyButton");
            ValidateTransparentMarketButton(market.transform, "RightPanel/Buttons/ExitButton");

            UnityEngine.Object.DestroyImmediate(canvasObject);
        }

        private static void ValidateTransparentMarketButton(Transform market, string path)
        {
            Button button = market.Find(path)?.GetComponent<Button>();
            if (button == null) throw new InvalidOperationException($"Market button was not found: {path}");

            AssertTransparent(button.targetGraphic as Image, path + " target");
            string[] chrome = { "Left", "Right", "Image", "Background", "Focus" };
            for (int i = 0; i < chrome.Length; i++)
            {
                Image image = button.transform.Find(chrome[i])?.GetComponent<Image>();
                if (image != null) AssertTransparent(image, path + "/" + chrome[i]);
            }
        }

        private static void AssertTransparent(Image image, string description)
        {
            if (image == null || image.color.a > 0.001f)
                throw new InvalidOperationException(description + " is not fully transparent.");
        }

        private static Button CreateValidationShortcut(Transform parent, string name, bool addLegacyIcon)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<Button>();

            GameObject buttonObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120f, 120f);
            Image surface = buttonObject.GetComponent<Image>();
            buttonObject.GetComponent<Button>().targetGraphic = surface;

            if (addLegacyIcon)
            {
                GameObject iconObject = new("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconObject.transform.SetParent(buttonObject.transform, false);
                RectTransform iconRect = iconObject.GetComponent<RectTransform>();
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = new Vector2(10f, 10f);
                iconRect.offsetMax = new Vector2(-10f, -10f);
            }
            else
            {
                GameObject labelObject = new("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                labelObject.transform.SetParent(buttonObject.transform, false);
                labelObject.GetComponent<Text>().text = "舰长";
            }

            return buttonObject.GetComponent<Button>();
        }

        private static Button CreateValidationCaptainShortcut(Transform parent)
        {
            Button button = CreateValidationShortcut(parent, "ThreeBodyCaptainButton", false);
            if (button == null) return null;

            Transform label = button.transform.Find("Label");
            if (label != null) label.gameObject.SetActive(false);

            Transform iconTransform = button.transform.Find("Icon");
            Image icon;
            if (iconTransform == null)
            {
                GameObject iconObject = new("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconObject.transform.SetParent(button.transform, false);
                icon = iconObject.GetComponent<Image>();
            }
            else
            {
                icon = iconTransform.GetComponent<Image>();
                if (icon == null) icon = iconTransform.gameObject.AddComponent<Image>();
            }

            RectTransform rect = icon.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(10f, 10f);
            rect.offsetMax = new Vector2(-10f, -10f);
            icon.sprite = Resources.Load<Sprite>("Textures/UI/captain");
            icon.color = Color.white;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.maskable = false;
            icon.enabled = icon.sprite != null;
            icon.gameObject.SetActive(icon.sprite != null);
            return button;
        }

        private static void ValidateShortcutSize(Button button, float buttonSize, float iconSize, string description)
        {
            if (button == null) throw new InvalidOperationException(description + " shortcut was not created.");
            RectTransform buttonRect = button.transform as RectTransform;
            ReUIIconGraphic icon = button.GetComponentInChildren<ReUIIconGraphic>(true);
            if (buttonRect == null || Mathf.Abs(buttonRect.sizeDelta.x - buttonSize) > 0.5f)
                throw new InvalidOperationException(description + " shortcut button size is incorrect.");
            if (icon == null || Mathf.Abs(icon.rectTransform.sizeDelta.x - iconSize) > 0.5f)
                throw new InvalidOperationException(description + " shortcut icon size is incorrect.");
        }

        private static void ValidateBottomIcon(
            Button button,
            ReUIIconKind kind,
            float iconSize,
            string description)
        {
            if (button == null)
                throw new InvalidOperationException(description + " bottom button was not found.");

            RectTransform buttonRect = button.transform as RectTransform;
            RectTransform host = button.transform.Find("ReUI Icon Host") as RectTransform;
            ReUIIconGraphic icon = host != null
                ? host.Find("ReUI Vector Icon")?.GetComponent<ReUIIconGraphic>()
                : null;
            Image surface = button.targetGraphic as Image;
            if (buttonRect == null || host == null || icon == null ||
                !host.gameObject.activeInHierarchy || !icon.gameObject.activeInHierarchy || !icon.enabled ||
                icon.Kind != kind || icon.color.a < 0.99f || icon.canvasRenderer.GetAlpha() < 0.99f ||
                buttonRect.rect.width <= 0f || buttonRect.rect.height <= 0f ||
                Mathf.Abs(host.rect.width - iconSize) > 0.5f ||
                Mathf.Abs(icon.rectTransform.rect.width - iconSize) > 0.5f)
                throw new InvalidOperationException(description + " bottom button/icon presentation is invalid.");

            if (surface == null || surface.color.a < 0.04f || surface.color.a > 0.10f ||
                !HasSingleBorder(surface))
                throw new InvalidOperationException(description + " bottom button fill or border is incorrect.");
            if (HasEnabledOutline(icon))
                throw new InvalidOperationException(description + " bottom icon still has a glow outline.");

            ReUIIconGraphic[] generated = button.GetComponentsInChildren<ReUIIconGraphic>(true);
            int visibleGenerated = generated.Count(item =>
                item != null && item.enabled && item.gameObject.activeInHierarchy);
            if (visibleGenerated != 1)
                throw new InvalidOperationException(description + " has duplicate or stale visible vector icons.");
        }

        private static void ValidateCaptainShortcut(Button button)
        {
            if (button == null)
                throw new InvalidOperationException("captain shortcut was not found.");

            Image icon = button.transform.Find("Icon")?.GetComponent<Image>();
            ReUIIconGraphic generated = button.GetComponentInChildren<ReUIIconGraphic>(true);
            if (icon == null || icon.sprite == null || icon.sprite.name != "captain" ||
                !icon.enabled || !icon.gameObject.activeInHierarchy ||
                icon.color.a < 0.99f || icon.canvasRenderer.GetAlpha() < 0.99f ||
                generated != null)
            {
                throw new InvalidOperationException(
                    "captain shortcut no longer preserves its authored bitmap icon.");
            }
        }

        private static bool HasPersistentMethod(Button button, string methodName)
        {
            if (button == null) return false;
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (button.onClick.GetPersistentMethodName(i) == methodName)
                    return true;
            }
            return false;
        }

        private static float GetEffectiveCanvasGroupAlpha(Transform transform)
        {
            float alpha = 1f;
            Transform current = transform;
            while (current != null)
            {
                CanvasGroup group = current.GetComponent<CanvasGroup>();
                if (group != null)
                {
                    alpha *= group.alpha;
                    if (group.ignoreParentGroups) break;
                }
                current = current.parent;
            }
            return alpha;
        }

        private static float ColorLuminance(Color color)
        {
            return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
        }

        private static bool Approximately(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.0001f &&
                   Mathf.Abs(a.g - b.g) < 0.0001f &&
                   Mathf.Abs(a.b - b.b) < 0.0001f &&
                   Mathf.Abs(a.a - b.a) < 0.0001f;
        }

        private static bool HasSingleBorder(Graphic graphic)
        {
            if (graphic == null) return false;
            Outline[] outlines = graphic.GetComponents<Outline>();
            int enabled = 0;
            for (int i = 0; i < outlines.Length; i++)
            {
                if (!outlines[i].enabled) continue;
                if (Mathf.Abs(outlines[i].effectDistance.x) > 1.1f ||
                    Mathf.Abs(outlines[i].effectDistance.y) > 1.1f)
                    return false;
                enabled++;
            }
            return enabled == 1;
        }

        private static bool HasEnabledOutline(Graphic graphic)
        {
            if (graphic == null) return false;
            Outline[] outlines = graphic.GetComponents<Outline>();
            for (int i = 0; i < outlines.Length; i++)
            {
                if (outlines[i].enabled) return true;
            }
            return false;
        }

        private static bool HasReUIStyleMarker(GameObject gameObject)
        {
            MonoBehaviour[] behaviours = gameObject.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] != null &&
                    behaviours[i].GetType().FullName == "ReUI.ReUIStyledElement")
                    return true;
            }
            return false;
        }

        private static Transform FindByName(Transform root, string name)
        {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == name) return all[i];
            }
            return null;
        }

        private static Button FindButtonByPersistentMethod(Scene scene, string methodName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Button[] buttons = roots[i].GetComponentsInChildren<Button>(true);
                for (int j = 0; j < buttons.Length; j++)
                {
                    for (int k = 0; k < buttons[j].onClick.GetPersistentEventCount(); k++)
                    {
                        if (buttons[j].onClick.GetPersistentMethodName(k) == methodName)
                            return buttons[j];
                    }
                }
            }
            return null;
        }

        private static Canvas FindCanvas(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Canvas canvas = roots[i].GetComponentInChildren<Canvas>(true);
                if (canvas != null) return canvas;
            }
            return null;
        }

        private static Transform FindPath(Scene scene, string path)
        {
            string[] parts = path.Split('/');
            GameObject root = scene.GetRootGameObjects().FirstOrDefault(item => item.name == parts[0]);
            if (root == null) return null;
            Transform current = root.transform;
            for (int i = 1; i < parts.Length; i++)
            {
                current = current.Find(parts[i]);
                if (current == null) return null;
            }
            return current;
        }

        private static int GetTechnologyFactionId(DbTechnology technology)
        {
            return technology switch
            {
                GameDatabase.DataModel.Technology_Component component => component.Faction.Id.Value,
                GameDatabase.DataModel.Technology_Ship ship => ship.Ship.Faction.Id.Value,
                GameDatabase.DataModel.Technology_Satellite satellite => satellite.Faction.Id.Value,
                _ => -1,
            };
        }

        public static void DumpSkillTreeMissingScripts()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/SkillTreeScene.unity", OpenSceneMode.Single);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < transforms.Length; i++)
                {
                    int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transforms[i].gameObject);
                    if (count > 0)
                        Debug.Log($"[ReUI Missing Script] path={GetPath(transforms[i])}, count={count}");
                }
            }
        }

        public static void ValidateReUI3Targets()
        {
            ValidateNamedTargets(
                "Assets/Scenes/StarMapScene.unity",
                new[] { "GameMenu", "Fleet", "Skills", "Quests", "Research", "CargoHold", "Exit" });
            ValidateNamedTargets(
                "Assets/ModulesShared/ShipEditor/Scenes/ShipEditorScene.unity",
                new[] { "ShipEditorWindow", "BackButton", "ShipsButton", "UndoButton", "BuildsButton", "ClearButton", "ExitButton", "RightPanel" });
            ValidateNamedTargets(
                "Assets/Scenes/SkillTreeScene.unity",
                new[] { "ExitButton" });
        }

        public static void ValidateMainMenu()
        {
            Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new InvalidOperationException("MainMenuScene could not be opened.");

            RectTransform menuRoot = FindRect(scene, "MainMenu");
            RectTransform programTitle = FindRect(scene, "ProgramTitle");
            RectTransform versionInfo = FindRect(scene, "VerstionInfo");
            int canvasCount = CountComponents<Canvas>(scene);
            int directButtonCount = CountDirectButtons(menuRoot);
            int missingScriptCount = CountMissingScripts(scene);

            Debug.Log(
                $"[ReUI Validation] scene={scene.name}, canvases={canvasCount}, " +
                $"mainMenu={(menuRoot != null)}, directButtons={directButtonCount}, " +
                $"programTitle={(programTitle != null)}, versionInfo={(versionInfo != null)}, " +
                $"missingScripts={missingScriptCount}");

            if (canvasCount == 0)
                throw new InvalidOperationException("MainMenuScene contains no Canvas.");
            if (menuRoot == null)
                throw new InvalidOperationException("MainMenu root was not found.");
            if (directButtonCount == 0)
                throw new InvalidOperationException("MainMenu root contains no direct Button children.");
            if (missingScriptCount != 0)
                throw new InvalidOperationException($"MainMenuScene contains {missingScriptCount} missing scripts.");
        }

        private static void ValidateNamedTargets(string scenePath, string[] objectNames)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new InvalidOperationException($"Scene could not be opened: {scenePath}");

            for (int i = 0; i < objectNames.Length; i++)
            {
                if (FindRect(scene, objectNames[i]) == null)
                    throw new InvalidOperationException($"{scene.name}: required object was not found: {objectNames[i]}");
            }

            int missingScripts = CountMissingScripts(scene);
            if (missingScripts != 0)
                throw new InvalidOperationException($"{scene.name} contains {missingScripts} missing scripts.");

            Debug.Log($"[ReUI3 Validation] scene={scene.name}, targets={objectNames.Length}, missingScripts={missingScripts}");
        }

        private static RectTransform FindRect(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                RectTransform[] rects = roots[i].GetComponentsInChildren<RectTransform>(true);
                for (int j = 0; j < rects.Length; j++)
                {
                    if (rects[j].name == objectName) return rects[j];
                }
            }
            return null;
        }

        private static int CountComponents<T>(Scene scene) where T : Component
        {
            int count = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
                count += roots[i].GetComponentsInChildren<T>(true).Length;
            return count;
        }

        private static int CountDirectButtons(RectTransform menuRoot)
        {
            if (menuRoot == null) return 0;

            int count = 0;
            for (int i = 0; i < menuRoot.childCount; i++)
            {
                if (menuRoot.GetChild(i).GetComponent<Button>() != null)
                    count++;
            }
            return count;
        }

        private static string GetPath(Transform transform)
        {
            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        private static int CountMissingScripts(Scene scene)
        {
            int count = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                    count += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transforms[j].gameObject);
            }
            return count;
        }
    }
}
