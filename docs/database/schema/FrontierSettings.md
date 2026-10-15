# FrontierSettings

Item type 104 is reserved for Frontier settings. No schema and no code use it.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `104` |
| Kind | reserved (settings range) |
| Schema | none |
| Generated class | none |

## Notes

### Loading

The schema has no file for this value, so the generated loader has no branch for it. A JSON record with this `ItemType` makes the loader throw `Unknown file type` ([DatabaseContent.cs:571](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/Storage/DatabaseContent.cs#L571)). The effect depends on the storage, as for bad JSON: see [Storage backends](../../database.md#storage-backends).

### Background

The Frontier names match another game of the same developer; the announcement window links to it ([AnnouncementWindow.cs:36](../../../Assets/Scripts/Gui/Dialogs/AnnouncementWindow.cs#L36)). This project has no code for Frontier records.

### Data rules

1. Do not use this item type. A record with it stops the load or is skipped, depending on the storage.
