# Media and resources

This guide explains where project assets live and how the game or Unity uses them. It covers media folders and other `Resources` folders. Database and localization resources appear in the index below; their existing subsystem guides own the details.

Unity uses three different reference paths here. Scenes, prefabs, and settings hold serialized asset references. `ResourceLocator` maps selected sprites and sound clips by name. `Resources.Load` resolves other assets by path at runtime. A file's presence in a `Resources` folder does not by itself prove that game code requests it.

| Area | Usage guide |
| --- | --- |
| [Core Resources](core.md) | Shared GUI prefabs, textures, materials, physics materials, and small configuration files under `Assets/Resources/`. |
| [Sprites and shared icons](sprites.md) | Ship and component art, UI sprite atlases, and `Assets/ModulesShared/Resources/`. |
| [Sound and music](audio.md) | Game sound effects, the bundled music playlist, and the Gamemaster sound collection. |
| [Combat Resources](combat.md) | Runtime combat prefabs under `Assets/Modules/BattleSimulator/Resources/`. |
| [Presentation](presentation.md) | Animation, fonts, application icons, splash images, and the WebGL template. |
| [Configuration and plugin Resources](configuration.md) | ScriptableObjects, the resource locator prefab, and integration or editor resources. |
| [Database resources](../database.md) | `Assets/Modules/Database/Resources/` supplies the built-in game catalog and migration inputs. |
| [Localization resources](../localization.md) | `Assets/ModulesShared/Localization/Resources/` supplies language lists and translated text catalogs. |

## Adding or changing an asset

For a `ResourceLocator` sprite or sound, check the ID used by game data and refresh the locator prefab through its editor `Reload` action. For a `Resources.Load` asset, keep the path and name expected by the caller. For a serialized reference, update the owning scene, prefab, asset, or Project Settings entry. Music source files must be built into the `musicbundle` file that `LocalAssetLoader` reads from `StreamingAssets`.
