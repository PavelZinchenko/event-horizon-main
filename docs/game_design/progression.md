# Progression

[Game design index](index.md)

## Purpose

Progression converts encounter results into stronger ships, broader choices, and greater campaign reach.
Several independent systems develop the player, individual ships, and the equipment catalog available for construction.

## Player actions

1. Earn experience through eligible battles.
2. Spend player skill points on selected capabilities.
3. Research faction technologies and their dependencies.
4. Acquire or craft components with useful modifications.
5. Develop ship levels, perk slots, perks, and hull expansions.

## Current rules

### Player experience and skills

Combat awards experience to participating ships from enemy damage progress.
Ship size affects the award, and the ship-experience skill supplies a multiplier.
Player experience is calculated from the resulting fleet experience gain.
It is a separate progression track with its own conversion formula.

Each player level supplies one skill point.
The skill-tree layout comes from `Resources/skills.json`.
The tree includes prerequisite and requirement nodes in addition to purchased bonuses.

| Skill area | Effects |
| --- | --- |
| Mother ship | Fuel capacity, travel speed and range, and rescue-unit access |
| Discovery | Nearby-star scanning and exploration-loot improvement |
| Combat | Attack, defense, shields, resistances, and ship-experience gains |
| Economy | Merchant prices and crafting prices or level requirements |
| Hangar | Capacity for ships across the supported size classes |
| Late progression | Requirements involving levels and faction-base capture; access to a higher ship-level cap |

Skill settings provide the numerical effects.
Some encounters disable combat skill bonuses, so campaign bonuses do not apply universally.
Resetting skills also revalidates the active hangar.

### Ship experience

Each ship stores experience independently.

| Rule | Formula or effect |
| --- | --- |
| Cumulative experience for level `n` | `100 × n³` |
| Level power multiplier | `11^(level / 100)` |
| Statistics affected by that multiplier | Armor, shield, weapon damage, and ramming bonuses |
| Player ship cap | Settings-controlled base cap, with a higher cap available through the limit-breaking skill unless disabled |

The level formula gives a multiplier of 1 at level 0 and 11 at level 100.
It does not multiply every ship statistic.
Military-base services can also sell ship-level increases in builds where those services exist.

### Research

Research points belong to individual factions.
A technology unlocks access to a component, ship, or satellite.
Normal research requires eligible dependencies and enough points from the technology's faction.
Special technologies do not pass the normal research-availability path.

Research rewards come from bosses, some fleets, and other configured rewards.
Blueprint rewards can grant a technology directly.
Some technologies are free, and hidden technologies need separate presentation or acquisition paths.

Researching a technology and manufacturing its item are different actions.
The [economy chapter](economy.md#crafting) owns workshop access, crafting prices, and quality costs.

### Component quality

Components can be plain, degraded, or improved.
The quality scale uses N3 through N1 for degraded items, P0 for plain items, and P1 through P3 for improved items.
A non-plain item applies a component modification at the selected quality.
The modification determines which statistics change, so quality is not a universal multiplier on every statistic.

Loot generation considers encounter level, component level, faction, rarity eligibility, and quality limits.
Improved crafting selects modifications from the component's allowed list.
Superior component crafting can produce P2 or P3 quality; its label does not guarantee P3.

Component upgrade levels are a different mechanism from modification quality.
The current `+N` upgrade level changes naming and identity but does not apply stat improvements.
The upgrade provider is empty in the campaign editor.
This is an unfinished system, documented in [SC6](../issues/ship_constructor.md#sc6-component-upgrades-are-wired-but-inert).

### Ship perks and perk slots

Ships support up to three perk slots.
Common crafted ships have no perk slots; improved, excellent, and superior ships receive one, two, and three empty slots respectively.
Installing a perk is a separate action.

Perks can improve resistances, reduce weight, change aiming or mount restrictions, increase satellite allowance, or change recharge behavior.
Some exchange one benefit for reduced armor, energy, shields, or damage.
Suitability and duplication rules limit which perks can be installed.

The perk-slot service can add a slot when faction, service-level, ship-level, and resource requirements pass.
The first, second, and third added slots require ship levels 50, 75, and 100.
They consume 10, 15, and 20 ship levels respectively, as well as their resource price.
This is a conversion of accumulated ship progress into customization capacity.
See [UnlockSlotPanel](../../Assets/Scripts/Gui/Craft/UnlockSlotPanel.cs).

### Hull expansion

Shipyard expansion adds eligible cells around the original hull layout.
Only cells adjacent to stock hull cells are candidates; expansions cannot chain indefinitely outward.
Hull and global settings can disable expansion.
The service also checks shipyard and ship-level requirements.
Repainting changes appearance separately from progression statistics.

## System connections

[Combat](combat.md) supplies experience and applies combat bonuses.
[Economy](economy.md) supplies research and manufacturing resources.
[Fleet](fleet.md) uses hangar growth.
[Ship building](ship_building.md) uses newly available items, perks, and hull space.

## Design intent and open questions

Progression should introduce new decisions and viable roles as well as numerical growth.
Research dependencies should guide discovery while preserving meaningful choice.
The cost of improving a ship should be understandable before resources or levels are consumed.

Open questions:

1. Should component upgrade levels become a functional progression system or be removed from player-facing data?
2. Does sacrificing ship levels for a perk slot provide a clear and worthwhile choice?
3. Should arena ships retain their campaign experience or use an explicit normalization rule? The [existing issue](../issues/ship_constructor.md#sc4-arena-ships-drop-experience-and-colour) leaves that intent unresolved.

## Sources

- [PlayerSkills](../../Assets/Scripts/Domain/Player/PlayerSkills.cs) and [SkillSettings](../database/schema/SkillSettings.md): points, bonuses, and caps.
- [CombatModel](../../Assets/Scripts/Combat/Domain/CombatModel.cs) and [CombatReward](../../Assets/Scripts/Combat/Domain/CombatReward.cs): experience awards.
- [ShipConstructor: game formulas](../ship_constructor.md#game-formulas): ship levels and power.
- [Research](../../Assets/Scripts/Domain/Research/Research.cs) and [Technology](../../Assets/Scripts/Domain/Technologies/Technology.cs): dependencies, unlocks, quality, and crafted slots.
- [ComponentInfo](../../Assets/Modules/ShipConstructor/Scripts/ComponentInfo.cs) and [ShipEditorState](../../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs): component quality and inactive upgrades.
- [UnlockSlotPanel](../../Assets/Scripts/Gui/Craft/UnlockSlotPanel.cs) and [ShipConstructor: layout](../ship_constructor.md#layout-and-hull-expansion): slot costs and expansion rules.
