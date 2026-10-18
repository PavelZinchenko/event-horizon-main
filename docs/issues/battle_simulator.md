# BattleSimulator Module Issues

This file records problems in `Assets/Modules/BattleSimulator/`. For a description of the current design, see [BattleSimulator module](../battle_simulator.md). Unlike the [application layer issues](application_layer.md), this file also lists local defects, because the module has no tests that would catch them.

The analysis was done by reading the code on 2026-10-11. No issue was reproduced in Play Mode. Each defect has a status:

- **Confirmed** – the code path is certain. The failure follows directly from the code.
- **Plausible** – the code is wrong, but the failure needs specific data (database content, AI trees, or timing).

## Structural issues

### B1. The AI thread reads and writes simulation state without synchronization

**Problem.** Ship controllers run on a worker thread (see [threading model](../battle_simulator.md#threading-model)). They read unit state and write ship state while the main thread changes the same objects. Only the unit lists have locks.

**Evidence.**
- `ShipControls.Apply` calls `SystemsState.Assign`, which first clears all bits and then sets them ([IAction.cs:16](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Actions/IAction.cs#L16), [SystemsState.cs:45](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Controls/SystemsState.cs#L45)). The main thread can read the cleared state in `ShipSystems.UpdatePhysics` and turn a held weapon off for one step. A continuous weapon then fires `OnDeactivate` and starts its cooldown.
- Behaviour tree nodes call `platform.Aim` and set `ActiveTarget` on weapon platforms ([AimAndAttackHandler.cs:29](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/Calculations/AimAndAttackHandler.cs#L29)). `WeaponPlatformBody.Aim` can rotate the platform from the worker thread while the main thread also rotates it ([WeaponPlatformBody.cs:114](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Body/WeaponPlatformBody.cs#L114)).
- Nodes change `UnitType.Owner` (`GoBerserkNode`, `Context.Mothership`) and call `Vanish()` from the worker thread.
- `KeyboardController` runs on the worker thread and fires `PlayerInputSignal` there ([KeyboardController.cs:113](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/KeyboardController.cs#L113)). Player input therefore has up to one fixed step plus one worker sleep of extra latency.
- `ThreatList` calls `Debug.Break()` from the worker thread ([ThreatList.cs:105](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/ThreatList.cs#L105)).

**Effect.** Rare, timing-dependent faults: weapons that stutter, platforms that jitter, and results that differ between WebGL (one thread) and other platforms. These faults are almost impossible to reproduce.

**Direction.**
1. Double-buffer ship intent. The worker writes a complete `ShipControls` snapshot, and the main thread copies it at the start of `FixedTick`.
2. Turn all mutations that AI requests (owner change, vanish, platform target) into commands that the main thread applies.
3. Run the player keyboard controller on the main thread.
4. Later, give AI a read-only snapshot of the scene for each step.

### B2. One failing controller stops the whole AI and spins a CPU core

**Problem.** `AiManager.DoWork` updates all controllers in one loop without a `try`/`catch` per controller ([AiManager.cs:55](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/AiManager.cs#L55)). `BackgroundTask` catches the exception outside the loop.

**Evidence.**
- The exception skips `_lastFrame = _currentFrame` ([AiManager.cs:88](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/AiManager.cs#L88)).
- The next loop iteration sees the same unprocessed frame and runs `DoWork` again at once. `OnIdle` (the sleep) runs only when `DoWork` returns `false`.
- Controllers before the failing one run again on every iteration with the full `deltaTime`. Controllers after it never run.

**Effect.** A persistent exception in one controller (see D1) freezes part of the AI, makes other AI timers run many times too fast, fills the log with exceptions, and uses a full CPU core.

**Direction.** Catch exceptions per controller. Log once per controller, then mark that controller dead or skip it. Always update `_lastFrame`.

### B3. `IShip` is too wide, and non-ship units implement it with `null` members

**Problem.** `Decoy` implements `IShip` so that it can join `Scene.Ships` and attract fire. It returns `null` for `Stats`, `Engine`, `Systems`, `Effects`, and `Specification` ([Decoy.cs:65](../../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Auxiliary/Decoy.cs#L65)). `WormSegment` and `WormSegmentBase` still contain the same members, although they no longer implement the interface.

**Evidence.** Code that receives an `IShip` usually assumes that these members exist. See D1.

**Effect.** Every new consumer of `IShip` is a possible `NullReferenceException`.

**Direction.** Split `IShip` into a small `ITargetable` (body, side, features) for target search, and the full `IShip`. Make decoys `ITargetable` only. Remove the leftover members from worm segments.

### B4. Legacy and current implementations exist side by side

**Problem.** Several features exist twice. Both versions stay in the build, and a fix usually goes into only one of them.

**Evidence.**
- AI: behaviour trees (`AI/BehaviorTree/`) and the legacy strategy AI (`Computer`, `Drone`, `Clone`, `Starbase`, `AI/Strategy/`, `AI/Actions/`, `AI/Conditions/`). The legacy AI runs when the database has no tree.
- Bullets: `BulletFactory`, and `BulletFactoryObsolete` with `AmmunitionClassExtensions`.
- Worm tails: `WormTailDevice` with `WormSegment`, and `WormTailDeviceV2` with `WormSegmentBase`.
- Camera framing: `ViewRect` and `ExplorationViewRect` are almost identical. Both keep about 100 lines of commented-out code.
- Weapon classification: AI code still uses the `[Obsolete]` `BulletEffectType` next to `WeaponCapability`.

**Effect.** More code to understand and test. Behaviour differs between old and new data.

**Direction.** Decide whether the default database and supported mods still need each legacy path. Convert data with a migration where possible, then delete the old path.

**Decision (2026-10-16).** Legacy ammunition: convert, then delete. Legacy AI and worm tail V1: keep, because mods use them. Details: [step 7 of the refactoring plan](../../TODO/battle_simulator_investigation.md#step-7-legacy-content-audit).

### B5. Dead and unreachable code

**Problem.** The module contains types, members, and features that have no effect.

**Evidence.**
- Types without callers: `StaticPlatform`, `Loot`, `LootCollisionBehaviour`, `DefaultDamageHandler`, `CameraUnit` (its factory method has no callers), `DroneEngine` (nothing calls `ShipFactory.CreateEngine` with `isDrone = true`), `MovingForwardNode`, `StayBehindAction`, `FlyAroundAction`, `WaitAction`, `HasEnergy`, `IsEnemyContition`, `MoreTimePassedCondition`, `AlwaysTrue`, `SimpleBulletSpawnSettings`, `IBulletSettingsExtensions`, `CamouflageType`, `IPowerLevelIndicator`.
- The legacy AI `TeleportAction` has an empty `Perform`. `HitAndRun.SuitabilityLevel` returns `-1`, so that strategy never runs.
- Ship performance statistics never run: `ShipFactory` always passes `collectStatistics = false` ([ShipFactory.cs:86](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L86)).
- `FeaturesData.ImmuneToEffects` is read ([AffectDroneAction.cs:31](../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/Action/AffectDroneAction.cs#L31)), but nothing sets it.
- `EffectFactory.IsObjectVisible` always returns `true` ([EffectFactory.cs:97](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/EffectFactory.cs#L97)), so eight visibility checks do nothing.
- `ControllerStatus.Idle` does not change scheduling.
- Unused fields: `DetonateAtTargetAction._hits`, `GravityGenerator._isEnabled`, `MaintainAttackRange._rangeLimit`, `RepairAction._distance`, `ShakeEffect._power`, `ParticleSystemInitializer._material`, `BulletFactory._prefabCache`, `CreateGravitationAction._frienlyFire`.

**Effect.** Readers spend time on code that does nothing. Some dead features look active (statistics, immunity, culling).

**Direction.** Delete the dead types and fields. Finish or remove statistics, immunity, and culling. Check prefabs before you delete a `MonoBehaviour` type.

**Status (2026-10-16).** Re-checked: every type above still has no callers and no prefab references. The owner decided to mark this code, not delete it. Do not port it to the new core.

### B6. Simulation rules depend on Unity statics and cannot be tested

**Problem.** Rules read `Time.fixedTime`, `Time.time`, and `UnityEngine.Random` directly, and most units need a `GameObject`.

**Evidence.** `Cooldown`, `PlayHitSoundAction`, and `SpawnHitEffectAction` read `Time.fixedTime`. `GameObjectBody` integrates with `Time.fixedTime`. Bullet spread, repair bot jitter, and gravity bombs use `UnityEngine.Random`. The module has no test assembly.

**Effect.** Defects such as D2–D10 reach players. A battle cannot be replayed from a seed.

**Direction.** Inject a clock and a random source into the scene. Start with pure classes (`Impact`, `Resistance`, `ShipStats`, `Energy`, `UnitList`, `ShipSystems` activation rules, behaviour tree nodes) and add an EditMode test assembly for them.

### B7. Per-frame allocations and unpooled effects

**Problem.** Hot paths allocate memory or create GameObjects.

**Evidence.**
- `CompositeEffect.Create` makes a new `GameObject` for every data-driven effect ([CompositeEffect.cs:14](../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/CompositeEffect.cs#L14)). Bullet hit and trail triggers call it often.
- `ThreatList.Update` allocates a `List<IUnit>` on every update for every ship ([ThreatList.cs:49](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/ThreatList.cs#L49)).
- `DroneReplicator` runs LINQ every step ([DroneReplicator.cs:43](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/DroneBays/DroneReplicator.cs#L43)).
- `GetEnemyForMissile` creates a `System.Random` for each candidate ([ShipListExtensions.cs:208](../../Assets/Modules/BattleSimulator/Scripts/Combat/Scene/ShipListExtensions.cs#L208)). Instances created in the same millisecond share a seed, so camouflage rolls correlate.
- `SpriteView`, `MeshView`, and `EffectBase` read `renderer.materials` in `OnDestroy`. This creates material copies only to destroy them.

**Effect.** Garbage collection spikes in large battles, mostly on mobile.

**Direction.** Pool composite effects, reuse the threat list buffer, remove LINQ from update loops, and keep one random source per scene.

### B8. Torus-wrap distance has no shared helper

**Problem.** The map is a torus, but there is no shared function for the shortest vector across the wrap line. Each call site either measures the raw world distance or carries its own copy of the wrap code.

**Evidence.**
- PR #600 (head f27b0ae0) adds the same wrap code in four places: `HomingController.WrapDirection`, `ShipNavigationHandler.WrapDirection`, `ShipListExtensions.WrapDirection`, and inline in `Helpers.DistanceWrap`. It passes the area width and height separately through 12 call sites.
- About 30 raw `Distance` and `Direction` calls remain under `Combat/AI`: attack-range conditions, target locators, `TargetingHelpers`, `ThreatList`, and the legacy actions.
- Navigation and missile targeting are wrapped after #600. Range checks and aiming are not. The two disagree for AI-vs-AI pairs on opposite sides of the wrap line.

**Effect.** Copies can drift. Each new wrap fix copies the code again. Pairs that include the player are not affected, because `CheckBounds` keeps every unit within half an area of the player.

**Direction.** Owner decision (2026-10-17): do not require deduplication in #600. Handle it in the refactoring: one minimum-image helper (shortest vector on the torus) on the scene, used by all distance and direction code. See [step 5 of the refactoring plan](../../TODO/battle_simulator_investigation.md#step-5-physics-requirements).

## Defects

### D1. Drones that target a decoy can crash AI nodes — High, Plausible

**Where.** `EnemyMatchingOptions.EnemyForDrone` takes a decoy 30% of the time ([ShipListExtensions.cs:29](../../Assets/Modules/BattleSimulator/Scripts/Combat/Scene/ShipListExtensions.cs#L29)). These nodes then read members that are `null` on a decoy: `TargetLowHpNode` ([line 20](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/Nodes/ConditionNodes/TargetLowHpNode.cs#L20)), `IsFaterThanTarget` ([line 17](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/Nodes/ConditionNodes/IsFaterThanTarget.cs#L17)), `EscapeAttackRadiusNode` ([line 37](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/Nodes/ActionNodes/EscapeAttackRadiusNode.cs#L37)), and `HasBetterAttackRange` ([line 38](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/Nodes/ConditionNodes/HasBetterAttackRange.cs#L38)). In the legacy AI, `SpecialRules.UseDevices` and `KamikazeAction` read `enemy.Systems` and `enemy.Stats`.

**Failure.** A drone whose tree uses one of these nodes targets a decoy. The node throws `NullReferenceException` on every AI step. Because of B2, the AI then stalls.

**Fix.** Exclude decoys in these nodes, or fix B3.

### D2. Three or more impulses in one impact throw `IndexOutOfRangeException` — High, Confirmed

**Where.** `Impulse` stores two `Vector2` values per impulse and counts values, not impulses, in `_count`. `Apply` loops `i < _count` and reads `_values[i*2]` and `_values[i*2+1]` ([Impact.cs:16](../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Impact.cs#L16)). `Append` resizes the array to exactly the new count ([Impact.cs:28](../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Impact.cs#L28)).

**Failure.** With one or two impulses, `Apply` also applies zero forces from unused slots. With three impulses (`_count = 6`, array length 8), it reads index 10 and throws. Ammunition with three push or pull impact effects (any mix of `Push`, `Pull`, `PushFromCenter`, and `PullToCenter`) breaks every collision of that bullet.

**Fix.** Loop `i < _count / 2`, or store a list of `(position, impulse)` pairs. Grow the array by doubling.

### D3. The drone bay platform is added twice — Medium, Confirmed

**Where.** `ShipFactory.CreateDroneBays` adds the drone bay platform at [ShipFactory.cs:167](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L167) and again at [ShipFactory.cs:181](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L181).

**Failure.** `ShipSystems` updates the platform twice per step. The 0.4 s launch cooldown becomes 0.2 s.

**Fix.** Remove the second `AddPlatform` call. Check drone launch balance after the fix, because players are used to the faster rate.

### D4. Cached effect templates ignore per-element parameters — Medium, Confirmed

**Where.** `PrefabCache.GetEffectPrefab` keys templates by `(int)data.Type + data.Image.Id` only ([PrefabCache.cs:153](../../Assets/Modules/BattleSimulator/Scripts/Combat/Services/PrefabCache.cs#L153)). Initializers bake other element values into the template: `SparkEffect` stores `Size`, `ParticleSize`, and `TurnRate` ([SparkEffect.cs:24](../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/SparkEffect.cs#L24)). `LightningEffect` stores `ParticleSize`, `Color`, and `ColorMode`. `SmokeEffect` stores `ParticleSize`. The key is also a string concatenation, so different pairs can collide (type `1` with image `"1x"` equals type `11` with image `"x"`).

**Failure.** Two visual effect elements with the same type and image, but different spark speed, particle size, or lightning color, both show the values of the element that loaded first.

**Fix.** Key the cache by element identity, or move per-element values out of the template and apply them to each instance.

### D5. Child bullets of charged weapons keep the power of the first shot — Medium, Confirmed

**Where.** `ChargeableCannon` sets `BulletFactory.Stats.PowerLevel` before each shot ([ChargeableCannon.cs:72](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/ChargeableCannon.cs#L72)). `BulletTriggerBuilder` caches child factories per trigger ([BulletFactory.cs:537](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L537)) and copies `PowerLevel` only when it creates them ([BulletFactory.cs:311](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L311)).

**Failure.** For a charged weapon whose ammunition spawns bullets, every later shot spawns children with the damage, size, and range of the first shot.

**Fix.** Read the parent power level when a child bullet is created, or pass it through `SpawnBulletsAction`.

### D6. AI threat detection uses local rotation for attached beams — Medium, Confirmed

**Where.** [ThreatList.cs:97](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/ThreatList.cs#L97) uses `item.Body.Rotation` for a child beam. For a child body, this value is the rotation relative to the weapon platform, not the world rotation.

**Failure.** AI ships detect beam threats in the wrong direction, unless the firing ship faces angle 0. `AvoidThreats`, `DefendWithFronalShield`, and `HasIncomingThreat` react to the wrong beams.

**Fix.** Use `item.Body.WorldRotation()`.

### D7. Clones get full damage and full resistance — Medium, Confirmed

**Where.** `ClonningDevice.ShipStatsWrapper` scales `WeaponDamageMultiplier`, `RammingDamage`, and the raw resistance values ([ClonningDevice.cs:134](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/ClonningDevice.cs#L134)). `ShipStats` reads `DamageMultiplier` and the `*Percentage` resistance values, which pass through unchanged ([ShipStats.cs:25](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Stats/ShipStats.cs#L25), [ShipStats.cs:29](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Stats/ShipStats.cs#L29)). Weapon damage comes from the unchanged platform data. The `size` argument is stored and never used ([ClonningDevice.cs:114](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/ClonningDevice.cs#L114)).

**Failure.** The decorator is created with attack and defense 0.9, but only armor, shield, and their recharge rates change.

**Fix.** Decide the intended clone strength. Then scale the values that `ShipStats` and `WeaponFactory` actually read, and remove the unused parameter.

### D8. Fixed worm segments never regenerate — Medium, Confirmed

**Where.** Nothing assigns `FixedWormSegment._regenerationCooldown` ([FixedWormSegment.cs:14](../../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/WormTail/FixedWormSegment.cs#L14)). `Affect` sets `_timeToRegeneration` to this zero value ([line 47](../../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/WormTail/FixedWormSegment.cs#L47)), so the regeneration branch never runs.

**Failure.** The two unbreakable segments next to the head never restore their hit points.

**Fix.** Pass a regeneration cooldown from `WormSegmentFactory`, or remove the regeneration code.

### D9. Auto-aiming satellites search for targets in the wrong arc — Low, Confirmed

**Where.** [AutoAimingSatelliteController.cs:68](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Controller/AutoAimingSatelliteController.cs#L68) computes the arc center as `default + (max − min) / 2`. For the right side (`min = −190`, `max = 10`), this gives `+100` instead of `−90`. Line 71 passes the full arc width (200°) where `GetEnemyForTurret` expects a half-angle, so the arc check never runs.

**Failure.** The satellite can lock a target outside its arc. It then points in its default direction for up to 1 s and ignores valid targets.

**Fix.** Use center `default + (min + max) / 2` and half-width `(max − min) / 2`.

### D10. Warp drive mode depends on device order — Low, Confirmed

**Where.** `WarpDrive` checks in its constructor whether the ship has a worm tail ([WarpDrive.cs:35](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/WarpDrive.cs#L35)). Devices later in the list do not exist yet. The check also ignores `WormTailV2`.

**Failure.** A ship whose worm tail comes after the warp drive in the device list, or that uses `WormTailV2`, flies in warp mode and can tear its tail apart.

**Fix.** Evaluate the condition on first activation, and include `WormTailV2`.

### D11. Bullet cooldown triggers cannot detonate the bullet — Low, Plausible

**Where.** `Bullet.UpdatePhysics` invokes `OnCooldown` actions and ignores the returned `CollisionEffect` ([Bullet.cs:84](../../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Bullet/Bullet.cs#L84)).

**Failure.** A `Detonate` trigger with the `Cooldown` condition does nothing.

**Fix.** Detonate when an `OnCooldown` action returns `Destroy`, in the same way as `Expire`.

### D12. `Scene.Clear` leaves disposed units in the active state — Low, Confirmed

**Where.** [Scene.cs:118](../../Assets/Modules/BattleSimulator/Scripts/Combat/Scene/Scene.cs#L118). `CombatManager.Exit` and `ExplorationSceneManager.Exit` call it.

**Failure.** Units are disposed, but their `State` stays `Active`. AI controllers do not see them as dead and keep running on pooled GameObjects until the scene unloads. `ShipDestroyedSignal` does not fire, and `PlayerShip` still points to a disposed ship.

**Fix.** Vanish units before disposal, reset the player and enemy references, and let `AiManager` drop the controllers.

### D13. Smaller defects

| ID | Where | Problem | Status |
| --- | --- | --- | --- |
| D13.1 | [ShipStats.cs:113](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Stats/ShipStats.cs#L113) | Shield damage statistics receive the armor damage value. Hidden today, because statistics are off (B5). | Confirmed |
| D13.2 | [IBulletSettingsExtensions.cs:15](../../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Bullet/SpawnSettings/IBulletSettingsExtensions.cs#L15) | `WithExtraRotation` and `WithExtraOffsetAndRotation` pass `0` instead of `rotation`. No callers today. | Confirmed |
| D13.3 | [RetributionDevice.cs:30](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/RetributionDevice.cs#L30) | `DeviceClass` is never set, so it reports `Accelerator` (enum value 0). | Confirmed |
| D13.4 | [BulletFactoryObsolete.cs:155](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs#L155) | Spread is rolled twice: velocity uses the first roll and rotation the second. For `Aura` and `Explosion` (bound to the cannon), a world rotation replaces the local rotation. | Confirmed |
| D13.5 | [ShipFactory.cs:232](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L232), [line 239](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L239) | Starbases and turrets replace the engine after devices registered their engine modifications on the old engine. | Confirmed |
| D13.6 | [BulletFactory.cs:450](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L450), [line 564](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L564) | Actions with the `OnCooldown` condition get two identical cooldown wrappers. | Confirmed |
| D13.7 | [RigidBodyAdapter.cs:241](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Body/RigidBodyAdapter.cs#L241) | Logs the error `"zero"` whenever a body is exactly at the origin, which is a valid position. | Confirmed |
| D13.8 | [Features.cs:50](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Features/Features.cs#L50) | `UpdateData` does not reset the camouflage chances and `ImmuneToEffects` before it applies modifications. A temporary modifier of these values would stay on forever. | Confirmed |
| D13.9 | [Impact.cs:92](../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Impact.cs#L92) | `AddDamage` throws on negative damage. Mod data with negative power breaks every collision of that ammunition. | Plausible |
| D13.10 | [SpaceObjectFactory.cs:210](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Objects/SpaceObjectFactory.cs#L210) | `CreateDecoy` calls `Run` twice on the same flash effect. The first call has no effect. | Confirmed |
| D13.11 | `EnergyShieldDamageHandler.RemoveDamage` | The last line reduces corrosive damage after an earlier line set it to zero. | Confirmed |
| D13.12 | `LongRange` and `AggressiveClone` constructors | The `AvoidThreats` and `UseAccelerators` policies are added twice. | Confirmed |

### D14. The gravity bomb is classified as a drone — Medium, Confirmed

**Where.** [SpaceObjectFactory.cs:305](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Objects/SpaceObjectFactory.cs#L305). `GravityGenerator` creates the bomb as `UnitClass.Drone` with `UnitSide.Undefined`, so it uses the Drone-Undefined collision layer (23).

**Failure.** Code that handles real drones also acts on the bomb (issue #535):
- `PointDefenseCollisionAction` fires at it and spends energy ([line 33](../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/Action/PointDefenseCollisionAction.cs#L33)).
- `StickyController` lets missiles stick to it ([line 72](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Controller/StickyController.cs#L72)).
- `DetonateAtTargetAction` detonates bullets on it ([line 17](../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/Action/DetonateAtTargetAction.cs#L17)).
- `RestoreLifeAction` extends bullet lifetime on contact ([line 35](../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/Action/RestoreLifeAction.cs#L35)).
- `AffectDroneAction` changes its owner ([line 30](../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/Action/AffectDroneAction.cs#L30)).

PR #599 proposes a point-defense-only patch (skip drones with `UnitSide.Undefined`). That patch leaves the other four effects.

**Fix.** Add `UnitClass.GravityField` at the end of the enum, and create the bomb with it.
1. Map it to the Drone layers in `UnitType.GetCollisionLayer`, so its collisions do not change. The method throws on an unknown class.
2. Return `false` for it in `ThreatAnalyzer.IsThreat`. Otherwise the side check marks the side-less bomb as a threat for every ship.
3. Remove the PR #599 point-defense special case.
4. Check every other `switch` on `UnitClass`; their defaults already ignore the bomb.

Do this after PR #599 merges.

### D15. Progressive damage restarts its ramp for all targets — Medium, Confirmed

**Where.** [ProgressiveDamageAction.cs:30](../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/Action/ProgressiveDamageAction.cs#L30). The action stores one `_elapsedTime` per bullet, not per target. A new contact sets it to zero and deals no damage.

**Failure.** A damage-over-time bullet with `ProgressiveDamage` touches target A, then target B. The contact with B resets the ramp, so A also drops back to the start of the ramp. A beam or aura that sweeps over several units never reaches full damage.

**Fix.** Keep the elapsed time per target, for example in a dictionary keyed by the target unit, and clear entries that get no contact for one step.

### D16. Damage over time counts once per target collider — Medium, Plausible

**Where.** [CommonCollider.cs](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Collider/CommonCollider.cs) `OnTriggerStay2D`. Unity reports one stay event per collider pair. `OnCollisionStay2D` skips repeats in one step with `TryUpdateActiveCollision`, but `OnTriggerStay2D` has no such check. 32 of 146 ship prefabs have 2–5 `Collider2D` components on one GameObject (for example `fas3`, `flagship13`, `starbase_red`).

**Failure.** A damage-over-time bullet that overlaps three colliders of one ship deals three times the damage per step. Large ships with several colliders take more damage from clouds, auras and webs than other ships.

**Fix.** Confirm in Play Mode. Then de-duplicate stay events per target unit and step, as `TryUpdateActiveCollision` does.

### D17. Some ship modifiers depend on install order — Low, Confirmed

**Where.** Modifiers run in device order: built-in devices first, then component devices in install order ([Ship.cs](../../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Ship/Ship.cs) `AddSystem`). Some modifiers set a value instead of combining it:

- `JammerDevice` sets `TargetPriority = High`. `StealthDevice` sets `TargetPriority = None` while active.
- `WarpDrive` sets `Throttle = 0` and `Deceleration = 0` during warp. `BrakeDevice` adds deceleration when the throttle is zero. `AcceleratorDevice` sets `Throttle = 1`.

**Failure.**
- A ship with a jammer installed after a stealth device stays a high-priority target while it is invisible.
- A ship with a brake installed after a warp drive decelerates during warp, so it warps a shorter distance. An accelerator installed after the warp drive can thrust during warp.

**Fix.** Give conflicting values an explicit priority: `None` beats `High` for target priority, and warp locks beat throttle and deceleration changes. See [battle simulator investigation](../../TODO/battle_simulator_investigation.md), step 1.2.

### D18. A time machine undoes the ship's own detonation — question

**Where.** `DetonatorDevice` destroys the ship through `Ship.Affect` with the `Destroy` effect. `Ship.Affect` then raises `DamageTaken`. `InfinityStone.OnEvent` sees zero armor and restores the ship when it is off cooldown.

**Effect.** A ship with both devices makes the explosion and survives. The owner must decide if this combination is intended. If not, skip the restore when the impact has the `Destroy` effect from the ship itself.

### D19. The torus wrap tears jointed units apart — Low, Plausible

**Where.** [Scene.cs:202](../../Assets/Modules/BattleSimulator/Scripts/Combat/Scene/Scene.cs#L202) `CheckBounds` moves each root unit separately. It skips units of class `Limb`, which both worm tail versions use for their segments. Jointed groups are not moved together: worm tails (hinge joints), harpoons (distance joint to the owner, fixed joint to the target) and tractor beams (distance joint).

**Failure.**
- An enemy worm-tail ship that crosses the wrap line jumps one area width. Its tail segments stay behind, and the hinge joints pull them across the map.
- A harpoon or tractor beam whose two ends are on different sides of the wrap line gets one end moved. The joint then pulls with a very large force.

**Fix.** Confirm in Play Mode. Then wrap connected groups as one: find the root of each joint group and move all its bodies by the same offset.

### D20. Missile dodge rolls allocate and share one seed — Low, Plausible

**Where.** [ShipListExtensions.cs:208](../../Assets/Modules/BattleSimulator/Scripts/Combat/Scene/ShipListExtensions.cs#L208) `GetEnemyForMissile` creates `new System.Random()` for every candidate ship with `ChanceToAvoidMissile > 0`.

**Failure.**
- Each check allocates a `Random` object (B7). Homing and magnetic missiles search again every 2 s, so many missiles make many allocations.
- On Mono and IL2CPP, the parameterless constructor seeds from the system tick count. Missiles that search in the same millisecond get the same roll, so they all skip the same target or all lock onto it.

**Fix.** Use one seeded random source for the scene. Do this only with owner approval, because it changes seeded combat results.

### D21. A non-finite player position moves every unit to `NaN` — Low, Confirmed

**Where.** [Scene.cs:214](../../Assets/Modules/BattleSimulator/Scripts/Combat/Scene/Scene.cs#L214) `CheckBounds` uses the player position as the wrap center. It checks that each unit position is finite, but it does not check the center. The gap came in with PR #598.

**Failure.** If the player position becomes `NaN` or infinite, for example after a physics blow-up, `dx` and `dy` are `NaN`. The in-range check is false, so `Body.Move` puts every root unit at `NaN`. The code before #598 moved nothing in this case. A `NaN` player already breaks the camera, so the change only spreads the damage.

**Fix.** Return early from `CheckBounds` when `center.x` or `center.y` is not finite.
