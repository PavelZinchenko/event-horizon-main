# SatelliteBuild

A satellite build is a [Satellite](Satellite.md) with installed components. Ship builds reference satellite builds for their left and right slot. Enemy ships also get random satellite builds.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `9` |
| Kind | collection |
| Folder | [`Resources/Database/Satellite/Builds/`](../../../Assets/Modules/Database/Resources/Database/Satellite/Builds/) |
| Records in the built-in catalog | 39: 25 with `DifficultyClass` 1, 5 with 2, 9 with 3 |
| Schema | [SatelliteBuild.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/SatelliteBuild.xml) |
| Generated class | `GameDatabase.DataModel.SatelliteBuild` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/SatelliteBuild.cs), [hand-written part](../../../Assets/Modules/Database/Scripts/DataModel/SatelliteBuild.cs)) |
| Runtime wrapper | `CommonSatellite` |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `SatelliteId` | ref [Satellite](Satellite.md) | required | The satellite. Model name `Satellite`. The loader throws when it is missing. |
| `NotAvailableInGame` | bool | | No effect. No code reads it ([DB11](../../issues/database.md#db11-schema-fields-without-runtime-effect)). Built-in builds 30 and 37 set it, and both can still appear as random satellites. |
| `DifficultyClass` | enum [DifficultyClass](enums.md#difficultyclass) | `Default` | Lowest companion class at which the build can be a random satellite (see [Random satellites](#random-satellites)). It does not change stats. |
| `Components` | struct list [InstalledComponent](structures.md#installedcomponent) | | Components at satellite cell positions. `BarrelId` `0` is the satellite's barrel. |

## Notes

### Random satellites

Enemy ships with a `Common` hull get random satellites. The rule differs by source:

| Source | Class limit | Slots | Code |
| --- | --- | --- | --- |
| Star fleets, quests, challenges | `(level + 10) / 20`: class 1 from level 10, 2 from 30, 3 from 50 | Each empty slot gets a satellite with a 2/3 chance. Build satellites stay. | [ShipExtensions.cs:113](../../../Assets/Modules/ShipConstructor/Scripts/Extensions/ShipExtensions.cs#L113), [Distance.cs:20](../../../Assets/Modules/ShipConstructor/Scripts/Maths/Distance.cs#L20) |
| Exploration encounters | `level / 25`; no satellites below level 25 | Each slot gets a satellite with a 2/3 chance and replaces a build satellite. | [EnemyShipBuilder.cs:43](../../../Assets/Scripts/Domain/Exploration/EnemyShipBuilder.cs#L43) |

In both cases a build passes when its `DifficultyClass` ≤ the limit and the satellite fits the hull ([SatelliteExtensions.cs:26](../../../Assets/Modules/ShipConstructor/Scripts/Satellites/SatelliteExtensions.cs#L26)). A build with class `Default` is a candidate at every level where satellites appear. No built-in build uses `Default`.

### Other consumers

- [ShipBuild](ShipBuild.md) `LeftSatelliteBuild` and `RightSatelliteBuild`. The ship gets the satellite without a fit check.
- The session saves a player satellite as satellite ID plus components, not as build ID.
- Startup check: in an editable database, `InitializationState` places each build's components in the satellite layout and logs `invalid satellite layout: <id>` when one does not fit ([InitializationState.cs:113](../../../Assets/Scripts/GameStateMachine/States/InitializationState.cs#L113)).

### Data rules

1. Give each build a `DifficultyClass` of at least 1, unless it should appear from level 10 on.
2. Keep all components inside the satellite layout. Weapons need the satellite's barrel.
3. Remove `NotAvailableInGame` or treat it as a comment. To keep a build out of random selection, give it a class above the reachable limit, for example 3 for levels below 50.
