# Configuration and plugin Resources

[Resource index](index.md)

## Game presentation settings

`Assets/ScriptableObjects/CurrencySettings.asset` stores currency icons and colors. The legacy `PricePanel` asks it for the icon associated with each price currency. Newer UI color choices also come from `UiTheme`, so the currency settings asset is not the only source of currency presentation.

`PlanetSettings.asset` maps planet types to images. Exploration and orbital object views call `GetPlanetImage(type, seed)`; the seed picks a stable variant for gas, barren, or terran planets. `PlayerSkillSettings.asset` maps skill types to icons and defines the fallback icon used by skill tree nodes.

`Assets/Modules/AppConfiguration/ScriptableObjects/UiTheme.asset` is the shared UI theme. `ServicesInstaller` passes it to `UiThemeLoader`; themed text, images, and item rows then read colors, font choices, and sizes from `UiTheme.Current`. `FontTable.asset` supplies platform and language font choices to `SystemFontLocator`. These are serialized ScriptableObjects, not files requested by path from a `Resources` folder.

## Resource locator prefab

`Assets/Modules/ResourceLocator/Resources/ResourceLocator.prefab` contains the editor collected sprite and sound arrays used by `ResourceLocator`. `LoaderScene` and `BaseTestScene` reference the prefab. Its editor `Reload` command loads the prefab by the `ResourceLocator` Resources path and saves refreshed arrays. Runtime consumers request sprites and sounds from the locator instance; they do not scan the source folders themselves.

## Integration and editor Resources

`Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset` is the Google Mobile Ads plugin's settings asset. `Assets/ModulesPrivate/Plugins/StansAssets/Support/Settings/Resources/` holds settings for the iOS native and deployment plugins. These assets belong to their integration packages; game feature code does not use them as general media.

`Assets/ModulesShared/Gui/Plugins/com.unity.uiextensions/Runtime/Resources/Shaders/` contains UI Extensions shaders for soft masks, particles, crop, and blend effects. The plugin's UI materials and components select them when those effects render. Keep them with the plugin because its runtime resource paths and material references can depend on that layout.

`Assets/Plugins/Zenject/OptionalExtras/MemoryPoolMonitor/Editor/Resources/Installers/MpmSettingsInstaller.asset` configures Zenject's optional editor memory pool monitor. It is an editor tool resource, not gameplay content. The Cyotek color picker images under `Assets/Modules/Database/.Editor/.../Resources/` provide its editor cell background, eyedropper cursor, and palette controls. They are editor UI assets and are outside the excluded database catalog resource tree.
