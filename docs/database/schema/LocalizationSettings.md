# LocalizationSettings

Localization settings hold the labels for corrosive damage in weapon descriptions. The ship editor and the craft panel show these labels in component descriptions. No other text comes from this record.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `117` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/Localization.json`](../../../Assets/Modules/Database/Resources/Database/Settings/Localization.json) |
| Schema | [LocalizationSettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/LocalizationSettings.xml) |
| Generated class | `GameDatabase.DataModel.LocalizationSettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/LocalizationSettings.cs)), read as `IDatabase.LocalizationSettings` |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The *Built-in* column shows the value in the built-in catalog.

| Field | Type | Default / range | Built-in | Meaning |
| --- | --- | --- | --- | --- |
| `CorrosiveDamageText` | string | `$WeaponDamage` | `$CorrosiveDamage` | Label of the corrosive damage of one shot ([ComponentItem.cs:237](../../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentItem.cs#L237)). |
| `CorrosiveDpsText` | string | `$WeaponDPS` | `$CorrosiveDPS` | Label of the corrosive damage per second of a continuous weapon ([ComponentItem.cs:239](../../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentItem.cs#L239)). |

## Notes

### Where the labels show

Component descriptions list one line per damage type. Kinetic, energy, and heat lines use fixed keys. The corrosive line uses this record ([ComponentItem.cs:217](../../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentItem.cs#L217)). The ship editor and the craft panel use the same description code ([ItemDescriptionPanel.cs:122](../../../Assets/Scripts/Legacy/GUI/ViewModel/Craft/ItemDescriptionPanel.cs#L122)).

The description passes each label to the localization. A `$` key is translated. A missing key falls back to English and logs an error ([LocalizationManager.cs:62](../../../Assets/ModulesShared/Localization/Scripts/Localization/LocalizationManager.cs#L62)). The built-in keys `CorrosiveDamage` and `CorrosiveDPS` exist only in the English and Russian files, so other languages show the English label.

The schema defaults are the generic damage labels. A catalog without this record shows corrosive damage as "Damage".

### Data rules

1. Use `$` localization keys and add the key to every language file of the mod.
2. A mod settings record replaces the whole record. Copy every value, not only the changed one ([DB7](../../issues/database.md#db7-settings-records-cannot-be-partly-overridden)).
