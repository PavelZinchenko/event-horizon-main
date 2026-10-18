# QuestItem

A quest item is a cargo resource: a commodity (minerals, scraps, alloys), an artifact, or the Christmas snowflakes. The player holds a count per item. Quests give, take, and check quest items, exploration and loot generators give commodities, and shops buy and sell them.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `20` |
| Kind | collection |
| Folder | [`Resources/Database/Quests/Items/`](../../../Assets/Modules/Database/Resources/Database/Quests/Items/) |
| Records in the built-in catalog | 10: commodities 5, 6, 7, 11, 12, 13, 14, 16; artifact 30; snowflakes 25 |
| Schema | [QuestItem.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/QuestItem.xml) |
| Generated class | `GameDatabase.DataModel.QuestItem` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/QuestItem.cs)) |
| Runtime wrapper | [`ArtifactItem`](../../../Assets/Scripts/Domain/Economy/ItemType/ArtifactItem.cs) (item type `a<ID>`) |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Name` | string | | Item name, usually a localization key. |
| `Description` | string | | Item description, usually a localization key. |
| `Icon` | image (`ArtifactIcon`) | | Item icon. The game looks in `Resources/Textures/Artifacts/` first, then in the database images ([ResourceLocator.cs:72](../../../Assets/Modules/ResourceLocator/Scripts/ResourceLocator.cs#L72)). The built-in folder has `artifact`, `eye`, `scull`, `snowflake`, `stone`, `tubes`. |
| `Color` | color | white | Icon tint. A missing or invalid value gives white. The color of item 25 is also the UI color of snowflake prices ([UiTheme.cs:346](../../../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs#L346)). |
| `Price` | int | 0…999,999,999 | Value in credits ([ArtifactItem.cs:25](../../../Assets/Scripts/Domain/Economy/ItemType/ArtifactItem.cs#L25)). Shops scale it with their own price factor. Artifact 30 and snowflakes 25 have `0`. |

## Notes

### Storage

The session stores a count per quest item ID ([PlayerResources.cs:55](../../../Assets/Scripts/Domain/Player/PlayerResources.cs#L55)). Giving an item adds to the count, and taking it subtracts ([ArtifactItem.cs:28](../../../Assets/Scripts/Domain/Economy/ItemType/ArtifactItem.cs#L28)). The player can give away at most the current count. The cargo hold lists every stored ID that still has a record ([CargoHoldInventory.cs:39](../../../Assets/Scripts/Legacy/Game/Quests/Inventory/CargoHoldInventory.cs#L39)).

| Consumer | Use |
| --- | --- |
| Loot type `QuestItem` | Gives items ([Loot](Loot.md)). |
| Requirement `HaveQuestItem` | The count is at least `Amount` ([ArtifactRequirement.cs:15](../../../Assets/Modules/Quests/Scripts/Requirements/ArtifactRequirement.cs#L15)). |
| Loot generator (exploration, wrecks, hives) | Gives commodities by the fixed IDs of `CommodityType` ([LootGenerator.cs:425](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L425)). |
| Snowflakes | `PlayerResources.Snowflakes` is the count of item 25 ([PlayerResources.cs:94](../../../Assets/Scripts/Domain/Player/PlayerResources.cs#L94)). The Christmas shop takes snowflakes as currency. |

### Hard-coded IDs

The code refers to these records by ID ([DB15.10](../../issues/database.md#db15-smaller-defects)):

| ID | Name | Code |
| ---: | --- | --- |
| 5 | Toxic waste | [CommodityType.cs:3](../../../Assets/Scripts/Legacy/Economy/Data/CommodityType.cs#L3) |
| 6 | Scraps | same |
| 7 | Polymers | same |
| 11 | Minerals | same |
| 12 | Gem stones | same |
| 13 | Alloys | same |
| 14 | Precious metals | same |
| 16 | Alien artifacts | same |
| 25 | Snowflakes | [PlayerResources.cs:94](../../../Assets/Scripts/Domain/Player/PlayerResources.cs#L94), [UiTheme.cs:346](../../../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs#L346) |

### Data rules

1. Keep records 5, 6, 7, 11, 12, 13, 14, 16, and 25. The loot generator and the snowflake currency need them.
2. Remember that `Price` 0 makes an item worth 0 credits in shops.
3. Do not reuse the ID of a removed item for a different item. Old saves keep the count of the ID.
