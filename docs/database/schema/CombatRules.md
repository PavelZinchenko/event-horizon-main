# CombatRules

A combat rules record sets the rules of one kind of battle: how many enemies fight at once, the round timer and what happens when it runs out, rewards, ship changes, skill bonuses, the battle area, and the music. [GalaxySettings](GalaxySettings.md), [SpecialEventSettings](SpecialEventSettings.md), and [CombatSettings](CombatSettings.md) select rules per battle type. A quest [Fleet](Fleet.md) can set its own. The combat model, the combat manager, the timer, and the reward code read combat rules.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `30` |
| Kind | collection |
| Folder | [`Resources/Database/Quests/CombatRules/`](../../../Assets/Modules/Database/Resources/Database/Quests/CombatRules/) |
| Records in the built-in catalog | 8, one per settings field: 1 `Default`, 2 `QuickCombat`, 3 `Survival`, 4 `FactionStarbase`, 5 `Flagship`, 6 `Arena`, 7 `Challenge`, 8 `Xmas`. No fleet sets rules. |
| Schema | [CombatRules.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/CombatRules.xml) |
| Generated class | `GameDatabase.DataModel.CombatRules` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/CombatRules.cs), [hand-written part](../../../Assets/Modules/Database/Scripts/DataModel/CombatRules.cs)) |
| Runtime wrapper | `CombatRulesAdapter`: the rules with the expressions evaluated for the battle's star level and the final ship selection mode ([CombatRules.cs:15](../../../Assets/Modules/Database/Scripts/DataModel/CombatRules.cs#L15)) |

## Fields

