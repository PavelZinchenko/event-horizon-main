# Quests Module

The Quests module runs quests. A quest is a graph of nodes defined in the database. The module picks a quest when a game event occurs, builds a runtime graph for it, moves the graph forward on events and player choices, and asks the application to apply side effects. The module owns no UI, no scene, and no save file. The application supplies these through context interfaces.

| | |
| --- | --- |
| Assembly | `Quests` ([Quests.asmdef](../Assets/Modules/Quests/Quests.asmdef)). References: Zenject, CommonComponents, Database, Localization, Diagnostics, Utilites. |
| Namespace | `Domain.Quests`. `ActionNode` is in the global namespace by mistake ([Q10.2](issues/quests.md#q10-smaller-defects)). |
| Source | [`Assets/Modules/Quests/Scripts/`](../Assets/Modules/Quests/Scripts/) |
| Application adapters | [`Assets/Scripts/Domain/Quests/`](../Assets/Scripts/Domain/Quests/), [`StarMapState`](../Assets/Scripts/GameStateMachine/States/StarMapState.cs) |
| Data | [Quest](database/schema/Quest.md), [Requirement](database/schema/structures.md#requirement), [Loot](database/schema/Loot.md), [Fleet](database/schema/Fleet.md), [Character](database/schema/Character.md), [QuestItem](database/schema/QuestItem.md), [CombatRules](database/schema/CombatRules.md), [enums](database/schema/enums.md#questtype) |
| Tests | None. |
| Known issues | [Quests issues](issues/quests.md). Data-level defects are in [Database issues](issues/database.md) (DB8, DB14, DB15.22–DB15.26). |

This document describes the runtime. The [Quest schema page](database/schema/Quest.md) describes every data field, node type, requirement type, seed formula, and data rule. Read both when you add content.

## Contents

1. [Terms](#terms)
2. [Source map](#source-map)
3. [Composition](#composition)
4. [Runtime overview](#runtime-overview)
5. [Session start](#session-start)
6. [Quest events](#quest-events)
7. [Quest selection](#quest-selection)
8. [Quest construction](#quest-construction)
9. [Graph execution](#graph-execution)
10. [Manager update loop](#manager-update-loop)
11. [Side effects: IQuestActionProcessor](#side-effects-iquestactionprocessor)
12. [Dialog flow](#dialog-flow)
13. [Battle flow](#battle-flow)
14. [Requirements](#requirements)
15. [Loot](#loot)
16. [Enemy fleets](#enemy-fleets)
17. [Seeds and determinism](#seeds-and-determinism)
18. [Persistence](#persistence)
19. [Public API and consumers](#public-api-and-consumers)
20. [Context interfaces and adapters](#context-interfaces-and-adapters)
21. [Extending the module](#extending-the-module)
22. [Validation checklist](#validation-checklist)

## Terms

These terms have one meaning in this document.

| Term | Meaning |
| --- | --- |
| Quest model | The database record `QuestModel` (alias of the generated `Quest` class). It is static data. |
| Quest | One runtime instance of a quest model: the class [`Quest`](../Assets/Modules/Quests/Scripts/Quest.cs). One quest model can have several quests at the same time, at different stars. |
| Quest star | The star where the quest belongs. The quest giver selects it when the quest starts. Region nodes and several requirements act on it. |
| Quest region | The region of the quest star. Region nodes act on its home star. |
| Node | One runtime graph element that implements [`INode`](../Assets/Modules/Quests/Scripts/Nodes/INode.cs). |
| Active node | The node where a quest currently waits. The save file stores its ID. |
| Requirement | A runtime condition that implements [`IRequirements`](../Assets/Modules/Quests/Scripts/Requirements/IRequirements.cs). |
| Start requirement | `QuestModel.Requirement`. The manager checks it before a quest starts. |
| Quest event | An `IQuestEventData` value that the game sends through `QuestEventSignal` ([QuestEventType.cs](../Assets/Modules/Quests/Scripts/QuestEventType.cs)). |
| Action | A side effect that a node asks for, such as a dialog or a battle. The application performs it through `IQuestActionProcessor`. |
| Action-required | A quest status. The active node waits until the application calls `InvokeAction`. |
| Quest seed | An integer that fixes every random choice inside one quest. |
| Event seed | An integer that the event carries or that the manager computes. It fixes the quest selection for that event. |

## Source map

| Path | Content |
| --- | --- |
| [Manager/IQuestManager.cs](../Assets/Modules/Quests/Scripts/Manager/IQuestManager.cs) | Public service API. |
| [Manager/QuestManager.cs](../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs) | Session lifecycle, event routing, update loop, active quest choice, beacons, persistence calls. |
| [Manager/QuestCollection.cs](../Assets/Modules/Quests/Scripts/Manager/QuestCollection.cs) | Quest models of one start condition, their eligibility filter, and the pick. |
| [Quest.cs](../Assets/Modules/Quests/Scripts/Quest.cs) | One quest: active node, status, transitions. |
| [IQuest.cs](../Assets/Modules/Quests/Scripts/IQuest.cs) | `QuestStatus`, the read-only `IQuest` view, restart and cooldown rules (`QuestExtensions`). |
| [QuestEventType.cs](../Assets/Modules/Quests/Scripts/QuestEventType.cs) | Event types, event payload classes, `QuestEventSignal`. |
| [UserInteraction.cs](../Assets/Modules/Quests/Scripts/UserInteraction.cs) | Dialog data for the UI: `IUserInteraction`, `UserAction`, `Severity`. |
| [Factory/QuestFactory.cs](../Assets/Modules/Quests/Scripts/Factory/QuestFactory.cs) | Creates a quest from a model or from saved progress. |
| [Factory/QuestBuilder.cs](../Assets/Modules/Quests/Scripts/Factory/QuestBuilder.cs) | Builds one quest: caches, node graph, start node. |
| [Factory/QuestInfo.cs](../Assets/Modules/Quests/Scripts/Factory/QuestInfo.cs) | Build parameters: quest model, quest star, level, faction, seed. |
| [Factory/NodeBuilder.cs](../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs) | Database node visitor. Creates and links runtime nodes. |
| [Factory/RequirementsBuilder.cs](../Assets/Modules/Quests/Scripts/Factory/RequirementsBuilder.cs) | Database requirement visitor. Creates runtime requirements. |
| [Factory/RequirementsFactory.cs](../Assets/Modules/Quests/Scripts/Factory/RequirementsFactory.cs) | Start requirements and quest givers for the manager. |
| [Factory/Cache.cs](../Assets/Modules/Quests/Scripts/Factory/Cache.cs) | Per-quest caches for requirements, enemy fleets, and loot. |
| [Factory/FleetFactory.cs](../Assets/Modules/Quests/Scripts/Factory/FleetFactory.cs) | `QuestEnemyData`: the enemy ships of a quest battle. |
| [Nodes/](../Assets/Modules/Quests/Scripts/Nodes/) | `INode` and all node classes. |
| [Requirements/](../Assets/Modules/Quests/Scripts/Requirements/) | `IRequirements` and all requirement classes. |
| [Loot/Loot.cs](../Assets/Modules/Quests/Scripts/Loot/Loot.cs) | `ILoot`, `Loot`, `LootItem`, `EmptyLoot`. |
| [QuestGiver/QuestGiver.cs](../Assets/Modules/Quests/Scripts/QuestGiver/QuestGiver.cs) | Selects the quest star from `QuestModel.Origin`. |
| [Utils/FactionFilter.cs](../Assets/Modules/Quests/Scripts/Utils/FactionFilter.cs) | Applies a database `RequiredFactions` filter to fleets, loot, research, and bases. |
| [Context/](../Assets/Modules/Quests/Scripts/Context/) | Interfaces that the application implements. |

## Composition

[GameInstaller.cs:111](../Assets/Scripts/Installers/GameInstaller.cs#L111) binds the module. All bindings are singletons in the game container.

| Binding | Implementation |
| --- | --- |
| `IQuestManagerContext` | [QuestManagerContext](../Assets/Scripts/Domain/Quests/QuestManagerContext.cs) |
| `IQuestBuilderContext` | [QuestBuilderContext](../Assets/Scripts/Domain/Quests/QuestBuilderContext.cs) |
| `IQuestManagerEventProvider` | [QuestManagerEventProvider](../Assets/Scripts/Domain/Quests/QuestManagerEventProvider.cs) |
| `IQuestDataProvider`, `IQuestDataStorage` | [QuestDataProvider](../Assets/Scripts/Domain/Quests/QuestDataProvider.cs) |
| `IStarMapDataProvider`, `IPlayerDataProvider` | [StarMapDataProvider](../Assets/Scripts/Domain/Quests/StarMapDataProvider.cs) |
| `IGameDataProvider` | [GameDataProvider](../Assets/Scripts/Domain/Quests/GameDataProvider.cs) |
| `ICharacterDataProvider` | [CharacterDataProvider](../Assets/Scripts/Domain/Quests/CharacterDataProvider.cs) |
| `IInventoryDataProvider` | [InventoryDataProvider](../Assets/Scripts/Domain/Quests/InventoryDataProvider.cs) |
| `ILootItemFactory` | [LootItemFactory](../Assets/Scripts/Domain/Quests/LootItemFactory.cs) |
| `QuestFactory`, `RequirementsFactory` | Module classes. |
| `IQuestManager`, `IInitializable`, `ITickable`, `IDisposable` | `QuestManager`, non-lazy. |
| Signals | `QuestListChangedSignal`, `QuestActionRequiredSignal`, `QuestEventSignal`, with triggers. |

`QuestCombatModelFacctory` ([QuestCombatModelFacctory.cs](../Assets/Scripts/Domain/Quests/QuestCombatModelFacctory.cs)) is bound in the same method. It is an application class: it turns `QuestEnemyData` into a combat model. `IQuestActionProcessor` has no binding. `StarMapState` implements it and passes itself to `InvokeAction` ([StarMapState.cs:21](../Assets/Scripts/GameStateMachine/States/StarMapState.cs#L21)).

## Runtime overview

```text
 game code ──QuestEventSignal──► QuestManager.OnQuestEvent
                                   │
          ┌────────────────────────┴───────────────────────────┐
          │ selection events                                   │ all other events
          ▼                                                    ▼
   QuestCollection.UpdateQuests                        Quest.TryProcessEvent (each quest)
   (level, cooldown, origin, restart rule,                     │ moved?
    start requirement)                                         ▼
          │ pick                                      _recentlyUpdatedQuests
          ▼                                                    │
   QuestFactory ─► QuestBuilder ─► NodeBuilder                 │
          │                        RequirementsBuilder          │
          ▼                                                    │
   QuestManager.Add ──────────────────────────────────────────►│
                                                               ▼
                                         QuestManager.Tick ─► OnQuestUpdated
                                           │  save progress (IQuestDataStorage)
                                           │  finished? → record result, remove
                                           │  action-required? → QuestActionRequiredSignal
                                           │  beacons → StarContentChangedSignal
                                           ▼
                       StarMapState.UpdateQuests ─► QuestManager.InvokeAction(this)
                                           │
                                           ▼
                       active node ─► IQuestActionProcessor (dialog, battle, shop, galaxy change)
```

The module is single-threaded. All calls run on the Unity main thread.

## Session start

`QuestManager.Initialize` subscribes to three events of `IQuestManagerEventProvider` ([QuestManager.cs:40](../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L40)). The session fires `SessionDataLoadedSignal` and then `SessionCreatedSignal`, both for a new game and for a loaded game ([SessionData.cs:114](../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L114)).

1. `SessionLoaded` calls `Reset` ([QuestManager.cs:92](../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L92)). It clears the quest list, the beacons, the update queue, the active quest, the timer, and the six start-condition collections.
2. `SessionCreated` calls `LoadQuests` ([QuestManager.cs:109](../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L109)):
   1. It rebuilds every saved quest with `QuestFactory.Create(QuestProgress)`.
   2. It starts every `GameStart` quest model that the restart rule allows, at the current star. It does not check `Level`, `Origin`, or the start requirement.
   3. It fills one `QuestCollection` per start condition: `Beacon`, `LocalEncounter`, `ArrivedAtStar`, `NewStarExplored`, `FactionMission`, `Daily`. Each entry gets a quest giver and a start requirement built with the seed `game seed + quest ID`.

`SessionCreated` fires on every load, so step 2 runs at every load. Only the restart rule prevents a second copy. `HolidayManager` also starts holiday quests on `SessionCreated` ([HolidayManager.cs:82](../Assets/Scripts/GameServices/HolidayManager.cs#L82)).

## Quest events

`QuestEventType` lists nine event types ([QuestEventType.cs:5](../Assets/Modules/Quests/Scripts/QuestEventType.cs#L5)). Game code fires them through `QuestEventSignal.Trigger`. `QuestManagerEventProvider` forwards the signal to `QuestManager.OnQuestEvent` ([QuestManager.cs:265](../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L265)).

| Event | Payload | Source | Manager reaction |
| --- | --- | --- | --- |
| `BeaconActivated` | `BeaconEventData(starId, seed)` | [LocalEvent.cs:24](../Assets/Scripts/Domain/Galaxy/StarContent/LocalEvent.cs#L24). The editor build uses a random seed and does not mark the event as completed. | Picks a `Beacon` quest. Active quests do not see the event. |
| `LocalEncounter` | `LocalEncounterEventData(starId, seed)` | Aggressive occupants attack the player ([Occupants.cs:79](../Assets/Scripts/Domain/Galaxy/StarContent/Occupants.cs#L79)). | Picks a `LocalEncounter` quest. Active quests do not see the event. |
| `NewStarSystemSecured` | `StarEventData` | [StarMapManager.cs:70](../Assets/Scripts/Domain/Player/StarMapManager.cs#L70) | Picks a `NewStarExplored` quest. Active quests do not see the event. |
| `FactionMissionAccepted` | `StarEventData` with the region home star | The faction panel ([FactionPanelViewModel.cs:69](../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/FactionPanelViewModel.cs#L69)) | Picks a `FactionMission` quest. Active quests do not see the event. |
| `ArrivedAtStarSystem` | `StarEventData` | The player arrives at a star that has no aggressive occupants, or secures it ([StarMapManager.cs:92](../Assets/Scripts/Domain/Player/StarMapManager.cs#L92)) | Picks an `ArrivedAtStar` quest, then sends the event to all quests. |
| `Timer` | `SimpleEventData.Timer` | `QuestManager.Tick`, every 10 s of play time, and after each finished quest | Picks the first available `Daily` quest, then sends the event to all quests. |
| `ActionButtonPressed` | `ButtonPressedEventData(node, index)` | A dialog button ([QuestAction.cs:51](../Assets/Scripts/Legacy/GUI/ViewModel/Quests/QuestAction.cs#L51)) | Sends the event to all quests. Only the dialog node that created the button reacts. |
| `CombatCompleted` | `CombatEventData(isVictory)` | The combat callback that `StarMapState` installs ([StarMapState.cs:313](../Assets/Scripts/GameStateMachine/States/StarMapState.cs#L313)) | Sends the event to all quests. Only a battle node that started a battle reacts. |
| `OccupantsDefeated` | `SimpleEventData` | Occupants are suppressed or destroyed ([Occupants.cs:57](../Assets/Scripts/Domain/Galaxy/StarContent/Occupants.cs#L57)) | Sends the event to all quests. |

Every event that reaches a quest also makes waiting `Switch`, `Condition`, and `Random` nodes check their requirements again. The four selection events do not reach active quests, so a waiting node sees the change only at the next event. The 10-second timer limits that delay.

## Quest selection

### Start conditions

| `StartCondition` | Started by | Pick | Event seed |
| --- | --- | --- | --- |
| `GameStart` | `LoadQuests` | All allowed models | None. The quest seed comes from `GenerateSeed`. |
| `Manual` | `IQuestManager.StartQuest` | The given model | The caller's seed increment |
| `Beacon` | `BeaconActivated` | Weighted | The event seed |
| `LocalEncounter` | `LocalEncounter` | Weighted | The event seed |
| `NewStarExplored` | `NewStarSystemSecured` | Weighted | game seed + star ID |
| `FactionMission` | `FactionMissionAccepted` | Weighted | game seed + star ID + total quest count + region reputation |
| `ArrivedAtStar` | `ArrivedAtStarSystem` | Weighted | game seed + star ID + total quest count |
| `Daily` | `Timer` | First available in catalog order | game seed + play-time minutes + total quest count |

The seed formulas are in [QuestManager.cs:269](../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L269)–[314](../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L314). "Total quest count" is completed + failed + active quest instances ([QuestData.cs:68](../Assets/Modules/GameSession/Scripts/Facade/Content/QuestData.cs#L68)).

### Eligibility

`QuestCollection.UpdateQuests` rebuilds the list of allowed quests for each event ([QuestCollection.cs:49](../Assets/Modules/Quests/Scripts/Manager/QuestCollection.cs#L49)). A quest model passes when all checks below pass, in this order:

1. `Level` is not greater than the level of the event star.
2. `IsOnCooldown` is false. Only `Daily` quests have a cooldown ([IQuest.cs:31](../Assets/Modules/Quests/Scripts/IQuest.cs#L31)).
3. `QuestGiver.GetStartSystem` returns a star ([QuestGiver.cs:15](../Assets/Modules/Quests/Scripts/QuestGiver/QuestGiver.cs#L15)). This star becomes the quest star.
4. `CanBeStarted` allows the quest at that star ([IQuest.cs:59](../Assets/Modules/Quests/Scripts/IQuest.cs#L59)).
5. The start requirement returns true from `CanStart(questStar, eventSeed)`.

The restart rules for step 4:

| `QuestType` | Can start when |
| --- | --- |
| `Common`, `Urgent` | No active instance at the same quest star. |
| `Singleton` | No active instance anywhere. |
| `Storyline` | No active instance and no completion. |
| `Temporary` | Always. |

`IsCancellable` allows the cancel button for `Common`, `Singleton`, and `Temporary` ([IQuest.cs:26](../Assets/Modules/Quests/Scripts/IQuest.cs#L26)).

The `Daily` cooldown is `1440 / Weight` minutes since the last start. It is 20 hours when `Weight` < 0.001 and 5 minutes when `Weight` > 288. A random part of −25 %…+25 % is added at each check ([IQuest.cs:51](../Assets/Modules/Quests/Scripts/IQuest.cs#L51)). The random part changes every play-time minute, so the effective cooldown is close to the lower bound ([Q8](issues/quests.md#q8-daily-selection-ignores-weight-and-shortens-the-cooldown)).

### Pick

- `CreateRandomWeighted` draws a value in `0…max(1, total weight)` with `PcgRandom(eventSeed)` and walks the allowed list ([QuestCollection.cs:34](../Assets/Modules/Quests/Scripts/Manager/QuestCollection.cs#L34)). When the total weight is below 1, no quest starts with probability `1 − total`.
- `CreateFirstAvailable` takes the first allowed entry ([QuestCollection.cs:26](../Assets/Modules/Quests/Scripts/Manager/QuestCollection.cs#L26)).

Both call `QuestFactory.Create(model, questStar, eventSeed)`. The event seed becomes the *seed increment* of the quest seed (see [Seeds and determinism](#seeds-and-determinism)).

### Manual start

`IQuestManager.StartQuest(model, seedIncrement)` ([QuestManager.cs:129](../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L129)) uses the current star as the quest star. It ignores `Origin` and `Level` ([Q6](issues/quests.md#q6-origin-and-level-are-ignored-for-manual-and-gamestart-quests)). It refuses and logs when:

- `StartCondition` is not `Manual`;
- the restart rule refuses;
- the start requirement is not met.

Callers: the `StartQuest` node ([StarMapState.cs:263](../Assets/Scripts/GameStateMachine/States/StarMapState.cs#L263)), holidays ([HolidayManager.cs:82](../Assets/Scripts/GameServices/HolidayManager.cs#L82)), and a starbase attack when `GalaxySettings.CaptureStarbaseQuest` is set ([StarBase.cs:63](../Assets/Scripts/Domain/Galaxy/StarContent/StarBase.cs#L63)).

### Quest giver

`QuestGiver` maps `QuestModel.Origin` to a quest star ([QuestGiver.cs:15](../Assets/Modules/Quests/Scripts/QuestGiver/QuestGiver.cs#L15)). `-1` means "no star", and the quest is not offered.

| `QuestOriginType` | Quest star |
| --- | --- |
| `CurrentStar` | The event star. |
| `CurrentFactionBase` | The home star of the event star's region. |
| `HomeStar` | Star 0. |
| `RandomStar` | A random star at a random distance in `MinDistance…MaxDistance` from the event star. |
| `RandomFactionBase` | The home star of a nearby visited, non-home region whose faction passes `Factions` and whose reputation is in range. The region list is built once per session ([DB15.22](issues/database.md#db15-smaller-defects)). |

The [Quest schema page](database/schema/Quest.md#questorigin) has the full rules.

## Quest construction

### Factory

`QuestFactory` has two entry points ([QuestFactory.cs:13](../Assets/Modules/Quests/Scripts/Factory/QuestFactory.cs#L13)):

| Method | Seed | Start node |
| --- | --- | --- |
| `Create(QuestProgress)` | The saved seed | The saved node ID. When the node does not exist, the node with the lowest ID. |
| `Create(QuestModel, starId, seedIncrement)` | `IQuestDataProvider.GenerateSeed(model, star) + seedIncrement` | The node with the lowest ID. |

When the saved quest ID is not in the database, the factory logs an exception and returns `null`. `QuestManager.Add` ignores `null`, so the save loses that quest.

### Builder

`QuestBuilder` builds one quest ([QuestBuilder.cs:8](../Assets/Modules/Quests/Scripts/Factory/QuestBuilder.cs#L8)):

1. It creates three caches: `LootCache`, `EnemyCache`, and `RequirementCache`. The caches live as long as the quest.
2. It creates `QuestInfo` from the model, the quest star, and the seed ([QuestInfo.cs:14](../Assets/Modules/Quests/Scripts/Factory/QuestInfo.cs#L14)). The quest level is `QuestModel.Level`, or the quest star level when `Level` is 0. The quest faction is the faction of the quest region.
3. It runs `NodeBuilder.Build` and gets a dictionary of nodes by `NodeId`.
4. It returns `null` and logs when the graph is empty.
5. It creates `Quest` and calls `Initialize` with the start node.

### Node builder

`NodeBuilder` implements the generated visitor `INodeFactory<INode>` ([NodeBuilder.cs:9](../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L9)). It works like this:

1. The constructor indexes the database nodes by `NodeId`. `NodeId` clamps IDs to 0…1000 ([NodeId.cs:11](../Assets/Modules/Database/Scripts/Types/NodeId.cs#L11), [DB8](issues/database.md#db8-quest-node-ids-above-1000-collide)).
2. `Build` calls `CreateNode` for every ID.
3. `CreateNode` returns the cached node, or dispatches the database node to the matching `Create(Node_X)` method.
4. Each `Create` method creates the runtime node, **adds it to the cache first**, and then resolves its targets with `CreateNode`. This order makes cycles safe: a target that points back gets the cached node.
5. A target ID that has no node becomes `null`.

The builder resolves these values at build time, not at run time:

- Region nodes target the home star of the quest region ([NodeBuilder.cs:203](../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L203)).
- Occupant and battle nodes target the quest star.
- Shipyard and workshop nodes use `Value`, or the quest star level when `Value` is 0 ([NodeBuilder.cs:280](../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L280)).
- Enemy fleets and loot come from the per-quest caches, so a dialog preview and a later node share the same object.
- A `Random` node gets the seed `quest seed + node ID` ([NodeBuilder.cs:111](../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L111)).
- A `StartQuest` node gets the seed increment `node ID`, or a new random number when the parent model has `UseRandomSeed` ([NodeBuilder.cs:194](../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L194)).

### Caches

The caches are in [Cache.cs](../Assets/Modules/Quests/Scripts/Factory/Cache.cs). The key is `(quest star, quest seed, database object reference)`. Because every quest has its own caches, the reference alone decides the hit.

| Cache | Value | `null` input |
| --- | --- | --- |
| `RequirementCache` | `INodeRequirements` from a new `RequirementsBuilder` | Returns `null`. The generated code never passes `null`. |
| `EnemyCache` | `QuestEnemyData` from `FleetFactory` | Returns an empty `QuestEnemyData` with no ships ([Q9](issues/quests.md#q9-missing-loot-and-enemy-references-fail-at-run-time)). |
| `LootCache` | `Loot` | Returns `null` ([Q9](issues/quests.md#q9-missing-loot-and-enemy-references-fail-at-run-time)). |

`RequirementsFactory` has its own private `LootCache` that does not cache. It is for start requirements, which the manager builds once per session.

## Graph execution

### Node contract

[`INode`](../Assets/Modules/Quests/Scripts/Nodes/INode.cs) has three ways to leave a node:

| Member | Caller | Meaning |
| --- | --- | --- |
| `Initialize()` | `Quest`, when the node becomes active | Resets the node's run-time state. |
| `TryProceed(out target)` | `Quest`, after every node change and after an action | Moves on without an event. Returns `false` to wait. |
| `TryProcessEvent(data, out target)` | `Quest`, for each quest event | Moves on because of the event. Returns `false` to stay. |
| `ActionRequired` | `Quest.Status` | `true` when the node waits for `TryInvokeAction`. |
| `TryInvokeAction(processor)` | `QuestManager.InvokeAction` | Performs the node's action. Returns `true` when it did something. |
| `GetRequirementsText`, `TryGetBeacons` | Quest log, star map | Status text and target stars. |

### Quest

[`Quest`](../Assets/Modules/Quests/Scripts/Quest.cs) holds the active node.

- `SetActiveNode` initializes the node and then calls `UpdateActiveNode` ([Quest.cs:72](../Assets/Modules/Quests/Scripts/Quest.cs#L72)).
- `UpdateActiveNode` follows `TryProceed` until a node waits ([Quest.cs:79](../Assets/Modules/Quests/Scripts/Quest.cs#L79)). It initializes each new node. After 100 steps it logs "infinite loop" and sets the active node to `null`.
- `TryProcessEvent` asks the active node. When the node moves, it calls `SetActiveNode` with the target ([Quest.cs:64](../Assets/Modules/Quests/Scripts/Quest.cs#L64)).
- `TryInvokeAction` asks the active node, then calls `UpdateActiveNode` ([Quest.cs:57](../Assets/Modules/Quests/Scripts/Quest.cs#L57)).
- A `null` active node reads as `TerminalNode.ErrorNode` (ID −1, type `Undefined`).

`Status` maps the active node to `QuestStatus` ([Quest.cs:37](../Assets/Modules/Quests/Scripts/Quest.cs#L37)):

| Active node | Status |
| --- | --- |
| `CompleteQuest` | `Completed` |
| `FailQuest` | `Failed` |
| `CancelQuest` | `Cancelled` |
| `Undefined`, or `null` (missing target, loop guard) | `Error` |
| any other node with `ActionRequired` | `ActionRequired` |
| any other node | `InProgress` |

`Completed`, `Failed`, `Cancelled`, and `Error` are finished statuses ([IQuest.cs:21](../Assets/Modules/Quests/Scripts/IQuest.cs#L21)). `ComingSoon` is a terminal node, but it is not finished: the quest stays active forever.

### Node classes

| Node class | Database types | `Initialize` | Waits for | Then |
| --- | --- | --- | --- | --- |
| [TextNode](../Assets/Modules/Quests/Scripts/Nodes/TextNode.cs) | `ShowDialog` | `ActionRequired` = any action requirement met | `InvokeAction` (shows the dialog), then `ActionButtonPressed` from its own button | The target of the pressed button. A button whose requirement is no longer met logs an error and stays. |
| [SwitchNode](../Assets/Modules/Quests/Scripts/Nodes/SwitchNode.cs) | `Switch` | – | Nothing, or any event while no transition and no default applies | The first met transition, else the default. |
| [ConditionNode](../Assets/Modules/Quests/Scripts/Nodes/ConditionNode.cs) | `Condition` | – | Any event while the requirement is not met | The single target. |
| [RandomNode](../Assets/Modules/Quests/Scripts/Nodes/RandomNode.cs) | `Random` | – | Any event while no pick is possible | A weighted pick among met transitions, else the default ([Q2](issues/quests.md#q2-a-random-node-without-a-default-can-wait-forever)). |
| [BattleNode](../Assets/Modules/Quests/Scripts/Nodes/BattleNode.cs) | `AttackFleet` | not started | `InvokeAction` (starts the battle), then `CombatCompleted` | Victory or defeat target. |
| [AttackEnemyNode](../Assets/Modules/Quests/Scripts/Nodes/AttackEnemyNode.cs) | `AttackOccupants`, `AttackStarbase` | not started | Same as `BattleNode` | Victory or defeat target. |
| [ActionNode](../Assets/Modules/Quests/Scripts/Nodes/ActionNode.cs) subclasses | all one-shot actions (table below) | `ActionRequired` = true | `InvokeAction` | `TargetNode`, immediately. |
| [TerminalNode](../Assets/Modules/Quests/Scripts/Nodes/TerminalNode.cs) | `CompleteQuest`, `FailQuest`, `CancelQuest`, `ComingSoon`, `Undefined` | – | Forever | – |

`ActionNode` subclasses and their calls:

| Class | Database types | `IQuestActionProcessor` call |
| --- | --- | --- |
| `LootNode` | `ReceiveItem` | None. It calls `ILoot.Consume()`, which gives each item through `IItemType.Consume`. |
| `RemoveItemNode` | `RemoveItem` | None. It calls `ILoot.Remove()`, which takes each item through `IItemType.Withdraw`. |
| `TradingNode` | `Trade` | `StartTrading(loot)` |
| `ShipyardNode` | `OpenShipyard` | `OpenShipyard(faction, level)` |
| `WorkshopNode` | `OpenWorkshop` | `OpenWorkshop(faction, level)` |
| `RetreatNode` | `Retreat` | `Retreat()` |
| `SuppressOccupantNode` | `SuppressOccupants`, `DestroyOccupants` | `SuppressOccupants(questStar, destroy)` |
| `CharacterRelationsNode` | `SetCharacterRelations`, `ChangeCharacterRelations` | `SetCharacterRelations(id, value, additive)` |
| `FactionRelationsNode` | `SetFactionRelations`, `ChangeFactionRelations` | `SetFactionRelations(homeStar, value, additive)` |
| `StarbasePowerNode` | `SetFactionStarbasePower`, `ChangeFactionStarbasePower` | `SetFactionStarbasePower(homeStar, value, additive)` |
| `CaptureStarBaseNode` | `CaptureStarBase`, `LiberateStarBase` | `CaptureStarBase(homeStar, capture)` |
| `ChangeFactionNode` | `ChangeFaction` | `ChangeFaction(homeStar, faction)` |
| `StartQuestNode` | `StartQuest` | `StartQuest(model, seedIncrement)` |

Several node classes report a wrong `INode.Type` ([Q10.1](issues/quests.md#q10-smaller-defects)). Only `Quest.Status` reads `Type`, and it reads only terminal types, so this has no effect today.

### What "wait" means

A waiting node checks again only when the quest gets an event. The timer event guarantees a check every 10 seconds of play time. There is no subscription to the state that a requirement reads.

## Manager update loop

`QuestManager.Tick` runs every frame after the game starts ([QuestManager.cs:54](../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L54)).

1. It drains `_recentlyUpdatedQuests` from the end and calls `OnQuestUpdated` for each quest. After 100 items it logs "infinite loop", calls `Debug.Break`, and stops for this frame.
2. When 10 s of play time passed since the last timer event, it sends `Timer`.

A quest enters `_recentlyUpdatedQuests` when:

- `Add` adds it;
- `InvokeAction` performed its action;
- `ProcessQuestEvent` moved it.

`OnQuestUpdated` ([QuestManager.cs:174](../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L174)):

1. It saves progress (not for `Temporary` quests).
2. When the status is finished, it calls `CompleteQuest`, then `FindActiveQuest`.
3. When the status is `ActionRequired`, it makes the quest the active quest if there is no active quest, the active quest no longer needs an action, or this quest is newer in the list. It sets `ActionRequired` and fires `QuestActionRequiredSignal`.
4. Otherwise, when this quest was the active quest, it calls `FindActiveQuest`. That method selects the newest quest with status `ActionRequired`.
5. It recomputes the beacons.

`CompleteQuest` ([QuestManager.cs:235](../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L235)):

1. It removes the quest from the list and from the update queue.
2. For a `Temporary` quest it stops here ([Q5](issues/quests.md#q5-a-finished-temporary-quest-does-not-refresh-the-quest-list)).
3. It records the result in `IQuestDataStorage`: completed, failed, or cancelled. `InProgress` (from `AbandonQuest`) counts as cancelled. `Error` and any other status log an exception and count as cancelled.
4. It sends `Timer` to the remaining quests and fires `QuestListChangedSignal`.

`AbandonQuest` calls `CompleteQuest` with the quest's current status and recomputes the beacons ([QuestManager.cs:83](../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L83), [Q4](issues/quests.md#q4-abandonquest-leaves-a-stale-active-quest)).

`QuestManager.ActionRequired` is a stored flag. It changes only in `OnQuestUpdated` and `FindActiveQuest`. A node that becomes action-required without moving does not update it ([Q3](issues/quests.md#q3-the-manager-misses-dialogs-that-become-available)).

### Beacons

A beacon marks a star on the map as a quest target. `UpdateQuestBeacons` asks each quest's active node for its beacons ([QuestManager.cs:200](../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L200)). Dialog, switch, condition, and random nodes return the `BeaconPosition` of their requirements. Only `StarRequirements` (`RandomStarSystem`, `ComeToOrigin`) has a beacon; boolean requirements return the first beacon of their children. For every star that gained or lost a beacon, the manager fires `StarContentChangedSignal`. `IsQuestObjective(starId)` answers the map ([StarData.cs:65](../Assets/Scripts/Domain/Galaxy/StarData.cs#L65)).

## Side effects: IQuestActionProcessor

[`IQuestActionProcessor`](../Assets/Modules/Quests/Scripts/Context/IQuestActionProcessor.cs) is the only way a node changes the game outside the module (except loot, which changes the inventory through `IItemType`). `StarMapState` implements it.

`StarMapState.UpdateQuests` calls `InvokeAction(this)` when the state is active and `ActionRequired` is true ([StarMapState.cs:228](../Assets/Scripts/GameStateMachine/States/StarMapState.cs#L228)). It runs on `QuestActionRequiredSignal`, on state activation, and on a player position change ([StarMapState.cs:97](../Assets/Scripts/GameStateMachine/States/StarMapState.cs#L97)). So a quest action never runs while another state (combat, a dialog, the ship editor) is on top. It runs when the star map becomes active again.

| Method | Implementation ([StarMapState.cs:237](../Assets/Scripts/GameStateMachine/States/StarMapState.cs#L237)–[311](../Assets/Scripts/GameStateMachine/States/StarMapState.cs#L311)) |
| --- | --- |
| `ShowUiDialog` | Loads `QuestState` additively. |
| `StartCombat` | Builds a combat model with `QuestCombatModelFacctory` and loads `CombatState`. |
| `AttackOccupants`, `AttackStarbase` | Builds the combat model of the star's occupants or starbase and loads `CombatState`. |
| `SuppressOccupants` | `Occupants.Suppress`. This fires `OccupantsDefeated`. |
| `StartTrading` | Opens the market with a quest inventory built from the loot. |
| `Retreat` | Loads `RetreatState`. |
| `SetCharacterRelations`, `SetFactionRelations` | Writes the session; the session clamps to −100…100. |
| `SetFactionStarbasePower`, `CaptureStarBase`, `ChangeFaction` | Changes the region object. |
| `StartQuest` | `IQuestManager.StartQuest`. |
| `OpenShipyard`, `OpenWorkshop` | Opens the window as an additive dialog state. |

The application layer plans to move this interface out of `StarMapState` ([application issue 5](issues/application_layer.md#5-game-flow-uses-an-ambiguous-global-exitsignal-and-in-memory-callbacks)).

## Dialog flow

1. A `TextNode` becomes active. `Initialize` sets `ActionRequired` when at least one action requirement is met.
2. `OnQuestUpdated` fires `QuestActionRequiredSignal`. `StarMapState` calls `InvokeAction`.
3. `TextNode.TryInvokeAction` builds one `UserAction` per met action ([TextNode.cs:95](../Assets/Modules/Quests/Scripts/Nodes/TextNode.cs#L95)). Each action holds a `ButtonPressedEventData(this node, action index)`. The node clears `ActionRequired` and calls `ShowUiDialog` with a `UserInteraction`. The interaction carries the message, the speaker, the view mode, the enemy preview, and the loot preview.
4. `QuestState` switches the view mode and opens `EventDialog` or `MiniEventDialog` ([QuestState.cs:28](../Assets/Scripts/GameStateMachine/States/QuestState.cs#L28)). `QuestEventDialog` fills the panels ([QuestEventDialog.cs:19](../Assets/Scripts/Gui/Quests/QuestEventDialog.cs#L19)).
5. The player presses a button. `QuestAction.OnButtonClicked` fires the stored event through `QuestEventSignal`, and the button closes the window. The Escape key is blocked, so the player must choose.
6. `QuestManager` sends the event to all quests. Only the node that is the event's `Context` reacts ([TextNode.cs:59](../Assets/Modules/Quests/Scripts/Nodes/TextNode.cs#L59)). The quest moves to the button's target.
7. `QuestState` unloads. `StarMapState` activates and calls `UpdateQuests` again.

The button text and the message are localization keys. The UI localizes them.

## Battle flow

1. A `BattleNode` or `AttackEnemyNode` becomes active with `ActionRequired` = true.
2. `InvokeAction` sets `_started` and calls `StartCombat`, `AttackOccupants`, or `AttackStarbase`.
3. `StarMapState` loads `CombatState` with a callback. At the end of the battle the callback fires `CombatEventData(isVictory)`.
4. The node takes `VictoryNode` or `DefeatNode`. Any result other than a victory is a defeat.

`_started` is not saved. When the game reloads during a battle, the node asks for the battle again ([Persistence](#persistence)). The loot of an `AttackFleet` node goes into the battle rewards through `QuestCombatModelFacctory.CreateCombatModel` ([QuestCombatModelFacctory.cs:32](../Assets/Scripts/Domain/Quests/QuestCombatModelFacctory.cs#L32)). Missing `CombatRules` fall back to `CombatSettings.DefaultCombatRules`.

## Requirements

### Interfaces

[IRequirements.cs](../Assets/Modules/Quests/Scripts/Requirements/IRequirements.cs) defines two views:

| Interface | Members | Used by |
| --- | --- | --- |
| `INodeRequirements` | `IsMet`, `GetDescription(localization)`, `BeaconPosition` | Nodes, dialog buttons |
| `IQuestRequirements` | `CanStart(questStar, seed)` | Quest selection |
| `IRequirements` | both | Every implementation |

`IsMet` reads live game state on every call. Nothing caches the result.

### Build contexts

`RequirementsBuilder` builds requirements for two contexts ([RequirementsBuilder.cs:7](../Assets/Modules/Quests/Scripts/Factory/RequirementsBuilder.cs#L7)):

| Context | Built by | `QuestInfo` | Life |
| --- | --- | --- | --- |
| Node requirement | `RequirementCache` in `QuestBuilder` | The quest: quest star, level, faction, quest seed, quest ID | The quest |
| Start requirement | `RequirementsFactory.CreateForQuest` | `new QuestInfo(seed)`: star 0, level 0, empty faction, quest ID 0 | The session (selection collections), or one call (manual start) |

The start context has no quest star. Requirements that read the quest star or quest ID therefore check star 0, the current star, or quest 0 ([DB15.25](issues/database.md#db15-smaller-defects)). The [Quest schema page](database/schema/Quest.md#start-requirements) lists the effect per type. Inside the graph, three classes also treat star 0 as "the current star" ([Q7](issues/quests.md#q7-star-0-means-current-star-in-region-requirements)).

### Classes

| Database type | Class | `IsMet` reads | `CanStart` |
| --- | --- | --- | --- |
| `Empty` | `EmptyRequirements` | – (true) | true |
| `Any`, `All`, `None` | `BooleanRequirements` | children. An empty list is met only for `None`. | children, with `CanStart` |
| `PlayerPosition` | `CurrentStarRequirements` | current star level in range, star secured unless unsafe allowed | `IsMet` |
| `RandomStarSystem` | `StarRequirements` | player at a star chosen at build time | `IsMet` |
| `ComeToOrigin` | `StarRequirements` | player at the quest star | `IsMet` |
| `AggressiveOccupants` | `OccupantsAttackingRequirements` | current star not secured | `IsMet` |
| `QuestCompleted`, `QuestActive` | `QuestRequirement` | quest history | `IsMet` |
| `CharacterRelations` | `CharacterRelationsRequirement` | character attitude in range | `IsMet` |
| `FactionRelations` | `FactionReputationRequirement` | quest region reputation in range | `IsMet` |
| `FactionStarbasePower` | `StarbasePowerRequirement` | quest region starbase power in range | `IsMet` |
| `StarbaseCaptured` | `StarbaseCaprturedRequirement` | quest region captured | `IsMet` |
| `Faction` | `FactionRequirements` | current star region faction | quest star region faction |
| `IsHostileFaction` | `HostileFactionRequirement` | current star region faction has `NoMissions` | quest star region |
| `HaveQuestItem` | `ArtifactRequirement` | quest item quantity | `IsMet` |
| `HaveItem`, `HaveItemById` | `ItemsRequirements` | `ILoot.CanBeRemoved` | `IsMet` |
| `TimeSinceQuestStart` | `TimeSinceQuestStart` | play time since the start of this quest at the quest star | `IsMet` |
| `TimeSinceLastCompletion` | `TimeSinceLastCompletion` | play time since the last completion or failure of this quest | `IsMet` |

Range rules: `CharacterRelations` and `FactionRelations` treat min > max with a 0 bound as an open bound. `FactionStarbasePower` treats max 0 with min > 0 as no upper bound. The constructors assert `min <= max`; the assert is stripped in release builds.

### Descriptions

`GetDescription` produces the status line in the quest log and the hint under a dialog button. Only these types return text in a release build: `StarRequirements` (`$Condition_GoToStar`), `ArtifactRequirement`, `ItemsRequirements`, and the two time requirements (`$TimeLeft…`). `BooleanRequirements` returns the first unmet child description for `All`, and nothing for `Any` and `None`. The other types return debug text in the editor only. Give a waiting `Switch` or `Condition` node a `Message` when the requirement has no text.

## Loot

[Loot.cs](../Assets/Modules/Quests/Scripts/Loot/Loot.cs) turns a database `LootModel` into `LootItem` values. A `LootItem` is an `IItemType` from CommonComponents and a quantity.

- `Loot` implements the generated visitor `ILootContentFactory<IEnumerable<LootItem>>`. Each `LootContent_*` case creates items through `ILootItemFactory`.
- `Loot` creates its items on the first read of `Items` or `CanBeRemoved`, and then keeps them. The random generator is `System.Random(quest seed + loot ID)`.
- `Consume()` gives the items to the player. `Remove()` takes them. `CanBeRemoved` is true when the player can give away every item (`IItemType.MaxItemsToWithdraw`).
- Quantities, random components, research points, and random money use the quest level and the quest faction from `QuestInfo`.
- `EmptyLoot` is the placeholder for a dialog without a loot preview.

The [Loot schema page](database/schema/Loot.md) describes each content type.

## Enemy fleets

[`FleetFactory.CreateCombatPlan`](../Assets/Modules/Quests/Scripts/Factory/FleetFactory.cs#L29) returns `QuestEnemyData`:

| Field | Value |
| --- | --- |
| `EnemyLevel` | max(0, quest level + `Fleet.LevelBonus`) |
| `EnemyFleet` | Random ships plus `Fleet.SpecificShips`, shuffled with `System.Random(quest seed)` |
| `Rules` | `Fleet.CombatRules`, or `null` for the default rules |
| `StarLevel` | The quest level |
| `Seed` | The quest seed |

The random part takes `1 + min(3…7, enemy level / 3)` common enemy builds that fit the level and pass the `Fleet.Factions` filter. `NoRandomShips` sets the count to 0. The faction filter uses the hull faction ([DB14](issues/database.md#db14-quest-fleets-filter-by-hull-faction)). The application turns `QuestEnemyData` into `QuestFleet` ([QuestCombatModelFacctory.cs:26](../Assets/Scripts/Domain/Quests/QuestCombatModelFacctory.cs#L26)).

## Seeds and determinism

The same quest model at the same quest star with the same history produces the same graph content. The chain:

1. **Base seed.** `QuestDataProvider.GenerateSeed` returns `game seed + (quest ID + star ID + 1) × (n + 1)`. `n` counts the active, completed, and failed instances of the quest model ([QuestDataProvider.cs:40](../Assets/Scripts/Domain/Quests/QuestDataProvider.cs#L40)). With `UseRandomSeed` it returns a new random number.
2. **Quest seed.** Base seed + seed increment. For event quests the increment is the event seed. For manual quests the caller gives it.
3. **Derived seeds.** Loot: quest seed + loot ID. Fleet: quest seed. Random node: quest seed + node ID + pick counter. `RandomStarSystem`: quest seed.

The save file stores the quest seed. A reload rebuilds the same loot, fleets, and random targets. Two things are not reproducible:

- The `Random` node pick counter is not saved. After a reload, a loop through a random node repeats the picks from the start.
- `UseRandomSeed` creates a new seed for `StartQuest` nodes at every build, including the rebuild after a reload.

Keep every new random choice derived from `QuestInfo.Seed`. Do not use `UnityEngine.Random` or an unseeded generator in the module.

## Persistence

The module writes through `IQuestDataStorage`. [`QuestDataProvider`](../Assets/Scripts/Domain/Quests/QuestDataProvider.cs) maps it to the session `QuestData` ([QuestData.cs](../Assets/Modules/GameSession/Scripts/Facade/Content/QuestData.cs)). See [GameSession](game_session.md) for the save format.

### Stored data

| Data | Key | Written by |
| --- | --- | --- |
| Progress: seed, active node ID, start time | quest ID → quest star | `SetQuestProgress` on `Add` and on each `OnQuestUpdated`. A new key also sets `LastStartTime`. |
| Statistics: completion count, failure count, last start time, last completion time | quest ID | `SetQuestCompleted` (success or failure). It also removes the progress. |
| Faction reputation | region home star | `StarMapState.SetFactionRelations` |
| Character relations | character ID | `StarMapState.SetCharacterRelations` |

`SetQuestCancelled` removes the progress and does not change the statistics. `TimeSinceLastCompletion` and the daily cooldown therefore ignore cancelled instances. "Last completion time" also changes on a failure.

The key `(quest ID, quest star)` allows only one saved instance per quest model and star. The restart rules match this key, except for `Temporary` quests, which are not saved.

### Not stored

- `Temporary` quests. They have no progress, no start time, and no result ([Q1](issues/quests.md#q1-temporary-quests-have-no-history)).
- Node state: `TextNode.ActionRequired`, battle `_started`, the random pick counter. `Initialize` rebuilds the first two on load.
- The selection collections. `LoadQuests` rebuilds them.

### Restore

`Create(QuestProgress)` rebuilds the graph with the saved seed and activates the saved node. `Initialize` and `TryProceed` run again. In practice:

- A dialog shows again.
- A battle asks for the battle again.
- An action node that was saved before its action runs the action once.
- When a mod removed the saved node, the quest restarts at its lowest node ID. Its start actions run again.

The session is written on application pause, focus loss, disable, and star map scene load ([GameDataManager.cs:180](../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L180)). The manager saves progress on the frame after an action. An action and its progress are not written in one step, but the gap is at most one frame.

## Public API and consumers

[`IQuestManager`](../Assets/Modules/Quests/Scripts/Manager/IQuestManager.cs):

| Member | Consumers |
| --- | --- |
| `ActionRequired`, `InvokeAction(processor)` | `StarMapState` |
| `Quests` (`IEnumerable<IQuest>`) | Quest log ([QuestLogPanelViewModel.cs:62](../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/QuestLogPanelViewModel.cs#L62)), faction panel ([FactionPanelViewModel.cs:62](../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/FactionPanelViewModel.cs#L62)) |
| `IsQuestObjective(starId)` | Star map markers through `StarData` |
| `AbandonQuest(quest)` | Quest log cancel button ([QuestLogPanelViewModel.cs:41](../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/QuestLogPanelViewModel.cs#L41)) |
| `StartQuest(model, seedIncrement)` | `StarMapState`, `HolidayManager`, `StarBase` |

[`IQuest`](../Assets/Modules/Quests/Scripts/IQuest.cs#L83) exposes `Model`, `StarId`, `Status`, `GetRequirementsText`, and `TryGetBeacons`. `QuestExtensions.IsCancellable` and `IsFactionMission` serve the UI. The quest log hides quests without `Name`.

Signals out of the module, through `IQuestManagerEventProvider`:

| Signal | When |
| --- | --- |
| `QuestListChangedSignal` | A quest is added, or a non-temporary quest finishes. |
| `QuestActionRequiredSignal` | A quest becomes the active action-required quest. |
| `StarContentChangedSignal` | A beacon appears or disappears at a star. |

## Context interfaces and adapters

All interfaces are in [Context/](../Assets/Modules/Quests/Scripts/Context/). The adapters live in the application layer ([application layer](application_layer.md)).

| Interface | Purpose | Adapter |
| --- | --- | --- |
| `IQuestManagerContext` | Bundle for `QuestManager`: event provider, storage, star map, game data | `QuestManagerContext` |
| `IQuestBuilderContext` | Bundle for builders: player, inventory, star map, characters, quest data, game data, loot factory, database | `QuestBuilderContext` |
| `IQuestManagerEventProvider` | Session events in, UI signals out | `QuestManagerEventProvider` |
| `IQuestDataProvider` / `IQuestDataStorage` | Quest history and seeds / progress and results | `QuestDataProvider` |
| `IStarMapDataProvider`, `IPlayerDataProvider` | Stars, regions, current star, random star, nearby regions | `StarMapDataProvider` |
| `IStarDataProvider`, `IRegionDataProvider` | One star: level, secured, region. One region: home star, faction, reputation, power, captured, visited, home | `StarDataProvider`, `RegionDataProvider` |
| `IGameDataProvider` | Game started, total play time (ticks), game seed | `GameDataProvider` |
| `ICharacterDataProvider` | Character attitude | `CharacterDataProvider` |
| `IInventoryDataProvider` | Quest item quantity | `InventoryDataProvider` |
| `ILootItemFactory` | Creates application item types for loot | `LootItemFactory` |
| `IQuestActionProcessor` | Side effects | `StarMapState` |

`StarMapDataProvider` caches the last two `StarDataProvider` objects. A `StarDataProvider` reads the occupants and the region live, so the cached objects return current values. Time values are `TimeSpan` ticks of total play time ([GameTime.cs](../Assets/Scripts/Services/InternetTime/GameTime.cs)), not wall-clock time.

## Extending the module

### Content with existing types

1. Edit the quest JSON in the Database module, or use the database editor.
2. Follow the [data rules](database/schema/Quest.md#data-rules).
3. Keep node IDs of released quests stable. Saved progress points to them.
4. Run `analyzer.cmd` ([Database module](database.md)).

### A new node type

1. Add the enum value and the case to the schema. Regenerate the Database code. Do not edit generated files ([Database module](database.md)).
2. Add the class under `Nodes/`. Derive from `ActionNode` for a one-shot action. Report the correct `NodeType`.
3. Add `Create(Node_X)` to `NodeBuilder`. Add the node to `_nodes` **before** you resolve its targets.
4. When the node causes a new side effect, add a method to `IQuestActionProcessor` and implement it in `StarMapState`.
5. Keep any randomness on `QuestInfo.Seed`.

### A new requirement type

1. Add the schema case and regenerate.
2. Add the class under `Requirements/`. Implement both `IsMet` and `CanStart`. Decide what the class does in the start context, where the quest star is 0.
3. Add `Create(Requirement_X)` to `RequirementsBuilder`.
4. When the requirement needs new game state, extend the narrowest context interface and its adapter.
5. Return a localized description when the player needs a hint.

### A new event type

1. Add the value to `QuestEventType` and a payload class when needed.
2. Fire it through `QuestEventSignal.Trigger` from the application.
3. When it starts quests, add a `StartCondition`, a `QuestCollection`, and a branch in `QuestManager.OnQuestEvent`. Decide whether active quests also get the event.

## Validation checklist

The module has no automated tests. Check a changed quest in Play Mode.

1. Start the quest through its start condition. Confirm that an ineligible start is refused.
2. Follow every branch, including unmet requirements, defeat, cancel, and timeouts.
3. Save and reload at each dialog and battle. Confirm the same node, loot, fleet, and target star.
4. Confirm that completion updates the quest log, beacons, cooldowns, and history once.
5. Confirm that no graph loop reaches the 100-step guard.
6. Check every localization key in messages, buttons, and waiting nodes.
