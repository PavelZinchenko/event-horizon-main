# Fleet

[Game design index](index.md)

## Purpose

The owned fleet gives the player a collection of ships to develop.
The active fleet turns that collection into a limited set of choices for encounters.

## Player actions

1. Acquire ships through starting equipment, rewards, markets, or crafting.
2. Assign suitable ships to the active hangar.
3. Reorder the active fleet and prepare specialized builds.
4. Select an owned frigate for planet exploration.
5. Refit, develop, rename, repaint, or sell eligible ships.

## Current rules

### Ownership and active selection

The owned fleet and active fleet are separate collections.
A ship can remain owned without occupying an active hangar slot.
Ordinary campaign battles use the active fleet.
Challenges and quick combat can supply different fleets.

A ship includes its hull, installed components, optional satellites, perks, experience, and appearance.
Acquiring a ship also records that hull as unlocked in the session statistics.
Prebuilt ships can contain locked components; their removal rules belong to [Ship building](ship_building.md#locked-equipment).

### Hangar capacity

Hangar skills control capacity across frigates, destroyers, cruisers, battleships, and titans.
The base cumulative capacity is three ships, of which at most one can be a destroyer.
Larger classes require the corresponding hangar skills.

The class capacities are cumulative thresholds rather than independent pools of extra ships.
The validator derives exact slot classes from those thresholds.
A smaller ship can use an available larger-class slot.
The same ship cannot be added twice through the normal squad-add operation.

Loading a session or resetting skills rechecks the active fleet.
Ships that exceed available slots leave the active group, but remain owned.
See [PlayerSkills](../../Assets/Scripts/Domain/Player/PlayerSkills.cs) and [ShipSquad](../../Assets/Scripts/Domain/Player/ShipSquad.cs).

### Battle participation

Ordinary combat deploys a single player ship at a time.
The remaining active fleet supplies replacement or withdrawal choices under the encounter's rules.
Fleet order matters when the rules select ships automatically.
Ship-launched drones and other support units are equipment effects, not extra hangar deployments.

Combat destruction normally affects the ship's availability within that encounter.
The ordinary combat model does not remove the corresponding owned ship.
Quests can impose their own item or ship changes, so their outcomes need separate review.
The detailed [combat rules](combat.md) own defeat, surrender, and withdrawal behavior.

### Exploration ship

The exploration ship must be an owned frigate.
It is selected separately from the active combat group.
Exploration uses that ship's build and player skill bonuses.
Changing the exploration ship therefore provides a separate preparation choice for environmental hazards and objective collection.

### Fleet power and restrictions

The campaign estimates fleet power by summing active-ship threat values.
The estimate considers ship level, hull and satellite cell counts, and additional threat class.
It is not a complete prediction of weapon matchups or piloting performance.
Some occupants become passive when this estimate makes them much weaker than the player.

Ships have availability and sale restrictions.
Unique ships cannot be sold through the standard sale rule.
The arena applies its own eligibility check to uploaded ships.

## System connections

[Ship building](ship_building.md) defines what each ship can carry.
[Progression](progression.md) defines hangar skills and ship development.
[Economy](economy.md) defines acquisition and sale paths.
[Combat](combat.md) determines how the selected roster enters battle.

## Design intent and open questions

Fleet growth should increase the player's tactical options.
Small ships should retain useful roles through fitting, piloting, and exploration requirements.
The interface should distinguish ownership from battle eligibility and active selection.

Open questions:

1. How clearly should the game explain that the threat indicator omits important matchup details?
2. Does the hangar interface explain cumulative class limits and the use of larger slots by smaller ships?
3. Should the exploration-frigate requirement receive explicit onboarding rather than only an eligibility restriction?

## Sources

- [PlayerFleet](../../Assets/Scripts/Domain/Player/PlayerFleet.cs): ownership, active group, exploration selection, and validation.
- [ShipSquad](../../Assets/Scripts/Domain/Player/ShipSquad.cs) and [PlayerSkills](../../Assets/Scripts/Domain/Player/PlayerSkills.cs): hangar limits.
- [ShipConstructor](../ship_constructor.md): ship model, threat calculation, sale rules, and validation.
- [CombatManager](../../Assets/Scripts/Combat/Manager/CombatManager.cs) and [ShipInfo](../../Assets/Scripts/Combat/Domain/ShipInfo.cs): encounter deployment and destruction state.
- [Fleet save data](../game_session/schema/fleet.md): ownership and assignments across sessions.
