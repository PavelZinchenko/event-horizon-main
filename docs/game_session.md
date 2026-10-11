# GameSession Module

The GameSession module holds the player's mutable game state: fleet, inventory, resources, star map, quests, research, purchases, and the rest of a save game. It defines the save schema, generates the binary model and its version converters, exposes the state through domain facades, tracks changes, and converts old saves to the current model. Local and cloud storage are outside the module; they reach it through two small storage contracts.

The module lives in [`Assets/Modules/GameSession/`](../Assets/Modules/GameSession/). It compiles as the `GameSession` assembly ([GameSession.asmdef](../Assets/Modules/GameSession/GameSession.asmdef)). Runtime types use the `Session`, `Session.Content`, `Session.Model`, `Session.v1`, and `Session.v2` namespaces. Consumers inject [`ISessionData`](../Assets/Modules/GameSession/Scripts/Facade/ISessionData.cs#L31). [`GameInstaller`](../Assets/Scripts/Installers/GameInstaller.cs#L157) binds one `SessionData` instance.

Related documents:

- [Save schema](game_session/schema/index.md): XML format, member types, root members, versioning rules, version history.
- [Binary format](game_session/serialization.md): envelope, bit-level coding, type layout, load path, version conversions, legacy format.
- [Code generator](game_session/code_generator.md): the `.CodeGenerator` submodule and the code it emits.
- [Storage](game_session/storage.md): save triggers, local, PlayerPrefs, and cloud backends, per-mod saves, export and import.
- Per-field reference by domain: [Game](game_session/schema/game.md), [Fleet](game_session/schema/fleet.md), [Economy](game_session/schema/economy.md), [Quests](game_session/schema/quests.md), [World](game_session/schema/world.md), [Social](game_session/schema/social.md).
- Known problems: [GameSession Module Issues](issues/game_session.md).

## Contents

- [Terms](#terms)
- [Directory map](#directory-map)
- [Dependencies](#dependencies)
- [Data flow](#data-flow)
- [SessionData](#sessiondata)
- [Content facades](#content-facades)
- [Signals](#signals)
- [Change tracking and DataVersion](#change-tracking-and-dataversion)
- [New game](#new-game)
- [Loading a save](#loading-a-save)
- [Save lifecycle](#save-lifecycle)
- [How domain code uses the session](#how-domain-code-uses-the-session)
- [Hand-written model code](#hand-written-model-code)
- [Editor mode stub](#editor-mode-stub)
- [Consumers](#consumers)
- [Versioning and migration workflow](#versioning-and-migration-workflow)
- [Tests and validation](#tests-and-validation)
- [Common changes](#common-changes)

## Terms

| Term | Meaning |
| --- | --- |
| Session | The state of one game: everything a save file holds. One session exists at a time. |
| Model | The generated classes in `Session.Model`. `Model.SaveGameData` is the root. |
| Facade | A hand-written class in `Session.Content` that wraps one part of the model and adds domain rules. Consumers use facades, not the model. |
| Object | A schema type generated as a class with a parent link. A section of the save, such as `FleetData`. |
| Struct | A schema type generated as an immutable `readonly struct`. A record in a collection, such as `ShipInfo`. |
| Schema version | The `major.minor` of the save layout, written into every save. Current: 3.0. |
| Serializer format | The version of the envelope around the payload. Current: 1. |
| `DataVersion` | A counter that grows once per batch of changes between saves. Storage saves it next to the payload. Not a schema version. |
| Game ID | `UtcNow.Ticks` at new-game time. Identifies one playthrough across saves. |
| Mod ID | The database (mod) the session belongs to. Empty for the built-in database. Each mod has its own save. |
| Legacy save | A save written before the envelope header existed. Read by `Scripts/Obsolete/`. |

## Directory map

| Path | Purpose | Edit policy |
| --- | --- | --- |
| [`.Schema/`](../Assets/Modules/GameSession/.Schema/) | Save schema: `versions.xml` and one full snapshot per version. See [Save schema](game_session/schema/index.md). | New version folder for layout changes. Never change a released snapshot's layout. |
| [`.CodeGenerator/`](../Assets/Modules/GameSession/.CodeGenerator/) | Git submodule with the generator. See [Code generator](game_session/code_generator.md). | Change in its own repository. |
| [`generate_code.cmd`](../Assets/Modules/GameSession/generate_code.cmd) | Runs the generator. | — |
| [`Scripts/GeneratedCode/Model/`](../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/) | Current model, 33 types. | Never edit; regenerate. |
| [`Scripts/GeneratedCode/v1/`](../Assets/Modules/GameSession/Scripts/GeneratedCode/v1/), [`v2/`](../Assets/Modules/GameSession/Scripts/GeneratedCode/v2/) | Read-only models of older versions. | Never edit; regenerate. |
| [`Scripts/GeneratedCode/SessionLoader.cs`](../Assets/Modules/GameSession/Scripts/GeneratedCode/SessionLoader.cs) | Version dispatch and generated conversions. | Never edit; implement the partial hooks. |
| [`Scripts/Migration/`](../Assets/Modules/GameSession/Scripts/Migration/) | Hand-written conversion hooks. One file per source version. | Edit when a new version needs manual conversion. |
| [`Scripts/Model/`](../Assets/Modules/GameSession/Scripts/Model/) | Partial structs: constructors and conversions to Database and ShipConstructor types. `v1/` has constructors of old structs used by the legacy loader. | Edit for behavior the schema cannot express. |
| [`Scripts/Facade/`](../Assets/Modules/GameSession/Scripts/Facade/) | `ISessionData`, `SessionData`, `ContentFactory`, `SessionSerializer`, `LegacyDataLoader`, `SessionDataStub`. | Runtime lifecycle and public API. |
| [`Scripts/Facade/Content/`](../Assets/Modules/GameSession/Scripts/Facade/Content/) | 20 facades with their interfaces. | Domain rules over the model. |
| [`Scripts/Signals.cs`](../Assets/Modules/GameSession/Scripts/Signals.cs) | Session signals. | Add a signal together with its binding in `GameInstaller`. |
| [`Scripts/Extensions/GameTimeExtensions.cs`](../Assets/Modules/GameSession/Scripts/Extensions/GameTimeExtensions.cs) | Converts UTC ticks to whole time units since game start and back. | — |
| [`Scripts/Obsolete/`](../Assets/Modules/GameSession/Scripts/Obsolete/) | Reader for headerless legacy saves. `SessionDataObsolete` and `ISessionDataObsolete` are dead. | Compatibility only. |
| [`Tests/`](../Assets/Modules/GameSession/Tests/) | Edit Mode NUnit tests (`GameSession.Tests`). | Add serialization and migration tests here. |

## Dependencies

The assembly references `Diagnostics`, `Zenject`, `CommonComponents`, `Database`, `ShipConstructor`, and `Utilites`.

| Dependency | Used for |
| --- | --- |
| `Utilites` ([`Assets/ModulesShared/Utilites/Serialization/`](../Assets/ModulesShared/Utilites/Serialization/)) | Stream readers and writers, the variable-length coder, the observable collections, `IDataChangedCallback`. Also `ObscuredInt` and `ObscuredLong` ([`Security/`](../Assets/ModulesShared/Utilites/Security/)). This code is in the `ModulesShared` submodule. |
| `CommonComponents` | Storage contracts [`IGameData`](../Assets/Modules/CommonComponents/Scripts/Services/IGameData.cs) and [`ISerializableGameData`](../Assets/Modules/CommonComponents/Scripts/Services/ISerializableGameData.cs) (namespace `Services.Storage`), `SmartWeakSignal`, and the legacy `GameModel.Serialization.Helpers`. |
| `Database` | `IDatabase` and record types for conversions in `Scripts/Model/`; legacy name tables for the obsolete reader. |
| `ShipConstructor` | `IShip`, `IShipPreset`, `ISatellite`, `IntegratedComponent`, `ComponentInfo` for ship conversions. |
| `Zenject` | `ITickable` on `SessionData`. |
| `Diagnostics` | `GameDiagnostics.Trace` logging. |

Two different interfaces are called `IGameData`: `Services.Storage.IGameData` (storage metadata) and `Session.Content.IGameData` (the game facade). `ISessionData` implements the first and exposes the second as `Game`.

## Data flow

```text
.Schema/v1..v3 ──generate_code.cmd──► Scripts/GeneratedCode (Model, v1, v2, SessionLoader)

                 ┌──────────── SessionData (ISessionData, ISerializableGameData, ITickable) ───────────┐
 storage ──bytes──► TryDeserialize ─► SessionSerializer ─► SessionLoader ─► Model.SaveGameData ◄─┐     │
 storage ◄─bytes─── Serialize ◄──────  SessionSerializer ◄──────────────────────────┘            │     │
                 │                                                                              │     │
                 │   ContentFactory ─► 20 facades (Session.Content) ──read/write──────────────┘     │
                 │                           │                                                     │
                 │                           └──fire──► value signals (money, stars, position, …)  │
                 │   OnDataChanged ◄── every model change (dirty flag, DataVersion)                 │
                 └─────────────────────────────────────────────────────────────────────────────────┘
                                │ SessionDataLoadedSignal, SessionCreatedSignal
                                ▼
          Domain services (PlayerFleet, PlayerResources, StarMapManager, QuestManager, …)
```

## SessionData

[`SessionData`](../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs) is the only live implementation of `ISessionData`. It implements:

| Interface | Role |
| --- | --- |
| `ISessionContent` | 20 facade properties ([ISessionData.cs:7](../Assets/Modules/GameSession/Scripts/Facade/ISessionData.cs#L7)). |
| `Services.Storage.IGameData` | Storage metadata: `GameId`, `TimePlayed`, `DataVersion`, `ModId`. |
| `Services.Storage.ISerializableGameData` | `Serialize()` and `TryDeserialize(...)` for storage backends. |
| `ITickable` | Counts play time. |
| `IDataChangedCallback` | Receives change notifications from the model root. |

`SessionData` owns the current `Model.SaveGameData` and one instance of each facade. A load or a new game replaces the model and **all** facades. Never cache a facade or a model collection across a load: take it from `ISessionData` each time, or rebuild your state in a [`SessionCreatedSignal`](#signals) handler.

**Play time.** `Tick` adds `Time.deltaTime` to `TimePlayed` while `Game.StartTime > 0` ([SessionData.cs:77](../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L77)). `TimePlayed` is storage metadata. It is not part of the model and does not mark the session dirty. Storage passes it back on load.

**Game started.** `IsGameStarted()` is true when `Game.GameStartTime > 0` ([ISessionData.cs:35](../Assets/Modules/GameSession/Scripts/Facade/ISessionData.cs#L35)). A new session has start time 0 until the new-game flow sets it. `GameDataManager` does not save a session that has not started.

## Content facades

[`ContentFactory`](../Assets/Modules/GameSession/Scripts/Facade/ContentFactory.cs) creates the facades. Zenject injects the database and the signal triggers into it, and it passes them to the facades that need them. Each facade receives the whole `Model.SaveGameData` and reads its own section.

| `ISessionContent` property | Facade | Model section | Signals fired | Main operations |
| --- | --- | --- | --- | --- |
| `Game` | [GameData](../Assets/Modules/GameSession/Scripts/Facade/Content/GameData.cs) | [`Game`](game_session/schema/game.md#gamedata) | — | Seed, `Counter` (read increments, [GS5.3](issues/game_session.md#gs5-smaller-defects)), start time, total play time, supply ship start. |
| `StarMap` | [StarMapData](../Assets/Modules/GameSession/Scripts/Facade/Content/StarMapData.cs) | [`StarMap`](game_session/schema/world.md#starmapdata) | `PlayerPositionChanged`, `NewStarSecured` | Position, visited stars, occupant state, planet data, bookmarks, zoom. |
| `Inventory` | [InventoryData](../Assets/Modules/GameSession/Scripts/Facade/Content/InventoryData.cs) | [`Inventory`](game_session/schema/economy.md#inventory) | — | Component and satellite inventories. |
| `Fleet` | [FleetData](../Assets/Modules/GameSession/Scripts/Facade/Content/FleetData.cs) | [`Fleet`](game_session/schema/fleet.md#fleetdata) | — | Ship list, hangar slots, exploration ship, `UpdateShips`. |
| `Shop` | [ShopData](../Assets/Modules/GameSession/Scripts/Facade/Content/ShopData.cs) | [`Shop`](game_session/schema/economy.md#shopdata) | — | Purchases per star and item, renewal count. |
| `Events` | [EventData](../Assets/Modules/GameSession/Scripts/Facade/Content/EventData.cs) | [`Events`](game_session/schema/world.md#eventdata) | — | Event completion time per star. |
| `Bosses` | [BossData](../Assets/Modules/GameSession/Scripts/Facade/Content/BossData.cs) | [`Bosses`](game_session/schema/world.md#bossdata-and-bossinfo) | — | Defeat count and time per boss. |
| `Regions` | [RegionData](../Assets/Modules/GameSession/Scripts/Facade/Content/RegionData.cs) | [`Regions`](game_session/schema/world.md#regiondata) | — | Region faction, captured state, starbase defense power. |
| `Purchases` | [IapData](../Assets/Modules/GameSession/Scripts/Facade/Content/IapData.cs) | [`Iap`](game_session/schema/economy.md#iapdata) | `StarsValueChanged` | Supporter pack, remove-ads, purchased stars. |
| `Pvp` | [PvpData](../Assets/Modules/GameSession/Scripts/Facade/Content/PvpData.cs) | [`Pvp`](game_session/schema/social.md#pvpdata) | — | Arena counters and timers. |
| `Wormholes` | [WormholeData](../Assets/Modules/GameSession/Scripts/Facade/Content/WormholeData.cs) | [`Wormholes`](game_session/schema/world.md#wormholedata) | — | Wormhole target per source star. |
| `Achievements` | [AchievementData](../Assets/Modules/GameSession/Scripts/Facade/Content/AchievementData.cs) | [`Achievements`](game_session/schema/game.md#achievements) | — | Unlocked achievements. |
| `CommonObjects` | [CommonObjectData](../Assets/Modules/GameSession/Scripts/Facade/Content/CommonObjectData.cs) | [`Common`](game_session/schema/game.md#commonobjectdata) | — | Generic values and use times per object ID. |
| `Research` | [ResearchData](../Assets/Modules/GameSession/Scripts/Facade/Content/ResearchData.cs) | [`Research`](game_session/schema/economy.md#researchdata) | — | Technologies, research points per faction. |
| `Statistics` | [StatisticsData](../Assets/Modules/GameSession/Scripts/Facade/Content/StatisticsData.cs) | [`Statistics`](game_session/schema/game.md#statistics) | — | Unlocked ships, defeated enemies. |
| `Resources` | [ResourcesData](../Assets/Modules/GameSession/Scripts/Facade/Content/ResourcesData.cs) | [`Resources`](game_session/schema/economy.md#resourcesdata) | `MoneyValueChanged`, `FuelValueChanged`, `TokensValueChanged`, `StarsValueChanged`, `ResourcesChanged` | Currencies and special resources. |
| `Upgrades` | [UpgradesData](../Assets/Modules/GameSession/Scripts/Facade/Content/UpgradesData.cs) | [`Upgrades`](game_session/schema/economy.md#upgradesdata) | `PlayerSkillsReset` | Experience, skills, skill reset. |
| `Social` | [SocialData](../Assets/Modules/GameSession/Scripts/Facade/Content/SocialData.cs) | [`Social`](game_session/schema/social.md#socialdata) | — | Daily reward dates. |
| `Quests` | [QuestData](../Assets/Modules/GameSession/Scripts/Facade/Content/QuestData.cs) | [`Quests`](game_session/schema/quests.md#questdata) | — | Quest progress and statistics, faction and character relations. |
| `ShipPresets` | [ShipPresetsData](../Assets/Modules/GameSession/Scripts/Facade/Content/ShipPresetsData.cs) | [`ShipPresets`](game_session/schema/fleet.md#shippresetsdata) | — | Ship editor presets, `UpdatePresets`. |

Some facades expose model collections directly (`Fleet.Ships`, `Inventory.Components`). Changes through these collections are tracked like any other model change. `Resources.Resources` returns a wrapper over the same dictionary whose callback also fires `ResourcesChangedSignal` ([ResourcesData.cs:84](../Assets/Modules/GameSession/Scripts/Facade/Content/ResourcesData.cs#L84)).

**Supporter pack stars.** With the supporter pack, `Resources.Stars` reads 100 more than the stored value, and its setter subtracts 100 before it stores ([ResourcesData.cs:73](../Assets/Modules/GameSession/Scripts/Facade/Content/ResourcesData.cs#L73)). `Purchases.RemoveAds` is also true when the player has the supporter pack or any purchased stars ([IapData.cs:23](../Assets/Modules/GameSession/Scripts/Facade/Content/IapData.cs#L23)).

## Signals

All signals are `SmartWeakSignal` types in [Signals.cs](../Assets/Modules/GameSession/Scripts/Signals.cs). `GameInstaller` binds each signal and its trigger ([GameInstaller.cs:166](../Assets/Scripts/Installers/GameInstaller.cs#L166)).

| Signal | Payload | Fired by | When |
| --- | --- | --- | --- |
| `SessionDataLoadedSignal` | — | `SessionData` | After a load or new game installs the model and facades. Handlers must not use other game services yet. |
| `SessionCreatedSignal` | — | `SessionData` | Right after `SessionDataLoadedSignal`. Game services are ready. |
| `MoneyValueChangedSignal` | `long` new value | `ResourcesData` | Money changed through the facade. |
| `StarsValueChangedSignal` | `long` displayed value | `ResourcesData`, `IapData` | Stars changed, or the supporter pack changed. |
| `FuelValueChangedSignal` | `int` | `ResourcesData` | Fuel changed. |
| `TokensValueChangedSignal` | `int` | `ResourcesData` | Tokens changed. |
| `ResourcesChangedSignal` | — | `ResourcesData` | Any special resource changed through `Resources.Resources`. |
| `PlayerPositionChangedSignal` | `int` star ID | `StarMapData` | The player moved to another star ([StarMapData.cs:70](../Assets/Modules/GameSession/Scripts/Facade/Content/StarMapData.cs#L70)). |
| `NewStarSecuredSignal` | `int` star ID | `StarMapData` | A star changed from `Unknown` or `Agressive` to `Empty` or `Passive` ([StarMapData.cs:115](../Assets/Modules/GameSession/Scripts/Facade/Content/StarMapData.cs#L115)). |
| `PlayerSkillsResetSignal` | — | `UpgradesData` | Skills were reset ([UpgradesData.cs:45](../Assets/Modules/GameSession/Scripts/Facade/Content/UpgradesData.cs#L45)). |

The value signals fire only for changes through the facade. A load or a new game does not fire them; listeners refresh on `SessionCreatedSignal`.

`SessionAboutToSaveSignal` is not part of this module. The application defines and fires it before every save ([GameDataManager.cs:242](../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L242)).

## Change tracking and DataVersion

Every generated setter and every observable collection calls `OnDataChanged` on its owner. Objects forward the call to their parent, up to `Model.SaveGameData`, whose parent is `SessionData` ([SessionData.cs:173](../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L173)).

`SessionData.OnDataChanged` ([SessionData.cs:176](../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L176)):

1. If the session is already dirty, do nothing.
2. Otherwise mark it dirty and increment `DataVersion`.

`Serialize` clears the dirty flag ([SessionData.cs:85](../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L85)). So `DataVersion` counts "saves with changes", not single changes. Storage stores it next to the payload and skips a save when `GameId` and `DataVersion` match the last saved file; see [Skipping unchanged saves](game_session/storage.md#skipping-unchanged-saves).

Not tracked:

- Members with `notimportant`: `GameData.TotalPlayTime`, `StarMapData.MapModeZoom`, `StarModeZoom`.
- `TimePlayed` and the facades' own runtime fields, such as `StarMapData.LastPlayerPosition`, which are not saved at all.
- `ObservableList.Sort` ([GS5.2](issues/game_session.md#gs5-smaller-defects)).

## New game

`SessionData.CreateNewGame(modId, keepPurchases = true)` ([SessionData.cs:125](../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L125)):

1. Create a new `Model.SaveGameData` with schema defaults. `GameData.Seed` gets the current UTC ticks; `FleetData.ExplorationShipId` gets −1.
2. If `keepPurchases` and a previous model exists, copy `Iap.RemoveAds`, `Iap.SupporterPack`, and `Iap.Stars`, and set `Resources.Stars` to the purchased stars. Earned stars are lost.
3. Set `GameId` to `UtcNow.Ticks`, `TimePlayed` and `DataVersion` to 0, `ModId` to the argument. Clear the dirty flag.
4. Create the facades and connect the model to `SessionData`.
5. Fire `SessionDataLoadedSignal`, then `SessionCreatedSignal`.

The model is empty after this call: `Game.StartTime` is 0, so the game is not started. [`StartingNewGameState`](../Assets/Scripts/GameStateMachine/States/StartingNewGameState.cs#L47) sets the start time and fills the starting fleet and resources.

## Loading a save

`SessionData.TryDeserialize(gameId, timePlayed, dataVersion, modId, data, startIndex)` ([SessionData.cs:91](../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L91)):

1. `SessionSerializer.TryDeserialize` reads the envelope, reads any supported version, and converts it to the current model. Headerless saves go through `LegacyDataLoader`. See [Load path](game_session/serialization.md#load-path).
2. On success, create the facades and connect the model.
3. Store the storage metadata from the arguments. Clear the dirty flag.
4. Fire `SessionDataLoadedSignal`, then `SessionCreatedSignal`. A handler exception is logged and does not turn the load into a failure.

A failed load returns `false`, but it can still replace the live model: see [GS1](issues/game_session.md#gs1-a-failed-load-replaces-the-live-model).

## Save lifecycle

The application owns the save timing. The module only produces and accepts bytes.

| Step | Code |
| --- | --- |
| Startup load | [`InitializationState`](../Assets/Scripts/GameStateMachine/States/InitializationState.cs#L140) loads the database, then asks `IDataStorage.TryLoad(session, databaseId)`. If that fails and the database is editable, it imports the built-in save into the mod. Otherwise it calls `CreateNewGame`. |
| Save triggers | [`GameDataManager.SaveSession`](../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L151) fires `SessionAboutToSaveSignal`, then `IDataStorage.Save`. It runs on application pause, focus loss, `OnDisable`, the star map scene load, a completed purchase, and a mod switch. It skips sessions that have not started. One cloud path skips the signal ([GS7](issues/game_session.md#gs7-a-cloud-save-to-a-new-slot-skips-the-write-back)). |
| Mod switch | `GameDataManager` saves, loads the other database, then loads that mod's save or creates a new game ([GameDataManager.cs:80](../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L80)). |
| Cloud | `LoadGameFromCloud`, `SaveGameToCloud`, `LoadGameFromLocalCopy` route through `ICloudStorage` and `ISavedGame`. |
| Backends | Contracts in [`ServicesFacade/Scripts/Storage/`](../Assets/Modules/ServicesFacade/Scripts/Storage/). Implementations in [`Services/Scripts/Storage/`](../Assets/Modules/Services/Scripts/Storage/) (PlayerPrefs) and the private `ModulesPrivate/Services/Scripts/Storage/` (local files, Apple and Google cloud). Each adds its own header with game ID, play time, and data version; local files and cloud saves also have a mod ID field. Local files are encrypted, PlayerPrefs data is compressed. |

The full description is in [Storage](game_session/storage.md).

## How domain code uses the session

Most gameplay code does not use facades directly. Domain services in [`Assets/Scripts/Domain/`](../Assets/Scripts/Domain/) keep live objects and synchronize them with the session. They derive from [`GameServiceBase`](../Assets/Scripts/GameServices/GameServiceBase.cs#L6), which subscribes to both session signals.

The pattern, using [`PlayerFleet`](../Assets/Scripts/Domain/Player/PlayerFleet.cs) as the example:

1. `OnSessionDataLoaded` / `OnSessionCreated`: read `ShipInfo` structs from `session.Fleet.Ships` and build `IShip` objects with `ShipInfo.ToShip(database)`.
2. Gameplay changes the live `IShip` objects, not the session.
3. `SessionAboutToSaveSignal`: if anything changed, write all ships back with `session.Fleet.UpdateShips(ships)` ([PlayerFleet.cs:142](../Assets/Scripts/Domain/Player/PlayerFleet.cs#L142)). `ObservableList.Assign` with the struct comparer changes only the entries that differ.

Simple values, such as money or the star map, are read and written through the facades directly.

## Hand-written model code

The partial structs in [`Scripts/Model/`](../Assets/Modules/GameSession/Scripts/Model/) add constructors and conversions to the generated structs:

| Struct | Adds | Converts to |
| --- | --- | --- |
| [`ShipInfo`](../Assets/Modules/GameSession/Scripts/Model/ShipInfo.cs) | Constructor from `IShip`; field constructor for the legacy loader. | `IShip` via `ToShip(database)`: hull, perks, layout modifications, components, satellites, name, color, experience. Returns `null` for an unknown hull. |
| [`ShipComponentInfo`](../Assets/Modules/GameSession/Scripts/Model/ShipComponentInfo.cs) | Constructor from `IntegratedComponent`; field constructor. | `IntegratedComponent`. Unknown components are logged and skipped. |
| [`SatelliteInfo`](../Assets/Modules/GameSession/Scripts/Model/SatelliteInfo.cs) | Constructor from `ISatellite` (null gives ID 0). | `ISatellite`, or `null` for ID 0 or an unknown satellite. |
| [`ShipPresetInfo`](../Assets/Modules/GameSession/Scripts/Model/ShipPresetInfo.cs) | Constructor from `IShipPreset`. | `IShipPreset`. |
| [`InventoryComponentInfo`](../Assets/Modules/GameSession/Scripts/Model/InventoryComponentInfo.cs) | Constructor from `ComponentInfo`. | `ComponentInfo`; `ComponentInfo.Empty` for an unknown component. |
| [`BossInfo`](../Assets/Modules/GameSession/Scripts/Model/BossInfo.cs), [`HangarSlotInfo`](../Assets/Modules/GameSession/Scripts/Model/HangarSlotInfo.cs), [`PurchaseInfo`](../Assets/Modules/GameSession/Scripts/Model/PurchaseInfo.cs), [`QuestProgress`](../Assets/Modules/GameSession/Scripts/Model/QuestProgress.cs), [`QuestStatistics`](../Assets/Modules/GameSession/Scripts/Model/QuestStatistics.cs) | Field constructors. | — |
| [`v1/BossInfo`](../Assets/Modules/GameSession/Scripts/Model/v1/BossInfo.cs), [`v1/PurchaseInfo`](../Assets/Modules/GameSession/Scripts/Model/v1/PurchaseInfo.cs) | Field constructors for the legacy loader. | — |

Saved ships store the hull ID, components, perks, layout modifications, satellites, name, color, and experience. They do not store a ship build. On load, components and satellites that the database no longer has are skipped, and an unknown hull makes `ToShip` return `null`.

## Editor mode stub

When the `EDITOR_MODE` symbol is defined (the separate database and ship editor build), `GameInstaller` binds [`SessionDataStub`](../Assets/Modules/GameSession/Scripts/Facade/SessionDataStub.cs) instead of `SessionData` ([GameInstaller.cs:159](../Assets/Scripts/Installers/GameInstaller.cs#L159)). Its facade properties are always `null`, and `Serialize`, `TryDeserialize`, and `CreateNewGame` throw `NotImplementedException`. Code that runs in editor mode must not touch the session.

## Consumers

About 65 files outside the module use the `Session` namespaces. The most used facades are `StarMap`, `Quests`, `Game`, `Resources`, and `Pvp`.

| Area | Main files | Uses |
| --- | --- | --- |
| Player domain | [`Assets/Scripts/Domain/Player/`](../Assets/Scripts/Domain/Player/) (`PlayerFleet`, `PlayerInventory`, `PlayerResources`, `PlayerSkills`, `MotherShip`, `StarMapManager`, `DailyReward`) | Fleet, inventory, resources, upgrades, star map, social. |
| Galaxy | [`Assets/Scripts/Domain/Galaxy/`](../Assets/Scripts/Domain/Galaxy/) (`StarContent/*`, `StarData`) | Bosses, events, regions, shop, wormholes, common objects, star map. |
| Quests | [`Assets/Scripts/Domain/Quests/`](../Assets/Scripts/Domain/Quests/) (`QuestDataProvider`, `GameDataProvider`, `StarMapDataProvider`, `QuestManagerEventProvider`) | Adapts `IQuestData` and game data to the [Quests module](quests.md). |
| Research and skills | [`Domain/Research/Research.cs`](../Assets/Scripts/Domain/Research/Research.cs), [`Domain/Skills/`](../Assets/Scripts/Domain/Skills/) | Research, upgrades. |
| Economy | [`Domain/Economy/`](../Assets/Scripts/Domain/Economy/) | Resources, inventory, shop. |
| Game states | [`Assets/Scripts/GameStateMachine/States/`](../Assets/Scripts/GameStateMachine/States/) (`InitializationState`, `StartingNewGameState`, `MainMenuState`, `StarMapState`, `TravelState`, `RetreatState`, `QuickCombatState`, `ShipEditorState`) | Load, new game, travel, ship editor. |
| Services | [`GameDataManager`](../Assets/Scripts/GameServices/DataManager/GameDataManager.cs), [`OfflineMultiplayer`](../Assets/Scripts/GameServices/OfflineMultiplayer/), [`RandomGenerator`](../Assets/Scripts/GameServices/Random/RandomGenerator.cs), [`GameTime`](../Assets/Scripts/Services/InternetTime/GameTime.cs), [`HolidayManager`](../Assets/Scripts/GameServices/HolidayManager.cs) | Persistence, PvP, seeds, play time, events. |
| Legacy GUI and map | [`Assets/Scripts/Legacy/`](../Assets/Scripts/Legacy/) | Star map views, skills, cheats. |
| Purchases | `ModulesPrivate/Services/Scripts/InAppPurchasing/`, [`ServicesFacade/Scripts/IapStorage/`](../Assets/Modules/ServicesFacade/Scripts/IapStorage/) | `IapData`, stars. |

## Versioning and migration workflow

For any change to persisted data:

1. Decide if the layout changes. Adding, removing, renaming, or reordering a member changes it. Only a same-signedness integer widening does not ([Compatibility rules](game_session/serialization.md#compatibility-rules)).
2. For a layout change, copy the latest `.Schema/v<N>/` folder to `v<N+1>/` and add `<version name="v<N+1>" major="<N+1>" minor="0" />` to `versions.xml`. Use a new major; minor versions are not supported ([GS5.9](issues/game_session.md#gs5-smaller-defects)).
3. Edit the new snapshot only.
4. From `Assets/Modules/GameSession/`, run `generate_code.cmd`. Read the console output; the exit code is always 0 ([GS5.8](issues/game_session.md#gs5-smaller-defects)).
5. Read the summary comment of the new `Upgrage_v<N>_0_to_v<N+1>_0` hook in `SessionLoader.cs`. Implement every listed item in `Scripts/Migration/SessionLoader_v<N>.cs`.
6. Update the `Scripts/Model/` partials if a struct's fields changed.
7. Update the facade, its interface, `ISessionContent`, `SessionData`, `SessionDataStub`, and `ContentFactory` if the public surface changed. Add signals to `Signals.cs` and `GameInstaller`.
8. Add a test that loads a save of version N and checks the converted fields ([GS3](issues/game_session.md#gs3-migration-and-the-envelope-have-no-regression-tests)).

The generator details are in [Code generator](game_session/code_generator.md). The XML rules are in [Save schema](game_session/schema/index.md#versioning-rules).

## Tests and validation

[`GameSessionTests`](../Assets/Modules/GameSession/Tests/GameSessionTests.cs) (Edit Mode, `UNITY_INCLUDE_TESTS`) covers:

| Test | Covers |
| --- | --- |
| `TestStreamReader` | `MemoryWriterStream` / `MemoryReaderStream` round trip. |
| `TestEliasGammaEncoder` | The variable-length coder. |
| `TestSessionDataWriter` | Mixed `Plain` and `EliasGamma` values. |
| `TestBitset`, `TestBitsetEnumeration` | `ObservableBitset` with 100 000 random keys; enumeration order. |
| `TestSaveLoad` | Round trip of 1 000 `BossInfo` entries through `Model.SaveGameData`. |

Not covered: the envelope, version conversions, the legacy loader, facades, and change tracking.

Checklist for a change to this module:

1. Run the generator into a scratch folder and diff it with `Scripts/GeneratedCode/`. Only intended changes may appear.
2. Run `GameSession.Tests`.
3. For a new version: load a save of every older supported version and of the headerless format.
4. Change a scalar and a collection through a facade. Check that `DataVersion` grows once per save and that the expected signal fires once.
5. Save, restart, and load with the built-in database and with a mod.
6. Feed a truncated save and a save with a higher major through storage, and check that the previous session stays intact ([GS1](issues/game_session.md#gs1-a-failed-load-replaces-the-live-model)).

## Common changes

| Change | Where |
| --- | --- |
| Add or change a persisted field | New `.Schema/v<N+1>/` snapshot, generator, migration hook. See [workflow](#versioning-and-migration-workflow). |
| Add a domain operation on existing data | The facade in `Scripts/Facade/Content/` and its interface. |
| Convert a session struct to a Database or ShipConstructor type | `Scripts/Model/` partial. |
| Add a session signal | `Scripts/Signals.cs`, `ContentFactory`, the facade, and `GameInstaller`. |
| Change the envelope or legacy routing | `SessionSerializer.cs`, `LegacyDataLoader.cs`. Bump the serializer format. |
| Change save timing or a storage backend | `GameDataManager` and the storage modules, not GameSession. See [Storage](game_session/storage.md#checklist-for-a-storage-change). |
| Change the coder or the collections | `Assets/ModulesShared/Utilites/Serialization/` (submodule). Any change to bytes breaks every existing save. |
