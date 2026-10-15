# Core Resources

[Resource index](index.md)

`Assets/Resources/` is Unity's runtime resource tree. Assets here can be loaded by path without a serialized reference. The game also stores ordinary scene and prefab references to some assets in this tree. These two routes have different maintenance rules: keep the path stable for `Resources.Load` callers, and keep the Unity reference intact for serialized users.

## Data and scene contract

`skills.json` defines the older player skill table. `GameServices.Database.Skills` loads it as a `TextAsset` at the `skills` path and reads skill IDs, types, and multipliers. Its editor `Assign` method writes back to the same file. `BillingMode.json` is present, but the inspected game code has no verified loader for it; treat it as an unconfirmed configuration input. `ZenjectDefaultSceneContractConfig.asset` supplies Zenject's default scene contract configuration.

## GUI prefabs

`Gui/CombatScene/`, `Gui/Settings/`, and `Gui/StarMapScene/` group windows and panels for combat, settings, and the star map. Their names describe the screen or task that hosts them: rewards and ship rows in combat; controls and key setup in settings; and markets, cargo, research, quests, arenas, shipyard, and other star map dialogs. `Gui/Elements/` contains a reusable label. The scenes and UI code can reference these prefabs as serialized assets; their location also makes them eligible for Unity Resources loading. No general `Gui/...` runtime loader was found in the inspected code, so do not infer that every prefab is instantiated by path.

`Prefabs/Gui/` contains older reusable controls and rows, such as ship controls, hangar entries, action buttons, messages, and shop items. `PrefabCache.LoadPrefab(string)` loads a caller supplied path under `Prefabs/`, which is the path convention for this tree. Some prefabs may instead be referenced directly from scenes or other prefabs.

## Materials, shaders, and physics

`Materials/` contains rendering rules for combat backgrounds, maps, planets, shields, trails, particles, outlines, and UI surfaces. Prefabs and scenes reference material assets; those materials reference shaders in the same folder. `GameObjectExtensions` explicitly loads `Materials/Default` as a fallback material. The `.shader.bak` files are source backups, not shader assets used by Unity's normal import path.

`PhysicsMaterials/` contains the default and bullet 2D physics materials. Collider references choose friction and bounce behavior. These are assigned to physics components, rather than looked up by `ResourceLocator`.

## Textures

`Textures/Avatars/`, `Artifacts/`, `Bullets/`, and `Effects/` have explicit runtime paths in `ResourceLocator`. A `SpriteId` category determines the folder; its ID supplies the filename. Avatar images appear in profile and dialogue contexts, artifact images in item displays, and bullet and effect images initialize combat visuals.

`Textures/GUI/Controls/` supplies action button pictures. The locator's editor `Reload` action gathers this folder into its control button array. `ShipControlsPanel` asks the locator for each system's control icon. `Textures/GUI/Flags/` has a separate direct loader: `SettingsGeneral` loads `Textures/GUI/Flags/` plus the selected language icon name for the language menu.

The rest of `Textures/GUI/` supports screens and widgets through serialized sprite references: interface backgrounds and frames, module and power up symbols, portraits, satellite symbols, and in app purchase art. `Textures/HUD/` supplies health cells and status or damage symbols. `Textures/Icons/` provides general navigation and action icons. These folders describe where art is organized; individual use depends on scene, prefab, or data references.

`Textures/Background/`, `Maps/`, `StarMap/`, and `StarSystem/` provide map backdrops and detail masks, map objects and markers, and star system bodies. `Textures/Terrain/` and `Objects/` provide exploration terrain and object images. `Textures/Currency/` supplies currency images that settings and UI can reference. The textures at the root of `Textures/` support backgrounds and simple combat or UI shapes. `AmmunitionAndEffects.spriteatlas` packs selected textures for rendering; it does not select an image at runtime.

A sprite without a special `SpriteId` category can still be loaded by its full Resources path through `ResourceLocator.GetSprite(string)`. That path is cached after the first successful load.
