# PvpSettings

Item type 114 is reserved for PvP settings. No schema and no code use it.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `114` |
| Kind | reserved (settings range) |
| Schema | none |
| Generated class | none |

## Notes

### Loading

The schema has no file for this value, so the generated loader has no branch for it. A JSON record with this `ItemType` makes the loader throw `Unknown file type` ([DatabaseContent.cs:571](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/Storage/DatabaseContent.cs#L571)). The effect depends on the storage, as for bad JSON: see [Storage backends](../../database.md#storage-backends).

### Background

The arena keeps its fight counters in the save, not in the database ([PvpData.cs:12](../../../Assets/Modules/GameSession/Scripts/Facade/Content/PvpData.cs#L12)).

### Data rules

1. Do not use this item type. A record with it stops the load or is skipped, depending on the storage.
