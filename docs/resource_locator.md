# ResourceLocator Module

The ResourceLocator module turns a sprite or audio-clip name from the database into a Unity asset. It keeps preloaded tables of ship, component, satellite, control, and GUI sprites and of sound clips. It loads other sprites from `Resources/` on demand. When it finds nothing built in, it asks the database for a mod image or mod clip.

| | |
| --- | --- |
| Assembly | `ResourceLocator` ([ResourceLocator.asmdef](../Assets/Modules/ResourceLocator/ResourceLocator.asmdef)). References: CommonComponents, Database, Zenject, ServicesFacade. |
| Namespace | `Services.Resources` |
| Source | [`Scripts/ResourceLocator.cs`](../Assets/Modules/ResourceLocator/Scripts/ResourceLocator.cs) (1 file, 149 lines) |
| Prefab | [`Resources/ResourceLocator.prefab`](../Assets/Modules/ResourceLocator/Resources/ResourceLocator.prefab) |
| Interface | [`IResourceLocator`](../Assets/Modules/ServicesFacade/Scripts/ResourceLocator/IResourceLocator.cs) in ServicesFacade |
| Binding | `ServicesInstaller` binds the prefab instance in `LoaderScene` and `BaseTestScene` (`FromInstance`). |
| Consumers | About 130 classes: combat views and effects, ship editor, star map, all item and ship UI. |
| Known issues | [ResourceLocator issues](issues/resource_locator.md) |

## Contents

1. [Interface](#interface)
2. [Sprite lookup](#sprite-lookup)
3. [Audio lookup](#audio-lookup)
4. [Preloaded tables](#preloaded-tables)
5. [Refreshing the tables](#refreshing-the-tables)
6. [Memory and build impact](#memory-and-build-impact)
7. [Adding a new sprite](#adding-a-new-sprite)

## Interface

```csharp
public interface IResourceLocator
{
    AudioClip GetAudioClip(AudioClipId id);
    Sprite GetSprite(SpriteId sprite);
    Sprite GetSprite(string name);
}
```

`SpriteId` ([SpriteId.cs](../Assets/Modules/CommonComponents/Scripts/DataTypes/SpriteId.cs)) is a name plus a category (`SpriteId.Type`). The database schema sets the category through the `typeid` of an image field. `AudioClipId` is a clip name plus a loop flag. See [Database: Media](database.md#media-images-and-audio).

## Sprite lookup

`GetSprite(SpriteId)` returns `null` for an empty ID. Otherwise it selects a source by category:

| Category | Source |
| --- | --- |
| `Ship` | `_shipSprites` table (ships and starbases). |
| `ShipIcon` | `_shipIconSprites`; if missing, the `Ship` table with the same name. |
| `Component` | `_componentSprites`. |
| `Satellite` | `_satelliteSprites`. |
| `ActionButton` | `_controlButtonSprites`. |
| `GuiIcon` | `_guiIconSprites`. |
| `AvatarIcon` | `Resources.Load("Textures/Avatars/<name>")` |
| `ArtifactIcon` | `Resources.Load("Textures/Artifacts/<name>")` |
| `Ammunition` | `Resources.Load("Textures/Bullets/<name>")` |
| `Effect` | `Resources.Load("Textures/Effects/<name>")` |
| `Default`, `SkillIcon` | `Resources.Load("<name>")` (the name is a full `Resources` path). |

If the result is `null` and the database is injected, the locator returns `IDatabase.GetImage(name).Sprite`. This is the mod image, or `null` when the mod has no such image.

Consequence: a built-in sprite always wins. A mod cannot replace a built-in sprite by giving an image with the same name. Mod image keys include the file extension (`ship.png`), so they rarely collide with built-in names.

`GetSprite(string)` loads `Resources.Load<Sprite>(name)` and caches the result in a case-insensitive dictionary. It does not cache misses, so each failed lookup repeats `Resources.Load`.

## Audio lookup

`GetAudioClip(AudioClipId)` returns `null` for an empty ID. It looks in `_audioClips` by name. If the name is missing, it returns `IDatabase.GetAudioClip(name).AudioClip` (a mod WAV or Ogg clip). The loop flag is not used here; [`SoundPlayer`](services.md#audio) reads it from the ID.

Music does not go through this method. See [Services: Audio](services.md#audio).

## Preloaded tables

The serialized arrays on the prefab are converted to dictionaries keyed by asset name on first use. Name comparison in these dictionaries is case-sensitive (unlike the `Resources` cache). Duplicate names throw `ArgumentException` from `ToDictionary` on first use.

Counts in the checked-in prefab:

| Field | Entries | Source folder used by **Reload** |
| --- | --- | --- |
| `_shipSprites` | 145 | `Assets/Sprites/Ships`, `Assets/Sprites/Starbases` |
| `_shipIconSprites` | 97 | `Assets/Sprites/ShipIcons` |
| `_componentSprites` | 128 | `Assets/Sprites/Components` |
| `_satelliteSprites` | 23 | `Assets/Sprites/Satellites` |
| `_controlButtonSprites` | 15 | `Assets/Resources/Textures/GUI/Controls` |
| `_guiIconSprites` | 39 | Not refreshed by **Reload**; edited by hand. |
| `_audioClips` | 56 | `Assets/Audio` |

## Refreshing the tables

In the editor, the component has a context-menu command **Reload** (`#if UNITY_EDITOR`). It loads the prefab from `Resources`, scans the folders in the table above with `AssetDatabase.LoadAssetAtPath`, assigns the arrays to both the scene instance and the prefab, and saves the prefab.

Run **Reload** after you add, rename, or delete a sprite in one of these folders. Otherwise the new sprite is not found by its category and falls back to the database (where it is also missing).

## Memory and build impact

The prefab is in a `Resources` folder and is referenced by `LoaderScene`. All 503 referenced assets are therefore in the build and are loaded into memory when the game starts. They stay loaded for the whole session, because `LoaderScene` is never unloaded. [`TODO.txt`](../Assets/Modules/ResourceLocator/TODO.txt) says "replace with Addressables".

## Adding a new sprite

1. Put the sprite in the folder for its category (see [Preloaded tables](#preloaded-tables)). For `Effect`, `Ammunition`, `AvatarIcon`, and `ArtifactIcon`, use the matching `Resources/Textures/` subfolder instead; no table change is needed.
2. For table categories, select the `ResourceLocator` object in `LoaderScene` and run **Reload** from the component context menu. For a GUI icon, add it to `_guiIconSprites` by hand.
3. Reference the sprite by its asset name (without extension) in the database.
4. For a new ship sprite, also create a combat prefab with **Window > Application Manager > Generate ship prefabs**, or rely on the `Default` prefab. See [AppConfiguration](app_configuration.md#application-manager-window).
