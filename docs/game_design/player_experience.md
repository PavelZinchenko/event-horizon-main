# Player Experience

[Game design index](index.md)

## Purpose

The interface connects a complex construction game to understandable player decisions.
The player needs to see what an action does, what it costs, and why it is unavailable.

## Player actions

1. Start or continue a campaign from the main menu.
2. Use the star map as the main hub for travel, encounters, and ship services.
3. Inspect equipment and statistics before changing a build.
4. Configure controls and use the test range where needed.
5. Review rewards, objectives, and progression after an activity.

## Current rules

### Session flow

Startup loads the selected database and mod, initializes services, and loads or prepares session data.
The main menu connects to campaign entry, quick combat, settings, control testing, and the encyclopedia path.
Platform services can add sign-in, purchase, cloud-save, or reward interactions.

A new game sets fuel from the base fuel capacity in skill settings.
It grants the configured starting inventory and creates the starting ship builds from galaxy settings.
Those ships enter the active group, and the first owned frigate becomes the exploration ship.
Starting main-hull components are explicitly unlocked during this initialization.

The star map is the campaign hub.
Combat, exploration, skill selection, ship editing, quests, and services return to that hub.
Dialogue and service windows can temporarily suspend map actions.

### Information and feedback

| Context | Existing information or interaction |
| --- | --- |
| Galaxy map | Stars, region borders, special-object icons, bookmarks, filters, and quest markers |
| Travel | Fuel warnings and confirmation for selected travel cases |
| Hangar | Owned ships, active assignments, and exploration selection |
| Ship editor | Layout restrictions, component descriptions, locks, statistics, and control settings |
| Combat | Player and enemy statistics, radar, remaining ships, timer, and available encounter actions |
| Exploration | Planet information, completed-objective progress, entry cost, scanning, and immediate loot feedback |
| Progression and services | Technology dependencies, crafting requirements, prices, and purchase availability |

These interfaces exist in code and assets. Their presence alone does not prove that each rule is explained adequately.
The design requirements below define the intended standard.

### Controls

Combat supports touch controls, keyboard, mouse, and gamepad input paths.
Weapons and devices map to six action groups configured per fitting.
Mouse input can be enabled separately.

Current keyboard rebinding covers left, right, forward, and the six action groups.
It changes the first keyboard binding for each supported action.
Backward movement and alternate arrow-key bindings are not part of that rebinding interface.
Do not describe this as unrestricted rebinding across every input device.

### Language and presentation

The localization system provides language catalogs, text lookup, formatting, and font fallback.
Mods can supply additional or replacement strings.
The exact language catalog belongs to the [localization reference](../localization.md#language-lists), rather than a permanent feature-count promise.

Readable text, clear icons, sound feedback, and visible damage effects support decisions during play.
Database playlists can vary music by activity or combat rules.
Asset inventories and presentation implementation remain in [Media and resources](../resources/index.md).

### Saving and recovery

The game saves when the star-map scene loads, on application pause or focus loss, after purchases, and when the data manager is disabled.
Persistent state includes fleet configuration, inventory, skills, research, map changes, quest progress, and relationships.
Local and cloud capabilities vary by platform and build.

The session is not a frame-by-frame combat checkpoint.
Loading a saved quest battle step can start the encounter again.
Players should not infer mid-battle continuation from the general promise of persistent campaign progress.

## System connections

[Vision](vision.md) defines the intended player experience.
[Galaxy](galaxy.md), [Ship building](ship_building.md), and [Combat](combat.md) define the rules that the interface must explain.
[Platforms and mods](platforms_and_mods.md) defines account, storage, and input-environment differences.

## Design intent and open questions

| Requirement | Meaning for the player |
| --- | --- |
| Readable | Threats, costs, restrictions, and state changes remain legible on supported screen sizes |
| Responsive | Steering, firing, menus, and fitting actions react consistently |
| Explainable | The player can identify why a purchase, fitting, travel action, or encounter failed |
| Discoverable | The opening and contextual guidance teach the core loop without requiring external documentation |
| Flexible | Supported input methods provide practical access to important actions |
| Persistent | Campaign progress survives supported save and load paths |
| Performant | Busy battles remain responsive and visually understandable |

The introductory region exposes several core activities, but content placement alone is not a complete tutorial specification.
The intended opening should teach travel, equipment changes, combat feedback, and the use of rewards.
This is a design requirement, not a claim that a verified tutorial sequence already covers every concept.

Open questions:

1. Which existing quest and interface sequence teaches the complete first-session loop, and where does it leave gaps?
2. The previous overview called Ehopedia suspended, but menu and state code still expose it. What player-facing availability is intended?
3. Which accessibility requirements need explicit acceptance criteria beyond current input and localization support?

## Sources

- [Application layer](../application_layer.md): session states, interface map, and save lifecycle.
- [StartingNewGameState](../../Assets/Scripts/GameStateMachine/States/StartingNewGameState.cs): starting fuel, inventory, fleet, and unlocked components.
- [MainMenuState](../../Assets/Scripts/GameStateMachine/States/MainMenuState.cs) and [MainMenu](../../Assets/Scripts/Gui/MainMenu/MainMenu.cs): entry paths.
- [GameServices: input](../game_services.md#input) and [ShipEditor: controls](../ship_editor.md#key-bindings-and-component-modes): input and rebinding limits.
- [Localization](../localization.md) and [Media and resources](../resources/index.md): language and presentation support.
- [GameSession](../game_session.md) and [Quest persistence](../quests.md#persistence): saved state and reconstruction limits.
