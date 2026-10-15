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
