# ExplorationSettings

Exploration settings define the planet exploration encounters that are not ordinary enemy ships: the hulls of outposts and turrets, the faction and boss of infected planets, and the damage of gas clouds. The exploration data, the exploration scene, and the loot generator read them.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `103` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/Exploration.json`](../../../Assets/Modules/Database/Resources/Database/Settings/Exploration.json) |
| Schema | [ExplorationSettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/ExplorationSettings.xml) |
| Generated class | `GameDatabase.DataModel.ExplorationSettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/ExplorationSettings.cs)), read as `IDatabase.ExplorationSettings` |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The *Built-in* column shows the value in the built-in catalog; empty means the schema default.

| Field | Type | Default / range | Built-in | Meaning |
| --- | --- | --- | --- | --- |
| `OutpostShip` | ref [Ship](Ship.md) | | 101 | Hull of an outpost objective. The game fills it with generated components: reactors, fuel cells, shields or resistances, armor, drone bays, the small drone factory, and drone upgrades. It has no guns. The outpost is a stationary turret ([RandomOutpostBuilder.cs:30](../../../Assets/Scripts/Domain/Exploration/RandomOutpostBuilder.cs#L30)). |
| `TurretShip` | ref [Ship](Ship.md) | | 100 | Hull of the 1–4 turrets around an outpost ([RandomTurretBuilder.cs:30](../../../Assets/Scripts/Domain/Exploration/RandomTurretBuilder.cs#L30)). The game fills it with reactors, fuel cells, armor, and one random weapon of at least 3 cells ([RandomTurretBuilder.cs:63](../../../Assets/Scripts/Domain/Exploration/RandomTurretBuilder.cs#L63)). |
| `InfectedPlanetFaction` | ref [Faction](Faction.md) | | 16 | Faction of infected planets ([Planet.cs:47](../../../Assets/Scripts/Domain/Exploration/Planet.cs#L47)). Hive rewards contain components, ships, and blueprints of this faction ([LootGenerator.cs:150](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L150)). |
| `HiveShipBuild` | ref [ShipBuild](ShipBuild.md) | | 275 | The hive: the boss of an infected-planet hive objective, at the planet level ([ExplorationData.cs:191](../../../Assets/Scripts/Domain/Exploration/ExplorationData.cs#L191)). Its 1–4 guardians are random enemy builds that suit the planet. |
| `GasCloudDPS` | expression `IntToFloat(level)` | `MIN(level*2,500)`, 1… | | Corrosive damage per second inside a gas cloud on a planet of this level ([ExplorationSceneManager.cs:344](../../../Assets/Scripts/Combat/Manager/ExplorationSceneManager.cs#L344), [SpaceObjectFactory.cs:546](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Objects/SpaceObjectFactory.cs#L546)). |

## Notes

### Levels

All encounters use the planet level, which equals the star level. Outposts, turrets, and the hive get the experience level `GalaxySettings.EnemyLevel(level)` ([GalaxySettings: Enemy level](GalaxySettings.md#enemy-level)). Exploration enemy ships and the faction rule for them are on [Faction: Distance ranges](Faction.md#distance-ranges).

### Hard-coded parts

The outpost and turret builders install components by ID: 76 (large nuclear reactor), 65 (large fuel cells), 52 (small drone factory) ([RandomOutpostBuilder.cs:106](../../../Assets/Scripts/Domain/Exploration/RandomOutpostBuilder.cs#L106)), and 85 (large projectile cannon) when no weapon fits the turret. A mod that changes these components changes every outpost. See [DB15.10](../../issues/database.md#db15-smaller-defects).

### Data rules

1. Set all four references. The exploration code does not check them for `null`.
2. Keep components 52, 65, 76, and 85.
3. Make `InfectedPlanetFaction` a faction with components, ship builds, and a research tree. Otherwise hive rewards contain only commodities.
4. A mod settings record replaces the whole record. Copy every value, not only the changed one ([DB7](../../issues/database.md#db7-settings-records-cannot-be-partly-overridden)).
