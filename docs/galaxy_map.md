# GalaxyMap Module and Star Map Rendering

The GalaxyMap module holds the prefabs and materials that draw the galaxy in `StarMapScene`: stars, their icons and borders, and the objects inside a star system. It contains no scripts. The scripts that drive these prefabs are in the application layer, in [`Assets/Scripts/Legacy/Map/`](../Assets/Scripts/Legacy/Map/). This document covers both, because neither works without the other.

| | |
| --- | --- |
| Assembly | `GalaxyMap` ([GalaxyMap.asmdef](../Assets/Modules/GalaxyMap/GalaxyMap.asmdef)). References AppConfiguration, Utilites, Diagnostics, CommonComponents, Database, ShipConstructor, ServicesFacade, but the folder has no `.cs` files, so Unity builds no assembly. |
| Assets | [`Prefabs/Stars/`](../Assets/Modules/GalaxyMap/Prefabs/Stars/) (28 prefabs), [`Prefabs/Planets/`](../Assets/Modules/GalaxyMap/Prefabs/Planets/) (7 prefabs), [`Materials/`](../Assets/Modules/GalaxyMap/Materials/) (2 materials, 2 shaders) |
| Scripts | [`Assets/Scripts/Legacy/Map/`](../Assets/Scripts/Legacy/Map/) (20 files, about 2,350 lines, `Assembly-CSharp`) |
| Scene | [`StarMapScene.unity`](../Assets/Scenes/StarMapScene.unity) |
| Domain | `StarMap`, `StarData`, `Star` in [`Scripts/Domain/Galaxy/`](../Assets/Scripts/Domain/Galaxy/); `MotherShip` in [`Scripts/Domain/Player/`](../Assets/Scripts/Domain/Player/). Layout math: `StarLayout`, `RegionLayout` in [CommonComponents](common_components.md). |
| Known issues | [GalaxyMap issues](issues/galaxy_map.md) |

## Contents

