# Faction

A faction owns star regions (territories), sends wandering enemy ships, sells goods, and has its own research tree. The galaxy generator, the enemy-fleet factories, merchants, loot, exploration, quests, and the research and workshop screens read faction records.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `14` |
| Kind | collection |
| Folder | [`Resources/Database/Faction/`](../../../Assets/Modules/Database/Resources/Database/Faction/) |
| Records in the built-in catalog | 20 |
| Schema | [Faction.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Faction.xml) |
| Generated class | `GameDatabase.DataModel.Faction` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Faction.cs), [hand-written part](../../../Assets/Modules/Database/Scripts/DataModel/Faction.cs)) |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Name` | string | | Faction name, usually a localization key (`$Faction1`). The game shows it on the star map, in the workshop, shipyard, encyclopedia, and research screens. |
| `Color` | color | white | Faction color: region color on the star map, faction icons, research points. A missing or invalid value gives white. |
| `NoTerritories` | bool | | The faction owns no star regions. When `SkillSettings.BeatAllEnemiesFactionList` is empty, the "beat all enemies" skill requirement skips this faction. |
| `HomeStarDistance` | int | 0…5000 | Lower bound of the star level at which a region can belong to this faction. `0` means no lower bound. Also the minimum star level for this faction's research points and merchant goods (see [Notes](#distance-ranges)). |
| `HomeStarDistanceMax` | int | 0…5000 | Upper bound of the territory range. `0` means no upper bound. Only territory selection reads it. |
| `NoWanderingShips` | bool | | The faction sends no random enemy fleets. Its ship builds do not appear in random fleets, challenges, or exploration encounters. |
| `WanderingShipsDistance` | int | 0…5000 | Lower bound of the star level at which ships of this faction appear as random enemies. `0` means no lower bound. |
| `WanderingShipsDistanceMax` | int | 0…5000 | Upper bound of the wandering-ships range. `0` means no upper bound. |
| `HideFromMerchants` | bool | | Merchants do not sell components, ships, or research items of this faction. |
| `HideResearchTree` | bool | | The research screen and the encyclopedia do not show this faction. The faction gives no research points as loot. |
| `NoMissions` | bool | | The faction is hostile. Its starbases offer no faction missions, the star map hides the reputation panel, and the quest requirement `IsHostileFaction` is true in its regions. |
| `Hidden` | bool | obsolete | Old flag. Migration 1.0 → 1.1 converts it to `NoTerritories`, `NoWanderingShips`, `HideFromMerchants`, and `HideResearchTree` ([DatabaseUpgrader_0_1.cs:85](../../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_0_1.cs#L85)). |
| `Hostile` | bool | obsolete | Old flag. Migration 1.0 → 1.1 converts it to `NoMissions`. |

## Notes

### The neutral faction

A reference to a faction that does not exist, including `0` and `-1`, resolves to `Faction.Empty`. This is the neutral faction: ID 0, gray, name `$NeutralFaction` ([Faction.cs:10](../../../Assets/Modules/Database/Scripts/DataModel/Faction.cs#L10)). It is not in `FactionList`. `FactionsWithEmpty` adds it in front of the list. The research screen, research points, and merchants use `FactionsWithEmpty`, so neutral technologies have their own research tree. Region selection uses `FactionList`, so the neutral faction never owns a region. See [Null references](../../database.md#null-references).

`Faction.Empty` does not run `OnDataDeserialized`, so its two ranges contain only level 0. A ship build whose build faction and hull faction are both neutral therefore never passes the wandering-ships filter. No built-in enemy build has this combination. See [DB15.11](../../issues/database.md#db15-smaller-defects).

### Distance ranges

A star's level is its distance from the galaxy center. The hand-written part builds two ranges after load ([Faction.cs:13](../../../Assets/Modules/Database/Scripts/DataModel/Faction.cs#L13)):

- `HomeStarRange` = `HomeStarDistance` … `HomeStarDistanceMax`.
- `WanderingShipsRange` = `WanderingShipsDistance` … `WanderingShipsDistanceMax`.

A bound of `0` becomes "no bound". When the lower bound is greater than the upper bound, the range swaps them. Both bounds are inclusive.

Different consumers read different parts of these fields:

| Consumer | Rule | Code |
| --- | --- | --- |
| Region owner | A random faction from `!NoTerritories` and `HomeStarRange` contains the region's home-star level. The seed is home star + game seed. The choice is stored in the session. | [Region.cs:113](../../../Assets/Scripts/Legacy/Game/Galaxy/Region.cs#L113), [Factions.cs:24](../../../Assets/Modules/Database/Scripts/Extensions/Factions.cs#L24) |
| Random enemies, challenges | `!NoWanderingShips` and `WanderingShipsRange` contains the star level. The faction is `ShipBuild.Faction` (build faction, else hull faction). | [ShipBuildQuery.cs:148](../../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs#L148) |
| Exploration encounters | Same rule, for planets without an owner. The hull faction is used. Infected planets and neutral hulls always pass. | [ExplorationData.cs:252](../../../Assets/Scripts/Domain/Exploration/ExplorationData.cs#L252) |
| Research points as loot | `!HideResearchTree` and `HomeStarDistance` ≤ level. The upper bound is ignored. | [Factions.cs:14](../../../Assets/Modules/Database/Scripts/Extensions/Factions.cs#L14), [LootGenerator.cs:345](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L345) |
| Black market ships | Not `HideFromMerchants`, and `WanderingShipsDistance` ≤ level (when it has wandering ships) or `HomeStarDistance` ≤ level. Upper bounds are ignored. | [BlackMarketInventory.cs:84](../../../Assets/Scripts/Legacy/Game/Quests/Inventory/BlackMarketInventory.cs#L84) |
| Quest filter `AllAvailable` | Fleet: wandering-ships rule. Loot: merchant rule with `HomeStarDistance` ≤ level. Research: research rule. Base: territory rule. | [FactionFilter.cs:28](../../../Assets/Modules/Quests/Scripts/Utils/FactionFilter.cs#L28) |

Every star level needs at least one faction with territories. When no faction fits, `Region.Faction` gets `null` and throws. Built-in factions 1, 2, and 6 have no territory bounds, so the built-in catalog is safe. See [DB15.12](../../issues/database.md#db15-smaller-defects).

### Other consumers

- Merchants: [`FactionExtensions.ValidForMerchants`](../../../Assets/Modules/Database/Scripts/Extensions/Factions.cs#L19) (research items in shops, [LootGenerator.cs:242](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L242)) and [`ComponentList.AvailableForMerchants`](../../../Assets/Modules/Database/Scripts/Extensions/ComponentList.cs#L20) (components by `Component.Faction`).
- Research screen and encyclopedia: [`FactionExtensions.WithTechTree`](../../../Assets/Modules/Database/Scripts/Extensions/Factions.cs#L9), used by [ResearchPanelViewModel.cs:42](../../../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/ResearchPanelViewModel.cs#L42) and [FactionsPanel.cs:30](../../../Assets/Scripts/Gui/Ehopedia/FactionsPanel.cs#L30).
- Missions: [FactionPanelViewModel.cs:61](../../../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/FactionPanelViewModel.cs#L61) (no missions) and [line 97](../../../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/FactionPanelViewModel.cs#L97) (reputation panel), [HostileFactionRequirement.cs](../../../Assets/Modules/Quests/Scripts/Requirements/HostileFactionRequirement.cs).
- Skills: the "beat all enemies" requirement uses `SkillSettings.BeatAllEnemiesFactionList`, or every faction without `NoTerritories` when the list is empty ([Skills.cs:63](../../../Assets/Scripts/Domain/Skills/Skills.cs#L63)).
- Starbase defense: [FactionsSettings](FactionsSettings.md), not this table.
- Test: [DatabaseTests.cs:64](../../../Assets/Modules/Database/Tests/DatabaseTests.cs#L64) requires ships for every faction that has territories or wandering ships.

### Data rules

1. Give each faction with territories or wandering ships at least one enemy ship build of that faction.
2. Keep at least one faction with territories at every star level (see [Distance ranges](#distance-ranges)).
3. A faction that should not exist in the galaxy (for example, a quest-only faction) needs all four flags: `NoTerritories`, `NoWanderingShips`, `HideFromMerchants`, `HideResearchTree`. Built-in factions 14 and 15 use this pattern.
4. Prefer `0` over `-1` for "no faction" in references. See [DB4](../../issues/database.md#db4-null-references-have-two-sentinels-and-id-0-is-not-protected).
