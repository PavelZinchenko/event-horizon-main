# Quests Domain

This page describes the saved quest state: the object `QuestData` and the structs `StarQuestMap`, `QuestProgress`, and `QuestStatistics`. The quest logic itself is in the [Quests module](../../quests.md). For the XML format and the type rules, see [Save schema](index.md). For the module overview, see [GameSession module](../../game_session.md).

## Contents

- [Overview](#overview)
- [QuestData](#questdata)
- [StarQuestMap](#starquestmap)
- [QuestProgress](#questprogress)
- [QuestStatistics](#queststatistics)
- [Quest time](#quest-time)

## Overview

```text
QuestData
├── Progress: map<questId, StarQuestMap>         active quest instances
│     └── StarQuestsMap: map<starId, QuestProgress(Seed, ActiveNode, StartTime)>
├── Statistics: map<questId, QuestStatistics>    history per quest
├── FactionRelations: map<homeStarId, int>       reputation per region
└── CharacterRelations: map<characterId, int>    attitude per character
```

One quest can run at several stars at the same time. An instance is the pair (quest ID, star ID). The save stores only the seed and the current node of each instance. The Quests module rebuilds the quest graph from the database and the seed on load.

## QuestData

| | |
| --- | --- |
| Schema | [`v3/Objects/QuestData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/QuestData.xml) |
| Generated class | [`Model.QuestData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/QuestData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IQuestData` / `QuestData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/QuestData.cs), property `ISessionData.Quests` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `FactionRelations` | `map<int,int>` | empty | Home star ID of a region → player reputation with that region's faction, −100 to 100. The key is a star ID, not a faction ID. |
| `CharacterRelations` | `map<int,int>` | empty | `Character` record ID → attitude, −100 to 100. |
| `Statistics` | `map<int,QuestStatistics>` | empty | `QuestModel` record ID → counters and times. |
| `Progress` | `map<int,StarQuestMap>` | empty | `QuestModel` record ID → active instances by star. |

### Facade API

| Member | Behavior |
| --- | --- |
| `HasBeenStarted(questId)` | True when the quest has statistics or an active instance. |
| `HasBeenCompleted(questId)` | `CompletionCount > 0`. |
| `IsQuestActive(questId)`, `IsQuestActive(questId, starId)` | An instance exists (at that star). |
| `IsActiveOrCompleted(questId)` | Active, or completed at least once. |
| `LastStartTime`, `LastCompletionTime`, `GetQuestStatistics` | Read `Statistics`; 0 or an empty struct when missing. |
| `QuestStartTime(questId, starId)` | `StartTime` of the instance, or 0. |
| `TotalQuestCount()` | Sum of all completions, failures, and active instances ([QuestData.cs:68](../../../Assets/Modules/GameSession/Scripts/Facade/Content/QuestData.cs#L68)). |
| `GetActiveQuests()`, `GetQuestProgress(questId)` | Enumerate instances as `QuestInformation(questId, starId, seed, activeNode)`. |
| `SetQuestProgress(questId, starId, seed, activeNode, currentTime)` | See [Instance lifecycle](#instance-lifecycle). |
| `CancelQuest(questId, starId)` | Removes the instance. Removes the quest's `StarQuestMap` when it becomes empty. Statistics do not change. |
| `SetQuestCompleted(questId, starId, success, completionTime)` | Increments `CompletionCount` or `FailureCount`, raises `LastCompletionTime` to `completionTime`, then cancels the instance ([QuestData.cs:135](../../../Assets/Modules/GameSession/Scripts/Facade/Content/QuestData.cs#L135)). |
| `Get/SetFactionRelations(starId, …)`, `Get/SetCharacterRelations(characterId, …)` | Read with default 0; write clamped to −100…100. |
| `Reset()` | Clears all four maps. Only the debug cheats call it ([Cheats.cs:160](../../../Assets/Scripts/Legacy/Debug/Cheats.cs#L160)). |

### Instance lifecycle

`SetQuestProgress` ([QuestData.cs:102](../../../Assets/Modules/GameSession/Scripts/Facade/Content/QuestData.cs#L102)):

1. Create the quest's `StarQuestMap` if it is missing.
2. If the instance exists with the same seed and node, do nothing.
3. If the instance exists with another seed or node, keep its `StartTime`.
4. If the instance is new, use `currentTime` as `StartTime`. Also write `LastStartTime = currentTime` into the statistics, and create the statistics entry if needed.
5. Store `QuestProgress(seed, activeNode, startTime)`.

So a quest that was started once has a statistics entry even when it was never finished.

### Signals

None. The Quests module raises its own events.

### Consumers

| Consumer | Use |
| --- | --- |
| [`QuestDataProvider`](../../../Assets/Scripts/Domain/Quests/QuestDataProvider.cs) | Implements the Quests module's `IQuestDataStorage` over the facade. Passes `GameTime.TotalPlayTime` as the time ([line 32](../../../Assets/Scripts/Domain/Quests/QuestDataProvider.cs#L32)). Generates deterministic seeds from `GameData.Seed`, the quest ID, the star ID, and the number of starts so far ([line 40](../../../Assets/Scripts/Domain/Quests/QuestDataProvider.cs#L40)). |
| [`CharacterDataProvider`](../../../Assets/Scripts/Domain/Quests/CharacterDataProvider.cs#L16) | Character attitude for quest requirements. |
| [`StarMapState`](../../../Assets/Scripts/GameStateMachine/States/StarMapState.cs#L247) | Quest action processor: sets or adds faction and character relations. |
| [`Region`](../../../Assets/Scripts/Legacy/Game/Galaxy/Region.cs#L49), [`FactionPanelViewModel`](../../../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/FactionPanelViewModel.cs#L79) | Read the reputation by the region's home star. |
| [`StarBase`](../../../Assets/Scripts/Domain/Galaxy/StarContent/StarBase.cs#L68) | Sets the reputation to −100 when the player attacks a starbase. |
| Quests module ([`NodeBuilder`](../../../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L201)) | Converts the current star to its region's home star before it changes faction relations. |

## StarQuestMap

| | |
| --- | --- |
| Schema | [`v3/Types/StarQuestMap.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Types/StarQuestMap.xml) |
| Generated struct | [`Model.StarQuestMap`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/StarQuestMap.cs#L14) |
| Hand-written partial | — |

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `StarQuestsMap` | `map<int,QuestProgress>` | — | Star ID → instance of the quest at that star. |

A wrapper struct, because a map cannot hold a map directly.

## QuestProgress

| | |
| --- | --- |
| Schema | [`v3/Types/QuestProgress.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Types/QuestProgress.xml) |
| Generated struct | [`Model.QuestProgress`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/QuestProgress.cs#L14) |
| Hand-written partial | [`Model/QuestProgress.cs`](../../../Assets/Modules/GameSession/Scripts/Model/QuestProgress.cs) (field constructor) |

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Seed` | `int` | — | Seed of the instance. Random choices inside the quest depend on it. |
| `ActiveNode` | `int` | — | ID of the current node in the quest's node graph (database `QuestModel.Nodes`). |
| `StartTime` | `long` | — | Play time in ticks when the instance started. See [Quest time](#quest-time). |

If a mod changes the node graph, `ActiveNode` can point to a node that no longer exists. The Quests module decides what happens then.

## QuestStatistics

| | |
| --- | --- |
| Schema | [`v3/Types/QuestStatistics.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Types/QuestStatistics.xml) |
| Generated struct | [`Model.QuestStatistics`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/QuestStatistics.cs#L14) |
| Hand-written partial | [`Model/QuestStatistics.cs`](../../../Assets/Modules/GameSession/Scripts/Model/QuestStatistics.cs) (field constructor) |

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `CompletionCount` | `int` | — | Successful completions. |
| `FailureCount` | `int` | — | Failures. Cancellations are not counted. |
| `LastStartTime` | `long` | — | Play time in ticks when the last new instance started. |
| `LastCompletionTime` | `long` | — | Play time in ticks of the last completion **or failure**. |

`LastCompletionTime` grows on failure too, so a "time since last completion" cooldown also restarts after a failure. The headerless loader fills it with the completion count instead ([GS4](../../issues/game_session.md#gs4-legacy-quest-statistics-get-a-wrong-completion-time)).

## Quest time

All quest times are **active play time in ticks**, taken from [`GameTime.TotalPlayTime`](game.md#gamedata). They are not UTC ticks and not hours since the game start. Menus, dialogs, and editors do not advance this clock, and neither does time when the game is closed.

| Reader | Compares |
| --- | --- |
| [`TimeSinceLastCompletion`](../../../Assets/Modules/Quests/Scripts/Requirements/TimeSinceLastCompletion.cs#L47) | `TotalPlayTime − LastCompletionTime` |
| [`TimeSinceQuestStart`](../../../Assets/Modules/Quests/Scripts/Requirements/TimeSinceQuestStart.cs#L29) | `TotalPlayTime − QuestStartTime(quest, star)` |
| [`IsOnCooldown`](../../../Assets/Modules/Quests/Scripts/IQuest.cs#L30) | Daily quests: `currentTime − LastStartTime` against a cooldown in minutes. |

`GameTime` writes its counter to `GameData.TotalPlayTime` only on `SessionAboutToSaveSignal`. The quest times use the live counter. A save path that skips this signal can therefore store quest times that are later than the saved play time. After that save is loaded, the affected cooldowns last longer by the difference. See [Save lifecycle](../../game_session.md#save-lifecycle) for the save paths.
