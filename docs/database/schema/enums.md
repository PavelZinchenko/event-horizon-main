# Database enums

JSON stores every enum as its integer value. Never renumber an existing value: saved games and mods store the numbers. Enum sources are in [`.Schema/v1/Enums/`](../../../Assets/Modules/Database/.Schema/v1/Enums/). Values without an explicit number count up from 0 in file order.

Back to the [schema index](index.md).

## ItemType

Selects the table of a record. See the [schema index](index.md#tables-by-domain) for the page of each value. Values 0–35 are collection tables. Values 100–118 are settings singletons. Values 0, 13, 33, 34, 104, 113–116 have no schema.

## Ships

### SizeClass

| Value | Name | Meaning |
| ---: | --- | --- |
| -1 | `Undefined` | No size. A ship hull with this size gets a circle collider instead of a polygon collider. |
| 0 | `Frigate` | Smallest hull class. |
| 1 | `Destroyer` | |
| 2 | `Cruiser` | |
| 3 | `Battleship` | |
| 4 | `Titan` | Largest normal hull class. Crafting price is tripled. |
| 5 | `Starbase` | Faction starbases. |

`ComponentRestrictions.ShipSizes` stores a list of these values.

### ShipType

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Common` | A normal ship. Can be an enemy, a player ship, or a market ship. |
| 1 | `Drone` | Launched from drone bays. Never a fleet ship. |
| 2 | `Starbase` | A faction base. Selected by `ShipBuildQuery.Starbases`. Immune to teleport effects. |
| 3 | `Special` | Scenario-only hulls (outposts, turrets). Excluded from all ship queries. Immune to teleport effects. |
| 4 | `Flagship` | A boss. Appears as boss enemies and can be a player ship. |

### ShipRarity

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Normal` | Regular ship. Included in `Common()` queries. |
| 1 | `Rare` | Included in `CommonAndRare()` queries. Crafting costs 1.5× credits. |
| 2 | `Hidden` | Excluded from normal enemy and loot selection. Intended for special fleets (see [issue DB15](../../issues/database.md#db15-smaller-defects): `HiddenShips()` returns nothing). |
| 3 | `Unique` | Cannot be sold. Migration 1.2→1.3 makes builds of unique hulls unavailable as enemies. |

### CellType

Characters in a [layout](../../database.md#layout) and in `Component.CellType`.

| Char | Name | Meaning |
| --- | --- | --- |
| `0` | `Empty` | No cell. A component with `Empty` cell type fits any non-empty cell. |
| `1` | `Outer` | Outer hull. |
| `2` | `Inner` | Inner hull. |
| `3` | `InnerOuter` | Accepts inner and outer components. An `InnerOuter` component fits inner and outer cells. |
| `4` | `Weapon` | Weapon mount. Connected weapon cells form one barrel. |
| `5` | `Engine` | Engine mount. |

### ShipPerkType

Built-in ship modifications of a [ShipBuild](ShipBuild.md) (`Perks`). Values come from [ShipModSettings](ShipModSettings.md) where noted.

| Value | Name | Effect |
| ---: | --- | --- |
| 0 | `Empty` | No perk. |
| 1 | `AutoTargeting` | Every barrel gets an auto-aiming arc of at least 20°. |
| 2 | `HeatDefense` | Adds `HeatDefenseValue` to the heat resistance bonus. |
| 3 | `KineticDefense` | Adds `KineticDefenseValue` to the kinetic resistance bonus. |
| 4 | `EnergyDefense` | Adds `EnergyDefenseValue` to the energy resistance bonus. |
| 5 | `LightWeight` | Multiplies hull weight by `WeightReduction`. |
| 6 | `Infected` | Adds `RegenerationValue` armor regeneration, multiplies armor by `RegenerationArmor`, and adds the toxic-waste device (Device 18). |
| 7 | `UnlimitedRespawn` | **Not implemented.** Building a ship with this perk throws. See [issue DB1](../../issues/database.md#db1-the-unlimitedrespawn-perk-crashes-ship-creation). |
| 8 | `WeaponClass` | Removes the slot restriction of every barrel and multiplies damage by `1 − AttackReduction`. |
| 9 | `SatelliteSize` | Raises the allowed satellite size class by one (at least Cruiser) and the allowed satellite model scale by 0.4. |
| 10 | `EnergyRechargeCooldown` | Multiplies energy by `1 − EnergyReduction` and the energy recharge delay by 0.01. |
| 11 | `ShieldRechargeCooldown` | Multiplies shield by `1 − ShieldReduction` and removes the shield recharge delay. |

### DifficultyClass

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Default` | Normal build. |
| 1–3 | `Class1`–`Class3` | Elite build. Enemy queries allow class `level / 25` at most, and class 1 or more from level 50. Enemy ships get random component upgrades, more threat, and more experience. Stats are not multiplied. See [ShipBuild: Difficulty class](ShipBuild.md#difficulty-class). |

### ToggleState

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Default` | Use the global or computed default. |
| 1 | `Enabled` | Force on. |
| 2 | `Disabled` | Force off. |

## Components

### ComponentCategory

UI grouping and research-tree label: `Undefined` (0), `Weapon` (1), `Defense` (2), `Energy` (3), `Engine` (4), `Drones` (5), `Special` (6).

### Availability

See [Component: Availability](Component.md#availability) for the consumers.

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `None` | Enemy-only modules. Never in random pools. A ship with such a component cannot enter the arena. |
| 1 | `Common` | Sold by merchants, used in random loot, and unlockable on ships without research. |
| 2 | `Rare` | Used in random loot and turret/outpost generation with `Common`. |
| 3 | `Special` | Meant for pre-installed (locked) modules. The game does not enforce it ([DB15.17](../../issues/database.md#db15-smaller-defects)). |
| 4 | `Hidden` | Excluded from random selection. Obtainable only through explicit loot or research. |
| 5 | `LootOnly` | Same runtime behavior as `Hidden`. |

### ComponentStatsType

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `PerComponent` | Stats apply once. |
| 1 | `PerOneCell` | Stats are multiplied by the component's cell count. |

### ModificationQuality

Quality grade of a [ComponentMod](ComponentMod.md). Each grade selects one column of `StatModification`.

| Value | Name | Column | Price factor |
| ---: | --- | --- | ---: |
| 0 | `N3` | `Gray3` | 50 % |
| 1 | `N2` | `Gray2` | 75 % |
| 2 | `N1` | `Gray1` | 90 % |
| 3 | `P1` | `Green` | 150 % |
| 4 | `P2` | `Purple` | 200 % |
| 5 | `P3` | `Gold` | 300 % |

### StatModificationType

The stat that a `StatModification` row changes. The value from the quality column is a multiplier, except `ExtraHitPoints`, which is added. The exact rules per component part are on [ComponentMod: Effects](ComponentMod.md#effects).

| Value | Name | Affects |
| ---: | --- | --- |
| 0 | `None` | Nothing. |
| 1 | `WeaponDamage` | Weapon damage. |
| 2 | `WeaponRange` | Weapon range. |
| 3 | `WeaponFireRate` | Fire rate. |
| 4 | `WeaponBulletSpeed` | Projectile speed. |
| 5 | `WeaponBulletMass` | Projectile weight (impulse and recoil). |
| 6 | `WeaponAoe` | Area-of-effect radius. |
| 10 | `DroneAttack` | Drone damage. |
| 11 | `DroneDefense` | Drone armor and shield. |
| 12 | `DroneSpeed` | Drone speed. |
| 13 | `DroneRange` | Drone operating range. |
| 20 | `EnergyCapacity` | Energy points (only when positive). |
| 21 | `EnergyRechargeRate` | Energy recharge. |
| 22 | `ShieldPoints` | Shield points (only when positive). |
| 23 | `ShieldRechargeRate` | Shield recharge (only when positive). |
| 24 | `ArmorPoints` | Armor points (only when positive). |
| 25 | `ArmorRepairRate` | Armor repair. |
| 30 | `Resistance` | All three resistances (only positive ones). |
| 40 | `DeviceCooldown` | Device cooldown. |
| 41 | `DeviceRange` | Device range. |
| 42 | `DevicePower` | Device power. |
| 50 | `EnginePower` | Engine power (with and without energy). |
| 51 | `EngineTurnRate` | Turn rate (with and without energy). |
| 60 | `Mass` | Component weight (only when positive). |
| 61 | `EnergyCost` | Energy consumption of the component, its device, drone bay, or ammunition. |
| 62 | `ExtraHitPoints` | Adds the value to armor points. |

### ActivationType

How the player triggers a device, weapon, or drone bay. It validates the key binding of an installed component. See [Device: Activation](Device.md#activation).

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `None` | No key. An active system runs automatically. |
| 1 | `Manual` | Needs a key binding. |
| 2 | `Mixed` | Can be bound to a key or run automatically. |

### DeviceClass

Selects the device implementation. See [Device](Device.md#device-classes) for the meaning of `Power`, `Range`, `Size`, `Cooldown`, and `Lifetime` per class.

`Accelerator` (0), `Decoy` (1), `EnergyShield` (2), `Ghost` (3), `GravityGenerator` (4), `PartialShield` (5), `PointDefense` (6), `RepairBot` (7), `Detonator` (8), `Stealth` (9), `Teleporter` (10), `Brake` (11), `SuperStealth` (12), `Fortification` (13), `ToxicWaste` (14), `WormTail` (15), `ClonningCenter` (16), `TimeMachine` (17), `Jammer` (18), `DroneCamouflage` (19), `MissileCamouflage` (20), `TeleporterV2` (21), `WormTailV2` (22), `Retribution` (23).

## Weapons and ammunition

### WeaponClass

| Value | Name | Behavior |
| ---: | --- | --- |
| 0 | `Common` | One bullet per shot. Cooldown `1 / FireRate`. |
| 1 | `Manageable` | One active bullet. Releasing the key detonates it. |
| 2 | `Continuous` | A beam that stays while the key is held and energy lasts. |
| 3 | `MashineGun` | Bursts of `Magazine` shots. |
| 4 | `MultiShot` | `Magazine` bullets at once. Energy cost × magazine. |
| 5 | `RequiredCharging` | Charges while held. Release fires with power equal to the charge. |

### DamageType

`Impact` (0, kinetic), `Energy` (1), `Heat` (2), `Corrosive` (3). Resistances reduce the matching type. Corrosive damage uses half of the smallest resistance and partly bypasses shields (see [ShipSettings](ShipSettings.md) `ShieldCorrosiveResistance`).

### BulletImpactType

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `HitFirstTarget` | Acts once on the first contact, then detonates or self-destructs. |
| 1 | `HitAllTargets` | Acts once per new contact. |
| 2 | `DamageOverTime` | Acts every frame of contact, scaled by time. |

### ImpactEffectType

What an `ImpactEffect` does on contact. `Power` is the strength. `Factor` is a secondary value. See [Ammunition: Impact effects](Ammunition.md#impact-effects) for the meaning of both per type.

| Value | Name | Effect |
| ---: | --- | --- |
| 0 | `Damage` | Deals `Power` damage of `DamageType`. |
| 1 | `Push` | Pushes the target along the bullet velocity. |
| 2 | `Pull` | Pulls the target against the bullet velocity. |
| 3 | `DrainEnergy` | Removes target energy. |
| 4 | `SiphonHitPoints` | Damages the target and repairs the owner. |
| 5 | `SlowDown` | Slows the target. |
| 6 | `CaptureDrones` | Takes control of enemy drones and missiles. |
| 7 | `Repair` | Repairs allies; harms enemies × `Factor`. |
| 8 | `RestoreLifetime` | The bullet starts with less lifetime and gains lifetime on hits. |
| 9 | `Devour` | Damages ships, shields, and limbs. Destroys asteroids and other bullets. A hit on a starbase destroys the bullet. |
| 10 | `Teleport` | Moves the target forward. Starbases and special ships are immune. |
| 11 | `DrainShield` | Damages the shield only. |
| 12 | `DriveDronesCrazy` | Makes drones attack everyone. |
| 13 | `IgnoreShield` | Damage from this ammunition bypasses shields. |
| 14 | `RechargeShield` | Restores ally shields; harms enemies × `Factor`. |
| 15 | `RechargeEnergy` | Restores ally energy; harms enemies × `Factor`. |
| 16 | `ProgressiveDamage` | Damage over time that ramps up over `Factor` seconds. |
| 17 | `PushFromCenter` | Pushes the target away from the bullet center. |
| 18 | `PullToCenter` | Pulls the target toward the bullet center. |

### BulletControllerType

How a bullet moves. See [Ammunition](Ammunition.md#bulletcontroller-switch).

`Projectile` (0), `Homing` (1), `Beam` (2), `Parametric` (3), `Harpoon` (4), `AuraEmitter` (5), `StickyMine` (6).

### BulletTriggerCondition

When a bullet trigger runs: `Undefined` (0), `Created` (1), `Destroyed` (2), `Hit` (3), `Disarmed` (4), `Expired` (5), `Detonated` (6), `OutOfAmmo` (7), `Cooldown` (8, every physics step, limited by the trigger `Cooldown`). See [BattleSimulator: Triggers](../../battle_simulator.md#triggers).

### BulletEffectType

What a bullet trigger does: `None` (0), `PlaySfx` (1, sound and attached effect), `SpawnBullet` (2), `Detonate` (3), `SpawnStaticSfx` (4, effect left in the world), `GravityField` (5).

### BulletShape

Selects the bullet prefab and view: `Projectile` (0), `Rocket` (1), `LaserBeam` (2), `LightningBolt` (3), `EnergyBeam` (4), `Spark` (5), `Mine` (6), `Wave` (7), `BlackHole` (8), `Harpoon` (9), `CircularSaw` (10), `PiercingLaser` (11). Beam shapes (`LaserBeam`, `LightningBolt`, `EnergyBeam`, `PiercingLaser`) use ray-cast colliders. `Spark` and `Mine` have no direction.

### AiBulletBehavior

A hint that tells AI and auto-aim how to use the weapon: `Projectile` (0, lead the target), `Homing` (1, no aiming), `Beam` (2, aim directly), `AreaOfEffect` (3), `Trap` (4), `Harpoon` (5).

### ColorMode

How a color combines with the parent (owner) color.

| Value | Name | Result |
| ---: | --- | --- |
| 0 | `TakeFromOwner` | Parent color. |
| 1 | `UseMyOwn` | Own color. |
| 2 | `Blend` | 50 % mix. |
| 3 | `Multiply` | Component-wise product. |

### VisualEffectType

Element kinds of a [VisualEffect](VisualEffect.md): `Flash` (0), `FlashAdditive` (1), `Shockwave` (2), `Smoke` (3), `SmokeAdditive` (4), `Shake` (5, camera shake), `Spark` (6), `Lightning` (7), `LightningStrike` (8), `Sprite` (9).

### ObjectPrefabType

[GameObjectPrefab](GameObjectPrefab.md) kinds: `Undefined` (0), `WormTailSegment` (1), `CircularSpriteObject` (2), `CircularOutlineObject` (3).

### Obsolete weapon enums

- `AmmunitionClassObsolete`: behavior classes of [AmmunitionObsolete](AmmunitionObsolete.md) (`Common`, `Acid`, `Aura`, `Carrier`, `Bomb`, … `HomingCarrier`, 32 values). Each class maps to fixed code in `BulletFactoryObsolete`. See [AmmunitionObsolete: Classes](AmmunitionObsolete.md#classes).
- `BulletTypeObsolete`: the old `BulletBody.Type` (`Projectile`, `Homing`, `Static`, `Continuous`, `Magnetic`). Migration 1.5→1.6 converts it.

## AI

- `BehaviorNodeType`, `BehaviorRequirementType`, `NodeExecutionMode`: see [BehaviorTree](BehaviorTree.md).
- `AiWeaponCategory`: weapon filter of the `SelectWeapon` node: `All` (0), `Repair` (1), `Damage` (2), `CaptureDrone` (3), `Recharge` (4).
- `AiDifficultyLevel`: `Easy` (0), `Medium` (1), `Hard` (2). The AI level is a number: 2 × star level for enemy fleets, 100 for arena opponents. Below 50 is Easy, below 80 Medium, otherwise Hard. Starbases, clones, drones, and the autopilot are always Hard. See [BehaviorTree: AI level](BehaviorTree.md#ai-level).

## Quests

See [Quest](Quest.md), [Loot](Loot.md), and [Shared structures](structures.md) for `NodeType`, `RequirementType`, `LootItemType`, `FactionFilterType`, `QuestOriginType`.

### QuestType

| Value | Name | Restart rule |
| ---: | --- | --- |
| 0 | `Common` | Cannot run twice at the same star at the same time. |
| 1 | `Singleton` | Only one active instance in total. |
| 2 | `Storyline` | Never restarts after completion. No cancel button. |
| 3 | `Temporary` | No restriction. Not saved. |
| 4 | `Urgent` | Like `Common`, but no cancel button. |

### StartCondition

| Value | Name | When the quest can start |
| ---: | --- | --- |
| 0 | `Manual` | Only from code or a `StartQuest` node. |
| 1 | `Beacon` | When the player activates a beacon. |
| 2 | `LocalEncounter` | Local event at a star. |
| 3 | `FactionMission` | When the player accepts a faction mission. |
| 4 | `GameStart` | Automatically when a session starts. |
| 5 | `NewStarExplored` | When a star system is secured. |
| 6 | `ArrivedAtStar` | When the player arrives at a star. |
| 7 | `Daily` | Daily selection with a cooldown derived from `Weight`. |

### Other quest enums

- `RequiredViewMode` (dialog node): `Any` (0), `StarSystem` (1), `StarMap` (2), `GalaxyMap` (3). The dialog switches the map to this view; `StarSystem` uses the small dialog window. See [Quest](Quest.md#node-switch).
- `RewardCondition` (combat rules; obsolete in [Fleet](Fleet.md)): `Default` (0, only on victory), `Always` (1), `Never` (2).
- `TimeOutMode` (combat rules): `CallNextEnemy` (0), `DrainPlayerHp` (1), `CallNextEnemyOrDraw` (2). See [CombatRules](CombatRules.md).
- `PlayerShipSelectionMode`: `Default` (0), `OnlyOneShip` (1, the battle ends when the ship dies), `ByOrder` (2, ships enter in order, no choice), `NoRetreats` (3, no ship change until it dies).

## Other

- `SkillType` (`Undefined`, `ShipAttack`, … `Engineer`): declared in the schema but not used by any table. The skill tree uses its own `SkillType` in `Assets/Scripts`.
- `TechType`: `Component` (0), `Ship` (1), `Satellite` (2). Switch of [Technology](Technology.md).
