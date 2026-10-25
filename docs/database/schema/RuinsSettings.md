# RuinsSettings

Ruins settings define which satellites ruins loot can drop and how large a satellite's layout may be at a star level. The ruins loot generator reads both.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `120` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/RuinsSettings.json`](../../../Assets/Modules/Database/Resources/Database/Settings/RuinsSettings.json) |
| Schema | [RuinsSettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/RuinsSettings.xml) |
| Generated class | `GameDatabase.DataModel.RuinsSettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/RuinsSettings.cs)), read as `IDatabase.RuinsSettings` |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The *Built-in* column shows the value in the built-in catalog; empty means the schema default.

| Field | Type | Default / range | Built-in | Meaning |
| --- | --- | --- | --- | --- |
| `ExcludedSatellites` | reference list [Satellite](Satellite.md) | | the four `Titan` satellites | Satellites ruins never drop. |
| `MaxSatelliteCells` | expression `IntToInt(level)` | 0…1000, default `MAX(6, FLOOR(level/2))` | | The largest satellite layout, in cells, that can drop at a star level ([LootGenerator.cs:295](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L295)). |

## Notes

### Old exclusion rules

Before database version 1.8 the ruins generator excluded every satellite of class `Titan` and capped the layout at the hard-coded `MAX(6, level / 2)` cells. Migration 1.7 -> 1.8 moves the exclusion into the list and the cap into the expression. The default formula reproduces the old cap, including integer rounding.

### Mod rules

1. A mod settings record replaces the whole record. A mod that ships this record but omits an inherited exclusion re-enables that satellite in ruins.
2. The migration of a 1.7 mod writes this record with the mod's own exclusions plus the inherited built-in ones, unless the mod sets `UnloadOriginalDatabase`.
