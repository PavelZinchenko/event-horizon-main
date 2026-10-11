# ResourceLocator Module Issues

This file records problems in `Assets/Modules/ResourceLocator/`. For the design, see [ResourceLocator module](../resource_locator.md).

The analysis was done by reading the code and the prefab on 2026-10-12. Severity and status use the same scale as the other issue files: **Confirmed**, **Confirmed (latent)**, or **Plausible**.

## Defects

### RL1. A missing audio clip with no database throws

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** `GetAudioClip` ([ResourceLocator.cs:95](../../Assets/Modules/ResourceLocator/Scripts/ResourceLocator.cs#L95)).

**Failure.** `GetSprite` checks `_database != null` before the database fallback. `GetAudioClip` does not. In a scene where the database is not injected (a test scene with stub services), an unknown clip name throws `NullReferenceException`. In the game, the database is always bound, so the code path is not reached.

**Fix.** Use the same null check as `GetSprite`.

### RL2. **Reload** does not refresh GUI icons

**Severity:** Low. **Status:** Confirmed.

**Where.** `Reload` ([ResourceLocator.cs:113](../../Assets/Modules/ResourceLocator/Scripts/ResourceLocator.cs#L113)) sets every array except `_guiIconSprites`.

**Failure.** A new GUI icon added to a folder is not picked up. A deleted icon stays referenced as a missing object, and `ToDictionary(item => item.name)` then throws `NullReferenceException` on the first `GuiIcon` lookup.

**Fix.** Give GUI icons a source folder and include them in **Reload**. Skip `null` entries when building the dictionaries.

### RL3. `Resources` misses are not cached

**Severity:** Low. **Status:** Confirmed.

**Where.** `GetSprite(string)` ([ResourceLocator.cs:98](../../Assets/Modules/ResourceLocator/Scripts/ResourceLocator.cs#L98)).

**Failure.** When `Resources.Load` returns `null`, nothing is stored. Every later request for the same name (for example a mod image name like `ship.png` in the `Effect` category) repeats a `Resources.Load` before the database fallback. UI lists that rebuild often pay this cost per item.

**Fix.** Cache `null` results as well.

### RL4. Name lookups have different case rules

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** The `Resources` cache uses `StringComparer.OrdinalIgnoreCase`. The category dictionaries use the default (case-sensitive) comparer.

**Failure.** A database entry `"Fighter"` for a sprite named `fighter` fails in the `Ship` table and falls through to the database, but works in the `Effect` category. Mod authors see inconsistent behaviour.

**Fix.** Use one comparer for all tables.

## Structural issues

### RL5. All catalog sprites are always in memory

**Problem.** The prefab references 503 sprites and clips and is loaded with `LoaderScene`, which lives for the whole session.

**Evidence.** [ResourceLocator.prefab](../../Assets/Modules/ResourceLocator/Resources/ResourceLocator.prefab); [TODO.txt](../../Assets/Modules/ResourceLocator/TODO.txt) ("replace with Addressables").

**Direction.** Move the tables to Addressables or to per-category `Resources` folders loaded on demand. Keep the `IResourceLocator` interface so consumers do not change.

### RL6. The prefab is in `Resources` but is used as a scene object

**Problem.** The prefab sits in a `Resources` folder only so that the editor-only **Reload** command can find it with `Resources.Load`. The game uses the scene instance.

**Evidence.** The only `Resources.Load<ResourceLocator>` call is inside `#if UNITY_EDITOR`.

**Direction.** Move the prefab out of `Resources` and find it with `AssetDatabase` in **Reload**. This removes a duplicate entry from the `Resources` index.