1. [Terms](#terms)
2. [Scene structure](#scene-structure)
3. [View modes and zoom](#view-modes-and-zoom)
4. [Input](#input)
5. [Visible stars](#visible-stars)
6. [Star rendering](#star-rendering)
7. [Region borders](#region-borders)
8. [Star system view](#star-system-view)
9. [Player ship](#player-ship)
10. [Backgrounds](#backgrounds)
11. [Events](#events)
12. [Prefab catalog](#prefab-catalog)
13. [Adding a star icon](#adding-a-star-icon)

## Terms

| Term | Meaning |
| --- | --- |
| Star ID | Integer index of a star. Its position comes from `StarLayout.GetStarPosition(id, seed)`: a hexagonal grid with a per-star offset from the game seed. Star 0 is the home star. |
| Map unit | World distance on the star map. `GalaxyMap.DistanceBetweenStars` (8 in the scene) converts layout coordinates to map units. |
| View mode | `ViewMode` ([ViewMode.cs](../Assets/Scripts/Domain/Player/ViewMode.cs)): `StarMap`, `StarSystem`, or `GalaxyMap`. Stored in `MotherShip.ViewMode`. |
| Star state | `Star.State`: how one star draws itself at the current camera size (`Galaxy`, `Map`, `Normal`, `StarSystem`, `Hidden`). |
| Map filters | `StarMap.ShowBosses`, `ShowStores`, `ShowBookmarks`, `ShowArenas`, `ShowXmas`. Set from toggles in [`GameMenu`](../Assets/Scripts/Gui/StarMap/GameMenu.cs); not saved. |

## Scene structure

The main objects in `StarMapScene`:

| Object script | Role |
| --- | --- |
| [`GalaxyMap`](../Assets/Scripts/Legacy/Map/GalaxyMap.cs) | Root of all star objects. Creates and pools stars, handles clicks, reacts to position and content changes. |
| [`MapScaler`](../Assets/Scripts/Legacy/Map/MapScaler.cs) | On the camera. Owns zoom and the view mode. |
| [`MapNavigator`](../Assets/Scripts/Legacy/Map/MapNavigator.cs) | Moves the `GalaxyMap` transform to pan the map; eases to a focus point. |
| [`ScreenCenter`](../Assets/Scripts/Legacy/Map/ScreenCenter.cs) | A `RectTransform` in the UI that marks the visual center (the free area beside the side panels). |
| [`ShipRange`](../Assets/Scripts/Legacy/Map/ShipRange.cs) | Circle that shows the flight range for 2 seconds when the player taps an unreachable star. |
| [`PlayerShipObject`](../Assets/Scripts/Legacy/Map/PlayerShipObject.cs) | The player's ship icon. |
| [`StarSystem`](../Assets/Scripts/Legacy/Map/System/StarSystem.cs) with [`OrbitalObjects`](../Assets/Scripts/Legacy/Map/System/OrbitalObjects.cs) and two [`FleetObjects`](../Assets/Scripts/Legacy/Map/System/FleetObjects.cs) | The zoomed-in system view. |
| [`Background`](../Assets/Scripts/Legacy/Map/Background.cs) | Space background shader (low and high quality materials). |
| [`MapBackground`](../Assets/Scripts/Legacy/Map/MapBackground.cs) | Scrolling nebula texture. Also used in `SkillTreeScene`. |
| [`MapNavigationPanelViewModel`](../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/MapNavigationPanelViewModel.cs) | Full-screen UI panel that turns pointer input into move, click, and zoom events. |

The scene installer [`StarMapSceneInstaller`](../Assets/Scripts/Installers/StarMapSceneInstaller.cs) binds a scene `Messenger` and a `GameObjectPool`. All stars and icons come from that pool.

## View modes and zoom

`MapScaler` keeps one zoom value per view mode and lerps the orthographic camera size toward it (`_zoomSpeed` 5).

| View mode | Camera size range (scene values) | What is drawn |
| --- | --- | --- |
| `StarSystem` | 1–2.5 | Only the current star and its system. |
| `StarMap` | 5–25 | Stars near the camera with names and icons. |
| `GalaxyMap` | 100–150 | Only notable stars (see [Visible stars](#visible-stars)) as large icons. |

- `OnZoom(scale)` multiplies the zoom. In `StarMap` mode, zooming out past the maximum with enough force (`_mapModeThreshold`) switches to `GalaxyMap`. In `GalaxyMap` mode, zooming in past the minimum switches back.
- `ShowStarMap`, `ShowStarSystem`, `ShowGalaxyMap` set the mode directly (buttons in the UI).
- `UpdateViewMode` writes the mode to `MotherShip.ViewMode` only when the camera size has reached the band of the new mode (with 50% allowance). `MotherShip` then broadcasts `ViewModeChanged`.
- On disable, the map and system zoom values are stored in the session (`StarMap.MapScaleFactor`, `StarScaleFactor`). The galaxy zoom always starts at its minimum.

Independently, each `Star` decides its own state from the camera size every frame: `Galaxy` at 90 or more, `Map` at 20 or more, `Normal` at 4.5 or more, otherwise `StarSystem` for the current star and `Hidden` for the others.

## Input

`MapNavigationPanelViewModel` implements the uGUI pointer interfaces:

1. Drag with one or more pointers: the average pointer delta in world units goes to `MapNavigator.OnMove`. After release, the panel keeps sending a decaying velocity (inertia).
2. Two-finger pinch or mouse wheel: `MapScaler.OnZoom`.
3. Click without drag: `GalaxyMap.OnClick(worldPosition)`.

`MapNavigator.OnMove` shifts the map and starts a 2-second cooldown. After the cooldown, the map eases back to the focus point (the player's star or the focused star).

`GalaxyMap.OnClick`:

1. In `StarSystem` mode, forwards the click to `StarSystem.OnClick`.
2. Otherwise, finds the first child tagged `Star` within 1.6 star distances (galaxy mode) or 0.4 (star map mode).
3. If the player is out of fuel, shows the "no fuel" balloon. Travel is still allowed: with zero fuel, `MotherShip` uses a fixed short flight range and a low speed.
4. If the star is not reachable (`MotherShip.IsStarReachable`: visited, or within flight range of the nearest safe visited star), flashes the range circle. Otherwise fires `StartTravelSignal(starId)`.
5. `StarMapState` pushes `TravelState`, or first a flight confirmation dialog when the map is in galaxy mode, the trip needs 3 or more fuel, or the player lacks the fuel for a trip of more than 1.

## Visible stars

`GalaxyMap.UpdateVisibleStars` runs on start, on every pan or zoom (`OnPositionChanged`), and on `StarMapContentChanged`.

1. It computes the visible rectangle from the camera and the map offset.
2. It asks the domain for the stars in the rectangle: `StarMap.GetVisibleStars` in star-map mode (every grid position), `StarMap.GetGalaxyViewVisibleStars` in galaxy mode (filtered), or only the current star in system mode.
3. It keeps star objects that are still visible, creates missing ones from the pool, and releases the rest. With `forceUpdate` it re-initializes the kept stars too.
4. It shows the "home star" arrow panel when star 0 is off screen.

`StarMap.ShouldBeVisibleOnGalaxyMap(id)` is true for quest objectives, and for visited stars that have a starbase, or that match an active filter (bookmark, black market, arena, undefeated boss, Christmas).

## Star rendering

[`Star.Initialize`](../Assets/Scripts/Legacy/Map/Star.cs) builds one star object:

1. Seeds `UnityEngine.Random` with the star ID and picks a halo size, rotation, and blink speed. `GetStarColor(id)` picks a yellow, cyan, or red tint the same way. Both calls reseed the global Unity random generator (see [GM1](issues/galaxy_map.md#gm1-star-rendering-reseeds-the-global-unity-random-generator)).
2. Colors the mini dot with the faction color of the region for visited stars, else `UnknownStarColor`.
3. For visited stars, adds a [region border](#region-borders).
4. Adds icons from the pool, each as a child that receives `OnStateChanged(Star.State)` messages:

| Condition | Icon |
| --- | --- |
| Quest objective | `QuestObjectiveIcon` + `MiniObjectiveIcon` |
| Star 0 | `HomeIcon` |
| Visited | `StarName` (bookmark text or star name in faction color) |
| Visited, has starbase | `FactionIcon` + `StarInfo` ("captured" level text or base defense %) |
| Visited, bookmark, shown on galaxy map | `StarInfo` with the bookmark text |
| Guardian present | `GuardianIcon` (aggressive) or `PassiveGuardianIcon` |
| Active local event | `QuestIcon` |
| Then the first match of: survival, undefeated boss, undefeated ruins, Christmas, wormhole, arena, uncompleted challenge, military, undefeated hive, black market | `DangerIcon`, `BossIcon` (+ `MiniBossIcon` when the boss filter is on), `RuinsIcon`, `XmasIcon` (+ mini), `WormholeIcon`, `ArenaIcon` (+ mini), `ChallengeIcon`, `MilitaryIcon`, `PandemicIcon`, `BlackMarketIcon` (+ `MiniShopIcon`) |

`OnStateChanged` shows the halo and core in `Normal` and `StarSystem` states, the small dot in `Map`, the enlarged dot in `Galaxy` (only if the star is shown on the galaxy map and has no mini icon), and nothing when `Hidden`. Icons adjust themselves: [`StarIcon`](../Assets/Scripts/Legacy/Map/StarIcon.cs) scales ×2 in `Map` and ×10 in `Galaxy`; [`StarItemAlign`](../Assets/Scripts/Legacy/Map/StarItemAlign.cs) cancels the parent scale and hides itself per state; [`DistressSignal`](../Assets/Scripts/Legacy/Map/DistressSignal.cs) pulses.

`Deinitialize` returns every child except the halo, core, and dot to the pool.

## Region borders

[`StarBorder`](../Assets/Scripts/Legacy/Map/StarBorder.cs) draws the edge of a faction region around one star. For each of the six neighbors (`StarLayout.GetAdjacentStars`) that belongs to another region, it adds a curved strip (a quadratic Bezier between the half-way points to the neighbors) with 16 segments. Unoccupied and captured regions get no border. The strip is a runtime mesh with the faction color. It is visible in `Normal` and `Map` states.

## Star system view

When the view mode becomes `StarSystem`, `GalaxyMap` moves the `StarSystem` object onto the current star and calls `StarSystem.Initialize(star, color)`.

1. [`OrbitalObjects.Initialize`](../Assets/Scripts/Legacy/Map/System/OrbitalObjects.cs) lays out objects on rings, starting at `MinOrbitRadius` and stepping `OrbitDistance` per ring:
   1. Planets from `Planet.Factory.CreatePlanets(starId)` (the same planets that [exploration](application_layer.md#domain) uses). Asteroid fields become belts; other planets get an orbit circle and a sprite from `PlanetSettings` by type and seed.
   2. Points of interest: faction capital (starbase with up to 20 defending ships), boss fleet with its flagship, wormhole, ruins, Christmas tree, arena, military base, challenge, survival, black market, hive, and local-event beacon. Each is registered with its `StarObjectType`.
2. The player's flagship sprite (database ship `mothership`) is placed outside the last ring.
3. If the star has a guardian (`Occupant`), its ships are spread around random planets. Aggressive guardians fly toward the player.

Clicking in the system view:

1. `OrbitalObjects.TryGetPlanet` or `TryGetPointOfInterest` finds an object near the click.
2. A planet broadcasts `ArrivedToPlanet(planetId)`; a point of interest broadcasts `ArrivedToObject(type)`. [`GameMenu`](../Assets/Scripts/Gui/StarMap/GameMenu.cs) opens the matching panel (planet, boss, faction, wormhole, black market, challenge, and so on).
3. The flagship flies to the object. If guardian ships are near the target, they scatter to other planets and `Occupant.Attack()` starts the guardian battle.

[`Ship`](../Assets/Scripts/Legacy/Map/System/Ship.cs) objects orbit their anchor in circles and fly along Bezier curves when moved. Their speed and orbit come from `System.Random(GetHashCode())`, so they differ between sessions.

## Player ship

[`PlayerShipObject`](../Assets/Scripts/Legacy/Map/PlayerShipObject.cs) sits on the current star and circles it slowly. During travel, `PlayerShipMoved(source, target, progress)` events move it along a Bezier curve toward the target star and play a looping engine sound. `PlayerPositionChanged` snaps it to the new star and stops the sound. In galaxy mode it is drawn large and static. It is hidden in system mode (the system view shows the flagship instead).

## Backgrounds

- [`Background`](../Assets/Scripts/Legacy/Map/Background.cs) sizes a quad to the camera and passes size, position, center, and parallax offset to its material. It uses `_materialHQ` when `GameSettings.QualityMode > 0`. It works on a material copy.
- [`MapBackground`](../Assets/Scripts/Legacy/Map/MapBackground.cs) builds a quad mesh and scrolls two texture layers (`_MainTex`, `_DecalTex`) with the map. It writes texture offset and scale into the shared material asset `Resources/Materials/MapBackground.mat` (see [GM2](issues/galaxy_map.md#gm2-mapbackground-modifies-the-material-asset)).

## Events

| Event (messenger) | Sender | Map reaction |
| --- | --- | --- |
| `PlayerPositionChanged(starId)` | `SignalsTranslator` (from the position signal) | Update the star, move range circle and focus, snap the ship icon, rebuild the system fleet. |
| `FocusedPositionChanged(starId)` | Flight confirmation dialog, quest log panel | Pan to the star (`-1` = current star). |
| `ViewModeChanged(mode)` | `MotherShip` | Open or close the system view, show or hide the ship icon, update visible stars. |
| `StarContentChanged(starId)` | `SignalsTranslator` (from the star content signal) | Re-initialize one star (and the system view if it is the current star). |
| `StarMapContentChanged` | Filters, quests, captures | Re-initialize all visible stars. |
| `PlayerShipMoved(from, to, progress)` | `TravelState`, `RetreatState` | Animate the ship icon. |

Outgoing: `StartTravelSignal(starId)`, and the messenger events `ArrivedToPlanet` and `ArrivedToObject`.

## Prefab catalog

| Prefab | Script | Purpose |
| --- | --- | --- |
| `Stars/Star` | `Star` | One star; holds references to all icon prefabs. Switch distances: 90 / 20 / 4.5. |
| `Stars/StarBorder` | `StarBorder` | Region edge mesh. |
| `Stars/StarName`, `StarInfo` | `StarItemAlign` + `TextMesh` | Name and info labels. |
| `Stars/*Icon` (Home, Quest, QuestObjective, Danger, Faction, Wormhole, Arena, Boss, Ruins, Xmas, Military, Challenge, Guardian, PassiveGuardian, BlackMarket, Pandemic, MiniBoss, MiniShop, MiniArena, MiniObjective, MiniXmas) | `StarIcon`, `StarItemAlign`, `DistressSignal` | Content markers. |
| `Stars/LabIcon`, `MultiplayerIcon`, `AttentionIcon` | — | Referenced by `Star` but never added (laboratory code is commented out); `AttentionIcon` is not referenced. |
| `Stars/ShipIcon` | `ShipIcon` | Not used by `Star`. |
| `Planets/Planet`, `StarBase`, `Ship`, `Orbit`, `AsteroidBelt`, `Wormhole`, `Beacon` | `Planet`, `StarBase`, `Ship`, `Circle`, `DistressSignal` | System-view objects. |
| `Materials/GalaxyMapStar`, `StarSurface` (+ shaders) | — | Not referenced by any scene or prefab. |

## Adding a star icon

1. Create the prefab in `Prefabs/Stars/` with `StarItemAlign` (and `StarIcon` if it should scale with zoom).
2. Add a field to `Star`, assign it in the `Star` prefab, and add the condition in `Star.Initialize`. Keep in mind that most content icons are an `else if` chain: only the first match is shown.
3. If the icon should appear in galaxy mode, update `StarMap.ShouldBeVisibleOnGalaxyMap` and set `_showMiniStarOnGalaxyMap = false` when a mini icon replaces the dot.
4. Make sure the code that changes the content broadcasts `StarContentChanged` or `StarMapContentChanged`.
