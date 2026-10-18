# SpecialEventSettings

Special event settings switch the Christmas, Easter, and Halloween events on and off, set their date windows, and name the quest that each event starts. Christmas also has battle rules and the snowflake price formula of the Christmas shop. The holiday manager, the galaxy star content, the loot generator, and the Christmas shop read them.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `106` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/SpecialEventSettings.json`](../../../Assets/Modules/Database/Resources/Database/Settings/SpecialEventSettings.json) |
| Schema | [SpecialEventSettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/SpecialEventSettings.xml) |
| Generated class | `GameDatabase.DataModel.SpecialEventSettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/SpecialEventSettings.cs)), read as `IDatabase.SpecialEventSettings` |
| Runtime | [`HolidayManager`](../../../Assets/Scripts/GameServices/HolidayManager.cs) |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The *Built-in* column shows the value in the built-in catalog; empty means the schema default.

| Field | Type | Default / range | Built-in | Meaning |
| --- | --- | --- | --- | --- |
| `EnableXmasEvent` | bool | `true` | | Turns the Christmas event on. |
| `XmasDaysBefore` | int | 24, 0…30 | | Days before December 25 when the event starts. |
| `XmasDaysAfter` | int | 15, 0…30 | | Days after December 25 when the event still runs. |
| `XmasQuest` | ref [Quest](Quest.md) | | 0 | Quest started at the start of a session during Christmas. `0`: none. |
| `XmasCombatRules` | ref [CombatRules](CombatRules.md) | | 8 | Rules of the Christmas tree battle. No fallback: `0` breaks the battle ([DB15.21](../../issues/database.md#db15-smaller-defects)). |
| `ConvertCreditsToSnowflakes` | expression `IntToInt(credits)` | `1 + credits/500`, 1… | | Snowflake price of a component in the Christmas shop, from its credit price ([SantaInventory.cs:67](../../../Assets/Scripts/Legacy/Game/Quests/Inventory/SantaInventory.cs#L67)). |
| `EnableEasterEvent` | bool | | `true` | Turns the Easter event on. |
| `EasterDaysBefore` | int | 0…30 | 0 | Days before Easter Sunday when the event starts. |
| `EasterDaysAfter` | int | 0…30 | 0 | Days after Easter Sunday when the event still runs. |
| `EasterQuest` | ref [Quest](Quest.md) | | 10 | Quest started at the start of a session during Easter. |
| `EnableHalloweenEvent` | bool | | `true` | Turns the Halloween event on. |
| `HalloweenDaysBefore` | int | 0…30 | 5 | Days before October 31 when the event starts. |
| `HalloweenDaysAfter` | int | 0…30 | 1 | Days after October 31 when the event still runs. |
| `HalloweenQuest` | ref [Quest](Quest.md) | | 0 | Quest started at the start of a session during Halloween. `0`: none. |

## Notes

### Date windows

The holiday manager uses the internet time service. An event runs when whole days between now and the event date are at most `DaysBefore` (before the date) or `DaysAfter` (after it) ([HolidayManager.cs:87](../../../Assets/Scripts/GameServices/HolidayManager.cs#L87)). Partial days are cut, so `0` still gives about one day on each side of the date. In January, Christmas uses December 25 of the previous year ([HolidayManager.cs:34](../../../Assets/Scripts/GameServices/HolidayManager.cs#L34)). Easter Sunday is computed for the current year ([HolidayManager.cs:54](../../../Assets/Scripts/GameServices/HolidayManager.cs#L54)).

With the built-in values, Christmas runs from about December 1 to January 9, Easter on Easter Saturday and Sunday, and Halloween from about October 26 to November 1.

### What each event does

| Event | Effect | Code |
| --- | --- | --- |
| All | At the start of a session, the manager starts the event quest. The quest needs `StartCondition` `Manual`, and its restart rule applies: a `Storyline` quest runs only once per game. | [HolidayManager.cs:79](../../../Assets/Scripts/GameServices/HolidayManager.cs#L79) |
| Christmas | Christmas tree stars appear on the star map, with a battle (`XmasCombatRules`, fleet with hard-coded starbase build 232) and the Christmas shop. | [StarData.cs:107](../../../Assets/Scripts/Domain/Galaxy/StarData.cs#L107), [XmasTree.cs:48](../../../Assets/Scripts/Domain/Galaxy/StarContent/XmasTree.cs#L48) |
| Christmas | The Christmas shop sells special blueprints (hard-coded technologies 264, 277–282) and 4–6 components with `P2` modifications for snowflakes. | [SantaInventory.cs:40](../../../Assets/Scripts/Legacy/Game/Quests/Inventory/SantaInventory.cs#L40) |
| Christmas | Some loot gets snowflakes (33 % chance). | [LootGenerator.cs:98](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L98) |
| Easter, Halloween | Only the event quest. | |

Snowflakes are the [QuestItem](QuestItem.md) 25.

### Data rules

1. Set `XmasCombatRules` while `EnableXmasEvent` is on ([DB15.21](../../issues/database.md#db15-smaller-defects)).
2. Give each event quest `StartCondition` `Manual`. Use `Storyline` for a quest that runs once per game. Other types start again at every session start during the event while the quest is not active.
3. Keep technologies 264 and 277–282, ship build 232, and quest item 25 when Christmas is on ([DB15.10](../../issues/database.md#db15-smaller-defects)).
4. A mod settings record replaces the whole record. Copy every value, not only the changed one ([DB7](../../issues/database.md#db7-settings-records-cannot-be-partly-overridden)).
