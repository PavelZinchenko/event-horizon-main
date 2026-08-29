# Event Horizon Game Design

This is the high-level game design document for **Event Horizon**. It describes the durable player experience, design principles, and foundational mechanics that new features should preserve. It is intentionally not a balance sheet or a catalog of every ship, weapon, faction, and quest; those values are data-driven and change over time.

Event Horizon: Frontier is a separate game and is outside the scope of this document.

## Game Overview

Event Horizon is a top-down 2D space action RPG and exploration sandbox. The player commands a growing fleet, travels through a vast galaxy, meets competing factions, completes missions, captures territory, and fights real-time battles between groups of spaceships.

The central fantasy is to begin as a lightly equipped space pilot and become an experienced fleet commander whose ships, technology, alliances, and territory reflect the player's choices. Progress comes from learning how ships behave, building effective loadouts, taking calculated risks, and adapting the fleet to increasingly dangerous regions and encounters.

### Product identity

- **Genre:** Top-down 2D action RPG, space exploration, and sandbox fleet-building game.
- **Perspective:** A strategic galaxy map connected to real-time top-down combat and planet-exploration scenes.
- **Primary experience:** Long-form progression through exploration, combat, collection, customization, research, trade, missions, and faction interaction.
- **Play style:** Accessible in short sessions, with persistent goals that can span many sessions.
- **Platforms:** PC, mobile, and web builds share the same core game; input, storefront services, and monetization may vary by platform.
- **Content model:** Data-driven and moddable. Ships, components, factions, quests, combat rules, and balance should remain content rather than hard-coded assumptions wherever practical.

## Design Pillars

### 1. Exploration creates opportunity and danger

The galaxy should feel much larger than any single playthrough. Every journey can reveal stars, planets, enemies, missions, merchants, faction territory, resources, bases, bosses, or routes through wormholes. Discovery is useful, not merely cosmetic: it opens new decisions and feeds progression.

Distance matters. Travel consumes fuel and time, flight range limits available destinations, and opposition generally becomes stronger farther from the starting area. Moving into unknown space should therefore be a deliberate risk with the promise of better discoveries and rewards.

### 2. Ship construction is a creative problem

Ships are not fixed stat packages. Each hull has a spatial layout that the player fills with compatible modules. Weapons, defenses, energy systems, engines, drones, and special equipment compete for limited cells and must work as a coherent build.

A strong loadout should result from meaningful trade-offs rather than simply equipping the rarest items. Size, shape, position, energy demand, mobility, protection, weapon range, damage type, support systems, and intended combat role should all matter. Different solutions should remain viable for different ships, enemies, missions, and player preferences.

### 3. Real-time combat rewards piloting and preparation

Combat is immediate, physics-driven, and readable from a top-down view. Fleets or groups oppose one another under encounter-specific rules. The player combines direct ship control with fleet composition and AI-controlled support, while enemies use behavior-driven tactics.

Success should depend on both layers:

- **Before combat:** choose ships, roles, modules, upgrades, and an appropriate order or deployment strategy.
- **During combat:** maneuver, aim, manage range and facing, use weapons and devices, protect vulnerable allies, focus targets, and decide when to continue or retreat.

Preparation must never completely replace player skill, and mechanical skill must not make progression or build choices irrelevant.

### 4. Risk turns into persistent growth

Combat, exploration, missions, trade, salvage, and research supply the resources needed to improve the fleet. Rewards may include currency, materials, research progress, experience, ships, components, satellites, or access to new technology.

Growth is expressed through several connected systems:

- acquiring and refitting ships;
- upgrading ships and individual modules;
- researching faction technologies and dependencies;
- developing player skills such as combat bonuses, command capacity, engineering, prices, learning, loot, or salvage;
- expanding hangar and fleet options;
- improving access to distant and more dangerous space.

Progression should unlock new decisions, not only larger numbers.

### 5. The galaxy is a persistent faction sandbox

Factions have identities expressed through territory, ships, technologies, bases, missions, merchants, and relationships. The player may cooperate, trade, take assignments, fight fleets, assault or liberate starbases, and change the balance of power in a region.

The sandbox should react clearly to consequential actions. Captured bases, faction relations, military power, discovered stars, secured systems, defeated bosses, quest progress, and unlocked wormhole routes persist in the save game.

### 6. Challenge grows through variety and adaptation

The early game should teach travel, combat, fitting, and rewards with understandable encounters. Later regions and special activities should demand specialized builds and stronger execution rather than relying only on inflated health and damage.

