# GalaxySettings

Galaxy settings define the new-game setup (ships, inventory), the enemy level curve, the star levels where hull sizes appear, special factions and builds, the starbase attack quest, and the combat rules of each battle type. The new-game state, the enemy fleet factories, the ship-build query, combat, and the star content (starbases, ruins, challenges, survival) read them.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `101` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/Galaxy.json`](../../../Assets/Modules/Database/Resources/Database/Settings/Galaxy.json) |
| Schema | [GalaxySettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/GalaxySettings.xml) |
| Generated class | `GameDatabase.DataModel.GalaxySettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/GalaxySettings.cs)), read as `IDatabase.GalaxySettings` |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The *Built-in* column shows the value in the built-in catalog; empty means the schema default.

| Field | Type | Default / range | Built-in | Meaning |
| --- | --- | --- | --- | --- |
| `AbandonedStarbaseFaction` | ref [Faction](Faction.md) | | 9 | Faction of the ruins guards. The ruins fleet has builds of this faction with class `Class1` or higher, twice the usual count ([Fleet.cs:108](../../../Assets/Scripts/Legacy/Model/Factories/Fleet.cs#L108)). The ruins reward includes research points of this faction ([LootGenerator.cs:307](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L307)). |
| `StartingShipBuilds` | ref list [ShipBuild](ShipBuild.md) | | 39, 45, 51 | Ships of a new game, all in the active group, with unlocked components. The first `Frigate` becomes the exploration ship ([StartingNewGameState.cs:65](../../../Assets/Scripts/GameStateMachine/States/StartingNewGameState.cs#L65)). The first entry is also a fallback ship of challenges ([Challenge.cs:72](../../../Assets/Scripts/Domain/Galaxy/StarContent/Challenge.cs#L72)). The loader throws on a missing build. |
| `StartingInventory` | ref [Loot](Loot.md) | | 12 | Items of a new game. Given once, without a quest (seed −1, level 0) ([StartingNewGameState.cs:56](../../../Assets/Scripts/GameStateMachine/States/StartingNewGameState.cs#L56)). `0`: nothing. |
| `SupporterPackShip` | ref [ShipBuild](ShipBuild.md) | | 92 | Ship added for owners of the supporter pack, when the fleet has no ship with the same hull ([PlayerFleet.cs:174](../../../Assets/Scripts/Domain/Player/PlayerFleet.cs#L174)). `0`: none. |
| `DefaultStarbaseBuild` | ref [ShipBuild](ShipBuild.md) | | 192 | Starbase of a region whose faction has no starbase build of the needed class ([Fleet.cs:102](../../../Assets/Scripts/Legacy/Model/Factories/Fleet.cs#L102)). |
| `MaxEnemyShipsLevel` | int | 300, 0…500 | | A variable for `EnemyLevel`. Nothing else reads it. |
| `EnemyLevel` | expression `IntToInt(distance)` | `MIN(3*distance/5 - 5, MaxEnemyShipsLevel)`, 0…500 | | Experience level of enemy ships at a star level. See [Enemy level](#enemy-level). |
| `ShipMinSpawnDistance` | expression `SizeClassToInt(size)` | `IF(size == Destroyer, 5, size == Cruiser, 15, size == Battleship, 50, size == Titan, 100, 0)`, 0…1000 | | Lowest star level where enemy hulls of a size class appear ([ShipBuildQuery.cs:89](../../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs#L89)). A test requires strictly growing values from `Frigate` to `Battleship` ([DatabaseTests.cs:25](../../../Assets/Modules/Database/Unity/Tests/DatabaseTests.cs#L25)). |
| `CaptureStarbaseQuest` | ref [Quest](Quest.md) | | 9 | Quest that an attack on a faction starbase starts. `0`: a direct battle that sets the region reputation to −100 ([StarBase.cs:60](../../../Assets/Scripts/Domain/Galaxy/StarContent/StarBase.cs#L60)). The quest needs `StartCondition` `Manual`. |
| `StartingInvenory` | ref [Loot](Loot.md) | obsolete | | Misspelled old field. Migration 1.1 → 1.2 creates a loot record and writes its ID here. Migration 1.2 → 1.3 moves it to `StartingInventory` ([DatabaseUpgrader_2_3.cs:11](../../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_2_3.cs#L11)). |
| `SurvivalCombatRules` | ref [CombatRules](CombatRules.md) | | 3 | Rules of survival battles. |
| `StarbaseCombatRules` | ref [CombatRules](CombatRules.md) | | 4 | Rules of faction starbase battles. |
| `FlagshipCombatRules` | ref [CombatRules](CombatRules.md) | | 5 | Rules of boss and ruins battles. |
| `ArenaCombatRules` | ref [CombatRules](CombatRules.md) | | 6 | Rules of arena battles. |
| `ChallengeCombatRules` | ref [CombatRules](CombatRules.md) | | 7 | Rules of challenge battles. |
| `QuickCombatRules` | ref [CombatRules](CombatRules.md) | | 2 | Rules of quick combat from the main menu. |

Every `…CombatRules` field falls back to `CombatSettings.DefaultCombatRules` when it is `0`. See [CombatRules: Where rules come from](CombatRules.md#where-rules-come-from).

## Notes

### Enemy level

`EnemyLevel` turns a star level into a ship experience level. With the default formula, a level-100 star gives level-55 enemies, and the curve stops at `MaxEnemyShipsLevel` (300) from star level 509 on. The result is clamped to 0…500.

| Consumer | Argument | Code |
| --- | --- | --- |
| Random enemies, quest fleets, bosses | Fleet level ± min(10, level / 5), per ship | [ShipExtensions.cs:107](../../../Assets/Modules/ShipConstructor/Scripts/Extensions/ShipExtensions.cs#L107) |
| Exploration enemies | Planet level; the result is lowered by a random 0…1/3 | [EnemyShipBuilder.cs:39](../../../Assets/Scripts/Domain/Exploration/EnemyShipBuilder.cs#L39) |
| Exploration outposts and turrets | Planet level | [RandomOutpostBuilder.cs:32](../../../Assets/Scripts/Domain/Exploration/RandomOutpostBuilder.cs#L32), [RandomTurretBuilder.cs:32](../../../Assets/Scripts/Domain/Exploration/RandomTurretBuilder.cs#L32) |
| Asteroid strength in battle | `StarLevel` of the battle | [CombatManager.cs:63](../../../Assets/Scripts/Combat/Manager/CombatManager.cs#L63) |

The variable resolver gives the expression access to `MaxEnemyShipsLevel` ([GalaxySettings.cs:90](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/GalaxySettings.cs#L90)). Expression errors: see [Expressions](../../database.md#expressions).

### Data rules

1. Keep `StartingShipBuilds` non-empty and include a `Frigate`. Without it, the new game has no exploration ship.
2. Set `DefaultStarbaseBuild`. Otherwise a region whose faction has no fitting starbase build gets `null` as its starbase.
3. Keep `ShipMinSpawnDistance` growing with the size class.
4. Set `StartCondition` `Manual` on `CaptureStarbaseQuest`.
5. A mod settings record replaces the whole record. Copy every value, not only the changed one ([DB7](../../issues/database.md#db7-settings-records-cannot-be-partly-overridden)).
