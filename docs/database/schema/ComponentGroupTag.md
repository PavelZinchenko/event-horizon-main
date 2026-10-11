# ComponentGroupTag

A component group tag limits how many components of one group a ship may install. Components join a group through `Restrictions.ComponentGroupTag` on [Component](Component.md). The built-in tags allow one device of a kind per ship: one cloaking device, one teleporter, one energy shield, and so on. The ship editor, fleet validation, and the ship builder read the limit.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `35` |
| Kind | collection |
| Folder | [`Resources/Database/Component/Tags/`](../../../Assets/Modules/Database/Resources/Database/Component/Tags/) |
| Records in the built-in catalog | 14, all with limit 1 |
| Schema | [ComponentGroupTag.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/ComponentGroupTag.xml) |
| Generated class | `GameDatabase.DataModel.ComponentGroupTag` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/ComponentGroupTag.cs)) |

## Fields

The record also has the [common fields](index.md#common-fields). The tag has no name. The ID is its identity.

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `MaxInstallableComponents` | int | 1… | The maximum number of components with this tag on one ship, satellites included. The loader raises `0` to 1. |

## Notes

### Counting

Every installed component with the tag counts once, whichever component it is. The count covers the hull and both satellites. `MaxComponentAmount` on the component is a separate limit per component; both apply.

| Consumer | Rule | Code |
| --- | --- | --- |
| Ship editor | A component cannot be placed when the group is full. | [ComponentTracker.cs:41](../../../Assets/ModulesShared/ShipEditor/Scripts/Model/ComponentTracker.cs#L41) |
| Fleet validation | Removes the components beyond the limit and returns them to the inventory. | [ShipValidator.cs:152](../../../Assets/Scripts/Domain/Player/ShipValidator.cs#L152) |
| Ship builder | Skips the components beyond the limit, in list order. A skipped incompatible component still counts ([DB15.18](../../issues/database.md#db15-smaller-defects)). | [ShipBuilder.cs:93](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L93) |

### Origin

Migration 1.6 → 1.7 created the table ([DatabaseUpgrader_6_7.cs:39](../../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_6_7.cs#L39)). It turned each old `UniqueComponentTag` string into a tag. It also gave each device component a tag by device class. `Stealth` and `SuperStealth` share one tag, and so do `EnergyShield` and `PartialShield`. The limit is the highest `MaxComponentAmount` in the group, at least 1. The migration numbers new tags from 1, so it assumes an empty table.

### Built-in tags

| ID | Components (device class) |
| ---: | --- |
| 1 | Afterburner (`Accelerator`) |
| 2 | Braking system (`Brake`) |
| 3 | Drone and missile camouflage (`DroneCamouflage`, `MissileCamouflage`) |
| 4 | Cloaking system (`Ghost`) |
| 5 | Detonator |
| 6 | ECM jammer (`Decoy`) |
| 7 | Energy shield and frontal shield (`EnergyShield`, `PartialShield`) |
| 8 | Fortification system |
| 9 | Gravity bomb (`GravityGenerator`) |
| 10 | Infinity stone (`TimeMachine`) |
| 11 | Point defense |
| 12 | Repair bots |
| 13 | Stealth field and super stealth |
| 14 | Boss teleporters (`Teleporter`) |

Cloning centers and the `TeleporterV2` component have no tag.

### Data rules

1. Use a tag for a "one of a kind per ship" rule. A tag works across different components, `MaxComponentAmount` does not.
2. Put a device component into the group of its device class. Two devices of one class on a ship both work, which is often not intended.
3. A changed or removed tag ID silently removes the limit from every component that references it.
