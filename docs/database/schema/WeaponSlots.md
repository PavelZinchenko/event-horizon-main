# WeaponSlots

Weapon slot settings give a name and an icon to each weapon slot letter. The component list of the ship editor groups weapons by these letters. The letters themselves live on [Component](Component.md) (`WeaponSlotType`) and [Barrel](structures.md#barrel) (`WeaponClass`).

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `118` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/WeaponSlots.json`](../../../Assets/Modules/Database/Resources/Database/Settings/WeaponSlots.json): letters `C`, `L`, `M`, `T`, `S` |
| Schema | [WeaponSlots.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/WeaponSlots.xml) |
| Generated class | `GameDatabase.DataModel.WeaponSlots` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/WeaponSlots.cs)), read as `IDatabase.WeaponSlots` |
| Consumer | `WeaponNode` ([ComponentTreeNode.cs:143](../../../Assets/ModulesShared/Gui/Scripts/ComponentList/ComponentTreeNode.cs#L143)) |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`.

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Slots` | struct list [WeaponSlot](#weaponslot) | | One weapon group per letter, in list order. |
| `DefaultSlotName` | string | `$GroupWeaponAny` | Name of the last group. It holds weapons without a slot letter and weapons with an unknown letter. |
| `DefaultSlotIcon` | image (`GuiIcon`) | `icon_weapon_x` | Icon of the last group. |

## Structures

### WeaponSlot

Schema: [WeaponSlot.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/WeaponSlot.xml).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Letter` | char | | The slot letter, case-sensitive. A duplicate letter logs `Duplicate weapon slot` and is skipped. |
| `Name` | string | | Group name, usually a localization key. |
| `Icon` | image (`GuiIcon`) | | Group icon. |

## Notes

### Grouping

`WeaponNode` builds one group per `Slots` entry and a last default group ([ComponentTreeNode.cs:147](../../../Assets/ModulesShared/Gui/Scripts/ComponentList/ComponentTreeNode.cs#L147)). It puts each weapon component into the group of its `WeaponSlotType` ([line 173](../../../Assets/ModulesShared/Gui/Scripts/ComponentList/ComponentTreeNode.cs#L173)). A weapon without a letter goes to the default group. A weapon with a letter that has no entry logs `Undefined weapon slot` and also goes to the default group.

This table only names the groups. It does not decide which weapon fits which barrel. The fit rule compares `Component.WeaponSlotType` with `Barrel.WeaponClass` directly (see [Barrel](structures.md#barrel)). A letter that is missing here still works in combat and in the ship editor.

The workshop item panel shows the raw `WeaponClass` letters of a ship's barrels, not the names from this table ([ItemDescriptionPanel.cs:153](../../../Assets/Scripts/Legacy/GUI/ViewModel/Craft/ItemDescriptionPanel.cs#L153)).

### Data rules

1. Add an entry for every letter that a component uses in `WeaponSlotType`. Otherwise the weapon appears in the default group with an error log.
2. Do not repeat a letter.
3. A mod settings record replaces the whole record ([DB7](../../issues/database.md#db7-settings-records-cannot-be-partly-overridden)).
