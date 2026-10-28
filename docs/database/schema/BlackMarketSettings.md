# BlackMarketSettings

Black market settings define which satellites the smuggler market never sells. The market reads the exclusion list when it builds its stock.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `119` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/BlackMarketSettings.json`](../../../Assets/Modules/Database/Resources/Database/Settings/BlackMarketSettings.json) |
| Schema | [BlackMarketSettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/BlackMarketSettings.xml) |
| Generated class | `GameDatabase.DataModel.BlackMarketSettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/BlackMarketSettings.cs)), read as `IDatabase.BlackMarketSettings` |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The *Built-in* column shows the value in the built-in catalog; empty means the schema default.

| Field | Type | Default / range | Built-in | Meaning |
| --- | --- | --- | --- | --- |
| `ExcludedSatellites` | reference list [Satellite](Satellite.md) | | the four `Titan` satellites | Satellites the black market never sells ([BlackMarketInventory.cs:71](../../../Assets/Scripts/Legacy/Game/Quests/Inventory/BlackMarketInventory.cs#L71)). |

## Notes

### Old exclusion rules

Before database version 1.8 the market excluded every satellite of class `Titan` through a hard-coded condition, plus each satellite's `NotAvailableInBlackMarket` flag. Migration 1.7 -> 1.8 moves both rules into this list. The flag is obsolete and read only by the migration.

### Mod rules

1. A mod settings record replaces the whole record. A mod that ships this record but omits an inherited exclusion re-enables that satellite in the market.
2. The migration of a 1.7 mod writes this record with the mod's own exclusions plus the inherited built-in ones, unless the mod sets `UnloadOriginalDatabase`.
