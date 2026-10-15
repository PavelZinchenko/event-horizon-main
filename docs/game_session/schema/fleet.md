# Fleet Domain

This page describes the saved player fleet and the ship editor presets: the objects `FleetData` and `ShipPresetsData`, and the structs `ShipInfo`, `ShipComponentInfo`, `SatelliteInfo`, `HangarSlotInfo`, and `ShipPresetInfo`. For the XML format and the type rules, see [Save schema](index.md). For the module overview, see [GameSession module](../../game_session.md).

## Contents

- [Overview](#overview)
- [FleetData](#fleetdata)
- [ShipInfo](#shipinfo)
- [ShipComponentInfo](#shipcomponentinfo)
- [SatelliteInfo](#satelliteinfo)
- [HangarSlotInfo](#hangarslotinfo)
- [ShipPresetsData](#shippresetsdata)
- [ShipPresetInfo](#shippresetinfo)
- [Load and save of the fleet](#load-and-save-of-the-fleet)

## Overview

The save does not hold live ships. It holds a description of each ship: the hull ID, the installed components, the satellites, and the ship's perks. [`PlayerFleet`](../../../Assets/Scripts/Domain/Player/PlayerFleet.cs) builds `IShip` objects from these structs on load and writes them back before each save. The ship editor does the same with presets. The conversions are in the hand-written partials in [`Scripts/Model/`](../../../Assets/Modules/GameSession/Scripts/Model/).

```text
FleetData
├── ExplorationShipId ─────────────► index into Ships
├── Ships: list<ShipInfo>
│     ├── Components: list<ShipComponentInfo>
│     ├── Satellite1, Satellite2: SatelliteInfo
│     │     └── Components: list<ShipComponentInfo>
│     ├── LayoutModifications: list<byte>
│     └── Modifications: list<long>
└── HangarSlots: list<HangarSlotInfo> ─► (squad slot, index into Ships)

ShipPresetsData
└── Presets: list<ShipPresetInfo>
      ├── Components: list<ShipComponentInfo>
      └── Satellite1, Satellite2: SatelliteInfo
```

## FleetData

The player's ships, the active squad, and the exploration ship.

| | |
| --- | --- |
| Schema | [`v3/Objects/FleetData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/FleetData.xml) |
| Generated class | [`Model.FleetData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/FleetData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IFleetData` / `FleetData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/FleetData.cs), property `ISessionData.Fleet` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `ExplorationShipId` | `int` | `-1` | Index into `Ships` of the ship used for planet exploration. −1 means none. It is a list index, not a database ID. |
| `Ships` | `list<ShipInfo>` | empty | All player ships, in `PlayerFleet` order. |
| `HangarSlots` | `list<HangarSlotInfo>` | empty | The occupied slots of the active squad. |

### Facade API

| Member | Behavior |
| --- | --- |
| `ExplorationShipId` | Get and set. |
| `Ships` | The model list. Changes through it are tracked. |
| `Hangar` | The model list `HangarSlots`. |
| `UpdateShips(IEnumerable<IShip>)` | Converts each ship to a `ShipInfo` and assigns the result with `ObservableList.Assign` and the generated `ShipInfo.EqualityComparer` ([FleetData.cs:28](../../../Assets/Modules/GameSession/Scripts/Facade/Content/FleetData.cs#L28)). `Assign` keeps the leading entries that are equal and replaces every entry from the first difference on. It marks the session dirty only if something changed ([ObservableList.cs:47](../../../Assets/ModulesShared/Utilites/Serialization/ObservableList.cs#L47)). |

### Signals

None.

### Consumers

| Consumer | Use |
| --- | --- |
| [`PlayerFleet`](../../../Assets/Scripts/Domain/Player/PlayerFleet.cs#L106) | The only writer. See [Load and save of the fleet](#load-and-save-of-the-fleet). |
| [`Skills.IsFree`](../../../Assets/Scripts/Domain/Skills/Skills.cs#L53) | Reads `ShipInfo.Experience` to check whether any ship reached the maximum level. The value is the one from the last save ([GS5.16](../../issues/game_session.md#gs5-smaller-defects)). |

## ShipInfo

One player ship.

| | |
| --- | --- |
| Schema | [`v3/Types/ShipInfo.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Types/ShipInfo.xml) |
| Generated struct | [`Model.ShipInfo`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/ShipInfo.cs#L14) |
| Hand-written partial | [`Model/ShipInfo.cs`](../../../Assets/Modules/GameSession/Scripts/Model/ShipInfo.cs) |
| Facade | Through `IFleetData.Ships` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Id` | `int` | — | `Ship` record ID: the hull. |
| `Name` | `string` | — | Player-given ship name. |
| `ColorScheme` | `long` | — | Packed `ShipColorScheme`. See [Color scheme](#color-scheme). |
| `Experience` | `long` | `encrypted` | Ship experience points. Held in memory as `ObscuredLong`. |
| `Components` | `list<ShipComponentInfo>` | — | Components installed on the hull. |
| `LayoutModifications` | `list<byte>` | — | Cells the player added to the hull layout. See [Layout modifications](#layout-modifications). |
| `Modifications` | `list<long>` | — | Ship perks. Each value is `(type << 32) \| seed` ([ShipModificationExtensions.cs:9](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ShipModificationExtensions.cs#L9)). `type` is a `ModificationType` value; `seed` makes the perk's random parameters reproducible. |
| `Satellite1`, `Satellite2` | `SatelliteInfo` | — | The two satellite slots. ID 0 means an empty slot. |

### Conversions

| Direction | Code | Behavior |
| --- | --- | --- |
| `IShip` → `ShipInfo` | [ShipInfo.cs:14](../../../Assets/Modules/GameSession/Scripts/Model/ShipInfo.cs#L14) | Copies the hull ID, name, color scheme, experience, components, layout, perks, and satellites. All collections get the given callback. |
| fields → `ShipInfo` | [ShipInfo.cs:27](../../../Assets/Modules/GameSession/Scripts/Model/ShipInfo.cs#L27) | Used by the legacy loader. |
| `ShipInfo` → `IShip` | [ShipInfo.cs:50](../../../Assets/Modules/GameSession/Scripts/Model/ShipInfo.cs#L50) | Builds a `ShipModel` from the hull, applies perks and layout, then a `CommonShip` with components and satellites. Returns `null` when the database has no such hull. Unknown components and satellites are skipped. Clears `DataChanged` on the new ship. |

#### Color scheme

`ShipColorScheme.Value` packs six bytes into the low 48 bits ([ShipColorScheme.cs:21](../../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipColorScheme.cs#L21)):

| Bits | 47–40 | 39–32 | 31–24 | 23–16 | 15–8 | 7–0 |
| --- | --- | --- | --- | --- | --- | --- |
| Value | `SchemeType` (0 = Default, 1 = Hsv) | saturation | hue | red | green | blue |

#### Layout modifications

`LayoutModifications.Serialize` writes one byte per customizable cell inside the hull's layout rectangle, row by row ([LayoutModifications.cs:179](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/LayoutModifications.cs#L179)). The list is empty when the player added no cells. On load, `Deserialize` tries the current format and falls back to an older one ([LayoutModifications.cs:68](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/LayoutModifications.cs#L68)). The bytes depend on the hull layout in the database. If a mod changes the layout, the stored cells are applied to the new layout.

## ShipComponentInfo

One component installed on a ship, a satellite, or a preset.

| | |
| --- | --- |
| Schema | [`v3/Types/ShipComponentInfo.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Types/ShipComponentInfo.xml) |
| Generated struct | [`Model.ShipComponentInfo`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/ShipComponentInfo.cs#L14) |
| Hand-written partial | [`Model/ShipComponentInfo.cs`](../../../Assets/Modules/GameSession/Scripts/Model/ShipComponentInfo.cs) |
| Facade | — |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Id` | `int` | — | `Component` record ID. |
| `Quality` | `sbyte` | — | [`ModificationQuality`](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/Enums/ModificationQuality.cs): 0 = N3 … 2 = N1, 3 = P1 … 5 = P3. |
| `Modification` | `int` | — | `ComponentMod` record ID. Widened from `sbyte` in place ([Version history](index.md#version-history)). |
| `UpgradeLevel` | `sbyte` | — | Component upgrade level. Always written as 0 ([GS5.14](../../issues/game_session.md#gs5-smaller-defects)). |
| `X`, `Y` | `short` | — | Position of the component in the hull layout grid. |
| `BarrelId` | `sbyte` | — | Index of the hull barrel the weapon is mounted on. |
| `KeyBinding` | `sbyte` | — | Player key or button group for activation. |
| `Behaviour` | `sbyte` | — | Activation mode chosen in the ship editor. |
| `Locked` | `bool` | — | The component cannot be removed in the ship editor. |

### Conversions

| Direction | Code | Behavior |
| --- | --- | --- |
| `IntegratedComponent` → struct | [ShipComponentInfo.cs:12](../../../Assets/Modules/GameSession/Scripts/Model/ShipComponentInfo.cs#L12) | Copies all values except the upgrade level, which it sets to 0 ([line 17](../../../Assets/Modules/GameSession/Scripts/Model/ShipComponentInfo.cs#L17)). |
| fields → struct | [ShipComponentInfo.cs:26](../../../Assets/Modules/GameSession/Scripts/Model/ShipComponentInfo.cs#L26) | Legacy loader. Casts the modification to `sbyte` ([GS5.4](../../issues/game_session.md#gs5-smaller-defects)). |
| struct → `IntegratedComponent` | [ShipComponentInfo.cs:50](../../../Assets/Modules/GameSession/Scripts/Model/ShipComponentInfo.cs#L50) | Looks up the component and the modification. Logs an exception and returns `null` for an unknown component. [`ToIntegratedComponents`](../../../Assets/Modules/GameSession/Scripts/Model/ShipComponentInfo.cs#L66) skips `null` results. |

## SatelliteInfo

A satellite slot of a ship or a preset.

| | |
| --- | --- |
| Schema | [`v3/Types/SatelliteInfo.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Types/SatelliteInfo.xml) |
| Generated struct | [`Model.SatelliteInfo`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/SatelliteInfo.cs#L14) |
| Hand-written partial | [`Model/SatelliteInfo.cs`](../../../Assets/Modules/GameSession/Scripts/Model/SatelliteInfo.cs) |
| Facade | — |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Id` | `int` | — | `Satellite` record ID. 0 means the slot is empty. |
| `Components` | `list<ShipComponentInfo>` | — | Components installed on the satellite. Empty for an empty slot. |

### Conversions

`ISatellite` → struct writes ID 0 and an empty list for `null` ([SatelliteInfo.cs:13](../../../Assets/Modules/GameSession/Scripts/Model/SatelliteInfo.cs#L13)). `ToSatellite` returns `null` for ID 0 and for an unknown satellite, without a log entry ([SatelliteInfo.cs:33](../../../Assets/Modules/GameSession/Scripts/Model/SatelliteInfo.cs#L33)).

## HangarSlotInfo

One occupied slot of the active squad.

| | |
| --- | --- |
| Schema | [`v3/Types/HangarSlotInfo.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Types/HangarSlotInfo.xml) |
| Generated struct | [`Model.HangarSlotInfo`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/HangarSlotInfo.cs#L14) |
| Hand-written partial | [`Model/HangarSlotInfo.cs`](../../../Assets/Modules/GameSession/Scripts/Model/HangarSlotInfo.cs) (field constructor) |
| Facade | Through `IFleetData.Hangar` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Index` | `int` | — | Slot index in the `ShipSquad`. |
| `ShipId` | `int` | — | Index into `FleetData.Ships`. Not a database ID. |

Both fields are positions, so the hangar is valid only together with the `Ships` list of the same save. `PlayerFleet` rewrites the whole hangar list on every fleet save ([PlayerFleet.cs:153](../../../Assets/Scripts/Domain/Player/PlayerFleet.cs#L153)).

## ShipPresetsData

Ship editor presets. New in v3.

| | |
| --- | --- |
| Schema | [`v3/Objects/ShipPresetsData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/ShipPresetsData.xml) |
| Generated class | [`Model.ShipPresetsData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/ShipPresetsData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IShipPresetsData` / `ShipPresetsData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/ShipPresetsData.cs), property `ISessionData.ShipPresets` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Presets` | `list<ShipPresetInfo>` | empty | All presets, for all hulls. |

### Facade API

| Member | Behavior |
| --- | --- |
| `Presets` | The model list. |
| `UpdatePresets(IEnumerable<IShipPreset>)` | Converts and assigns like `UpdateShips`. The new structs get the wrong change callback ([GS5.5](../../issues/game_session.md#gs5-smaller-defects)). |

### Consumers

[`ShipEditorState.PresetStorage`](../../../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs#L107) is the only user. It converts all presets to `IShipPreset` when the ship editor opens ([line 117](../../../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs#L117)) and writes them back with `UpdatePresets` in `Dispose` ([line 137](../../../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs#L137)). A preset with an unknown hull breaks the editor ([GS5.15](../../issues/game_session.md#gs5-smaller-defects)).

## ShipPresetInfo

One saved ship editor preset.

| | |
| --- | --- |
| Schema | [`v3/Types/ShipPresetInfo.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Types/ShipPresetInfo.xml) |
| Generated struct | [`Model.ShipPresetInfo`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/ShipPresetInfo.cs#L14) |
| Hand-written partial | [`Model/ShipPresetInfo.cs`](../../../Assets/Modules/GameSession/Scripts/Model/ShipPresetInfo.cs) |
| Facade | Through `IShipPresetsData.Presets` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Id` | `int` | — | `Ship` record ID: the hull the preset is for. |
| `Name` | `string` | — | Preset name. |
| `Components` | `list<ShipComponentInfo>` | — | Components and their positions. |
| `Satellite1`, `Satellite2` | `SatelliteInfo` | — | Satellites and their components. |

A preset has no color, experience, perks, or layout modifications.

### Conversions

`IShipPreset` → struct ([ShipPresetInfo.cs:12](../../../Assets/Modules/GameSession/Scripts/Model/ShipPresetInfo.cs#L12)). `ToShipPreset` returns `null` for an unknown hull and skips unknown components ([ShipPresetInfo.cs:21](../../../Assets/Modules/GameSession/Scripts/Model/ShipPresetInfo.cs#L21)).

## Load and save of the fleet

[`PlayerFleet`](../../../Assets/Scripts/Domain/Player/PlayerFleet.cs) owns the live fleet. It follows the [domain write-back pattern](../../game_session.md#how-domain-code-uses-the-session).

**Load** ([PlayerFleet.cs:106](../../../Assets/Scripts/Domain/Player/PlayerFleet.cs#L106)), on `SessionDataLoadedSignal`:

1. Convert every `ShipInfo` with `ToShip`. A `null` result or an exception puts `null` at that position of a temporary list.
2. Fill the live collection with the non-null ships. `OnShipAdded` unlocks each hull in [`Statistics`](game.md#statistics).
3. Fill the squad: `squad[slot.Index] = temp[slot.ShipId]`. A slot whose ship failed to load becomes empty.
4. Set the exploration ship from `ExplorationShipId`, or `null` for −1.
5. Clear the change flag.

On `SessionCreatedSignal`, `ShipValidator.RemoveInvalidParts` moves parts that no longer fit into the inventory, and the squad is checked against the player's skills ([PlayerFleet.cs:91](../../../Assets/Scripts/Domain/Player/PlayerFleet.cs#L91)).

**Save** ([PlayerFleet.cs:142](../../../Assets/Scripts/Domain/Player/PlayerFleet.cs#L142)), on `SessionAboutToSaveSignal`, only when a ship, the squad, or the collection changed:

1. `UpdateShips` with the live ships.
2. Clear `HangarSlots`. Add one `HangarSlotInfo(slot, index)` for every non-empty squad slot whose ship is in the fleet.
3. Write the index of the exploration ship, or −1.

**Unknown hulls.** A ship whose hull is missing from the current database is not loaded, but its `ShipInfo` stays in the save until the next fleet save. The next fleet change then writes only the loaded ships, and the unknown ship is removed from the save for good. A player who switches to a mod version without that hull and back loses the ship if the fleet changed in between.

**Index fields.** `ExplorationShipId` and `HangarSlotInfo.ShipId` index the temporary list, which keeps `null` entries. So the indices stay correct when a ship fails to load. An index outside the list throws inside the `SessionDataLoadedSignal` handler; `SessionData` logs the exception, and the fleet stays partly loaded.
