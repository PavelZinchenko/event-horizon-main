# Quest

A quest is a graph of nodes with start rules. Game events offer a quest, the quest manager picks one that can start, and the graph then shows dialogs, starts battles, gives or takes items, and changes the galaxy. The quest manager, the quest module builders, and the star-map state read quest records. For the runtime flow, see the [Quests module](../../quests.md).

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `15` |
| Kind | collection |
| Folder | [`Resources/Database/Quests/`](../../../Assets/Modules/Database/Resources/Database/Quests/): 2 quests in the folder itself, `QuestsBeacon/` (6), `QuestsFaction/` (3), `QuestsScavengers/` (6), `QuestsSpecial/` (3) |
| Records in the built-in catalog | 20. The highest node ID is 100. 8 quests have a `Name`. Only quest 200 sets `Origin`. |
| Schema | [Quest.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/Quest.xml), [QuestOrigin.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/QuestOrigin.xml), [Node.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/Node.xml), [NodeAction.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/NodeAction.xml), [NodeTransition.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/NodeTransition.xml), node list [NodeType.xml](../../../Assets/Modules/Database/.Schema/v1/Enums/Quests/NodeType.xml) |
| Generated class | `GameDatabase.DataModel.QuestModel` (alias of `Quest`), with switch classes `Node_*` ([Quest.cs](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Quest.cs), [Node.cs](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Node.cs)) |
| Runtime | [`QuestManager`](../../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs), [`QuestCollection`](../../../Assets/Modules/Quests/Scripts/Manager/QuestCollection.cs), [`QuestGiver`](../../../Assets/Modules/Quests/Scripts/QuestGiver/QuestGiver.cs), [`QuestBuilder`](../../../Assets/Modules/Quests/Scripts/Factory/QuestBuilder.cs), [`NodeBuilder`](../../../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs), nodes in [`Nodes/`](../../../Assets/Modules/Quests/Scripts/Nodes/), actions in [StarMapState.cs:237](../../../Assets/Scripts/GameStateMachine/States/StarMapState.cs#L237) |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Name` | string | | Title in the quest log, usually a localization key. The quest log hides a quest without a name ([QuestLogPanelViewModel.cs:62](../../../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/QuestLogPanelViewModel.cs#L62)). |
| `QuestType` | enum [QuestType](enums.md#questtype) | `Common` | Restart rule and cancel button. See [Restart rules](#restart-rules). |
| `StartCondition` | enum [StartCondition](enums.md#startcondition) | `Manual` | The event that can start the quest. See [Start and selection](#start-and-selection). |
| `Weight` | float | 0…1000 | Selection weight among the quests of one event. A total below 1 leaves a chance that no quest starts. For `Daily` quests, it sets the cooldown instead. `GameStart` and `Manual` quests ignore it. |
| `Origin` | struct [QuestOrigin](#questorigin) | `CurrentStar` | Selects the quest star: the star where the quest belongs. Node actions and requirements refer to this star. |
| `Requirement` | struct [Requirement](structures.md#requirement) | `Empty` | Condition to start the quest. It is evaluated without a quest star (see [Start requirements](#start-requirements)). |
| `Level` | int | 0…1000 | Two uses. (1) Event-started quests need a current star level of at least `Level` ([QuestCollection.cs:60](../../../Assets/Modules/Quests/Scripts/Manager/QuestCollection.cs#L60)). (2) The quest level: fleets, loot, and shops scale with it. `0` uses the quest star's level ([QuestInfo.cs:18](../../../Assets/Modules/Quests/Scripts/Factory/QuestInfo.cs#L18)). |
| `UseRandomSeed` | bool | | The quest seed is a new random number at every start, not a value derived from the game seed. `StartQuest` nodes then also pass a random seed increment. See [Seeds](#seeds). |
| `Nodes` | struct list [Node](#node-switch) | | The quest graph. The node with the lowest ID is the start node. |

## Structures

### QuestOrigin

Selects the quest star when the quest is offered ([QuestGiver.cs:15](../../../Assets/Modules/Quests/Scripts/QuestGiver/QuestGiver.cs#L15)). When no star fits, the quest is not offered.

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Type` | enum `QuestOriginType` | `CurrentStar` | See the table below. |
| `Factions` | struct [FactionFilter](structures.md#factionfilter) | | `RandomFactionBase` only. The region faction must pass the filter. The filter runs at level 0 and has no star faction, so `AllAvailable` accepts only factions whose territory range contains level 0, and `StarOwnersAndList` acts as `ListOnly`. |
| `MinDistance` | int | 0…9999 | `RandomStar`: the smallest distance in star-grid steps from the current star. `RandomFactionBase`: the inner radius of the region search. |
| `MaxDistance` | int | 0…9999 | The largest distance. A value below `MinDistance` becomes `MinDistance`. |
| `MinRelations` | int | −100…100 | `RandomFactionBase`: the lowest player reputation with the region. |
| `MaxRelations` | int | −100…100 | The highest reputation. When both values are `0`, reputation is not checked. |

| `Type` | Value | Quest star |
| --- | ---: | --- |
| `CurrentStar` | 0 | The player's current star. |
| `CurrentFactionBase` | 1 | The home star (starbase) of the current star's region. |
| `RandomFactionBase` | 2 | The home star of a region near the current star. The region is not the player's home region, it is visited, its faction passes `Factions`, and its reputation is in range. The search starts at a random region (selection seed) and takes the first one that fits. The region list is built once per session ([DB15.22](../../issues/database.md#db15-smaller-defects)). |
| `HomeStar` | 3 | Star 0, the player's starting star. |
| `RandomStar` | 4 | A random star at a distance drawn uniformly from `MinDistance`…`MaxDistance` (selection seed). When no star exists at that distance, star 0. |

### Node (switch)

`Type` selects the node. The JSON field names are generic; the *Model name* column gives the name in the generated class. "Quest star" is the star from `Origin`. "Quest region" is the region of the quest star.

| Field | Type | Case | Model name | Default / range | Meaning |
| --- | --- | --- | --- | --- | --- |
| `Id` | int | | | 1…999,999 | Node ID, unique in the quest. Transitions and saved progress refer to it. The runtime clamps it to 0…1000 ([DB8](../../issues/database.md#db8-quest-node-ids-above-1000-collide)). |
| `Type` | enum `NodeType` | | | `Undefined` | The node. See [Node types](#node-types). |
| `RequiredView` | enum `RequiredViewMode` | `ShowDialog` | | `Any` | The map view that the dialog switches to. `StarSystem` also uses the small dialog window. `Any` keeps the current view ([QuestState.cs:32](../../../Assets/Scripts/GameStateMachine/States/QuestState.cs#L32)). |
| `Message` | string | `ShowDialog` | | | Dialog text, usually a localization key. |
| `Message` | string | `Switch`, `Condition`, `Random` | | | Status line in the quest log while the node waits. `Switch` and `Condition` localize it. `Random` shows it as written ([DB15.26](../../issues/database.md#db15-smaller-defects)). Empty: the descriptions of the transition requirements. |
| `DefaultTransition` | int | `Switch`, `Random` | | 0…999,999 | Target when no transition is taken. `0` or a missing node: the node waits and checks again at the next event. |
| `DefaultTransition` | int | action nodes (see [Node types](#node-types)) | `Transition` | 1…999,999 | The next node after the action. |
| `DefaultTransition` | int | `AttackFleet`, `AttackOccupants`, `AttackStarbase` | `VictoryTransition` | 1…999,999 | The next node after a victory. |
| `FailureTransition` | int | `AttackFleet`, `AttackOccupants`, `AttackStarbase` | | 1…999,999 | The next node after any other battle result. |
| `Enemy` | ref [Fleet](Fleet.md) | `ShowDialog`, `AttackFleet` | | | `AttackFleet`: the enemy fleet. `ShowDialog`: a preview of the fleet in the dialog. The quest builds the fleet once, so the preview shows the ships of the later battle. |
| `Loot` | ref [Loot](Loot.md) | `ShowDialog`, `ReceiveItem`, `RemoveItem`, `Trade`, `AttackFleet` | | | `ShowDialog`: a preview of items. `ReceiveItem`: items to give. `RemoveItem`: items to take. `Trade`: the merchant's stock. `AttackFleet`: an extra battle reward. See [Loot in a quest](#loot-in-a-quest). |
| `Quest` | ref [Quest](Quest.md) | `StartQuest` | | | The quest to start. It must have `StartCondition` `Manual`. |
| `Character` | ref [Character](Character.md) | `ShowDialog`, `SetCharacterRelations`, `ChangeCharacterRelations` | | | `ShowDialog`: the speaker's name and avatar; `0` shows no speaker. Relation nodes: the character. Required there ([DB15.24](../../issues/database.md#db15-smaller-defects)). |
| `Faction` | ref [Faction](Faction.md) | `OpenShipyard`, `OpenWorkshop`, `ChangeFaction` | | | Shipyard or workshop faction. `ChangeFaction`: the new owner of the quest region. `0` makes the region neutral, which the game treats as a pirate base ([Region.cs:51](../../../Assets/Scripts/Legacy/Game/Galaxy/Region.cs#L51)). |
| `Value` | int | `SetCharacterRelations`, `ChangeCharacterRelations`, `SetFactionRelations`, `ChangeFactionRelations` | | −100…100 | The new relation value (`Set…`) or the change (`Change…`). The session clamps the result to −100…100 ([QuestData.cs:156](../../../Assets/Modules/GameSession/Scripts/Facade/Content/QuestData.cs#L156)). |
| `Value` | int | `OpenShipyard`, `OpenWorkshop` | `Level` | 0…10,000 | Level of the goods. `0` uses the quest star's level ([NodeBuilder.cs:283](../../../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L283)). |
| `Value` | int | `SetFactionStarbasePower` | | 0…100,000 | New starbase defense power of the quest region, in percent. Values below `FactionsSettings.StarbaseMinDefense` become that minimum. |
| `Value` | int | `ChangeFactionStarbasePower` | | −100,000…100,000 | Change of the starbase defense power, in percent points. |
| `Actions` | struct list [NodeAction](#nodeaction) | `ShowDialog` | | | Dialog buttons. |
| `Transitions` | struct list [NodeTransition](#nodetransition) | `Switch`, `Random`, `Condition` | | | Branches. `Condition` reads only the first entry and needs one. |

### NodeAction

A dialog button of a `ShowDialog` node.

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `TargetNode` | int | 1…999,999 | The node that the button leads to. A missing node ends the quest with an error. |
| `Requirement` | struct [Requirement](structures.md#requirement) | `Empty` | The dialog shows the button only when the requirement is met. |
| `ButtonText` | string | | Button label, usually a localization key. |

### NodeTransition

A branch of a `Switch`, `Random`, or `Condition` node.

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `TargetNode` | int | 1…999,999 | The node that the branch leads to. |
| `Requirement` | struct [Requirement](structures.md#requirement) | `Empty` | The branch is possible only when the requirement is met. |
| `Weight` | float | 0…1000 | `Random` only: the selection weight. Other nodes ignore it. |

## Notes

### Node types

"Wait" means that the node stays active and checks again at the next quest event. Quest events include arrival at a star, a battle result, a button press, and a timer every 10 seconds of play time ([QuestManager.cs:351](../../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L351)). An *action node* runs its action once and then goes to `Transition`.

| `Type` | Value | Kind | Runtime effect |
| --- | ---: | --- | --- |
| `Undefined` | 0 | terminal | Error. The manager logs an exception and records the quest as cancelled. |
| `ComingSoon` | 1 | terminal | The quest stays active forever. The quest log shows `$Quest_ComingSoon`. |
| `ShowDialog` | 10 | dialog | Shows `Message` with one button per action whose requirement is met. Waits while no action is available ([TextNode.cs:95](../../../Assets/Modules/Quests/Scripts/Nodes/TextNode.cs#L95)). |
| `OpenShipyard` | 11 | action | Opens the shipyard of `Faction` at `Level`. |
| `OpenWorkshop` | 12 | action | Opens the workshop of `Faction` at `Level`. |
| `Switch` | 15 | branch | Takes the first transition whose requirement is met, else `DefaultTransition`, else waits ([SwitchNode.cs:60](../../../Assets/Modules/Quests/Scripts/Nodes/SwitchNode.cs#L60)). |
| `Random` | 16 | branch | Picks one transition whose requirement is met, by `Weight`. When all weights are 0, the pick is uniform. When the weights add up to less than 1, the rest is the chance of `DefaultTransition`. When no transition is possible, `DefaultTransition`, else waits ([RandomNode.cs:57](../../../Assets/Modules/Quests/Scripts/Nodes/RandomNode.cs#L57)). |
| `Condition` | 17 | branch | Waits until the requirement of the first transition is met, then goes to its target ([ConditionNode.cs:47](../../../Assets/Modules/Quests/Scripts/Nodes/ConditionNode.cs#L47)). |
| `AttackFleet` | 20 | battle | Starts a battle against `Enemy`. `Loot` is added to the battle rewards. The node then waits for the battle result. |
| `AttackOccupants` | 21 | battle | A battle against the occupants of the quest star. |
| `AttackStarbase` | 22 | battle | A battle against the starbase defenders of the quest region. |
| `DestroyOccupants` | 25 | action | Removes the occupants of the quest star. This also lowers the region's starbase power by `FactionsSettings.DefenseLossPerEnemyDefeated` ([Occupants.cs:40](../../../Assets/Scripts/Domain/Galaxy/StarContent/Occupants.cs#L40)). |
| `SuppressOccupants` | 26 | action | Makes the occupants of the quest star passive. They no longer attack. |
| `Retreat` | 30 | action | The player's ship flies to the nearest visited star. |
| `ReceiveItem` | 35 | action | Gives the `Loot` items. |
| `RemoveItem` | 36 | action | Takes the `Loot` items. |
| `Trade` | 37 | action | Opens a shop with the `Loot` items as stock. |
| `CompleteQuest` | 40 | terminal | The quest is completed. |
| `FailQuest` | 41 | terminal | The quest is failed. |
| `CancelQuest` | 42 | terminal | The quest is cancelled. |
| `StartQuest` | 43 | action | Starts `Quest` at the player's current star. When the quest cannot start, the manager logs an error, and this quest continues. |
| `SetCharacterRelations` | 50 | action | Sets the relation of `Character` to `Value`. |
| `SetFactionRelations` | 51 | action | Sets the reputation of the quest region to `Value`. |
| `SetFactionStarbasePower` | 52 | action | Sets the starbase power of the quest region to `Value`. |
| `ChangeCharacterRelations` | 55 | action | Adds `Value` to the relation of `Character`. |
| `ChangeFactionRelations` | 56 | action | Adds `Value` to the reputation of the quest region. |
| `ChangeFactionStarbasePower` | 57 | action | Adds `Value` to the starbase power of the quest region. |
| `CaptureStarBase` | 60 | action | Marks the quest region as captured by the player. |
| `LiberateStarBase` | 61 | action | Marks the quest region as not captured. |
| `ChangeFaction` | 62 | action | Gives the quest region to `Faction`. |

The region nodes act on the home star of the quest region ([NodeBuilder.cs:203](../../../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L203)). The actions run in the star-map state ([StarMapState.cs:247](../../../Assets/Scripts/GameStateMachine/States/StarMapState.cs#L247)).

A transition to a missing node, including `0`, makes the target `null`. The quest then has the status `Error` and ends ([Quest.cs:37](../../../Assets/Modules/Quests/Scripts/Quest.cs#L37)). The quest follows at most 100 immediate transitions in one update. A longer chain is treated as an infinite loop and ends the quest with an error ([Quest.cs:84](../../../Assets/Modules/Quests/Scripts/Quest.cs#L84)).

### Start and selection

Each start condition has its own collection of quests ([QuestManager.cs:109](../../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L109)). On an event, the collection checks every quest of that condition ([QuestCollection.cs:49](../../../Assets/Modules/Quests/Scripts/Manager/QuestCollection.cs#L49)):

1. `Level` ≤ the current star's level.
2. A `Daily` quest is not on cooldown.
3. `Origin` gives a quest star.
4. The [restart rule](#restart-rules) allows a start at the quest star.
5. `Requirement` is met (see [Start requirements](#start-requirements)).

Then one quest is picked from the quests that pass:

| `StartCondition` | Event | Pick | Code |
| --- | --- | --- | --- |
| `Beacon`, `LocalEncounter` | The player activates a beacon or a local event | By `Weight`, event seed | [QuestManager.cs:269](../../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L269) |
| `NewStarExplored` | A star is secured | By `Weight`, game seed + star | [QuestManager.cs:283](../../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L283) |
| `FactionMission` | The player accepts a mission at a starbase | By `Weight`, game seed + star + quest count + reputation | [QuestManager.cs:291](../../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L291) |
| `ArrivedAtStar` | The player arrives at a star | By `Weight`, game seed + star + quest count | [QuestManager.cs:301](../../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L301) |
| `Daily` | The 10-second timer | The first quest that passes, in catalog order | [QuestManager.cs:309](../../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L309) |
| `GameStart` | A session starts | Every quest that the restart rule allows. `Level` and `Requirement` are not checked. | [QuestManager.cs:117](../../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L117) |
| `Manual` | A `StartQuest` node, a holiday ([SpecialEventSettings](SpecialEventSettings.md)), a starbase attack ([GalaxySettings](GalaxySettings.md) `CaptureStarbaseQuest`) | The restart rule and `Requirement` at the current star. `Level` is not checked. | [QuestManager.cs:129](../../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L129) |

The weighted pick draws a value in 0…max(1, total weight) and walks the list ([QuestCollection.cs:34](../../../Assets/Modules/Quests/Scripts/Manager/QuestCollection.cs#L34)). A quest with weight 0 is practically never picked.

The `Daily` cooldown counts from the last start of the quest ([IQuest.cs:31](../../../Assets/Modules/Quests/Scripts/IQuest.cs#L31)). It is `1440 / Weight` minutes, 20 hours when `Weight` < 0.001, and 5 minutes when `Weight` > 288. A random part of −25 %…+25 % is added at each check.

### Restart rules

`CanBeStarted` ([IQuest.cs:59](../../../Assets/Modules/Quests/Scripts/IQuest.cs#L59)) and `IsCancellable` ([IQuest.cs:26](../../../Assets/Modules/Quests/Scripts/IQuest.cs#L26)) read `QuestType`:

| `QuestType` | Can start when | Cancel button | Saved |
| --- | --- | --- | --- |
| `Common` | It is not active at the same quest star. | yes | yes |
| `Singleton` | It is not active anywhere. | yes | yes |
| `Storyline` | It is not active and was never completed. A failed storyline quest can start again. | no | yes |
| `Temporary` | Always. | yes | no. A load loses it, and the start counter does not grow ([QuestManager.cs:240](../../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L240)). |
| `Urgent` | Like `Common`. | no | yes |

### Start requirements

The quest-level `Requirement` is built without a quest star: star 0, level 0, the neutral faction ([RequirementsFactory.cs:25](../../../Assets/Modules/Quests/Scripts/Factory/RequirementsFactory.cs#L25)). Some requirement types therefore check another star than inside the graph ([DB15.25](../../issues/database.md#db15-smaller-defects)):

| Requirement | In `Quest.Requirement` | In a node |
| --- | --- | --- |
| `Faction`, `IsHostileFaction` | The region of the candidate quest star | The region of the player's current star |
| `FactionRelations`, `FactionStarbasePower`, `StarbaseCaptured` | The region of the player's current star | The quest region (the current star's region when the quest star is star 0) |
| `ComeToOrigin`, `RandomStarSystem` | Star 0 | The quest star |
| `TimeSinceQuestStart`, `TimeSinceLastCompletion` | Quest ID 0. Do not use. | This quest |
| Others | Same as in a node | |

Use the start requirement for player state: position, items, completed or active quests, character relations.

### Seeds

The quest seed fixes every random choice in the graph. Without `UseRandomSeed` it is `game seed + (quest ID + star ID + 1) × (n + 1)`, where `n` counts the active, completed, and failed instances of the quest ([QuestDataProvider.cs:40](../../../Assets/Scripts/Domain/Quests/QuestDataProvider.cs#L40)). Cancelled instances do not count. The same quest at the same star therefore gives the same content until an instance completes or fails. A save stores the seed with the active node, so a reload rebuilds the same quest.

- `Random` node: seed = quest seed + node ID + the number of earlier picks at this node ([NodeBuilder.cs:111](../../../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L111)). The pick counter is not saved.
- `StartQuest` node: the started quest gets its own seed plus the node ID, or plus a random number with `UseRandomSeed` ([NodeBuilder.cs:194](../../../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L194)).
- Loot: quest seed + loot ID. See [Loot](Loot.md).
- Fleet: the quest seed. See [Fleet](Fleet.md).

### Loot in a quest

The quest caches each `Loot` record by quest star, seed, and record ([Cache.cs:120](../../../Assets/Modules/Quests/Scripts/Factory/Cache.cs#L120)). All nodes of one quest that name the same `Loot` record get the same items. A dialog can preview loot that a later `ReceiveItem` node gives, and a `HaveItemById` requirement can check the items that a later `RemoveItem` node takes.

### Saving

The session stores the quest ID, the quest star, the seed, and the active node ID. When a mod removes the saved node, the quest restarts at its lowest node ID ([QuestBuilder.cs:37](../../../Assets/Modules/Quests/Scripts/Factory/QuestBuilder.cs#L37)). When a mod removes the quest, the save loses it.

### Data rules

1. Give every quest a start node with the lowest ID, and keep node IDs between 1 and 1000 ([DB8](../../issues/database.md#db8-quest-node-ids-above-1000-collide)).
2. Point every transition and action to an existing node. A missing target ends the quest with an error.
3. Give every `Condition` node one transition, and every relation node a `Character`. Otherwise the quest build throws ([DB15.24](../../issues/database.md#db15-smaller-defects)).
4. Give every `AttackFleet` node an `Enemy`.
5. Set `StartCondition` `Manual` on a quest that a `StartQuest` node, a holiday, or `GalaxySettings.CaptureStarbaseQuest` starts. The manager refuses other conditions.
6. Do not put quest-star requirements into `Quest.Requirement` (see [Start requirements](#start-requirements)). Put them into the first node.
7. Give a waiting `Switch` or `Random` node a `Message`, or the quest log shows the raw requirement descriptions.
8. Prefer `Common` or `Singleton` over `Temporary` for quests that the player can save in the middle of.
