# Satellite

A satellite (in the UI: companion) is a small hull that flies next to a ship. It has a cell layout for components and at most one weapon mount. A ship has a left and a right satellite slot. A [SatelliteBuild](SatelliteBuild.md) adds components to a satellite. The ship editor, the ship builder, combat, loot, and the workshop read satellite records.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `7` |
| Kind | collection |
| Folder | [`Resources/Database/Satellite/`](../../../Assets/Modules/Database/Resources/Database/Satellite/) |
| Records in the built-in catalog | 25: 21 without size class (7 designs in sizes 0.8, 0.6, 0.4), 4 of class `Titan` |
| Schema | [Satellite.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Satellite.xml) |
| Generated class | `GameDatabase.DataModel.Satellite` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Satellite.cs), [hand-written part](../../../Assets/Modules/Database/Scripts/DataModel/Satellite.cs)) |
| Runtime wrapper | `CommonSatellite` (`ISatellite`): the satellite plus installed components |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Name` | string | | Display name, usually a localization key. |
| `ModelImage` | image (`Satellite`) | | Sprite in combat, the ship editor, and the workshop. The name also selects an optional combat prefab `Resources/Combat/Satellites/<name>`. |
| `ModelScale` | float | 0.1…100 | Size in combat world units. It also sets the distance from the ship and the fit for satellites without a size class. |
| `SizeClass` | enum [SizeClass](enums.md#sizeclass) | `Frigate` | Smallest ship class that can carry the satellite. `Undefined` uses `ModelScale` instead (see [Fit](#fit)). The default `Frigate` fits every hull except hulls of class `Undefined`. |
| `Layout` | [layout](../../database.md#layout) | | Cell grid for components. The cell count sets the weight, the price, and the threat. |
| `Barrels` | struct list [Barrel](structures.md#barrel) | | The weapon mount. Use zero or one barrel. More than one barrel makes the ship builder throw ([DB15.6](../../issues/database.md#db15-smaller-defects)). |

## Notes

### Fit

A ship can carry a satellite when one rule holds ([SatelliteExtensions.cs:18](../../../Assets/Modules/ShipConstructor/Scripts/Satellites/SatelliteExtensions.cs#L18)):

1. The satellite has a size class, and the ship's maximum satellite class is equal or bigger.
2. The satellite has `SizeClass` `Undefined`, and the ship's maximum satellite scale ≥ 2 × the satellite's `ModelScale`.

The maximum satellite class is the hull's `SizeClass`, and the maximum scale is the hull's `ModelScale` ([ShipModel.cs:111](../../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipModel.cs#L111)). The `SatelliteSize` perk raises the class by one (to at least `Cruiser`) and the scale by 0.4. Starbases carry no satellites ([ShipExtensions.cs:16](../../../Assets/Modules/ShipConstructor/Scripts/Extensions/ShipExtensions.cs#L16)).

The ship editor ([ComponentTracker.cs:54](../../../Assets/ModulesShared/ShipEditor/Scripts/Model/ComponentTracker.cs#L54)) and the fleet validator ([ShipValidator.cs:57](../../../Assets/Scripts/Domain/Player/ShipValidator.cs#L57)) apply the rule to player ships. Random enemy satellites use the hull values without perks. A ship build's own satellites skip the check.

### Combat

`SatelliteFactory.CreateSatellite` builds the satellite ([SatelliteFactory.cs:38](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/SatelliteFactory.cs#L38)):

1. A prefab `Resources/Combat/Satellites/<ModelImage>` wins when it exists. Otherwise the `default` prefab gets the sprite.
2. The body scale is `ModelScale`. The mass is `cells × ShipSettings.DefaultWeightPerCell / 1000` ([ShipBuilder.cs:525](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L525)).
3. The left satellite sits at `(−0.5, 0.5) × ship scale`, moved out by `1.5 × ModelScale`. The right one is mirrored.
4. A satellite turns toward targets within −10°…190° (left) or −190°…10° (right) when its barrel has an arc of 5° or more. A barrel with an `Image` or a 360° arc is a turret instead: the satellite stays fixed and the turret turns.
5. A destroyed satellite plays the old drone explosion.

The satellite's components add their stats to the ship. Its weapon uses the barrel index after the hull's barrels.

### Economy

| Value | Formula, with `n` = cell count | Code |
| --- | --- | --- |
| Price | `20 × n²` credits, or `n` stars | [Price.cs:10](../../../Assets/Modules/Database/Scripts/Extensions/Price.cs#L10) |
| Crafting price | `200 × n²` credits and `max(0, n / 7 − 1)` stars | [Price.cs:20](../../../Assets/Modules/Database/Scripts/Extensions/Price.cs#L20) |

- A satellite [technology](Technology.md) unlocks crafting. The workshop crafts a satellite without components, in `Common` quality only ([Technology.cs:243](../../../Assets/Scripts/Domain/Technologies/Technology.cs#L243)).
- Ruins give a random satellite with a 1/3 chance: not `Titan`, at most `max(6, level / 2)` cells ([LootGenerator.cs:295](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L295)).
- The black market sells satellites that are not `Titan` ([BlackMarketInventory.cs:71](../../../Assets/Scripts/Legacy/Game/Quests/Inventory/BlackMarketInventory.cs#L71)).
- [Loot](Loot.md) with item type `Satellite` gives a fixed satellite.

### Data rules

1. Give a satellite zero or one barrel. Use a barrel when the satellite should carry a weapon.
2. Set `SizeClass` explicitly. Use `Undefined` for satellites that fit by scale. The default `Frigate` lets almost every ship carry the satellite.
3. Use `Titan` for large satellites that must stay out of random loot and the black market.
4. A changed satellite ID breaks saves: the session stores satellites by ID.
