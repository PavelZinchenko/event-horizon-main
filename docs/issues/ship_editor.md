# ShipEditor Module Issues

This file records problems in `Assets/ModulesShared/ShipEditor/` and in the application code that drives it. For a description of the current design, see [ShipEditor module](../ship_editor.md). Related issues in other files are not repeated here:

| ID | Topic |
| --- | --- |
| [DB15.17](database.md#db15-smaller-defects) | `ShipValidator.HasForbiddenComponents` is never called. |
| [DB15.18](database.md#db15-smaller-defects) | `ShipBuilder` counts limits before the compatibility check. The editor checks in the right order. |
| [GS5.15](game_session.md#gs5-smaller-defects) | `PresetStorage` crashes on a preset whose hull a mod removed. |

The analysis was done by reading the code, the scene, and the prefabs on 2026-10-12. No issue was reproduced in Play Mode. Each issue has a severity (High, Medium, or Low) and a status:

- **Confirmed** – the code path is certain. The failure follows directly from the code.
- **Confirmed (latent)** – the code path is certain, but current data or current UI does not trigger it.
- **Plausible** – the code is wrong, but the failure needs specific timing or UI conditions.

Defects use Where / Failure / Fix. Structural issues use Problem / Evidence / Direction.

## Defects

### SE1. Reselecting the installed satellite type empties it

**Severity:** Medium. **Status:** Confirmed.

**Where.** `TryInstallSatellite` starts with `if (_satellite[location] == satellite) return true;` ([ShipEditorModel.cs:150](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L150)). The left side is an `ISatellite`. The right side is a database `Satellite`. `Satellite` does not implement `ISatellite`, so this is a reference comparison that is always false, except when both are `null`.

**Failure.** In live mode, the player opens the satellite list for a side and taps the type that is already installed. The panel lists that type when the inventory has another copy. The model then:

1. Calls `RemoveSatellite`. All unlocked components on that satellite go back to the inventory.
2. Withdraws a satellite of the same type and installs it empty.

The player loses the satellite's layout. Undo cannot restore it, because `SatelliteChanged` clears the undo commands of that side. Locked components on that satellite are destroyed (see [SE2](#se2-removing-a-satellite-destroys-its-locked-components)).

**Fix.** Compare `_satellite[location]?.Information == satellite`.

### SE2. Removing a satellite destroys its locked components

**Severity:** Medium. **Status:** Confirmed.

**Where.** `RemoveSatellite` calls `RemoveAllComopnents(layout)` with `keepLocked = true` ([ShipEditorModel.cs:172](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L172), [line 374](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L374)). It then drops the layout and the `ISatellite` and returns only `satellite.Information` to the inventory ([line 175](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L175)). `ShipLayoutModel.RemoveAll(true)` leaves locked components in the layout and does not report them to the tracker ([ShipLayoutModel.cs:80](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipLayoutModel.cs#L80)).

A satellite has locked components when it comes from a build: `CommonShip` creates `CommonSatellite(build)`, and `FromDatabase` sets `Locked = true` ([CommonShip.cs:30](../../Assets/Modules/ShipConstructor/Scripts/Ships/CommonShip.cs#L30), [IntegratedComponent.cs:128](../../Assets/Modules/ShipConstructor/Scripts/IntegratedComponent.cs#L128)).

**Failure.**
1. The locked components disappear. They are not in the inventory and not on the ship.
2. The `ComponentTracker` still counts them. Per-component and group-tag limits stay used until the player selects another ship.
3. `LoadShipFromPreset` removes both satellites first ([line 288](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L288)), so loading any preset triggers the same loss. [SE1](#se1-reselecting-the-installed-satellite-type-empties-it) triggers it too.

`RemoveSatellite` also writes the components into the satellite object at [line 171](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L171) and then discards that object. The write has no effect.

**Fix.** Blocked by open question [SEQ1](#open-questions). Then either block removal of a satellite that holds locked components, or return the locked components to the inventory and report every removed component to the tracker.

### SE3. A failed satellite install removes the current satellite

**Severity:** Low. **Status:** Confirmed.

**Where.** `TryInstallSatellite` calls `RemoveSatellite` ([ShipEditorModel.cs:151](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L151)) before the size check ([line 155](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L155)) and the inventory withdrawal ([line 158](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L158)).

**Failure.** When the new satellite does not fit or is not in the inventory, the method returns `false` and the slot is empty. The UI disables incompatible items, so the main path is `LoadShipFromPreset` with a satellite the player no longer owns. The preset load then also reports a partial load, which is correct, but the old satellite is gone from the slot.

**Fix.** Check size and inventory first. Remove the old satellite only after both checks pass.

### SE4. A preset restores the locked flag

**Severity:** Low. **Status:** Confirmed.

**Where.** `InstallComponentsFromPreset` installs each component with `component.Locked` from the preset ([ShipEditorModel.cs:325](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L325)). `SaveShipToPreset` stores locked components with `Locked = true`.

**Failure.**
1. The player saves a preset while the ship has a locked component.
2. The player unlocks and removes that component. The item goes to the inventory.
3. The player loads the preset. The editor withdraws the item from the inventory and installs it as locked.

The player must pay the unlock price again to remove a component that they already own. The same happens with a preset made on another ship of the same hull.

**Fix.** Install components from a preset with `Locked = false`. A locked component that is still on the ship is already skipped by the `FindComponent` check at [line 315](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L315).

### SE5. Database mode loses satellite changes and shares satellite builds

**Severity:** Medium. **Status:** Confirmed.

**Where.** `EditorModeShip` writes only its component list to the build ([EditorModeShip.cs:44](../../Assets/Modules/ShipConstructor/Scripts/Ships/EditorModeShip.cs#L44)). `Database.SaveShipBuild` serializes only `Components` ([Database.cs:203](../../Assets/Modules/Database/Scripts/Database.cs#L203)). The `FirstSatellite` / `SecondSatellite` setters of `BaseShip` only set `DataChanged` ([BaseShip.cs:30](../../Assets/Modules/ShipConstructor/Scripts/Ships/BaseShip.cs#L30)). `EditorModeSatellite` writes its components to the shared `SatelliteBuild` ([EditorModeSatellite.cs](../../Assets/Modules/ShipConstructor/Scripts/Satellites/EditorModeSatellite.cs)).

**Failure.**
1. A database author installs, replaces, or removes a satellite on a ship build. The editor shows the change. After exit, the build's `LeftSatelliteBuild` / `RightSatelliteBuild` are unchanged.
2. A component change on an installed satellite build is written to that `SatelliteBuild`. Every ship build that references it changes too. The editor gives no warning.
3. `DatabaseInventoryProvider` creates one `EditorModeSatellite` per build ([DatabaseEditorContext.cs:65](../../Assets/ModulesShared/ShipEditor/Scripts/Context/DatabaseEditorContext.cs#L65)). The author can install the same object on both sides. Both layouts then edit one component collection, and the last save wins. The ship's own satellites are other objects for the same build, so they also overwrite each other.

**Fix.** Blocked by open question [SEQ2](#open-questions). Persist the satellite assignment in `SaveShipBuild` (add the satellite build fields), or disable satellite replacement in database mode. Create a new `EditorModeSatellite` per installation, and show which ship builds share a satellite build.

### SE6. Module mesh objects leak

**Severity:** Low. **Status:** Confirmed.

**Where.** `ShipModulesLayout.Cleanup` calls `Destroy(group.Layout)` ([ShipModulesLayout.cs:55](../../Assets/ModulesShared/ShipEditor/Scripts/Layout/ShipModulesLayout.cs#L55)). `group.Layout` is the `ShipLayoutElement` component returned by `Instantiate` ([line 35](../../Assets/ModulesShared/ShipEditor/Scripts/Layout/ShipModulesLayout.cs#L35)). `Destroy` on a component removes only that component.

**Failure.** Each `GenerateModules` call leaves one GameObject with a `MeshFilter` and a `MeshRenderer` per icon texture. `GenerateModules` runs on every component removal ([EditorShipLayout.cs:125](../../Assets/ModulesShared/ShipEditor/Scripts/Layout/EditorShipLayout.cs#L125)), every bulk change, every ship switch, and every satellite change. The objects render nothing, because `ShipLayoutElement.OnDestroy` destroys their mesh and materials. They stay in the scene until it unloads. A long session with many drags creates thousands of empty objects under each layout.

**Fix.** Call `Destroy(group.Layout.gameObject)`.

### SE7. The placement preview is skipped at the previous cell

**Severity:** Low. **Status:** Confirmed.

**Where.** `EditorShipLayout.ShowSelection` returns early when `position == _selectedPosition` ([EditorShipLayout.cs:105](../../Assets/ModulesShared/ShipEditor/Scripts/Layout/EditorShipLayout.cs#L105)). Hiding the preview (`component == null`) and `Initialize` do not reset `_selectedPosition`.

**Failure.** The player drops a component, then starts a new drag. While the pointer maps to the same cell as the last preview, no preview appears. The check also ignores the component: a different component at the same cell keeps no preview. The same happens after a ship or satellite switch.

**Fix.** Cache the component together with the position, and reset the cache when the preview is hidden or the layout changes.

### SE8. Component list lock icons can go stale after a ship switch

**Severity:** Low. **Status:** Plausible.

**Where.** `ComponentListItem.Initialize` returns early when the item already shows the same `ComponentInfo` ([ComponentListItem.cs:37](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentListItem.cs#L37)). The lock icon and button state come from `IsSuitable(ship)` and are computed only after that return. `ComponentsPanel` does not refresh the list on `ShipChanged` ([ComponentsPanel.cs:132](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentsPanel.cs#L132)).

**Failure.** The player switches from a small ship to a large one. A recycled list item keeps the lock and the disabled button from the previous ship, or the reverse. The model still rejects an invalid drop, so only the display is wrong. Whether an item is recycled with the same component depends on `ListScrollRect`.

**Fix.** Recompute `canInstall` before the early return, or cache the ship together with the component.

### SE9. Smaller defects

| ID | Where | Defect | Status |
| --- | --- | --- | --- |
| SE9.1 | [ComponentTracker.cs:112](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ComponentTracker.cs#L112) | The tracker stores one key per database `Component`. Removing one instance deletes the key while other instances remain. The next instance then gets a new free key instead of the shared one. | Confirmed |
| SE9.2 | [ControlsPanel.cs:36](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ControlsPanel.cs#L36) | For `ActivationType.None`, `Initialize` returns before it resets `_keyBinding` and `_componentMode`. `ComponentPanel.OnDragStarted` then stores the key and behaviour of the previously shown component on a passive component. The values have no effect in combat. | Confirmed |
| SE9.3 | [BuildsPanel.cs:29](../../Assets/ModulesShared/ShipEditor/Scripts/UI/BuildsPanel.cs#L29) | The panel listens to `ShipChanged` only while visible. After a ship switch, it keeps the old `_selectedItem`, and the load button stays visible. Load then fails the hull check ([ShipEditorModel.cs:284](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L284)) and shows `$PartiallyLoadedPreset` although nothing loaded. | Confirmed |
| SE9.4 | [DatabaseEditorContext.cs:41](../../Assets/ModulesShared/ShipEditor/Scripts/Context/DatabaseEditorContext.cs#L41) | `_satellites` is never filled. A database without satellite builds offers no satellites in database mode. | Confirmed (latent) |
| SE9.5 | [BarrelMapBuilder.cs:32](../../Assets/ModulesShared/ShipEditor/Scripts/Model/BarrelMapBuilder.cs#L32) | The inner loop runs x over `yMin..yMax`. Only stock weapon cells start a barrel, and they always lie in `0..Size-1`, which both ranges cover. The result is correct today. A non-square stock rectangle would skip barrels. | Confirmed (latent) |
| SE9.6 | [ICommand.cs:24](../../Assets/ModulesShared/ShipEditor/Scripts/Model/Commands/ICommand.cs#L24) | `CommandList` calls `DataChanged.Invoke()` without a null check (lines 24, 30, 38, 49). It throws when no window is subscribed. | Confirmed (latent) |
| SE9.7 | [ShipEditorModel.cs:210](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L210) | `UnlockComponent` throws `InvalidOperationException` when payment fails. In "unlock all", one failure stops the loop after partial payment. The buy dialog blocks this path today. | Confirmed (latent) |
| SE9.8 | [ShipEditorModel.cs:357](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L357) | `SaveShip` always assigns both satellites. The setters mark the ship as changed, so every ship that the editor opens is marked dirty. | Confirmed |
| SE9.9 | [ComponentImage.cs:23](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentImage.cs#L23) | `SetIcon` reads `icon.rect` without a null check. A component whose icon is missing (for example in a mod) throws when the player drags it. | Confirmed (latent) |
| SE9.10 | [ComponentImage.cs:71](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentImage.cs#L71) | `OnPopulateMesh` writes `vertex.position.y` into z. It should keep `vertex.position.z`. | Confirmed |
| SE9.11 | [ComponentItem.cs:219](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentItem.cs#L219) | The magazine suffix uses Cyrillic `х` (U+0445), not Latin `x` or `×`. Fonts without Cyrillic show a missing glyph. | Confirmed |
| SE9.12 | [ComponentItem.cs:180](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentItem.cs#L180) | Two different rows use the label `$DroneReconstructionTime` (lines 180 and 182): one shows seconds, the other a multiplier. | Confirmed |
| SE9.13 | [ShipEditorScene.unity:21402](../../Assets/ModulesShared/ShipEditor/Scenes/ShipEditorScene.unity#L21402) | UnityEvents call methods that do not exist: `DeleteButtonPressed`, `UnlockAllButtonPressed` (line 29668), `UnlockButtonPressed` (line 30728), six `SetObserveMode` calls (lines 26652–26702), and `ConfigureControls` in [TextButton.prefab:96](../../Assets/ModulesShared/ShipEditor/Prefabs/TextButton.prefab#L96). Unity skips them at run time. They hide real bindings in the Inspector. | Confirmed |
| SE9.14 | [CameraController.cs:42](../../Assets/ModulesShared/ShipEditor/Scripts/UI/CameraController.cs#L42) | `AspectFromFocus` divides by zero when the focus anchor is at the top or bottom screen edge. | Confirmed (latent) |
| SE9.15 | [SatelliteItem.cs:30](../../Assets/ModulesShared/ShipEditor/Scripts/UI/SatelliteItem.cs#L30) | The `Satellite` overload does not clear `SatelliteBuild`. A reused item that showed a build keeps installing that build. Each context lists only one kind today. | Confirmed (latent) |
| SE9.16 | [ShipLayoutElement.cs:47](../../Assets/ModulesShared/ShipEditor/Scripts/Layout/ShipLayoutElement.cs#L47) | Uses `DestroyImmediate` at run time for meshes and materials (lines 47, 56). `Destroy` is the run-time API. | Confirmed |
| SE9.17 | [ComponentItem.cs:302](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentItem.cs#L302) | Dead code: `GetWeaponDamageText(Ammunition, …)`, `FormatInt` ([line 343](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentItem.cs#L343)), the unused `count` in `ModuleMeshBuilder.ComponentRect`, the unused `using UnityEngine.Rendering` in [ComponentTracker.cs:8](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ComponentTracker.cs#L8), commented-out logs in `DatabaseEditorContext`, and the typo `RemoveAllComopnents`. | Confirmed |

## Structural issues

### SE10. Placement rules live in the UI module and have copies

**Problem.** The rules that decide whether a ship is valid live in the `ShipEditor` assembly, next to Unity UI code. Two other copies of similar rules exist in ShipConstructor. A rule change in one place does not reach the others.

**Evidence.**
- `ShipValidator` in the application builds `ShipLayoutModel` and `ComponentTracker` to validate every player ship and every editable build ([ShipValidator.cs:28](../../Assets/Scripts/Domain/Player/ShipValidator.cs#L28), [line 87](../../Assets/Scripts/Domain/Player/ShipValidator.cs#L87)).
- `ShipBuilder.Build` repeats the tag and count limits in a different order ([ShipBuilder.cs:91](../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L91), see DB15.18).
- `ShipLayoutObsolete` has its own placement and barrel logic and is still used for satellite builds at startup ([InitializationState.cs:118](../../Assets/Scripts/GameStateMachine/States/InitializationState.cs#L118)).
- `ItemDescriptionPanel` in the crafting UI calls `ShipEditor.UI.ComponentItem.GetDescription` ([ItemDescriptionPanel.cs:122](../../Assets/Scripts/Legacy/GUI/ViewModel/Craft/ItemDescriptionPanel.cs#L122)).

**Direction.** Move `ShipLayoutModel`, `ComponentTracker`, `BarrelMapBuilder`, and `CellIndex` into ShipConstructor as the single placement and limit rule set. Let `ShipBuilder` use the same tracker. Move `GetDescription` to a shared Gui or ShipConstructor helper. Retire `ShipLayoutObsolete`.

### SE11. Statistics duplicate ShipBuilder and omit combat bonuses

**Problem.** The statistics panel recomputes ship stats with its own copy of the builder logic. It does not apply the bonuses that combat applies. The player sees different numbers in the editor and in battle.

**Evidence.** `ShipStatsPanel.UpdateStats` copies the energy-dependent installation order of `ShipBuilder.Build` ([ShipStatsPanel.cs:112](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipStatsPanel.cs#L112)). It sets only `BaseStats`. Combat also sets `Bonuses` from ship experience ([BaseShip.cs:63](../../Assets/Modules/ShipConstructor/Scripts/Ships/BaseShip.cs#L63)) and from player skills ([ShipBuilderExtensions.cs:10](../../Assets/Scripts/Combat/Domain/ShipBuilderExtensions.cs#L10)). The panel also skips the builder's tag/limit filter and `IsSuitable` filter. The editor enforces those rules, so this matters only for invalid data.

**Direction.** Build stats through `ShipBuilder` with the same bonuses as combat, or label the panel as base stats. Blocked by open question [SEQ3](#open-questions).

### SE12. Undo covers placement only

**Problem.** The undo list records only single-component placement and removal. Other edits either cannot be undone or wipe the history.

**Evidence.** Key binding, behaviour, and unlock call the model directly ([ComponentPanel.cs:52](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentPanel.cs#L52)). Remove all, preset load, and satellite removal fire `MultipleComponentsChanged`, which clears the list ([ShipEditorWindow.cs:335](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipEditorWindow.cs#L335)). A satellite change clears that side ([line 332](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipEditorWindow.cs#L332)). The most destructive actions are therefore the ones without undo. "Remove all" has a confirmation dialog; satellite replacement has none.

**Direction.** Add commands for satellite changes, remove all, and preset load (store a snapshot of exported components and satellites). Then stop clearing the list on bulk events.

### SE13. No tests for pure model code

**Problem.** The model is plain C# with no Unity dependency except `Vector2Int`, but nothing tests it. The defects above in SE1–SE4 and SE9.1 would show in small unit tests.

**Evidence.** The module has no test assembly. `ShipLayoutModel`, `ComponentTracker`, `BarrelMapBuilder`, `CommandList`, and `ShipEditorModel` (with a fake `IShipEditorContext`) need no scene.

**Direction.** Add an edit-mode test assembly for the model. Cover placement at layout edges, weapon classes, limits across hull and satellites, inventory conservation, locked components, presets, and undo.

## Open questions

These questions wait for a design decision. Do not fix the linked issues until the owner answers.

| ID | Question | Blocks | Status |
| --- | --- | --- | --- |
| SEQ1 | Should a player be able to remove a satellite that carries locked components? | [SE2](#se2-removing-a-satellite-destroys-its-locked-components) | Open |
| SEQ2 | In database mode, should satellite replacement persist to the ship build, or should the editor block it? | [SE5](#se5-database-mode-loses-satellite-changes-and-shares-satellite-builds) | Open |
| SEQ3 | Should the statistics panel show combat values with skill bonuses, or base values? | [SE11](#se11-statistics-duplicate-shipbuilder-and-omit-combat-bonuses) | Open |
