# Social Domain

This page describes the saved arena (PvP) timers and the daily reward streak: the objects `PvpData` and `SocialData`. For the XML format and the type rules, see [Save schema](index.md). For the module overview, see [GameSession module](../../game_session.md).

## Contents

- [PvpData](#pvpdata)
- [SocialData](#socialdata)

## PvpData

Arena battle limit and its recovery timer.

| | |
| --- | --- |
| Schema | [`v3/Objects/PvpData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/PvpData.xml) |
| Generated class | [`Model.PvpData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/PvpData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IPvpData` / `PvpData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/PvpData.cs), property `ISessionData.Pvp` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `ArenaFightsFromTimerStart` | `int` | 0 | Arena battles fought since the recovery timer started. |
| `ArenaLastFightTime` | `long` | 0 | UTC ticks of the last arena fight, as reported by the server. 0 = no server data yet. |
| `ArenaTimerStartTime` | `long` | 0 | Arena time in ticks when the recovery timer started. 0 = the timer is not running. |

### Facade API

Three get/set properties with shorter names: `FightsFromTimerStart`, `LastFightTime`, `TimerStartTime`. The facade adds no rules.

### Consumers

[`ArenaTimeManager`](../../../Assets/Scripts/GameServices/OfflineMultiplayer/ArenaTimeManager.cs) holds the rules. The player has at most 10 arena battles, and one battle comes back every 10 minutes ([ArenaTimeManager.cs:131](../../../Assets/Scripts/GameServices/OfflineMultiplayer/ArenaTimeManager.cs#L131)).

| Rule | Code |
| --- | --- |
| **Arena time.** `CurrentTime` is the device's UTC time until the server reports the time since the last fight. After that it is `LastFightTime + timeFromLastFight + time since the report`. So the timer follows the server and not a changed device clock. | [ArenaTimeManager.cs:14](../../../Assets/Scripts/GameServices/OfflineMultiplayer/ArenaTimeManager.cs#L14) |
| **Server report.** Stores `LastFightTime = now − timeFromLastFight`, then resets the timer if all battles have recovered. | [ArenaTimeManager.cs:30](../../../Assets/Scripts/GameServices/OfflineMultiplayer/ArenaTimeManager.cs#L30) |
| **Available battles.** 10 − max(0, fights − recovered), where recovered = elapsed time since `TimerStartTime` / 10 minutes. 10 when the timer is not running. | [ArenaTimeManager.cs:51](../../../Assets/Scripts/GameServices/OfflineMultiplayer/ArenaTimeManager.cs#L51) |
| **Fight.** Starts the timer at `CurrentTime` if it is not running, then increments the fight count. | [ArenaTimeManager.cs:97](../../../Assets/Scripts/GameServices/OfflineMultiplayer/ArenaTimeManager.cs#L97) |
| **Refill.** Subtracts fights; at 0 fights the timer stops. | [ArenaTimeManager.cs:105](../../../Assets/Scripts/GameServices/OfflineMultiplayer/ArenaTimeManager.cs#L105) |

[`OfflineMultiplayer`](../../../Assets/Scripts/GameServices/OfflineMultiplayer/OfflineMultiplayer.cs#L132) calls the fight and refill operations; [`PlayerInfo`](../../../Assets/Scripts/GameServices/OfflineMultiplayer/PlayerInfo.cs#L105) passes the server report.

## SocialData

The daily reward streak.

| | |
| --- | --- |
| Schema | [`v3/Objects/SocialData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/SocialData.xml) |
| Generated class | [`Model.SocialData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/SocialData.cs#L13) |
| Hand-written partial | — |
| Facade | [`ISocialData` / `SocialData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/SocialData.cs), property `ISessionData.Social` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `FirstDailyRewardDate` | `int` | 0 | First day of the current streak of daily rewards. 0 = no streak. |
| `LastDailyRewardDate` | `int` | 0 | Day of the last collected daily reward. |

A **day** is `DateTime.Ticks / TicksPerDay` of the internet time: whole days since 1 January of year 1 ([DailyReward.cs:109](../../../Assets/Scripts/Domain/Player/DailyReward.cs#L109)). It is not related to `GameData.StartTime`.

### Facade API

Two get/set properties. The facade adds no rules.

### Consumers

[`DailyReward`](../../../Assets/Scripts/Domain/Player/DailyReward.cs) holds the rules:

1. **Available** ([DailyReward.cs:37](../../../Assets/Scripts/Domain/Player/DailyReward.cs#L37)). The game started at least one day ago (real time), the internet time is known, and today is later than the last reward day.
2. **Last day.** The last reward day is the maximum of `LastDailyRewardDate` and `GameSettings.LastDailyRewardDate`. The second value is device-wide, outside the save, so a new game does not give a second reward on the same day.
3. **Collect** ([DailyReward.cs:85](../../../Assets/Scripts/Domain/Player/DailyReward.cs#L85)). Stores today in both places. If the last reward was yesterday and a streak exists, the reward size is the streak length in days. Otherwise the streak restarts today with size 1. Collect does not check availability itself; the caller must check it first ([GS5.27](../../issues/game_session.md#gs5-smaller-defects)).
4. **Clock moved back** ([DailyReward.cs:57](../../../Assets/Scripts/Domain/Player/DailyReward.cs#L57)). If the stored last day is in the future, it is set to today and the streak is cleared.
5. **Reward level.** The loot level comes from [`StarMapData.FurthestVisitedStar`](world.md#starmapdata) ([DailyReward.cs:52](../../../Assets/Scripts/Domain/Player/DailyReward.cs#L52)).
