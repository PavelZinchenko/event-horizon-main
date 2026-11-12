# Quests Module Issues

This file records problems in `Assets/Modules/Quests/` and in the application code that drives it. For a description of the current design, see [Quests module](../quests.md). Data-level quest defects are in [Database issues](database.md) and are not repeated here:

| ID | Topic |
| --- | --- |
| [DB8](database.md#db8-quest-node-ids-above-1000-collide) | Node IDs above 1000 collide. |
| [DB14](database.md#db14-quest-fleets-filter-by-hull-faction) | Quest fleets filter by hull faction. |
| [DB15.22](database.md#db15-smaller-defects) | `RandomFactionBase` keeps the region list of the first check. |
| [DB15.24](database.md#db15-smaller-defects) | Empty `Condition` transitions and missing `Character` throw. |
| [DB15.25](database.md#db15-smaller-defects) | Start requirements are built without a quest star and quest ID. |
| [DB15.26](database.md#db15-smaller-defects) | `RandomNode` does not localize its message. |

The initial analysis used the code and built-in quest data on 2026-10-12. Later entries state their verification separately. No issue was reproduced in Play Mode. Each issue has a severity (High, Medium, or Low) and a status:

- **Confirmed** – the code path is certain. The failure follows directly from the code.
- **Confirmed (latent)** – the code path is certain, but built-in data does not trigger it. Mod data can.
- **Plausible** – the code is wrong, but the failure needs specific timing or UI conditions.

Defects use Where / Failure / Fix. Structural issues use Problem / Evidence / Direction.

## Defects

### Q1. Temporary quests have no history

**Severity:** Medium. **Status:** Confirmed (latent).

**Where.** `QuestManager` does not save progress or results of `Temporary` quests ([QuestManager.cs:240](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L240), [line 328](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L328)). Only `SetQuestProgress` writes `LastStartTime` and the start time ([QuestData.cs:102](../../Assets/Modules/GameSession/Scripts/Facade/Content/QuestData.cs#L102)). The readers return 0 when nothing is stored ([QuestData.cs:56](../../Assets/Modules/GameSession/Scripts/Facade/Content/QuestData.cs#L56), [line 80](../../Assets/Modules/GameSession/Scripts/Facade/Content/QuestData.cs#L80)).

**Failure.**
1. A `Temporary` + `Daily` quest is on cooldown from play time 0 until the first cooldown ends, because `LastStartTime` is 0 ([IQuest.cs:53](../../Assets/Modules/Quests/Scripts/IQuest.cs#L53)). After that, the cooldown never applies again. The restart rule always allows `Temporary`, so the timer starts a new copy every 10 seconds while the quest is eligible.
2. `TimeSinceQuestStart` inside a `Temporary` quest is met at once: the start time reads as 0.
3. `TimeSinceLastCompletion`, `QuestCompleted`, and the seed counter never see a `Temporary` quest. Every instance at the same star gets the same seed.

The built-in `Temporary` quests (5 and 9) are `LocalEncounter` and `Manual` and use none of these features.

**Fix.** Keep start and completion times for `Temporary` quests in session statistics without saving progress. Or reject `Temporary` with `Daily` and with time requirements in the database analyzer.

### Q2. A random node without a default can wait forever

**Severity:** Medium. **Status:** Confirmed (latent).

**Where.** `RandomNode.TryProceed` seeds its generator with `seed + _counter` ([RandomNode.cs:83](../../Assets/Modules/Quests/Scripts/Nodes/RandomNode.cs#L83)). The counter grows only when a transition is taken. When the weights add up to less than 1, the roll can fall into the "no pick" part. Without `DefaultTransition` the node then returns `false` and keeps the counter ([line 112](../../Assets/Modules/Quests/Scripts/Nodes/RandomNode.cs#L112)).

**Failure.** At the next event the node rolls the same value and fails again. The quest stays at this node until the set of met transitions changes. With unconditional transitions it never changes, so the quest never ends. The quest log shows the node's message or nothing. The only built-in random node (quest 100, node 5) has zero weights and always picks.

**Fix.** Increase the counter on a miss as well, or treat the remainder as "wait and roll again". Make the analyzer require `DefaultTransition` when the weights add up to less than 1.

### Q3. The manager misses dialogs that become available

**Severity:** Medium. **Status:** Confirmed (latent).

**Where.** On any event other than its own button, `TextNode.TryProcessEvent` recomputes `ActionRequired` and returns `false` ([TextNode.cs:82](../../Assets/Modules/Quests/Scripts/Nodes/TextNode.cs#L82)). `Quest.TryProcessEvent` then returns `false` ([Quest.cs:67](../../Assets/Modules/Quests/Scripts/Quest.cs#L67)), and `ProcessQuestEvent` does not queue the quest ([QuestManager.cs:322](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L322)). `QuestManager.ActionRequired` changes only in `OnQuestUpdated` and `FindActiveQuest` ([line 174](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L174), [line 217](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L217)).

**Failure.** A dialog whose actions all have requirements starts as `InProgress`. When the player later meets a requirement, `Quest.Status` becomes `ActionRequired`, but the manager fires no `QuestActionRequiredSignal`. The dialog opens only when another quest happens to trigger `FindActiveQuest`. Until then the quest looks stuck. Every built-in dialog has at least one action without a requirement, so built-in quests are not affected.

**Fix.** Let `Quest.TryProcessEvent` report a change of `Status`, not only a node change. Or make `QuestManager.ActionRequired` a computed property over the quest list.

### Q4. AbandonQuest leaves a stale active quest

**Severity:** Low. **Status:** Plausible.

**Where.** `AbandonQuest` calls `CompleteQuest` with the quest's current status ([QuestManager.cs:83](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L83)). `CompleteQuest` has no case for `ActionRequired`, so the `default` branch logs an exception ([line 254](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L254)). Neither method resets `_activeQuest` or `ActionRequired`.

**Failure.**
1. Abandoning a quest whose status is `ActionRequired` (for example after Q3) logs "QuestManager: Error has occured" for a normal player action.
2. When the abandoned quest was the active quest, `ActionRequired` stays true. The next `StarMapState.UpdateQuests` calls `InvokeAction` on the removed quest and can open its dialog. The dialog buttons then reach no quest.

The quest log is usually opened from the active star map, where an action-required quest shows its dialog at once. That makes the case rare.

**Fix.** Treat `ActionRequired` like `InProgress` in `CompleteQuest`. Call `FindActiveQuest` in `AbandonQuest`.

### Q5. A finished temporary quest does not refresh the quest list

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** `CompleteQuest` returns early for `Temporary` quests, before `ProcessQuestEvent(Timer)` and `FireQuestsUpdatedEvent` ([QuestManager.cs:240](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L240)).

**Failure.** A named `Temporary` quest stays in the quest log after it ends, until another change refreshes the list. The other quests also miss the follow-up timer event. The built-in `Temporary` quests have no `Name`, so the log hides them anyway.

**Fix.** Skip only the storage calls for `Temporary` quests.

### Q6. Origin and Level are ignored for Manual and GameStart quests

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** `StartQuest` and `LoadQuests` use the current star as the quest star ([QuestManager.cs:133](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L133), [line 114](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L114)). They do not call the quest giver and do not check `Level`. `LoadQuests` also skips the start requirement.

**Failure.** A `Manual` or `GameStart` quest with `Origin` `HomeStar` or `RandomFactionBase` starts at the player's star. Region nodes then act on the wrong region. The editor shows `Origin` and `Level` for these quests without effect. Only quest 200 sets `Origin`, and it is a `Daily` quest.

**Fix.** Route both paths through `QuestGiver` and the same checks as `QuestCollection`, or hide the fields for these start conditions.

### Q7. Star 0 means "current star" in region requirements

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** `FactionReputationRequirement`, `StarbasePowerRequirement`, and `StarbaseCaprturedRequirement` read `_starId > 0 ? GetStarData(_starId) : CurrentStar` ([FactionReputationRequirement.cs:28](../../Assets/Modules/Quests/Scripts/Requirements/FactionReputationRequirement.cs#L28), [StarbasePowerRequirement.cs:25](../../Assets/Modules/Quests/Scripts/Requirements/StarbasePowerRequirement.cs#L25), [StarbaseCapturedRequirement.cs:17](../../Assets/Modules/Quests/Scripts/Requirements/StarbaseCapturedRequirement.cs#L17)). Star 0 is a real star: the player's starting star.

**Failure.** The quest star is 0 for `Origin` `HomeStar`, for `RandomStar` when no star exists at the distance, and for any quest that starts while the player is at star 0. This includes `GameStart` quests in a new game. In these quests the three requirements follow the player instead of the quest region. This is the same sentinel that causes [DB15.25](database.md#db15-smaller-defects) for start requirements. No built-in quest combines star 0 with these requirements.

**Fix.** Pass "current star" explicitly (for example `-1` or a flag) and keep 0 as a normal star ID.

### Q8. Daily selection ignores weight and shortens the cooldown

**Severity:** Low. **Status:** Confirmed.

**Where.**
- The timer picks with `CreateFirstAvailable` ([QuestManager.cs:313](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L313)), which takes the first allowed quest in catalog order ([QuestCollection.cs:26](../../Assets/Modules/Quests/Scripts/Manager/QuestCollection.cs#L26)).
- The cooldown jitter is rolled again at each check ([IQuest.cs:51](../../Assets/Modules/Quests/Scripts/IQuest.cs#L51)). The seed contains the play-time minute ([QuestManager.cs:311](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L311)), so it changes every minute. One `System.Random` serves all quests in the loop ([QuestCollection.cs:54](../../Assets/Modules/Quests/Scripts/Manager/QuestCollection.cs#L54)), so each quest's jitter also depends on the quests before it.

**Failure.**
1. When two `Daily` quests are eligible, the one with the lower catalog position always starts. `Weight` only sets the cooldown.
2. The cooldown ends at the first minute whose roll is below the elapsed time. The effective cooldown is close to 75 % of the nominal value. Built-in quest 200 (`Weight` 0.3, nominal 80 hours) becomes available after about 60 hours.

**Fix.** Derive the jitter from the quest ID and its last start time, so it is fixed per cycle. Decide whether `Daily` should pick by weight, and document the result in the schema.

### Q9. Missing loot and enemy references fail at run time

**Severity:** Medium. **Status:** Confirmed (latent).

**Where.** `LootCache.Get(null)` returns `null` ([Cache.cs:130](../../Assets/Modules/Quests/Scripts/Factory/Cache.cs#L130)). `EnemyCache.Get(null)` returns a `QuestEnemyData` without ships ([Cache.cs:102](../../Assets/Modules/Quests/Scripts/Factory/Cache.cs#L102)). `NodeBuilder` passes these values on without a check.

**Failure.**
1. `ReceiveItem` and `RemoveItem` without `Loot` throw `NullReferenceException` in `LootExtensions` when the action runs ([Loot.cs:21](../../Assets/Modules/Quests/Scripts/Loot/Loot.cs#L21)). The exception leaves `StarMapState.UpdateQuests`. The node already cleared `ActionRequired`, so the quest stays at this node with no action.
2. `HaveItemById` without `Loot` throws on every `IsMet`. As a start requirement it throws inside `QuestCollection.UpdateQuests`, which blocks every quest of that start condition.
3. `Trade` without `Loot` passes `null` to `StartTrading`.
4. `AttackFleet` without `Enemy` starts a battle with a `null` enemy fleet ([QuestCombatModelFacctory.cs:28](../../Assets/Scripts/Domain/Quests/QuestCombatModelFacctory.cs#L28)). The combat behavior with no enemy is not defined.

No built-in node has these gaps.

**Fix.** Validate references in `NodeBuilder` and `RequirementsBuilder`. Build an error node (status `Error`) instead of a broken node, and report the quest and node ID. Add the same check to the database analyzer.

### Q10. Smaller defects

| ID | Where | Defect | Status |
| --- | --- | --- | --- |
| Q10.1 | [FactionRelationsNode.cs:8](../../Assets/Modules/Quests/Scripts/Nodes/FactionRelationsNode.cs#L8), [RemoveItemNode.cs:8](../../Assets/Modules/Quests/Scripts/Nodes/RemoveItemNode.cs#L8), [TradingNode.cs:8](../../Assets/Modules/Quests/Scripts/Nodes/TradingNode.cs#L8), [AttackEnemyNode.cs:23](../../Assets/Modules/Quests/Scripts/Nodes/AttackEnemyNode.cs#L23) | `INode.Type` is wrong. Faction relation nodes report character relation types. `RemoveItem` and `Trade` report `ReceiveItem`. `AttackOccupants` and `AttackStarbase` report `AttackFleet`. Only `Quest.Status` reads `Type`, and only for terminal types, so nothing fails today. Any future consumer gets wrong data. | Confirmed |
| Q10.2 | [ActionNode.cs:6](../../Assets/Modules/Quests/Scripts/Nodes/ActionNode.cs#L6) | `ActionNode` is in the global namespace. The rest of the module uses `Domain.Quests`. | Confirmed |
| Q10.3 | [HolidayManager.cs:82](../../Assets/Scripts/GameServices/HolidayManager.cs#L82), [QuestManager.cs:142](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L142) | `HolidayManager` calls `StartQuest` at every session start in the holiday period. While the `Storyline` holiday quest is active or after it is completed, the restart rule refuses, and `StartQuest` logs an error at each load. | Confirmed |
| Q10.4 | [QuestManager.cs:134](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L134), [QuestFactory.cs:28](../../Assets/Modules/Quests/Scripts/Factory/QuestFactory.cs#L28) | `StartQuest` computes a seed for the requirement check, and `QuestFactory` computes the quest seed again. With `UseRandomSeed` both calls return different random numbers, so the checked requirement and the built quest use different seeds. | Confirmed (latent) |
| Q10.6 | [QuestData.cs:147](../../Assets/Modules/GameSession/Scripts/Facade/Content/QuestData.cs#L147) | A failure also sets `LastCompletionTime`. `TimeSinceLastCompletion` therefore counts from the last completion *or failure*. The name and the schema text say "completion". | Confirmed |
| Q10.7 | [StarbaseCapturedRequirement.cs:5](../../Assets/Modules/Quests/Scripts/Requirements/StarbaseCapturedRequirement.cs#L5), [QuestCombatModelFacctory.cs:10](../../Assets/Scripts/Domain/Quests/QuestCombatModelFacctory.cs#L10) | Misspelled type names: `StarbaseCaprturedRequirement`, `QuestCombatModelFacctory` (see also [application issue 15](application_layer.md#15-inconsistent-naming-and-namespaces)). | Confirmed |
| Q10.8 | [QuestManager.cs:269](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L269) | `BeaconActivated`, `LocalEncounter`, `NewStarSystemSecured`, and `FactionMissionAccepted` return before `ProcessQuestEvent`. Waiting nodes in active quests see these changes only at the next event, up to 10 seconds later. | Confirmed |

Q10.5 (zero-weight fall-through) was resolved by [PR #547](https://github.com/PavelZinchenko/event-horizon-main/pull/547). Q14 records the remaining zero-weight ordering defect.

### Q14. Zero-weight loot fallback always selects by file order

**Severity:** Medium (P2). **Status:** Confirmed (latent).

**Where.** Introduced by [PR #547](https://github.com/PavelZinchenko/event-horizon-main/pull/547), reviewed at `6fa761b5237933f3b13e1e5668cfef3b59c56292`. `WeightedIndex` assigns positive infinity to zero weights ([EnumerableExtensions.cs:287](https://github.com/PavelZinchenko/event-horizon-main/blob/6fa761b5237933f3b13e1e5668cfef3b59c56292/Assets/Modules/CommonComponents/Scripts/Utils/EnumerableExtensions.cs#L287)). Equal keys sort by original index ([line 330](https://github.com/PavelZinchenko/event-horizon-main/blob/6fa761b5237933f3b13e1e5668cfef3b59c56292/Assets/Modules/CommonComponents/Scripts/Utils/EnumerableExtensions.cs#L330)). `RandomItems` takes eligible entries from that order ([Loot.cs:165](https://github.com/PavelZinchenko/event-horizon-main/blob/6fa761b5237933f3b13e1e5668cfef3b59c56292/Assets/Modules/Quests/Scripts/Loot/Loot.cs#L165)).

**Failure.** With three eligible entries weighted `[0, 0, 0]` and `MinAmount = MaxAmount = 1`, every seed selects the first entry. Mods that omit weights or set all weights to zero lose random rewards. When positive-weight entries cannot fill the requested amount, the earliest eligible zero-weight entries always fill the remaining slots.

**Verification.** On 2026-10-21, isolated C# 9 checks compiled the PR's actual helper and collection source. Across 1,000 seeds, selection counts were `1000, 0, 0`; the previous uniform fallback produced `332, 334, 334`. No zero-weight `RandomItems` entries were found in the inspected built-in quest data. No Unity build was run.

**Fix.** Give the zero-weight fallback group a seeded random order. Keep that group after positive-weight entries. Use the original index only to break remaining ties.

The owner accepted merging PR #547 with this defect deferred on 2026-10-21.

## Structural issues

### Q11. No automated tests and no graph validation

**Severity:** Medium.

**Problem.** The module has no test assembly. Quest graphs are checked only when a player reaches each node.

**Evidence.**
- All game state enters through the interfaces in [Context/](../../Assets/Modules/Quests/Scripts/Context/), and all randomness comes from seeds. The module is easy to test with fakes, but no test exists.
- Graph errors appear only at run time: a missing target ends the quest with `Error`, an empty `Condition` throws during the build ([DB15.24](database.md#db15-smaller-defects)), missing references throw during actions (Q9), and a stuck `Random` node waits forever (Q2).
- Q1, Q2, Q3, and Q8 are timing and state rules that a unit test would catch.

**Direction.**
1. Add an EditMode test assembly with fake context providers.
2. Add one test that builds every built-in quest and checks: start node exists, every target exists, every path reaches a terminal node, every reference is set.
3. Move the same checks into the database analyzer, so mod authors get them too.

### Q12. Exceptions escape the event handler

**Severity:** Medium.

**Problem.** No code in the module catches exceptions. A defect in one quest can break event handling for all quests.

**Evidence.**
- `ProcessQuestEvent` loops over all quests ([QuestManager.cs:319](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L319)). An exception in one quest's requirement stops the loop, and the later quests miss the event.
- `Tick` removes a quest from the update queue before it calls `OnQuestUpdated` ([QuestManager.cs:72](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L72)). An exception there drops the quest's save and status update for this change.
- `LoadQuests` builds all saved quests in one loop ([QuestManager.cs:111](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L111)). An exception in one build stops the restore of the rest and the setup of all selection collections.

**Direction.** Catch exceptions per quest in the manager. Put the failing quest into `Error` and continue with the others. Log the quest ID and node ID.

### Q13. QuestManager holds too many responsibilities

**Severity:** Low.

**Problem.** `QuestManager` selects quests, runs graphs, saves progress, chooses the action-required quest for the UI, and computes map beacons in one class with shared mutable state.

**Evidence.**
- Six `QuestCollection` fields with one branch each in `OnQuestEvent` ([QuestManager.cs:265](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L265)). A new start condition needs edits in three places.
- `ActionRequired` and `_activeQuest` are cached UI state that several paths must keep in sync (Q3, Q4).
- `IQuestActionProcessor` lives in `StarMapState`, so quest side effects need the star map state ([application issue 5](application_layer.md#5-game-flow-uses-an-ambiguous-global-exitsignal-and-in-memory-callbacks)).

**Direction.**
1. Map `StartCondition` to a collection in a dictionary.
2. Compute the action-required quest on demand from the quest list.
3. Move persistence calls into a small service that observes quest status changes.

## Accepted behavior

### Loot requirements are evaluated again after loading

PR #547 adds requirements to individual loot entries. Loading a saved quest rebuilds its loot objects. Their first evaluation checks requirements against the current game state ([Loot.cs:90](https://github.com/PavelZinchenko/event-horizon-main/blob/6fa761b5237933f3b13e1e5668cfef3b59c56292/Assets/Modules/Quests/Scripts/Loot/Loot.cs#L90), [line 165](https://github.com/PavelZinchenko/event-horizon-main/blob/6fa761b5237933f3b13e1e5668cfef3b59c56292/Assets/Modules/Quests/Scripts/Loot/Loot.cs#L165)).

The save retains the seed, not the resolved rewards. State-dependent requirements can therefore change receive, remove, or trade loot after a reload, even with the same seed. Loot stays cached after its first evaluation during that session.

The owner accepted this behavior on 2026-10-16. Seeded loot changes caused by replacing the sampling algorithm were also accepted.
