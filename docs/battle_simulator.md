# BattleSimulator Module

The BattleSimulator module owns Event Horizon's reusable real-time simulation layer: combat units, ship systems, projectiles, collision responses, effects, input and AI controllers, factories, and the scene collection that advances them. It turns `ShipConstructor` specifications and database records into live combat objects.

The module lives in `Assets/Modules/BattleSimulator/` and compiles as the `BattleSimulator` assembly. Most code uses the `Combat` namespace. Application-level battle rules, result tracking, UI, and mode-specific scene managers remain in `Assets/Scripts/Combat/`; the module supplies the simulation they orchestrate.

## Runtime Flow

```text
ICombatModel + CombatSceneInstaller (Assets/Scripts)
                         |
                         v
          Scene + CollisionManager + AiManager
                         |
       ShipFactory / SatelliteFactory / SpaceObjectFactory
                         |
        database data + IShipSpecification + resources
                         v
       IUnit = body + view + collider + physics + behavior
                         |
          +--------------+---------------+
          |                              |
       Scene.Tick                    fixed-tick services
       view/effects             unit physics, AI and bounds
          |                              |
          +---------- collision ----------+
                         |
             signals and application result logic
```

`Assets/Scripts/Installers/CombatSceneInstaller.cs` is the main combat composition root. It binds the module's `Scene`, `CollisionManager`, `AiManager`, behavior-tree builder, factories, caches, input adapters, and pools alongside application-owned managers and UI. Exploration and other modes can reuse the same factories and unit model with different orchestration.

## Directory Map

| Path | Purpose |
| --- | --- |
| `Scripts/Combat/AI/` | Player, autopilot, drone, starbase, clone, and behavior-tree controllers; target selection, actions, conditions, calculations, and strategies. |
| `Scripts/Combat/Collision/` | Collider callbacks, collision data, behavior composition, impact calculation, and collision actions. |
| `Scripts/Combat/Component/` | Composable bodies, colliders, controls, engines, physics, views, stats, platforms, systems, triggers, damage handlers, and effectors. |
| `Scripts/Combat/Effects/` | Runtime visual/audio effects such as lasers, trails, lightning, smoke, text, outlines, waves, and composite effects. |
| `Scripts/Combat/Factory/` | Construction of ships, satellites, space objects, weapons, devices, drone bays, bullets, effects, and controllers. |
| `Scripts/Combat/Scene/` | Unit and ship collections, view framing, world bounds, update dispatch, and ship-created/destroyed signals. |
| `Scripts/Combat/Services/` | Adapters for resources, audio, object pooling, materials, prefabs, trails, and collision services. |
| `Scripts/Combat/Unit/` | Base unit contracts and concrete ships, satellites, bullets, auxiliary units, space objects, and worm segments. |
| `Resources/Combat/Ships/` | Special-purpose ship prefabs plus the fallback `Default` prefab used by `ShipFactory`. |
| `Plugins/Collider2D Optimization/` | Polygon and edge collider optimization helpers used when building runtime colliders. |

The assembly references Zenject, Database, CommonComponents, ResourceLocator, ShipConstructor, ServicesFacade, GameServices, Diagnostics, Localization, Utilites, and Unity's Input System.

## Unit and Scene Lifecycle

`IUnit` is the common runtime contract. A unit exposes its classification, body, view, collider, collision behavior, physics manager, defense multiplier, and state. `UnitBase` owns disposable resources and provides separate `UpdatePhysics` and `UpdateView` paths. Concrete units decide how collisions, state changes, and disappearance are handled.

`Combat.Scene.Scene` owns two synchronized collections:

- `Units` contains every live `IUnit`.
- `Ships` is the indexed subset of `IShip` instances.

During `Tick`, the scene updates the unit collection, advances views, updates camera framing, and decays screen shake. During `FixedTick`, it advances unit physics, finds the nearest enemy, and wraps top-level units around the configured combat area. Inactive units are removed and disposed by the collection. Adding or removing a ship fires `ShipCreatedSignal` or `ShipDestroyedSignal`.

`SceneSettings` supplies map width, map height, and player-centering behavior. The application normally derives the map size from `ICombatModel.Rules.BattleMapSize` when installing the combat scene.

## Object Construction

Factories are the boundary between data and simulation objects:

