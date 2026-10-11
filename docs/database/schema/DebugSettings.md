# DebugSettings

Debug settings turn on the in-game log console and define numeric codes that give the player loot. The player types a code in the developer console of the main menu. The debug console logger and the cheat code processor read debug settings.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `108` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/Debug.json`](../../../Assets/Modules/Database/Resources/Database/Settings/Debug.json) |
| Schema | [DebugSettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/DebugSettings.xml) |
| Generated class | `GameDatabase.DataModel.DebugSettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/DebugSettings.cs)), read as `IDatabase.DebugSettings` |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The *Built-in* column shows the value in the built-in catalog; empty means the schema default.

| Field | Type | Default / range | Built-in | Meaning |
| --- | --- | --- | --- | --- |
| `Codes` | struct list [DebugCode](#debugcode) | | empty | Loot codes for the developer console. |
| `EnableDebugConsole` | bool | | `true` | `true`: the log console is on for every catalog. `false`: it is on only for an editable catalog (Unity Editor or folder mod) ([DebugConsoleLogger.cs:136](../../../Assets/ModulesShared/Gui/Scripts/DebugConsole/DebugConsoleLogger.cs#L136)). |

## Structures

### DebugCode

Schema: [DebugCode.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/DebugCode.xml).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Code` | int | 0…999,999 | The number that the player types. |
| `Loot` | struct [LootContent](structures.md#lootcontent) | | The loot that the code gives. |

## Notes

### Log console

When the console is on, the logger collects Unity exceptions and the messages of the game's diagnostic trace. After an error or a warning, the console shows a button that opens the log ([DebugConsole.cs:29](../../../Assets/ModulesShared/Gui/Scripts/DebugConsole/DebugConsole.cs#L29)). The logger checks the setting each time the database loads ([DebugConsoleLogger.cs:119](../../../Assets/ModulesShared/Gui/Scripts/DebugConsole/DebugConsoleLogger.cs#L119)).

### Loot codes

1. The developer console sends the typed number to the cheat processor ([DevConsoleViewModel.cs:86](../../../Assets/Scripts/Legacy/GUI/ViewModel/MainMenu/DevConsoleViewModel.cs#L86)).
2. The processor checks the database codes first, before the hard-coded cheats ([Cheats.cs:58](../../../Assets/Scripts/Legacy/Debug/Cheats.cs#L58)). Database codes need no device key and work in release builds.
3. The first code with the typed number wins ([DatabaseCodesProcessor.cs:21](../../../Assets/Scripts/Legacy/Debug/DatabaseCodesProcessor.cs#L21)).
4. The processor creates the loot as a quest loot with quest seed 0, star 0, and level 0 ([DatabaseCodesProcessor.cs:37](../../../Assets/Scripts/Legacy/Debug/DatabaseCodesProcessor.cs#L37)). Random items therefore have level 0, and the same code always gives the same random result.
5. The player gets the items at once, and the loot window shows them ([DatabaseCodesProcessor.cs:28](../../../Assets/Scripts/Legacy/Debug/DatabaseCodesProcessor.cs#L28)).

Loot types and their rules: [LootContent](structures.md#lootcontent), [Loot: Item creation](Loot.md#item-creation).

### Data rules

1. Give each code a unique `Code`. Only the first one with a number works.
2. Use fixed amounts and fixed items. Level-based types such as `SomeMoney` and `RandomComponents` use level 0.
3. A code can be typed again. Do not use codes for one-time rewards.
4. A mod settings record replaces the whole record. Copy every value, not only the changed one ([DB7](../../issues/database.md#db7-settings-records-cannot-be-partly-overridden)).
