# World Domain

This page describes the saved state of the galaxy: the objects `StarMapData`, `RegionData`, `WormholeData`, `BossData`, and `EventData`, and the struct `BossInfo`. For the XML format and the type rules, see [Save schema](index.md). For the module overview, see [GameSession module](../../game_session.md).

## Contents

- [Overview](#overview)
- [StarMapData](#starmapdata)
- [RegionData](#regiondata)
- [WormholeData](#wormholedata)
- [BossData and BossInfo](#bossdata-and-bossinfo)
- [EventData](#eventdata)

## Overview

The galaxy itself is not saved. Star positions, star content, regions, and region home stars are generated from `GameData.Seed` (see [Game domain](game.md#gamedata)). The save holds only what the player changed: where the player is, which stars were visited or cleared, region ownership, and per-star cooldowns.

All keys on this page are **star IDs** or **region IDs**. Both are generated numbers, not database IDs. Star 0 is the player's home star.

## StarMapData

Player position, visited and cleared stars, planet exploration, bookmarks, and map zoom.

| | |
| --- | --- |
| Schema | [`v3/Objects/StarMapData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/StarMapData.xml) |
| Generated class | [`Model.StarMapData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/StarMapData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IStarMapData` / `StarMapData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/StarMapData.cs), property `ISessionData.StarMap` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `PlayerPosition` | `int` | 0 | Star ID of the mothership. |
| `MapModeZoom` | `single` | 0, `notimportant` | Zoom of the galaxy map view. |
| `StarModeZoom` | `single` | 0, `notimportant` | Zoom of the star system view. |
| `DiscoveredStars` | `bitset` | empty | Stars the player visited. Stars are never removed. |
| `SecuredStars` | `bitset` | empty | First bit of the occupant state. See [Occupant state](#occupant-state). |
| `EnemiesOnStars` | `bitset` | empty | Second bit of the occupant state. |
| `PlanetData` | `map<long,int>` | empty | Planet key → exploration bits. See [Planet data](#planet-data). |
| `Bookmarks` | `map<int,string>` | empty | Star ID → player note. |

The three bitsets replaced the v1 `StarData` map in v2; see [v1.0 → v2.0](../serialization.md#v10--v20).

### Facade API

| Member | Behavior | Signal |
| --- | --- | --- |
| `PlayerPosition` | Set does nothing for the same star. Otherwise it stores the old star in `LastPlayerPosition`, stores the new star, and marks it visited ([StarMapData.cs:58](../../../Assets/Modules/GameSession/Scripts/Facade/Content/StarMapData.cs#L58)). | `PlayerPositionChangedSignal(starId)` |
| `LastPlayerPosition` | The previous star. A runtime field of the facade: not saved, 0 after every load or new game. | — |
| `MapScaleFactor`, `StarScaleFactor` | Get and set the two zoom fields. | — |
| `IsVisited(starId)`, `SetVisited(starId)` | Read and add a `DiscoveredStars` bit. | — |
| `FurthestVisitedStar` | `DiscoveredStars.LastIndex`: the highest visited star ID ([StarMapData.cs:78](../../../Assets/Modules/GameSession/Scripts/Facade/Content/StarMapData.cs#L78)). Star IDs are assigned in square rings around the center ([StarLayout.cs:27](../../../Assets/Modules/CommonComponents/Scripts/Galaxy/StarLayout.cs#L27)), so a higher ID is on the same or an outer ring. The value therefore serves as a progress measure. | — |
| `GetEnemy(starId)`, `SetEnemy(starId, occupant)` | Read and write the occupant state. | `NewStarSecuredSignal(starId)`; see below. |
| `GetPlanetData`, `SetPlanetData` | Read (default 0) and write a planet's bits. | — |
| `GetBookmark`, `HasBookmark`, `SetBookmark` | Read (default `""`) and write. An empty or `null` text removes the bookmark. | — |
| `Reset()` | Sets the position to 0 and clears secured stars, enemies, bookmarks, and planet data. Keeps `DiscoveredStars` and the zoom. Only the debug cheats call it ([Cheats.cs:212](../../../Assets/Scripts/Legacy/Debug/Cheats.cs#L212)). | — |

#### Occupant state

Two bitsets encode four states per star ([StarMapData.cs:94](../../../Assets/Modules/GameSession/Scripts/Facade/Content/StarMapData.cs#L94)):

| `IStarMapData.Occupant` | `SecuredStars` | `EnemiesOnStars` | Meaning in [`Occupants`](../../../Assets/Scripts/Domain/Galaxy/StarContent/Occupants.cs) |
| --- | --- | --- | --- |
| `Unknown` (default) | 0 | 0 | The local fleet was never met. It can attack the player. |
| `Empty` | 1 | 0 | The local fleet was destroyed. |
| `Passive` | 1 | 1 | The local fleet is present but does not attack: the player drove it off or is much stronger. |
| `Agressive` | 0 | 1 | The local fleet is present and attacks. |

`SetEnemy` fires `NewStarSecuredSignal` when a star changes from `Unknown` or `Agressive` to `Empty` or `Passive` ([StarMapData.cs:113](../../../Assets/Modules/GameSession/Scripts/Facade/Content/StarMapData.cs#L113)). [`StarData`](../../../Assets/Scripts/Domain/Galaxy/StarData.cs#L151) sets star 0 to `Empty` and visited on every load.

#### Planet data

The key is `(starId << 32) + planetIndex` ([StarMapData.cs:82](../../../Assets/Modules/GameSession/Scripts/Facade/Content/StarMapData.cs#L82)). The value is a `uint` bit field stored as `int`:

| Bit | Set by | Meaning |
| --- | --- | --- |
| 31 | [`Planet.OnExplorationStarted`](../../../Assets/Scripts/Domain/Exploration/Planet.cs#L118) | The player started exploring the planet. |
| 0 … n−1 | [`ExplorationData.Complete`](../../../Assets/Scripts/Domain/Exploration/ExplorationData.cs#L77) | Exploration objective with that ID is completed. |

`Planet.WasExplored` is true when any bit is set ([Planet.cs:125](../../../Assets/Scripts/Domain/Exploration/Planet.cs#L125)).

### Consumers

| Consumer | Use |
| --- | --- |
| [`MotherShip`](../../../Assets/Scripts/Domain/Player/MotherShip.cs#L29), [`TravelState`](../../../Assets/Scripts/GameStateMachine/States/TravelState.cs#L48), [`RetreatState`](../../../Assets/Scripts/GameStateMachine/States/RetreatState.cs#L46) | Read and move the player position. |
| [`RetreatState.OnLoad`](../../../Assets/Scripts/GameStateMachine/States/RetreatState.cs#L57) | Retreats to the nearest visited star, or to `LastPlayerPosition` when none is found. After a load that falls back to star 0. |
| [`Occupants`](../../../Assets/Scripts/Domain/Galaxy/StarContent/Occupants.cs#L29) | Occupant state of each star. |
| [`StarData`](../../../Assets/Scripts/Domain/Galaxy/StarData.cs#L48) | Visited flags and bookmarks for the map. |
| [`Planet`](../../../Assets/Scripts/Domain/Exploration/Planet.cs), [`ExplorationData`](../../../Assets/Scripts/Domain/Exploration/ExplorationData.cs) | Planet bits. |
| [`DailyReward`](../../../Assets/Scripts/Domain/Player/DailyReward.cs#L52) | Reward level from `FurthestVisitedStar`. |
| [`MapScaler`](../../../Assets/Scripts/Legacy/Map/MapScaler.cs#L70) | Zoom. |
| [`StarMapDataProvider`](../../../Assets/Scripts/Domain/Quests/StarMapDataProvider.cs#L36) | Current star for the Quests module. |

## RegionData

Faction, starbase defense, and capture state of each region.

| | |
| --- | --- |
| Schema | [`v3/Objects/RegionData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/RegionData.xml) |
| Generated class | [`Model.RegionData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/RegionData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IRegionData` / `RegionData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/RegionData.cs), property `ISessionData.Regions` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `MilitaryPower` | `map<int,uint>` | empty | Region ID → starbase defense power, in percent. Present only after the value changed once. |
| `CapturedBases` | `bitset` | empty | Regions whose starbase the player captured. |
| `Factions` | `map<int,int>` | empty | Region ID → `Faction` record ID that owns the region. |

`MilitaryPower` and `CapturedBases` replaced the v1 `DefeatedFleetCount` map in v2. The conversion formula is in [v1.0 → v2.0](../serialization.md#v10--v20).

### Facade API

| Member | Behavior |
| --- | --- |
| `ExploredRegionCount`, `Regions` | Count and keys of `Factions`: the regions whose faction was decided. |
| `TryGetRegionFactionId`, `SetRegionFactionId` | Read and write `Factions`. |
| `IsRegionCaptured`, `SetRegionCaptured` | Read and write a `CapturedBases` bit. |
| `GetCapturedFactions()` | Faction IDs of the captured regions. Captured regions without a faction entry are skipped. |
| `DiscoveredRegions` | Keys of `MilitaryPower`: the regions whose defense changed at least once. |
| `TryGetStarbaseDefensePower`, `SetStarbaseDefensePower` | Read and write `MilitaryPower`. |
| `Reset()` | Clears `Factions` and `CapturedBases`. Keeps `MilitaryPower`. Only the debug cheats call it. |

### Consumers

[`Region`](../../../Assets/Scripts/Legacy/Game/Galaxy/Region.cs) holds the rules:

| Value | Rule |
| --- | --- |
| Faction | Read from `Factions`. When missing, chosen at random among factions with starbases for the home star level, seeded by the home star and `GameData.Seed`, and stored at once ([Region.cs:100](../../../Assets/Scripts/Legacy/Game/Galaxy/Region.cs#L100)). Quests can change it ([StarMapState.cs:285](../../../Assets/Scripts/GameStateMachine/States/StarMapState.cs#L285)). |
| Defense power | Read from `MilitaryPower`, or `FactionsSettings.StarbaseInitialDefense(homeStarLevel)` when missing ([Region.cs:81](../../../Assets/Scripts/Legacy/Game/Galaxy/Region.cs#L81)). Each defeated region fleet subtracts `DefenseLossPerEnemyDefeated`. Writes are clamped to `StarbaseMinDefense`. Defender level = home star level × power / 100. |
| Captured | The player's home region is always captured. Otherwise read from `CapturedBases`. Set by [`StarBase`](../../../Assets/Scripts/Domain/Galaxy/StarContent/StarBase.cs#L77) after a victory, or by quests. |

Other users: [`RegionMap`](../../../Assets/Scripts/Legacy/Game/Galaxy/RegionMap.cs#L33) (known regions), [`StarMapManager`](../../../Assets/Scripts/Domain/Player/StarMapManager.cs#L52) (factions met), [`Skills.IsFree`](../../../Assets/Scripts/Domain/Skills/Skills.cs#L65) (the "beat all enemies" skill requirement).

## WormholeData

Paired wormhole exits.

| | |
| --- | --- |
| Schema | [`v3/Objects/WormholeData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/WormholeData.xml) |
| Generated class | [`Model.WormholeData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/WormholeData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IWormholeData` / `WormholeData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/WormholeData.cs), property `ISessionData.Wormholes` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `Routes` | `map<int,int>` | empty | Wormhole star ID → the star ID of its other end. Every route is stored in both directions. |

### Facade API

| Member | Behavior |
| --- | --- |
| `GetTarget(source)` | The other end, or −1. |
| `SetTarget(source, target)` | Adds both directions. Throws `InvalidOperationException` when either star already has a route ([WormholeData.cs:18](../../../Assets/Modules/GameSession/Scripts/Facade/Content/WormholeData.cs#L18)). |

### Consumers

[`Wormhole.Enter`](../../../Assets/Scripts/Domain/Galaxy/StarContent/Wormhole.cs#L18) links a wormhole on first use. It picks the next wormhole star among the following 1000 star IDs that has no route yet, then moves the player there.

## BossData and BossInfo

Defeat count and time of star bosses and of other one-time star objects.

| | `BossData` | `BossInfo` |
| --- | --- | --- |
| Schema | [`v3/Objects/BossData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/BossData.xml) | [`v3/Types/BossInfo.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Types/BossInfo.xml) |
| Generated code | [`Model.BossData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/BossData.cs#L13) | [`Model.BossInfo`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/BossInfo.cs#L14) |
| Hand-written partial | — | [`Model/BossInfo.cs`](../../../Assets/Modules/GameSession/Scripts/Model/BossInfo.cs): constructor `(count, time)` |
| Facade | [`IBossData` / `BossData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/BossData.cs), property `ISessionData.Bosses` | — |

### Fields

| Type | Field | Type | Meaning |
| --- | --- | --- | --- |
| `BossData` | `Bosses` | `map<int,BossInfo>` | Star ID → defeat record. The facade calls the key `id`; all callers pass a star ID. |
| `BossInfo` | `DefeatCount` | `int` | Number of victories at this star. |
| `BossInfo` | `LastDefeatTime` | `int` | Whole hours since `GameData.StartTime` of the last victory. Was `long` UTC ticks in v1. |

### Facade API

| Member | Behavior |
| --- | --- |
| `CompletedTime(id)` | `LastDefeatTime` converted back to UTC ticks, or 0 when there is no record ([BossData.cs:19](../../../Assets/Modules/GameSession/Scripts/Facade/Content/BossData.cs#L19)). |
| `DefeatCount(id)` | Count, or 0. |
| `SetCompleted(id)` | Increments the count and stores the current hour. |

### Consumers

| Consumer | Use of the record |
| --- | --- |
| [`Boss`](../../../Assets/Scripts/Domain/Galaxy/StarContent/Boss.cs#L58) | Boss level = star level + 5 × defeat count. The boss returns 24 to 72 hours after the last victory. A victory also lowers the region's defense. |
| [`Ruins`](../../../Assets/Scripts/Domain/Galaxy/StarContent/Ruins.cs#L28), [`XmasTree`](../../../Assets/Scripts/Domain/Galaxy/StarContent/XmasTree.cs#L29) | `DefeatCount > 0` means "already done". |

The map is shared by star ID. One star has one content type, so the users do not collide.

## EventData

Last completion of a local event (beacon) per star.

| | |
| --- | --- |
| Schema | [`v3/Objects/EventData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/Objects/EventData.xml) |
| Generated class | [`Model.EventData`](../../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/EventData.cs#L13) |
| Hand-written partial | — |
| Facade | [`IEventData` / `EventData`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/EventData.cs), property `ISessionData.Events` |

### Fields

| Field | Type | Default / options | Meaning |
| --- | --- | --- | --- |
| `CompletedTime` | `map<int,int>` | empty | Star ID → whole hours since `GameData.StartTime` of the last completion. Was `long` UTC ticks in v1. |

### Facade API

`CompletedTime(starId)` returns UTC ticks, or 0 when there is no record. `Complete(starId)` stores the current hour.

### Consumers

[`LocalEvent`](../../../Assets/Scripts/Domain/Galaxy/StarContent/LocalEvent.cs#L17) makes an event available again `5 + (completedTime mod 25)` days after the last completion. Since v2 the completion time is a whole hour, so the modulo no longer varies between stars ([GS5.18](../../issues/game_session.md#gs5-smaller-defects)). The event seed also includes the completion time. In editor builds, `Start` does not call `Complete`, so events never go on cooldown there.
