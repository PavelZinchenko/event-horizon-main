# Gui Module Issues

This file records problems in `Assets/ModulesShared/Gui/`, including its vendored plugins. For the design, see [Gui module](../gui.md).

The analysis was done by reading the code on 2026-10-12. Each issue has a severity (High, Medium, or Low) and a status:

- **Confirmed** – the code path is certain.
- **Confirmed (latent)** – the code path is certain, but current data or settings do not trigger it.
- **Plausible** – the code is wrong, but the failure needs specific conditions.

The module is in the `ModulesShared` submodule, which Frontier also uses. A fix there changes both games. Check Frontier before you change a public type or a rule.

## Defects

### GU1. The Escape check has no null check for the keyboard

**Severity:** Low. **Status:** Plausible.

**Where.** `GuiManager.Tick` ([GuiManager.cs:352](../../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs#L352)).

**Failure.** `Keyboard.current` is `null` when the Input System has no keyboard device. `Tick` then throws `NullReferenceException` every frame. The game ships on touch devices, so this needs a platform where no keyboard device is created. It was not observed in this analysis.

**Fix.** `var keyboard = Keyboard.current; if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;`.

### GU2. A plain `Window` always closes with `Cancel`

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [Window.cs:39](../../Assets/ModulesShared/Gui/Scripts/Core/Window.cs#L39), [line 51](../../Assets/ModulesShared/Gui/Scripts/Core/Window.cs#L51).

**Failure.** `Window` sets its exit code to `Cancel` and has no way to change it. Its default class is `HudElement`, which is automatic. So a `Window` closed by code goes into the reopen queue and comes back at the next chance. A close callback never sees `Ok`. The only `Window` in the project (in `StarMapScene`) is not closed this way today.

**Fix.** Add `Close(WindowExitCode)` as in `AnimatedWindow`.

### GU3. `OpenWindow` with arguments throws when a callback is pending

**Severity:** Low. **Status:** Plausible.

**Where.** [GuiManager.cs:77](../../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs#L77); compare [line 62](../../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs#L62).

**Failure.** The overload with `WindowArgs` adds the close callback with `Dictionary.Add`. A second call for a window that is still open with a callback throws `ArgumentException`. The window is not updated with the new arguments. The overload without arguments only logs a warning. `ShowConfirmationDialog` and `ShowBuyConfirmationDialog` use the throwing overload. A modal dialog blocks input to other windows, so a second call needs code, not a click.

**Fix.** Use the same rule in both overloads. Either replace the callback (and run the old one with `Cancel`) or reject the second open without an exception.

### GU4. A duplicate window ID stops the scene registration

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** `OnWindowsLoaded` ([GuiManager.cs:249](../../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs#L249)).

**Failure.** When two loaded windows have the same GameObject name, the manager logs with `UnityEngine.Debug.Log` (not an error), calls `Debug.Break()`, and then calls `_windows.Add`, which throws. In a player build `Debug.Break` does nothing. The exception stops the loop, so the rest of that scene's windows are not registered. Opening them later throws `ArgumentException`. A scene-scoped name clash is easy to create when a prefab window is placed twice.

**Fix.** Log an error with both object paths and skip the duplicate. Or check for unique names in an editor validation step.

### GU5. The debug console records `Trace` exceptions twice

**Severity:** Low. **Status:** Confirmed.

**Where.** `DebugConsoleLogger.LogException` ([DebugConsoleLogger.cs:105](../../Assets/ModulesShared/Gui/Scripts/DebugConsole/DebugConsoleLogger.cs#L105)) and `OnLogMessageReceived` ([line 129](../../Assets/ModulesShared/Gui/Scripts/DebugConsole/DebugConsoleLogger.cs#L129)).

**Failure.** `GameDiagnostics.Trace.LogException(e)` stores an entry, then forwards to `UnityEngine.Debug.LogException`. Unity reports that message through `logMessageReceivedThreaded` as `LogType.Exception`, and the logger stores it again. The console shows each such exception twice. `Database.TryLoad` uses this call.

**Fix.** Do not store the entry in `LogException`; let the Unity callback capture it. Or ignore the callback while the logger itself forwards.

### GU6. The debug console store has no size limit

**Severity:** Low. **Status:** Confirmed.

**Where.** `WriteLogMessage` ([DebugConsoleLogger.cs:112](../../Assets/ModulesShared/Gui/Scripts/DebugConsole/DebugConsoleLogger.cs#L112)); `GetMessages` ([line 70](../../Assets/ModulesShared/Gui/Scripts/DebugConsole/DebugConsoleLogger.cs#L70)).

**Failure.** While the console is enabled, every `Trace` call and every exception adds an entry. Only opening the console panel clears the list, and the panel shows at most 100 entries. A mod with a repeated error (for example, a missing localization key on every frame) grows the list without limit. The panel also drops the older entries when it reads them, so reopening it shows nothing old.

**Fix.** Use a ring buffer of a fixed size. Keep the entries after reading, and let the panel show the last N.

### GU7. Theme changes do not reach UI that already exists

**Severity:** Low. **Status:** Plausible.

**Where.** `ThemedImage.Start` ([ThemedImage.cs:25](../../Assets/ModulesShared/Gui/Scripts/Theme/Wrappers/ThemedImage.cs#L25)), `ThemedText.Start` ([ThemedText.cs:27](../../Assets/ModulesShared/Gui/Scripts/Theme/Wrappers/ThemedText.cs#L27)), `CommonNode.DefaultColor` ([ComponentTreeNode.cs:308](../../Assets/ModulesShared/Gui/Scripts/ComponentList/ComponentTreeNode.cs#L308)).

**Failure.** `UiThemeLoader` imports new colors on every database load. The wrappers read the theme once in `Start`. `CommonNode.DefaultColor` is a `static readonly` field, read once per process. After the player loads a mod with other `UiSettings`, objects that already started keep the old colors, and the group icons in the ship editor keep the first theme until the game restarts. The effect depends on which scenes stay loaded across a mod change.

**Fix.** Fire a theme-changed signal from `UiThemeLoader` and let the wrappers reapply. Make `DefaultColor` a property that reads `UiTheme.Current`.

### GU8. The theme import changes the shared asset

**Severity:** Low. **Status:** Confirmed.

**Where.** `UiThemeLoader.OnDatabaseLoaded` ([UiThemeLoader.cs:33](../../Assets/ModulesShared/Gui/Scripts/Theme/UiThemeLoader.cs#L33)) calls `UiTheme.Import` ([UiTheme.cs:296](../../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs#L296)) on the asset object.

**Failure.** In the Unity Editor, a ScriptableObject asset keeps changes made in Play mode. After a mod session, `UiTheme.asset` holds the mod colors until the asset is reloaded. If anything marks the asset dirty and the project is saved, the mod colors are written to disk and committed. In player builds the asset is reloaded at every start, so there is no effect.

**Fix.** Import into a runtime copy: `UiTheme.Current = Instantiate(_defaultTheme); UiTheme.Current.Import(database);`.

### GU9. Group item counts are cached and go stale

**Severity:** Low. **Status:** Confirmed.

**Where.** [ComponentTreeNode.cs](../../Assets/ModulesShared/Gui/Scripts/ComponentList/ComponentTreeNode.cs): `WeaponNode._count` ([line 205](../../Assets/ModulesShared/Gui/Scripts/ComponentList/ComponentTreeNode.cs#L205)) is never reset; `CommonNode._count` ([line 302](../../Assets/ModulesShared/Gui/Scripts/ComponentList/ComponentTreeNode.cs#L302)) and `RootNode._count` are reset by `Add` but not by `Clear`.

**Failure.** The ship editor calls `RootNode.Assign` (clear, then add all) on every inventory change. The weapon group keeps the item count from the first list build of the editor session. A category group that becomes empty keeps its old count, because `Clear` does not reset it and no `Add` follows. The count decides whether a group is shown inline (fewer than 3 items), whether its button is active, and when `ComponentsPanel.RefreshList` moves up from an empty group. Example: the player installs every weapon from the inventory. The weapon group stays clickable and opens an empty list.

**Fix.** Reset the cache in `Clear` and in `WeaponNode.Add`, or compute the count without a cache. The trees are small.

### GU10. A `RootNode` without a database fails on weapons

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** `RootNode` constructor ([ComponentTreeNode.cs:16](../../Assets/ModulesShared/Gui/Scripts/ComponentList/ComponentTreeNode.cs#L16)).

**Failure.** The `database` parameter is optional. Without it, `_weaponNode` is `null`. `Add` of a weapon throws `NullReferenceException`, and `Nodes`, `Components`, `ItemCount`, and `Clear` enumerate a `null` child and throw. `SatellitesPanel` creates `new RootNode(null)` only to label a back button, and that path reads none of these members.

**Fix.** Make the database required, or skip the weapon node when it is `null`.

### GU11. An animated window reports "opened" twice

**Severity:** Low. **Status:** Plausible.

**Where.** `AnimatedWindowBase.OpenWindow` and `StartAnimation` ([AnimatedWindowBase.cs:13](../../Assets/ModulesShared/Gui/Scripts/Core/AnimatedWindowBase.cs#L13), [line 55](../../Assets/ModulesShared/Gui/Scripts/Core/AnimatedWindowBase.cs#L55)).

**Failure.**
1. `OpenWindow` activates the GameObject. `OnEnable` calls `StartAnimation`, which starts the wait coroutine.
2. `OpenWindow` then calls `StartAnimation` again. The coroutine is running, so this call goes to the `else` branch and calls `OnWindowOpened` at once.
3. When the animator reaches `Open`, the coroutine calls `OnWindowOpened` a second time.

`OnWindowOpenedEvent` runs twice. With `_waitOpenAnimationDone`, the opened signal fires before the animation and again after it, so the option has no effect. The second signal makes `GuiManager` apply the class rules again. The first call also sets `blocksRaycasts` to true during the animation. This assumes the animator does not start in state `Open` after activation.

**Fix.** In `StartAnimation`, do nothing when the coroutine is running; the coroutine already checks the target state.

### GU12. `WindowDestroyedSignal` is never fired

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** Handler `OnWindowDestroyed` ([GuiManager.cs:311](../../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs#L311)). No code fires the signal.

**Failure.** A window destroyed outside a scene unload stays registered. `OpenWindow` on it then calls a destroyed object and throws `MissingReferenceException`. Today windows are destroyed only with their scene, which the manager handles through `SceneBeforeUnloadSignal`.

**Fix.** Fire the signal from `OnDestroy` in `Window` and `AnimatedWindow`, or remove the signal and the handler.

### GU13. Escape picks an arbitrary window between equal classes

**Severity:** Low. **Status:** Confirmed.

**Where.** `Tick` ([GuiManager.cs:360](../../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs#L360)); `HasHigherClosePriority` ([WindowClass.cs:53](../../Assets/ModulesShared/Gui/Scripts/Core/WindowClass.cs#L53)).

**Failure.** `HasHigherClosePriority` returns `true` for any two classes that are not `Balloon` or `ModalDialog`. So between two such windows with `EscapeKeyAction.Close`, the last one in `HashSet` order wins. The set order is not defined, so the window that Escape closes can differ between runs. The rule also ignores which window opened last.

**Fix.** Track the open order and close the most recent eligible window.

## Structural issues

### GU14. The shared module carries game-specific code

**Problem.** The module is meant to be shared between two games, but it depends on Event Horizon content.

**Evidence.**
1. `Gui.ComponentList` depends on `ShipConstructor`, `Localization`, and the `WeaponSlots` settings. Only the ship editor uses it.
2. `UiTheme` depends on `Economy.Currency`, `ItemQuality`, the `UiSettings` record, and quest item 25 ([UiTheme.cs:346](../../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs#L346)).
3. `GuiManagerExtensions` hard-codes three window names that the application must provide.
4. The `AppConfiguration` reference in `Gui.asmdef` is not used.

**Direction.** Move `ComponentList` into the ShipEditor module. Move `UiTheme.Import` and the currency and quality lookups into the application. Remove the unused reference. Agree the change with Frontier first.

### GU15. The UI Extensions copy is almost unused

**Problem.** 191 C# files (about 31,700 lines) and 17 shaders compile into every build. The project uses one component.

**Evidence.** Only `FlowLayoutGroup` has a scene reference (`CommonGuiScene`). No code uses the `UnityEngine.UI.Extensions` namespace. The copy is version 2.3.2 from 2023, outside the Package Manager, so updates are manual.

**Direction.** Install the package through the Package Manager, or keep only `FlowLayoutGroup.cs` with its BSD license text.

### GU16. `ListScrollRect` is Asset Store code in a public repository

**Problem.** The `.meta` files of `Plugins/ListScrollRect` say `licenseType: Store`. The folder has no license file, author, or version. The `ModulesShared` repository is public under GPLv3.

**Evidence.** [ListScrollRect.cs.meta](../../Assets/ModulesShared/Gui/Plugins/ListScrollRect/Scripts/ListScrollRect.cs.meta).

**Direction.** Find the original package and check that its license allows redistribution of the source. If not, replace it with an open-source recycling list or move it to the private submodule. This is a legal question; this file does not answer it.
