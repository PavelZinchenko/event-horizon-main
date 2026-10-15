# Services, ServicesFacade, and SecurityStub Issues

This file records problems in `Assets/Modules/Services/`, `Assets/Modules/ServicesFacade/`, and `Assets/Modules/SecurityStub/`. For the design, see [Services module](../services.md).

The analysis was done by reading the code on 2026-10-12. No issue was reproduced in Play Mode. Severity is High, Medium, or Low. Status is **Confirmed** (the code path is certain), **Confirmed (latent)** (certain, but current data does not trigger it), or **Plausible** (needs specific conditions).

## Defects

### S1. `ShowEnemyMessages` is stored in the `showDamage` key

**Severity:** Medium. **Status:** Confirmed.

**Where.** [GameSettings.cs:144](../../Assets/Modules/Services/Scripts/Settings/GameSettings.cs#L144). The getter and setter use `_showDamage`. The field `_showEnemyMessages` (key `enemyComms`) is created but never used.

**Failure.** The two combat settings toggles are one setting. Turning off "enemy transmissions" in [SettingsCombat](../../Assets/Scripts/Gui/MainMenu/SettingsCombat.cs) also hides damage numbers, and the reverse. `CombatSceneInstaller` reads both and gets the same value.

**Fix.** Use `_showEnemyMessages` in the property.

### S2. `PlayerPrefsStorage` writes mod saves into the base-game slot

**Severity:** High. **Status:** Confirmed.

**Where.** `Save` writes to `_key` (`savegame`) for every session ([PlayerPrefsStorage.cs:40](../../Assets/Modules/Services/Scripts/Storage/PlayerPrefsStorage.cs#L40)). `TryLoad` reads `savegame.<mod>` for a mod ([line 64](../../Assets/Modules/Services/Scripts/Storage/PlayerPrefsStorage.cs#L64)). The record does not contain the mod ID.

**Failure.** This storage is used by all open-source builds and by commercial WebGL builds.
1. The player starts a mod and plays. Each save overwrites the base-game save.
2. On the next start with the mod, `savegame.<mod>` is empty, so the mod progress is lost.
3. On the next start without the mod, the base game loads the mod session as if it were a base-game save. Items and ships from the mod database are missing or resolve to other IDs.

The private file storage avoids this: it stores the mod ID in the header and uses the mod name as the file name.

**Fix.** Write to `savegame.<gameData.ModId>` when `ModId` is not empty, and store the mod ID in the header (format 4), as `LocalStorage` does. Reset `_currentGameId` when the mod changes.

### S3. A missing sound clip throws in `SoundPlayer.Update`

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [SoundPlayer.cs:128](../../Assets/Modules/Services/Scripts/Audio/SoundPlayer.cs#L128).

**Failure.** `Play(AudioClipId)` queues the clip that `IResourceLocator` returns. When the name does not exist (for example a typo in a mod), the clip is `null`. A `null` clip with `soundId == 0` is not treated as a stop request, so `data.AudioClip.loadState` throws `NullReferenceException`. The exception ends this frame's `Update`; the remaining queue waits one frame. The error repeats each time the sound plays.

**Fix.** In `Play`, return when the resolved clip is `null`. In `Update`, skip `null` clips.

### S4. Open-source builds have no music until the bundle is built

**Severity:** Medium. **Status:** Confirmed.

**Where.** [LocalAssetLoader.cs:15](../../Assets/Modules/Services/Scripts/Assets/LocalAssetLoader.cs#L15); `Assets/StreamingAssets/` is ignored by Git.

**Failure.**
1. A fresh clone has no `StreamingAssets/musicbundle`. The game logs "Failed to load AssetBundle!" and plays no default music.
2. `LocalAssetLoader` never invokes the callback on failure. `MusicPlayer._bundleLoadStarted` stays `true`, so it never retries in the session.
3. `AssetBundle.LoadFromFile` with `streamingAssetsPath` does not work on WebGL, where streaming assets are URLs.

**Fix.** Document **Assets > Build AssetBundles** in the README, or build the bundle in a pre-build hook. Call back with `null` on failure. Use `UnityWebRequestAssetBundle` on WebGL.

### S5. Adding a listener during a broadcast can skip or repeat handlers

**Severity:** Low. **Status:** Plausible.

**Where.** `DelegateList.Broadcast` iterates `_delegates.Values` by index with a count read before the loop ([MessengerContext.cs:146](../../Assets/Modules/Services/Scripts/Messenger/MessengerContext.cs#L146)).

**Failure.** If a handler adds the first listener of another scene for the same event (for example by opening a scene that subscribes in `Start`), `SortedList` inserts a new entry. Later entries shift by one: one scene's handlers run twice and the last entry is skipped. The same happens in reverse when a scene unloads during a broadcast.

**Fix.** Copy the values to an array before the loop.

### S6. Two messenger events are never sent

**Severity:** Low. **Status:** Confirmed.

**Where.** `EventType.GuardianDefeated` has a listener in [StarSystem.cs:96](../../Assets/Scripts/Legacy/Map/System/StarSystem.cs#L96). `EventType.GraphicsQualityChanged` has listeners in [Background.cs:32](../../Assets/Scripts/Legacy/Map/Background.cs#L32) and [CombatBackground.cs:31](../../Assets/Scripts/Combat/Background/CombatBackground.cs#L31). No code broadcasts either event.

**Failure.** The star-system view never removes the defeated guardian's ships by this path; today it is rebuilt when the star map scene reloads after combat, so the bug is hidden. A graphics quality change does not switch the background material until the scene reloads.

**Fix.** Broadcast the events from the code that changes the state, or remove the listeners.

### S7. Changing the sound volume loses looping sound IDs

**Severity:** Low. **Status:** Confirmed.

**Where.** The `Volume` setter rebuilds `_audioSources` with `id = 0` for every source ([SoundPlayer.cs:41](../../Assets/Modules/Services/Scripts/Audio/SoundPlayer.cs#L41)).

**Failure.** A looping sound that plays during the change (for example the star-map engine sound of `PlayerShipObject`) keeps playing, but `Stop(id)` no longer finds its source. The sound loops until the source is reused.

**Fix.** Set the volume on the existing entries; build the list once in `Initialize`.

### S8. Dead settings

**Severity:** Low. **Status:** Confirmed.

**Where.** `EditorText` (no key, no reader), `LastFacebookPostDate` (no reader), `RateButtonClicked` uses the key `settings`.

**Failure.** No player impact. The interface carries fields that suggest features that no longer exist.

**Fix.** Remove `EditorText` and `LastFacebookPostDate` from `IGameSettings`. Keep the `settings` key name for compatibility, but rename the field.

### S9. `GameScene.Battle` points to a missing scene

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [SceneName.cs:53](../../Assets/Modules/ServicesFacade/Scripts/SceneManager/SceneName.cs#L53) maps `Battle` to `BattleScene`. No such scene exists, and no code uses `GameScene.Battle`.

**Failure.** A state that requires `GameScene.Battle` would make `LoadSceneAsync` fail and leave the scene manager in `Loading`.

**Fix.** Remove the value.

## Structural issues

### S10. Two event systems

**Problem.** The enum messenger and typed signals do the same job. About 25 signals are re-broadcast as messenger events by `SignalsTranslator`.

**Evidence.** [EventType.cs](../../Assets/Modules/ServicesFacade/Scripts/Messenger/EventType.cs) (56 values, 15 unused); [SignalsTranslator.cs](../../Assets/Scripts/Services/Messenger/SignalsTranslator.cs).

**Direction.** Stop adding messenger events. Move listeners to signals area by area, then delete the unused enum values. See [Application layer issues](application_layer.md).

### S11. `GameSettings` mixes preferences and identity data

**Problem.** `GameSettings` stores user preferences, a per-install GUID, daily-reward dates, and the active mod in `PlayerPrefs`. Clearing preferences (for example on WebGL when the browser storage is cleared) also resets the daily reward and the active mod.

**Evidence.** `rdata`, `guid`, and `mod` keys in [GameSettings.cs](../../Assets/Modules/Services/Scripts/Settings/GameSettings.cs).

**Direction.** Keep preferences in `GameSettings`. Move gameplay data such as the daily reward date into the session.
