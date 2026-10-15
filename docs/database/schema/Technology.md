# Technology

A technology is one entry in a faction's research tree. Researching it lets the player craft one component, ship, or satellite in a workshop. A researched component technology also lets the player unlock that component on a ship. The research screen, the workshop, the ship editor, and loot (blueprints) read technology records.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `10` |
| Kind | collection, switch on `Type` |
| Folder | [`Resources/Database/Technology/`](../../../Assets/Modules/Database/Resources/Database/Technology/) |
| Records in the built-in catalog | 361: 216 `Component`, 120 `Ship`, 25 `Satellite` |
| Schema | [Technology.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Technology.xml) |
| Generated class | Abstract `GameDatabase.DataModel.Technology` with `Technology_Component`, `Technology_Ship`, `Technology_Satellite` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Technology.cs)) |
| Runtime wrapper | `ITechnology`: `ComponentTechnology`, `ShipTechnology`, `SatelliteTechnology` ([Technology.cs](../../../Assets/Scripts/Domain/Technologies/Technology.cs)), built by the [`Technologies`](../../../Assets/Scripts/Domain/Technologies/Technologies.cs) service on every database load |

## Fields

The record also has the [common fields](index.md#common-fields). The *Case* column lists the `Type` values for which a field applies. An empty cell means all values.

| Field | Type | Case | Default / range | Meaning |
| --- | --- | --- | --- | --- |
| `Type` | enum [TechType](enums.md#other) | | `Component` | The kind of unlocked item. Selects the generated class. |
| `ItemId` | ref [Component](Component.md) | `Component` | required | The component that the technology unlocks. Model name `Component`. The loader throws when it is missing. |
| `ItemId` | ref [Ship](Ship.md) | `Ship` | required | The hull that the technology unlocks. Model name `Ship`. |
| `ItemId` | ref [Satellite](Satellite.md) | `Satellite` | required | The satellite that the technology unlocks. Model name `Satellite`. |
| `Faction` | ref [Faction](Faction.md) | `Component`, `Satellite` | neutral | The research tree that holds the technology, and the faction whose research points pay for it. `0` or a missing faction means the neutral tree. A ship technology uses the hull's `Ship.Faction` instead, and the loader ignores this field. |
| `Price` | int | | 0…10000 | Research-point cost. A value of `0` makes the technology free: it is researched at the start of every session. `Price` is also the base of the workshop level (see `CustomCraftingLevel`). |
| `Hidden` | bool | | | The tree shows the technology as a locked placeholder (`?????`, hidden icon, no price) until it is researched. The player cannot research it with points. It comes only as a blueprint (loot, quests, shops). |
| `Special` | bool | | | The technology is never available for research and is not drawn in the tree. Random blueprint loot never contains it. It comes only from explicit sources: `Loot` blueprints, the Christmas shop. A technology that depends on a `Special` technology can never be researched. |
| `DoesnPreventUnlocking` | bool | `Component` | | The ship editor can unlock this component on a ship before the technology is researched. Without the flag, unlocking needs the researched technology. The name is misspelled in the schema; keep it. |
| `Dependencies` | ref list [Technology](Technology.md) | | | Technologies that must be researched first. Every dependency must also be available itself (recursive). The runtime wrapper calls the list `Requirements`. |
| `CustomCraftingLevel` | int | | 0… | Workshop level needed to craft the item. `0` uses the computed level: `Price` + the workshop levels of all dependencies (recursive). No built-in record sets it. |

## Notes

### Research

The [`Research`](../../../Assets/Scripts/Domain/Research/Research.cs) service tracks researched technologies and research points per faction.

1. Each faction, including the neutral faction, has its own pool of research points. The player gets points from research items in loot and shops.
2. A technology is *available* when it is not `Special` and every dependency is researched and available ([Research.cs:150](../../../Assets/Scripts/Domain/Research/Research.cs#L150)).
3. `ResearchTech` checks availability, takes `Price` points from the technology's faction pool, and stores the technology ID in the session ([Research.cs:65](../../../Assets/Scripts/Domain/Research/Research.cs#L65)).
4. A blueprint calls `ResearchTechForFree`. It researches the technology without the availability check and adds `Price` to the stored points, so the player keeps the points ([Research.cs:39](../../../Assets/Scripts/Domain/Research/Research.cs#L39), [BlueprintItem.cs:30](../../../Assets/Scripts/Domain/Economy/ItemType/BlueprintItem.cs#L30)).
5. On session load, the service marks every technology with `Price` ≤ 0 as researched ([Research.cs:120](../../../Assets/Scripts/Domain/Research/Research.cs#L120)). It then computes the free points as stored points minus the prices of all researched technologies ([Research.cs:136](../../../Assets/Scripts/Domain/Research/Research.cs#L136)). A changed `Price` therefore changes the free points of existing saves.

The research screen ([TechItemViewModel.cs:45](../../../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/TechItemViewModel.cs#L45), [TechTreePanelViewModel.cs:59](../../../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/TechTreePanelViewModel.cs#L59)) shows one tree per faction from `FactionsWithEmpty` without `HideResearchTree`. The layout puts a technology one row below its deepest dependency ([TechTreePanelViewModel.cs:293](../../../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/TechTreePanelViewModel.cs#L293)). It only sees technologies of the same faction that are not `Special`. A dependency in another faction or on a `Special` technology is never placed, so the dependent technology is missing from the tree. The layout then loops 1000 times and logs `BuildTechTree: bad tech data`. See [DB15.13](../../issues/database.md#db15-smaller-defects).

### Blueprints in loot

Random loot picks from technologies that are *available* and not yet researched ([`LootGenerator`](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs)):

| Source | Pool | Code |
| --- | --- | --- |
| Outposts | Owner faction, `Hidden` or `Price` ≤ 10 | [LootGenerator.cs:126](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L126) |
| Hives (infected planets) | `ExplorationSettings.InfectedPlanetFaction`, `Hidden` or `Price` ≤ 10 | [LootGenerator.cs:158](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L158) |
| Captured starbase (30 % chance) | Region faction, `Hidden` or `Price` ≤ 10 | [LootGenerator.cs:247](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L247) |
| Space worm (30 % chance) | Any faction, `Price` ≤ 15 | [LootGenerator.cs:279](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L279) |

[Loot](Loot.md) records with item type `Blueprint` give a fixed technology. The Christmas shop sells technologies 264 and 277–282 by hard-coded ID ([SantaInventory.cs:40](../../../Assets/Scripts/Legacy/Game/Quests/Inventory/SantaInventory.cs#L40), [DB15.10](../../issues/database.md#db15-smaller-defects)).

### Workshop

A starbase workshop lists researched technologies whose faction is the region faction or neutral ([CraftDialog.cs:108](../../../Assets/Scripts/Gui/Craft/CraftDialog.cs#L108), [Technologies.cs:116](../../../Assets/Scripts/Domain/Technologies/Technologies.cs#L116)). The required workshop level is computed in three steps:

1. Base level: `CustomCraftingLevel`, or `Price` + the base levels of all dependencies ([ITechnology.cs:99](../../../Assets/Scripts/Domain/Technologies/ITechnology.cs#L99)).
2. Quality bonus: Improved +max(1, level/10), Excellent +max(3, level/7), Superior +max(5, level/5) ([ITechnology.cs:24](../../../Assets/Scripts/Domain/Technologies/ITechnology.cs#L24)).
3. Skill: the `CraftingLevel` skill subtracts `SkillSettings.CraftingLevelReduction` ([CraftPanel.cs:115](../../../Assets/Scripts/Gui/Craft/CraftPanel.cs#L115)).

The crafting cost in credits, stars, and technology points does not come from this table. It comes from the crafted item: [Price.cs](../../../Assets/Modules/Database/Scripts/Extensions/Price.cs) and `GetCraftPrice` in [Technology.cs](../../../Assets/Scripts/Domain/Technologies/Technology.cs#L77). `SatelliteTechnology.CreateItem` accepts only Common quality.

The shipyard upgrade of a ship (an extra modification slot) uses the same level formula. The base level is the workshop level of the hull's technology, or 50 when the hull has no technology. The shipyard must also belong to the hull faction, unless the hull is neutral ([UnlockSlotPanel.cs:44](../../../Assets/Scripts/Gui/Craft/UnlockSlotPanel.cs#L44)).

### Unlocking components

The ship editor can unlock a locked component on a ship only when the component has a technology, and that technology is researched or has `DoesnPreventUnlocking` ([ShipEditorState.cs:92](../../../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs#L92)). A component without any technology can never be unlocked.

### Data rules

1. Create one technology per item. `TryGetComponentTechnology` and `TryGetShipTechnology` return the first match and ignore the rest ([Technologies.cs:41](../../../Assets/Scripts/Domain/Technologies/Technologies.cs#L41)).
2. Keep dependencies in the same faction tree, and never depend on a `Special` technology.
3. Avoid dependency cycles. The loader accepts them, but research and the workshop-level formula recurse forever. See [DB12](../../issues/database.md#db12-the-loader-does-not-detect-reference-cycles).
4. A changed or removed technology ID breaks saves: the session stores researched technology IDs, and unknown IDs are skipped with a log message.
5. Do not set `Faction` on ship technologies. 87 built-in ship technologies carry it; all values match the hull faction, but the game ignores them.
