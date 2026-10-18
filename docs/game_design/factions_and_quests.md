# Factions and Quests

[Game design index](index.md)

## Purpose

Factions give regions recognizable equipment, opponents, services, and objectives.
Quests provide authored decisions and consequences within the generated galaxy.

## Player actions

1. Discover faction regions and inspect their bases.
2. Accept missions, activate event beacons, or respond to arrival encounters.
3. Choose dialogue actions and meet quest requirements.
4. Fight, trade, deliver items, or travel to marked objectives.
5. Weaken regional defenses, capture a base, or complete another territorial outcome.

## Current rules

### Regional identity and relations

Faction data controls ship availability, encounter selection, research associations, and other content restrictions.
A faction can occupy more than one region.
The campaign stores reputation by region home star, not as one universal relationship value for every region of that faction.
Character relations are stored separately.

Relations are bounded from -100 to 100.
The region's general relation display treats captured and home regions as friendly.
Quest checks can still read the stored regional reputation instead of that displayed value.
Capture therefore does not necessarily rewrite every relationship condition.

Uncaptured faction-region occupants remain aggressive under the normal occupant rule.
After capture, sufficiently weak occupants can become passive through the fleet-power comparison.
Positive reputation alone does not replace that aggression rule.

### Defenses and capture

Defeating regional fleets reduces the base's defense power toward a configured minimum.
The base-defender level uses the home star's level and remaining defense power.
This provides a way to weaken a region before attacking its base.

The built-in starbase attack starts the configured capture quest.
Its attack path sets reputation to -100, starts the base battle, and captures the region after victory.
The direct attack fallback also sets hostile relations and captures on victory.
See the [capture quest](../../Assets/Modules/Database/Resources/Database/Quests/CaptureStarbase.json).

Capture persists for the region.
Quest actions can change capture status, faction identity, reputation, and defense power.
Changing a region to the empty faction makes it behave as a neutral or pirate-base region.
These are authored changes; they do not establish an autonomous galaxy-wide diplomacy simulation.

### Quest entry and choices

Quests can start at game load, through manual triggers, beacon activation, local encounters, star arrivals, new secured systems, faction missions, or timers.
Eligibility can depend on level, location, progress, relationships, inventory, and other requirements.
Most event selection is weighted. Timer-driven Daily selection currently takes the first available quest.

A quest is a graph of steps and transitions.
Steps can present dialogue, branch on requirements, wait for events, start combat, trade, give or remove items, or change regional state.
They can also open workshops and shipyards or start another quest.

The player sees the current objective, dialogue, and available choices rather than the graph itself.
Requirements determine whether a choice or transition is possible.
The quest manager advances actions when the star map can process them.
Combat and other modal activities can therefore delay the next quest action until the player returns.

### Repeat and cancellation rules

| Quest type | Restart restriction | Player cancellation |
| --- | --- | --- |
| Common | No second active instance at the same quest star | Allowed |
| Urgent | No second active instance at the same quest star | Not allowed |
| Singleton | No second active instance anywhere | Allowed |
| Storyline | Cannot start while active or after successful completion | Not allowed |
| Temporary | No restart restriction from quest type | Allowed |

Start requirements and cooldowns can impose additional restrictions.
Singleton means one active instance, not a permanently single-use quest.
Storyline failure is distinct from successful completion under the restart check.

Daily quest cooldowns use active play time, despite the name.
This differs from the real-time cooldowns of some [star activities](activities.md#time-and-repeat-behavior).

### Persistence

Normal quests save their seed, current step, start time, and completion or failure statistics.
Reloading reconstructs the quest at its saved step.
A saved dialogue can appear again, and a saved battle step can request another battle.
The save does not resume a combat simulation in the middle of a fight.

Temporary quests do not save progress or completion history.
Cancellation removes active progress without recording an ordinary completion or failure.
If a mod removes a saved step, reconstruction can restart the quest at its lowest step ID.

## System connections

[Galaxy](galaxy.md) supplies locations and arrival events.
[Combat](combat.md) resolves battle steps.
[Economy](economy.md) handles item exchanges and service costs.
[Progression](progression.md) supplies faction technologies and capture-related skill requirements.

## Design intent and open questions

Quests should use the galaxy and its existing systems to create understandable choices.
Factions should have visible mechanical identities and persistent consequences.
Objectives, costs, cancellation restrictions, and rewards should be clear before commitment.

Open questions:

1. Should friendly regional reputation affect occupant aggression, or should hostility remain tied to capture and fleet strength?
2. Should the interface distinguish regional reputation from relationships with a faction across the galaxy?
3. Should Daily quests use active play time, calendar time, or a less ambiguous player-facing name?

## Sources

- [Region](../../Assets/Scripts/Legacy/Game/Galaxy/Region.cs) and [RegionDataProvider](../../Assets/Scripts/Domain/Quests/RegionDataProvider.cs): relations and defense state.
- [Occupants](../../Assets/Scripts/Domain/Galaxy/StarContent/Occupants.cs) and [StarBase](../../Assets/Scripts/Domain/Galaxy/StarContent/StarBase.cs): aggression and capture entry.
- [CaptureStarbase](../../Assets/Modules/Database/Resources/Database/Quests/CaptureStarbase.json): built-in attack flow.
- [Quests](../quests.md) and [IQuest](../../Assets/Modules/Quests/Scripts/IQuest.cs): selection, actions, repeat rules, and cancellation.
- [Quest save data](../game_session/schema/quests.md): persistence and active-play-time semantics.
- [Faction schema](../database/schema/Faction.md): content identity and availability.
