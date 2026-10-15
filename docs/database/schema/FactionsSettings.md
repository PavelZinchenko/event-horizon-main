# FactionsSettings

Faction settings define the defense power of faction starbases: the start value per region, the lowest value, and the loss for each defeated enemy fleet. The power sets the size of the starbase fleet. Only the region model reads these settings.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `111` |
| Kind | settings (singleton) |
| File | none in the built-in catalog. The loader creates the record with the schema defaults. |
| Schema | [FactionsSettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/FactionsSettings.xml) |
| Generated class | `GameDatabase.DataModel.FactionsSettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/FactionsSettings.cs)), read as `IDatabase.FactionsSettings` |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The built-in catalog uses the schema defaults.

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `StarbaseInitialDefense` | expression `IntToInt(distance)` | `MIN(1000, 300 + 5*distance)`, 1…100,000 | Start power of a region's starbase, in percent, for the level of the region's home star. The game uses it until the power first changes, then the session stores the value ([Region.cs:86](../../../Assets/Scripts/Legacy/Game/Galaxy/Region.cs#L86)). |
| `StarbaseMinDefense` | int | 50, 1… | The lowest power. A change below it sets it ([Region.cs:92](../../../Assets/Scripts/Legacy/Game/Galaxy/Region.cs#L92)). It does not limit the start value. |
| `DefenseLossPerEnemyDefeated` | int | 10, 0… | Power points that the region loses when the player destroys the occupants of one of its stars or defeats a boss there ([Region.cs:41](../../../Assets/Scripts/Legacy/Game/Galaxy/Region.cs#L41), [Occupants.cs:49](../../../Assets/Scripts/Domain/Galaxy/StarContent/Occupants.cs#L49), [Boss.cs:65](../../../Assets/Scripts/Domain/Galaxy/StarContent/Boss.cs#L65)). |

## Notes

### What the power does

The power *P* is a percentage. The starbase fleet of a region has ([Fleet.cs:73](../../../Assets/Scripts/Legacy/Model/Factories/Fleet.cs#L73)):

- *P* / 25 random ships of the region faction (`Common` and `Rare` builds that fit the home-star level),
- *P* / 100 flagships of class `Class1`, or up to `Class2` when there are 2 or more,
- one starbase build of the faction, or `GalaxySettings.DefaultStarbaseBuild`.

With the defaults, a region at level 0 starts at 300 % (12 ships, 3 flagships), and a region from level 140 on starts at 1000 % (40 ships, 10 flagships). The star map and the faction panel show the power ([FactionPanelViewModel.cs:99](../../../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/FactionPanelViewModel.cs#L99)). The starbase reward contains 3 + *P* / 400 research points of the region faction ([LootGenerator.cs:235](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L235)).

The quest nodes `SetFactionStarbasePower` and `ChangeFactionStarbasePower` change the power, and the requirement `FactionStarbasePower` reads it ([Quest: Node types](Quest.md#node-types)).

### Data rules

1. Keep `StarbaseInitialDefense` at least `StarbaseMinDefense`. The minimum does not apply to the start value.
2. Values below 25 give a starbase without escort ships.
3. A mod settings record replaces the whole record. Copy every value, not only the changed one ([DB7](../../issues/database.md#db7-settings-records-cannot-be-partly-overridden)).
