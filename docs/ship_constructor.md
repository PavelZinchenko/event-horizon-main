# ShipConstructor Module

The ShipConstructor module is the ship model of the game. It describes a ship as a hull, a cell layout, installed components, two optional satellites, and up to three perks. It converts that description into a combat specification: final stats, weapon platforms, devices, and drone bays. It also holds the component compatibility rules, the component and ship modifications, and several small game formulas (experience, threat, distance). It has no scenes, no MonoBehaviours, and no Zenject bindings.

| | |
| --- | --- |
| Assembly | `ShipConstructor` ([ShipConstructor.asmdef](../Assets/Modules/ShipConstructor/ShipConstructor.asmdef)). References: Database, CommonComponents, Localization, Diagnostics, Utilites. |
| Namespaces | `Constructor` (component data, builder), `Constructor.Component`, `Constructor.Model` (stats, layout), `Constructor.Modification` (component modifications), `Constructor.Ships`, `Constructor.Ships.Modification` (perks), `Constructor.Satellites`, `Constructor.Extensions`, `Constructor.Detail` (builder output), `Maths`. `ModificationFactory` is in the global namespace. |
| Source | [`Assets/Modules/ShipConstructor/Scripts/`](../Assets/Modules/ShipConstructor/Scripts/) (66 files, about 5,600 lines) |
| Referenced by | BattleSimulator, GameSession, Gui (component list), ShipEditor, GalaxyMap (reference not used), and 84 files of the application layer. |
| Data | [Ship](database/schema/Ship.md), [ShipBuild](database/schema/ShipBuild.md), [Satellite](database/schema/Satellite.md), [SatelliteBuild](database/schema/SatelliteBuild.md), [Component](database/schema/Component.md), [ComponentStats](database/schema/ComponentStats.md), [ComponentMod](database/schema/ComponentMod.md), [Device](database/schema/Device.md), [DroneBay](database/schema/DroneBay.md), [Weapon](database/schema/Weapon.md), [Ammunition](database/schema/Ammunition.md), [ShipSettings](database/schema/ShipSettings.md), [ShipModSettings](database/schema/ShipModSettings.md) |
| Tests | None. |
| Known issues | [ShipConstructor issues](issues/ship_constructor.md). |

## Contents

