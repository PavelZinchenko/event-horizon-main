# Economy

[Game design index](index.md)

## Purpose

The economy connects activity rewards to fleet preparation and persistent growth.
Different resources constrain travel, purchases, research, and construction in different ways.

## Player actions

1. Collect encounter, quest, and exploration rewards.
2. Keep useful equipment and sell eligible surplus items.
3. Reserve fuel for travel and exploration.
4. Choose between research and crafting uses for faction points.
5. Buy, craft, unlock, or develop equipment at suitable services.

## Current rules

### Resources

| Resource | Sources | Uses |
| --- | --- | --- |
| Credits | Combat rewards, sales, quests, and configured loot | Markets, crafting, equipment unlocking, and ship services |
| Stars | Purchases and selected activity rewards | Premium-priced goods and services, crafting, perks, and repeat exploration |
| Tokens | Arena victories | Arena shop |
| Fuel | Starting allocation and configured fuel rewards or supplies | Ordinary travel and planet-exploration entry |
| Faction research points | Research rewards from bosses, some fleets, exploration, and quests | Technologies and some crafting or perk-slot costs |
| Commodities and quest items | Combat, exploration, trade, and quests | Sales or specific quest requirements and exchanges |
| Snowflakes | Christmas rewards | Seasonal shop |

Experience is progression rather than a general currency.
The perk-slot service does consume ship levels; that exception is described in [Progression](progression.md#ship-perks-and-perk-slots).

Fuel is capped by the player's fuel capacity.
Awarding fuel through the resource service can therefore discard any amount beyond the cap.
Fuel consumption fails when the requested amount is unavailable, except for the explicit [short-travel exception](galaxy.md#travel).

### Reward delivery

Combat handles loot and experience separately.
The encounter determines whether each requires victory, is always granted, or is disabled.
Random combat loot is generated from destroyed enemy ships, using location and faction context.
Special rewards are added independently of random loot, but still require the encounter's loot condition to pass.

Ordinary ship loot can include credits, components, scrap commodities, toxic waste, and seasonal items.
Flagships can provide additional component and premium rewards.
Research items and other special rewards can be attached by the encounter builder.
The exact catalog and quantities remain in loot data and generation rules.

Planet exploration grants loot immediately for each completed objective.
Quest nodes can grant or remove items as part of their flow.
These delivery differences matter when predicting the economic cost of failure.

### Trade and services

Inventory providers define what each service offers.
Ordinary merchants, faction services, black markets, arena shops, and seasonal shops have different catalogs and currencies.
The trading skill changes merchant prices through the configured price factor.
Unique or otherwise ineligible ships cannot use the standard ship-sale path.

The black market provides an alternative source of ships, satellites, and modified equipment.
The workshop manufactures items from technologies.
The shipyard expands and repaints hulls.
Military bases sell ship levels where the build permits those locations.
Quest flow can control access to faction services.

### Crafting

Crafting needs an available researched item, a sufficient workshop level, and the displayed resource price.
The price can combine credits, stars, and points from the item's faction.
Crafting skills reduce prices and required workshop levels.
Resources are consumed when the item is created; this path has no production wait timer.

The player selects a quality where the item supports it.
Higher quality changes the cost and can increase workshop requirements.
Ship quality determines empty perk-slot count.
Component quality selects an allowed modification and quality range.
Satellites are crafted only at common quality through the current interface.

Crafting and research compete for the same available faction points when a recipe charges points.
The decision is between expanding future options and obtaining an immediate item.

### Price types and build differences

The normal component unlock cost is twice its credit price.
Perk installation uses a premium price of 10 units in the current modification factory.
Other costs depend on the item, service level, quality, or progression state.

Under `IAP_DISABLED`, prices created through `Price.Premium(n)` become `500 × n` credits.
This conversion also affects rewards created through that price helper.
It is not evidence that every direct use of the star balance has the same conversion.
In particular, purchase-disabled `CraftingPrice` currently drops its star portion without adding the corresponding credit amount.
Its constructor clears that value before conversion. This is a code-level discrepancy, not established design intent.
Build-specific economic behavior belongs to [Platforms and mods](platforms_and_mods.md#purchases-and-economic-variants).

## System connections

[Galaxy](galaxy.md) owns travel and exploration entry costs.
[Progression](progression.md) owns research eligibility, ship development, and skill effects.
[Activities](activities.md) owns reward opportunities and repeat conditions.
[Factions and quests](factions_and_quests.md) controls some service access and item exchanges.

## Design intent and open questions

Rewards should reflect challenge, distance, and player commitment.
Costs should create useful choices without trapping the player.
Unwanted loot should retain useful sale or exchange value.
That intent does not imply a universal salvage or recycling action for every item.

Open questions:

1. Can the player recover from an unsuccessful expedition without excessive repetition or premium spending?
2. Do purchase-enabled and purchase-disabled builds offer comparable progression choices after currency conversion?
3. Does spending research points on quality crafting compete fairly with unlocking technologies?
4. Should purchase-disabled crafting convert its star portion to credits, as the general premium-price helper does?

## Sources

- [PlayerResources](../../Assets/Scripts/Domain/Player/PlayerResources.cs): balances, fuel cap, and consumption.
- [LootGenerator](../../Assets/Scripts/Domain/Economy/LootGenerator.cs) and [CombatReward](../../Assets/Scripts/Combat/Domain/CombatReward.cs): reward creation and conditions.
- [Market inventory providers](../../Assets/Scripts/Legacy/Game/Quests/Inventory/): service-specific goods and trade behavior.
- [Technology](../../Assets/Scripts/Domain/Technologies/Technology.cs) and [CraftPanel](../../Assets/Scripts/Gui/Craft/CraftPanel.cs): quality prices and resource payment.
- [CraftingPrice](../../Assets/Scripts/Domain/Technologies/ITechnology.cs): combined prices and the purchase-disabled discrepancy.
- [Price](../../Assets/Modules/CommonComponents/Scripts/Economy/Price.cs): premium conversion.
- [ShipConstructor](../ship_constructor.md) and [Economy save data](../game_session/schema/economy.md): sale and perk rules, currencies, and persistence.
