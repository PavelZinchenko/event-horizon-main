# CombatSettings

Combat settings select the default [BehaviorTree](BehaviorTree.md) for each AI role and the default [CombatRules](CombatRules.md). A ship build's `CustomAI` and a battle's own rules override them. The controller factory and the combat model builder read combat settings.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `109` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/Combat.json`](../../../Assets/Modules/Database/Resources/Database/Settings/Combat.json) |
| Schema | [CombatSettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/CombatSettings.xml) |
| Generated class | `GameDatabase.DataModel.CombatSettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/CombatSettings.cs)), read as `IDatabase.CombatSettings` |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The *Built-in* column shows the value in the built-in catalog. Code: [ControllerFactory.cs:37](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ControllerFactory.cs#L37).

| Field | Type | Default / range | Built-in | Meaning |
| --- | --- | --- | --- | --- |
| `EnemyAI` | ref [BehaviorTree](BehaviorTree.md) | | 1 (`Ship`) | AI of enemy ships without `CustomAI`. `0`: the old hard-coded AI with the fleet's AI level. |
| `AutopilotAI` | ref [BehaviorTree](BehaviorTree.md) | | 7 (`Autopilot`) | AI of the player's ship on autopilot. `0`: the old hard-coded AI at level 100. |
| `CloneAI` | ref [BehaviorTree](BehaviorTree.md) | | 4 (`Clone`) | AI of clones from cloning devices without `CustomAI`. `0`: the old clone AI. |
| `DefensiveDroneAI` | ref [BehaviorTree](BehaviorTree.md) | | 3 (`DefensiveDrone`) | AI of drones in defensive mode, when neither the bay nor the drone build has a tree. Not used for bays with `ImprovedAi`. |
| `OffensiveDroneAI` | ref [BehaviorTree](BehaviorTree.md) | | 2 (`Drone`) | Same for drones in aggressive mode. |
| `StarbaseAI` | ref [BehaviorTree](BehaviorTree.md) | | 10 (`Starbase`) | AI of starbases and turrets without `CustomAI`. `0`: the old starbase AI. |
| `DefaultCombatRules` | ref [CombatRules](CombatRules.md) | | 1 (`Default`) | Rules of ordinary battles, and the fallback for battles whose specific rules are `0`. Must not be `0` (data rule 1). |

## Notes

### AI selection

| Ship | Order | Code |
| --- | --- | --- |
| Enemy | build `CustomAI`, `EnemyAI`, old AI. An AI level below 0 gives no AI (test only). | [ControllerFactory.cs:37](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ControllerFactory.cs#L37) |
| Clone | build `CustomAI`, `CloneAI`, old AI | [ControllerFactory.cs:49](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ControllerFactory.cs#L49) |
| Starbase, turret | build `CustomAI`, `StarbaseAI`, old AI | [ControllerFactory.cs:58](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ControllerFactory.cs#L58) |
| Drone | bay tree, drone build `CustomAI`, then (without `ImprovedAi`) `OffensiveDroneAI` or `DefensiveDroneAI`, old AI | [DroneBay: Drone AI](DroneBay.md#drone-ai) |
| Player autopilot | `AutopilotAI`, old AI | [ControllerFactory.cs:80](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ControllerFactory.cs#L80) |

The AI level of each role is on [BehaviorTree: AI level](BehaviorTree.md#ai-level).

### Rules selection

The combat model builder starts with `DefaultCombatRules` ([CombatModelBuilder.cs:29](../../../Assets/Scripts/Combat/Domain/CombatModelBuilder.cs#L29)). Each battle type then sets its own rules, mostly with this record as the fallback. See [CombatRules: Where rules come from](CombatRules.md#where-rules-come-from).

Migration 1.4 → 1.5 creates `CombatRules` 1 with default values and sets `DefaultCombatRules` to 1 ([DatabaseUpgrader_4_5.cs](../../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_4_5.cs)).

### Data rules

1. Always set `DefaultCombatRules`. With `0`, every ordinary battle throws `NullReferenceException` when the combat model is built ([CombatModelBuilder.cs:52](../../../Assets/Scripts/Combat/Domain/CombatModelBuilder.cs#L52)).
2. Set all six AI trees. A `0` falls back to old hard-coded AI that ignores the tree features.
3. A mod settings record replaces the whole record. Copy every value, not only the changed one ([DB7](../../issues/database.md#db7-settings-records-cannot-be-partly-overridden)).
