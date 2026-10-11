# BattleSimulator Module

The BattleSimulator module is the real-time 2D simulation that runs every battle and every planet exploration in Event Horizon. It builds ships, projectiles, and space objects from database records. It then moves them, resolves collisions, applies damage, drives AI and player input, and shows visual effects.

The module lives in [`Assets/Modules/BattleSimulator/`](../Assets/Modules/BattleSimulator/) and compiles as the `BattleSimulator` assembly ([BattleSimulator.asmdef](../Assets/Modules/BattleSimulator/BattleSimulator.asmdef)). It has about 490 C# files and 31,000 lines. Almost all code uses namespaces under `Combat`.

Known defects and risks are listed in [issues/battle_simulator.md](issues/battle_simulator.md).

## Contents

1. [Scope and boundaries](#scope-and-boundaries)
2. [Terms](#terms)
3. [Directory map](#directory-map)
4. [Composition root](#composition-root)
5. [Frame loop](#frame-loop)
6. [Threading model](#threading-model)
7. [Units](#units)
8. [Bodies and transforms](#bodies-and-transforms)
9. [Ships](#ships)
10. [Ship systems](#ship-systems)
11. [Weapons and platforms](#weapons-and-platforms)
12. [Devices](#devices)
13. [Drone bays, replicators, and clones](#drone-bays-replicators-and-clones)
14. [Bullets](#bullets)
15. [Collision and damage](#collision-and-damage)
16. [Space objects and area effects](#space-objects-and-area-effects)
17. [Triggers](#triggers)
18. [Views, effects, and prefabs](#views-effects-and-prefabs)
19. [Target selection](#target-selection)
20. [AI](#ai)
21. [Player input](#player-input)
22. [Integration with the application layer](#integration-with-the-application-layer)
23. [Resources](#resources)
24. [How to extend the module](#how-to-extend-the-module)
25. [Validation checklist](#validation-checklist)

## Scope and boundaries

The module owns the simulation. It does not own battle rules, rewards, fleets, or UI.

| Concern | Owner |
| --- | --- |
| Units, physics adapters, collisions, damage, weapons, devices, drones, AI, effects | This module |
| Which ships spawn, victory and defeat, rewards, timers, retreat, scene exit | `Assets/Scripts/Combat/` (see [Application layer](application_layer.md)) |
| Ship stats and equipment (`IShipSpecification`, `IShipStats`) | `ShipConstructor` module |
| Data records (`Ammunition`, `DeviceStats`, `WeaponStats`, `BehaviorTreeModel`, `VisualEffect`) | [Database module](database.md) |
| HUD, radar, stats panels | `Assets/Scripts/Gui/Combat/` |

The assembly references `Zenject`, `Database`, `CommonComponents`, `ResourceLocator`, `ShipConstructor`, `Unity.InputSystem`, `ServicesFacade`, `GameServices`, `Diagnostics`, `Localization`, and `Utilites`. Only `Assembly-CSharp` (the application layer) references this assembly.

Four scenes install the simulation: combat, exploration, Ehopedia (the encyclopedia ship viewer), and the controls configuration scene. See [Composition root](#composition-root).

## Terms

The code uses these terms consistently. This document uses them in the same way.

| Term | Meaning |
| --- | --- |
| **Unit** | Any simulated object. It implements [`IUnit`](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/IUnit.cs). Ships, bullets, shields, asteroids, and explosions are units. |
| **Body** | The position, rotation, scale, velocity, and weight of a unit ([`IBody`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Body/IBody.cs)). A body can have a parent body. |
| **View** | The renderer side of a unit ([`IView`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/View/IView.cs)): color, size, and life (a 0–1 value that views use for fading). |
| **Collider** | The adapter between Unity 2D physics and the module ([`ICollider`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Collider/ICollider.cs)). It reports contacts. |
| **Collision behaviour** | The object that turns a contact into damage and other results ([`ICollisionBehaviour`](../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/ICollisionBehaviour.cs)). |
| **Impact** | A value that collects damage, repair, impulse, and effect flags for one unit in one contact ([`Impact`](../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Impact.cs)). |
| **System** | A ship weapon, device, or drone bay ([`ISystem`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/ISystem.cs)). |
| **Platform** | A weapon mount on a ship ([`IWeaponPlatform`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Platform/IWeaponPlatform.cs)). It aims and limits fire rate. |
| **Modification** | An object that changes engine, feature, resistance, or system data every frame ([`IModification<T>`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Mods/IModification.cs)). |
| **Trigger** | An action or effect that runs on a unit or system event, such as destroy, hit, or activate ([`IUnitAction`, `IUnitEffect`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Triggers/)). |
| **Unit controller** | Code that moves a bullet, satellite, or helper unit every physics step ([`Combat.Component.Controller.IController`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Controller/IController.cs)). It runs on the main thread. |
| **Ship controller** | Code that decides what a ship does: player input or AI ([`Combat.Ai.IController`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/IController.cs)). It runs on the AI thread. Two different interfaces have the name `IController`. |
| **Controls** | The intent of a ship: throttle, course, and active systems ([`IControls`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Controls/IControls.cs)). Ship controllers write it. The ship reads it. |

## Directory map

All paths are under `Assets/Modules/BattleSimulator/`.

| Path | Content |
| --- | --- |
| [`Scripts/Combat/Scene/`](../Assets/Modules/BattleSimulator/Scripts/Combat/Scene/) | `Scene` (unit collection and frame loop), `UnitList`, `ShipList`, camera framing (`ViewRect`, `ExplorationViewRect`), target search helpers (`ShipListExtensions`), ship signals. |
| [`Scripts/Combat/Unit/`](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/) | `IUnit`, `UnitBase`, `UnitType`/`UnitClass`/`UnitSide`, `UnitTriggers`, `Ship`, `Bullet` and bullet actions, auxiliary units, worm tails, objects, satellites, resource points (`Energy`, `HitPoints`). |
| [`Scripts/Combat/Component/`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/) | Bodies, colliders, unit controllers, controls, damage handlers, effectors, engines, features, prefab initializers, modifications, joints, platforms, stats, systems (weapons, devices, drone bays), triggers, views. |
| [`Scripts/Combat/Collision/`](../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/) | `CollisionManager`, `CollisionData`, `Impact`, `Resistance`, collision behaviours, and about 30 collision actions. |
| [`Scripts/Combat/Factory/`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/) | `ShipFactory`, `SatelliteFactory`, `SpaceObjectFactory`, `WeaponFactory`, `DeviceFactory`, `DroneBayFactory`, `BulletFactory`, `BulletFactoryObsolete`, `EffectFactory`, `ControllerFactory`. |
| [`Scripts/Combat/AI/`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/) | `AiManager`, ship controllers, player input, the behaviour tree, and the older strategy AI (actions, conditions, strategies, target and threat lists). |
| [`Scripts/Combat/Effects/`](../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/) | Short-lived visual effects: `EffectBase`, `CompositeEffect`, `ViewEffect`, and concrete effects. |
| [`Scripts/Combat/Services/`](../Assets/Modules/BattleSimulator/Scripts/Combat/Services/) | `PrefabCache`, `MaterialCache`, `TrailRendererPool`, `GameServicesProvider`, `CustomPrefabLoader`. |
| [`Scripts/Combat/Helpers/`](../Assets/Modules/BattleSimulator/Scripts/Combat/Helpers/) | `GameObjectHolder` (pooled GameObject wrapper) and `RadioTransmitter` (floating text messages from ships). |
| [`Plugins/Collider2D Optimization/`](../Assets/Modules/BattleSimulator/Plugins/Collider2D%20Optimization/) | Ramer–Douglas–Peucker simplification for `PolygonCollider2D`. `ShipFactory` uses the `Optimize` extension method. |
| [`Resources/Combat/`](../Assets/Modules/BattleSimulator/Resources/Combat/) | Prefabs for ships, satellites, bullets, effects, objects, guns, and exploration objects. See [Resources](#resources). |

## Composition root

Each scene installer binds the same set of simulation services. The combat scene uses [`CombatSceneInstaller`](../Assets/Scripts/Installers/CombatSceneInstaller.cs).

| Binding | Lifetime | Notes |
| --- | --- | --- |
| `Scene` | single, non-lazy | Bound to `IScene`, `ITickable`, `IFixedTickable`, `IDisposable`. Gets `SceneSettings` as an argument. |
| `ViewRect` or `ExplorationViewRect` | transient | Camera framing. Exploration uses `ExplorationViewRect`. |
| `CollisionManager` | single | Bound to `ICollisionManager`. |
| `AiManager` | single, non-lazy | Bound to `IAiManager`, `IInitializable`, `IDisposable`, `IFixedTickable`. |
| `BehaviorTreeBuilder` | single | Shares one `MessageHub` and one `ShipTargetLocator` for all trees in the scene. |
| `ShipFactory` | single | Gets `ShipFactory.Settings` as an argument. |
| `WeaponFactory`, `DeviceFactory`, `DroneBayFactory`, `SatelliteFactory`, `SpaceObjectFactory`, `EffectFactory`, `ControllerFactory` | single | |
| `GameServicesProvider` | single | Bundles the resource locator, trail pool, collision manager, sound player, object pool, and prefab cache for bullets. |
| `RadioTransmitter` | single | `ITickable`. Shows text that ships broadcast. |
| `GameObjectPool`, `TrailRendererPool`, `MaterialCache` | single / scene instance | Pools and caches. |
| `InputSystemKeyboard`, `InputSystemMouse` | single | Player input adapters. |

`ShipCreatedSignal`, `ShipDestroyedSignal`, `CeasefireSignal`, and `PlayerInputSignal` are bound once in [`GameInstaller`](../Assets/Scripts/Installers/GameInstaller.cs).

Scene settings differ per mode:

| Scene | Area size | `PlayerAlwaysInCenter` | `ShipFactory.Settings` |
| --- | --- | --- | --- |
| Combat | `ICombatModel.Rules.BattleMapSize` | from game settings | `NoDamageIndicator` and `NoEnemyMessages` from game settings |
| Exploration | 1000 × 1000 | true | `Shadows = true`, `StaticWrecks = true` |
| Ehopedia | 200 × 200 | false | defaults |

`Scene` also sets `PlayerAlwaysInCenter` when `IGameSettings.CenterOnPlayer` is true.

## Frame loop

Zenject calls `Scene.Tick` from `Update` and `Scene.FixedTick` from `FixedUpdate`. `AiManager.FixedTick` also runs from `FixedUpdate`.

### `Scene.FixedTick` (physics step, main thread)

[`Scene.cs`](../Assets/Modules/BattleSimulator/Scripts/Combat/Scene/Scene.cs)

1. Call `UpdatePhysics(deltaTime)` on every unit. `deltaTime` is the time since the previous fixed step.
2. Find the nearest active enemy ship of class `Ship`. Store it as `EnemyShip`.
3. Every 0.5 s, wrap units around the player ship. A unit that is more than half the area width or height away moves by one area width or height. The wrap skips the player ship, child bodies, and units of class `Limb`.

### `Scene.Tick` (frame, main thread)

1. Apply pending additions in `UnitList.UpdateCollection`. Each new unit fires `UnitAdded`. Dispose units that were removed in earlier updates.
2. Call `UpdateView(Time.deltaTime)` on every unit.
3. Update the camera rectangle (`IViewRect.Update`) from the player ship and the nearest enemy.
4. Decay the screen shake value.

### Unit list rules

[`UnitList.cs`](../Assets/Modules/BattleSimulator/Scripts/Combat/Scene/UnitList.cs)

- `AddUnit` only queues the unit. The unit joins the list at the start of the next `Tick`. Until then it does not receive physics or view updates.
- During `UpdateItems`, a unit that returns `State != Active` leaves the list. The list fires `UnitRemoved` at once, but it calls `Dispose` on the unit only in the next `UpdateCollection`.
- `ShipList` mirrors the subset of units that implement `IShip`. `Scene` fires `ShipCreatedSignal` when a ship joins and `ShipDestroyedSignal` when it leaves. The signal name says "destroyed", but it also fires for ships that vanished (retreated).
- Both lists expose a `LockObject`. Code on the AI thread must lock it before it reads `Items`.

### Player and enemy tracking

- `PlayerShip` is the first active ship of class `Ship` and side `Player`. A new player ship replaces it only after the old one stops being active.
- `EnemyShip` is the nearest active enemy of class `Ship`.
- `ViewRect` frames both ships. When more than one enemy exists, or `PlayerAlwaysInCenter` is set, it keeps the player in the middle.
- `FindFreePlace` picks a random point in the area around the player that is at least `minDistance` from every hostile ship. After 100 failed tries it returns the player position.

### Unit update order

[`UnitBase`](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/UnitBase.cs) runs the same order for every unit except `Bullet`, which has its own copy:

- Physics: `OnUpdatePhysics` (subclass logic), then `Body.UpdatePhysics`, then `Collider.UpdatePhysics`, then active trigger actions.
- View: `OnUpdateView`, then `Body.UpdateView`, then `View.UpdateView`, then active trigger effects.

A unit with `State != Active` gets no updates.

## Threading model

The simulation runs on two threads outside WebGL.

| Thread | Work |
| --- | --- |
| Main thread | Unity physics, `Scene` updates, colliders, collisions, damage, systems, effects, factories. |
| AI thread | `AiManager` runs every ship controller, including the player keyboard controller. Controllers read the scene and write `ship.Controls`. |

[`AiManager`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/AiManager.cs) derives from [`BackgroundTask`](../Assets/Modules/CommonComponents/Scripts/Legacy/BackgroundTask.cs). `FixedTick` only increments a frame counter. The worker thread checks the counter in a loop. When the counter changed, it runs every controller once with `deltaTime = fixedDeltaTime × frames passed`. When nothing changed, it sleeps for one fixed step. On WebGL, `FixedTick` runs the controllers directly on the main thread.

Rules for code that runs on the AI thread:

1. Lock `IScene.Ships.LockObject` or `IScene.Units.LockObject` before you enumerate units.
2. Read body data only through `IBody`. [`RigidBodyAdapter`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Body/RigidBodyAdapter.cs) returns cached values when a non-main thread asks for `Position` or `Rotation`.
3. Do not call Unity APIs (`Time`, `Transform`, `Physics2D`, `Debug.Break`, `UnityEngine.Random`).
4. Write ship intent into a private `ShipControls` buffer, then call `ShipControls.Apply(ship)` once.
5. Treat all shared state as racy. Many fields (for example `UnitType.Owner`, `Controls`, platform targets) change on both threads without locks.

`AiManager` sets `Options.DisableThreats` when more than 30 controllers exist. Threat lists then stay empty.

## Units

### Classification

[`UnitType`](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Classification/UnitType.cs) has a class, a side, and an optional owner ship.

- `Side` returns the owner's side when an owner exists, unless the type was created with `ignoreOwnerSide`. Changing `Owner` at run time changes the side. Drone capture and "go berserk" use this.
- [`UnitSide`](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Classification/UnitSide.cs): `Player` and `Ally` are friends. Every other pair of different sides is hostile. `Undefined` is hostile to everything except itself.
- `CollisionLayer` maps class and side to a Unity layer:

| Classes | Base layer | Side offset |
| --- | --- | --- |
| `SpaceObject` | `Default` (no offset) | – |
| `Ship`, `Shield` | `Ship1` (8) | Player/Ally +0, Enemy +1, Neutral +2, Undefined +3 |
| `Missile`, `Limb` | `Missile1` (12) | same |
| `EnergyBolt`, `AreaOfEffect`, `Camera`, `Loot`, `BackgroundObject` | `Energy1` (16) | same |
| `Drone`, `Decoy` | `Drone1` (20) | same |
| `Platform` | `Platform1` (24) | same |

The Physics 2D layer matrix in project settings decides which layers touch. `CommonCollider` updates the GameObject layer when the unit's side changes.

### Unit state

[`UnitState`](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/UnitState.cs):

- `Active` – the unit updates and collides.
- `Inactive` – the unit vanished. `Vanish()` sets it. A retreating ship uses it. No destroy triggers run.
- `Destroyed` – the unit was killed. Destroy triggers usually run before the state changes.

### Unit classes

| Class | File | Purpose |
| --- | --- | --- |
| `Ship` | [Unit/Ship/Ship.cs](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Ship/Ship.cs) | Ships and drones. See [Ships](#ships). |
| `Bullet` | [Unit/Bullet/Bullet.cs](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Bullet/Bullet.cs) | All projectiles, beams, and auras. It implements `IUnit` directly, not through `UnitBase`. See [Bullets](#bullets). |
| `Decoy` | [Unit/Auxiliary/Decoy.cs](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Auxiliary/Decoy.cs) | Short-lived fake ship. It implements `IShip`, so it is in `Scene.Ships`. `Stats`, `Engine`, `Systems`, `Effects`, and `Specification` are `null`. |
| `EnergyShield` | [Unit/Auxiliary/EnergyShield.cs](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Auxiliary/EnergyShield.cs) | Bubble or frontal shield. Its damage handler converts damage into energy cost. |
| `PointDefenseShield` | [Unit/Auxiliary/PointDefenseShield.cs](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Auxiliary/PointDefenseShield.cs) | Circle sensor that disarms missiles and energy bolts. |
| `RepairBot` | [Unit/Auxiliary/RepairBot.cs](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Auxiliary/RepairBot.cs) | Drone that orbits a ship and repairs it. |
| `WormSegment` | [Unit/Auxiliary/WormSegment.cs](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Auxiliary/WormSegment.cs) | Tail segment for the first worm-tail device version. |
| `WormSegmentBase`, `FixedWormSegment`, `WormSegmentDestroyable` | [Unit/WormTail/](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/WormTail/) | Self-growing tail for `WormTailV2`. Each segment spawns the next one. |
| `Satellite` | [Unit/Satellite/Satellite.cs](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Satellite/Satellite.cs) | Companion that carries a weapon platform next to a ship. |
| `Asteroid` | [Unit/Object/Asteroid.cs](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Object/Asteroid.cs) | Physical obstacle with hit points. |
| `SpaceObject` | [Unit/Object/SpaceObject.cs](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Object/SpaceObject.cs) | Planets and exploration objects. It runs `OnActivate` triggers when its behaviour sets the `Activate` flag. |
| `ShortLivedObject` | [Unit/Object/ShortLivedObject.cs](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Object/ShortLivedObject.cs) | Explosions, clouds, gravity fields. With `lifetime <= 0` it lives as long as its `Parent`. |
| `StaticPlatform`, `Loot`, `CameraUnit` | various | Present in code. No factory or application code creates `StaticPlatform` or `Loot`. `CameraUnit` has a factory method that nothing calls. |

## Bodies and transforms

[`IBody`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Body/IBody.cs) stores local values. Default interface methods compute world values through the parent chain: `WorldPosition`, `WorldRotation`, `WorldScale`, `WorldVelocity`, `TotalWeight`, and the visual (interpolated) variants.

- `Offset` moves the body origin along its own forward axis. Beams use it so that a child effect sits at the beam tip.
- `ChildPosition` adds the parent offset to a child's local position.

| Implementation | Used by | Behaviour |
| --- | --- | --- |
| [`RigidBodyAdapter`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Body/RigidBodyAdapter.cs) | ships, most bullets, objects (188 prefabs) | Wraps `Rigidbody2D`. A root body is dynamic. A child body is kinematic. Accelerations become impulses (`AddForce × mass`). It caches world position and rotation for the AI thread. It can clamp speed to 120. |
| [`GameObjectBody`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Body/GameObjectBody.cs) | beams, arcs, saws, black holes (32 prefabs) | Kinematic integration without a rigidbody. It interpolates the view between fixed steps. |
| [`SimpleBody`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Body/SimpleBody.cs) | fixed weapon platforms, drone bay platform | Static child transform created at run time. |
| [`WeaponPlatformBody`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Body/WeaponPlatformBody.cs) | auto-aiming platforms | Child transform that turns toward a target within `±maxAngle` at `rotationSpeed`. It searches a target every 0.1–1.0 s with `GetEnemyForTurret`. |
| [`EmptyBody`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Body/EmptyBody.cs), [`BodyWrapper`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Body/BodyWrapper.cs) | helpers | `BodyWrapper` reads another body and ignores writes, except rotation. Bullet spawners use it as a fake platform body. |

Unity joints connect bodies through [`PhysicsManager`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Physics/PhysicsManager.cs): distance joints (harpoons, tractor beams), hinge joints (worm tails), and fixed joints (harpoon hits).

## Ships

### Construction

[`ShipFactory.CreateShip`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs) builds a ship in this order:

1. Load the prefab `Resources/Combat/Ships/<ShipModel.ModelImage.Id>`. If it does not exist, load `Combat/Ships/Default`, set the database sprite, and add a `CircleCollider2D` (size class `Undefined`) or a `PolygonCollider2D` simplified with `ShipModel.ColliderTolerance`.
2. Initialize the collider with the `ICollisionManager`. Apply the HSV color scheme through `MaterialCache` when the scheme uses HSV.
3. Initialize the body at the given position and rotation, with `ModelScale` as scale and `Weight` as mass.
4. Create [`ShipStats`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Stats/ShipStats.cs). Create the `Ship` as class `Drone` when a mother ship is given, otherwise as class `Ship`.
5. Add a damage indicator (floating damage numbers) for non-drones, unless settings disable it.
6. Create the engine (`ShipEngine`) and `CommonControls`.
7. Add engine light and trail effects for every engine in the model, an optional shadow, the destruction effect, and the shield effect when `ShieldPoints > 0`.
8. For every platform in the specification that has a companion or a weapon: create a satellite if needed, create the platform, then create and add each weapon (new and obsolete formats).
9. Create every device through `DeviceFactory`.
10. For non-drones: create cloning centers, the drone bay platform, the optional `DroneReplicator`, and every drone bay.
11. Activate the GameObject, add the ship to the scene, create the ship controller, and add it to `AiManager`.
12. Attach `RadioTransmitter` unless enemy messages are disabled.

Public entry points:

| Method | Side | Controller |
| --- | --- | --- |
| `CreatePlayerShip` | `Player` | keyboard; plus autopilot when `Stats.Autopilot` is true |
| `CreateEnemyShip` | `Enemy` | default AI for the AI level, or `spec.CustomAi` |
| `CreateShip` | any | any `IControllerFactory` |
| `CreateDrone` | owner's side | drone AI |
| `CreateClone` | owner's side | clone AI |
| `CreateStarbase` | any | starbase AI; engine replaced by `StarbaseEngine` (slow spin) |
| `CreateTurret` | any | starbase AI; engine replaced by `NullEngine` |

### Ship physics step

[`Ship.OnUpdatePhysics`](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Ship/Ship.cs) runs:

1. Copy `Controls.Course` and `Controls.Throttle` into the engine.
2. `Engine.Update` applies acceleration, deceleration, and turning. It uses the "without energy" engine stats when energy is zero.
3. `Features.UpdatePhysics` recomputes features and sets `Collider.Enabled = !Invulnerable`.
4. `Stats.UpdatePhysics` recharges energy, shield, and armor. If armor reached zero, the ship runs `OnDestroy` triggers and becomes `Destroyed`.
5. `Systems.UpdatePhysics` updates platforms, then every system. See [Ship systems](#ship-systems).
6. `Effects.UpdatePhysics` updates temporary ship effects (for example retreat).

### Modification pipeline

Each frame, these components rebuild their data from base values and then apply every registered modification:

| Component | Data | Typical modifiers |
| --- | --- | --- |
| [`ShipEngine`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Engine/ShipEngine.cs) | `EngineData` (velocity, turn rate, propulsion, throttle, deceleration, course) | accelerator, brake, detonator, warp drive, retreat |
| [`Features`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Features/Features.cs) | `FeaturesData` (target priority, color, opacity, invulnerability, camouflage chances) | stealth, ghost, energy shield, fortification, jammer, camouflage, warp drive |
| `ShipStats.Resistance` | `Resistance` | fortification, worm tails |
| `ShipSystems.Modifications` | `ISystemsModification` (can a system activate) | ghost, stealth, detonator, warp drive, retreat |

`Ship.AddSystem` and `Ship.AddEffect` register a system's or effect's modifications and its unit action. A modification stays registered until `TryApplyModification` returns `false`.

### Damage pipeline

`Ship.OnCollision` calls `Affect(impact)`, then runs `OnHit` triggers. `Affect` applies the impulse, then calls [`ShipStats.ApplyDamage`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Stats/ShipStats.cs), then raises `SystemEventType.DamageTaken` on all systems.

`ShipStats.ApplyDamage`:

1. Return if the ship is already dead.
2. Compute the current resistance (base values plus modifications).
3. `Impact.ApplyShield`: unless the impact ignores shields, the current shield value absorbs shield damage first, then corrosive damage (reduced by shield corrosive resistance), then kinetic, energy, and heat damage in proportion. Absorbed damage becomes `ShieldDamage`.
4. Report the damage to the damage indicator.
5. Compute armor damage: `kinetic × (1 − Kinetic) + energy × (1 − Energy) + heat × (1 − Heat) + corrosive × (1 − 0.5 × min(Kinetic, Energy, Heat))`.
6. If `EnergyAbsorption > 0`, convert that fraction of the raw energy damage (divided by `HitPointsMultiplier`) into energy.
7. Subtract `Repair` from the armor damage, then apply it to armor. Negative damage repairs.
8. Drain energy by `EnergyDrain` and shield by `ShieldDamage`.
9. If the impact has the `Destroy` flag, remove all armor.
10. Update performance statistics. Statistics are off: `ShipFactory` always passes `collectStatistics = false`.

Resource points ([`IResourcePoints.cs`](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/HitPoints/IResourcePoints.cs)):

- `Energy` stores a 0–1 fraction. It recharges at `rechargeRate` per second after `rechargeDelay` seconds without use. It uses interlocked operations so that the AI thread can read it.
- `HitPoints` is non-regenerating armor. `TryGet` throws `NotImplementedException`.
- Armor uses `Energy` when `ArmorRepairRate > 0`. Ships with armor below 0.1 get `EmptyResources` and log an error.

## Ship systems

[`ShipSystems`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/ShipSystems.cs) holds all systems in creation order. The index in `Systems.All` is the system ID. Controls, AI, and key bindings use this ID.

Each physics step:

1. Update every weapon platform.
2. Remove dead system modifications.
3. For each system, decide whether it should be active, set `system.Active`, notify modifications with `OnSystemActivated`, then call `UpdatePhysics`.

A system is active when all of these are true:

1. `CanBeActivated` is true. Each system defines it, usually from cooldown and energy.
2. The key binding check passes:
   - `KeyBinding >= 0`: `Controls.Systems[id]` must be set.
   - `KeyBinding < 0` (automatic system): the ship must keep an energy reserve. The reserve is `0.1 × (−KeyBinding − 1)` of maximum energy. `−1` means no reserve, `−2` means 10%, `−3` means 20%. An active system adds its activation cost back to the check so that it does not flicker.
3. Every system modification allows it (`CanActivateSystem`).

[`SystemBase`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/SystemBase.cs) gives each system a cooldown (`MaxCooldown`, `TimeFromLastUse`), its own `UnitTriggers`, and default modification properties that return `null`.

## Weapons and platforms

### Weapon creation

[`WeaponFactory`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/WeaponFactory.cs) creates a bullet factory, multiplies the fire rate by the stat modifier, and selects a class from `WeaponStats.WeaponClass`:

| `WeaponClass` | Class | Behaviour | Triggers |
| --- | --- | --- | --- |
| `Common` | [`CommonCannon`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/CommonCannon.cs) | One bullet per shot, cooldown `1 / FireRate`. | shot sound and flash on `OnActivate` |
| `MashineGun` | [`MachineGun`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/MachineGun.cs) | Burst of `Magazine` shots limited by platform cooldown, then weapon cooldown. Random factor 0.2. | same |
| `MultiShot` | [`MultishotCannon`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/MultishotCannon.cs) | `Magazine` bullets at once. Energy cost × magazine. Random factor 0.3. | same |
| `Manageable` | [`ManageableCannon`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/ManageableCannon.cs) | One active bullet. Releasing the button detonates it. Cooldown starts when the bullet dies. | loop sound start/stop, flash |
| `Continuous` | [`ContinuousCannon`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/ContinuousCannon.cs) | One beam bullet. It restores the bullet lifetime every step while active and energy lasts. | loop sound, flash on activate and remain active |
| `RequiredCharging` | [`ChargeableCannon`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/ChargeableCannon.cs) | Charges while active for `1 / FireRate` seconds. Release fires with `PowerLevel` = charge fraction. | charge sound, power level effect, shot sound on `OnDischarge` |

[`WeaponBase`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/WeaponBase.cs) creates bullets with `BulletFactory.Create(platform, spread, 0, Vector2.zero)` and asks the platform to aim with bullet speed, range, and parent velocity effect. [`WeaponInfo`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/WeaponInfo.cs) exposes data for AI: weapon type, bullet behaviour, capability flags, range, speed, energy cost, and recoil.

Weapons with bullets bound to the cannon keep their bullets in a `BulletCompositeDisposable`. Disposing the weapon makes those bullets vanish.

### Platforms

| Platform | When | Behaviour |
| --- | --- | --- |
| [`FixedPlatform`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Platform/FixedPlatform.cs) | `AutoAimingArc == 0`, or the satellite has its own aiming | Fixed mount. It forwards aiming to a satellite aiming system if one exists. |
| [`AutoAimingPlatform`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Platform/AutoAimingPlatform.cs) | `AutoAimingArc > 0` | Uses `WeaponPlatformBody` to turn within the arc. Turrets with an image track targets every step. |

Every platform has a shot cooldown of 0.04 s. The drone bay platform has 0.4 s. When the platform data has an image and weapons, `ShipFactory` adds a turret sprite from `Combat/Guns/turret`.

### Satellites

[`SatelliteFactory.CreateSatellite`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/SatelliteFactory.cs) creates a companion unit on the left or right side of the ship. A `SatelliteController` keeps it at a fixed offset. An `AutoAimingSatelliteController` also turns it toward targets within its side arc. The satellite is destroyed or vanishes together with its ship.

## Devices

[`DeviceFactory`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/DeviceFactory.cs) maps `DeviceStats.DeviceClass` to a class. Device files are in [`Component/Systems/Devices/`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/).

| `DeviceClass` | Class | Behaviour |
| --- | --- | --- |
| `Accelerator` | `AcceleratorDevice` | While active: throttle 1, propulsion and velocity × `Power`. Energy per second. |
| `Brake` | `BrakeDevice` | Automatic. Adds deceleration when throttle is zero. |
| `Decoy` | `DecoyDevice` | Spawns `Power` decoys with `ArmorMultiplier × 10` hit points. |
| `Detonator` | `DetonatorDevice` | Locks engine and systems for `Lifetime`, then makes a heat explosion and destroys the ship. |
| `EnergyShield` | `EnergyShieldDevice` + `EnergyShield` unit | While active the ship is invulnerable to direct contact; the shield unit absorbs damage and spends energy (`1 / (Power × ShieldMultiplier)` per damage point). |
| `PartialShield` | `FrontalShieldDevice` + `EnergyShield` unit | Frontal shield with energy per second. |
| `Fortification` | `FortificationDevice` | While active, resistances move toward 1 by `Power / 100`. |
| `Ghost` | `GhostDevice` | Invulnerable for `Lifetime`. Blocks other systems except stealth. |
| `GravityGenerator` | `GravityGenerator` | Spawns a 0.5 s gravity bomb that pushes units away and destroys missiles and bolts. |
| `PointDefense` | `PointDefenseSystem` + `PointDefenseAction` | Creates a `PointDefenseShield` unit that disarms enemy missiles and bolts in range and damages drones. |
| `RepairBot` | `RepairSystem` + `RepairBotAction` | Keeps a `RepairBot` alive. It repairs `Power % of max armor` per second while active. |
| `Stealth`, `SuperStealth` | `StealthDevice` | Target priority `None` and low opacity. A hit or most other systems end it. `SuperStealth` is not ended by hits. |
| `Teleporter` | `TeleporterDevice` | Jumps forward by `Range` and stops the ship. |
| `TeleporterV2` | `WarpDrive` | Fast flight for `Range / Power` seconds while invulnerable and transparent. Ships with a `WormTail` device jump instantly instead. |
| `TimeMachine` | `InfinityStone` | When armor reaches zero (or on use), restores energy, armor, and shield and grants invulnerability for `Lifetime`. |
| `ToxicWaste` | `ToxicWaste` | On ship destruction, creates a corrosive cloud. |
| `Retribution` | `RetributionDevice` | On ship destruction, fires the device ammunition from the ship. |
| `Jammer` | `JammerDevice` | Sets target priority `High`. |
| `DroneCamouflage`, `MissileCamouflage` | `CamouflageDevice` | Chance that drones or missiles ignore the ship during target search. |
| `WormTail` | `WormTailDevice` | Creates a fixed chain of `WormSegment` units. Resistance grows with the number of intact segments. |
| `WormTailV2` | `WormTailDeviceV2` | Creates a self-growing tail with `WormSegmentFactory`. It respawns after `Cooldown × 1.5`. |
| `ClonningCenter` | `ClonningDevice` | Spawns one clone with 90% armor and shield and no drone bays. A new clone is possible only after the old one is destroyed. |

The factory adds the device sound and visual effects as triggers.

## Drone bays, replicators, and clones

- [`DroneBay`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/DroneBays/DroneBay.cs) builds the drone specification once with `ShipBuilder` and the bay multipliers (defense, damage, speed with ±0.2 random spread). Each activation launches one drone through `ShipFactory.CreateDrone`. Capacity limits drones in the hangar plus drones in space.
- [`DroneReplicator`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/DroneBays/DroneReplicator.cs) exists when `DroneBuildSpeed > 0`. Every `1 / DroneBuildSpeed` seconds it asks drone bays to restore one drone. `BuildExtraCycles` adds extra cycles per drone.
- A drone's `Type.Owner` is the mother ship. Drones vanish when the mother ship retreats (legacy AI) or when the behaviour tree says so.
- Clones use `ClonningDevice.ShipSpecDecorator`, which removes drone bays and scales armor, shield, and their recharge rates by 0.9.

## Bullets

Two factories create bullets. Both implement [`IBulletFactory`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/IBulletFactory.cs).

| Factory | Data | Status |
| --- | --- | --- |
| [`BulletFactory`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs) | `Ammunition` (body, controller, impact effects, triggers) | Current format. |
| [`BulletFactoryObsolete`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs) | `AmmunitionObsoleteStats` with `AmmunitionClassObsolete` | Legacy format. Behaviour comes from the class through [`AmmunitionClassExtensions`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/AmmunitionClassExtensions.cs). |

### `BulletFactory.Create` pipeline

1. Take a pooled instance of the bullet prefab (`PrefabCache.GetBulletPrefab`, chosen by `BulletPrefab.Shape`).
2. Configure the body:
   - `AttachedToParent` and an active owner: the bullet is a child of the platform body.
   - Otherwise: world position and rotation from the platform, plus random spread.
   - Non-continuous controllers get velocity `speed × StartingVelocityMultiplier` plus `ParentVelocityEffect × parent velocity`.
3. Configure the view (color, life 0; size 0 for beams).
4. Choose the unit class: `Missile` if `HitPoints > 0`, `EnergyBolt` if impact type is `HitFirstTarget`, otherwise `AreaOfEffect`. The side comes from the owner unless `FriendlyFire` is set; then the side is `Neutral`.
5. Build the collision behaviour from impact effects (table below).
6. Configure the collider: owner, source (to ignore own ship), one-hit mode for non-DoT ammunition, and max range for continuous controllers.
7. Create the unit controller (table below).
8. Create the damage handler: `MissileDamageHandler` (has hit points), `BeamDamageHandler` (continuous), or `BulletDamageHandler`.
9. Build bullet triggers (table below).
10. Add the bullet to the scene, run one view and one physics update, and register it with the platform when it is attached.

Stats come from [`BulletStats`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs):

- Damage and effect power scale with `PowerLevel` (charge level; 1 for other weapons).
- Size and weight scale with `0.5 + 0.5 × PowerLevel`.
- Range scales with `PowerLevel` and is zero below 0.1. When the ammunition has no lifetime, lifetime is range divided by speed.
- `RandomFactor` adds random variation to speed and lifetime.
- Nested bullets also scale range, length, and speed by the area-of-effect multiplier.

### Impact effects

| `ImpactEffectType` | Collision action |
| --- | --- |
| `Damage` | `DealDamageAction` |
| `ProgressiveDamage` | `ProgressiveDamageAction` (DoT only; ramps up over `Factor` seconds) |
| `Push`, `Pull` | `PushAction` (along bullet velocity) |
| `PushFromCenter`, `PullToCenter` | `RadialPushAction` |
| `DrainEnergy` | `DrainEnergyAction` |
| `SiphonHitPoints` | `SiphonHitPointsAction` (returns repair to the owner through the damage handler) |
| `SlowDown` | `SlowDownAction` |
| `CaptureDrones`, `DriveDronesCrazy` | `AffectDroneAction` (changes `Type.Owner` of drones and missiles) |
| `Repair`, `RechargeShield`, `RechargeEnergy` | `RepairAction`, `RechargeShieldAction`, `RechargeEnergyAction` (help allies, harm enemies × `Factor`) |
| `Teleport` | `TeleportAction` (moves target forward; destroys limbs; skips starbases) |
| `DrainShield` | `DamageShieldAction` |
| `Devour` | `DevourAction` |
| `IgnoreShield` | `IgnoreShieldAction` |
| `RestoreLifetime` | `RestoreLifeAction` (bullet starts with less lifetime and gains it on hits) |

With `HitFirstTarget`, the factory adds `DetonateAtTargetAction` (bullet has hit points) or `SelfDestructAction`.

### Unit controllers

| `Ammunition.Controller` | Controller | Behaviour |
| --- | --- | --- |
| `Projectile` | none | Ballistic. |
| `Homing` | `HomingController`, or `MagneticController` when `IgnoreRotation` | Re-target every 2 s and prefer targets in a 30° cone; turn and accelerate. `SmartAim` leads the target. |
| `Beam` | `BeamController` | Child beam with random spread jitter. |
| `AuraEmitter` | `AuraController` | Grows to full size, then shrinks with lifetime. |
| `Parametric` | `ParametricController` | Moves, turns, scales, and sets ray length from database expressions of time. |
| `Harpoon` | `HarpoonController` | Distance joint to owner; fixed joint to the first target; detonates when stretched past 1.5 × range. |
| `StickyMine` | `StickyController` | Sticks to the first target and detonates after its lifetime. |

A non-continuous bullet with a beam shape and positive speed is wrapped in `MovingBeamController`. The beam first grows to its length, then flies forward.

### Triggers

`BulletTriggerCondition` maps to the bullet `ConditionType`:

| Database | Runtime | When |
| --- | --- | --- |
| `Created` | `None` | Once, when the action is added. |
| `Hit` | `OnCollide` | Every reported contact. |
| `Destroyed` | `OnDestroy` | Bullet killed or expired without detonation. |
| `Detonated` | `OnDetonate` | Bullet detonated. |
| `Expired` | `OnExpire` | Lifetime ended. |
| `Disarmed` | `OnDisarm` | Point defense disarmed it (`CanBeDisarmed`). |
| `Cooldown` | `OnCooldown` | Every physics step, limited by `Cooldown`. |
| `OutOfAmmo` | special | Built only for conditions whose spawn triggers hit the nesting limit. |

| Trigger | Action |
| --- | --- |
| `PlaySfx` | sound (`PlayHitSoundAction` on hit; `PlaySoundAction` otherwise) plus a visual effect (`ShowHitEffectAction`, `ShowMultipleHitEffectsAction`, `AttachEffectAction`, or `PlayEffectAction`) |
| `SpawnStaticSfx` | sound plus `SpawnHitEffectAction` or `SpawnEffectAction` |
| `SpawnBullet` | `SpawnBulletsAction` with a nested `BulletFactory`. Nesting stops at `MaxNestingLevel` (default 100). Child factories are cached per trigger. |
| `Detonate` | `DetonateAction` |
| `GravityField` | `CreateGravitationAction` |

Triggers with a cooldown are wrapped in `ActionWithCooldown`. Bullet actions are in [`Unit/Bullet/Action/`](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Bullet/Action/).

### Bullet life cycle

[`Bullet`](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Bullet/Bullet.cs):

1. Each physics step: unit controller, body, lifetime, collider, cooldown actions; then `Expire` when lifetime runs out.
2. `OnCollision`: damage handler, then `OnCollide` actions. A `Disarm` flag disarms the bullet if allowed. A `Destroy` flag detonates it (`DetonateWhenDestroyed`) or destroys it.
3. `Detonate`, `Destroy`, and `Disarm` first mark the bullet `Destroyed`, then run their actions. This order prevents spawned child bullets from attaching to a dead parent.
4. `Expire` runs `OnExpire` actions while the bullet is temporarily marked destroyed. If any action returns `Destroy`, the bullet detonates; otherwise it is destroyed.

`BulletFactoryObsolete` builds the same bullet type. It maps each legacy ammunition class to fixed actions: explosions, clouds, EMP, webs, cluster fragments, carrier payloads, and black holes.

## Collision and damage

### Colliders

| Collider | Used by | How it reports |
| --- | --- | --- |
| [`CommonCollider`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Collider/CommonCollider.cs) | almost all prefabs | Unity callbacks `OnCollisionEnter2D`/`Stay2D` and `OnTriggerEnter2D`/`Stay2D`. It ignores units whose owner is its `Source`. After 1 s of continuous contact it pushes non-static bodies apart. `OneHitOnly` and `_ignoreTriggerStayEvent` skip stay events. |
| [`RayCastCollider`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Collider/RayCastCollider.cs) | beams (7 prefabs) | Raycast of `MaxRange` each step. Stops at the first hit unless pass-through. Sets the beam view length. |
| [`CircleCollider`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Collider/CircleCollider.cs) | point defense | Overlap circle of `MaxRange` each step. |

`CollisionData` holds the contact point, relative velocity, `TimeInterval` (the last physics delta), and `IsNew` (first frame of this contact).

### Collision manager

[`CollisionManager.OnCollision(first, second, data)`](../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Manager/CollisionManager.cs):

1. Return if `first` has no collision behaviour, or either unit is not active.
2. Create two empty impacts.
3. Run `first.CollisionBehaviour.Process` to fill both impacts.
4. Call `first.OnCollision(selfImpact)` and `second.OnCollision(targetImpact)`.

Only the reporting unit's behaviour runs. With Unity callbacks, both colliders in a contact report, so each unit applies its own behaviour once. Raycast and circle colliders report only from their own side.

### Behaviours

| Behaviour | Used by | Effect |
| --- | --- | --- |
| [`DefaultCollisionBehaviour`](../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/DefaultCollisionBehaviour.cs) | ships, asteroids | Ramming: on a new contact with a hostile unit, impact damage `0.1 × |relative velocity| × min(weights) × RammingDamageMultiplier` if it exceeds 1. |
| [`BulletCollisionBehaviour`](../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/BulletCollisionBehaviour.cs) | bullets, explosions, clouds | Runs a list of collision actions in order. |
| `DockingStationCollisionBehaviour` | exploration objects | Sets `Activate` when the player ship touches it and slows the ship. |
| `GravityBombCollisionBehaviour` | gravity bomb | Destroys missiles, bolts, and areas; pushes everything. |
| `LootCollisionBehaviour` | – | Not used by any factory. |

### Impact types

Collision actions follow the bullet's `BulletImpactType`:

- `HitFirstTarget`: act once, on the first new contact. Later contacts do nothing.
- `HitAllTargets`: act once per new contact.
- `DamageOverTime`: act every report, scaled by `TimeInterval`.

### Impact

[`Impact`](../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Impact.cs) has kinetic, energy, heat, corrosive, shield damage, repair, energy drain, an "ignores shield" flag, a list of impulses, and `CollisionEffect` flags (`Destroy`, `Disarm`, `Activate`). `AddDamage` throws when the amount is negative. [`Resistance`](../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Resistance.cs) applies `damage × (1 − resistance)`. Corrosive damage uses half of the smallest resistance.

Damage handlers ([`Component/DamageHandler/`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/DamageHandler/)) let non-ship units react to impacts. `EnergyShieldDamageHandler` pays damage with the parent ship's energy and passes the rest to the ship. `MissileDamageHandler` subtracts damage only from missiles, bolts, and areas, so that ramming does not destroy harpoons.

## Space objects and area effects

[`SpaceObjectFactory`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Objects/SpaceObjectFactory.cs) creates objects that are not ships or bullets:

| Method | Result |
| --- | --- |
| `CreateExplosion`, `CreateSmokeExplosion`, `CreateStrongExplosion`, `CreateFlash` | `ShortLivedObject` area with `ApplyExplosionDamageAction` (damage and impulse fall off with distance), visual effects, and screen shake. |
| `CreateEmp` | Area with damage, energy drain, and shield damage. |
| `CreateCloud` | Drifting area with corrosive or other DoT damage. |
| `CreateWeb` | Area with `AreaEffector2D` drag and DoT damage. |
| `CreateGravitation` | Child area with `PointEffector2D` attraction (black holes, gravity triggers). |
| `CreateGravityBomb` | Repulsive area for `GravityGenerator`. |
| `CreateDecoy` | `Decoy` unit. |
| `CreateWormTail`, `CreateWormTailSegment`, `CreateSpawnEffect` | Worm tail segments. |
| `CreateAsteroid`, `CreatePlanet` | Combat background objects. |
| `CreatePlanetary*` | Exploration objects with `DockingStationCollisionBehaviour`; `CreatePlanetaryGasCloud(…, dps)` creates a damaging cloud. |

## Triggers

Units and systems run event-driven behaviour through [`UnitTriggers`](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/UnitTriggers.cs).

- Events: `OnDestroy`, `OnHit`, `OnActivate`, `OnDeactivate`, `OnRemainActive`, `OnDischarge` ([`ConditionType`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Triggers/ConditionType.cs)).
- `IUnitAction` runs in the physics step. `IUnitEffect` runs in the view step.
- `Invoke(condition)` calls every trigger whose condition contains the event. If the trigger returns `true`, it joins the active list and gets per-frame updates until `TryUpdate…` returns `false`.
- A trigger with `TriggerCondition == None` is active from the start.

Common triggers: explosion effects (`ShipExplosionAction`, `DroneExplosionActionObsolete`, `AsteroidExplosionAction`), wrecks (`ShipWreckAction`), engine lights (`EngineLightEffect`), shield glow (`ShieldEffect`), flashes (`FlashEffect`, `StaticEffect`, `PowerLevelEffect`), sounds (`SoundEffect`, `SoundLoopEffect`), and device helpers (`AuxiliaryUnitAction`, `PointDefenseAction`, `RepairBotAction`). The application layer uses `PlayerDockingAction`, `UnitDestroyedAction`, `ShipRetreatingEffect`, and `ShipWarpEffect`.

## Views, effects, and prefabs

### Views

Views are `MonoBehaviour` components on unit prefabs. [`BaseView`](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/View/BaseView.cs) stores position, rotation, size, color, and life, and applies only changed values in `UpdateView`. Concrete views: `ShipView` and `OrganicShipView` (HSV material), `SpriteView`, `RollingSpriteView`, `LaserView`, `LightningView`, `ArcView`, `CircularSawView`, `ParticleSystemView`, `TrailView`, `MeshView`, `AsteroidView`, `CircleView`, `ShipSegmentView`, `ComplexView`, and `EmptyView`.

### Effects

Effects are short-lived visuals that are not units.

- [`IEffect`](../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/IEffect.cs): position, rotation, size, color, life, `Run(lifetime, velocity, angularVelocity)`, `Dispose`.
- [`EffectBase`](../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/EffectBase.cs): pooled prefab effect. After `Run`, it moves and fades by itself and disposes when life reaches zero.
- [`ViewEffect`](../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/ViewEffect.cs): effect that drives an `IView` (lightning).
- [`CompositeEffect`](../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/CompositeEffect.cs): data-driven effect from a database `VisualEffect`. It creates one child effect per element and quantity, and schedules each by `StartTime`, `Lifetime`, `Loop`, `Inverse`, `GrowthRate`, and `TurnRate`. It is a new `GameObject` each time (not pooled).
- [`ShakeEffect`](../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/ShakeEffect.cs): shakes the camera through `IScene.Shake`.

[`EffectFactory`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/EffectFactory.cs) creates effects by name (`Combat/Effects/<name>`), by `PrefabId`, or by database element. It also creates damage numbers, text, and screen shake. `IsObjectVisible` always returns `true`; the visibility check is commented out.

### Prefab cache and pools

- [`PrefabCache`](../Assets/Modules/BattleSimulator/Scripts/Combat/Services/PrefabCache.cs) loads prefabs from `Resources` and caches them. It also builds template prefabs from database data:
  - Bullet prefabs: one template per `BulletPrefab` ID. The shape selects the base prefab (`CommonProjectile`, `CommonRocket`, `Laser`, `Lightning`, `EnergyBeam`, `PiercingLaserBeam`, `Spark`, `Mine`, `Arc`, `WormHole`, `Hook`, `CircularSaw`). `IBulletPrefabInitializer` applies sprite, colors, and size.
  - Effect prefabs: one template per element type and image. `IEffectPrefabInitializer` applies the data.
  - Custom object prefabs (`GameObjectPrefab`): worm segments and shields, through `CustomPrefabLoader`.
  - The cache clears database-based templates when a database loads.
- [`GameObjectHolder`](../Assets/Modules/BattleSimulator/Scripts/Combat/Helpers/GameObjectHolder.cs) takes a GameObject from `IObjectPool` and returns it on `Dispose`. Components added at run time are destroyed on release.
- [`TrailRendererPool`](../Assets/Modules/BattleSimulator/Scripts/Combat/Services/TrailRendererPool.cs) reuses trail renderers after their trail has faded. In power-saving quality mode it returns `null`, so trails are off.
- [`MaterialCache`](../Assets/Modules/BattleSimulator/Scripts/Combat/Services/MaterialCache.cs) shares one HSV material per hue and saturation pair.

## Target selection

[`ShipListExtensions`](../Assets/Modules/BattleSimulator/Scripts/Combat/Scene/ShipListExtensions.cs) holds the shared search code.

`GetEnemy(unit, options)` scans active hostile ships with target priority above `None`:

1. Skip drones if `IgnoreDrones`. Skip decoys if the "ignore decoy" roll succeeds. Skip ships whose drone camouflage roll succeeds (`UseDroneCamouflage`).
2. Skip ships farther than `MaxDistance` from the unit, or from `whereToLook` (for example the mother ship).
3. Prefer a decoy when the "take decoy" roll succeeds.
4. Otherwise score candidates: `300 × priority difference + 300 × relative distance advantage + 200 × relative size advantage`. The higher score wins.

`EnemyMatchingOptions` presets:

| Preset | Ignore decoys | Take decoys | Drone camouflage |
| --- | --- | --- | --- |
| `EnemyForShip` | always | never | no |
| `EnemyForDrone` | never | 30% | yes |
| `EnemyForSatellite` | never | never | no |

`GetEnemyForTurret` prefers targets inside range and arc. It keeps tracking a target up to 2 × range. `GetEnemyForMissile` prefers targets inside range and a deviation cone, then higher priority, then nearer. With `trueVision` it ignores decoys, stealth, and camouflage.

`GetObjectsInRange` returns root units within a radius. A second overload returns child units in a separate list.

## AI

### Ship controllers

[`ControllerFactory`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ControllerFactory.cs) selects a controller. A database behaviour tree always wins over the legacy AI.

| Ship | First choice | Fallback |
| --- | --- | --- |
| Player | `KeyboardController` | – |
| Player autopilot | `CombatSettings.AutopilotAI` tree | `Computer` with level 100 in autopilot mode |
| Enemy | `spec.CustomAi`, then `CombatSettings.EnemyAI` | `Computer` with the AI level; `EmptyController` when the level is negative |
| Clone | `spec.CustomAi`, then `CombatSettings.CloneAI` | `Clone` |
| Starbase, turret | `spec.CustomAi`, then `CombatSettings.StarbaseAI` | `Starbase` |
| Drone | bay tree for the behaviour, then `Drone.CustomAI`; without `ImprovedAi`, `CombatSettings.Offensive/DefensiveDroneAI` | `Drone` |

`AiManager` removes a controller when its `Status` is `Dead`. `Idle` has no effect on scheduling.

### Behaviour tree

Files: [`AI/BehaviorTree/`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/).

Build:

1. [`BehaviorTreeBuilder.Build`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/BehaviorTreeBuilder.cs) creates `ShipCapabilities` (one `WeaponWrapper` per weapon), a `NodeBuilder`, and a `Context`.
2. [`NodeBuilder`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/NodeBuilder.cs) visits the database node tree. It first checks the node `Requirement` with [`RequirementChecker`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs) (devices, drones, weapon types, AI level, size class, engine, and more). A node whose requirement fails becomes `EmptyNode.Failure`.
3. Composite nodes drop children that are constant failures or successes where the result is fixed. A selector or sequence with one child becomes that child. Nodes that need a missing device or weapon return `EmptyNode.Failure` from their `Create` method.
4. AI level comes from `AiSettings`: below 50 is Easy, below 80 is Medium, otherwise Hard. Drones use Hard and a limited range.

Run (every AI step):

1. `Context.Update` stores the time step, the AI options, and clears locked energy.
2. Evaluate the root node. Nodes return `Running`, `Success`, or `Failure`.
3. If the result is not `Failure`, apply the collected controls to the ship. If it is `Failure`, reset the controls; apply them once more if the previous step succeeded, so that the ship stops cleanly.
4. If cease-fire is on, clear the ship systems.

`ShipControls` accepts the first course and the first thrust that a node sets in a step. Later nodes cannot override them. Systems are an OR of all requests.

[`Context`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/Context.cs) holds the main target (`TargetShip`), mother ship, secondary target list (up to 5 nearest enemies), threat list, selected weapons, 64 boolean flags, saved targets by name, the last message sender, and energy accounting (`LockedEnergy` reserves energy for charging weapons).

Node groups (files in [`Nodes/`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/Nodes/)):

| Group | Nodes |
| --- | --- |
| Composite | `Selector`, `Sequence`, `Parallel`, `ParallelSequence`, `RandomSelector`, `IfThenElse`, `Invertor`, `Cooldown`, `Execute`, `PreserveTarget`, `SubTree` |
| Targeting | `FindEnemy` (nearest, in attack range, or near mother ship), `FindDamagedAlly`, `TargetMothership`, `TargetAllyStarbase`, `TargetEnemyStarbase`, `TargetMessageSender`, `SaveTarget`, `LoadTarget`, `ForgetMainTarget`, `ForgetSavedTarget`, `MakeTargetMothership`, `LookForAdditionalTargets`, `LookForThreats` |
| Movement | `MoveToAttackRange`, `MaintainAttackRange`, `KeepDistance`, `FlyAroundMothership`, `Chase`, `Ram`, `EscapeTargetAttackRadius`, `MatchVelocityWithTarget`, `SlowDown`, `AvoidThreats`, `BypassObstacles`, `LookAtTarget` |
| Weapons | `SelectWeapon`, `AttackMainTarget`, `AttackAdditionalTargets`, `AttackTurretTargets`, `ChargeWeapons`, `SustainAim`, `TrackControllableAmmo`, `HoldHarpoon`, `UseRecoil` |
| Devices and ship | `ActivateDevice`, `SpawnDrones`, `DetonateShip`, `DefendWithFronalShield`, `RechargeEnergy`, `Vanish`, `GoBerserk` |
| Conditions | `HasMainTarget`, `HasSavedTarget`, `HasAdditionalTargets`, `MainTargetIsAlly`, `MainTargetIsEnemy`, `MainTargetLowHp`, `MainTargetWithinAttackRange`, `TargetDistance`, `HasLongerAttackRange`, `IsFasterThanTarget`, `IsLowOnHp`, `HasEnoughEnergy`, `HasIncomingThreat`, `EnginePropulsionForce`, `IsNotControledByPlayer`, `HasMothership`, `MotherShipRetreated`, `MotherShipDestroyed`, `MothershipDistanceExceeded`, `MothershipLowHp`, `MothershipLowEnergy`, `MothershipLowShield`, `Wait` |
| Special | `SetValue`, `GetValue`, `SendMessage`, `MessageReceived`, `ShowMessage`, `DebugLog`, `Success`, `Failure` |

Weapon handling uses three strategies per weapon from [`WeaponWrapper`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/Capabilities/WeaponWrapper.cs):

- Aiming: none (homing, area, trap), direct (beams, low AI level), or with lead prediction (projectiles and harpoons at Hard level).
- Activation: always, in range, aimed roughly, or aimed precisely (slow projectiles without auto-aim).
- Controlling: when a held weapon should stay active (charging, continuous, manageable, harpoon).

[`AimAndAttackHandler`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Calculations/AimAndAttackHandler.cs) applies these strategies and picks the course of the weapon that needs the smallest turn. [`ShipNavigationHandler`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Calculations/ShipNavigationHandler.cs) implements fly-around, keep-distance, chase-and-ram, and velocity matching.

Ships communicate through [`MessageHub`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/Utils/MessageHub.cs). `SendMessage` delivers to listeners of the same name on allied ships. The hub and its message IDs are shared by all trees in the scene.

[`ThreatList`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/ThreatList.cs) collects hostile non-ship units within 20 units whose path crosses the ship within 5 s, plus overlapping child area effects (beams and auras). `TimeToHit` is the smallest crossing time.

### Legacy strategy AI

The older AI is still the fallback when the database has no tree.

- [`Computer`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Computer.cs): picks an enemy every 5 s with `GetEnemyForMissile(trueVision)`. Picks a strategy every 10 s with [`StrategySelector`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Strategy/Factories/StrategySelector.cs), which scores `CloseRange`, `LongRange`, and `Ramming` (`HitAndRun` is disabled) and adds random variation. In autopilot mode it waits 2 s after any player input.
- [`Drone`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Drone.cs), [`Clone`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Clone.cs), [`Starbase`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Starbase.cs): fixed strategies.
- A strategy is an ordered list of `Policy(condition, action, oppositeAction)`. Conditions are in [`AI/Conditions/`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Conditions/). Actions are in [`AI/Actions/`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Actions/). Rule builders (`AttackRules`, `DefenseRules`, `MovementRules`, `SpecialRules`) add groups of policies.
- `AwakeCondition` makes lower AI levels attack only part of each 5 s cycle (`10 + 3 × level` percent).
- Many legacy actions use the obsolete `BulletEffectType` to classify weapons.

## Player input

- [`KeyboardController`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/KeyboardController.cs) is a ship controller, so it runs on the AI thread. It maps throttle, left/right turn (course ±175°), joystick direction, and mouse direction to controls. Key slot `i` activates all systems whose `KeyBinding` sorts into slot `i` (`GetKeyBindings` groups systems by binding value). It fires `PlayerInputSignal` when the player does anything.
- [`InputSystemKeyboard`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Keyboard.cs) reads the `Combat` action map with six fire actions. It reloads bindings on `KeyBindingsChangedSignal`.
- [`InputSystemMouse`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Mouse.cs) reads the `Mouse` action map: look, thrust, and two fire buttons. It goes inactive after 0.5 s without input and ignores clicks over UI.
- On-screen controls in `Assets/Scripts/Gui/Combat/` write `ship.Controls` directly from the main thread.

## Integration with the application layer

| Application code | Uses |
| --- | --- |
| [`CombatManager`](../Assets/Scripts/Combat/Manager/CombatManager.cs) | `ShipFactory` to spawn fleets, `SpaceObjectFactory` for asteroids and planets, `ShipRetreatEffect` with `ShipRetreatingEffect`, `ShipWarpEffect`, `SoundLoopEffect` for retreat, `IScene.Clear` on exit. |
| [`ExplorationSceneManager`](../Assets/Scripts/Combat/Manager/ExplorationSceneManager.cs) | `CreatePlanetary*` objects, `PlayerDockingAction`, `UnitDestroyedAction`. |
| [`EnemySpawner`](../Assets/Scripts/Combat/Unit/Object/EnemySpawner.cs) | `ShipVanishEffect` to remove ships that drift too far from the player. |
| [`CombatModel`](../Assets/Scripts/Combat/Domain/CombatModel.cs), `SignalsTranslator` | `ShipDestroyedSignal`, `ShipCreatedSignal`. |
| `ShipControlsPanel` | Writes `ship.Controls` from on-screen buttons and fires `PlayerInputSignal`. |
| `Radar` | `IScene.ViewPoint` and the unit lists. |
| `CombatCamera` | `IScene.ViewRect` (with shake offset). |

`IScene.Clear` disposes every unit at once. It does not change unit states and does not fire `ShipDestroyedSignal`.

## Resources

All runtime prefabs are in [`Resources/Combat/`](../Assets/Modules/BattleSimulator/Resources/Combat/) and load by path.

| Folder | Content | Load rule |
| --- | --- | --- |
| `Ships/` | 146 prefabs, including `Default` | `Combat/Ships/<ModelImage.Id>`, fallback `Default` |
| `Satellites/` | Companion prefabs plus `default` | `Combat/Satellites/<ModelImage.Id>`, fallback `default` |
| `Bullets/` | Base bullet shapes and legacy bullets | By shape (`PrefabCache.GetBulletPrefab`) or legacy `PrefabId` with fallback `Laser` or `Plasma2` |
| `Effects/` | Flash, smoke, wave, spark, shields, trails, text, wreck | `EffectFactory.CreateEffect(name)` |
| `Objects/` | Area, decoy, shields, point defense, repair bot, gravitation, planet, worm segments | Factory constants |
| `Guns/` | `turret`, `arm` | Turret sprites on platforms |
| `Exploration/` | Planet surface objects | `SpaceObjectFactory.CreatePlanetary*` |

A ship prefab must have a body (`IBodyComponent`), a view (`IView`), a collider (`ICollider`), and a `PhysicsManager`. A bullet prefab also needs a `BulletDependencyInjector` (it injects the collision manager and trail pool) and an `IBulletPrefabInitializer` when it is a shape template.

## How to extend the module

### Add a device

1. Add the `DeviceClass` value and data fields in the database schema, then run the code generator (see [Database module](database.md)).
2. Create a class in `Component/Systems/Devices/` that derives from `SystemBase` and implements `IDevice`. Set `DeviceClass` in the constructor.
3. Implement activation in `OnUpdatePhysics`: check `Active && CanBeActivated`, spend energy with `TryGet`, set `TimeFromLastUse = 0`, and call `InvokeTriggers(OnActivate)`.
4. Return modifications (`EngineModification`, `FeaturesModification`, `StatsModification`, `SystemsModification`) or a `UnitAction` when the device changes the ship.
5. Add a `case` to `DeviceFactory.Create`. Add effects and sounds as triggers there.
6. If AI should use it, add a requirement or node in the behaviour tree code and, if needed, a rule in the legacy strategies.

### Add an impact effect

1. Add the `ImpactEffectType` value in the schema.
2. Create an `ICollisionAction` in `Collision/Behaviour/Action/`. Respect `BulletImpactType` (`IsNew`, `_isAlive`, `TimeInterval`).
3. Map the type in `BulletFactory.CreateCollisionBehaviour`. Update `BulletStats.Capability` if AI should know about it.

### Add a bullet trigger

1. Add the `BulletTrigger_*` type in the schema.
2. Add a `Create` overload in `BulletFactory.BulletTriggerBuilder`.
3. Create an `IAction` in `Unit/Bullet/Action/` when a new action is needed.

### Add a behaviour tree node

1. Add the `BehaviorTreeNode_*` type in the schema.
2. Create an `INode` in the right `Nodes/` folder. Keep it free of Unity API calls; it runs on the AI thread.
3. Add a `Create` overload in `NodeBuilder.NodeFactory`. Return `EmptyNode.Failure` when the ship cannot use the node.
4. Add requirement types to `RequirementChecker` when needed.

### Add a unit type

1. Derive from `UnitBase`. Implement `State`, `Vanish`, `OnCollision`, and the update hooks.
2. Choose a `UnitClass` that maps to the right collision layer.
3. Create it in a factory with a pooled `GameObjectHolder`, add the holder as a resource, and call `IScene.AddUnit`.

## Validation checklist

The module has no test assembly. Validate changes in Play Mode.

1. Open `CombatScene` and check that `CombatSceneInstaller` has all serialized references.
2. Exercise the affected creation paths: player, enemy, drone, clone, satellite, starbase, and turret.
3. Check physics and view behaviour, including pause, time scale, destruction, retreat, and scene exit.
4. Test collisions at low and high relative velocity. Check that pooled objects return cleanly.
5. Spawn more than 30 AI ships to enter the reduced-threat path.
6. For AI changes, test the threaded path (standalone or mobile) and WebGL.
7. For database-driven changes, test with the default database and with a mod.
