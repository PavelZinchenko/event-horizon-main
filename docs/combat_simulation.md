# Combat Simulation

`Assets/ModulesShared/CombatSimulation/` is the new battle simulator. It will replace `Assets/Modules/BattleSimulator/` ([battle simulator](battle_simulator.md)). Event Horizon and Frontier share one copy through the `ModulesShared` submodule ([Gui: Shared code constraint](gui.md#shared-code-constraint)).

Status: in progress. Physics and the phase 2 core (steps 1–11) exist; phase 2 is complete: a scripted battle runs on the core rules without Unity. Core provides the fixed tick loop, spawn identity, the core-owned roster, roster commands, the battle result, the timer, ships with an engine, resources and control frames, the published snapshot and event batch, the contact pipeline with damage and ramming, the bullet store with the cannon, homing missile and beam, point defense, drone bays with persistent inventory, and the per-tick target grid; Runtime runs pilots in locked mode, including drone self-destruct requests. Stage P1 is closed (2026-10-17, with the known limits below). It has the stage P1 features: torus broad phase, circles, convex polygons, a soft-step contact solver with speculative contacts, and a free-position query. Stage P2 is closed (2026-10-18, with the known limits below): capsules, convex decomposition, compound bodies, whole-body depenetration, inner-edge contacts and the time-of-impact check of speculative contacts. Stage P3 is closed (2026-10-19, with the known limits below): collision filters, bullets with swept hits and pass-through, ray casts and circle overlaps. Stage P4 is closed (2026-10-19, with the known limits below): hinges, ropes, bullets attached to bodies, group teleport, and contact and joint impulses. The plan and its progress are in `TODO/battle_simulator_implementation.md`; stage results in `TODO/battle_simulator_physics.md`. The findings and decisions are in `TODO/battle_simulator_investigation.md`. The `TODO/` folder is local and is not part of this repository.

## Assemblies

| Assembly | Folder | References |
| --- | --- | --- |
| `CombatSimulation.Physics` | `Physics/` | `Utilites` |
| `CombatSimulation.Physics.Tests` (Editor) | `Physics/Tests/` | `CombatSimulation.Physics`, `Utilites` |
| `CombatSimulation.Core` | `Core/` | `CombatSimulation.Physics`, `Utilites`, `Diagnostics` |
| `CombatSimulation.Core.Tests` (Editor) | `Core/Tests/` | `CombatSimulation.Core`, `CombatSimulation.Physics`, `CombatSimulation.Runtime`, `Utilites` |
| `CombatSimulation.Runtime` | `Runtime/` | `CombatSimulation.Core`, `CombatSimulation.Physics` (public snapshot types expose `Vec2`) |

Rules:

1. Physics, Core and Runtime are engine-free (`noEngineReferences: true`). Follow the [engine-free module rules](architecture.md#engine-free-modules).
2. The new core and the old `BattleSimulator` do not reference each other, in either direction.
3. Phase 2 does not reference `Database` or `ShipConstructor`. Phase 4 adds the database path using contracts shared with Frontier.
4. Unit tests go into `Physics/Tests/` or `Core/Tests/` with an Editor-only asmdef. `Tests~/EngineFree.Tests.csproj` compiles both. Runtime tests live in `Core/Tests/`.
5. Core grants internal access only to Runtime and Core tests. Core does not reference Network or the application.

## Core foundation

`BattleSetup` holds immutable battle ID, match seed, roster, arena dimensions, time limit and the active-ship limit per side. Dimensions default to 200 × 200 and must be finite and positive.
The roster is a read-only copy of the caller's slot list. Each `RosterSlotSetup` has a unique roster ID, a side (0 or 1), an owner (player index, 0 in phase 2) and a `ShipDefinition`. Each side needs at least one slot.
`ShipDefinition` holds hull radius, mass, the engine, an optional engine without energy, maximum armor, the shield and energy `ResourceDefinition` (maximum, recharge rate, recharge delay), barrels and devices. The hull is one circle until phase 4 loads collider outlines.
`EngineDefinition` holds derived stats in core units: propulsion (u/s²), maximum velocity (u/s), turn rate (rad/s²) and maximum angular velocity (rad/s). Phase 4 derives them from engine power and weight.
Each `Battle` owns a separate `PhysicsWorld`. Physics state is internal; the public surface exposes setup, battle ID, tick, `IsCompleted`, `Snapshot`, `Submit(RosterCommand)`, `Submit(ControlFrame)` and `Step()`.

`Tick` identifies the next tick to execute. It starts at zero. The first `Step()` processes tick zero and leaves `Tick` at one.
The stage order is commands, systems, modifiers, motion, spawn requests, queries, physics, contacts, response, damage, deaths, structure, battle flow, publish.
Only the modifiers stage is empty. Stage 6 queries beam rays and point-defense overlaps before physics advances by `0.02f` seconds with eight substeps.
Publish writes the snapshot and event batch of the current tick, then increments it.
Runtime owns pilot scheduling; pacing against a wall clock does not exist yet.

### Identity

`BattleId` and `EntityId` wrap unsigned 64-bit values. `RosterId` wraps an unsigned 32-bit value.
They provide value equality, unsigned comparison and allocation-free hash codes. Entity ID zero means no entity.
Battle and roster ID zero remain valid. The caller supplies battle IDs; the core does not generate them from a clock.

Internal `SpawnIdentity` hashes these fields using 64-bit FNV-1a, then applies the Murmur3 `fmix64` finalizer:

| Spawn | Fields, in order |
| --- | --- |
| Root | Tag byte 1, battle ID (64 bits), roster ID (32 bits), deployment number (32 bits), spawn tick (64 bits) |
| Child | Tag byte 2, parent entity ID (64 bits), spawn tick (64 bits), source index (32 bits), slot (32 bits), sequence (32 bits) |

All numeric fields use little-endian byte order. The offset basis is `14695981039346656037`; the prime is `1099511628211`.
Arithmetic wraps at 64 bits. A zero result becomes one.
FNV-1a alone leaves each low output bit a function of the same low input bits. The finalizer is a bijection that mixes every bit, so `Value mod P` is safe for think offsets.
The source index is the device index on a ship parent and the trigger index on a bullet parent. The slot is the barrel or bay index; a bullet parent uses 0.
The sequence counts spawns with the same parent, tick, source and slot, in creation order. It covers a `SpawnBullet` trigger with `Quantity` > 1 and a device that fires more than once in a tick. Spawn ticks must be nonnegative; a child requires a nonzero parent ID.
A roster deployment creates one root, so its spawn slot is implicit. The caller changes the deployment number for each redeployment. The spawn tick is redundant with the deployment number; it stays so that roots and children use the same tick rule.
The hash uses no allocation history, shared random sequence, or runtime hash code. Match seed affects future random behavior, not identity.
Fixed test vectors pin the byte layout. The network gate must still validate this derivation.
The `StableHash` helper holds the FNV-1a and `fmix64` code; spawn identity and counter-based random share it.

Ships are pooled `ShipEntity` objects in a `ShipStore` with a LIFO free list. The internal storage handle is kind, index and generation. `EntityRegistry` maps each live `EntityId` to its handle.
Registering an ID that is already alive is a defect. It throws when the internal `ThrowOnDefect` flag is set (tests set it). Otherwise it logs through `GameDiagnostics.Trace` and the spawn fails: the deployment is rejected and the slot stays `Ready`.
A removed entity's ID leaves the registry and never returns to life. Stages visit roster ships in setup order, then drones in entity ID order. Storage allocation order does not determine processing order.

### Counter-based random

`CounterRandom.Create(matchSeed, entityId, tick, purpose)` returns a `Utilites.RandomState` seeded with `PcgRandomAlgorithm.InitializeState` of a `StableHash` over tag byte 3, seed (64 bits), entity ID (64 bits), tick (64 bits) and purpose (32 bits).
The caller draws a fixed sequence from that state. A draw never depends on how many draws other entities made. Purposes are appended to `RandomPurpose`; existing values never change.

### Roster

A slot is `Ready`, `Active` or `Destroyed`. Its status is stored; it is never inferred from whether an entity exists. A pending deployment is a command, not a status.
Each slot counts its deployments, records its battle-wide deployment order (the last activated ship has the highest) and increments a revision on every transition.

| Transition | When | Entity |
| --- | --- | --- |
| `Ready` → `Active` | Deploy, stage 1 | Spawn with root ID `(battle, roster, deployment count + 1, tick)` |
| `Active` → `Ready` | Retreat completion, stage 12 of tick `start + 350` | Removed; the slot keeps the armor and shield deficits (B4) |
| `Active` → `Destroyed` | Death, stage 12 | Removed |
| Any → `Destroyed` | Surrender (own side) or kill-all (other side), stage 12 | Only active entities removed; reserves get no entity |

Stage 12 applies one batch in roster order: deaths, then retreat completions, then surrender and kill-all. A death in the completion tick wins over the retreat. A repeated destruction is a no-op.
A deployment in the surrender tick is destroyed in the same batch. Death has no damage source yet: the internal `QueueDeath(EntityId)` feeds stage 11, and the damage stage will use the same list.

Spawn placement follows legacy `Scene.FindFreePlace` and `ShipInfo.Create`, with counter-based random (purpose `SpawnPlacement`, the new root ID and the tick):

1. Rotation: a whole degree from 0 to 359.
2. Up to 100 uniform candidates over the torus. The first candidate at 40 units or more from every ship of the other side wins; otherwise the candidate farthest from them.
3. `TryFindFreePosition` moves the body up to 40 units to clear any overlap, allies included.

### Roster commands

`RosterCommand` carries battle ID, command ID, target tick and kind: `Deploy(rosterId)`, `Retreat(entityId)`, `Surrender(side)` or `KillAll(requestingSide)`.
`Submit` rejects a wrong battle ID, a past tick, an unknown kind, an invalid side and a command ID that is already pending. It returns `false` and logs. It throws after the result.
The buffer stays sorted by target tick and command ID. Stage 1 applies the commands of its tick in command ID order and validates them against committed state:

1. Deploy: the slot is `Ready` and its side is below `MaxActiveShipsPerSide` (default 1).
2. Retreat: the entity is a live roster ship with no retreat running. The completion tick is fixed at `tick + 350`.
3. Surrender and kill-all: the side is marked for stage 12.

A rejection logs a warning and increments the internal `RejectedCommandCount`. Phase 2 has no rejection event.

### Control frames

`ControlFrame` carries battle ID, command ID, target tick, entity ID, a course in radians or none, a throttle and 64 system bits (bit = device index).
Command IDs share one pending namespace with roster commands and self-destruct commands.
`Submit` also rejects a null entity, a throttle outside [0, 1] or NaN, a non-finite course and a second frame for the same entity and tick.
The entity is checked at stage 1, after the roster commands: a frame can name the derivable root ID of a ship that deploys in the same tick.
A frame for an unknown or removed entity is dropped, logged and counted in `RejectedCommandCount`.

1. A frame stays in force until the next frame for that entity. A deployment starts with empty controls.
2. At stage 1, before the frames, each ship copies its system bits to `PreviousSystems`. `PressedSystems` and `ReleasedSystems` give the edges; the driver sends no events.

### Ship resources

`ResourcePool` ports legacy `Energy` (`IResourcePoints.cs`) with absolute values. A pool starts full with no delay.

1. Stage 2 updates energy, armor and shield, in that order, before any device. Only the part of the step after the delay recharges.
2. A spend with a positive rate restarts the full delay. A failed `TryConsume` changes nothing. `Consume` clamps and returns the removed amount; a negative amount adds.
3. A negative rate drains (net consumption). A zero maximum means no resource.
4. Armor has rate 0: it never regenerates and never starts a delay (Q7).
5. B4: stage 12 stores the absolute armor and shield deficits on the slot at retreat completion. Deploy applies them as a spend, as legacy `ShipInfo.Create`, so a shield deficit starts the shield delay at deployment. Energy starts full.

### Engine

`ShipEngine` ports legacy `ShipEngine` in radians. The legacy formulas are linear in the angle unit, so the conversion changes no result. Stage 4 runs it for active roster ships, then drones in entity ID order.

1. The engine reads `Energy > 0` sampled at the start of stage 2, before the recharge. Without energy it uses the engine without energy; without one, the ship neither thrusts nor turns.
2. Thrust only above throttle 0.01. It steers the velocity toward `MaxVelocity` along the heading: a soft cap that keeps faster forward motion. The velocity changes by `acceleration × dt` before the physics step.
3. With a course, the legacy turn controller brakes inside 120° when the spin would overshoot, and drops to a tenth of the turn rate above the maximum angular velocity. Without a course it brakes the spin.
4. A retreating ship has its engine off and drifts (legacy `ShipRetreatEffect`).
5. Body damping is 0, as the legacy prefab drag.

### Contacts and damage

Stages 8–11 run one pipeline for every contact source. Physics only reports; the core selects the valid hits. Ramming, bullet sweeps, beam rays and point-defense overlaps share this pipeline.

1. **Shape table.** Physics shapes carry no user data. `ShapeTable` maps each live `ShapeId` to its entity, its hit priority (`Shield` before `Hull`) and its order in the definition. A lookup checks the shape generation. A stale `ShapeId` therefore resolves to nothing: the delayed end event of a destroyed body, or a reused index. Deploy adds the hull circles in `ShipDefinition.HullParts` order: the main circle first, then the extra parts. Removal releases them.
2. **Candidates.** Each source adds `ContactCandidate`s to `ContactResolver` at stage 8. The sort key is a tuple, not one packed number, so the 64-bit IDs stay intact. The key order is: time (a fraction of the step), kind (`Disarm` before `Hit`), attacker ID, attacker device index (point defense; -1 otherwise), ray distance fraction, target shape priority, target ID, target shape order. Non-ray sources use zero for ray distance. Beam time stays zero; distance selects the nearest valid target within that ray. Without this field, the global sort could select a farther target by ID. Two candidates with the same key are a defect: the battle throws when `ThrowOnDefect` is set, and logs otherwise.
3. **Walk (stage 9).** The resolver walks the sorted candidates. It keeps per-bullet state for the step: stop time, disarmed, consumed own hit, damage taken. A candidate is skipped when:
   - its attacker is disarmed, has already stopped at its own hit, or stopped at an earlier time;
   - its target is a disarmed bullet, or a bullet that stopped at an earlier time.

   A target that stops at the same time stays valid, so two bullets that hit each other both take the hit. A hit on a bullet with `TargetHitPoints` above 0 adds its damage to the target's damage taken. The hit that brings it to the hit points stops the target at that time. Several weak hits in one step therefore stop a missile at the lethal one. A candidate with `OncePerTargetEntity` (damage over time, D16) counts once per attacker and target entity per step. Ships never become invalid within a step.
4. **Ramming (Q8).** Stage 4 stores each active ship's linear velocity before the step (`PreStepVelocity`). Physics reports a touch only after the solve, when the velocities have already changed. `RammingTracker` counts the touching shape pairs per enemy ship pair. At stage 8 it handles begin events first, then end events. When a count goes from 0 to 1, it adds one candidate per direction at time 1. The damage is `0.1 × |vA − vB| × min(mass A, mass B) × the attacker's RammingDamageMultiplier`, and it applies only above 1 (legacy `DefaultCollisionBehaviour`). The candidate's target shape is the smallest (priority, order) among the pair's begin events in that step. A removal forgets every pair of the removed ship. Ally pairs are not tracked.
5. **Damage (stage 10).** Stage 9 adds each accepted hit on a ship or a missile to its `PendingDamage` and emits a `Hit` event. Stage 10 applies the total in one pass, as legacy `Impact.ApplyShield` and `ShipStats.ApplyDamage` do. The shield absorbs up to its value and restarts its delay; the rest goes to armor. A missile loses hit points (legacy `MissileDamageHandler`). The resolver and stage 10 add the same values in the same order, so they agree on a missile's death. In phase 2 the local runtime owns every entity, so it applies all damage.
6. **Deaths (stage 11).** A ship with armor 0 or less is marked; stage 12 removes it with reason `Death` and a `Killed` transition. A missile with hit points 0 or less gets removal reason `Death`; it wins over an expiry in the same tick. `QueueDeath` remains as a test hook.

### Result and timer

Stage 13 evaluates elimination first: a side whose slots are all `Destroyed` loses; both sides in one tick give a draw. A `Ready` reserve keeps its side alive.
Then the timer: it advances one tick only while both sides have an active slot (B5). At `TimeLimitTicks` (0 means no limit) the battle ends in a draw with reason `Timeout`.
Any ship deployment or removal on either side resets the timer to zero (owner, 2026-10-20). The tick of the reset counts when both sides then have a ship. This replaces the legacy player/enemy rules until phase 4 adds `TimeOutMode`.
`BattleResult` holds outcome (`Victory` or `Draw`), winning side, reason (`Elimination` or `Timeout`) and tick. After the result tick, `Step()` and `Submit()` throw; Runtime stops stepping.
The result is internal; the snapshot publishes it.

### Bullets and cannon

Phase 2 steps 6–7. Devices are explicit code per device (`ShipDevice`); the data form waits for the data-model gate (A4).

1. **Definitions.** `ShipDefinition.Devices` lists at most 64 `DeviceDefinition`s; the list index is the device index and its control bit. `ShipDefinition.Barrels` lists `BarrelDefinition`s: the muzzle point and angle in the ship frame (the legacy platform offset included). `CannonDefinition` holds barrel index, cooldown (`1 / FireRate`), energy cost, spread (full angle, radians) and a `BulletDefinition` (radius, speed, range, damage, weight, `ParentVelocityEffect`, hit points, homing, `StartingVelocityMultiplier`). Deployment creates or resets cached devices for the definition. A missile launcher is a cannon with homing ammunition: legacy launchers are ordinary weapons, and the ammunition decides the behaviour.
2. **Whole ticks.** `TickTime` converts legacy seconds once, at setup, with exact arithmetic. A legacy `>=` check takes ceil(seconds × 50) ticks; a legacy `>` check takes floor(seconds × 50) + 1. A value within 0.001 tick of a boundary counts as on it. Legacy accumulates float time per step, so at an exact multiple it can differ by one tick.
3. **Cannon (Q1, legacy `CommonCannon` on a `FixedPlatform`).** At stage 2, after the resources recharge, each device of a ship that is not retreating runs in index order (legacy `ShipRetreatEffect` blocks every system). A cannon fires when its bit is held, its cooldown is over (`CooldownTicks`, a `>=` check; it starts ready), its barrel is ready, and `Energy.TryConsume(cost)` succeeds. A failed check spends nothing.
4. **Barrels.** Weapons on one barrel share a 0.04 s cooldown (legacy weapon platform, a `>` check): 3 ticks. A deployment counts as a barrel shot in the tick before, so the first shot comes 2 ticks after the deploy tick, as in legacy.
5. **Spawn (stage 5).** A device writes a `SpawnRequest` at stage 2; stage 5 creates the bullet. The ID is `CreateChild(ship, tick, device index, barrel index, sequence)`. The angle is the ship angle + the barrel angle + (r − 0.5) × spread, with r from `CounterRandom` (`WeaponSpread`, the bullet ID). The velocity is speed × `StartingVelocityMultiplier` along the angle + the ship velocity × `ParentVelocityEffect`; the ship velocity is the one after stage 4.
6. **Recoil.** An impulse of −weight × speed × `StartingVelocityMultiplier` × `ParentVelocityEffect` along the shot (legacy `BulletStats.Recoil`), at the muzzle point; an off-centre barrel also spins the ship. Legacy applies it only for obsolete ammunition. The core applies it for every bullet (owner, 2026-10-20). `PreStepVelocity` keeps the stage 4 value.
7. **Store.** `BulletStore` is a struct array with a LIFO free list and generations. The free list grows with the array, so a tick that frees many bullets late in a battle allocates nothing; `EntityRegistry` maps bullet IDs too. A physics bullet maps back to its entry by physics index and `BulletId`. Physics bullets are pass-through, so physics reports every target; the core keeps the first valid hit (contact ordering). The bullet's `Owner` body is its ship, so it never hits that ship.
8. **Filters.** `CollisionFilters` gives one category bit per side and class, following the legacy layer matrix. Ship hulls are `Ship(side)` and ignore nothing. An energy bolt is `Energy(side)` and ignores its own side's ships and all energy bolts. It hits enemy ships and missiles and passes allies and other bolts. A missile is `Missile(side)` and ignores its own side's ships, bolts and missiles. It meets enemy ships, missiles and bolts.
9. **Hits.** At stage 8 each physics hit on a live ship becomes a candidate at its sweep fraction (`BulletSweep`). Q1: a cannon bullet stops at its first valid hit, so stage 9 marks it for removal with reason `Death`. The damage then follows the ship pipeline: shield first, then armor. A hit on a bullet becomes a candidate only when the target is a missile, with the missile's hit points as `TargetHitPoints` (see Missiles and target grid).
10. **Lifetime.** Range / speed, with a `>` check: `LifetimeTicks`. A bullet spawned at tick s flies in the physics of ticks s to s + `LifetimeTicks` − 1. Stage 11 of the last tick marks it `Expiry`.
11. **Removal (stage 12).** Bullets leave after the ship changes, in entity ID order, so the events do not depend on storage. A bullet outlives the ship that fired it; physics ignores its stale owner body.

### Missiles and target grid

Phase 2 step 7 (Q2a, Q2b). The rules port legacy `HomingController` on new ammunition, with `W = 1 / (0.2 + 2 × weight)`.

1. **Missile.** A bullet with hit points above 0 is a missile (legacy `UnitClass.Missile`). It stops at its first valid hit on a ship or a missile (legacy `DetonateAtTargetAction`): impact damage only. Area explosions come with triggers in phase 5 (owner, 2026-10-20).
2. **Bullet contacts.** A bolt that hits a missile deals its damage and stops. Two missiles that meet both detonate, whatever their hit points: the candidate of each one stops it. A missile that meets a bolt deals nothing and does not stop (legacy `BulletDamageHandler`); only the hit of the bolt counts.
3. **Lifetime.** Range / speed, as for every bullet (Q2a; the legacy 1.2 factor is not kept).
4. **Target grid (R9).** At the start of stage 4, `TargetGrid` takes every active ship: entity ID, side, position and hull radius, at the poses at the start of the step. It is a uniform torus grid with 32 u cells, rebuilt once per tick. It allocates nothing once its arrays fit the battle. Drones join it on the tick after launch: stage 4 precedes the stage 5 spawn.
5. **Target rule.** Range is the centre distance at the nearest torus image minus both radii. The search covers 1.3 × the range of the missile. Inside it, the nearest enemy within 30° of the heading wins. Without one, the enemy with the smallest deviation wins. Equal keys go to the lower `EntityId`, so the result does not depend on grid order. Beyond the radius there is no target (owner, 2026-10-20). Legacy chases an enemy at any distance, and its choice depends on list order. Target priority, decoys and `ChanceToAvoidMissile` come later.
6. **Search timing.** Legacy runs no search at creation, so the first search is in the tick after the launch. The next one comes 101 ticks later (`TickTime.After(2 s)`). A removed target starts a search at once, at stage 4 of the next tick (redesign, owner, 2026-10-20). Every search restarts the period.
7. **Steering (stage 4, `HomingSteering`).** Below full speed along the heading, the velocity changes by `normalize(speed × heading − v) × 0.5 × speed × W × dt`. This also removes sideways velocity. The turn rate is `120 × W` deg/s toward the target, and 0 inside ±5° or without a target. The rate is set, not accelerated. Physics never turns a bullet, so the core sets the heading after the turn before the physics step. The legacy body has the same heading after its step. Smart aim and `IgnoreRotation` come later.

### Beams

Phase 2 step 8 (Q3). Immediate shutdown replaces the legacy lifetime tail (owner decision, 2026-10-20).

1. **Definition.** `BeamDefinition` is a device with barrel index, cooldown seconds, activation cost, energy per second, range and damage per second. Values must be finite; range is positive and costs, cooldown and damage are nonnegative. Phase 2 uses a fixed muzzle direction without spread or recoil.
2. **Activation.** Stage 2 checks the held bit, weapon cooldown, shared barrel cooldown and activation payment. The first active tick pays only activation energy. Later ticks pay `EnergyPerSecond × 0.02` after resource recharge. Each successful tick deals `DamagePerSecond × 0.02`. Failed payment spends nothing and gives no partial damage. The existing resource rule requires positive available energy even for a zero cost.
3. **Shutdown.** Release, failed payment or retreat start stops the ray before stage 6 and starts cooldown on that tick. A held bit can restart after cooldown and energy recovery, but never on the shutdown tick. A beam records one barrel shot on activation; maintaining it does not occupy the barrel.
4. **Lifetime and identity.** Stage 5 creates one entity in `BulletStore` using the normal child ID. It has no physics body or collider, cannot be hit, and has no projectile expiry. The same ID lasts through the activation. Stage 12 removes stopped beams with `Disposal`. Parent removal marks attached beams before releasing parent state; projectiles still outlive their parent.
5. **Queries.** Stage 6 samples the current muzzle pose and performs one pass-through ray cast with eight reusable hit slots. It uses the energy collision filter and ignores the parent body. Enemy ships and missiles block the beam; friendly ships, friendly missiles and energy bolts do not. The ray and its no-hit endpoint use the physics limit of the smaller arena dimension.
6. **Selection.** Stage 8 collects the retained ray candidates; stage 9 selects the nearest valid target after disarms. A hit consumes this tick's ray, not the beam entity. Shield priority breaks equal-distance ties before entity ID and shape order. Damage-over-time deduplication limits each attacker/target pair to one hit per tick.
7. **Overflow.** If all eight returned targets are invalid and physics reports overflow, that tick has no hit. `BeamQueries.ExhaustedRayCount` increments and a warning identifies the battle, tick and beam. Overflow with an accepted target does not increment it.
8. **Validation.** `BeamTests` covers legacy LaserBeam_S_1 values, energy boundaries, cooldowns, retreat, identity, query ordering, shields, missiles, wrapping, snapshots and exhausted buffers. `PointDefenseTests` also checks real disarms through complete battle steps, including exhausted ray buffers. Sustained hits and repeated activations allocate zero bytes after warm-up.

### Point defense

Phase 2 steps 9 and 10 (Q4). The owner chose payment before each zap and legacy checking while ready. Step 10 adds drone energy damage.

1. **Definition.** `PointDefenseDefinition` supplies radius, cooldown seconds, energy cost and optional nonnegative `droneDamage` (default 0). Radius is positive; cooldown and cost are nonnegative. All values are finite. The device needs no barrel. The legacy fixture uses radius 4 (size 3 plus hull radius 1), cooldown 0.3 s and cost 1.
2. **Activation.** Stage 2 checks the held bit and pays the energy cost once. Release or retreat disables the device before queries. Failed zap payment also disables it. Reactivation needs payment and `TickTime.AtLeast(cooldown)` ticks since shutdown. Zero-cost activation and zaps retain the existing resource rule requiring positive available energy.
3. **Query timing.** The first query occurs at first activation tick plus `TickTime.After(cooldown)`. A successful query processes all affordable targets, then the next query waits the same delay. At 0.3 s the delay is 16 ticks. Empty queries leave the device ready, so it queries again next tick. Disable/re-enable preserves the query deadline; redeployment creates fresh state.
4. **Overlap.** Stage 6 queries a circle at the ship's current position, before physics. It includes enemy missiles and energy bolts spawned at stage 5. Enemy drones are also included. Allies, roster ships, beams and bullets pending removal are excluded. Physics measures distance to the target surface and handles torus wrapping. The reusable buffer starts at 32 entries and grows on overflow until every target fits.
5. **Payment and ordering.** Projectile overlaps become time-zero `Disarm` candidates. Drone overlaps become time-zero `Hit` candidates, one per device and drone, including compound hulls. Disarms sort before hits, so projectiles receive payment first. Drone damage reduces shield, then armor at stage 10; it never drains energy. Drones remain valid targets throughout the contact walk, even after lethal accumulated damage. The ship ID and device index identify the attacker. Stage 9 checks contact validity before payment. A failed payment spends nothing, leaves the target valid and stops this device's remaining zaps. A target already disarmed by another device costs nothing and does not reset the skipped device's query deadline. Accepted zaps restart the energy recharge delay.
6. **Removal and events.** An accepted disarm immediately invalidates the bullet within the contact walk, before any time-zero hit. Projectiles and beams can then select valid targets behind it. Stage 9 emits `BattleEventKind.Disarm` with attacker ID, target ID, `DeviceIndex` and the target position sampled at stage 6. Stage 12 removes the bullet with `Disposal`. The device has no separate entity.
7. **Authority.** Phase 2 owns every entity locally. The network gate will separate the defense owner's energy and cooldown from the bullet owner's destruction decision.
8. **Validation.** All 534 engine-free tests pass, including 29 point-defense cases. Coverage includes payment, cooldowns, retreat, redeployment, filtering, wrapping, same-tick spawns, overlapping devices, allocation order, real beam/projectile continuation and 70-target salvos. Repeated interception and activation allocate zero bytes after warm-up. The large-salvo test also fixed a resolver bug: array growth must finish before the disarm array reference is fetched.

Core and Core.Tests compile separately with Unity 6000.0.75f1's compiler and assembly references. Unity Editor imported the new sources and compiled both assemblies.

### Drone bays and self-destruction

Phase 2 step 10 (Q5/Q6). A drone is a full `ShipEntity` without a roster slot. Its bay belongs to `(RosterId, device index)`.

1. **Definition and inventory.** `DroneBayDefinition` supplies positive capacity, nonnegative finite launch energy cost and a drone `ShipDefinition`. Nested bays are rejected, as legacy excludes bays on drones. Each roster slot owns persistent bay inventory. Launches consume hangar stock; deaths never restore it. Surviving drones and unlaunched stock remain through retreat and redeployment. Replication is deferred.
2. **Launch timing.** Held controls launch while stock, energy and the shared mothership bay timer permit. The timer is separate from weapon barrels. It uses `TickTime.After(0.4f)`: first launch at deployment tick + 20, subsequent launches at least 21 ticks apart. Device index resolves competition between bays. Failed payment spends nothing. Release preserves readiness; retreat blocks launch; redeployment resets the timer.
3. **Intentional timing correction (owner, 2026-10-21).** Legacy `ShipFactory.CreateDroneBays` registers the same platform twice. `ShipSystems` therefore advances its 0.4-second timer twice per tick. The new core advances it once and uses the declared duration.
4. **Spawn.** Stage 2 pays and queues; stage 5 creates at the mothership position and heading, with zero velocity and empty controls. The child ID uses the current mothership ID, tick, device index, lifetime launch slot and sequence. The free-position query searches up to 40 units. On failure it still launches at the returned least-overlapping position and retains the payment (owner, 2026-10-21).
5. **Lifecycle.** Mothership retreat or destruction does not remove drones. Redeployment exposes the new mothership ID through the same bay without resetting drone resources, controls or pilots. Surrender and kill-all destroy roster slots; surviving drones remain in the frozen result snapshot. Drones do not consume roster deployment limits, prevent elimination, prevent timer pauses or reset the timer (owner, 2026-10-21).
6. **Combat.** Drones use the ship engine, resources, devices, damage and ramming pipeline. They touch all drones and enemy roster ships, but pass through allied roster ships. Enemy projectiles and beams hit them. Newly launched drones participate in same-tick point-defense queries and physics. Their first systems/motion update and target-grid entry occur next tick.
7. **Public state.** Ship snapshots and spawn/removal events include nullable `BayKey` and `IsDrone`; a drone's `RosterId` is default. The snapshot supplies the current `MothershipId`; events use `ParentId`. An absent mothership has a null entity ID. No drone removal creates a roster transition. Both snapshot buffers grow to include drones.
8. **Pilot command.** `SelfDestructCommand` carries battle ID, command ID, target tick and entity ID. `Submit` rejects wrong battles, past ticks, null IDs and duplicate pending command IDs. Stage 1 processes it after roster commands and before frames, rejecting targets that are not live drones. Stage 11 marks death; stage 12 removes the drone normally. Runtime exposes `SelfDestruct` and forwards `PilotControls.SelfDestruct` at the locked delay. Providers select `PilotSchedule.Drone` (delay 2, period 2). Pilots decide whether an absent mothership warrants self-destruction.
9. **Allocation.** Pooled ships cache device instances per definition and reset their transient state on reuse. The ship free list reserves capacity as the pool grows. Repeated launches, drone deaths and weapon fire allocate zero bytes after warm-up. Sustained drone zaps also allocate zero bytes.

### Snapshots and events

Stage 14 writes a `BattleSnapshot` of the tick it completed, then increments `Battle.Tick`. `Battle.Snapshot` returns the published one. The constructor publishes tick −1: every slot `Ready`, no ships, no events.

1. Contents: battle ID, tick, timer, result, `Ships` (active roster ships in roster order, then drones by entity ID), `Bullets` (live bullets in storage order; the order has no meaning), `Roster` (every slot in setup order) and `Events` (the tick's batch). The lists are `ReadOnlySpan` views over arrays the battle reuses.
2. `ShipState`: entity, roster ID, side, owner, definition, pose and velocities from the physics body, armor, shield and energy (value and maximum), the controls in force and the retreat completion tick.
3. `BulletState`: entity, parent entity, side, owner, definition, pose, velocity and expiry tick. Projectiles retain `Definition` (`BulletDefinition`). Beams set `BeamDefinition` and `IsBeam`, with `Definition` null. Their pose is the sampled ray origin and angle; `BeamEndPoint` and `BeamTargetId` describe the accepted hit or full range. Beam velocity is zero and expiry is `long.MaxValue` because the device owns lifetime.
4. `RosterSlotState`: roster ID, side, owner, status, deployment count and order, revision, active entity (null unless active), the retained B4 deficits and `DamageFraction` (B2).
5. Two buffers: the battle writes one while readers use the other. A reader finishes with a snapshot before the next `Step()` returns; the snapshot of tick `t` stays intact while tick `t + 1` runs. A worker-thread reader needs a third buffer.
6. Publishing allocates nothing once the arrays fit the battle (unit tests: ships, cannon hits, missile salvos, sustained beam hits and repeated beam activations, and the full fleet of the scripted battle). The copy size is measured in [Scripted battle](#scripted-battle).

`BattleEvent` is one flat struct per event, tagged by `Kind`. Its identity is battle ID, tick and `Sequence` (the position in the batch). Beam spawn/removal events carry `BeamDefinition`, `ParentId`, `IsBeam` and `IsBullet`; `BulletDefinition` is null. A beam emits one spawn and removal per activation and a `Hit` for each accepted tick. Events follow stage order. Roster transitions follow roster order; drone removals follow entity ID order; hits follow contact order. An entity event comes before the roster transition of the same operation:

| Operation | Stage | Events |
| --- | --- | --- |
| Deploy | 1 | `Spawn` (with the `ShipDefinition`), then `RosterTransition` Ready → Active, `Deployed` |
| Shot | 5 | `Spawn` of a bullet: `BulletDefinition`, `ParentId`, side and owner; `IsBullet` is true |
| Hit | 9 | `Hit` per accepted hit on a ship or a missile, in contact order: attacker (a ship or a bullet), target, damage before the shield, point (zero for a ship touch) |
| Point-defense zap | 9 | `Disarm`: attacking ship ID, device index, target bullet ID and query position |
| Death | 12 | `Removal` (`Death`), then `RosterTransition` → Destroyed, `Killed` |
| Retreat completion | 12 | `Removal` (`Retreat`), then `RosterTransition` → Ready, `Retreated`, with the retained deficits |
| Surrender, kill-all | 12 | For an active slot `Removal` (`Death`); for every slot `RosterTransition` → Destroyed, `ForcedDestruction`. A reserve has no entity. |
| Bullet removal | 12 | `Removal` of a bullet (`Death` at a hit, `Expiry`, or `Disposal` after disarm or beam shutdown), after the ship changes, in entity ID order |
| Result | 13 | `Result` with the `BattleResult` |

A repeated destruction emits nothing. `RosterTransitionReason` has the same values as the application's `RosterChangeReason`. Every phase 2 event is committed.

B2 participation facts: `DamageFraction` is the share of a slot's armor that is lost. It is 1 when the slot is destroyed (also without an entity), the armor deficit over the maximum while active or after a retreat, and 0 when never deployed. The core emits no separate checkpoint event. A `Retreated` or `Killed` transition and the `Result` event are the checkpoints. The consumer reads the fractions from the same snapshot, after the tick's roster updates. It credits the increase of the enemy sum, as legacy `CombatModel` does. A `ForcedDestruction` transition is not a checkpoint. At completion, the consumer credits the slot with the highest `DeploymentOrder` of its side.

### Pilots and scheduling

`BattleRunner` (Runtime) owns a `Battle`, assigns command IDs and runs pilots. Callers submit roster commands through it, so command IDs never clash.

1. `IPilot.Think(snapshot, ship, out controls)` reads the snapshot and returns course, throttle, system bits and optional `SelfDestruct`. The runner submits self-destruction with the same `N + Delay` as the control frame. `false` keeps the last frame in force; the runner submits nothing.
2. `IPilotProvider.CreatePilot` runs once per ship `Spawn` event (bullet events are skipped) and returns a pilot and a `PilotSchedule`, or null. A redeployment is a new spawn, so the provider is asked again. A `Removal` event drops the pilot.
3. Locked mode: after each step, a pilot thinks on the snapshot of tick `N` when `N mod Period` equals `EntityId mod Period`. Its frame targets `N + Delay`; the battle's command buffer holds it until then. Pilots run in spawn order.
4. Presets (Q9): `Ship` (delay 2, period 1), `Drone` (2, 2), `PlayerShip` (1, 1).
5. Pilots run synchronously on the calling thread, so a pilot is never late. Free-running mode is deferred to the worker thread (owner, 2026-10-20): without one it equals locked mode with delay 1.
6. No pilot thinks on the result snapshot. Frames already queued for a ship that leaves are dropped at stage 1 and counted as rejected.

Core tests cover tick advancement, fixed physics timing, independent battle instances, setup validation, identity derivation and value semantics, the entity registry, counter-based random, roster transitions, roster commands, control frames, spawn placement, the result, the timer, resources, B4 deficits, the engine, snapshots, events, pilot scheduling, contact ordering and selection, ramming, damage, B2 facts, the cannon (Q1) and bullets: hits, expiry, filters, identity, events, the snapshot and allocation, the target grid, and the homing missile (Q2): launch, thrust, turn, search timing, retarget, hits on ships, bolts and missiles, expiry and allocation.
Beam (Q3), point-defense (Q4) and drone-bay (Q5/Q6) tests cover full battle steps and allocation. The scripted battle tests cover all of it together. All 572 engine-free tests pass. Core, Runtime and Core.Tests compile separately with Unity 6000.0.75f1's compiler.

### Scripted battle

Phase 2 step 11. `Core/Tests/ScriptedBattle.cs` is test code: the core knows nothing of it.

1. **Fleets.** Each side has three slots with 600 armor, in this order: a gunship (cannon and point defense), a striker (homing missile launcher and beam on separate barrels) and a carrier (a bay with four cannon drones). Two ships per side are active; the carrier is the reserve. Arena 200 × 200, time limit 6000 ticks.
2. **Pilots.** One `ChasePilot` per ship and drone, through `BattleRunner` with the Q9 schedules. It flies at the nearest enemy by the shortest torus vector and only turns inside 6 units. It holds a weapon bit when the enemy is in range and inside the aim cone: 20° for bolts, 60° for missiles, 10° for the beam. Point defense and bays are always held. A drone self-destructs when its bay slot is `Destroyed`; after a retreat it waits for the redeployment (Q6).
3. **Commander.** After each step it deploys `Ready` slots in roster order while a side is below its active limit. It orders one retreat per gunship slot below 60 % armor, so the ship survives the 7 s charge and redeploys.
4. **Result (seed 7).** Side 0 wins by elimination at tick 3506 (70 s). The battle has 4 roster deaths, 3 drone deaths, 2 retreats and 2 redeployments. It spawns 8 ships, 8 drones, 1119 bolts, 34 missiles and 46 beams. It has 2053 hits (1222 beam, 8 ramming) and 54 disarms.
5. **Determinism.** The log holds every event and a per-tick digest of ship and bullet poses, velocities and resources as raw float bits. One run alone and two runs stepped interleaved in one process give the same log.
6. **Allocation.** The same fleet with huge armor and energy, all three slots active, steps 3000 ticks after a 1500-tick warm-up with zero bytes allocated, pilots included. The window has bolts, missiles, beam hits, point-defense zaps and eight live drones. Buffers grow until about tick 510, at the first salvo peak; afterwards nothing allocates.
7. **Snapshot size.** Element sizes on .NET 8 x64, measured as the allocation of a 1024-element array: `ShipState` 128 B, `BulletState` 88 B, `RosterSlotState` 64 B, `BattleEvent` 152 B. The scripted battle copies 1.8 KB per tick on average and 3.6 KB at most (peak 12 ships, 16 bullets). At R2 (2,000 bullets, 130 ships) the ship and bullet copy is 192,640 B per tick, about 9.6 MB/s at 50 Hz. That is 3.2 times the design estimate of 60 KB, which assumed 24-byte bullets. The performance gate decides whether to publish only changed ranges or shrink the states.

## Physics library

`PhysicsWorld` owns the body state. The simulation reads and writes bodies through `BodyId` handles and keeps no copy (investigation 1.7). One `Step(dt)` runs these phases in order:

1. Compute the motion margin of each body from its current velocity.
2. Rebuild the torus grid and find the pairs.
3. Merge the pairs into the persistent contacts.
4. Update the manifolds.
5. Continue, end and start whole-body depenetrations.
6. Prepare the joints, then run the soft-step solver (joints, then contacts, in each stage).
7. Set the touching flags and the begin/end events.
8. Write back the poses.
9. Sweep the bullets against the bodies' motion of this step and against each other, then move them (`BulletHits`); attached bullets move with their parent. Skipped without bullets.

Conventions:

| Item | Rule |
| --- | --- |
| Coordinates | Canonical torus coordinates in `[0, Width) × [0, Height)`. `Torus.Delta` gives the shortest offset across the wrap; every pair test and every impulse point uses it. |
| Units | Game units and seconds. Angles are in radians (the game uses degrees, so the core converts). |
| Position | `GetPosition` is the pivot (body origin), as in Unity. Velocity and impulses act on the centre of mass, which follows from the shapes. |
| Mass | Set by code (`BodyDef.Mass`, `SetMass`). Inertia comes from the shapes, scaled to that mass. |
| Flags | `FixedPosition`: infinite mass, and the origin is never written, so it stays exact to the last bit. The body turns about its origin. `FixedRotation`: infinite inertia. |
| Material | `ShapeDef.Default`: friction 0.2, restitution 0.2. Mixing: friction √(a·b), restitution max(a, b). |
| Speculative contacts | A pair gets manifold points up to `4 × LinearSlop + motion of both bodies in the step`. The solver lets a positive gap close in the step, and no more (owner decision 8). A time of impact clears the points of a pair that does not meet in the step ("Ghost passes" below). |
| Manifold timing | `Step` computes manifolds before the solve, from the poses at the start of the step. After a step, `GetContact` describes the previous poses, not the new ones. |
| Touching | A contact touches when a point is closer than `LinearSlop`, or when the solver pushed in the step. A fast hit can bounce on a speculative point before the gap closes, and it still counts. |
| Allocation | The step allocates nothing once its buffers have grown to the scene size. |
| Determinism | The same inputs on the same build give bit-identical results. Cross-platform equality is not a goal (investigation 6.2). Measured in P1 and P2 (P2-4: 358 scene hashes): the gate scenes give bit-identical results on desktop .NET (x64) and Android IL2CPP (ARM64); a compiler change can break this. P3-1: 454 of 460 match; the energy-shield bullet scene and the bullet cost scene differ (same verdicts). |

Shapes:

| Shape | Use | Notes |
| --- | --- | --- |
| `Circle` | Small drones, shields, bullets | |
| `Polygon` | Convex, counter-clockwise, at most 8 vertices | `FromPoints` builds the convex hull. `Radius` is the rounding radius: 0, except for a capsule core. |
| `Capsule` | Segment plus radius (Unity `CapsuleCollider2D`; `Capsule.FromSize` converts its size and direction) | The polygon manifold treats it as a rounded 2-vertex polygon. |

A compound body is a body with several shapes. Mass, centre of mass and inertia sum over the shapes, and the broad phase never pairs two shapes of one body. Contacts and events are per shape pair, so one hull-to-hull hit can give several begin events.

`ConvexDecomposition.Decompose` splits a concave outline into convex parts once, at load ("load-time split", owner decision 1). Each part becomes one polygon shape of the body.

1. Input: one outline path in body units. Scale it first, because `Polygon` has an absolute minimum edge length (0.001). A collider with several paths is decomposed path by path.
2. Cleanup: welds near-duplicate points, drops collinear points, accepts either winding. A self-intersecting outline throws.
3. Bayazit's split at each reflex vertex, then a Hertel–Mehlhorn merge of neighbouring parts while the union stays convex and within 8 vertices. The parts cover the outline exactly.
4. Outline data: each part knows which of its edges are cuts (inside the outline, between two parts) and the outline normals at each vertex. `ConvexDecomposition.AttachOutline` adds the same data to parts made by hand. A part edge that continues a cut along the outline in one straight line counts as outer.
5. Deterministic: plain arithmetic in a fixed order, so the same outline gives the same parts on every run of the same build.
6. It allocates; it is for loading, not for the step.
7. Holes: a clockwise path inside another path is a hole in Unity. The hull loader must skip it, so the outer path makes the hole solid (the harness does this; 2 holes in `mothership`). Decomposing a hole as its own path puts overlapping parts on top of the outer path.

Measured on the 124 ship outline paths (ship prefabs, scaled × 2): all decompose; parts per path median 4, 90th percentile 9, max 33 (`mothership`); area error at most 0.005%.

Polygon manifold (`Collide.Polygons`, adapted from Box2D v3, with radius support). When the shapes are apart:

1. If the incident edge overlaps the reference face along the face, it is clipped against the side planes of the face, widened by the speculative distance. A corner past the end of the face keeps its own point, measured against the face plane. Box2D v3 does not widen. Here the speculative distance grows with the motion, so without the widening both points of a corner hit near the end of a face fall on the face end, and the one-sided push spins the body (seen in P2: a drone hitting an octagon vertex at 40 u/s spun at 134 rad/s and sank in 123%).
2. If the incident edge lies wholly beside the face (corner past corner), one point lies between the closest features of the two edges, with the true gap.

Inner edges (`Collide`, "Outline parts"). A contact with a part of a concave outline never takes its normal from a cut. A body that slides across a seam between two parts would otherwise catch on the cut: in P2-1 a probe lost 90–102% of its speed on 4-part bars. Allowed normals are the outer face normals and, at a vertex, the directions between the two outline normals there. Polygons without cut edges take the plain code path, unchanged.

1. Polygon against circle: an outer face works as for any polygon. At a vertex, the direction to the centre is clamped into the vertex's outline normals; on a straight seam this gives the neighbour's face normal. A centre beyond a cut gets no point, because the neighbour part holds the whole cut. A centre inside the part gets the outline axis with the largest separation.
2. Polygon against polygon: the separating-axis test still culls pairs that are too far apart. When its axis is an outline direction of both polygons, the plain manifold follows. When a cut faces the other polygon, the outline axis with the largest separation replaces it (outer faces and outline vertex normals of both, measured with the support points of both). Apart across a cut, one point between the true closest points stays when their direction is an outline direction of both; otherwise the neighbour part across the cut is at least as near. An outline axis is not used as a speculative normal there: beyond a cut its gap along a sideways outer face is small, and such a point stopped f6s1 sideways at 140 u/s, which then sank 154% into the mothership (P2-3 trial).
3. Result (P2-3): 0.0% loss on the 4-part bars with straight and 45° cuts, for box and circle probes at 5, 40 and 140 u/s.
4. Not covered: seams between different colliders of one hull (polygon and circle, capsule or box). Their edges are not cuts. On the test hulls every such junction is a concave corner (52–90°), a real wall.

`WorldSettings` holds the torus size and the solver tuning: cell size, substeps, contact hertz and damping ratio, push speed, slop, restitution threshold, and the depenetration trigger. The defaults are the Box2D v3 values, except three that are tuned for drone crowds at the database thrust caps: 8 substeps (Box2D: 4), contact 100 Hz (30) and push speed 5 u/s (3). The solver caps the contact hertz at a quarter of the substep rate, so fewer substeps also make the contacts softer. The game sets these values in the project configuration at build time, not in the database (owner decision); the core passes them to `PhysicsWorld`.

`TryFindFreePosition` places spawns and teleports without overlap (owner decision 9). It tests every shape (no grid), because spawns and teleports are rare.

Whole-body depenetration (`Separate`, `Dynamics/Separation.cs`) separates two overlapping bodies as whole bodies. Inside a compound hull, each part pushes along its own normal, and the pushes can cancel each other: a captured drone stays wedged. Depenetration replaces all contacts of the pair with one push along the shortest way out.

1. Start: the core calls `Separate(a, b)` when it knows about the overlap, for example on capture (owner decision 2026-10-18: no free-position jump on capture). It also starts on its own when a contact stays deeper than `SeparationDepth` (25% of the smaller shape's bound radius) for more than `SeparationSteps` (10) steps, and one of the two bodies is compound. These are the fast-hit limit values. A pair of single convex shapes is left to its contacts: it has one consistent normal, and in the P1 crowd such pairs are deep only because other drones press them together.
2. Direction: 16 fixed directions in the frame of the heavier body, plus the line between the centres. For each, the clearance is the shortest move of the lighter body that frees the pair. Each shape pair overlaps in one interval of the move; separating axes give the interval (exact for polygons, a little long for round shapes), and the clearance is the first move that no interval covers. The shortest clearance wins; a tie goes to the lower candidate.
3. Push: the solver skips the pair's contacts and solves one constraint instead: normal along the direction, anchors at the centres of mass (no torque), no friction, no bounce, separation minus the clearance. It pushes at most at `PushMaxVelocity`, and the relax pass removes the push velocity, so the body slides out and does not launch. The constraint also blocks motion back into the hull.
4. End: when the pair overlaps by 5% of the smaller body's radius or less; the contacts handle the rest. When the clearance does not drop for 1 s, the direction is chosen again once; then it ends and `FailedSeparationCount` grows.
5. Cost (desktop): start 0.16 ms for a drone in the mothership, 4.9 ms for a mothership inside another one (40 × 40 parts); per step while it runs, up to about 0.3 ms for the hull pair.

Ghost passes (`Dynamics/SpeculativeContacts.cs`, `Collision/Distance.cs`, `Collision/TimeOfImpact.cs`). A speculative point takes its normal from the closest features at the start of the step, and the solver stops any approach along that normal. A body that passes close by, but does not touch, would stop too: in P2-1 passes at a gap of 0.05 lost 31–113% of their speed. Before the solve, a time of impact checks such contacts and clears their points for the step when the shapes do not come within the speculative distance (owner decision 2026-10-18: keep the motion-based margin, add a time of impact only where needed).

1. Which contacts: all points speculative (gap above `4 × LinearSlop`), and the gap can close within the step plus one substep, from the velocities, forces, damping and spin of both bodies. The extra substep is the relax pass after the last substep, which already stops an approach into the next substep. Other contacts keep their points, so the solver result does not change for them.
2. Free motion only: the time of impact assumes that no other contact acts on the two bodies. An island is a group of movable bodies linked by contacts with points and by joints; a body that can neither move nor turn links nothing. When an island holds a contact that the solver acts on (touching, deep, a time-of-impact hit, a depenetration) or a joint, no contact in it is cleared. Without this rule, crowds pushed drones into neighbours whose contacts were cleared (2 blow-ups to 76% in the 40-position crowd sweep), and a hull that hit with one part turned another part into the target.
3. Time of impact: GJK distance and conservative advancement on separating axes (adapted from Box2D, `b2ShapeDistance`, `b2TimeOfImpact`). The path is the solver's own free motion: the poses after each substep, a straight line between them and a normalized lerp of the rotation. A pair that only translates on one line is checked over the whole step in one go; the separation along a fixed axis is then concave, so the ends of the interval bound it. Otherwise the step is split at the substeps, and up to 4 times more while the bodies turn; the largest dip that the turn can add within an interval is added to the target.
4. Result (P2-4): every ghost-pass row passes (0.000% speed change, no begin event, 8 directions × 24 hull pairs, 8 reference passes). A brute-force check of about 16,000 cleared contacts (2,000 distance samples each, fast hit, fast spin, ghost pass, crowd, impact and cost scenes) found no pair that came closer than the target.
5. Cost (desktop): the step-cost scenes check 0 contacts per step at the median, 5 at most; step time unchanged (0.145 ms median). `TimeOfImpactCount` and `DroppedSpeculativeCount` give the last step's numbers.

Bullets, filters and queries (P3; `Dynamics/Bullet.cs`, `Dynamics/BulletSweep.cs`, `Collision/ShapeCast.cs`, `Queries/Queries.cs`).

| Item | Rule |
| --- | --- |
| Filter | `ShapeDef.Filter` and `BulletDef.Filter`: `Categories` and `Ignore` (64 bits each; the core puts side × class in them). Two filters pair unless one ignores a category of the other. All zero pairs with everything, so a `ShapeDef` without a filter behaves as before filters existed (the inverse of the Box2D mask, where a zero mask pairs with nothing). The filter applies to solid contacts (in the broad phase), bullet hits and queries. `SetShapeFilter` switches a shape at run time (a shield on and off). |
| Bullets | `CreateBullet` with one circle, polygon or capsule. Not bodies: the solver never sees them (about 2,000 of the 2,130 R2 bodies are bullets, and they need detection only). The core sets the velocity (homing); the world moves a bullet on a straight line and never turns it. The `Owner` body is the bullet's broad-phase group, so a bullet never hits its own ship, and bullets of one ship never hit each other. The core destroys bullets. |
| Sweep | After the solve, each bullet is cast along its motion relative to each candidate: a body keeps its start rotation and moves its centre on a line from its start to its end pose (P3 owner decision 3); two bullets both move on their lines. Shapes that overlap at the start hit at fraction 0. |
| Target unit | Hits are counted per unit: the shapes of one body with the same filter categories are one target (the parts of a hull), a shield shape with other categories is another. A bullet without pass-through reports its first hit only; a pass-through bullet reports the first hit on each unit in the step. `Begin` is set when the bullet did not hit that unit in the previous step. |
| Order | `BulletHits` is sorted by fraction, then bullet index, then target (bodies before bullets, by index), then shape index (contact ordering, investigation 1.5; the core maps indices to entity IDs). Each bullet of a hit pair gets its own hit on the other. |
| Shape cast | Newton steps on the GJK distance of the cores. For a pure translation the distance is convex in time, so each step lands at or before the first contact and never passes through. A flat face converges in one step. Tolerance: a gap of 1e-5 u plus the float resolution of the cast frame (computed near the origin, not at map coordinates); a tolerance relative to the bullet size let large bullets report a graze, early, as their hit. A step below 1e-6 of the translation counts as converged, so float noise never turns a contact into a miss. |
| Contact normal | The direction between the closest core points while they are apart (always for circles and capsules). When a vertex lands exactly on a face, the cores touch and that direction is noise; the face normal with the largest separation then gives the normal (separating axis test, cut edges excluded). Polygon manifolds are not used for this: they prefer a face of A within a tolerance, also when a vertex of A touches a face of B. Shapes that overlap at the start take the manifold normal and its deepest point. |
| Ray cast | `RayCast`: a point cast through the query grid (grid walk in unwrapped cell coordinates). The ray is clipped to `min(Width, Height)`, each target is tested in each torus image that its cells cover and keeps its nearest hit, and the `ignore` body (the shooter) is never hit. Without pass-through it returns the first hit; with pass-through up to `hits.Length` (8 in the game), the nearest, and an overflow flag. A ray that starts inside a target hits it at 0, with the reverse ray direction as the normal. |
| Overlap | `OverlapCircle`: exact distance to each convex part (not a bounding circle), each unit once at its nearest image, nearest surface first, up to `results.Length` (32) with an overflow flag. |
| Query grid | Tight boxes of the current poses of shapes and bullets, rebuilt on the first query after a step or after a change of shapes, poses or bullets. |
| Allocation | The step and the queries allocate nothing once their buffers have grown. Free lists grow with their pools, so many removals in one late step allocate nothing. A new peak still grows a buffer once (P3 cost scene: the sweep pair buffer, 2,048 → 4,096 pairs, 32 KB, in one of 1,000 steps). |

Joints (P4; `Dynamics/Joint.cs`, `Dynamics/JointSolver.cs`).

| Item | Rule |
| --- | --- |
| Hinge | `CreateHinge(HingeDef)`: a shared point of two bodies (Box2D v3 revolute joint, without spring and motor; P4 owner decision 2). Anchors are body-local and relative to the body origin, as Unity anchors. An optional limit holds the joint angle within `[LowerAngle, UpperAngle]`, both within [-π, π]. |
| Rope | `CreateRope(RopeDef)`: the distance between two anchors never exceeds `Length` (Box2D v3 distance joint with only the upper limit: no spring, no lower limit, never rigid). It only pulls. While slack it acts as a speculative constraint: the ends may reach the rope length within the substep, but no more; it applies no impulse otherwise. `SetRopeLength` reels in or out; a shorter rope pulls the ends together at the joint softness. `GetJointKind` tells hinges and ropes apart. |
| Joint impulse | `GetJointImpulse`: the sum of the linear impulses that the joint applied to body B in the last step, through its anchor (warm start and solve; body A got the negative). Hinge limits apply angular impulses only, which the sum leaves out. |
| Joint angle | Angle of B relative to A minus `ReferenceAngle`, in (-π, π]. The caller always passes the reference angle; the library never takes it from the current poses (P4 owner decision 3). Today's `HingeJoint2D` takes it at creation, before the rigidbody sync, which is a candidate cause of the tail that coils at an impossible angle. |
| Solver | Soft step, as the contacts: the point constraint and the limits are soft with bias (`JointHertz` 200, `JointDampingRatio` 2, the hertz capped at half the substep rate; Box2D v3 solves joints with twice the contact hertz), and the relax pass removes the bias velocity. A limit that is not reached acts as a speculative constraint: the angle may reach the limit within the substep, but no more. Joints come before contacts in each stage. |
| Seam | Prepared from `Torus.Delta` of the two centres at the start of the step; within the step only the motion of the two bodies counts. A chain across the wrap line needs no special case. Each offset across the line rounds at the precision of coordinates near the map size (about 1.5e-5 u on map 200), so a chaotic chain across the line drifts from the same chain in the middle (unit test: 5e-3 u after 60 steps, 3e-4 u away from the line). |
| Contacts | `CollideConnected` off (the default, as in Unity): the two bodies get no contact. `CreateHinge` drops the pair's existing contacts at once; touching ones end in the next step's events. |
| Islands | A joint links its bodies into one island and marks it: no contact in that island is cleared by the time of impact, because a joined body never moves freely. |
| Teleport | `TeleportGroup(body, position)` moves the body and every body joined to it by hinges (P4 owner decision 4); rotations and velocities are not written. A rope never moves its other end. `TeleportGroup(body, position, angle)` also turns the group about the body's origin and turns the velocities. Relative positions keep their value within 2 ulp of the coordinates (P4 owner decision 5). |
| Lifetime | `DestroyJoint`; `DestroyBody` destroys the body's joints. |
| Contact impulse | `ManifoldPoint.TotalNormalImpulse` and `TotalTangentImpulse`: the impulse the point applied in the whole step (Box2D v3.1 `totalNormalImpulse`), for momentum checks and ramming damage. Body B received it. |

Attachment (P4; `Dynamics/Bullet.cs`, `Dynamics/BulletSweep.cs`): a bullet that follows a body (harpoon hook, sticky mine, `AttachedToParent` ammunition). It is not a joint.

| Item | Rule |
| --- | --- |
| Pose | `AttachBullet(bullet, parent, local)` stores the pose in the parent's frame, relative to its origin; `AttachBullet(bullet, parent)` takes the current world pose. The world pose is the parent pose times the local pose. The local pose is never written back, so it stays exact to the last bit. `SetBulletPose` throws for an attached bullet. |
| Motion | Each step sweeps the bullet from its pose on the parent's start pose to its pose on the parent's end pose, with the parent's start rotation (as bodies in the sweep), and never against its parent. The velocity is that of the parent's point. A teleport of the parent (`SetPose`, `TeleportGroup`) moves it at once. |
| Lifetime | `DetachBullet` keeps the world pose and the point velocity. When the parent is destroyed, the bullet comes free at the next step with its last pose and velocity. |
| Rope to a hook | The core anchors the rope on the target at the hook's local point (`GetBulletLocalPose`); the rope anchor and the hook are then at the same point to the last bit. |

Known limits:

1. A polygon against a circle gives one manifold point. A polygon that spins fast after a glancing hit can swing another corner into the circle for one step (P1 impact results).
2. When an incident edge overlaps the reference face only partly, the point of its corner past the face end measures against the face plane. This is conservative: a body can stop slightly early (ghost collision), never late. Corner past corner measures the true gap. The time of impact removes these early stops for bodies in free motion; in an island with an active contact or a joint they stay. Example: a ship that passes close to a hull while drones rest on that hull is still braked; so is a ship that passes close to a worm.
3. An AABB must be smaller than half the torus on each axis.
4. Soft contacts give way under load: penetration grows with the force (P1 crowd results).
5. The solver handles each contact once per substep, in a fixed order. When other contacts push the same body in the same substep, a fast pair can pass its speculative limit and overlap deeply for 1–3 steps; it then separates at no more than the push speed. Seen when crowd drones bounce off a target into drones that still arrive (P1 arrival peak).
6. Fast spin: a hull whose tip moves at 140 u/s (f6s1 at 233 rad/s, flagship4 at 47 rad/s) can sink 31–45% for one step. The manifold point comes from the start of the step, and a fast turn brings another feature in. Before P2-4 the ghost stop of the previous step braked some of these spins early (flagship4: 0%).
7. Bullet sweeps ignore the target's spin within the step (P3 owner decision 3). A hull tip that turns through a bullet in one step is missed; the bullet is then inside the hull at the next step and hits it at fraction 0 there, one step late.
8. A bullet, its swept box and the boxes of its targets must stay below half the torus (limit 3). On map 50 the P3 sweep scene skips its 30 u bullets for that reason.
9. Joints have no friction or motor. A worm tail sways for seconds after the head stops; only the core's sideways damping rule removes energy.
10. Warm start after a sudden stop: in the first step after a strong turn kick ends, the warm-started joint impulses can raise the kinetic energy by a few percent (6% at the ship engine caps). Box2D v3 behaves the same.
11. A chain far faster than the database worms (engine caps: 1,200°/s) opens its joints by up to 18% and whips the tail tip past 120 u/s. The core caps the worm's turn rate.
12. A rope is stiff and inelastic: a jerk stops the far end at the rope length without a bounce.
13. Chaotic joint scenes (a swinging chain) drift apart between any two positions, also away from the wrap line, through float rounding. Seam checks compare the first steps only.

Third-party code: [THIRD_PARTY.md](../Assets/ModulesShared/CombatSimulation/Physics/THIRD_PARTY.md) (Box2D, MIT).

## Gate harness

`Experimental/CombatSimulationHarness/` is a throwaway .NET console project. It compiles the physics sources directly, runs the gate scenes of the physics stages and checks their limits. It is outside `Assets/` and is not in Git. Game code never references it.

```
dotnet run -c Release --project Experimental/CombatSimulationHarness
```

It prints a table of measured values against the limits, and it writes the same table to `Results/P1-<date>.md` with the harness version and a state hash per scene. Two runs of the same build give the same hashes. Each scene runs in the arena middle and on the wrap corner. With `-- --replay` it also records each scene, and `Experimental/CombatSimulationHarness/Viewer/index.html` plays the recordings step by step (harness README).
