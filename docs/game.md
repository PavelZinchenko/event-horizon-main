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

## Game Systems in the Current Build

The sections above set the design intent. This section describes what the current code and built-in database actually do. It is a snapshot: values come from the database and code, and they change. Each part links to the technical doc that owns the detail.

One gap between intent and code: the design treats retreat as a normal decision, but under the default combat rules the player can change ships during a battle only after learning the rescue-unit skill ([CombatRules: Retreat](database/schema/CombatRules.md#retreat)).

### Session flow

1. The game loads mods and the database, signs in to the platform account, and loads or creates the save.
2. The main menu offers: continue or start a new game, quick battle, settings, the controls test range, and the in-game encyclopedia (Ehopedia, currently suspended). Mobile and web builds also show the daily reward.
3. A new game sets the start fuel, the starting inventory, and the starting fleet from `GalaxySettings`. The first frigate becomes the exploration ship.
4. The star map is the hub. Travel, battles, planet exploration, the skill tree, the ship editor, shops, the workshop, and the shipyard all start from it and return to it.
5. The game saves when the star map loads, when the application pauses or loses focus, after a purchase, and on exit.

See [Application layer: States](application_layer.md#states) and [Session lifecycle](application_layer.md#session-lifecycle).

### Galaxy and star content

The galaxy is a spiral of star IDs. The game seed and the star ID give each star its position and level. The star level grows with distance from the start and drives almost every encounter value. Regions group stars around a faction home star. The player's home region and unoccupied regions have no faction. See [CommonComponents: Galaxy layout](common_components.md#galaxy-layout) and [GalaxyMap](galaxy_map.md).

A star can hold enemy occupants and one or more special objects. The game rolls a number from 0 to 999 for each star and maps ranges to objects. Some objects appear only in faction regions, some only in neutral space.

| Object | Share of stars | Where | What the player does | Repeat |
| --- | --- | --- | --- | --- |
| Occupants | – | Any star | Fight a fleet sized by the star level. A fleet much weaker than the player's becomes passive. Faction regions send faction defenders. Victory can give research progress. | Once; the star becomes safe |
| Faction starbase | Home star of each faction region | Faction regions | Starts the capture quest. Capture marks the region captured. | Once per region |
| Boss | 5 % (2 % outside faction regions) | Any | Fight a flagship fleet. The reward includes faction research. | Respawns after 24–72 hours, 5 levels stronger |
| Ruins | 2.5 % | Neutral | Fight guards of the abandoned-starbase faction for special loot. | Once |
| Survival | 2.5 % | Neutral | Fight a fleet that sends ship after ship under survival rules. | Every 4 hours |
| Challenge | 2 % | Any | Fly a fixed loaner ship against 5 ever-larger enemies, one per attempt. Each stage gives a component; the last also gives 1–3 stars. | Until all 5 stages are won |
| Local event | 10 % | Neutral | Activate a beacon that starts a random quest. | After 5–29 days |
| Wormhole | 2.5 % | Any | Jump to the next wormhole further along the star IDs. | Always |
| Arena | 2.5 % | Faction regions | Asynchronous PvP (see [Arena](#arena)). | Battle limit |
| Black market | 2 % | Neutral | Buy ships, satellites, and high-quality components. | Always |
| Military base | 1 % | Any; not in builds without purchases | Buy ship levels for credits or stars, up to max(5, star level / 2). | Always |
| Hive | 5 % | Faction regions | An infected planet. Explore all its objectives to defeat it. | Once |
| Christmas tree | 1 % | Any, only during Christmas | Fight the holiday fleet for holiday rewards. | Once |

Eighteen stars near the start get a fixed, shuffled set: ruins, a black market, a challenge, a boss, three local events, survival, a wormhole, and a Christmas tree in season. See [Application layer: Galaxy model](application_layer.md#galaxy-model).

### Travel

- The mother ship jumps between stars within its flight range. Travel takes time and consumes fuel. The engine skill raises both range and speed.
- The fuel-tank skill sets the fuel capacity. Fuel never exceeds it.
- A quest can make the fleet retreat to the nearest safe visited star.
- The map has bookmarks and filters. It shows region borders and an icon for each star object.

See [GalaxyMap](galaxy_map.md) and [SkillSettings](database/schema/SkillSettings.md).

### Factions and territory

- Each region has a relation value to the player. Attacking a starbase sets it to −100.
- Capturing a starbase captures the whole region. Defenders in an uncaptured faction region always attack; in a captured region they attack only when they are not much weaker than the player's fleet.
- Quests can change the owner of a region. An owner of `0` makes the region neutral, which the game treats as a pirate base.
- Faction records control where a faction's ships wander, whether merchants sell them, and the faction's research tree.

See [Faction](database/schema/Faction.md) and [Quests](quests.md).

### Resources and currencies

| Resource | Main sources | Main uses |
| --- | --- | --- |
| Credits | Combat loot, sales, quests | Shops, crafting, unlocking components, ship levels |
| Stars (premium) | Purchases, challenges, some loot | Crafting, perk installation (10 stars each), the military base, shops |
| Tokens | Arena victories | The arena shop |
| Fuel | Loot | Travel; 10 per planet exploration |
| Research points (per faction) | Research items from bosses and some fleets | Researching technologies and crafting |
| Quest items | Exploration, loot, quests | Commodities to sell or use in quests; artifacts; Christmas snowflakes |
| Player experience | Combat experience of the fleet | One skill point per player level |
| Ship experience | Damage dealt in combat | Ship level and power |

See [Economy: IapData](game_session/schema/economy.md#iapdata), [QuestItem](database/schema/QuestItem.md), and [Application layer: Domain](application_layer.md#domain).

### Ship progression

- **Ship level.** Level *n* needs 100 × *n*³ experience. Each level multiplies armor, shield, damage, and ramming; the factor is 11^(level / 100). Skills cap the level. The "exceed the limits" skill raises the cap.
- **Component quality.** A component can be degraded (N3–N1), plain (P0), or improved (P1–P3). Every grade except P0 adds a modification; better grades appear in loot of higher levels.
- **Perks.** A ship has up to three perk slots. Crafted ships get one to three empty slots by quality. Perks give resistances, lighter weight, auto-aim, larger satellites, faster recharge, or bionic regeneration. Several have a cost, for example less damage or less energy.
- **Satellites.** Each ship has two satellite slots. The ship class or model size limits the satellite size. Starbases take no satellites.
- **Locked components.** Ships from builds come with locked components. The player unlocks one for twice its credit price. The component must have a technology, and some components need that technology researched.
- **Shipyard.** A faction shipyard expands the hull layout (the ship and the shipyard need a minimum level) and repaints ships.
- **Bionic hulls** repair 1 % of their armor per second and accept a different set of components.

See [ShipConstructor](ship_constructor.md) and [ShipEditor](ship_editor.md).

### Research and crafting

- Each faction has a research tree of technologies with dependencies. A technology makes one component, ship, or satellite available.
- Research points are per faction. Boss victories and some fleet victories give them.
- The workshop crafts researched items for credits, stars, and research points. Each item needs a minimum workshop level. Crafting skills lower the price and the level.
- Blueprints in loot give technologies directly.
- Faction workshops and shipyards open through quests, usually at faction starbases.

See [Technology](database/schema/Technology.md).

### Player skills

The skill tree is a fixed layout loaded from `Resources/skills.json`. Each player level gives one skill point. Skills cover:

- the mother ship: fuel capacity, engine power (range and speed), the rescue unit (ship change in combat), planetary and space scanners;
- combat: attack, defense, shield strength and recharge, heat, energy, and kinetic resistance, ship experience;
- the economy: trading, master trader, crafting price, crafting level;
- the fleet: five hangar slots;
- requirement nodes: a maximum-level requirement, "beat all enemies" (capture the listed faction starbases), and "exceed the limits".

See [SkillSettings](database/schema/SkillSettings.md).

### Planet exploration

- A star system can have gas, barren, terran, and infected planets. Exploring one costs 10 fuel and uses the exploration ship.
- Objectives are meteorites, minerals, rare minerals, wrecks, containers, and outposts with turrets. A terran planet can have an outpost as its first objective.
- Infected planets hold a hive boss with guardians. Hive rewards come from the infected faction.
- Gas clouds damage ships. The exploration-loot skill increases the loot.

See [ExplorationSettings](database/schema/ExplorationSettings.md) and [BattleSimulator](battle_simulator.md).

### Battle types

Every battle uses a combat-rules record. The rules set the starting and maximum enemy count, the map size, the round time limit and what happens when it runs out, loot and experience conditions, ship selection, skill bonuses, asteroids, the background planet, and the music.

| Rules | Used by |
| --- | --- |
| Default | Star occupants; quest battles without their own rules |
| Quick combat | Quick battle from the main menu |
| Survival | Survival stars |
| Faction starbase | Starbase attacks |
| Flagship | Bosses and ruins |
| Arena | Arena battles |
| Challenge | Challenge stars |
| Christmas | Christmas trees |

The star level sets other values. The round time falls from 100 to 40 seconds, larger hull classes appear every 25 levels, and enemy satellites appear from level 10. See [CombatRules](database/schema/CombatRules.md) and [ShipConstructor: Game formulas](ship_constructor.md#game-formulas).

### Quests

Quests are database graphs that start on game events, for example arrival at a star, a local event, a holiday, or a new session. Nodes show dialogs, start battles, give or take items, change relations, open shops, workshops, and shipyards, and capture or hand over regions. The quest type controls restarts: common, singleton, storyline (no cancel, no restart), temporary, and urgent (no cancel). See [Quests module](quests.md) and [Quest](database/schema/Quest.md).

### Arena

The arena is asynchronous PvP. The player uploads the active fleet to the server and fights a fleet that another player uploaded. Matchmaking picks an opponent with a similar rating. The opponent's fleet gets a power bonus from its rating, and a victory pays tokens. The player has 10 battles per recovery window. A rewarded ad adds a battle. Ships that fail the arena rules cannot take part. See [Server: Arena](server.md#arena).

### Special events

`HolidayManager` uses internet time to detect Christmas, Halloween, and Easter. It starts the event quest of each holiday. Christmas also adds Christmas-tree stars and a shop that sells high-quality items for snowflakes. A daily reward grows with the number of consecutive days. See [SpecialEventSettings](database/schema/SpecialEventSettings.md).

### Platform features and monetization

- **Accounts:** Google Play Games on Android, Game Center on iOS, Steam on PC. Cloud saves exist on Android and iOS. Some platforms also export and import saves.
- **Purchases:** six star packs and a supporter pack. Builds without purchases have no military bases.
- **Ads (mobile):** a rewarded video gives loot in the store or an arena battle. An interstitial ad shows at start-up unless the player removed ads.
- **Open-source builds** replace all of these with stubs.

See [Services](services.md), [ModulesPrivate](modules_private.md), and [Build pipeline](build.md).

### Mods

A mod is a full or partial game database. The player selects it in the settings. It can replace or extend ships, components, factions, quests, rules, and text. Mods come from a folder or a packed file, which can be encrypted. Authors edit them with the external database editor. See [Database: Mods and merge rules](database.md#mods-and-merge-rules) and [Database editor](database/editor.md).

### Controls and languages

- Combat supports touch, keyboard, mouse, and gamepad. Players can rebind keys. See [GameServices: Input](game_services.md#input).
- The ship editor sets a key and an activation mode for each weapon and device. See [ShipEditor: Key bindings](ship_editor.md#key-bindings-and-component-modes).
- The game has 14 languages. Mods can add or override text. See [Localization](localization.md).

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
- technical architecture documentation (start at [Architecture](architecture.md));
- platform-specific release and monetization requirements.

When implementation and this document disagree, determine whether the implementation is legacy behavior, the document is outdated, or the design has intentionally changed. Update the relevant documentation and data together.

## Public References

The design summary is grounded in the current repository and the official pages linked by Zipa Games. Public feature counts and platform-specific descriptions vary by release, so they are supporting evidence rather than fixed requirements.

- [Zipa Games](https://zipagames.com/) - Official project and community hub.
- [Event Horizon on Steam](https://store.steampowered.com/app/465000/Event_Horizon/) - Core exploration, combat, research, resources, trade, construction, upgrade, faction, and planet-exploration description.
- [Event Horizon on itch.io](https://pavel-zinchenko.itch.io/event-horizon) - Current sandbox, fleet, grid customization, mission, faction, modding, and open-source overview.
- [Event Horizon on Google Play](https://play.google.com/store/apps/details?id=com.ZipasGames.EventHorizon) - Mobile-facing description of exploration, real-time arcade combat, ships, upgrades, factions, and PvP.
- [Event Horizon on the App Store](https://apps.apple.com/us/app/event-horizon-cosmic-rpg/id1098794574) - iOS-facing description and version history.
