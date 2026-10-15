# Economy Domain

This page describes the saved currencies, inventories, shop purchases, in-app purchases, player skills, and research: the objects `Inventory`, `ResourcesData`, `ShopData`, `IapData`, `UpgradesData`, and `ResearchData`, and the structs `InventoryComponentInfo`, `PurchasesMap`, and `PurchaseInfo`. For the XML format and the type rules, see [Save schema](index.md). For the module overview, see [GameSession module](../../game_session.md).

## Contents

- [Inventory](#inventory)
- [InventoryComponentInfo](#inventorycomponentinfo)
- [ResourcesData](#resourcesdata)
- [ShopData](#shopdata)
- [PurchasesMap and PurchaseInfo](#purchasesmap-and-purchaseinfo)
- [IapData](#iapdata)
- [UpgradesData](#upgradesdata)
- [ResearchData](#researchdata)

## Inventory

Components and satellites in the player's storage, with quantities. Installed components are part of the ships; see [Fleet domain](fleet.md).

| | |
| --- | --- |
| Schema | [`v3/Objects/Inventory.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/Inventory.xml) |
| Generated class | [`Model.Inventory`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/Inventory.cs#L13) |
| Hand-written partial | — |
| Facade | [`IInventoryData` / `InventoryData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/InventoryData.cs), property `ISessionData.Inventory` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Components` | `inventory<InventoryComponentInfo>` | empty | Component with its modification and quality → quantity. Changed from packed `long` keys in v2. |
| `Satellites` | `inventory<int>` | empty | `Satellite` record ID → quantity. |

Quantities are held in memory as `ObscuredInt` ([GS5.6](../../issues/game_session.md#gs5-smaller-defects)).

### Facade API

`Components` and `Satellites` return the model inventories. The facade adds no rules.

### Consumers

[`PlayerInventory`](../../../Assets/Scripts/Domain/Player/PlayerInventory.cs) is the only user. It follows the [domain write-back pattern](../../game_session.md#how-domain-code-uses-the-session):

1. **Load** ([PlayerInventory.cs:36](../../../Assets/Scripts/Domain/Player/PlayerInventory.cs#L36)). Each component key is converted with `ToComponentInfo`. Unknown components are logged and skipped. A component whose modification is not valid for it gets a new random modification of the same quality, seeded by `GameData.Seed` ([line 50](../../../Assets/Scripts/Domain/Player/PlayerInventory.cs#L50)). Each satellite ID is looked up in the database.
2. **Save** ([PlayerInventory.cs:66](../../../Assets/Scripts/Domain/Player/PlayerInventory.cs#L66)), on `SessionAboutToSaveSignal`. A changed collection is cleared in the model and written again in full.

Unknown components disappear from the save at the next inventory save. An unknown satellite throws during load and can delete other satellites ([GS6](../../issues/game_session.md#gs6-an-unknown-satellite-in-the-inventory-deletes-stored-satellites)).

## InventoryComponentInfo

The key of the component inventory: one component type with one modification and quality. New in v2.

| | |
| --- | --- |
| Schema | [`v3/Types/InventoryComponentInfo.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Types/InventoryComponentInfo.xml) |
| Generated struct | [`Model.InventoryComponentInfo`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/InventoryComponentInfo.cs#L14) |
| Hand-written partial | [`Model/InventoryComponentInfo.cs`](../../../Assets/Modules/GameSession/Scripts/Model/InventoryComponentInfo.cs) |
| Facade | Through `IInventoryData.Components` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Id` | `uint` | — | `Component` record ID. |
| `Quality` | `byte` | — | [`ModificationQuality`](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/Enums/ModificationQuality.cs), as in [`ShipComponentInfo`](fleet.md#shipcomponentinfo). |
| `Modification` | `uint` | — | `ComponentMod` record ID. Widened from `byte` in place ([Version history](index.md#version-history)). |
| `UpgradeLevel` | `byte` | — | Component upgrade level, 0 to 20 after clamping. Only components converted from v1 saves can have a level above 0 ([SessionLoader_v1.cs:63](../../../Assets/Modules/GameSession/Scripts/Migration/SessionLoader_v1.cs#L63)). |

The generated equality compares all four fields, so two components that differ only in quality are separate inventory entries.

### Conversions

| Direction | Code | Behavior |
| --- | --- | --- |
| fields → struct | [InventoryComponentInfo.cs:11](../../../Assets/Modules/GameSession/Scripts/Model/InventoryComponentInfo.cs#L11) | Parameter order is `(id, modification, quality, upgradeLevel)`. It differs from the schema order. |
| `ComponentInfo` → struct | [InventoryComponentInfo.cs:23](../../../Assets/Modules/GameSession/Scripts/Model/InventoryComponentInfo.cs#L23) | Copies all four values, including the level. |
| struct → `ComponentInfo` | [InventoryComponentInfo.cs:31](../../../Assets/Modules/GameSession/Scripts/Model/InventoryComponentInfo.cs#L31) | Logs an exception and returns `ComponentInfo.Empty` for an unknown component. |

The inventory keeps the upgrade level, but a component installed on a ship loses it ([GS5.14](../../issues/game_session.md#gs5-smaller-defects)).

## ResourcesData

Currencies, fuel, and quest items.

| | |
| --- | --- |
| Schema | [`v3/Objects/ResourcesData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/ResourcesData.xml) |
| Generated class | [`Model.ResourcesData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/ResourcesData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IResourcesData` / `ResourcesData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/ResourcesData.cs), property `ISessionData.Resources` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Money` | `long` | 0, `encrypted` | Credits. |
| `Stars` | `long` | 0, `encrypted` | Stars (premium currency), without the supporter pack bonus. |
| `Fuel` | `int` | 0, `encrypted` | Fuel. `PlayerResources` caps it at the fuel capacity from the player skills. |
| `Tokens` | `int` | 0, `encrypted` | Arena tokens. |
| `Resources` | `inventory<int>` | empty, `encrypted` (ignored) | `QuestItem` record ID → quantity. Also holds snowflakes; see below. |

### Facade API

| Member | Behavior | Signal |
| --- | --- | --- |
| `Money` | Get; set when the value differs. | `MoneyValueChangedSignal(newValue)` |
| `Fuel` | Same. | `FuelValueChangedSignal(newValue)` |
| `Tokens` | Same. | `TokensValueChangedSignal(newValue)` |
| `Stars` | Get returns the stored value plus 100 with the supporter pack. Set stores the value minus that bonus ([ResourcesData.cs:73](../../../Assets/Modules/GameSession/Scripts/Facade/Content/ResourcesData.cs#L73)). | `StarsValueChangedSignal(displayedValue)` |
| `Resources` | Returns a new `ObservableInventory` over the same dictionary, with the facade as callback ([ResourcesData.cs:84](../../../Assets/Modules/GameSession/Scripts/Facade/Content/ResourcesData.cs#L84)). The facade forwards each change to the model and fires the signal. | `ResourcesChangedSignal` |

Changes made directly on `Model.ResourcesData` fire no signal.

### Consumers

[`PlayerResources`](../../../Assets/Scripts/Domain/Player/PlayerResources.cs) wraps the facade without a cache: every read and write goes to the session. It clamps fuel, tokens, and snowflakes to 0 or more, and fuel to the fuel capacity. Prices and rewards reach the session through it ([`PriceExtensions`](../../../Assets/Scripts/Domain/Economy/PriceExtensions.cs)). The Quests module reads quest items through [`InventoryDataProvider`](../../../Assets/Scripts/Domain/Quests/InventoryDataProvider.cs#L16).

### Notes

**Snowflakes.** Snowflakes are not a separate field. `PlayerResources.Snowflakes` reads and writes key 25 of `Resources` ([PlayerResources.cs:43](../../../Assets/Scripts/Domain/Player/PlayerResources.cs#L43)). Key 25 is the built-in `QuestItem` named `$Snowflakes`. A mod that gives ID 25 to another quest item shares one counter between both. The setter uses `SetValue`, which keeps an entry with quantity 0 ([GS5.17](../../issues/game_session.md#gs5-smaller-defects)).

**Stars from purchases.** A star pack adds the same amount to `IapData.Stars` and to `ResourcesData.Stars` ([IapPurchaseProcessor.cs:84](../../../Assets/ModulesPrivate/Services/Scripts/InAppPurchasing/IapPurchaseProcessor.cs#L84)). A new game keeps only the purchased stars; see [New game](../../game_session.md#new-game).

## ShopData

What the player bought in each star's market, and when.

| | |
| --- | --- |
| Schema | [`v3/Objects/ShopData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/ShopData.xml) |
| Generated class | [`Model.ShopData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/ShopData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IShopData` / `ShopData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/ShopData.cs), property `ISessionData.Shop` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Purchases` | `map<int,PurchasesMap>` | empty | Market ID (the star ID) → purchases in that market. |

### Facade API

| Member | Behavior |
| --- | --- |
| `GetPurchase(starId, itemId)` | The stored `PurchaseInfo`, or an empty one (quantity 0, time 0). |
| `SetPurchase(starId, itemId, quantity)` | Stores the quantity with the current game time in hours ([ShopData.cs:31](../../../Assets/Modules/GameSession/Scripts/Facade/Content/ShopData.cs#L31)). Creates the star's map on first use. |
| `NumberOfPurchasedItems(purchase, renewalTime)` | Without renewal (`renewalTime <= 0`): the stored quantity. With renewal: the stored quantity minus one for each full `renewalTime` (ticks) of real time since the purchase, not below 0 ([ShopData.cs:42](../../../Assets/Modules/GameSession/Scripts/Facade/Content/ShopData.cs#L42)). |

### Consumers

| Consumer | Use |
| --- | --- |
| [`MarketQuantityProvider`](../../../Assets/Scripts/Domain/Economy/Products/CommonProduct.cs#L169) | Computes the stock as `maxQuantity − NumberOfPurchasedItems`. A purchase stores the new purchased count with `SetPurchase`. |
| [`ProductFactory.TryCreateRandomComponentProduct`](../../../Assets/Scripts/Domain/Economy/Products/ProductFactory.cs#L71) | Uses `PurchaseInfo.Time` in the seed of a random market component. The item ID is `"component" + slot`. |

### Notes

**Renewal.** One purchased item "comes back" per `renewalTime`. Every purchase stores the current time, so the renewal clock restarts at each purchase. Items that came back before the purchase are not lost: the provider computed them into the new quantity first. The stored time has whole-hour precision, so a renewal can arrive up to one hour early.

**Random component markets.** Because the time is part of the seed, a market slot offers a new random component after each purchase of that slot.

## PurchasesMap and PurchaseInfo

| | `PurchasesMap` | `PurchaseInfo` |
| --- | --- | --- |
| Schema | [`v3/Types/PurchasesMap.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Types/PurchasesMap.xml) | [`v3/Types/PurchaseInfo.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Types/PurchaseInfo.xml) |
| Generated struct | [`Model.PurchasesMap`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/PurchasesMap.cs#L14) | [`Model.PurchaseInfo`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/PurchaseInfo.cs#L14) |
| Hand-written partial | — | [`Model/PurchaseInfo.cs`](../../../Assets/Modules/GameSession/Scripts/Model/PurchaseInfo.cs): constructor `(quantity, time)` |

| Struct | Field | Type | Meaning |
| --- | --- | --- | --- |
| `PurchasesMap` | `Purchases` | `map<string,PurchaseInfo>` | Item ID → last purchase. The item ID is the item type ID, or `"component<slot>"` for random components. |
| `PurchaseInfo` | `Time` | `int` | Whole hours since `GameData.StartTime` of the last purchase. Was `long` UTC ticks in v1. |
| `PurchaseInfo` | `Quantity` | `int` | Number of items bought and not yet renewed at `Time`. |

`PurchasesMap` is a struct that only wraps a map, because a collection cannot hold a collection directly.

## IapData

Real-money purchases.

| | |
| --- | --- |
| Schema | [`v3/Objects/IapData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/IapData.xml) |
| Generated class | [`Model.IapData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/IapData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IIapData` / `IapData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/IapData.cs), property `ISessionData.Purchases` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `RemoveAds` | `bool` | false | The "remove ads" product was bought. |
| `SupporterPack` | `bool` | false | The supporter pack was bought. |
| `Stars` | `int` | 0, `encrypted` | Total stars bought with real money. |

### Facade API

| Member | Behavior | Signal |
| --- | --- | --- |
| `RemoveAds` | Get is true when `RemoveAds`, `SupporterPack`, or `Stars > 0` ([IapData.cs:23](../../../Assets/Modules/GameSession/Scripts/Facade/Content/IapData.cs#L23)). Set stores the flag only. | — |
| `SupporterPack` | Set when the value differs. | `StarsValueChangedSignal` with the new displayed star count. |
| `PurchasedStars` | Get and set `Stars`. Does not change `ResourcesData.Stars`. | — |

### Consumers

| Consumer | Use |
| --- | --- |
| [`IapPurchaseProcessor`](../../../Assets/ModulesPrivate/Services/Scripts/InAppPurchasing/IapPurchaseProcessor.cs#L75) | Sets `SupporterPack` and fires `SupporterPackPurchasedSignal`; adds star packs to `PurchasedStars` and `Resources.Stars`. |
| [`GameDataManager.OnIapDataReceived`](../../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L63) | Restores purchases from the platform store. Adds the missing stars when the store reports more than the session. |
| [`InitializationState`](../../../Assets/Scripts/GameStateMachine/States/InitializationState.cs#L146) | Shows ads only without `RemoveAds`. |
| [`StartingNewGameState`](../../../Assets/Scripts/GameStateMachine/States/StartingNewGameState.cs#L75), [`PlayerFleet.AddSupporterPack`](../../../Assets/Scripts/Domain/Player/PlayerFleet.cs#L174) | Give the supporter pack ship. |
| [`SessionData.CreateNewGame`](../../../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L129) | Copies all three fields into the new game. |

## UpgradesData

Player experience and the skill tree.

| | |
| --- | --- |
| Schema | [`v3/Objects/UpgradesData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/UpgradesData.xml) |
| Generated class | [`Model.UpgradesData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/UpgradesData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IUpgradesData` / `UpgradesData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/UpgradesData.cs), property `ISessionData.Upgrades` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `ResetCounter` | `int` | 0, `encrypted` | Number of skill resets, at most 10. |
| `PlayerExperience` | `long` | 0, `encrypted` | Player (mothership) experience. The player level and the skill points come from it. |
| `Skills` | `set<int>` | empty | Learned skill-tree nodes. The IDs are node IDs from [`Assets/Resources/skills.json`](../../../Assets/Resources/skills.json), not database IDs. |

### Facade API

| Member | Behavior | Signal |
| --- | --- | --- |
| `PlayerExperience` | Get and set. | — |
| `AddSkill(id)`, `HasSkill(id)`, `Skills` | Set operations. | — |
| `ResetCounter` | Read only. | — |
| `ResetSkills()` | Does nothing when no skill is learned. Otherwise clears `Skills`, increments `ResetCounter` up to 10 ([UpgradesData.cs:39](../../../Assets/Modules/GameSession/Scripts/Facade/Content/UpgradesData.cs#L39)). | `PlayerSkillsResetSignal` |

### Consumers

| Consumer | Use |
| --- | --- |
| [`PlayerSkills`](../../../Assets/Scripts/Domain/Player/PlayerSkills.cs) | Caches the experience on load ([line 165](../../../Assets/Scripts/Domain/Player/PlayerSkills.cs#L165)) and writes every change through. Computes skill levels and spent points from `Skills`. Available points = player level − learned common skills. |
| [`SkillTree`](../../../Assets/Scripts/Legacy/GUI/ViewModel/Skills/SkillTree.cs#L134) | Reset price = `ResetCounter × 10` stars. |
| [`PlayerFleet`](../../../Assets/Scripts/Domain/Player/PlayerFleet.cs#L101) | Rechecks the squad on `PlayerSkillsResetSignal`. |

Skill IDs that are not in `skills.json` stay in the set but count for nothing.

## ResearchData

Researched technologies and research points.

| | |
| --- | --- |
| Schema | [`v3/Objects/ResearchData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/ResearchData.xml) |
| Generated class | [`Model.ResearchData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/ResearchData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IResearchData` / `ResearchData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/ResearchData.cs), property `ISessionData.Research` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Technologies` | `set<int>` | empty | `Technology` record IDs the player researched or got for free. Technologies that the database marks as free are not stored. |
| `ResearchPoints` | `map<int,int>` | empty, `encrypted` (ignored, [GS5.6](../../issues/game_session.md#gs5-smaller-defects)) | `Faction` record ID → research points **earned in total**. Spent points are not subtracted here. |

### Facade API

`Technologies`, `AddTechnology(id)`, `RemoveTechnology(id)`, `GetResearchPoints(faction)` (default 0), `SetResearchPoints(faction, points)`.

### Consumers

[`Research`](../../../Assets/Scripts/Domain/Research/Research.cs) is the only user.

1. **Load** ([Research.cs:114](../../../Assets/Scripts/Domain/Research/Research.cs#L114)). Researched = free technologies plus the stored IDs. Unknown IDs are logged and skipped. Available points per faction = stored total − price of every researched technology of that faction, not below 0.
2. **Research** ([Research.cs:65](../../../Assets/Scripts/Domain/Research/Research.cs#L65)). Adds the ID and lowers the cached available points. The stored total does not change.
3. **Free research** ([Research.cs:39](../../../Assets/Scripts/Domain/Research/Research.cs#L39)). Adds the price to the stored total and adds the ID, so the available points stay the same.
4. **Earn points** ([Research.cs:92](../../../Assets/Scripts/Domain/Research/Research.cs#L92)). Adds to the stored total and to the cache.
5. **Forget** ([Research.cs:53](../../../Assets/Scripts/Domain/Research/Research.cs#L53)). Removes the ID. The points come back at the next load.

Because spending is derived from `Technologies`, a technology removed from the database or a changed price recomputes the available points on load.
