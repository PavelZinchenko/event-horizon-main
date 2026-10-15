# Diagnostics Module

The Diagnostics module gives the project one logging entry point that does not depend on `UnityEngine.Debug` directly. It has two loggers with different build behavior: `Debug` logs only in the Unity Editor, and `Trace` logs in every build and can be redirected. It also has an editor-only `Assert` class that nothing uses.

| | |
| --- | --- |
| Assembly | `Diagnostics` ([Diagnostics.asmdef](../Assets/ModulesShared/Diagnostics/Diagnostics.asmdef)). No references. Auto-referenced. |
| Namespace | `GameDiagnostics` |
| Location | [`Assets/ModulesShared/Diagnostics/`](../Assets/ModulesShared/Diagnostics/), inside the `Assets/ModulesShared` Git submodule. |
| Source | [`Scripts/`](../Assets/ModulesShared/Diagnostics/Scripts/) (3 files, 100 lines) |
| Assemblies that use it | Database, GameSession, ShipConstructor, BattleSimulator, Localization, Gui, ShipEditor, and the application (`Assets/Scripts`). |
| Redirection | [`DebugConsoleLogger`](gui.md#debug-console) in the Gui module. |
| Tests | None. |
| Known issues | [Diagnostics issues](issues/diagnostics.md) |

## Contents

1. [Shared code constraint](#shared-code-constraint)
2. [Source map](#source-map)
3. [Debug and Trace](#debug-and-trace)
4. [Redirecting Trace](#redirecting-trace)
5. [Assert](#assert)
6. [Use in the project](#use-in-the-project)
7. [Which call to use](#which-call-to-use)

## Shared code constraint

`Assets/ModulesShared` is a separate Git repository ([event-horizon-modules](https://github.com/PavelZinchenko/event-horizon-modules)). Its README says it holds "components shared between Event Horizon and Frontier". Frontier is another game by the same developer. This repository does not contain Frontier, so the effect of a change on Frontier cannot be checked here.

For Diagnostics the constraint is light. The assembly has no references, so it compiles in any Unity project. Do not add a reference to an Event Horizon assembly. Do not change the meaning of `Debug` or `Trace`; both games compile against them.

## Source map

| File | Contents |
| --- | --- |
| [Debug.cs](../Assets/ModulesShared/Diagnostics/Scripts/Debug.cs) | `ILogger` interface; static `Debug` (editor only) and `Trace` (all builds). |
| [UnityLogger.cs](../Assets/ModulesShared/Diagnostics/Scripts/UnityLogger.cs) | `UnityLogger`: the default `ILogger`. Forwards to `UnityEngine.Debug`. `UnityLogger.Instance` is a shared instance. |
| [Assert.cs](../Assets/ModulesShared/Diagnostics/Scripts/Assert.cs) | Editor-only assertions that throw `InvalidOperationException`. |

## Debug and Trace

`ILogger` has four methods: `Log`, `LogWarning`, `LogError`, and `LogException`. Each takes an optional `GameObject` context, which Unity uses to highlight the object in the Console.

| | `GameDiagnostics.Debug` | `GameDiagnostics.Trace` |
| --- | --- | --- |
| Builds | Unity Editor only. Every method has `[Conditional("UNITY_EDITOR")]`. | All builds. |
| Effect in a player build | The compiler removes the call and the evaluation of its arguments. | The message goes to the current logger. |
| Logger | Always `UnityLogger.Instance`. | `Trace.Logger`. Default `UnityLogger.Instance`. Can be replaced. |
| Context object | Supported. | Not passed. `Trace` methods have no context parameter. |

A conditional method is a method that the C# compiler removes at the call site when the named symbol is not defined. `UNITY_EDITOR` is defined only when the Unity Editor compiles the scripts. So `Debug.LogError` in a player build costs nothing and shows nothing.

`GameDiagnostics.Debug` has the same name as `UnityEngine.Debug`. Code that imports both namespaces must use the full name. The project always writes `GameDiagnostics.Debug.LogError(...)` and `GameDiagnostics.Trace.LogError(...)` in full.

## Redirecting Trace

`Trace.Logger` is a static property. Its getter returns the assigned logger, or `UnityLogger.Instance` when none is assigned. Setting it to `null` restores the default.

The only code that sets it is [`DebugConsoleLogger`](gui.md#debug-console):

1. When the in-game debug console is enabled, it sets `Trace.Logger` to itself.
2. Each call stores a copy of the message for the console, then forwards it to `UnityLogger.Instance`.
3. When the console is disabled, it sets `Trace.Logger` back to `null`.

So `Trace` messages always reach the Unity log. With the console enabled, they also reach the in-game console. `Debug` messages never reach the in-game console.

The property is static and not thread-safe. Set it only from the main thread.

## Assert

`Assert` has `IsTrue`, `IsFalse`, `AreEqual(uint, uint)`, `Less` (for `uint` and `int`), and `Greater(uint, uint)`. Each is conditional on `UNITY_EDITOR`. A failed check throws `InvalidOperationException` with a short text such as `Condition is false: 3 < 2`.

No code calls `GameDiagnostics.Assert`. The 11 assertion sites in the project use `UnityEngine.Assertions.Assert` instead. See [DG2](issues/diagnostics.md#dg2-assert-is-unused-and-ignores-its-message).

## Use in the project

Counts are from a search of `Assets/` on 2026-10-12.

| Call | Count | Where |
| --- | --- | --- |
| `Trace.LogError` | 46 | Database (loading, JSON parse errors, invalid images and colors, migration), GameSession (unknown format, missing EOF marker), ShipConstructor (invalid builds), Localization (missing key), Gui component list, ShipEditor limits, application domain (inventory, ship validation, products). |
| `Trace.LogWarning` | 8 | Database and session migration steps. |
| `Trace.Log` | 2 | `InitializationState` progress. |
| `Trace.LogException` | 1 | `Database.TryLoad`. |
| `Debug.*` | 7 | `UiTheme` (missing font), `ThemedImage` and `ThemedText` (theme errors), `ShipValidator` (component replaced), `TravelState` (3 progress lines). |

Most code in the project still calls `UnityEngine.Debug` directly. Those messages reach the Unity log, but the in-game console captures only exceptions from them (see [Debug console](gui.md#debug-console)).

## Which call to use

1. Use `Trace.LogError` for a data or content error that a mod author must see. This includes bad database records and failed loads. With the debug console enabled, the player sees it in the game.
2. Use `Trace.LogWarning` for a recoverable condition that is useful in a player log, such as a migration step.
3. Use `GameDiagnostics.Debug` for developer-only messages that must cost nothing in a release build. Do not build expensive strings outside the call; the argument is removed with the call, but a string built in a local variable is not.
4. Do not use `GameDiagnostics.Assert`. Use `UnityEngine.Assertions.Assert`, which the project already uses, or throw an exception.
