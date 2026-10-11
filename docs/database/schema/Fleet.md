# Fleet

A fleet record describes the enemies of a quest battle: random ships from a faction filter, fixed ship builds, a level offset, and the battle rules. A quest `AttackFleet` node fights it, and a `ShowDialog` node can preview it. Only quests read fleet records.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `18` |
| Kind | collection |
| Folder | [`Resources/Database/Quests/Fleets/`](../../../Assets/Modules/Database/Resources/Database/Quests/Fleets/) |
| Records in the built-in catalog | 7. Fleets 1 and 4 use only fixed ships. No built-in fleet sets `CombatRules`. |
| Schema | [Fleet.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/Fleet.xml) |
| Generated class | `GameDatabase.DataModel.Fleet` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Fleet.cs)) |
| Runtime | [`FleetFactory`](../../../Assets/Modules/Quests/Scripts/Factory/FleetFactory.cs) (ship list), [`QuestCombatModelFacctory`](../../../Assets/Scripts/Domain/Quests/QuestCombatModelFacctory.cs) (battle), [`QuestFleet`](../../../Assets/Scripts/Legacy/Model/Military/QuestFleet.cs) (ships) |

The legacy factory `Model.Factories.Fleet` (random enemies, bosses, starbases) does not read this table.

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Factions` | struct [FactionFilter](structures.md#factionfilter) | `AllButList`, empty | Factions of the random ships. The filter uses the fleet rule at the enemy level, and the quest region's faction for `StarOwnersAndList`. The game tests the hull faction, not the build faction ([DB14](../../issues/database.md#db14-quest-fleets-filter-by-hull-faction)). |
| `LevelBonus` | int | −10,000…10,000 | Added to the quest level. Enemy level = max(0, quest level + `LevelBonus`) ([FleetFactory.cs:31](../../../Assets/Modules/Quests/Scripts/Factory/FleetFactory.cs#L31)). |
| `NoRandomShips` | bool | | The fleet has only `SpecificShips`. |
| `SpecificShips` | ref list [ShipBuild](ShipBuild.md) | | Builds that are always in the fleet, in addition to the random ships. The loader throws on a missing build. The faction filter and the level filters do not apply to them. |
| `CombatRules` | ref [CombatRules](CombatRules.md) | | Rules of the battle. `0`: `CombatSettings.DefaultCombatRules` ([QuestCombatModelFacctory.cs:37](../../../Assets/Scripts/Domain/Quests/QuestCombatModelFacctory.cs#L37)). |
| `CombatTimeLimit` | int | obsolete, 0…999 | Not read. No migration converts it. Use `CombatRules.TimeLimit`. |
| `LootCondition` | enum `RewardCondition` | obsolete | Not read. No migration converts it ([DB15.23](../../issues/database.md#db15-smaller-defects)). Use `CombatRules.LootCondition`. |
| `ExpCondition` | enum `RewardCondition` | obsolete | Not read. No migration converts it. Use `CombatRules.ExpCondition`. |
| `NoShipChanging` | bool | obsolete | Not read. Use `CombatRules.ShipSelection`. |
| `PlayerHasOneShip` | bool | obsolete | Not read. Use `CombatRules.ShipSelection`. |

## Notes

### Ship list

`FleetFactory.CreateFleet` builds the list once per quest ([FleetFactory.cs:56](../../../Assets/Modules/Quests/Scripts/Factory/FleetFactory.cs#L56)). The generator uses the quest seed.

1. Count: `0` with `NoRandomShips`, else `1 + min(3 + r, enemy level / 3)`, where `r = a × b / 2000` with `a`, `b` random in 0…99, so `r` is 0…4 ([FleetFactory.cs:74](../../../Assets/Modules/Quests/Scripts/Factory/FleetFactory.cs#L74)). The fleet has 1 to 8 random ships.
2. Candidates: enemy builds (`AvailableForEnemy`) with a `Common` hull of `Normal` rarity, whose difficulty class and hull size fit the enemy level ([ShipBuild: Difficulty class](ShipBuild.md#difficulty-class), `GalaxySettings.ShipMinSpawnDistance`), and whose hull faction passes `Factions`.
3. Exactly *count* random candidates; one build can appear several times ([EnumerableExtensions.cs:154](../../../Assets/Modules/CommonComponents/Scripts/Utils/EnumerableExtensions.cs#L154)). Then `SpecificShips`, then a shuffle.

When no candidate fits, the fleet has only `SpecificShips`. With `NoRandomShips` and an empty `SpecificShips`, the battle has no enemies.

### Levels

| Value | Source | Use |
| --- | --- | --- |
| Quest level | `Quest.Level`, else the quest star's level | `CombatRules` expressions (`StarLevel`), loot scaling |
| Enemy level | Quest level + `LevelBonus`, at least 0 | Candidate filter, fleet size |
| Ship level | `GalaxySettings.EnemyLevel(enemy level ± min(10, enemy level / 5))`, per ship | Ship experience ([ShipExtensions.cs:107](../../../Assets/Modules/ShipConstructor/Scripts/Extensions/ShipExtensions.cs#L107)) |
| AI level | 2 × enemy level | [BehaviorTree: AI level](BehaviorTree.md#ai-level) |

The fleet builds ships through `ShipExtensions.Create`, like random enemies. Random satellites and component upgrades follow [ShipBuild](ShipBuild.md).

### Battle

The `AttackFleet` node starts the battle with the fleet, the quest level as `StarLevel`, the rules, and the node's `Loot` as an extra reward. The result goes to `VictoryTransition` or `FailureTransition` ([Quest: Node types](Quest.md#node-types)). Where the rules come from for other battles: [CombatRules: Where rules come from](CombatRules.md#where-rules-come-from).

`Character.Fleet` refers to this table but is not read ([DB11](../../issues/database.md#db11-schema-fields-without-runtime-effect)).

### Data rules

1. Give every fleet at least one possible ship: random ships or `SpecificShips`.
2. Set battle rules through `CombatRules` (`TimeLimit`, `LootCondition`, `ExpCondition`, `ShipSelection`). The five obsolete fields do nothing.
3. Built-in fleets 1 and 10 still set `LootCondition` `Never` (fleet 1 also `ExpCondition` `Never`). To keep that behavior, give them a `CombatRules` record with these values ([DB15.23](../../issues/database.md#db15-smaller-defects)).
4. A `ListOnly` filter with a faction that has no `Common` builds at the enemy level gives no random ships.
