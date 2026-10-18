# Galaxy

[Game design index](index.md)

## Purpose

The galaxy connects exploration, resource planning, and encounter choice.
Travel exposes the player to new equipment, factions, quests, and increasing difficulty.

## Player actions

1. Inspect stars, region borders, encounter icons, and quest markers.
2. Choose a reachable destination and review its travel cost.
3. Resolve hostile occupants or use available local services.
4. Explore planets, enter wormholes, or continue toward another objective.
5. Bookmark useful locations and revisit unfinished activities.

## Current rules

### World structure and discovery

The game generates star positions and levels from star IDs and the game seed.
Star level broadly increases with distance from the starting area.
It influences enemy generation, rewards, and activity difficulty.
Regions associate nearby stars with a home star and regional faction state.

The save records discoveries and changes rather than storing the entire generated galaxy.
Visited stars and cleared stars are distinct: a visited location can still have hostile occupants.
The space-scanner skill marks nearby stars visited after a qualifying arrival.

Ordinary occupants and special star objects are separate layers of content.
Special objects include bases, bosses, ruins, survival encounters, challenges, local events, wormholes, arenas, markets, and hives.
Their placement combines deterministic random ranges, faction restrictions, platform conditions, and holiday conditions.
The [activity chapter](activities.md) owns their encounter and repeat rules.

Eighteen selected stars near the start receive a shuffled introductory set, with unused entries left empty.
The set includes a boss, ruins, a market, a challenge, three local events, survival, and a wormhole.
Christmas adds its seasonal object. These placement rules currently live in code.

### Travel

Reachability is measured against the discovered network.
A visited destination passes the reachability check.
An unvisited destination must lie within flight range of its nearest visited, non-aggressive star.
Fuel cost and travel duration still use the distance from the current position.

| Rule | Current behavior |
| --- | --- |
| Fuel cost | `max(1, floor(distance))` |
| Travel duration | `distance / flight speed` |
| Normal range and speed | Derived from the mother-ship engine skill |
| Fuel capacity | Derived from the fuel-tank skill |
| At zero fuel | Range becomes 1.5 and speed becomes 0.1 in map units |
| Insufficient fuel | Travel costing more than one fuel fails; a one-fuel trip can proceed without payment |

The zero-fuel rule provides a slow travel path. It does not provide free long-distance travel.
The map shows a fuel warning, and longer or unaffordable trips use a confirmation dialog.
See [MotherShip](../../Assets/Scripts/Domain/Player/MotherShip.cs) and [TravelState](../../Assets/Scripts/GameStateMachine/States/TravelState.cs).

Wormhole entry assigns and saves a destination when needed, then changes the current star directly.
The search selects a later, unpaired wormhole in star-ID order.
Wormhole entry does not use the ordinary flight fuel calculation.

A quest can trigger campaign retreat to the nearest visited, non-aggressive star.
The fallback is the previous player position. This retreat does not deduct ordinary travel fuel.
It is separate from [withdrawing a ship during combat](combat.md#ship-selection-and-withdrawal).

### Planet exploration

Planet types include barren, terran, gas, asteroid, and infected locations.
The player selects an owned frigate as the exploration ship.
Every entry costs 10 fuel. The first entry has no additional premium price.

A repeat entry costs `min(10, 1 + floor(star level / 5))` premium price units.
That means stars in purchase-enabled builds, or the converted credit price in builds with purchases disabled.
The entry button disappears when all objectives are complete.
These checks are in [PlanetPanel](../../Assets/Scripts/Gui/Exploration/PlanetPanel.cs).

Exploration generates objectives such as containers, wrecks, meteorites, minerals, defended outposts, and hives.
The player scans ordinary objects or destroys hostile objectives.
Gas clouds can damage the ship. Infected locations use a designated faction and include hive guardians.

Each completed objective records progress and grants its loot immediately.
Completed objectives remain cleared on later visits.
Leaving or losing the exploration ship ends the excursion; it does not roll back rewards already granted.
This differs from the victory-dependent reward rules of ordinary combat.

## System connections

Travel and exploration consume [economic resources](economy.md).
Exploration requires a suitable [fleet ship](fleet.md#exploration-ship).
Discovery exposes [activities](activities.md) and [faction objectives](factions_and_quests.md).
Skills alter travel, discovery, and exploration rewards through [progression](progression.md).

## Design intent and open questions

Discovery should expose meaningful opportunities and recognizable danger.
Travel should require planning while preserving a way to recover from resource shortage.
Planet exploration should reward a suitable build and clear risk assessment.

Open questions:

1. Does the travel interface explain network-based reachability and the zero-fuel exception clearly enough?
2. Does the repeat-entry premium cost support the intended recovery experience after exploration failure?
3. Should placement rules remain fixed in code or become part of editable world-generation data?

## Sources

- [GalaxyMap](../galaxy_map.md) and [CommonComponents: galaxy layout](../common_components.md#galaxy-layout).
- [StarData](../../Assets/Scripts/Domain/Galaxy/StarData.cs) and [StarMapManager](../../Assets/Scripts/Domain/Player/StarMapManager.cs): placement and discovery.
- [MotherShip](../../Assets/Scripts/Domain/Player/MotherShip.cs), [TravelState](../../Assets/Scripts/GameStateMachine/States/TravelState.cs), and [RetreatState](../../Assets/Scripts/GameStateMachine/States/RetreatState.cs): movement rules.
- [Wormhole](../../Assets/Scripts/Domain/Galaxy/StarContent/Wormhole.cs): route selection.
- [Planet](../../Assets/Scripts/Domain/Exploration/Planet.cs), [PlanetPanel](../../Assets/Scripts/Gui/Exploration/PlanetPanel.cs), and [ExplorationSceneManager](../../Assets/Scripts/Combat/Manager/ExplorationSceneManager.cs): exploration costs, objectives, and reward delivery.
- [World save data](../game_session/schema/world.md): persistent discovery and exploration state.