Encounter variety can come from fleet composition, AI behavior, battle-map conditions, time limits, hazards, objectives, ship-selection rules, bases, bosses, planet environments, and reward conditions. When a build fails, the player should be able to understand why and form a new plan.

## Core Gameplay Loop

The main loop is:

1. Review the galaxy, nearby threats, faction activity, and available missions.
2. Choose a destination or objective and prepare a fleet for the expected risk.
3. Spend fuel and travel to another star, using known routes and flight range.
4. Explore, trade, accept a mission, investigate a planet, or engage an enemy group.
5. Resolve the encounter through real-time combat or another activity.
6. Collect rewards, experience, loot, research progress, or territorial gains.
7. Repair, trade, research, upgrade, construct, and refit ships.
8. Push toward a more difficult region or pursue a longer-term faction, collection, or progression goal.

Each trip should produce at least one of three things: useful material progress, new knowledge about the galaxy, or an interesting decision about what to do next.

## Foundational Mechanics

### Galaxy map and travel

- The galaxy contains a very large number of stars generated from a saved seed.
- Stars transition from unknown to discovered and may later be secured or marked as containing enemies.
- The player occupies a specific star and travels between reachable destinations.
- Fuel, flight range, and flight speed make route planning and ship progression meaningful.
- Bookmarks help the player attach intent and memory to important locations.
- Faction regions, captured bases, bosses, planets, missions, and wormhole connections give space strategic structure.
- Enemy level and the availability of larger hull classes can scale with distance, creating a natural outward difficulty curve.

The map must communicate reachability, cost, danger, ownership, and available activity before the player commits to travel.

### Fleet and hangar

- The player owns multiple persistent ships and assigns them to hangar or fleet slots.
- One ship may be designated for planet exploration, while the combat roster is selected according to the encounter rules.
- Ships retain identity through name, experience, color scheme, installed components, upgrades, modifications, layout changes, and satellites.
- Hull classes range from small frigates through destroyers, cruisers, battleships, and titans; starbases, drones, flagships, and special ships have distinct roles.
- Fleet capacity is a progression constraint. Adding a ship should create a tactical choice, not be an automatic upgrade.

Fleet design should encourage complementary roles such as damage, interception, durability, support, repair, drone control, or base assault.

### Ship construction and equipment

Each hull provides a grid-like layout. Components occupy cells and can have placement, slot, hull, size, or other compatibility restrictions. The principal equipment categories are:

- **Weapons:** Produce damage through different delivery methods, ranges, behaviors, and damage types.
- **Defense:** Improve survivability through hull, shields, resistance, repair, or related protection.
- **Energy:** Supply and manage the power required by weapons and ship systems.
- **Engines:** Determine acceleration, speed, maneuverability, and the ability to control engagement range.
- **Drones:** Deploy autonomous offensive, defensive, or support craft.
- **Special systems:** Add unusual active or passive behavior that can define a build.

Components may be looted, bought, researched, crafted, modified, and upgraded. The editor must clearly show why an item can or cannot be installed and how a change affects the complete ship, not just one isolated stat.

### Real-time combat

Battles take place on bounded 2D maps and may include ships, drones, asteroids, planets, bases, hazards, and reinforcements. Encounter rules can define initial and maximum enemy counts, time limits, deployment and ship-selection behavior, retreat restrictions, environmental objects, and the conditions for loot or experience.

The combat rhythm is:

1. Read the opposing fleet and battlefield.
2. Establish range and choose priority targets.
3. Maneuver while balancing offense, defense, energy, and positioning.
4. Coordinate with allied AI and react to enemy behavior or reinforcements.
5. Destroy, outlast, complete the objective, or retreat when the risk exceeds the likely reward.

Weapons and effects must provide clear visual and audio feedback. A player should be able to identify the source and type of a major threat, recognize whether attacks are effective, and understand why a ship was disabled or destroyed.

### Encounters and activities

The main systems support multiple activity types:

- hostile fleet battles and wandering encounters;
- starbase assaults, captures, and liberations;
- faction and storyline quests with dialogue, choices, requirements, rewards, and relationship changes;
- planet exploration with ships, defenses, enemies, loot, and environmental hazards;
- boss and flagship battles;
- survival, arena, challenge, and quick-combat rule sets;
- research, crafting, shopping, trade, and resource collection;
- PvP or social features in builds and platforms where they are supported.

These activities should reuse the same ships, components, and progression systems while changing the tactical problem.

### Economy, rewards, and research

