# Animation, fonts, and platform artwork

[Resource index](index.md)

## UI animation

`Assets/Animation/` contains Animator controllers and clips for interface transitions. The controller is assigned to an Animator in a scene or prefab; the controller then selects clips and states. `CenterPanel.controller` appears on dialogs and panels in combat, settings, exploration, and the star map. `FadeoutPanel.controller` appears on scene panels and windows that fade. `SidePanel.controller` appears on menu and star map side panels; `SidePanelLeft.controller` appears in the main menu scene. Their open and closed clips provide the corresponding movement or visibility states.

The folder also contains menu, shop item, toggle, and blend panel animation assets. A scan of serialized GUID references outside `Assets/Animation/` did not find a current scene or prefab reference to `MainMenu.controller`, `ShopItem.controller`, or `ToggleButton.controller`. Clips may be referenced inside controllers, so inspect the controller before removing a clip. Folder membership alone does not make an animation active.

## Fonts

`Assets/Fonts/` contains source typefaces and imported Unity font assets. `UiTheme.asset` selects the Bicubik, Exo2 Regular, and TTWPGOTT font sources for themed text. `ThemedText` reads the theme's font and size choices when presenting interface labels. Scenes and prefabs can also hold direct font references.

`FontTable.asset` is a separate platform and language font selector used by `SystemFontLocator`. It names system fonts for Windows, Android, and iOS, and includes a built in font reference for Armenian on iOS. The font table belongs to presentation; this guide does not cover translated text resources.

## Application icon and splash art

`Assets/ApplicationIcon/` contains platform and store artwork. `ProjectSettings.asset` currently references `appstore_icon_1024.JPG` as the general icon, `icon_white.png` for an iPhone icon slot, and `banner.png` as an Android banner. The ordinary `icon.png` and seasonal `appstore_icon_xmas_1024.png` have no verified serialized reference in the inspected project. One iPhone 1024 pixel slot points to a GUID with no matching asset metadata in this checkout; verify that slot before relying on it for a store build.

`Assets/SplashImage/` contains Android and PC splash artwork. Neither image has a verified serialized GUID reference in current assets or `ProjectSettings.asset`. The Android splash screen field is empty. These are available artwork sources, but the inspected settings do not establish that either appears in a build.

## WebGL page

`Assets/WebGLTemplates/FullscreenWindow/index.html` is the HTML shell for the custom WebGL player page. `ProjectSettings.asset` selects `PROJECT:FullscreenWindow`, so Unity uses this template when generating a WebGL build. The template controls the page around the Unity canvas and its fullscreen behavior; it is not loaded through `Resources.Load`.
