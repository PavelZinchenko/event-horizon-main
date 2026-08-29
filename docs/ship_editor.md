# ShipEditor Module

The ShipEditor module provides the scene, model, UI, and mesh-building tools used to configure ships and their two satellite slots. It lets a player or database author select a ship, place compatible components on cell layouts, edit key bindings and behaviors, unlock components, manage satellites and presets, inspect statistics, and undo placement changes.

The module lives in `Assets/ModulesShared/ShipEditor/` and compiles as the `ShipEditor` assembly with the `ShipEditor` root namespace. It depends on ShipConstructor for mutable ship abstractions and layout rules, Database for catalog records, Gui and ServicesFacade for presentation services, and a context interface for game-specific inventory and persistence.

## Runtime Flow

```text
ShipEditorState (Assets/Scripts) or standalone scene fallback
                         |
                 IShipEditorContext
      ship + inventory + presets + unlocks + view data
                         |
              ShipEditorSceneInstaller
                         |
          ShipEditorModel + CommandList
                         |
       ShipLayoutModel / ComponentTracker / events
                         |
          ShipEditorWindow and child panels
                         |
         EditorShipLayout mesh builders + prefabs
                         |
             save back to IShip / session presets
```

`Assets/ModulesShared/ShipEditor/Scenes/ShipEditorScene.unity` contains the reusable editor scene. `ShipEditorSceneInstaller` binds `ShipEditorModel` and `CommandList`. The owning game state normally binds an `IShipEditorContext` before the scene installer runs; if none is present, the installer creates a database-backed test context from its configured ship build ID.

## Directory Map

| Path | Purpose |
| --- | --- |
| `Scripts/Context/` | Integration contracts plus the catalog-backed `DatabaseEditorContext` and optional ship presentation data. |
| `Scripts/Installer/` | Zenject bindings and the standalone/test-context fallback. |
| `Scripts/Model/` | Editor state, ship/satellite layouts, placed components, compatibility tracking, events, barrel mapping, and commands. |
| `Scripts/Layout/` | Runtime mesh generation for hull cells, modules, locked cells, selection feedback, and weapon-class overlays. |
| `Scripts/UI/` | Main window, panels, lists, drag/drop, camera/navigation, ship view, satellite controls, presets, and statistics. |
| `Scenes/ShipEditorScene.unity` | Reusable additive scene loaded by `ShipEditorState`. |
| `Prefabs/` | Layout, panels, buttons, stats rows, group rows, and shared label prefabs. |
| `Materials/` and `Textures/` | Cell, lock, selection, and weapon-class presentation assets. |

The assembly references CommonComponents, Database, Zenject, ServicesFacade, ShipConstructor, Gui, Localization, Diagnostics, and Utilites.

## Context and Operating Modes

`IShipEditorContext` is the module's application boundary. It supplies:

- The initial mutable `Constructor.Ships.IShip`.
- `IInventoryProvider` for available ships, satellite builds, owned satellites, components, quantities, and unlock payments.
- `IShipPresetStorage` for creating, listing, and deleting presets.
- `IShipDataProvider` for optional display position, rotation, scale, and pre-rendered-image behavior.
- `IComponentUpgradesProvider` for component upgrade information.
- Component unlock eligibility and whether the ship name can be edited.

`GameStateMachine.States.ShipEditorState` provides the normal application integration:

- Live game mode adapts `PlayerInventory`, `PlayerFleet`, `PlayerResources`, research, and session-backed presets. Names are editable, inventory changes are real, and unlocks charge twice the component price.
- Database mode binds `DatabaseEditorContext`. It exposes catalog ships, satellite builds, and component modifications with effectively unlimited quantities, disabled naming, an in-memory preset fallback, and no live player-economy mutation.
- Opening `ShipEditorScene` directly uses the installer's fallback `DatabaseEditorContext`, selecting `_testShipBuildId` or the first database build.

The state also binds `CloseEditorSignal`; `ShipEditorWindow.Exit` saves the current ship and fires this signal so the state machine can load its configured next state.

## Editor Model

`IShipEditorModel` is the main API consumed by UI components. `ShipEditorModel` owns the selected ship and three element slots: the ship hull, left satellite, and right satellite. Each populated slot has a `ShipLayoutModel`.

Selecting a ship performs these operations:

1. Save any changed layouts on the previous ship.
2. Create a new `ComponentTracker` for ship-wide restrictions and key assignments.
3. Build the hull layout and populate its installed components.
4. Build layouts for installed left and right satellites.
5. Publish `ShipChanged` so the UI can rebuild and reset its undo history.

