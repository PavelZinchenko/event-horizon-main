# ShipEditor Module

The ShipEditor module is the screen where a player or a database author fits a ship. The player places components on the hull and on two satellites, sets key bindings and drone behaviour, unlocks built-in components, and saves presets. The module owns the scene, the editing model, the undo list, the UI, and the runtime meshes. The application supplies the ship, the inventory, and persistence through one context interface.

| | |
| --- | --- |
| Assembly | `ShipEditor` ([ShipEditor.asmdef](../Assets/ModulesShared/ShipEditor/ShipEditor.asmdef)). References: CommonComponents, Database, Zenject, ServicesFacade, ShipConstructor, Gui, Localization, Diagnostics, Utilites. |
| Namespaces | `ShipEditor` (meshes, signal), `ShipEditor.Model`, `ShipEditor.Context`, `ShipEditor.UI`. The installer is in `Installers`. |
| Source | [`Assets/ModulesShared/ShipEditor/Scripts/`](../Assets/ModulesShared/ShipEditor/Scripts/) (49 files, about 5,400 lines) |
| Scene | [`ShipEditorScene.unity`](../Assets/ModulesShared/ShipEditor/Scenes/ShipEditorScene.unity), loaded as `GameScene.ShipEditor` |
| Application adapter | [`ShipEditorState`](../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs) |
| Other consumers | [`ShipValidator`](../Assets/Scripts/Domain/Player/ShipValidator.cs) (layout validation), [`ItemDescriptionPanel`](../Assets/Scripts/Legacy/GUI/ViewModel/Craft/ItemDescriptionPanel.cs) (component stats text) |
| Data | [Component](database/schema/Component.md), [ComponentGroupTag](database/schema/ComponentGroupTag.md), [Satellite](database/schema/Satellite.md), ship and satellite builds |
| Tests | None. |
| Known issues | [ShipEditor issues](issues/ship_editor.md). |

## Contents

