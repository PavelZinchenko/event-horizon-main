# Loot

A loot record is a reusable description of items: credits, fuel, components, ships, blueprints, quest items, and nested random choices. Quest nodes give, take, sell, and preview loot. Quest requirements check that the player owns it. [GalaxySettings](GalaxySettings.md) uses one loot record as the starting inventory.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `16` |
| Kind | collection |
| Folder | [`Resources/Database/Quests/Loot/`](../../../Assets/Modules/Database/Resources/Database/Quests/Loot/) (18), plus [`Settings/StartingInventory.json`](../../../Assets/Modules/Database/Resources/Database/Settings/StartingInventory.json) (ID 12) |
| Records in the built-in catalog | 19 |
| Schema | [Loot.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/Loot.xml), content [LootContent.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/LootContent.xml), [LootItem.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/LootItem.xml) |
| Generated class | `GameDatabase.DataModel.LootModel` (alias of `Loot`) ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Loot.cs), [hand-written part](../../../Assets/Modules/Database/Scripts/DataModel/LootModel.cs)) |
| Runtime | [`Domain.Quests.Loot`](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs), items from [`LootItemFactory`](../../../Assets/Scripts/Domain/Quests/LootItemFactory.cs) |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Loot` | struct [LootContent](structures.md#lootcontent) | `None` | The root of the item description. Use `AllItems`, `RandomItems`, or `ItemsWithChance` to combine several entries. |

The `LootContent` and `LootItem` fields and types are on [Shared structures](structures.md#lootcontent).

## Notes

### Who reads loot

| Consumer | Use | Code |
| --- | --- | --- |
| Quest node `ReceiveItem` | Gives the items. | [LootNode.cs](../../../Assets/Modules/Quests/Scripts/Nodes/LootNode.cs) |
| Quest node `RemoveItem` | Takes the items. | [RemoveItemNode.cs](../../../Assets/Modules/Quests/Scripts/Nodes/RemoveItemNode.cs) |
| Quest node `Trade` | Merchant stock. | [TradingNode.cs](../../../Assets/Modules/Quests/Scripts/Nodes/TradingNode.cs) |
| Quest node `AttackFleet` | Extra battle reward. | [QuestCombatModelFacctory.cs:40](../../../Assets/Scripts/Domain/Quests/QuestCombatModelFacctory.cs#L40) |
| Quest node `ShowDialog` | Item preview in the dialog. | [TextNode.cs](../../../Assets/Modules/Quests/Scripts/Nodes/TextNode.cs) |
| Requirement `HaveItemById` | The player can give away every item ([structures](structures.md#requirement)). | [ItemsRequirements.cs](../../../Assets/Modules/Quests/Scripts/Requirements/ItemsRequirements.cs) |
| `GalaxySettings.StartingInventory` | Given once when a new game starts. | [StartingNewGameState.cs:56](../../../Assets/Scripts/GameStateMachine/States/StartingNewGameState.cs#L56) |
| `Character.Inventory` | Not read ([DB11](../../issues/database.md#db11-schema-fields-without-runtime-effect)). | |

The `HaveItem` requirement and the [DebugSettings](DebugSettings.md) codes hold an inline `LootContent` instead of a loot reference. The runtime wraps it in a loot record with ID −1 ([LootModel.cs:7](../../../Assets/Modules/Database/Scripts/DataModel/LootModel.cs#L7), [RequirementsBuilder.cs:114](../../../Assets/Modules/Quests/Scripts/Factory/RequirementsBuilder.cs#L114), [DatabaseCodesProcessor.cs:37](../../../Assets/Scripts/Legacy/Debug/DatabaseCodesProcessor.cs#L37)). The starting inventory is wrapped the same way ([StartingNewGameState.cs:60](../../../Assets/Scripts/GameStateMachine/States/StartingNewGameState.cs#L60)).

### Random choices

The loot builds its item list once, on first use, with a generator seeded by quest seed + loot ID ([Loot.cs:59](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs#L59)). The level is the quest level, and the faction is the quest region's faction. A wrapped loot (ID −1) uses quest seed − 1. The starting inventory and debug codes have no quest: seed −1, level 0, the neutral faction, and star 0.

Inside one quest, every node and requirement that names the same loot record shares one item list ([Quest: Loot in a quest](Quest.md#loot-in-a-quest)). Two different loot records with the same content give different items, because the seed includes the ID.

### Item creation

| `Type` | Item | Code |
| --- | --- | --- |
| `SomeMoney` | Credits for level L = max(1, quest level × `ValueRatio`): base = 100 + 2 × L, amount = 0.9 × base + random(0…0.2 × base). | [Loot.cs:102](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs#L102), [LootItemFactory.cs:53](../../../Assets/Scripts/Domain/Quests/LootItemFactory.cs#L53), [Distance.cs:17](../../../Assets/Modules/ShipConstructor/Scripts/Maths/Distance.cs#L17) |
| `Fuel`, `Money`, `Stars` | The amount, drawn uniformly from `MinAmount`…`MaxAmount` (both inclusive; swapped when min > max). | [Loot.cs:108](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs#L108) |
| `StarMap` | Reveals the map around the quest star. | [Loot.cs:126](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs#L126) |
| `RandomComponents` | *amount* random `Common` or `Rare` components with `Level` ≤ 1.5 × L, where L = quest level × `ValueRatio`. The component faction must pass `Factions` with the loot rule. Random modifications follow [Component: Random selection](Component.md#random-selection). | [Loot.cs:131](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs#L131) |
| `ResearchPoints` | Points of one random faction (neutral included) that passes `Factions` with the research rule. No faction: nothing. | [Loot.cs:140](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs#L140) |
| `RandomItems` | See [RandomItems](#randomitems). | [Loot.cs:150](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs#L150) |
| `AllItems` | Every entry. | [Loot.cs:183](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs#L183) |
| `ItemsWithChance` | Each entry independently, with probability `Weight`. A weight of 1 or more is certain. | [Loot.cs:188](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs#L188) |
| `QuestItem`, `Component`, `Satellite` | The referenced item, *amount* times. Components have no modification. | [Loot.cs:200](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs#L200) |
| `Ship`, `EmptyShip` | One ship: the build with its components, or the empty hull. | [Loot.cs:206](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs#L206) |
| `Blueprint` | Researches the technology for free when given. | [BlueprintItem.cs:28](../../../Assets/Scripts/Domain/Economy/ItemType/BlueprintItem.cs#L28) |

### RandomItems

`RandomItems` draws *amount* from `MinAmount`…`MaxAmount` and walks `Items` in order. Each entry is picked at most once:

- When all weights are 0, it picks *amount* different entries uniformly.
- Otherwise an entry is picked with probability `Weight × remaining picks / remaining weight`. The result is exactly min(*amount*, number of entries with a positive weight) different entries. An entry with weight 0 is never picked.

*Amount* above the number of entries does not repeat entries. To give one entry several times, nest it again, or use an amount inside the entry.

### Data rules

1. Set `ItemId` for every typed entry. The loader throws on a missing item.
2. Use `ValueRatio` for level-scaled rewards (`SomeMoney`, `RandomComponents`). The fixed-amount types ignore the quest level.
3. Do not reuse one loot record in a quest when the nodes must give different items. Make two records.