1. [Terms](#terms)
2. [Source map](#source-map)
3. [Object model](#object-model)
4. [Ship classes](#ship-classes)
5. [Ship model and base stats](#ship-model-and-base-stats)
6. [Layout and hull expansion](#layout-and-hull-expansion)
7. [Components](#components)
8. [Component compatibility](#component-compatibility)
9. [Component modifications](#component-modifications)
10. [Ship perks](#ship-perks)
11. [Satellites](#satellites)
12. [Building a ship specification](#building-a-ship-specification)
13. [Stats calculation](#stats-calculation)
14. [Engine stats](#engine-stats)
15. [Weapon damage calculator](#weapon-damage-calculator)
16. [Ship validation](#ship-validation)
17. [Game formulas](#game-formulas)
18. [Serialization formats](#serialization-formats)
19. [Consumers](#consumers)
20. [Extending the module](#extending-the-module)
21. [Validation checklist](#validation-checklist)

## Terms

| Term | Meaning |
| --- | --- |
| Hull | The database `Ship` record: layout, barrels, features, images. The code also calls it "original ship". |
| Ship | An `IShip` object: a hull model plus components, satellites, name, colour, and experience. |
| Ship model | An `IShipModel` (`ShipModel`). It holds the hull, the perks, the hull expansion, and the derived `ShipBaseStats`. |
| Layout | The cell grid of a hull or satellite (`IShipLayout`). Cell types: Outer, Inner, InnerOuter, Engine, Weapon, Empty. |
| Hull expansion | Extra cells that the player buys around the stock layout (`LayoutModifications`). `*` is a free expansion slot. `X` is an added weapon cell. |
| Component | A database `Component` with a modification and a quality, as a `ComponentInfo` value. |
| Installed component | An `IntegratedComponent`: a `ComponentInfo` with a position, barrel, key binding, behaviour, and locked flag. |
| Component modification | A database `ComponentMod` applied at a `ModificationQuality` (N3…P3). Shown to the player as item quality. |
| Perk | A ship modification (`IShipModification`). A ship has up to three perk slots. |
| Satellite | A small companion element on the left or right of a ship (`ISatellite`). It has its own layout and components. |
| Barrel | A weapon mount (`Barrel` record). Each barrel becomes one weapon platform in combat. |
| Specification | The builder output (`IShipSpecification`): stats, platforms, devices, drone bays. Combat creates a ship from it. |
| Bonuses | Multipliers from outside the ship: experience, difficulty, skills, arena power (`ShipBonuses`). |

## Source map

| File | Role |
| --- | --- |
| [Ships/IShip.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/IShip.cs) | `IShip`: the mutable ship that the game stores and edits. |
| [Ships/BaseShip.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/BaseShip.cs) | Common ship code: satellites, experience, colour scheme, change tracking, `CreateBuilder`. |
| [Ships/CommonShip.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/CommonShip.cs), [EnemyShip.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/EnemyShip.cs), [ArenaShip.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/ArenaShip.cs), [EditorModeShip.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/EditorModeShip.cs) | The four `IShip` implementations. |
| [Ships/IShipModel.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/IShipModel.cs), [ShipModel.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/ShipModel.cs) | Hull wrapper with perks and hull expansion. Builds `ShipBaseStats`. |
| [Ships/ShipColorScheme.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/ShipColorScheme.cs) | Player colour: RGB or hue/saturation, packed into one `long`. |
| [Ships/ShipPreset.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/ShipPreset.cs) | `IShipPreset`, `ShipPreset`: a saved fitting for one hull. Used by ShipEditor. |
| [Ships/ShipValidator.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/ShipValidator.cs) | Player availability, viability, and arena rules. |
| [Ships/IShipSpecification.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/IShipSpecification.cs) | Builder output interfaces: `IShipSpecification`, `ShipInfo`, `IWeaponPlatformData`, `IWeaponData`, `WeaponStatModifier`, `IDeviceData`, `IDroneBayData`, `ICompanionData`, `DroneBehaviour`. |
| [Ships/SizeClass.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/SizeClass.cs) | Icon scale and localized name of a size class. |
| [Ships/Modification/](../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/) | Perks: `IShipModification`, `ModificationType`, `ModificationFactory`, eleven perk classes, `LayoutModifications` (hull expansion), serialization helpers. |
| [Satellites/](../Assets/Modules/ShipConstructor/Scripts/Satellites/) | `ISatellite`, `CommonSatellite`, `EditorModeSatellite`, size rules. |
| [ComponentInfo.cs](../Assets/Modules/ShipConstructor/Scripts/ComponentInfo.cs) | Component identity: data, modification, quality, upgrade level. Random generation, price, obsolete packing. |
| [ComponentQuality.cs](../Assets/Modules/ShipConstructor/Scripts/ComponentQuality.cs) | `ComponentQuality` (N3…P3 with P0) and level formulas. |
| [IntegratedComponent.cs](../Assets/Modules/ShipConstructor/Scripts/IntegratedComponent.cs) | Installed component and its obsolete byte formats. `FromDatabase` for build components. |
| [Component/](../Assets/Modules/ShipConstructor/Scripts/Component/) | `IComponent`, `CommonComponent` (stats of one installed component), `CompatibilityChecker`, `WeaponDamageCalculator`, key validation. |
| [CellType.cs](../Assets/Modules/ShipConstructor/Scripts/CellType.cs), [DeviceClass.cs](../Assets/Modules/ShipConstructor/Scripts/DeviceClass.cs), [CompanionSpecification.cs](../Assets/Modules/ShipConstructor/Scripts/CompanionSpecification.cs) | Cell compatibility, device class rule, `CompanionLocation`. |
| [Modification/](../Assets/Modules/ShipConstructor/Scripts/Modification/) | `IModification`, `ComponentModification`, `EmptyModification` for components. |
| [Model/](../Assets/Modules/ShipConstructor/Scripts/Model/) | `ShipBaseStats`, `ShipEquipmentStats`, `ShipBonuses`, `ShipStatsCalculator` (`IShipStats`), `EngineStats`, `ColorScheme`, `IShipLayout`, `LayoutRect`, `ShipLayoutAdapter`. |
| [Builder/ShipBuilder.cs](../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs) | `ShipBuilder` and its output classes (`ShipBuilderResult`, `WeaponPlatform`, `DeviceData`, `DroneBayData`, `CompanionSpec`). |
| [Builder/IComponentConverter.cs](../Assets/Modules/ShipConstructor/Scripts/Builder/IComponentConverter.cs) | `ComponentSpec`, `DefaultComponentConverter`, `EnemyComponentConverter`. |
| [Upgrades/](../Assets/Modules/ShipConstructor/Scripts/Upgrades/) | `IComponentUpgrades`, `IComponentUpgradesProvider`. Declared but not applied (see [SC6](issues/ship_constructor.md#sc6-component-upgrades-are-wired-but-inert)). |
| [Extensions/ShipExtensions.cs](../Assets/Modules/ShipConstructor/Scripts/Extensions/ShipExtensions.cs) | Ship price, scrap, sale rule, quality, enemy ship creation from builds. |
| [Maths/](../Assets/Modules/ShipConstructor/Scripts/Maths/) | `Experience`, `Threat`, `Distance`, `Format`. |
| [ShipLayoutObsolete.cs](../Assets/Modules/ShipConstructor/Scripts/ShipLayoutObsolete.cs) | Old placement model. Only the satellite build check in `InitializationState` uses it. |

## Object model

```text
IShip  (CommonShip / EnemyShip / ArenaShip / EditorModeShip)
 ├─ Model: IShipModel (ShipModel)
 │    ├─ OriginalShip: Ship (database hull)
 │    ├─ Modifications: IShipModification ×0..3   (perk slots)
 │    ├─ LayoutModifications ── BuildLayout() ──► IShipLayout
 │    └─ Stats: ShipBaseStats  (rebuilt on every perk or layout change)
 ├─ Components: IntegratedComponent ×n
 │    └─ Info: ComponentInfo (Component + ComponentMod + quality + level)
 ├─ FirstSatellite / SecondSatellite: ISatellite
 │    └─ Information: Satellite, Components: IntegratedComponent ×n
 ├─ ColorScheme, Experience, Name, CustomAi, ExtraThreatLevel
 └─ CreateBuilder() ──► ShipBuilder ── Build(ShipSettings) ──► IShipSpecification
                                                                ├─ Stats: IShipStats (ShipStatsCalculator)
                                                                ├─ Platforms (one per barrel and satellite)
                                                                ├─ Devices, ClonningCenters
                                                                └─ DroneBays
```

`IShip` is the stored, editable state. `IShipSpecification` is a one-time snapshot for one battle. Combat ([BattleSimulator: Construction](battle_simulator.md#construction)) reads only the specification.

## Ship classes

| Class | Created by | Components | Special behaviour |
| --- | --- | --- | --- |
| `CommonShip` | Session load (`ShipInfo.ToShip`), crafting (`Technology.CreateItem`), market, outposts, turrets | `ObservableCollection`; a change sets `DataChanged` | Editable name. From a `ShipBuild`, it creates `CommonSatellite` objects and marks every component locked. `RandomColor` builds get a random HSV colour from `UnityEngine.Random`. |
| `EnemyShip` | Enemy fleets (`ShipExtensions.Create`), challenges, exploration | As `CommonShip` | `ExtraThreatLevel` = build `DifficultyClass`. When that is not `Default`, `CreateBuilder` installs an `EnemyComponentConverter` seeded by the experience value. |
| `ArenaShip` | `ArenaFleet` (offline multiplayer) | Copy of the source list | Copies name, components, and satellites only. Experience starts at 0 and the colour is default ([SC4](issues/ship_constructor.md#sc4-arena-ships-drop-experience-and-colour)). `CreateBuilder` multiplies armor, shield, damage, and ramming by the arena power multiplier. |
| `EditorModeShip` | ShipEditor database mode | Lazy list from the build; `Locked` cleared | `Name` is the build ID. Every collection change writes the build to its JSON file through `IDatabase.SaveShipBuild`. |

`BaseShip` ([BaseShip.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/BaseShip.cs)) supplies the shared parts:

- `FirstSatellite`, `SecondSatellite`, and `Experience` setters set `DataChanged`.
- `DataChanged` is true when the ship, a satellite, the model, or the colour scheme changed. Setting it to `false` clears all of them.
- `CreateBuilder` ([line 63](../Assets/Modules/ShipConstructor/Scripts/Ships/BaseShip.cs#L63)) creates a `ShipBuilder`, multiplies armor, shield, damage, and ramming bonuses by `Experience.PowerMultiplier`, and adds both satellites.
- `Name` returns the hull name. Its setter throws `InvalidOperationException`; `CommonShip` overrides it.

## Ship model and base stats

`ShipModel` ([ShipModel.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/ShipModel.cs)) has two constructors:

| Constructor | Used for | Faction | Perks | Hull expansion |
| --- | --- | --- | --- | --- |
| `ShipModel(Ship, IDatabase)` | Player ships from a save, crafted ships, outposts, turrets | `Ship.Faction` | Empty list; the caller adds slots or assigns saved perks | Empty; the session deserializes it |
| `ShipModel(ShipBuild, IDatabase)` | Builds: enemies, drones, starting ships, database mode | `BuildFaction`, or `Ship.Faction` when empty | `Perk1`–`Perk3` mapped by `ToModificationType`; `Empty` is skipped | `FullyUpgrade` when `ExtendedLayout` is set |

Both constructors subscribe `OnModificationsChanged` to the perk list and to the hull expansion. `OnModificationsChanged` ([line 97](../Assets/Modules/ShipConstructor/Scripts/Ships/ShipModel.cs#L97)) rebuilds `ShipBaseStats`:

1. It copies the hull feature bonuses into `StatMultiplier` fields: ship weight, equipment weight, velocity, turn rate, shield, armor, energy, drone attack, drone defense, drone build speed.
2. It sets `MaxSatelliteSize` to the hull size class and `MaxSatelliteModelSize` to the hull model scale.
3. It sets `Layout` from `LayoutModifications.BuildLayout()`, `BuiltinDevices` from the hull features, and `Barrels` from the hull.
4. For each perk, it calls `Apply(ref stats)`. Before the first perk with `ChangesBarrels`, it clones the barrel list, so perks never change the database records.
5. It sets `DataChanged`.

`IsBionic` is `Ship.Features.Regeneration`. Bionic hulls cannot take repair bots, and components can be restricted to bionic or mechanical hulls.

A `StatMultiplier` ([StatMultiplier.cs](../Assets/Modules/CommonComponents/Scripts/DataTypes/StatMultiplier.cs)) stores `value − 1`. The default value is therefore 1. `+` adds the bonuses (additive). `*` multiplies the values. `Bonus` returns `value − 1`.

## Layout and hull expansion

`IShipLayout` ([ShipLayout.cs](../Assets/Modules/ShipConstructor/Scripts/Model/ShipLayout.cs)) has `CellCount`, `Rect` (a `LayoutRect` with inclusive bounds), and an indexer `[x, y]` that returns a `CellType`. Two classes implement it:

| Class | Rectangle | Use |
| --- | --- | --- |
| `ShipLayoutAdapter` | `0..Size−1` on both axes | Satellites, and hulls with expansion disabled |
| `LayoutModifications.CustomLayout` | Stock square plus one cell on every edge that has a stock cell; `xMin`/`yMin` can be −1 and `xMax`/`yMax` can be `Size` | Hulls with expansion enabled |

The database `Layout` indexer returns `Empty` outside the square ([Layout.cs:48](../Assets/Modules/Database/Scripts/Types/Layout.cs#L48)), so neighbour checks at the edge are safe.

### Expansion rules

`LayoutModifications.IsDisabledForShip` ([LayoutModifications.cs:17](../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/LayoutModifications.cs#L17)) reads `Ship.CellsExpansions`: `Enabled` and `Disabled` win; otherwise `ShipSettings.DisableCellsExpansions` decides.

`CustomLayout.Reset` copies every stock cell. An empty cell that touches a stock cell on one of four sides becomes `*` and counts as customizable. Other empty cells stay empty.

`TryAddCell(x, y, type)` → `TryModifyCell` accepts a change when:

1. The cell is inside the rectangle.
2. The stock cell is empty and the current cell is not empty (it is `*` or already expanded).
3. The type is `Outer`, or the type equals the stock type of one of the four neighbours.

A `Weapon` cell is stored as `X`. Each accepted change increments `AddedCellCount`. `CellCount` is the stock count plus `AddedCellCount`. Only stock neighbours count, so expansions never chain outward.

`FullyUpgrade` (used for `ShipBuild.ExtendedLayout`) fills every `*` cell with the strongest neighbour type: Weapon (`X`), then Inner, then Engine, else Outer. It does not increment `AddedCellCount` ([DB15.15](issues/database.md#db15-smaller-defects)).

The ship service upgrade panel ([UpgradePanel.cs](../Assets/Scripts/Gui/ShipService/UpgradePanel.cs)) is the only caller of `TryAddCell` and `IsCellValid`. `Reset` returns the hull to stock; the panel then removes components that no longer fit.

Placement of components on these cells is not in this module. The rules are in ShipEditor ([Layout model and placement rules](ship_editor.md#layout-model-and-placement-rules)) and its barrel map ([Barrel map and weapon classes](ship_editor.md#barrel-map-and-weapon-classes)).

## Components

### ComponentInfo

`ComponentInfo` ([ComponentInfo.cs](../Assets/Modules/ShipConstructor/Scripts/ComponentInfo.cs)) is an immutable struct:

| Field | Meaning |
| --- | --- |
| `Data` | Database `Component`. Returns `Component.Empty` for a default value. |
| `ModificationType` | `ComponentMod`, or `ComponentMod.Empty`. |
| `ModificationQuality` | N3, N2, N1, P1, P2, P3. Forced to N3 when the modification is empty. |
| `Level` | Upgrade level 0–20. It adds `+N` to the name and takes part in equality. It changes no stat. |

The implicit `bool` conversion is true when `Data` is set. `Equals` compares component ID, modification, quality, and level.

| Member | Result |
| --- | --- |
| `CreateComponent(shipSize)` | A `CommonComponent` with the modification attached. `shipSize` is the hull cell count. |
| `ItemQuality` | `Common` without a modification; `Low` for N1–N3; `Medium` P1; `High` P2; `Perfect` P3. |
| `Price` | Credits: `Component.Price()` without a modification, `Component.Price(quality)` with one. |
| `PremiumPrice` | Stars, or credits under `IAP_DISABLED`. |
| `IsValidModification` | The modification is empty or listed in `PossibleModifications`. |

### Quality and random components

`ComponentQuality` adds `P0` (no modification) to the modification scale. `GetLevel(baseLevel)` gives the level that each quality needs:

| Quality | Level |
| --- | --- |
| N3 / N2 / N1 | 70 % / 80 % / 90 % of base |
| P0 | base |
| P1 | 4/3 × base + 40 |
| P2 | 5/3 × base + 100 |
| P3 | 2 × base + 150 |

`FromLevel(level, baseLevel)` inverts this table. `Randomize` moves the quality one step up or down with `SquareRange`.

`TryCreateRandomComponent(database, level, faction, random, allowRare, maxQuality)` ([line 48](../Assets/Modules/ShipConstructor/Scripts/ComponentInfo.cs#L48)):

1. Picks a random component with `Level ≤ 1.5 × level` from common (or common and rare) components, filtered by faction when one is given.
2. Computes the quality with `FromLevel(max(10, level), max(10, component.Level))`, then `Randomize`, then clamps to `maxQuality`.
3. Returns the plain component for P0. Otherwise it picks a random modification from `PossibleModifications`.

`CreateRandom` does the same for a given list. `CreateRandomModification` picks a random modification with a quality between two bounds. Loot, crafting, and the Xmas box call these methods. All take a `System.Random`; results are deterministic for a seed.

### IntegratedComponent

`IntegratedComponent` ([IntegratedComponent.cs](../Assets/Modules/ShipConstructor/Scripts/IntegratedComponent.cs)) has read-only `Info`, `X`, `Y`, `BarrelId` and mutable `KeyBinding`, `Behaviour`, `Locked`. `X`, `Y` is the top-left cell of the component's square layout. The meaning of `KeyBinding` and `Behaviour` is in [ShipEditor: Key bindings](ship_editor.md#key-bindings-and-component-modes).

`ComponentExtensions.FromDatabase(InstalledComponent)` converts a build component. It always sets `Locked = true`. `ToString` throws `NotSupportedException`.

### IComponent and CommonComponent

`IComponent` ([IComponent.cs](../Assets/Modules/ShipConstructor/Scripts/Component/IComponent.cs)) is the stats view of one installed component. `CommonComponent` is the only real implementation; `Empty` returns nothing.

| Member | `CommonComponent` behaviour |
| --- | --- |
| `GetStats()` | `ShipEquipmentStats.FromComponent(Stats, Layout.CellCount)`, plus passive device energy, plus drone bay passive energy × (0.5 + 0.005 × ship size), then the modification. |
| `PassiveEnergyConsumption` | Negative recharge rate + passive device energy + unscaled drone bay energy; never below 0 ([DB15.3](issues/database.md#db15-smaller-defects)). |
| `RequiresEnergyToInstall()` | `PassiveEnergyConsumption > 0`. The builder installs such components last. |
| `Devices` | Device stats with energy scaled by `0.05 × ship size` when `ScaleEnergyWithShipSize`, then the modification. |
| `DroneBays` | Drone bay stats with the modification, paired with the drone `ShipBuild`. |
| `Weapons` | One `WeaponData` (weapon, ammunition, `WeaponStatModifier` from the modification) when the component has `Ammunition`. |
| `WeaponsObsolete` | Weapon and `AmmunitionObsolete` stats with the modification applied directly. |
| `UpdateWeaponPlatform` | Raises the platform's auto-aim arc to `AutoAimingArc` and adds `TurretTurnSpeed`. |
| `IsSuitable(ship)` | `CompatibilityChecker.IsCompatibleComponent`. |
| `Upgrades` | Stored, never read ([SC6](issues/ship_constructor.md#sc6-component-upgrades-are-wired-but-inert)). |

`ShipEquipmentStats.FromComponent` ([ShipEquipmentStats.cs:58](../Assets/Modules/ShipConstructor/Scripts/Model/ShipEquipmentStats.cs#L58)) multiplies every stat by the cell count when `ComponentStats.Type` is `PerOneCell`. A positive `EnergyRechargeRate` is recharge; a negative one is consumption. A negative `Weight` goes to `WeightReduction`. Engine power and turn rate of a component that needs no energy also go to the "without energy" fields; combat uses them when the ship runs out of energy.

`ActivationTypeExtension.ValidateKey` normalizes a key: `None` → −1; `Manual` with a negative key → 0; else unchanged.

## Component compatibility

`CompatibilityChecker.IsCompatibleComponent(component, ship)` ([CompatibilityChecker.cs](../Assets/Modules/ShipConstructor/Scripts/Component/CompatibilityChecker.cs)) returns false when:

1. The component is `null`.
2. `Restrictions.ShipSizes` is not empty and does not contain the hull size class.
3. `NotForMechanicShips` and the hull is not bionic, or `NotForOrganicShips` and the hull is bionic.
4. The component has a device and `IsCompatibleDevice` fails: a repair bot on a bionic hull, or a device class that the hull already has as a built-in device.

`IsCompatibleWeapon` always returns true. `DeviceClassExtension.IsSuitable` is an unused copy of the device rule.

Other rules live elsewhere:

| Rule | Where |
| --- | --- |
| Cell type match (`CellType.CompatibleWith`) | [CellType.cs](../Assets/Modules/ShipConstructor/Scripts/CellType.cs); applied by ShipEditor `ShipLayoutModel` and by `ShipLayoutObsolete` |
| Weapon class of a barrel | ShipEditor `ShipLayoutModel.IsCellCompatible` |
| `MaxComponentAmount`, `ComponentGroupTag` limits | ShipEditor `ComponentTracker`; repeated in `ShipBuilder.Build` |
| Satellite size | `ShipExtensions.IsSuitableSatelliteSize` (see [Satellites](#satellites)) |

`CellType.CompatibleWith(component, target)`: `Empty` and `*` cells accept nothing. An `Empty` component fits any other cell. Equal types fit. `InnerOuter` fits `Inner` and `Outer` in both directions.

## Component modifications

`ComponentModification` ([ComponentModification.cs](../Assets/Modules/ShipConstructor/Scripts/Modification/ComponentModification.cs)) applies every `StatModification` of a `ComponentMod`. The value comes from the quality: N3 → `Gray3`, N2 → `Gray2`, N1 → `Gray1`, P1 → `Green`, P2 → `Purple`, P3 → `Gold`. `Create` returns `EmptyModification.Instance` for `ComponentMod.Empty`.

| Stat type | Target | Effect |
| --- | --- | --- |
| `ArmorPoints`, `EnergyCapacity`, `ShieldPoints`, `ShieldRechargeRate`, `Mass` | Equipment stats | × value, only when the stat is positive |
| `EnergyRechargeRate`, `ArmorRepairRate`, `EnergyCost` | Equipment stats | × value |
| `Resistance` | Equipment stats | × value on each positive resistance |
| `EnginePower`, `EngineTurnRate` | Equipment stats | × value on positive values, with and without energy |
| `ExtraHitPoints` | Equipment stats | + value to armor |
| `DroneAttack`, `DroneDefense`, `DroneSpeed`, `DroneRange` | Equipment stats | Bonus × value (`AmplifyDelta`), only when the multiplier is set |
| `DroneAttack`, `DroneDefense`, `DroneSpeed` | Drone bay | + (value − 1) |
| `DroneRange`; `EnergyCost` | Drone bay | × value; × value when positive |
| `DeviceCooldown`, `DeviceRange`, `DevicePower`; `EnergyCost` | Device | × value; × value when positive |
| `WeaponDamage`, `WeaponFireRate`, `WeaponRange`, `WeaponBulletSpeed`, `WeaponBulletMass`, `WeaponAoe`, `EnergyCost` | `WeaponStatModifier` | Multiplier × value |
| Same weapon types | Obsolete weapon and ammunition stats | Direct × value (`EnergyCost` only when positive; bullet mass also scales recoil) |

`GetDescription` formats `ExtraHitPoints` as a signed number and every other value as a signed percent of `value − 1`.

## Ship perks

Perks are `IShipModification` objects in `IShipModel.Modifications`. Each perk changes `ShipBaseStats` in `Apply`. A perk slot without a perk holds a `Constructor.Ships.Modification.EmptyModification`. Crafting adds one to three empty slots by quality ([Technology.cs:105](../Assets/Scripts/Domain/Technologies/Technology.cs#L105)); the modifications panel replaces a slot with `ModificationFactory.Create(type)`. `ShipExtensions.Quality` counts all entries, empty slots included, to give the ship its item quality.

| Type (value) | Class | `Apply` | Multiple | Extra rule |
| --- | --- | --- | --- | --- |
| `AutoTargeting` (0) | `AutoTargetingModification` | Raises every barrel's auto-aim arc to at least 20°. Changes barrels. | No | Suitable when a barrel has arc 0 |
| `HeatDefense` (1) | `HeatDefenseModification` | `HeatResistanceMultiplier += HeatDefenseValue` | Yes | |
| `KineticDefense` (2) | `KineticDefenseModification` | `KineticResistanceMultiplier += KineticDefenseValue` | Yes | |
| `EnergyDefense` (3) | `EnergyDefenseModification` | `EnergyResistanceMultiplier += EnergyDefenseValue` | Yes | |
| `ExtraBlueCells` (4) | — | The factory returns an empty slot | — | Legacy value |
| `LightWeight` (5) | `LightWeightModification` | `ShipWeightMultiplier ×= WeightReduction` | Yes | |
| `Infected` (6) | `InfectedModification` | `RegenerationRate += RegenerationValue`, `ArmorMultiplier ×= RegenerationArmor`, adds built-in device 18 (toxic waste) | No | |
| `UnlimitedRespawn` (7) | — | No factory case: `Create` throws ([DB1](issues/database.md#db1-the-unlimitedrespawn-perk-crashes-ship-creation), [SC1](issues/ship_constructor.md#sc1-a-saved-perk-without-a-factory-case-deletes-the-ship)) | — | |
| `WeaponClass` (8) | `WeaponClassMod` | Clears the weapon class of every barrel; `DamageMultiplier ×= 1 − AttackReduction`. Changes barrels. | No | Suitable when a barrel has a class; hidden by `RemoveWeaponSlotMod` |
| `SatelliteSize` (9) | `SatelliteSizeMod` | Max satellite class: Frigate/Destroyer → Cruiser, else +1. Max satellite model scale + 0.4. | No | Suitable below Titan; hidden by `RemoveBiggerSatellitesMod` |
| `EnergyRechargeCooldown` (10) | `EnergyRechargeCooldownMod` | `EnergyMultiplier ×= 1 − EnergyReduction`; recharge cooldown × 0.01 | No | Hidden by `RemoveEnergyRechargeCdMod` |
| `ShieldRechargeCooldown` (11) | `ShieldRechargeCooldownMod` | `ShieldMultiplier ×= 1 − ShieldReduction`; shield cooldown × 0 | No | Hidden by `RemoveShieldRechargeCdMod` |

Values come from [ShipModSettings](database/schema/ShipModSettings.md). `ModificationFactory.AvailableMods` lists what the player can install. `ModificationTypeExtension.IsSuitable` checks "multiple" and the extra rules. `GetInstallPrice` is 10 stars for any perk. `GetIconId` and `GetColor` give the UI icon and colour.

`ShipModificationExtensions.Serialize` packs a perk as `(type << 32) | seed`. No perk uses its seed. `CreateRandomModifications` has no caller.

`ShipPerkType` (database) and `ModificationType` (code) use different numbers: `ShipPerkType` starts with `Empty = 0`, `ModificationType` with `Empty = −1`. `ToModificationType` maps them by name.

## Satellites

| Class | Created by | Components |
| --- | --- | --- |
| `CommonSatellite` | Session load, `CommonShip(ShipBuild)`, enemy fleets, ShipEditor, `CreateCopy` | `ObservableCollection`; a change sets `DataChanged`. From a `SatelliteBuild`, every component is locked. |
| `EditorModeSatellite` | `EditorModeShip` | Lazy list with `Locked` cleared. Every change writes the `SatelliteBuild` JSON. `DataChanged` is always false. |

`Satellite.IsSuitable(shipClass, shipModelScale)` ([SatelliteExtensions.cs:18](../Assets/Modules/ShipConstructor/Scripts/Satellites/SatelliteExtensions.cs#L18)): when the satellite has a size class, the ship class must be at least that class. Otherwise the ship model scale must be at least twice the satellite model scale. `IShip.IsSuitableSatelliteSize` adds that starbases accept no satellite and uses the model's `MaxSatelliteSize` and `MaxSatelliteModelSize`, which the `SatelliteSize` perk raises.

`SuitableFor(Ship)` and `LimitClass(DifficultyClass)` filter satellite builds for enemy ships. They use the database hull, so the `SatelliteSize` perk of a build does not widen the choice.

`SatelliteExtensions.CreateCopy` creates a `CommonSatellite` with the same `IntegratedComponent` objects, not copies.

## Building a ship specification

`ShipBuilder` ([ShipBuilder.cs](../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs)) turns a ship into an `IShipSpecification`.

| Constructor | Use | Extra settings |
| --- | --- | --- |
| `ShipBuilder(IShip)` | `IShip.CreateBuilder` | Visual level, custom AI, threat level, ship and turret colour |
| `ShipBuilder(ShipBuild)` | Drones (`DroneBay`) | Damage, armor, shield × (1 + 0.5 × `DifficultyClass`); the drone bay then overwrites them ([DB15.14](issues/database.md#db15-smaller-defects)) |
| `ShipBuilder(IShipModel, components)` | Base for the others | — |

Callers set `Bonuses`, `Converter`, and `ExtraDroneBayCapacity` before `Build`. `SizeMultiplier` is copied into the stats but nothing reads it ([SC7](issues/ship_constructor.md#sc7-dead-and-duplicate-code)).

### Build steps

`Build(ShipSettings)` ([line 65](../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L65)):

1. Create a `ShipStatsCalculator` for the hull. Copy `Bonuses`, colours, and `ShipBaseStats` from the model.
2. Create `ShipInfo(id, visual difficulty, visual level, size)`. `size` = hull cell count + half of all satellite cell counts.
3. Create weapon platforms (`GetPlatforms`):
   1. One per hull barrel, with `BarrelId` 0, 1, 2, … in barrel order.
   2. One per satellite, with a `CompanionSpec` (satellite, side, weight = cells × `DefaultWeightPerCell` / 1000). A satellite with one barrel gets the next `BarrelId`. A satellite without a barrel gets `Barrel.Empty` and `BarrelId` −1. A satellite with more barrels throws ([DB15.6](issues/database.md#db15-smaller-defects)).
4. Add the hull's built-in devices. A manual built-in device gets key 5; others get −1.
5. Enumerate components (`Components`): hull components first, then each satellite's components. The converter adds a barrel offset: hull barrels count first, then each satellite's barrels.
6. For each component, in order:
   1. Skip it when its `ComponentGroupTag` has reached `MaxInstallableComponents`; else count it.
   2. Skip it when `MaxComponentAmount` > 0 is reached; else count it. Both counts happen before the next check ([DB15.18](issues/database.md#db15-smaller-defects)).
   3. Create the `IComponent` for the hull cell count. Skip it when `IsSuitable` fails.
   4. Put it in the energy-dependent list when `RequiresEnergyToInstall`, else in the independent list.
7. Install every independent component, then every energy-dependent component. An energy-dependent component installs only when the current `EnergyRechargeRate` ≥ its energy consumption. Otherwise only its consumption and weight are added.
8. `ApplyStats` copies the final weapon multipliers to every platform and the drone multipliers to every drone bay.

Installing one component (`InstallComponent`, [line 131](../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L131)):

1. Add its `ShipEquipmentStats` to the ship totals.
2. Add each device (`DeviceData`, key normalized by `ValidateKey`). Cloning centers go to a separate list.
3. Add each drone bay with `Capacity + ExtraDroneBayCapacity`, the key, and `DroneBehaviour`.
4. When `BarrelId ≥ 0` and a platform with that ID exists: update the platform's aim arc and turn speed, then add each weapon.

Satellite components count in the ship totals: their armor, energy, engines, and weight add to the hull's stats.

### Converters

`IComponentConverter.Process(component, firstBarrelId)` returns a `ComponentSpec` (info, global barrel ID, key, behaviour).

- `DefaultComponentConverter` keeps the info and adds the barrel offset.
- `EnemyComponentConverter(level, random)` improves enemy components. It draws `quality` with `SquareRange(min, max)`: `min` = 0 below level 150, else 1; `max` = min(level / 75, 3). A component already at `N1 + quality` or better stays. A component with a modification gets quality `N1 + quality`. A plain component gets a random modification at that quality. Quality 0 changes nothing. `EnemyShip`, outposts, and turrets use it.

### Output classes

| Class | Content |
| --- | --- |
| `ShipBuilderResult` | `Info`, `Stats`, platforms, devices, cloning centers, drone bays, custom AI. |
| `WeaponPlatform` | Barrel position, rotation, offset, size, image; auto-aim arc (0 when the turn speed is 0); turn speed (barrel value, or 270 when unset); companion; weapon lists. The getters push the platform multipliers into each weapon. |
| `WeaponPlatform.WeaponData` | Weapon, ammunition, and `WeaponStatModifier` × platform damage, fire rate, energy, and range multipliers. |
| `DeviceData` | Device stats and key. |
| `DroneBayData` | Drone bay stats with the ship's drone bonuses added and then × total damage and defense bonus; drone build; key; behaviour. |
| `CompanionSpec` | Satellite, side, weight. |

## Stats calculation

`ShipStatsCalculator` ([ShipStatsCalculator.cs](../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs)) implements `IShipStats`. It combines four sources:

- `ShipSettings`: global base values per ship and per cell.
- `BaseStats` (`ShipBaseStats`): hull features and perks.
- `EquipmentStats` (`ShipEquipmentStats`): the sum of all installed components. Multipliers inside it add their bonuses.
- `Bonuses` (`ShipBonuses`): experience, arena, difficulty, player skills.

`CellCount` is the hull layout count with expansions. Satellite cells are not included. In the formulas, Σ means the equipment total, and `B.x` and `Bo.x` mean the `Value` of a base-stat or bonus multiplier.

| Stat | Formula |
| --- | --- |
| `ArmorMultiplier` | `Bo.ArmorPoints × B.Armor` |
| `ArmorPoints` | `(BaseArmorPoints + ArmorPointsPerCell × CellCount + ΣArmor) × ArmorMultiplier`; 0 when the sum before the multiplier is below 1 |
| `EnergyPoints` | `(BaseEnergyPoints + ΣEnergy) × B.Energy × Bo.Energy` |
| `EnergyRechargeRate` | `BaseEnergyRechargeRate − ΣConsumption + ΣRecharge × B.Energy × Bo.Energy` |
| `ShieldPoints` | `ΣShield × B.Shield × Bo.ShieldPoints` |
| `ShieldRechargeRate` | `(ΣShieldRecharge + BaseShieldRechargeRate) × Bo.ShieldPoints × B.Shield × Bo.ShieldRecharge` |
| `ArmorRepairRate` | `ΣRepair × Bo.ArmorPoints × B.Armor + max(0, ArmorPoints × regen)`; `regen` = `B.RegenerationRate`, + 0.01 for a regenerating hull |
| Kinetic / energy / thermal resistance | `r + (ArmorPoints + r) × bonus`; `r` = Σresistance × `ArmorMultiplier`; `bonus` = hull feature + perk bonus + skill bonus |
| Resistance percentage | `R / (ArmorPoints + R)` |
| `EnergyAbsorption` | `ΣAbsorption × Bo.ArmorPoints` |
| `Weight` | `max(minWeight, DefaultWeightPerCell × CellCount × B.ShipWeight + ΣWeight × B.EquipmentWeight + ΣWeightReduction) / 1000`; `minWeight` = `max(MinimumWeightPerCell × CellCount × B.ShipWeight, 5 × MinimumWeightPerCell)` |
| `EnginePower`, `TurnRate` | `Bo.Velocity × B.Velocity × ΣEnginePower`; `Bo.TurnRate × B.TurnRate × ΣTurnRate`. Same for the "without energy" values. |
| `DamageMultiplier` | `Bo.Damage × B.Damage`. Combat uses it for devices. |
| `WeaponDamageMultiplier` | `ΣWeaponDamage × Bo.Damage × B.Damage` |
| `WeaponFireRateMultiplier`, `WeaponEnergyCostMultiplier`, `WeaponRangeMultiplier` | Equipment total only |
| `DroneDamageMultiplier`, `DroneDefenseMultiplier` | Equipment total + base drone attack / defense (bonuses add) |
| `DroneSpeedMultiplier`, `DroneRangeMultiplier` | Equipment total only |
| `DroneBuildSpeed` | `(BaseDroneReconstructionSpeed + ΣSpeed) × B.DroneBuildSpeed / ΣBuildTimeMultiplier` |
| `DroneBuildTime` | `ΣBuildTimeMultiplier / speed`, or 0 when the speed is 0 |
| Armor / energy / shield recharge cooldown | `ShipSettings` cooldown × max(0, Σ cooldown multiplier × base multiplier). Armor has no base multiplier. |
| `RammingDamageMultiplier` | `ΣRammingMultiplier × Bo.Ramming × (1 + ΣRamming / (ΣRamming + ArmorPoints))` |
| `ShieldCorrosiveResistancePercentage` | `ShipSettings.ShieldCorrosiveResistance` |

The `*BaseCooldown` fields of `ShipEquipmentStats` are never set, so components change cooldowns only through multipliers ([SC7](issues/ship_constructor.md#sc7-dead-and-duplicate-code)).

### Where bonuses come from

| Source | Code | Bonuses |
| --- | --- | --- |
| Ship experience | `BaseShip.CreateBuilder` | Armor, shield, damage, ramming × `11^(level / 100)` |
| Arena power | `ArenaShip.CreateBuilder` | Same four × power multiplier |
| Player skills | [ShipBuilderExtensions.cs](../Assets/Scripts/Combat/Domain/ShipBuilderExtensions.cs) | Damage, armor, shield, shield recharge, three extra resistances |
| Drone bay | [DroneBay.cs:36](../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/DroneBays/DroneBay.cs#L36) | Armor, shield, damage, velocity, turn rate (overwritten) |
| Build difficulty | `ShipBuilder(ShipBuild)` | Damage, armor, shield × (1 + 0.5 × class) |

The ShipEditor statistics panel builds a `ShipStatsCalculator` itself, without bonuses ([SE11](issues/ship_editor.md#se11-statistics-duplicate-shipbuilder-and-omit-combat-bonuses)).

## Engine stats

`EngineStats` ([EngineStats.cs](../Assets/Modules/ShipConstructor/Scripts/Model/EngineStats.cs)) converts engine power and turn rate into motion limits. `W` is the ship weight, and `n` is the hull cell count. The `Max*` values come from `ShipSettings`.

| Value | Formula |
| --- | --- |
| `Velocity` | `min(MaxVelocity, power × 50 / (√W × n))` |
| `AngularVelocity` | `min(MaxAngularVelocity, turn × 50 / (√W × n)) × 30` (degrees) |
| `Propulsion` | `min(MaxAcceleration, power × 100 / (W × n))` |
| `TurnRate` | `min(MaxAngularAcceleration, turn × 200 / (W × √W × n)) × 30` (degrees) |

`IsNull` is true when propulsion and turn rate are both near 0. Combat ([ShipFactory.cs:306](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L306)) creates one set with energy and one without. The ShipEditor statistics panel uses the same struct.

## Weapon damage calculator

`WeaponDamageCalculator` ([WeaponDamageCalculator.cs](../Assets/Modules/ShipConstructor/Scripts/Component/WeaponDamageCalculator.cs)) estimates damage for UI text. Combat does not use it. The ShipEditor component panel and the crafting description call it.

`CalculateWeaponDamage(weapon, ammunition, modifier)` for the current ammunition format:

1. A damage-over-time weapon is one whose ammunition impact type is `DamageOverTime`. For a `Common` weapon, the bullet must also live at least one fire interval.
2. Per-shot damage is `GetDPS(ammunition, 1)`; 0 for damage-over-time weapons.
3. DPS is `GetContinuousDPS` for damage-over-time weapons, else `GetDPS(ammunition, fire rate × modifier)`.
4. `GetDPS` sums the impact effects (damage, siphon, devour, progressive damage; repair; shield drain/recharge; energy drain/recharge) and special flags. It multiplies by the fire rate, and for damage over time also by the bullet lifetime. It adds the DPS of `SpawnBullet` triggers recursively, up to 100 levels.
5. Damage and DPS are multiplied by the damage modifier. Energy drain uses the energy cost modifier.
6. Range, velocity, impulse, and area of effect use the matching modifiers. Area of effect depends on bullet shape and nested triggers.

`CalculateWeaponDamage(weapon, ammunitionObsolete)` handles the old format with a switch on `AmmunitionClassObsolete`.

`DamageInfo` holds kinetic, heat, energy, corrosive, repair, shield, energy drain, and `WeaponSpecialEffect` flags. `WeaponInfo` adds magazine, continuous flag, energy cost, fire rate, range, bullet velocity, impulse, and area of effect.

## Ship validation

Two classes are named `ShipValidator`. This module has `Constructor.Ships.ShipValidator` ([ShipValidator.cs](../Assets/Modules/ShipConstructor/Scripts/Ships/ShipValidator.cs)). The application has `Domain.Shipyard.ShipValidator` ([ShipValidator.cs](../Assets/Scripts/Domain/Player/ShipValidator.cs)) for layout checks.

| Method | Rule | Callers |
| --- | --- | --- |
| `IsAvailableForPlayer` | No extra threat level, and no component with `Availability.None` on the hull or satellites | `IsAllowedOnArena` |
| `IsShipViable` | Builds the ship. Energy recharge > 0; no weapon costs more energy per shot than the ship has; at least one weapon or drone bay. | `InitializationState` (editable database check), `IsAllowedOnArena` |
| `IsAllowedOnArena` | `IsAvailableForPlayer`, no `ExtraBlueCells` perk, no forbidden component (always none), `IsShipViable` | Arena fleet filter and ship list |

## Game formulas

These classes are general game rules. They are in this module because they read `IShip`.

### Experience

`Maths.Experience` ([Experience.cs](../Assets/Modules/ShipConstructor/Scripts/Maths/Experience.cs)) stores the value in an `ObscuredLong` (memory obfuscation, [ObscuredLong.cs](../Assets/ModulesShared/Utilites/Security/ObscuredLong.cs)).

| Member | Formula |
| --- | --- |
| `LevelToExp(level)` | `100 × level³` |
| `Level` | The largest level with `LevelToExp(level) ≤ exp` |
| `PowerMultiplier` | `11^(level / 100)`: ×1 at level 0, ×3.3 at 50, ×11 at 100 |
| `TotalExpForShip(ship)` | `(1 + (level + 1)^1.2) × cells^1.2 × (1 + threat level)`. Combat gives this × damage dealt. |

### Threat

`Maths.Threat` ([Threat.cs](../Assets/Modules/ShipConstructor/Scripts/Maths/Threat.cs)): ship power = `PowerMultiplier × (hull + satellite cells) × (1 + 0.5 × threat level)`. `GetLevel(player, enemy)` uses `enemy / (player + enemy)`: below 0.2 VeryEasy, 0.4 Easy, 0.6 Average, 0.8 Hard, else VeryHard. Fleets, the threat panel, and `Occupants` use it.

### Distance

`Maths.Distance` ([Distance.cs](../Assets/Modules/ShipConstructor/Scripts/Maths/Distance.cs)) maps a star level ("distance") to content values:

| Method | Formula |
| --- | --- |
| `FleetSize` | `1 + min(3 + rand(100) × rand(100) / 2000, distance / 3)` |
| `CombatTime` | `max(40, 100 − distance)` |
| `AiLevel` | `2 × distance` |
| `ComponentLevel` | `max(distance, 1)` |
| `Credits` | `100 + 2 × max(distance, 1)` |
| `MaxShipClass` / `MinShipClass` | `distance / 25`; `Default` below 50, else `Class1` |
| `CompanionClass` | `(distance + 10) / 20` |

### Ship extensions

[ShipExtensions.cs](../Assets/Modules/ShipConstructor/Scripts/Extensions/ShipExtensions.cs):

| Method | Result |
| --- | --- |
| `Price` | `cells²` credits |
| `Scraps` | `cells / 5` |
| `CanBeSold` | Not `Unique`, and type `Common` or `Flagship` |
| `Quality` | Item quality from the number of perk slots: 1 Medium, 2 High, 3+ Perfect |
| `Create(builds, distance, random, database)` | One `EnemyShip` per build. Level = `GalaxySettings.EnemyLevel(distance ± min(10, distance / 5))`. A `Common` ship gets each empty satellite slot filled with a 2/3 chance from suitable satellite builds up to `CompanionClass(distance)`. |
| `SetLevel`, `OfLevel`, `Unlocked`, `RandomizeColor`, `CreateCopy` | Helpers; the last four have no live caller |

## Serialization formats

Current saves do not store ShipConstructor objects. GameSession converts them to its own structs ([GameSession](game_session.md)). This module keeps these formats:

| Format | Code | Used by |
| --- | --- | --- |
| Perk `long`: `(type << 32) \| seed` | `ShipModificationExtensions` | GameSession `ShipInfo` |
| Hull expansion bytes: one byte per customizable cell, row-major over the rectangle (0 none, 1 outer, 2 inner, 3 engine, 4 weapon); empty when nothing was added | `LayoutModifications.Serialize` / `Deserialize` | GameSession `ShipInfo` |
| Old hull expansion: run-length cells in stock coordinates | `CustomLayout.DeserializeObsolete` (fallback) | Old saves |
| Colour `long`: B bits 0–7, G 8–15, R 16–23, hue 24–31, saturation 32–39, type 40–47 | `ShipColorScheme.Value` | GameSession `ShipInfo` |
| `ComponentInfo` as 30-bit `int` or 64-bit `long`; `IntegratedComponent` as bytes or `long` | `[Obsolete]` methods in `ComponentInfo.cs` and `IntegratedComponent.cs` | GameSession legacy loaders, arena `FleetWebSerializer` |

## Consumers

| Consumer | Uses |
| --- | --- |
| [GameSession](game_session.md) | Converts `IShip`, `ISatellite`, `IShipPreset`, `IntegratedComponent`, `ComponentInfo` to and from save structs. Creates `ShipModel`, `CommonShip`, `CommonSatellite`, `ModificationFactory`. |
| [BattleSimulator](battle_simulator.md) | Reads `IShipSpecification` and `IShipStats`; builds drones with `ShipBuilder(ShipBuild)`; uses `EngineStats`, `CompanionLocation`, `ColorScheme`. |
| [ShipEditor](ship_editor.md) | Edits `IShip` and satellites; uses `IShipLayout`, `CompatibilityChecker`, `ShipStatsCalculator`, `WeaponDamageCalculator`, `ShipPreset`, `IComponentUpgradesProvider`. |
| Gui component list | Builds the inventory tree from `ComponentInfo`. |
| Application layer | Fleet, inventory, loot, crafting, ship service (hull expansion, perks), arena, combat entry (`CreateBuilder().ApplyPlayerSkills().Build()`), exploration enemies, threat display. See [Application layer](application_layer.md). |

## Extending the module

| Change | Where |
| --- | --- |
| New component stat | Add the field to `ComponentStats` in the schema, then to `ShipEquipmentStats` (`FromComponent`, `AddStats`), then a property in `IShipStats` and `ShipStatsCalculator`. Update `ClonningDevice`'s stats wrapper, the ShipEditor statistics panel, and combat `ShipStats`. |
| New component modification type | Add a `StatModificationType` value in the schema and a `case` in each `ComponentModification.Apply` that it affects. |
| New perk | Add a `ShipPerkType` value (schema), a `ModificationType` value, a class implementing `IShipModification`, a `ModificationFactory.Create` case, an `AvailableMods` entry, and `ToModificationType`, `IsSuitable`, `AllowMultiple`, `GetIconId`, `GetColor` cases. Set `ChangesBarrels` when `Apply` changes barrels. Add the `$Ship_*` localization key. |
| New compatibility rule | `CompatibilityChecker.IsCompatibleComponent`. It then applies to the editor, `ShipValidator`, and `ShipBuilder`. |
| New bonus source | Multiply a `ShipBonuses` field on the builder before `Build`. |
| New `IShip` kind | Derive from `BaseShip`. Override `Components`, and `Name` if it is editable. |

Keep the numbering of `ModificationType` stable: saves store it.

## Validation checklist

The module has no tests. Check through the game:

1. Start a new game. Check the starting ship stats in the ShipEditor and in combat.
2. Craft ships at every quality. Install, replace, and save each perk. Reload the save.
3. Expand a hull with every cell type in the ship service. Reload, reset, and check the removed components.
4. Fight enemies with difficulty classes above `Default`. Check enemy component qualities and satellites.
5. Fight with satellites that have one barrel and no barrel. Check satellite weapons and weights.
6. Check a ship whose energy recharge does not cover all energy-dependent components: those components must not work.
7. Run the arena with a ship that has an `Availability.None` component and with a non-viable ship.
8. Open an editable database and read the "invalid build" and "invalid satellite layout" log lines.
