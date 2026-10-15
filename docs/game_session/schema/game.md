# Game Domain

This page describes the save root and the four small objects that hold game-wide state: `SaveGameData`, `GameData`, `Statistics`, `Achievements`, and `CommonObjectData`. For the XML format and the type rules, see [Save schema](index.md). For the module overview, see [GameSession module](../../game_session.md).

## Contents

- [SaveGameData](#savegamedata)
- [GameData](#gamedata)
- [Statistics](#statistics)
- [Achievements](#achievements)
- [CommonObjectData](#commonobjectdata)

## SaveGameData

The root object of the save. It owns one instance of each of the 20 section objects and has no fields of its own. The section order in the schema is the byte order in the save; see [Root members (v3)](index.md#root-members-v3).

| | |
| --- | --- |
| Schema | [`v3/SaveGameData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/SaveGameData.xml) |
| Generated class | [`Model.SaveGameData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/SaveGameData.cs#L13) |
| Hand-written partial | — |
| Facade | None. `SessionData` owns the root and creates the facades from it ([SessionData.cs:151](../../../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L151)). |

`VersionMajor` = 3 and `VersionMinor` = 0 on the root class are the schema version that `SessionSerializer` writes into the envelope ([SaveGameData.cs:38](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/SaveGameData.cs#L38)). Every child object reports changes to the root, and the root reports them to `SessionData`. See [Change tracking and DataVersion](../../game_session.md#change-tracking-and-dataversion).

## GameData

Seed, start time, play time, and two small counters of the current playthrough.

| | |
| --- | --- |
| Schema | [`v3/Objects/GameData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/GameData.xml) |
| Generated class | [`Model.GameData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/GameData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IGameData` / `GameData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/GameData.cs), property `ISessionData.Game` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `GameId` | `string` | `""` | Unused. No code reads or writes it, so it is always empty. The storage game ID is `SessionData.GameId` (`long`), which is not part of the model. |
| `Seed` | `int` | `timestamp`: `(int)UtcNow.Ticks` ([GameData.cs:34](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/GameData.cs#L34)) | Seed of the galaxy and of all seeded content. Star positions, levels, regions, quests, loot, and the random table depend on it. |
| `StartTime` | `long` | 0 | UTC ticks when the player started this game. 0 means "not started". |
| `TotalPlayTime` | `long` | 0, `notimportant` | Active play time in ticks. Counted outside the module by `GameTime`. |
| `SupplyShipStartTime` | `long` | 0 | UTC ticks when the supply ship was started. |
| `Counter` | `int` | 0 | A counter that grows on every read through the facade. Gives a new value for each random roll that must differ between calls. |

### Facade API

| Member | Behavior |
| --- | --- |
| `Seed` | Read only. |
| `Regenerate()` | Sets `Seed` to `(int)DateTime.Now.Ticks` (local time, not UTC) ([GameData.cs:24](../../../Assets/Modules/GameSession/Scripts/Facade/Content/GameData.cs#L24)). Only the debug cheats call it ([Cheats.cs:211](../../../Assets/Scripts/Legacy/Debug/Cheats.cs#L211)). Services that cached the seed keep the old value until the next load. |
| `Counter` | Returns the stored value and increments it ([GameData.cs:23](../../../Assets/Modules/GameSession/Scripts/Facade/Content/GameData.cs#L23)). Every read marks the session dirty. See [GS5.3](../../issues/game_session.md#gs5-smaller-defects). |
| `GameStartTime` | Get and set `StartTime`. [`ISessionData.IsGameStarted()`](../../../Assets/Modules/GameSession/Scripts/Facade/ISessionData.cs#L35) is `GameStartTime > 0`. |
| `TotalPlayTime` | Get and set. |
| `SupplyShipStartTime`, `StartSupplyShip()` | Read the value; set it to `UtcNow.Ticks`. No code outside the module calls either member. |

### Signals

None.

### Consumers

| Field | Consumer | Use |
| --- | --- | --- |
| `Seed` | [`RandomGenerator`](../../../Assets/Scripts/GameServices/Random/RandomGenerator.cs#L46) | Builds the shuffled random table once per load. |
| `Seed` | [`StarData`](../../../Assets/Scripts/Domain/Galaxy/StarData.cs#L127), [`Occupants`](../../../Assets/Scripts/Domain/Galaxy/StarContent/Occupants.cs#L90), [`Region`](../../../Assets/Scripts/Legacy/Game/Galaxy/Region.cs#L98), [`StarBorder`](../../../Assets/Scripts/Legacy/Map/StarBorder.cs#L50) | Star content, star levels, region home star level and faction, map borders. |
| `Seed` | [`QuestDataProvider`](../../../Assets/Scripts/Domain/Quests/QuestDataProvider.cs#L47), [`StarMapDataProvider`](../../../Assets/Scripts/Domain/Quests/StarMapDataProvider.cs#L40), [`GameDataProvider`](../../../Assets/Scripts/Domain/Quests/GameDataProvider.cs#L19) | Quest seeds and star data for the Quests module. |
| `Seed` | [`PlayerInventory`](../../../Assets/Scripts/Domain/Player/PlayerInventory.cs#L40) | Starting inventory roll. |
| `Counter` | [`GameDataProvider.Counter`](../../../Assets/Scripts/Domain/Quests/GameDataProvider.cs#L20), [`CraftPanel`](../../../Assets/Scripts/Gui/Craft/CraftPanel.cs#L77) | Varies quest and crafting seeds between calls. |
| `StartTime` | [`StartingNewGameState`](../../../Assets/Scripts/GameStateMachine/States/StartingNewGameState.cs#L47) | Sets it to `UtcNow.Ticks` when a new game starts. |
| `StartTime` | [`MainMenuState`](../../../Assets/Scripts/GameStateMachine/States/MainMenuState.cs#L79), [`DailyReward`](../../../Assets/Scripts/Domain/Player/DailyReward.cs#L39), [`GameDataManager`](../../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L157) | "Game started" checks; the first daily reward waits one day after the start. |
| `TotalPlayTime` | [`GameTime`](../../../Assets/Scripts/Services/InternetTime/GameTime.cs#L29) | See below. |

### Notes

**Play time.** [`GameTime`](../../../Assets/Scripts/Services/InternetTime/GameTime.cs) keeps its own counter. It reads `TotalPlayTime` on `SessionDataLoadedSignal` ([GameTime.cs:51](../../../Assets/Scripts/Services/InternetTime/GameTime.cs#L51)) and writes it back on `SessionAboutToSaveSignal` ([GameTime.cs:40](../../../Assets/Scripts/Services/InternetTime/GameTime.cs#L40)). It counts `DateTime.Now` ticks only while the game is started and the current state is not a menu, dialog, editor, or skill tree ([GameTime.cs:57](../../../Assets/Scripts/Services/InternetTime/GameTime.cs#L57)). A gap of one minute or more between two frames is not counted ([GameTime.cs:34](../../../Assets/Scripts/Services/InternetTime/GameTime.cs#L34)). If the stored value is greater than the live counter, the write-back logs an exception and skips. The Quests module compares quest times with this value; see [Quests domain](quests.md).

**Two play-time values.** `GameData.TotalPlayTime` is in the save and counts active play only. `SessionData.TimePlayed` is storage metadata and counts every tick while the game is started ([SessionData.cs:77](../../../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L77)). They are not related.

**Legacy conversion.** The headerless loader copies `Counter`, `Seed`, `StartTime`, `SupplyShipStartTime`, and `TotalPlayTime` ([LegacyDataLoader.cs:63](../../../Assets/Modules/GameSession/Scripts/Facade/LegacyDataLoader.cs#L63)).

## Statistics

Ship hulls the player has owned, and a defeated-enemies counter.

| | |
| --- | --- |
| Schema | [`v3/Objects/Statistics.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/Statistics.xml) |
| Generated class | [`Model.Statistics`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/Statistics.cs#L13) |
| Hand-written partial | — |
| Facade | [`IStatisticsData` / `StatisticsData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/StatisticsData.cs), property `ISessionData.Statistics` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `UnlockedShips` | `set<int>` | empty | `Ship` record IDs (hulls) that were ever added to the player's fleet. Entries are never removed. |
| `DefeatedEnemies` | `int` | 0 | Counter of defeated enemies. No live code changes it; only the legacy loader copies it ([LegacyDataLoader.cs:172](../../../Assets/Modules/GameSession/Scripts/Facade/LegacyDataLoader.cs#L172)). |

### Facade API

| Member | Behavior |
| --- | --- |
| `UnlockShip(ItemId<Ship>)` | Adds the ID. Adding an ID that is already in the set does not mark the session dirty. |
| `UnlockedShips` | The IDs as `ItemId<Ship>`. |
| `DefeatedEnemies` | Get and set. |

### Consumers

| Consumer | Use |
| --- | --- |
| [`PlayerFleet.OnShipAdded`](../../../Assets/Scripts/Domain/Player/PlayerFleet.cs#L72) | Unlocks the hull of every ship added to the fleet, including ships created on load. |
| [`QuickCombatState`](../../../Assets/Scripts/GameStateMachine/States/QuickCombatState.cs#L136) | Adds one player build per unlocked hull to the quick-combat ship pool. |

## Achievements

Unlocked achievement IDs.

| | |
| --- | --- |
| Schema | [`v3/Objects/Achievements.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/Achievements.xml) |
| Generated class | [`Model.Achievements`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/Achievements.cs#L13) |
| Hand-written partial | — |
| Facade | [`IAchievementData` / `AchievementData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/AchievementData.cs), property `ISessionData.Achievements` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Gained` | `set<int>` | empty | Achievement type values. |

### Facade API

`IsUnlocked(int type)` and `UnlockAchievement(int type)`.

### Consumers

None. No code outside the module uses `ISessionData.Achievements`. The only writer is the legacy loader ([LegacyDataLoader.cs:57](../../../Assets/Modules/GameSession/Scripts/Facade/LegacyDataLoader.cs#L57)). Platform achievements (Google Play, Game Center) are not stored here.

## CommonObjectData

Generic per-star values for star content that needs one number of state.

| | |
| --- | --- |
| Schema | [`v3/Objects/CommonObjectData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/CommonObjectData.xml) |
| Generated class | [`Model.CommonObjectData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/CommonObjectData.cs#L13) |
| Hand-written partial | — |
| Facade | [`ICommonObjectData` / `CommonObjectData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/CommonObjectData.cs), property `ISessionData.CommonObjects` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `IntValues` | `map<int,int>` | empty | Star ID → integer value. |
| `LongValues` | `map<int,long>` | empty | Star ID → time value. The facade calls it "use time". |

### Facade API

| Member | Behavior |
| --- | --- |
| `GetIntValue(id)`, `SetIntValue(id, value)` | Read with default 0; write. |
| `GetUseTime(id)`, `SetUseTime(id, time)` | Read with default 0; write. |

The keys are star IDs in both current users. The facade does not check this, so two content types on the same star would share a value.

### Consumers

| Map | Consumer | Value |
| --- | --- | --- |
| `IntValues` | [`Challenge`](../../../Assets/Scripts/Domain/Galaxy/StarContent/Challenge.cs#L37) | Challenge level reached on the star, 0 to 5. Incremented after a victory ([Challenge.cs:116](../../../Assets/Scripts/Domain/Galaxy/StarContent/Challenge.cs#L116)). |
| `LongValues` | [`Survival`](../../../Assets/Scripts/Domain/Galaxy/StarContent/Survival.cs#L23) | UTC ticks of the last attack. The next attack is allowed 4 hours later ([Survival.cs:28](../../../Assets/Scripts/Domain/Galaxy/StarContent/Survival.cs#L28)). |
