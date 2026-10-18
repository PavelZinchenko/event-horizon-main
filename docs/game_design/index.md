# Event Horizon Game Design

Event Horizon is a top-down 2D space action RPG about exploration, ship construction, and persistent fleet growth.
The player travels between stars, fights encounters, collects equipment, and develops access to stronger ships and technologies.

This document describes the game from the player's perspective. It combines the existing design principles with rules traced through documentation and code.
Event Horizon: Frontier is a separate game and is outside this document's scope.

## Chapters

| Chapter | Owns |
| --- | --- |
| [Vision](vision.md) | Player fantasy, design pillars, core loops, and content principles |
| [Galaxy](galaxy.md) | World structure, discovery, travel, and planet exploration |
| [Fleet](fleet.md) | Ship ownership, active hangar, fleet selection, and exploration ship |
| [Ship building](ship_building.md) | Layouts, component placement, equipment compatibility, satellites, and activation settings |
| [Combat](combat.md) | Piloting, damage, deployment, timers, victory, defeat, and ship withdrawal |
| [Progression](progression.md) | Player skills, ship experience, research, component quality, perks, and hull expansion |
| [Economy](economy.md) | Resources, reward delivery, trading, crafting, prices, and resource consumption |
| [Factions and quests](factions_and_quests.md) | Regional relations, territorial changes, missions, and quest persistence |
| [Activities](activities.md) | Encounter variants, repeat conditions, arena, holidays, and daily rewards |
| [Player experience](player_experience.md) | Session flow, onboarding, interface feedback, controls, language, and saving |
| [Platforms and mods](platforms_and_mods.md) | Platform services, purchases, advertisements, and mod behavior |

Read Vision first, then follow the chapters for the system being designed or changed.
Each system chapter covers purpose, player actions, current rules, system connections, design intent, open questions, and sources.

## Evidence and scope

The review date is **2026-10-18**. The evidence is the local repository, including its working-tree changes, rather than a verified released build.
This review uses static inspection. It does not establish that every described path works in a running game or connected service.

| Label or section | Meaning |
| --- | --- |
| Current rules | Behavior supported by the cited implementation, data, or technical reference |
| Design intent | Principles carried forward from the previous game design document; these are not claims of complete implementation |
| Inference | A proposed explanation of behavior, without evidence that this was the original designer's reason |
| Open question | A conflict, missing decision, or uncertainty that needs design review |

Current rules describe the built-in game unless a chapter identifies a mod or platform exception.
Code and active data establish current behavior. They do not automatically establish the desired behavior.
Known defects remain defects even when they affect the player.

This document owns the explanation of player decisions and system rules.
Technical documentation owns implementation details. Database records own content and tunable values.
Examples and selected constants explain mechanics; they are not an exhaustive balance sheet or a permanent price guarantee.

## Terms

| Term | Meaning |
| --- | --- |
| Mother ship | The campaign fleet's travel representation on the galaxy map |
| Owned fleet | All ships owned by the player |
| Active fleet | The ships assigned to the hangar for ordinary battles |
| Hull | A ship model with a cell layout, weapon mounts, class, and built-in features |
| Build | A hull with an equipment arrangement; database builds supply many starting and enemy ships |
| Component | An equipment item installed in hull or satellite cells; also called a module in existing text |
| Satellite | An attached equipment platform with its own layout; distinct from a launched drone |
| Perk | A ship modification installed in a dedicated perk slot |
| Star level | The generated difficulty value associated with a map location |
| Ship level | The level derived from an individual ship's experience |
| Player level | The level that supplies skill points |
| Region | A group of stars associated with a home star and regional state |
| Credits / stars / tokens | Separate currencies; currency stars are distinct from stars on the map |
| Research points | A faction-specific resource used for technology and some crafting costs |

## Maintenance

1. Put each rule in its owning chapter and link to it from other chapters.
2. Check the live caller before treating a class, schema field, or enum value as a playable feature.
3. Record platform conditions beside rules that differ between builds.
4. Link a disputed behavior to its technical issue instead of duplicating the defect analysis.
5. Update current rules and source links when code or data changes the player experience.
6. Label new design proposals before treating them as accepted intent.

Start implementation research at [Architecture](../architecture.md), [Application layer](../application_layer.md), and [Database](../database.md).
The [project structure](../project_structure.md) maps the source folders. Existing defects are tracked in [component issue documents](../issues/).
