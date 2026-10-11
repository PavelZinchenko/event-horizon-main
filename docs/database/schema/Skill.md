# Skill

Item type 13 is reserved for a skill table. No schema and no code use it. The skill tree comes from a resource file, and its bonuses from [SkillSettings](SkillSettings.md).

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `13` |
| Kind | reserved (collection range) |
| Schema | none |
| Generated class | none |

## Notes

### Loading

The schema has no file for this value, so the generated loader has no branch for it. A JSON record with this `ItemType` makes the loader throw `Unknown file type` ([DatabaseContent.cs:571](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/Storage/DatabaseContent.cs#L571)). The effect depends on the storage, as for bad JSON: see [Storage backends](../../database.md#storage-backends).

### Background

The schema also has an enum `SkillType` that no table uses ([DB11](../../issues/database.md#db11-schema-fields-without-runtime-effect)). The game uses its own `SkillType` enum in code.

### Data rules

1. Do not use this item type. A record with it stops the load or is skipped, depending on the storage.
