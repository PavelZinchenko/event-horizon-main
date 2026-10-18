# StatUpgradeTemplate

`StatUpgradeTemplate` is an unused table. It describes a level-based price for stat upgrades, but the built-in catalog has no records, and the game never reads one ([DB11](../../issues/database.md#db11-schema-fields-without-runtime-effect)).

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `32` |
| Kind | collection, unused |
| Folder | none in the built-in catalog |
| Records | 0 |
| Schema | [StatUpgradeTemplate.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/StatUpgradeTemplate.xml) |
| Generated class | `GameDatabase.DataModel.StatUpgradeTemplate` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/StatUpgradeTemplate.cs)) |

## Fields

Each record also has the three [common fields](index.md#common-fields). The meanings come from the names only. No code gives them an effect.

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `MaxLevel` | int | 20, 0… | Highest upgrade level. |
| `Stars` | expression `IntToInt(level)` | | Price in stars for an upgrade level. |
| `Credits` | expression `IntToInt(level)` | | Price in credits for an upgrade level. |
| `Resources` | expression `IntToInt(level)` | | Price in resources for an upgrade level. |

## Notes

### Code references

The loader accepts the records and keeps them in `IDatabase.StatUpgradeTemplateList`. No game code reads that list or the fields.

### Data rules

1. Do not add records. They load and have no effect.