1. [Terms](#terms)
2. [Source map](#source-map)
3. [Composition](#composition)
4. [Operating modes](#operating-modes)
5. [Context interfaces](#context-interfaces)
6. [Editor model](#editor-model)
7. [Layout model and placement rules](#layout-model-and-placement-rules)
8. [Barrel map and weapon classes](#barrel-map-and-weapon-classes)
9. [Component tracker: limits and default keys](#component-tracker-limits-and-default-keys)
10. [Satellites](#satellites)
11. [Saving](#saving)
12. [Presets](#presets)
13. [Locked components and unlocking](#locked-components-and-unlocking)
14. [Commands and undo](#commands-and-undo)
15. [Events](#events)
16. [UI](#ui)
17. [Input flow](#input-flow)
18. [Key bindings and component modes](#key-bindings-and-component-modes)
19. [Statistics panel](#statistics-panel)
20. [Meshes and rendering](#meshes-and-rendering)
21. [Coordinates](#coordinates)
22. [Use outside the editor](#use-outside-the-editor)
23. [Extending the module](#extending-the-module)
24. [Validation checklist](#validation-checklist)

## Terms

| Term | Meaning |
| --- | --- |
| Element | One of three editable parts: the ship hull, the left satellite, or the right satellite. Enum `ShipElementType` (`Ship`, `SatelliteL`, `SatelliteR`). |
| Layout | The cell grid of an element. Each cell has a `CellType`: Outer, Inner, InnerOuter, Engine, Weapon, or Empty. |
| Custom cell | A cell that the player added through hull expansion. `*` (`CustomizableCell`) is a free slot. `X` (`CustomWeaponCell`) is an added weapon cell. |
| Component | A database `Component` with a modification and quality, wrapped in `ComponentInfo`. |
| Placed component | A component on a layout. The model keeps it as `ComponentModel`. The ship stores it as `IntegratedComponent`. |
| Barrel | A weapon mount of the hull or satellite. A connected group of weapon cells belongs to one barrel. |
| Locked component | A placed component that came with the ship build. The player cannot remove it until they unlock it. |
| Context | The `IShipEditorContext` object that the application binds. It supplies the ship, the inventory, and persistence. |
| Live mode | The editor opened from the star map on a player ship. |
| Database mode | The editor opened from the main menu on a ship build of an editable database. |

## Source map

| File | Role |
| --- | --- |
| [Context/IShipEditorContext.cs](../Assets/ModulesShared/ShipEditor/Scripts/Context/IShipEditorContext.cs) | `IShipEditorContext`, `IInventoryProvider`, `IShipPresetStorage`. |
| [Context/ShipDataProvider.cs](../Assets/ModulesShared/ShipEditor/Scripts/Context/ShipDataProvider.cs) | `IShipDataProvider`, `ShipGameObjectData`, `EmptyDataProvider`. |
| [Context/DatabaseEditorContext.cs](../Assets/ModulesShared/ShipEditor/Scripts/Context/DatabaseEditorContext.cs) | Context for database mode and for the scene fallback. |
| [Installer/ShipEditorSceneInstaller.cs](../Assets/ModulesShared/ShipEditor/Scripts/Installer/ShipEditorSceneInstaller.cs) | Zenject bindings of the scene. Creates a test context when the application bound none. |
| [Signals.cs](../Assets/ModulesShared/ShipEditor/Scripts/Signals.cs) | `CloseEditorSignal`. |
| [Model/ShipEditorModel.cs](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs) | `IShipEditorModel` and `ShipEditorModel`: ship selection, installation, satellites, saving, presets, unlocking. |
| [Model/ShipLayoutModel.cs](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipLayoutModel.cs) | One element: cell occupancy, placement rules, component list. |
| [Model/ComponentTracker.cs](../Assets/ModulesShared/ShipEditor/Scripts/Model/ComponentTracker.cs) | Ship-wide limits, ship compatibility, satellite size, default keys. |
| [Model/BarrelMapBuilder.cs](../Assets/ModulesShared/ShipEditor/Scripts/Model/BarrelMapBuilder.cs) | Maps every weapon cell to a barrel index. |
| [Model/ComponentModel.cs](../Assets/ModulesShared/ShipEditor/Scripts/Model/ComponentModel.cs) | `IComponentModel`, `ComponentModel`, `ComponentSettings`. |
| [Model/ShipEditorEvents.cs](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorEvents.cs) | Model events and their triggers. |
| [Model/ShipElementContainer.cs](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipElementContainer.cs) | Struct with one slot per element. Used by the model and by `ShipView`. |
| [Model/Extensions.cs](../Assets/ModulesShared/ShipEditor/Scripts/Model/Extensions.cs) | `CellIndex` (packs x, y into a `ulong`) and element/location conversion. |
| [Model/Commands/](../Assets/ModulesShared/ShipEditor/Scripts/Model/Commands/) | `ICommand`, `CommandList`, `InstallComponentCommand`, `RemoveComponentCommand`. |
| [Layout/EditorShipLayout.cs](../Assets/ModulesShared/ShipEditor/Scripts/Layout/EditorShipLayout.cs) | Renders one element: hull, components, locks, selection, weapon classes, ship image. |
| [Layout/ShipMeshBuilder.cs](../Assets/ModulesShared/ShipEditor/Scripts/Layout/ShipMeshBuilder.cs) | Hull cell mesh. |
| [Layout/ModuleMeshBuilder.cs](../Assets/ModulesShared/ShipEditor/Scripts/Layout/ModuleMeshBuilder.cs), [ShipModulesLayout.cs](../Assets/ModulesShared/ShipEditor/Scripts/Layout/ShipModulesLayout.cs) | Component icon meshes, one mesh per icon texture. |
| [Layout/LockedCellsMeshBuilder.cs](../Assets/ModulesShared/ShipEditor/Scripts/Layout/LockedCellsMeshBuilder.cs) | Lock icons on locked cells. |
| [Layout/SelectionMeshBuilder.cs](../Assets/ModulesShared/ShipEditor/Scripts/Layout/SelectionMeshBuilder.cs) | Green/red placement preview. |
| [Layout/WeaponClassesMeshBuilder.cs](../Assets/ModulesShared/ShipEditor/Scripts/Layout/WeaponClassesMeshBuilder.cs), [WeaponClassLayout.cs](../Assets/ModulesShared/ShipEditor/Scripts/Layout/WeaponClassLayout.cs) | Weapon class letters on free weapon cells. |
| [Layout/ShipLayoutElement.cs](../Assets/ModulesShared/ShipEditor/Scripts/Layout/ShipLayoutElement.cs) | Owns one runtime mesh and its materials. |
| [UI/ShipEditorWindow.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipEditorWindow.cs) | Main controller. Connects input, panels, model, view, camera, undo. |
| [UI/ShipView.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipView.cs) | Holds three `EditorShipLayout` objects. Converts world points to cells. |
| [UI/ComponentsPanel.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentsPanel.cs), [ComponentListItem.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentListItem.cs) | Inventory component tree. |
| [UI/ComponentPanel.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentPanel.cs), [ComponentItem.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentItem.cs), [ComponentActionPanel.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentActionPanel.cs) | Details of one component, actions, stat text. |
| [UI/ControlsPanel.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ControlsPanel.cs), [RadioGroup.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/RadioGroup.cs) | Key binding, automatic mode, drone behaviour. |
| [UI/SatellitesPanel.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/SatellitesPanel.cs), [SatelliteItem.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/SatelliteItem.cs) | Satellite choice for one side. |
| [UI/ShipsPanel.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipsPanel.cs), [ShipListContentFiller.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipListContentFiller.cs), [ShipItem.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipItem.cs) | Ship list. |
| [UI/BuildsPanel.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/BuildsPanel.cs), [ShipPresetItem.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipPresetItem.cs) | Presets. |
| [UI/ShipStatsPanel.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipStatsPanel.cs), [NameValueItem.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/NameValueItem.cs) | Ship statistics. |
| [UI/NavigationPanel.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/NavigationPanel.cs) | Pointer input on the ship area: click, pan, pinch, scroll, long-press drag. |
| [UI/DraggableComponent.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/DraggableComponent.cs), [DragHandler.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/DragHandler.cs), [ComponentImage.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentImage.cs) | The dragged component icon. |
| [UI/CameraController.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/CameraController.cs), [CanvasTransformHelper.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/CanvasTransformHelper.cs), [Background.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/Background.cs) | Smoothed camera with an off-centre focus, screen/world helpers, nebula background. |

Assets: [`Prefabs/`](../Assets/ModulesShared/ShipEditor/Prefabs/) (layout, right panel, stats rows, buttons, labels), [`Materials/`](../Assets/ModulesShared/ShipEditor/Materials/) (`ShipCell.shader` and four materials), [`Textures/`](../Assets/ModulesShared/ShipEditor/Textures/) (cell, selection, and letter sprites).

## Composition

```text
Star map / main menu
   │  OpenShipEditorSignal(IShip)
   ▼
ShipEditorState.InstallBindings ──► IShipEditorContext, CloseEditorSignal
   │  loads ShipEditorScene (child of LoaderScene, contract "services")
   ▼
ShipEditorSceneInstaller ──► IShipEditorContext (fallback only), ShipEditorModel, CommandList
ObjectFactoryInstaller   ──► IGameObjectFactory
   ▼
ShipEditorModel ── ShipLayoutModel ×3 ── ComponentTracker (shared)
   │ events                    │
   ▼                           ▼
ShipEditorWindow, panels    ShipView ── EditorShipLayout ×3 ── mesh builders
   │ Exit
   ▼
SaveShip ──► IShip / ShipBuild JSON;  CloseEditorSignal ──► ShipEditorState loads NextState
```

The scene's `SceneContext` runs two installers: [`ObjectFactoryInstaller`](../Assets/Scripts/Installers/ObjectFactoryInstaller.cs) and [`ShipEditorSceneInstaller`](../Assets/ModulesShared/ShipEditor/Scripts/Installer/ShipEditorSceneInstaller.cs). Its parent contract is `services`, so the root container in `LoaderScene` supplies `IDatabase`, `IGuiManager`, `ILocalization`, `IResourceLocator`, `ISoundPlayer`, and the other services.

`ShipEditorSceneInstaller.InstallBindings` ([line 20](../Assets/ModulesShared/ShipEditor/Scripts/Installer/ShipEditorSceneInstaller.cs#L20)) makes three bindings:

1. `IShipEditorContext` from `CreateTestContext`, with `IfNotBound()`. The state normally binds the context first, so this binding is skipped.
2. `BindInterfacesTo<ShipEditorModel>().AsSingle()`. This binds `IShipEditorModel` and `IDisposable`. Zenject calls `Dispose` when the scene unloads, and `Dispose` saves the ship.
3. `CommandList` as a single instance.

`ShipEditorState.InstallBindings` ([line 41](../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs#L41)) runs before the scene installers through `SceneContext.ExtraBindingsInstallMethod` (see [Application layer](application_layer.md#per-state-bindings)). It binds the context for the selected mode, the `CloseEditorSignal`, and its trigger.

`GameTime` pauses game time while `StateType.ShipEditor` is active ([GameTime.cs:65](../Assets/Scripts/Services/InternetTime/GameTime.cs#L65)).

### Entry and exit

| Entry | Code | Ship | Mode | Next state |
| --- | --- | --- | --- | --- |
| Star map hangar | [StarMapState.cs:184](../Assets/Scripts/GameStateMachine/States/StarMapState.cs#L184) | Player ship from the fleet | Live | The same `StarMapState` instance |
| Main menu "constructor" button | [MainMenu.cs:82](../Assets/Scripts/Gui/MainMenu/MainMenu.cs#L82), [MainMenuState.cs:99](../Assets/Scripts/GameStateMachine/States/MainMenuState.cs#L99) | `EditorModeShip` for the build ID typed in the input field, or the first build | Database | The same `MainMenuState` instance |
| Scene opened directly | [ShipEditorSceneInstaller.cs:27](../Assets/ModulesShared/ShipEditor/Scripts/Installer/ShipEditorSceneInstaller.cs#L27) | Build `_testShipBuildId` (scene value 271; code default 316), or the first build | Fallback `DatabaseEditorContext` | None |

The main menu shows the constructor button only when `IDatabase.IsEditable` is true ([MainMenu.cs:173](../Assets/Scripts/Gui/MainMenu/MainMenu.cs#L173)).

Exit:

1. The back button calls `ShipEditorWindow.Exit` ([line 162](../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipEditorWindow.cs#L162)). In overview mode, the first press only leaves overview mode.
2. `Exit` calls `IShipEditorModel.SaveShip` and fires `CloseEditorSignal`.
3. `ShipEditorState.OnExit` calls `LoadState(NextState)`. The state machine replaces the editor state with the state that opened it.
4. The scene unloads. Zenject disposes `ShipEditorModel` (a second `SaveShip`) and, in live mode, `PresetStorage` (writes presets to the session).

The fallback scene has no `CloseEditorSignal` binding. `Exit` then saves and does nothing else. With `_lockTestShipModules` = 0 (the scene value), the fallback ship is an `EditorModeShip`, so saving writes to the build's JSON file. The standalone path was not run in Play Mode for this note.

## Operating modes

| Aspect | Live mode | Database mode |
| --- | --- | --- |
| Context | `ShipEditorState.ShipEditorContext` ([line 70](../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs#L70)) | [`DatabaseEditorContext`](../Assets/ModulesShared/ShipEditor/Scripts/Context/DatabaseEditorContext.cs) |
| Ship type | `CommonShip` from the player fleet | `EditorModeShip` wrapping a `ShipBuild` |
| Ship list | `PlayerFleet.Ships` | Every `ShipBuild` as a new `EditorModeShip` |
| Components | `PlayerInventory.Components` with real quantities | Every database component, plus every possible modification at quality P3; quantity is always 999 |
| Satellites | `PlayerInventory.Satellites` with quantities; no satellite builds | Every `SatelliteBuild` as `EditorModeSatellite`; the plain satellite list is empty |
| Inventory changes | Real: install withdraws, removal returns | None: every call succeeds and changes nothing |
| Ship name | Editable | Not editable; `EditorModeShip.Name` is the build ID |
| Locked components | Kept from the ship | `EditorModeShip` clears `Locked` on every component |
| Unlock | Price = 2 × component price in credits; requires the component technology (and research when `RequiredToUnlock`) | Price = 100 credits, never charged; requires that a component technology exists |
| Presets | `PresetStorage` over `ISessionData.ShipPresets`, written at scene unload | In-memory list, lost at exit |
| Upgrades provider | Returns no upgrades | Returns no upgrades |
| Ship display data | `EmptyDataProvider` | `EmptyDataProvider` |
| Where saves go | `IShip.Components` and satellites of the player ship; the session saves them later | `ShipBuild` and `SatelliteBuild` JSON files through `IDatabase.SaveShipBuild` / `SaveSatelliteBuild` (see [Database](database.md)) |

In database mode, only component lists persist. `Database.SaveShipBuild` writes `Components` only. A change of satellite on a build is lost at exit. Component changes on an installed satellite build are written to that `SatelliteBuild`, which all ship builds that use it share. See [SE5](issues/ship_editor.md#se5-database-mode-loses-satellite-changes-and-shares-satellite-builds).

## Context interfaces

All interfaces are in [IShipEditorContext.cs](../Assets/ModulesShared/ShipEditor/Scripts/Context/IShipEditorContext.cs) and [ShipDataProvider.cs](../Assets/ModulesShared/ShipEditor/Scripts/Context/ShipDataProvider.cs).

`IShipEditorContext`:

| Member | Use |
| --- | --- |
| `IShip Ship` | The ship that the editor opens first. |
| `IInventoryProvider Inventory` | Ships, satellites, components, quantities, unlock payment. |
| `IShipDataProvider ShipDataProvider` | Optional display position, rotation, size, and image flag for a ship. |
| `IShipPresetStorage ShipPresetStorage` | Preset list per hull. |
| `IComponentUpgradesProvider UpgradesProvider` | Component upgrades for stat display. Both contexts return none. |
| `bool CanBeUnlocked(Component)` | Unlock rule for non-`Common` components. |
| `bool IsShipNameEditable` | Enables the name field. |

`IInventoryProvider`:

| Member | Called by |
| --- | --- |
| `Ships` | Ship list. The window hides the ships button when it is empty. |
| `SatelliteBuilds` | Satellite list. When it is not empty, the panel shows builds and ignores `Satellites`. |
| `Satellites`, `GetQuantity(Satellite)` | Satellite list in live mode. |
| `TryRemoveSatellite`, `AddSatellite` | Satellite install and removal. |
| `Components`, `GetQuantity(ComponentInfo)` | Component tree and component panel. |
| `TryRemoveComponent`, `AddComponent` | Install withdraws one item. Removal returns one item. |
| `GetUnlockPrice`, `TryPayForUnlock` | Unlock dialog and unlock action. |

`IShipPresetStorage` has `GetPresets(Ship hull)`, `Create(Ship hull)`, and `Delete(IShipPreset)`. Presets belong to a hull (`Ship` data record), not to one player ship.

`IShipDataProvider.TryGet(IShip, out ShipGameObjectData)` returns position, rotation, size, and `HasImage`. When `HasImage` is true, the window does not draw the ship sprite. Both shipped contexts use `EmptyDataProvider`, whose `Default` is all zeros. The window then draws the sprite at scale 1.

## Editor model

`IShipEditorModel` ([ShipEditorModel.cs:27](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L27)) is the only API that the UI uses to change the ship. `ShipEditorModel` keeps:

- `_ship`: the selected `IShip`.
- `_layout`: a `ShipElementContainer<ShipLayoutModel>` with one layout per element. A satellite slot without a satellite holds `null`.
- `_satellite`: a `ShipElementContainer<ISatellite>` for the two satellites.
- `_compatibilityChecker`: one `ComponentTracker` for the whole ship. All three layouts report to it.
- `_shipName`: the edited name, or `null` when unchanged.

### Ship selection

The constructor calls `SelectShip(context.Ship)`. The ship list calls it again for another ship. `SelectShip` ([line 122](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L122)):

1. Saves the previous ship (see [Saving](#saving)).
2. Creates a new `ComponentTracker` for the new ship and clears the edited name.
3. Creates the hull `ShipLayoutModel` from `ship.Model.Layout` and `ship.Model.Barrels`.
4. Installs the ship's components with `InitializeLayout`.
5. Creates the satellite layouts with `InitializeSatellite` for both sides. Each call fires `SatelliteChanged`.
6. Fires `ShipChanged`.

`InitializeLayout` ([line 391](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L391)) checks each stored component with `IsSuitableLocation` and `ComponentTracker.IsCompatible`. It logs and skips a component that fails. A skipped component is not returned to the inventory. The fleet normally contains no such components, because `PlayerFleet` runs `ShipValidator.RemoveInvalidParts` when a session loads ([PlayerFleet.cs:96](../Assets/Scripts/Domain/Player/PlayerFleet.cs#L96)). At the end, `InitializeLayout` resets `DataChanged`, so an untouched layout is not written back.

The constructor runs before any UI object subscribes. The window therefore rebuilds the view itself in `Start` after one frame ([ShipEditorWindow.cs:85](../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipEditorWindow.cs#L85)).

### Installing a component

`TryInstallComponent(element, position, info, settings)` ([line 216](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L216)) checks, in this order:

1. The element has a layout.
2. `ComponentTracker.IsCompatible(component)`: ship restrictions and limits.
3. `ShipLayoutModel.IsSuitableLocation(x, y, component)`: every cell of the component footprint.
4. `Inventory.TryRemoveComponent(info)`: the inventory has one item.

When all checks pass, the layout installs the component and the model fires `ComponentAdded`. `position` is the top-left cell of the component's square layout in element coordinates.

### Removing a component

`RemoveComponent(component)` ([line 236](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L236)) throws `InvalidOperationException` for a locked component. Otherwise it removes the component from its layout, returns the item to the inventory, and fires `ComponentRemoved`.

`RemoveAllComponents` ([line 246](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L246)) removes every unlocked component from all three elements and returns the items. It fires one `MultipleComponentsChanged`. Locked components stay.

### Changing settings

`SetComponentKeyBinding` and `SetComponentBehaviour` ([line 182](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L182)) replace `ComponentSettings` when the value differs. They fire `ComponentModified`. These changes are not undoable.

## Layout model and placement rules

`ShipLayoutModel` ([ShipLayoutModel.cs](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipLayoutModel.cs)) wraps an `IShipLayout` from ShipConstructor. The hull uses the ship model's layout, which includes hull expansions (`LayoutModifications.CustomLayout`). A satellite uses `ShipLayoutAdapter` over the database `Layout`.

### Layout rectangle

`IShipLayout.Rect` is a `LayoutRect` with inclusive bounds. A satellite rectangle is `0..Size-1` on both axes. A hull with expansions can extend one cell past each edge that has a stock cell, so `xMin` and `yMin` can be −1 and `xMax` and `yMax` can be `Size` ([LayoutModifications.cs:117](../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/LayoutModifications.cs#L117)). The rectangle is not always square.

`Cell(x, y)` ([line 36](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipLayoutModel.cs#L36)) maps custom cells for the editor: `X` becomes `Weapon`, and `*` becomes `Empty`.

### Indexes

- `_components` is a `List<ComponentModel>`. A component's `Id` is its index in this list. `RemoveComponent` swaps the last component into the freed index and updates its `Id` ([line 119](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipLayoutModel.cs#L119)). An `Id` is therefore stable only until the next removal.
- `_filledCells` maps each occupied cell (`CellIndex.FromXY`) to its component. Click selection and overlap checks use it.
- `HasComponent` accepts a model only when `_components[model.Id]` is the same object. A removed model fails this check.

### Placement rule

`IsSuitableLocation(x, y, component)` ([line 177](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipLayoutModel.cs#L177)) iterates the component's square `Layout`. It skips empty cells and calls `IsCellCompatible(x + j, y + i, component)` for every other cell. Only the component's `CellType` matters. The per-cell letters of the component layout are not compared with the hull cells.

`IsCellCompatible` ([line 153](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipLayoutModel.cs#L153)) returns true when all these hold:

1. The cell is inside `Rect`.
2. No component occupies the cell.
3. If the cell and the component are both `Weapon`:
   - A component without `WeaponSlotType` fits any weapon cell.
   - Otherwise the cell must belong to a barrel. The barrel's `WeaponClass` must be empty or contain the slot letter.
4. Otherwise `component.CellType.CompatibleWith(cellType)` ([CellType.cs](../Assets/Modules/ShipConstructor/Scripts/CellType.cs)):
   - Empty and `*` cells accept nothing.
   - A component with `CellType.Empty` fits any non-empty cell.
   - Equal types fit.
   - `InnerOuter` cells accept `Inner` and `Outer` components. `InnerOuter` components fit `Inner` and `Outer` cells.

`Component.CellType` is marked obsolete in the schema but still drives this rule (see [DB9](issues/database.md#db9-live-fields-are-marked-obsolete)).

## Barrel map and weapon classes

`BarrelMapBuilder` ([BarrelMapBuilder.cs](../Assets/ModulesShared/ShipEditor/Scripts/Model/BarrelMapBuilder.cs)) assigns a barrel index to every weapon cell. `ShipLayoutModel` builds it once in its constructor.

1. The builder scans the rectangle row by row ([line 31](../Assets/ModulesShared/ShipEditor/Scripts/Model/BarrelMapBuilder.cs#L31)).
2. Each unassigned stock `Weapon` cell starts a new barrel, until the barrel count reaches the number of barrels in the data.
3. A breadth-first fill from that cell gives the same barrel to 4-connected stock weapon cells and to adjacent custom `X` cells.
4. An `X` cell joins a barrel but does not spread it further, and it never starts a barrel.
5. Weapon cells after the last data barrel stay unassigned (`-1`).

The barrel order is therefore the row-major order of the first cell of each group. Database authors must list `Barrels` in that order.

`GetBarrelId(component)` returns the barrel of the first non-empty cell of the component footprint. `ExportComponents` stores it in `IntegratedComponent.BarrelId`. Combat uses this value to attach the weapon to a `WeaponPlatform` ([ShipBuilder.cs:155](../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L155)). A weapon spanning two barrels mounts on the first one.

The weapon class overlay draws each letter of a barrel's `WeaponClass` on every free weapon cell of that barrel (see [Meshes and rendering](#meshes-and-rendering)).

## Component tracker: limits and default keys

`ComponentTracker` ([ComponentTracker.cs](../Assets/ModulesShared/ShipEditor/Scripts/Model/ComponentTracker.cs)) implements two interfaces:

- `IComponentTracker`: layouts report `OnComponentAdded`, `OnComponentRemoved`, and `OnKeyBindingChanged`.
- `ICompatibilityChecker`: the model and the UI ask `IsCompatible`, `ComponentLimitReached`, and `GetDefaultKey`.

One tracker serves the hull and both satellites. A limit therefore counts components on all three elements.

| Rule | Code | Source data |
| --- | --- | --- |
| Ship size, mechanic/organic, built-in device class | `Constructor.Component.CompatibilityChecker.IsCompatibleComponent` ([CompatibilityChecker.cs](../Assets/Modules/ShipConstructor/Scripts/Component/CompatibilityChecker.cs)) | `Component.Restrictions`, ship model |
| Per-component maximum | `ComponentLimitReached` ([line 41](../Assets/ModulesShared/ShipEditor/Scripts/Model/ComponentTracker.cs#L41)) | `Restrictions.MaxComponentAmount` (> 0) |
| Group tag maximum | `ComponentLimitReached` | `Restrictions.ComponentGroupTag.MaxInstallableComponents` |
| Satellite size | `IsCompatible(Satellite)` → `IShip.IsSuitableSatelliteSize` ([ShipExtensions.cs:16](../Assets/Modules/ShipConstructor/Scripts/Extensions/ShipExtensions.cs#L16)) | Starbases accept no satellite. Otherwise `SizeClass`, or model scale ≥ 2 × satellite scale. |

`IsCompatible(Component)` combines the first three rows. The UI uses it to enable dragging and to show "already installed" or "not compatible".

### Default key

`GetDefaultKey(component)` ([line 68](../Assets/ModulesShared/ShipEditor/Scripts/Model/ComponentTracker.cs#L68)):

1. Returns 0 for components with `ActivationType.None`.
2. Returns the key already used by another component of the same database `Component`.
3. Otherwise returns the lowest key in 0–5 that no tracked component uses.
4. Returns 0 when all six keys are used.

The tracker stores one key per database `Component`, not per placed instance. `OnComponentRemoved` deletes that entry even when other instances remain ([SE9](issues/ship_editor.md#se9-smaller-defects)).

## Satellites

The ship has two satellite slots: `SatelliteLocation.Left` and `Right`. Each slot holds an `ISatellite` and, when present, a `ShipLayoutModel` built from `satellite.Information.Layout` and `Barrels`.

| Operation | Code | Behaviour |
| --- | --- | --- |
| Install from inventory | `TryInstallSatellite(location, Satellite)` ([line 148](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L148)) | Removes the current satellite, checks size, withdraws one satellite, creates an empty `CommonSatellite`. |
| Install a build | `InstallSatellite(location, ISatellite)` ([line 142](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L142)) | Removes the current satellite and installs the given object with its components. No size check and no inventory change. Used in database mode. |
| Remove | `RemoveSatellite(location)` ([line 165](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L165)) | Saves the satellite's components into the satellite object, returns unlocked components to the inventory, returns the satellite, fires `MultipleComponentsChanged` and `SatelliteChanged`. |

The satellite panel disables incompatible items, so the UI never calls `InstallSatellite` with an incompatible build. The components panel hides the satellite groups for starbases and when the inventory has no satellites and the slot is empty ([ComponentsPanel.cs:136](../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentsPanel.cs#L136)).

Known defects: selecting the satellite type that is already installed empties the slot ([SE1](issues/ship_editor.md#se1-reselecting-the-installed-satellite-type-empties-it)). Removal destroys locked satellite components ([SE2](issues/ship_editor.md#se2-removing-a-satellite-destroys-its-locked-components)).

## Saving

`SaveShip()` ([line 343](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L343)) runs on ship switch, on `Exit`, and on `Dispose`.

1. When `_shipName` is not empty, it writes `IShip.Name`.
2. When the hull layout has `DataChanged`, it replaces `IShip.Components` with `ExportComponents(hull)` and clears the flag.
3. For each side, `SaveSatellite` writes the satellite's components when that layout has `DataChanged`. It then assigns the satellite to `FirstSatellite` / `SecondSatellite`. This assignment always runs and always marks the ship as changed.

`ExportComponents` ([line 361](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L361)) sorts components by `Y * 10000 + X` and creates one `IntegratedComponent` per component with: `ComponentInfo`, X, Y, barrel ID, key binding, behaviour, and the locked flag.

The ship object decides where the data goes:

- `CommonShip` keeps it in memory. The session serializer saves the fleet later.
- `EditorModeShip` and `EditorModeSatellite` listen to their component collection. Each `Assign` writes the build to its JSON file at once ([EditorModeShip.cs:44](../Assets/Modules/ShipConstructor/Scripts/Ships/EditorModeShip.cs#L44)).

## Presets

A preset ([ShipPreset.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/ShipPreset.cs)) stores a hull, a name, the hull components, and two optional satellites with their components.

| Action | Code | Behaviour |
| --- | --- | --- |
| Save new | `BuildsPanel.SavePreset` ([line 60](../Assets/ModulesShared/ShipEditor/Scripts/UI/BuildsPanel.cs#L60)) | `Presets.Create(hull)`, then `SaveShipToPreset`, then the name from the input field. |
| Overwrite | Same method, with a selected preset | Asks for confirmation, then `SaveShipToPreset`. |
| Load | `IShipEditorModel.LoadShipFromPreset` ([line 282](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L282)) | See below. |
| Delete | `BuildsPanel.DeletePreset` | Asks for confirmation, then `Presets.Delete`. |
| Rename | `ShipPresetItem.OnTextChanged` | Writes `IShipPreset.Name` on each keystroke. |

`LoadShipFromPreset`:

1. Returns `false` when the preset hull differs from `Ship.Model.OriginalShip`.
2. Removes all unlocked components and both satellites, returning them to the inventory.
3. Installs each hull component from the preset. It skips a component that is already at that position (a kept locked component). It counts a failure when the location, the compatibility check, or the inventory withdrawal fails.
4. Installs each preset satellite with `TryInstallSatellite`, then its components.
5. Fires `SatelliteChanged` for both sides and `MultipleComponentsChanged`.
6. Returns `false` when any part failed. The UI then shows `$PartiallyLoadedPreset`.

Loading is best effort: the editor loads what the inventory allows. Preset loading also restores each component's locked flag ([SE4](issues/ship_editor.md#se4-a-preset-restores-the-locked-flag)).

In live mode, `PresetStorage` reads `ISessionData.ShipPresets` when the scene loads and writes the whole list back in `Dispose` ([ShipEditorState.cs:107](../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs#L107)). See [GS5.15](issues/game_session.md#gs5-smaller-defects) for a preset whose hull a mod removed.

## Locked components and unlocking

A locked component comes with a ship build. `ComponentExtensions.FromDatabase` creates every build component with `Locked = true` ([IntegratedComponent.cs:128](../Assets/Modules/ShipConstructor/Scripts/IntegratedComponent.cs#L128)). `CommonShip` and `CommonSatellite` keep that flag. `EditorModeShip` and `EditorModeSatellite` clear it.

The editor treats a locked component as follows:

- Remove, drag, and "remove all" skip it. `RemoveComponent` throws on it.
- The lock mesh draws a lock icon on each of its cells.
- The component panel shows "unlock" and "unlock all" instead of "remove" ([ComponentPanel.cs:102](../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentPanel.cs#L102)).

`CanBeUnlocked(model)` ([ShipEditorModel.cs:195](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L195)) returns true for a locked `Availability.Common` component. For other components, it asks the context.

`UnlockComponent(model)` ([line 205](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L205)) calls `Inventory.TryPayForUnlock` and clears the flag. It throws when the rule or the payment fails. The buy dialog disables its confirm button when the player cannot pay ([BuyConfirmationDialog.cs:24](../Assets/Scripts/Gui/Dialogs/BuyConfirmationDialog.cs#L24)), so the throw is not reached in normal use.

"Unlock all" sums `GetUnlockPrice` of every unlockable component on all three elements into one credit price. After confirmation, it unlocks each component and pays per component.

## Commands and undo

`CommandList` ([ICommand.cs:13](../Assets/ModulesShared/ShipEditor/Scripts/Model/Commands/ICommand.cs#L13)) is a linear undo stack. It has no redo.

| Command | Execute | Rollback |
| --- | --- | --- |
| `InstallComponentCommand` | `TryInstallComponent(element, position, info, settings)` | Finds a component with the same position and `ComponentInfo` and removes it. |
| `RemoveComponentCommand` | `RemoveComponent(model)` | Reinstalls `model.Info` at the model's position with its saved settings. |

Rules:

- `TryExecute` adds the command only when it succeeds.
- `Undo` pops the last command, then calls `TryRollback`. A failed rollback is dropped silently.
- `Clear()` empties the list. `Clear(element)` removes the commands of one element.
- Every change fires `DataChanged`. The window uses it to enable the undo button.

| Trigger | Effect on the undo list |
| --- | --- |
| `ShipChanged` | `Clear()` |
| `SatelliteChanged(side)` | `Clear(side)` |
| `MultipleComponentsChanged` (remove all, preset load, satellite removal) | `Clear()` |
| Key binding, behaviour, unlock | Not recorded |

Only placement from the inventory, drag from the ship, and removal from the component panel are undoable.

## Events

`IShipEditorEvents` ([ShipEditorEvents.cs](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorEvents.cs)) are plain C# events. Subscribers attach in `OnEnable` and detach in `OnDisable`.

| Event | Fired by | Subscribers |
| --- | --- | --- |
| `ShipChanged(IShip)` | `SelectShip` | Window (view, name, camera, undo), components panel (satellite groups), satellites panel, builds panel, stats panel |
| `SatelliteChanged(side)` | `InitializeSatellite`, `RemoveSatellite`, `LoadShipFromPreset` | Window (satellite view, undo), components panel, satellites panel |
| `ComponentAdded(model)` | `TryInstallComponent` | Window (view, install sound), components panel, component panel, stats panel |
| `ComponentRemoved(model)` | `RemoveComponent` | Window (view), components panel, component panel (closes), stats panel |
| `ComponentModified(model)` | Key binding, behaviour, unlock | Window (lock mesh), component panel (refresh) |
| `MultipleComponentsChanged` | `RemoveAllComponents`, `RemoveSatellite`, `LoadShipFromPreset` | Window (rebuilds all meshes, clears undo), components panel, stats panel |

`InitializeLayout` and `InstallComponentsFromPreset` install components without per-component events. The bulk event that follows covers them.

## UI

### Panels

The right side shows one panel at a time. `ShipEditorWindow.ShowPanel` ([line 257](../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipEditorWindow.cs#L257)) switches between them.

| Panel | Class | Content |
| --- | --- | --- |
| Component list | `ComponentsPanel` | Inventory tree from `Gui.ComponentList` (`RootNode`), with two extra groups for the satellite slots. Quantities come from the inventory. |
| Component | `ComponentPanel` | One component: icon, name, modification, stats, required cell, size, controls, action buttons. |
| Satellite list | `SatellitesPanel` | Satellites or satellite builds for one side, plus "remove satellite". |
| Ship list | `ShipsPanel` | `Inventory.Ships`. A click calls `SelectShip`. |
| Build list | `BuildsPanel` | Presets of the current hull. |

The stats panel (`ShipStatsPanel`) is always visible, in a mini or full form.

### Component panel states

`ComponentActionPanel.Status` decides which labels and buttons appear.

| Opened from | Condition | Status | Drag handle |
| --- | --- | --- | --- |
| Inventory item | `IsCompatible` | `CanInstall` | Shown |
| Inventory item | Limit reached | `AlreadyInstalled` | Hidden |
| Inventory item | Otherwise | `NotCompatible` | Hidden |
| Installed component | Not locked | `CanRemove` | Shown when compatible and quantity > 0 (drags a new copy) |
| Installed component | Locked and unlockable | `Locked` (unlock, unlock all) | Same rule |
| Installed component | Locked, not unlockable | `None` | Same rule |

The panel closes when a component is removed. It also closes when an install leaves the shown component at quantity 0.

### Component list item

`ComponentListItem.Initialize` ([line 32](../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentListItem.cs#L32)) shows the quantity. It disables the button and shows a lock icon when `IComponent.IsSuitable(ship)` fails. This check covers restrictions only, not limits. Items can be dragged straight onto the ship.

### Component description

`ComponentItem.GetDescription` ([line 123](../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentItem.cs#L123)) builds the stat rows: armour, energy, shield, engine, weapon multipliers, resistances, drone multipliers, turret turn rate, weapon damage per damage type, drone bay data, and weight. It creates the component for the current hull size, because some stats scale with ship size. The legacy crafting UI calls this static method too.

## Input flow

`NavigationPanel` ([NavigationPanel.cs](../Assets/ModulesShared/ShipEditor/Scripts/UI/NavigationPanel.cs)) covers the ship area. It forwards input to the window through UnityEvents.

| Gesture | NavigationPanel | ShipEditorWindow |
| --- | --- | --- |
| Tap without drag | `_clicked(worldPoint)` | `OnClick`: selects the component under the point on any element and opens the component panel. Ignored in overview mode. |
| One-finger drag | `_moved(delta)` each frame, with inertia after release | `OnMove`: moves the camera. The camera stays inside the rotated bounding box of the ship and satellites. |
| Press ≥ 0.1 s, then drag | `_dragged(eventData)` once | `OnDrag`: picks the component under the press point. If it is unlocked, executes `RemoveComponentCommand` and starts a drag with its key binding and behaviour. |
| Pinch or mouse wheel | `_zoomed(factor)` | `OnZoom`: clamps the orthographic size between `_cameraZoomMin` and `_cameraZoomMax`, both multiplied by the view scale. |

Drag and drop:

1. A list item or the component panel calls `DraggableComponent.Initialize(content, eventData)`. `DragHandler` starts this only after the pointer moves more than 10% of the screen. The draggable takes over the pointer (`eventData.pointerDrag`).
2. Each `OnDrag` moves the icon and calls `ShipEditorWindow.ShowPlacement`. `ShipView.ShowSelection` draws a green/red preview on all three elements.
3. `OnEndDrag` calls `ShipEditorWindow.DropComponent`. The window hides the preview, then tries `InstallComponentCommand` on the hull, then the left satellite, then the right satellite. The first success wins.
4. A drop that fits nowhere does nothing. A component that the player dragged off the ship is then back in the inventory, and undo can restore it.

Overview mode (`SetOverviewMode`) moves the camera focus to the screen centre and zooms to the whole ship. Clicks and drags are ignored in this mode.

The name field calls `OnNameChanged` on each edit. The model stores the name only when it differs from the localized initial name and the context allows editing.

## Key bindings and component modes

`IntegratedComponent.KeyBinding` is an `int` with two meanings ([ControlsPanel.cs:33](../Assets/ModulesShared/ShipEditor/Scripts/UI/ControlsPanel.cs#L33)):

| Activation type | Value | Meaning | UI |
| --- | --- | --- | --- |
| `None` | Ignored | Passive component | Controls panel hidden |
| `Manual` | 0–5 | Action key index | Key radio group. A new component gets `GetDefaultKey`. |
| `Mixed` | ≥ 0 | Manual on that key | "Automatic" toggle off, key radio group |
| `Mixed` | < 0 | Automatic. `-value` is the slider level. The label shows `(level − 1) × 10` % minimum energy. | "Automatic" toggle on, slider |

A new `Mixed` component starts in automatic mode at level 1 (`KeyBinding = -1`).

`Behaviour` holds the drone bay mode index from the drone behaviour radio group. Other components ignore it.

Changes in the controls panel call `ComponentPanel.OnKeyBindingChanged`. For an installed component, it sets both key and behaviour on the model. For an inventory component, the values travel with the dragged component.

## Statistics panel

`ShipStatsPanel.UpdateStats` ([line 102](../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipStatsPanel.cs#L102)) recomputes all stats on `ShipChanged`, `ComponentAdded`, `ComponentRemoved`, and `MultipleComponentsChanged`.

1. It creates a `ShipStatsCalculator` with the hull's base stats.
2. It creates an `IComponent` for every installed component on all three elements.
3. It adds components that need no passive energy first.
4. It adds each energy-dependent component only when the current recharge rate covers its consumption. Otherwise it adds the negative stats only.
5. It writes the summary (hit points, energy and recharge, velocity, turn rate) and the full list.

This order copies `ShipBuilder.Build` ([ShipBuilder.cs:91](../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L91)). Like combat, it includes satellite components in the ship totals. Unlike combat, it applies no experience bonus and no player skill bonuses ([SE11](issues/ship_editor.md#se11-statistics-duplicate-shipbuilder-and-omit-combat-bonuses)). Red values flag zero or negative armour, recharge, engine power, and turn rate.

## Meshes and rendering

Each element is one `EditorShipLayout` ([EditorShipLayout.cs](../Assets/ModulesShared/ShipEditor/Scripts/Layout/EditorShipLayout.cs)) from the `ShipLayout` prefab. It owns these child renderers:

| Layer | Builder | Rebuilt when |
| --- | --- | --- |
| Hull cells | `ShipMeshBuilder` | `Initialize` |
| Component icons | `ShipModulesLayout` + `ModuleMeshBuilder` | Add: the icon's texture group only. Remove, bulk change, `Initialize`: all groups. |
| Lock icons | `LockedCellsMeshBuilder` | `GenerateModules`, `ComponentModified` |
| Placement preview | `SelectionMeshBuilder` | Each drag update that moves to another cell |
| Weapon class letters | `WeaponClassLayout` + `WeaponClassesMeshBuilder` | `Initialize`; add and remove update a cell set |
| Ship image | `SpriteRenderer` | `Initialize` |

Details:

- `ShipMeshBuilder` shares vertices between neighbour cells of the same type and stores the cell type colour in vertex colours. An `InnerOuter` cell becomes two triangles with the outer and inner colours.
- `ShipModulesLayout` groups components by icon texture. Each group has its own `ShipLayoutElement` with one material, so every texture costs one draw call.
- `ModuleMeshBuilder` sizes each icon to the component's square layout, corrects the aspect ratio, and centres it on the bounding box of the non-empty cells. For a packed sprite, it reads the UV bounds from the sprite.
- `WeaponClassesMeshBuilder` starts with every stock weapon cell that has a class. It removes cells that a component covers and adds them back on removal. It draws one letter quad per class letter, using the `letters` sprites `A`, `B`, ….
- `ShipLayoutElement` destroys its mesh and materials when it gets a new mesh and in `OnDestroy`.
- The sprite of the hull is scaled to `OriginalSize` cells and offset by the layout rectangle, so it lines up with expanded layouts.

`ShipView` places the satellites left and right of the hull, `_satelliteOffset` units apart. The view's scale, rotation, and position come from `IShipDataProvider`.

`CameraController` builds its own projection matrix so the visible centre follows a UI `RectTransform` ("focus"). It smooths position, rotation, zoom, and focus with `SmoothDamp`. `Background` scales a nebula quad to the camera and scrolls it with the camera.

## Coordinates

| Space | Origin and axes |
| --- | --- |
| Cell | Integer `(x, y)` in the layout rectangle. y grows downward. |
| Element local | `EditorShipLayout._content` is at `(-Width/2, Height/2)`. A cell `(x, y)` spans `((x − xMin)·cell, −(y − yMin)·cell)` to the next corner. |
| View local | `ShipView` transform: position, rotation, uniform scale. |
| World | Camera world space. Input converts screen points with `Camera.main.ScreenToWorldPoint`. |

`ShipView.WorldToCell(point, element, size)` ([ShipView.cs:52](../Assets/ModulesShared/ShipEditor/Scripts/UI/ShipView.cs#L52)) converts a world point to view space, subtracts the element offset, and subtracts half the component size. It rounds to the nearest cell and adds `xMin`/`yMin`. The result is the top-left cell for a component of that size centred on the pointer. Clicks use size 1.

`CanvasTransformHelper.GetCellSize` measures one world unit in canvas pixels, times the view scale. The drag icon uses it so the icon matches the cell grid. It assumes `ShipView._cellSize` = 1, which the scene uses.

## Use outside the editor

The application uses editor classes as shared rules:

- [`ShipValidator`](../Assets/Scripts/Domain/Player/ShipValidator.cs) builds `ShipLayoutModel` and `ComponentTracker` to validate player ships (`IsLayoutValid`, `RemoveInvalidParts`). `PlayerFleet` runs it when a session loads. `InitializationState` runs it on every build of an editable database. The placement and limit rules in this module are therefore the rules for every saved ship.
- [`ItemDescriptionPanel`](../Assets/Scripts/Legacy/GUI/ViewModel/Craft/ItemDescriptionPanel.cs) calls `ComponentItem.GetDescription` for the crafting screen.
- `ShipBuilder` has its own limit logic for combat, and `ShipLayoutObsolete` has its own placement logic. They are separate copies of these rules ([SE10](issues/ship_editor.md#se10-placement-rules-live-in-the-ui-module-and-have-copies)).

## Extending the module

| Change | Where |
| --- | --- |
| New placement rule | `ShipLayoutModel.IsCellCompatible`. The same rule then applies to `ShipValidator`. Check `ShipBuilder` and `ShipLayoutObsolete` for copies. |
| New ship-wide limit | `ComponentTracker.ComponentLimitReached` and its add/remove counters. Mirror it in `ShipBuilder.Build`. |
| New undoable action | Implement `ICommand` in `Model/Commands/`. Execute it through `CommandList.TryExecute`. Make `BelongsToElement` correct so satellite changes clear it. |
| New model operation | Add it to `IShipEditorModel`, change inventory through `IInventoryProvider`, and fire a model event. Use `MultipleComponentsChanged` for bulk changes. |
| New panel | Add a `PanelType` and a `Visible` property, add it to `ShowPanel`, and subscribe to model events in `OnEnable`/`OnDisable`. |
| New game mode | Implement `IShipEditorContext` in the application. Bind it in the state's `InstallBindings` before the scene loads. |
| New stat row | `ShipStatsPanel.UpdateStats`, plus a `NameValueItem` in the scene. |
| New rendering layer | Add a builder in `Layout/` and a `ShipLayoutElement` child to the `ShipLayout` prefab. Keep `.meta` files with the assets. |

Serialized fields and UnityEvent bindings live in `ShipEditorScene.unity` and the prefabs. A renamed public method silently breaks its binding. Several bindings in the scene already point to missing methods ([SE9](issues/ship_editor.md#se9-smaller-defects)).

## Validation checklist

The module has no tests. Check model behaviour and the rendered scene together.

1. Open the editor in live mode from the star map and in database mode from the main menu.
2. Install, drag, remove, and undo components on the hull and both satellites.
3. Place multi-cell components at every edge of an expanded hull. Try occupied cells, wrong cell types, weapon classes, per-component limits, and group-tag limits across hull and satellites.
4. Count inventory items before and after install, removal, satellite change, ship switch, preset load, remove all, and exit.
5. Open a ship that has locked components on the hull and on a satellite. Check lock icons, unlock price, unlock all, and that locked components survive remove all.
6. Save and load presets with and without satellites, and with missing inventory items.
7. Exit, reopen, and check positions, modifications, key bindings, automatic levels, drone behaviour, locks, name, and satellites.
8. In database mode, check the changed build JSON files.
9. Check pan, pinch, mouse wheel, overview mode, long-press drag, and the placement preview on touch and mouse, at several aspect ratios.
