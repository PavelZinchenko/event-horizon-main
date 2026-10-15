# Diagnostics Module Issues

This file records problems in `Assets/ModulesShared/Diagnostics/`. For the design, see [Diagnostics module](../diagnostics.md).

The analysis was done by reading the code on 2026-10-12. Each issue has a severity (High, Medium, or Low) and a status:

- **Confirmed** – the code path is certain.
- **Confirmed (latent)** – the code path is certain, but current data or settings do not trigger it.
- **Plausible** – the code is wrong, but the failure needs specific conditions.

The module is in the `ModulesShared` submodule, which Frontier also uses. A fix there changes both games.

## Defects

### DG1. The in-game console sees only part of the log

**Severity:** Low. **Status:** Confirmed.

**Where.** [Debug.cs:28](../../Assets/ModulesShared/Diagnostics/Scripts/Debug.cs#L28) (`Trace`); about 270 direct `UnityEngine.Debug.Log*` calls in `Assets/Scripts`, `Assets/Modules`, and `Assets/ModulesShared`.

**Failure.** Only `Trace` calls reach the in-game debug console as messages. The console also captures exceptions from the Unity log. A direct `UnityEngine.Debug.LogError` and every `GameDiagnostics.Debug` call never reach it. A mod author who enables the console therefore misses most errors. For example, the window manager reports a duplicate window ID with `UnityEngine.Debug.Log` ([GuiManager.cs:251](../../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs#L251)).

**Fix.** Route content errors through `Trace`. Or let `DebugConsoleLogger` capture `LogType.Error` and `LogType.Warning` from `Application.logMessageReceivedThreaded`, and drop its own copy in `Trace` calls to avoid duplicates (see [GU5](gui.md#gu5-the-debug-console-records-trace-exceptions-twice)).

### DG2. `Assert` is unused and ignores its message

**Severity:** Low. **Status:** Confirmed.

**Where.** [Assert.cs:6](../../Assets/ModulesShared/Diagnostics/Scripts/Assert.cs#L6).

**Failure.**
1. No code calls `GameDiagnostics.Assert`. The project uses `UnityEngine.Assertions.Assert` in 11 places.
2. `IsTrue(condition, message)` throws with the fixed text "Condition is false" and drops `message`.

**Fix.** Delete the class, or pass `message` to `Throw`. Check Frontier first, because the class is public in a shared assembly.

## Structural issues

### DG3. Two loggers with the same name as Unity's

**Problem.** `GameDiagnostics.Debug` hides `UnityEngine.Debug` when both namespaces are imported. The difference in build behavior (editor only against all builds) is not visible at the call site.

**Evidence.** All call sites write the namespace in full to avoid the clash. `ShipValidator` reports a replaced component with `GameDiagnostics.Debug.LogError` ([ShipValidator.cs:118](../../Assets/Scripts/Domain/Player/ShipValidator.cs#L118)), so a player build replaces the component without any log line.

**Direction.** Rename `Debug` to a name that states the scope, such as `EditorLog`. Use `Trace` for any message that matters in a player build.