- `ShipFactory` consumes an `IShipSpecification` from ShipConstructor and creates the GameObject holder, body, collider, view, stats, controls, engine, platforms, systems, triggers, and controller. It registers the completed ship with both `IScene` and `IAiManager`.
- `WeaponFactory`, `DeviceFactory`, and `DroneBayFactory` translate specification entries and database models into `ISystem` implementations attached to a ship.
- `BulletFactory` translates ammunition and bullet-trigger records into projectiles, spawn rules, collision actions, and secondary effects. `BulletFactoryObsolete` remains for legacy ammunition data.
- `SatelliteFactory` and `SpaceObjectFactory` build auxiliary units and environmental objects.
- `ControllerFactory` selects keyboard, autopilot, default AI, custom behavior-tree, clone, drone, or starbase controllers.
- `EffectFactory` constructs presentation effects without making the owning manager depend on their concrete types.

`ShipFactory` first looks for a specialized prefab at `Resources/Combat/Ships/<model image id>`. If none exists, it uses `Resources/Combat/Ships/Default`, assigns the database sprite, and builds a circle or optimized polygon collider from the ship model. Prefab changes must preserve the components expected by the factory, including the combat collider, view, and physics components.

## Ship Components and Systems

The simulation favors small interfaces and composed implementations rather than a single ship behavior class:

- `Body`, `Physics`, and `Engine` own spatial state and movement.
- `Controls` carries player or AI intent into the ship.
- `Platform` represents weapon/device mounting and aiming.
- `Systems/Weapons`, `Systems/Devices`, and `Systems/DroneBays` implement active combat capabilities.
- `Stats` and `DamageHandler` calculate resources, armor, shields, damage, and destruction.
- `Triggers` attach responses to conditions such as activation, damage, destruction, and collisions.
- `View` and `Effects` keep rendering and audio behavior separate from core unit state.

When adding a data-driven system, keep database-to-runtime selection in the corresponding factory and the live behavior in the closest `Component/Systems/` or `Component/Triggers/` folder. Update the generated database schema and data through the [Database module](database.md) when a new serialized variant is required.

## Collision Processing

Unity collider adapters report contacts to `ICollisionManager`. `CollisionManager` asks the first unit's `ICollisionBehaviour` to calculate impacts for both participants, then dispatches the results to each unit through `OnCollision`. Collision behavior and collision action classes determine damage, impulse, attachment, detonation, and other responses.

Keep collision calculations in the collision layer. A new collider component must set its owning `IUnit`, initialize with `ICollisionManager`, and produce complete `CollisionData`, including contact position, relative velocity, time interval, and whether the contact is new.

## AI and Behavior Trees

All controllers implement `IController`. `AiManager` accepts controllers as units are created and updates them on fixed-frame progress. It passes cease-fire state, elapsed time since player input, and a load-shedding flag that disables threat calculations when the controller count exceeds its optimization threshold.

Outside WebGL, `AiManager` derives from `BackgroundTask` and performs controller updates on a worker thread; WebGL performs the same work from `FixedTick`. Controller and behavior-tree changes must therefore avoid unsafe Unity API access and unsynchronized shared mutable state in their update path.

Database `BehaviorTreeModel` records are built into runtime trees by `BehaviorTreeBuilder`. Node implementations are grouped into action, composite, condition, special, and utility folders. Higher-level controller strategies live under `AI/Strategy/`, while reusable target selection and calculations live beside them.

## Common Change Locations

- Add or change ship construction: `Factory/ShipFactory.cs` and the relevant component or system folder.
- Add a weapon or device behavior: `Factory/Systems/` plus `Component/Systems/`; update database types when the behavior is data-driven.
- Add ammunition triggers or projectile behavior: `Factory/Bullets/`, `Unit/Bullet/`, and `Unit/Bullet/Action/`.
- Add an AI behavior-tree node: `AI/BehaviorTree/Nodes/` and the builder/factory code that maps its database model.
- Add a combat effect: `Effects/` and `Factory/EffectFactory.cs`.
- Change world update, view framing, or wrapping: `Scene/`.
- Change battle victory, rewards, spawning rules, or UI: use `Assets/Scripts/Combat/`; those concerns are outside this module.

## Validation Checklist

There is currently no co-located BattleSimulator test assembly, so validate changes through focused Unity scene tests and the affected game mode.

1. Open `CombatScene` and verify the `CombatSceneInstaller` has all serialized references assigned.
2. Exercise player, enemy, drone, satellite, and starbase creation paths affected by the change.
3. Check both frame and fixed-frame behavior, including pause, time scaling, destruction, and scene cleanup.
4. Test collision behavior at low and high relative velocity and confirm pooled objects are returned cleanly.
5. Test with enough AI-controlled units to enter the optimized controller path.
6. For platform-sensitive AI changes, test both the threaded build path and WebGL's single-threaded path.
