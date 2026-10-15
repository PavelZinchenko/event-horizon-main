# Undefined

`Undefined` is the value 0 of the [ItemType](enums.md#itemtype) enum. It is not a table. A JSON record without an `ItemType` field gets this value.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `0` |
| Kind | enum default, not a table |
| Schema | none |
| Generated class | none |

## Notes

### Loading

The loader reads `ItemType` first and has no branch for 0. A record with `ItemType` 0, or without the field, makes the loader throw `Unknown file type` ([DatabaseContent.cs:571](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/Storage/DatabaseContent.cs#L571)). The effect depends on the storage, as for bad JSON: see [Storage backends](../../database.md#storage-backends).

### Data rules

1. Give every record an `ItemType`, including settings records.
2. Keep only database records as `*.json` files in a folder mod. The folder storage reads every JSON file in the folder tree.
