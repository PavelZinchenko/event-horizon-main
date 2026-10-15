# GameServices Module Issues

This file records problems in `Assets/Modules/GameServices/`. For the design, see [GameServices module](../game_services.md).

The analysis was done by reading the code on 2026-10-12. No issue was reproduced in Play Mode. Severity is High, Medium, or Low. Status is **Confirmed**, **Confirmed (latent)**, or **Plausible**.

## Defects

### GS1. A scene dropped while loading leaves the manager in `Loading`

**Severity:** Medium. **Status:** Plausible.

**Where.** [GameSceneManager.cs](../../Assets/Modules/GameServices/Scripts/SceneManager/GameSceneManager.cs), `Load` and `OnSceneLoaded`.

**Failure.**
1. `Load({A, B})` starts loading B. `_loadingScenes = {B}`.
2. Before B finishes, `Load({A})` runs. B is not required, but it is still in `_loadingScenes`, and `Load` does not remove it. `_loadingScenes.Count` is 1, so the state stays `Loading`.
3. B finishes. `OnSceneLoaded` sees that B is not required, unloads it, and returns early without removing it from `_loadingScenes`.
4. The state never becomes `Ready`. The state machine never activates the top state; the game hangs on the loading screen.

This needs two state changes inside one scene load, for example a quick double tap that starts and cancels a transition.

**Fix.** In `OnSceneLoaded`, remove the scene from `_loadingScenes` before the "unexpected" branch and re-check the count. Remove the `Debug.Break()` calls.

### GS2. Re-requesting a scene that is still unloading loads a second copy

**Severity:** Low. **Status:** Plausible.

**Where.** `Load` removes unloaded scenes from `_activeScenes` at once (`IntersectWith`), before `UnloadSceneAsync` completes.

**Failure.** If the next `Load` asks for the same scene before the unload finishes, the manager starts a new `LoadSceneAsync`. For a short time two scenes with the same name exist. `GetSceneByName` in a later unload can pick the wrong one.

**Fix.** Track unloading scenes and wait for `sceneUnloaded` before loading the same scene again.

### GS3. WebGL background detection is disabled

**Severity:** Low. **Status:** Confirmed.

**Where.** [WebGlApplication.cs](../../Assets/Modules/GameServices/Scripts/Application/WebGlApplication.cs). The `Agava.WebUtility` lines are commented out; `OnApplicationInBackground` is never called.

**Failure.** When the browser tab is hidden but the canvas keeps focus state, the game is not paused and audio is not muted. Behaviour depends on the browser's own throttling.

**Fix.** Use `document.visibilitychange` through a small `.jslib`, or remove the dead branch.

### GS4. Mobile and standalone fire activation signals on every callback

**Severity:** Low. **Status:** Confirmed.

**Where.** `OnApplicationFocus` and `OnApplicationPause` in [MobileApplication.cs](../../Assets/Modules/GameServices/Scripts/Application/MobileApplication.cs) and [StandaloneApplication.cs](../../Assets/Modules/GameServices/Scripts/Application/StandaloneApplication.cs).

**Failure.** On Android, one trip to the home screen calls both callbacks, so `AppActivatedSignal(false)` and `GamePausedSignal(true)` fire twice. Handlers that save or show UI on these signals run twice. The WebGL version already compares the old and new state.

**Fix.** Fire only when `IsActive` or `Paused` changes.

### GS5. `Configuration.Create` allocates an unused `InputActions`

**Severity:** Low. **Status:** Confirmed.

**Where.** `var defaultActions = new InputActions().Combat;` in [Configuration.cs:17](../../Assets/Modules/GameServices/Scripts/Input/Configuration.cs#L17).

**Failure.** Every call creates an `InputActionAsset` that is never used or disposed. It leaks one asset per settings save.

**Fix.** Delete the line.

## Structural issues

### GS6. Dead and unused code

**Problem.** `Timer` is never created; only its static `Paused` flag is set. The asmdef references `ResourceLocator` and `UniRx`, which no file uses.

**Evidence.** No `new Timer(` in the project. No `using UniRx` or `Services.Resources` in the module.

**Direction.** Delete `Timer` and the `Timer.Paused` assignments. Remove the two references from the asmdef.

### GS7. Each consumer owns its own `InputActions`

**Problem.** `InputSystemKeyboard`, `InputSystemMouse`, and `KeyBindingPanel` each create an `InputActions`. Overrides must be copied between them through a JSON string and a signal.

**Evidence.** Three `new InputActions()` calls in [Keyboard.cs](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Keyboard.cs), [Mouse.cs](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Mouse.cs), and [KeyBindingPanel.cs](../../Assets/Scripts/Gui/MainMenu/KeyBindingPanel.cs).

**Direction.** Bind one `InputActions` instance in the root container and use the Input System's `SaveBindingOverridesAsJson` and `LoadBindingOverridesFromJson`. This also allows rebinding all actions, not just nine.