The model returns removed items to `IInventoryProvider` and withdraws items before installation. Satellite installation checks size compatibility; component installation checks ship compatibility, component/tag limits, layout occupancy, cell type, weapon slot class, and inventory quantity. Locked components cannot be removed until `CanBeUnlocked` succeeds and the inventory provider accepts payment.

Saving exports placed components as `IntegratedComponent` records, including position, modification, quality, key binding, behavior, and lock state, then writes them back through the mutable ShipConstructor ship/satellite objects. Presets use the same exported component form. Loading a preset is best-effort: incompatible or unavailable items are skipped and the method returns `false` if any part could not be installed.

## Layout and Compatibility

`ShipLayoutModel` wraps a ShipConstructor `IShipLayout` and keeps two indexes:

- An ordered component list provides stable per-layout component IDs.
- A cell dictionary maps every occupied layout cell to its component for fast selection and overlap checks.

Component layouts may span multiple cells. Every non-empty component cell must lie inside the target layout, be unoccupied, and be compatible with the underlying hull cell. Weapon components on weapon cells additionally honor the barrel's `WeaponClass` when the component requests a particular slot type.

`ComponentTracker` applies restrictions that cross layout boundaries, including per-component maximums, component group tag limits, ship-model compatibility, satellite size, and default assignment among six action keys. Because the tracker spans hull and both satellites, adding a component to one element can affect availability in the others.

## UI, Commands, and Events

`ShipEditorWindow` coordinates the ship, component, satellite, detail, and build/preset panels. It translates drag positions from world coordinates to layout cells through `ShipView`, creates commands, controls navigation and zoom, and listens to model events to update only affected presentation elements.

Component placement and removal use `ICommand` implementations stored by `CommandList`:

- `InstallComponentCommand` installs a component and rolls back by finding and removing that exact placement.
- `RemoveComponentCommand` removes a component and rolls back by reinstalling its saved component info and settings.

The command list is a linear undo stack, not a redo history. It is cleared when the selected ship changes or when a bulk model refresh occurs; commands belonging to a satellite are removed when that satellite changes. Direct model actions such as remove-all, preset loading, satellite changes, unlocks, and some component edits are not represented as undoable commands.

`IShipEditorEvents` publishes ship, satellite, component-added, component-removed, component-modified, and bulk-component changes. New UI should subscribe and unsubscribe with its Unity enable/disable lifecycle, following `ShipEditorWindow`.

## Mesh and Asset Pipeline

`EditorShipLayout` creates runtime meshes rather than instantiating one GameObject per cell. Dedicated builders generate:

- Hull cells by type: outer, inner, engine, and weapon.
- Installed component shapes.
- Locked-cell markers.
- Valid/invalid placement selection feedback.
- Weapon-class overlays derived from barrel placement and occupancy.

`ShipView` owns the hull and two satellite layout renderers and converts between world space and layout coordinates. If `IShipDataProvider` has no custom data, the main window loads the ship model sprite through `IResourceLocator`; otherwise the context can position, rotate, scale, or suppress that sprite for database-oriented editing.

Keep material, shader, texture, prefab, and scene `.meta` files paired with their assets. Changes to serialized UI fields must also update the corresponding scene or prefab references.

## Common Change Locations

- Change installation, saving, satellites, presets, or unlock behavior: `Model/ShipEditorModel.cs` and the relevant context provider.
- Change cell occupancy or weapon-slot validation: `Model/ShipLayoutModel.cs` and `Model/BarrelMapBuilder.cs`.
- Change ship-wide component limits or default action keys: `Model/ComponentTracker.cs`.
- Add an undoable interaction: implement `ICommand` under `Model/Commands/` and execute it through `CommandList`.
- Change panels or drag/drop flow: `UI/ShipEditorWindow.cs` and the owning panel component.
- Change hull, component, lock, selection, or weapon-class rendering: `Layout/` plus the matching materials/shader.
- Integrate a new game mode: implement `IShipEditorContext` in the owning application assembly and bind it before `ShipEditorSceneInstaller`.

## Validation Checklist

There is currently no co-located ShipEditor test assembly, so validate model behavior and the rendered scene together.

1. Open the editor through both live game mode and database mode; also verify the standalone scene fallback when it is part of the workflow.
2. Install, remove, drag, and undo components on the hull and both satellites.
3. Exercise multi-cell components at every edge, incompatible cell types, occupied cells, weapon classes, component limits, and group-tag limits.
4. Verify inventory quantities are conserved through install, removal, satellite replacement, ship switching, preset loading, and exit.
5. Confirm locked-component visibility, payment, unlock state, and removal behavior.
6. Save, leave, and reopen the editor; verify positions, modifications, quality, key bindings, behavior, name, satellites, and presets.
7. Check camera navigation, selection meshes, overlays, localization, and layout at supported aspect ratios and touch platforms.