The economy connects field activity to fleet improvement. The persistent resource model includes money, fuel, premium or special currencies where applicable, research points, and inventory resources. The exact sources, sinks, prices, and quantities are balance data and may differ by release or platform.

The economy should follow these principles:

- rewards should relate to challenge, distance, and player commitment;
- travel, fitting, crafting, and upgrading should create useful trade-offs without trapping the player;
- research dependencies should guide discovery while leaving room for faction and build choice;
- duplicate or unwanted loot should retain value through sale, crafting, salvage, or another conversion path;
- monetization must not become a requirement for understanding or enjoying the core loop.

### Quests and factions

Quests provide authored structure inside the sandbox. They may be common, one-time, storyline, temporary, or urgent. Quest flow can include dialogue, random or conditional branches, fleet and base combat, exploration, trade, receiving or removing items, retreat, relationship changes, faction changes, and territorial outcomes.

Quest design should:

- give a clear objective and enough context to make a decision;
- use the galaxy and core mechanics rather than creating disconnected minigames;
- make faction identity visible through goals, technology, ships, rewards, and behavior;
- communicate time limits, failure conditions, and irreversible consequences before commitment;
- preserve player agency where a meaningful choice is offered.

### Difficulty, failure, and recovery

Difficulty rises primarily as the player travels farther, meets advanced factions, and selects harder activities. Ship size, enemy level, fleet composition, AI, hazards, and encounter rules can all contribute.

Failure should cost time, opportunity, fuel, rewards, or combat progress, but the game should preserve a practical route to recovery. Retreat is a legitimate strategic decision unless explicitly disabled by a special encounter. The player should be able to return to safer space, earn resources, change a build, improve piloting, and try a new approach.

## Player Experience Requirements

- **Readable:** Important map, fitting, and combat information is understandable at a glance on both large and small screens.
- **Responsive:** Steering, aiming, firing, device activation, menus, and drag-and-drop fitting react immediately and consistently.
- **Explainable:** Losses, restrictions, costs, rewards, and stat changes expose their causes.
- **Discoverable:** The game teaches through a gentle opening, contextual guidance, clear feedback, and an in-game reference rather than requiring external knowledge.
- **Flexible:** Touch, mouse and keyboard, and other supported input methods preserve equivalent tactical control.
- **Persistent:** Save data retains the fleet, inventory, research, skills, quests, map discovery, relationships, and territorial progress across sessions and supported platforms.
- **Performant:** Large battles remain responsive and visually legible at the target simulation rate on supported hardware.

## Content and Balance Guidelines

- Prefer new tactical roles, interactions, and counters over simple stat escalation.
- Give factions recognizable mechanical identities as well as distinct art and narrative.
- Keep early choices forgiving; make advanced optimization deep without making the editor opaque.
- Avoid a single universally best hull, weapon, defense, or fleet composition.
- Ensure dangerous destinations advertise risk and provide rewards that justify it.
- Test new content in the full progression economy, not only in isolated combat.
- Treat storefront counts such as the advertised number of ships or modules as current catalog facts, not permanent design constraints.
- Keep platform-specific advertising, purchases, social systems, and store services outside the core balance assumptions.

## Scope Boundaries

This document is normative for the game's high-level experience but does not replace:

- database content and schemas for exact mechanics and values;
- quest specifications and dialogue scripts;
- economy and progression balance sheets;
- UI flows and wireframes;
- technical architecture documentation;
- platform-specific release and monetization requirements.

When implementation and this document disagree, determine whether the implementation is legacy behavior, the document is outdated, or the design has intentionally changed. Update the relevant documentation and data together.

## Public References

The design summary is grounded in the current repository and the official pages linked by Zipa Games. Public feature counts and platform-specific descriptions vary by release, so they are supporting evidence rather than fixed requirements.

- [Zipa Games](https://zipagames.com/) - Official project and community hub.
- [Event Horizon on Steam](https://store.steampowered.com/app/465000/Event_Horizon/) - Core exploration, combat, research, resources, trade, construction, upgrade, faction, and planet-exploration description.
- [Event Horizon on itch.io](https://pavel-zinchenko.itch.io/event-horizon) - Current sandbox, fleet, grid customization, mission, faction, modding, and open-source overview.
- [Event Horizon on Google Play](https://play.google.com/store/apps/details?id=com.ZipasGames.EventHorizon) - Mobile-facing description of exploration, real-time arcade combat, ships, upgrades, factions, and PvP.
- [Event Horizon on the App Store](https://apps.apple.com/us/app/event-horizon-cosmic-rpg/id1098794574) - iOS-facing description and version history.