The record also has the [common fields](index.md#common-fields). The expressions take `level`: the star level of the battle (0 for quick combat, arena, and challenge battles).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `InitialEnemyShips` | expression `IntToInt(level)` | `1`, 1… | Enemy ships at the start. The first one arrives through the normal spawn; the others spawn when the battle starts ([CombatManager.cs:94](../../../Assets/Scripts/Combat/Manager/CombatManager.cs#L94)). |
| `MaxEnemyShips` | expression `IntToInt(level)` | `12`, 1… | Limit of active enemies for reinforcements (see [Timer](#timer)). It does not limit `InitialEnemyShips`. |
| `BattleMapSize` | int | 200, 50… | Width and height of the square battle area in world units ([CombatSceneInstaller.cs:45](../../../Assets/Scripts/Installers/CombatSceneInstaller.cs#L45)). |
| `TimeLimit` | expression `IntToInt(level)` | `MAX(40, 100 - level)`, 0… | Seconds per round. `0` means no timer and no reinforcements. |
| `TimeOutMode` | enum [TimeOutMode](enums.md#other-quest-enums) | `CallNextEnemy` | What happens when the round time runs out. See [Timer](#timer). |
| `LootCondition` | enum [RewardCondition](enums.md#other-quest-enums) | `Default` | When the player gets loot. `Default`: only on victory ([ICombatModel.cs:36](../../../Assets/Scripts/Combat/Domain/ICombatModel.cs#L36)). |
| `ExpCondition` | enum [RewardCondition](enums.md#other-quest-enums) | `Default` | When the player gets experience. |
| `ShipSelection` | enum [PlayerShipSelectionMode](enums.md#other-quest-enums) | `Default` | How the player changes ships. `Default` becomes `NoRetreats` unless the player can retreat (see [Retreat](#retreat)). |
| `DisableSkillBonuses` | bool | | The player fleet fights without skill bonuses ([CombatModelBuilder.cs:52](../../../Assets/Scripts/Combat/Domain/CombatModelBuilder.cs#L52)). It also allows retreat. |
| `DisableRandomLoot` | bool | | No random loot from destroyed ships. Special rewards of the battle stay ([CombatReward.cs:57](../../../Assets/Scripts/Combat/Domain/CombatReward.cs#L57)). |
| `DisableAsteroids` | bool | | No asteroids. Otherwise 10 asteroids spawn, with hit points scaled by the enemy level of the star ([CombatManager.cs:66](../../../Assets/Scripts/Combat/Manager/CombatManager.cs#L66)). |
| `DisablePlanet` | bool | | No background planet. |
| `NextEnemyButton` | bool | `true` | No effect. The button follows the reinforcement rule instead ([DB11](../../issues/database.md#db11-schema-fields-without-runtime-effect)). |
| `KillThemAllButton` | bool | | Shows a debug button that destroys all enemies ([CombatManager.cs:161](../../../Assets/Scripts/Combat/Manager/CombatManager.cs#L161)). |
| `CustomSoundtrack` | struct list [SoundTrack](structures.md#soundtrack) | | Combat music for this battle. Empty: `MusicPlaylist.CombatMusic` ([DatabaseMusicPlaylist.cs:32](../../../Assets/Scripts/GameServices/Audio/DatabaseMusicPlaylist.cs#L32)). |

## Notes

### Where rules come from

| Battle | Rules | Code |
| --- | --- | --- |
| Ordinary enemies at a star | `CombatSettings.DefaultCombatRules` | [Occupants.cs:109](../../../Assets/Scripts/Domain/Galaxy/StarContent/Occupants.cs#L109) |
| Boss, ruins | `GalaxySettings.FlagshipCombatRules`, else default | [Boss.cs:51](../../../Assets/Scripts/Domain/Galaxy/StarContent/Boss.cs#L51), [Ruins.cs:47](../../../Assets/Scripts/Domain/Galaxy/StarContent/Ruins.cs#L47) |
| Faction starbase | `GalaxySettings.StarbaseCombatRules`, else default. The level is the region's home star level. | [StarBase.cs:38](../../../Assets/Scripts/Domain/Galaxy/StarContent/StarBase.cs#L38) |
| Survival | `GalaxySettings.SurvivalCombatRules`, else default | [Survival.cs:48](../../../Assets/Scripts/Domain/Galaxy/StarContent/Survival.cs#L48) |
| Challenge | `GalaxySettings.ChallengeCombatRules`, else default | [Challenge.cs:90](../../../Assets/Scripts/Domain/Galaxy/StarContent/Challenge.cs#L90) |
| Arena | `GalaxySettings.ArenaCombatRules`, else default | [OfflineMultiplayer.cs:154](../../../Assets/Scripts/GameServices/OfflineMultiplayer/OfflineMultiplayer.cs#L154) |
| Quick combat | `GalaxySettings.QuickCombatRules`, else default | [QuickCombatState.cs:118](../../../Assets/Scripts/GameStateMachine/States/QuickCombatState.cs#L118) |
| Christmas tree | `SpecialEventSettings.XmasCombatRules`, no fallback ([DB15.21](../../issues/database.md#db15-smaller-defects)) | [XmasTree.cs:48](../../../Assets/Scripts/Domain/Galaxy/StarContent/XmasTree.cs#L48) |
| Quest battle | `Fleet.CombatRules`, else default | [QuestCombatModelFacctory.cs:37](../../../Assets/Scripts/Domain/Quests/QuestCombatModelFacctory.cs#L37) |

### Enemy spawning

1. When no enemy is active, the next ship of the enemy fleet spawns after 3 s. When no ship is left, the battle ends ([CombatManager.cs:227](../../../Assets/Scripts/Combat/Manager/CombatManager.cs#L227)).
2. At the start, `InitialEnemyShips − 1` more ships spawn at once.
3. Reinforcements (the "next enemy" button and the timer) are possible only when `TimeOutMode` is `CallNextEnemy` or `CallNextEnemyOrDraw`, `TimeLimit` is above 0, fewer than `MaxEnemyShips` enemies are active, and the fleet has ships left ([CombatManager.cs:195](../../../Assets/Scripts/Combat/Manager/CombatManager.cs#L195)).

### Timer

[`CombatTimer`](../../../Assets/Scripts/Combat/Manager/CombatTimer.cs) counts the round time. It restarts when an enemy ship spawns and when an enemy ship dies. In `DrainPlayerHp` mode it also restarts when the player's ship dies. In `CallNextEnemyOrDraw` mode an enemy death does not restart it once the fleet has no ships left in reserve.

| `TimeOutMode` | When the time runs out | Code |
| --- | --- | --- |
| `CallNextEnemy` | The next enemy joins and the timer restarts. Without possible reinforcements the timer shows 0 and nothing happens. | [CombatTimer.cs:109](../../../Assets/Scripts/Combat/Manager/CombatTimer.cs#L109) |
| `CallNextEnemyOrDraw` | Like `CallNextEnemy` while the fleet has ships left. Then the limit grows by 2 × `TimeLimit`, the alarm sounds in the last 15 s, and the battle ends at 0 as a draw: an enemy is still alive, so it is not a victory. | [CombatTimer.cs:124](../../../Assets/Scripts/Combat/Manager/CombatTimer.cs#L124) |
| `DrainPlayerHp` | The player's ship loses armor and shield every second: *n* % of the maximum per second, where *n* is the number of seconds over the limit. | [CombatTimer.cs:147](../../../Assets/Scripts/Combat/Manager/CombatTimer.cs#L147) |

### Retreat

`CombatRulesAdapter` turns `ShipSelection` `Default` into `NoRetreats` unless the player has the rescue-unit skill or the rules set `DisableSkillBonuses` ([CombatRules.cs:23](../../../Assets/Modules/Database/Scripts/DataModel/CombatRules.cs#L23)). Only `Default` lets the player change ships during a fight ([CombatManager.cs:155](../../../Assets/Scripts/Combat/Manager/CombatManager.cs#L155)). `Default`, `NoRetreats`, and `OnlyOneShip` open the ship choice after a ship dies; `ByOrder` sends the next ship automatically. `OnlyOneShip` ends the battle when the first ship dies.

### Data rules

1. Give `TimeLimit` a value above 0 when the battle should call reinforcements.
2. Use `DrainPlayerHp` or `CallNextEnemyOrDraw` with a constant `TimeLimit` for duels; the default formula depends on the star level.
3. Do not rely on `NextEnemyButton`; it has no effect.
4. Set the settings fields that select rules. `XmasCombatRules` must not be `0`.
5. A bad expression becomes 0 without a load error ([DB6](../../issues/database.md#db6-expression-errors-appear-late-and-become-zero)). The minimum of 1 then applies to the ship counts, and a `TimeLimit` of 0 disables the timer.
