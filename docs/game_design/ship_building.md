# Ship Building

[Game design index](index.md)

## Purpose

Ship building converts collected equipment into a combat or exploration role.
Spatial limits and equipment interactions make construction a constrained design problem.

## Player actions

1. Select a ship and inspect its layout, mounts, and built-in features.
2. Place compatible components in free cells.
3. Compare mobility, defenses, energy supply, and weapon performance.
4. Fit optional satellites.
5. Assign weapon and device controls, then test the result.
6. Save or reuse a fitting through the available preset tools.

## Current rules

### Hulls and placement

A hull defines its class, cell layout, weapon mounts, base features, and visual model.
Components occupy shaped areas of the layout.
Every occupied component cell must fit inside the hull and avoid overlap.

| Constraint | Effect on construction |
| --- | --- |
| Cell type | Interior, exterior, engine, and weapon restrictions determine usable locations |
| Mixed interior/exterior cells | Accept the corresponding interior or exterior components |
| Weapon mount class | Restricts typed weapons to compatible mounts |
| Ship class | A component can allow only selected hull sizes |
| Biological or mechanical hull | Component restrictions can exclude either hull type |
| Component and group limits | Cap repeated equipment or equipment sharing a limit group |
| Built-in devices | Can prevent installing a duplicate device class |

Empty space and unpurchased expansion slots accept no equipment.
The editor checks mount restrictions as part of placement.
A component's overall cell type drives compatibility; individual letters in its shape do not supply separate placement rules.

### Equipment roles

| Role | Main trade-offs |
| --- | --- |
| Weapons | Damage, range, aiming behavior, energy cost, cooldown, and mount compatibility |
| Armor and shields | Survivability against incoming damage versus space, weight, and energy requirements |
| Reactors and energy storage | Sustained operation, activation capacity, and recharge behavior |
| Engines | Thrust, speed, turning, and mass requirements |
| Devices | Special movement, repair, defense, concealment, or other effects with activation constraints |
| Drone equipment | Launch capacity, drone capabilities, replacement rate, and energy consumption |

The final ship combines hull features, installed equipment, component modifications, perks, and external bonuses.
Equipment can improve several statistics while worsening others.
Component quality and perks are described in [Progression](progression.md).

Bionic hulls use regeneration and a different compatibility set.
They cannot equip repair bots under the standard compatibility rule.
Do not treat a biological hull as a mechanical hull with a cosmetic change.

### Satellites

A ship has two optional satellite positions.
Each satellite has its own layout and installed components.
Satellite eligibility depends on class or model scale, with a perk able to increase the permitted size.
Starbases do not accept satellites through the standard suitability rule.

Satellites are attached platforms, while drones are launched units.
Their construction, control, and replacement rules are different.

### Locked equipment

Components supplied by ship and satellite builds can be locked.
New-game initialization explicitly unlocks the starting ships' main-hull components.
Locked components cannot be dragged out or removed by the editor's normal removal actions.
The interface offers unlocking where the component's availability and technology rules permit it.

Unlocking charges a credit price through the campaign editor context.
The normal price is twice the component's credit price.
Technology requirements can prevent payment from being offered at all.
Unlocking changes whether equipment can be removed; it does not improve its combat statistics.

### Activation settings

Manual systems use one of six action groups.
Some systems also support automatic activation with a minimum energy reserve.
Automatic reserve levels progress in ten-percentage-point steps, starting at zero reserve.
The system still needs sufficient energy, a completed cooldown, and any device-specific conditions.

Drone bays have separate behavior settings.
These settings affect operation in battle, so a fitting includes control choices as well as physical placement.

## System connections

[Combat](combat.md) applies the resulting statistics and activation settings.
[Progression](progression.md) supplies quality, perks, levels, and hull expansion.
[Economy](economy.md) supplies equipment and pays for unlocking or crafting.
[Fleet](fleet.md) determines which prepared ships can enter an encounter.

## Design intent and open questions

Construction should reward coherent roles and visible trade-offs.
The player should understand why a component cannot fit or activate.
Fitting feedback should explain energy shortages, restrictions, and meaningful performance differences.

The editor's statistics omit some combat bonuses, including bonuses applied when entering battle.
The [existing statistics issue](../issues/ship_editor.md#se11-statistics-duplicate-shipbuilder-and-omit-combat-bonuses) records that limitation.
Displayed fitting values should therefore not be treated as an exact prediction of every encounter.

Open questions:

1. Which statistics should show both the base fitting value and the expected campaign combat value?
2. Does the interface distinguish locked equipment, incompatible equipment, and equipment blocked by a group limit?

## Sources

- [ShipConstructor](../ship_constructor.md): component compatibility, statistics, perks, and satellites.
- [ShipEditor](../ship_editor.md): placement, locks, presets, and activation settings.
- [CompatibilityChecker](../../Assets/Modules/ShipConstructor/Scripts/Component/CompatibilityChecker.cs): biological, class, and device restrictions.
- [ShipLayoutModel](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipLayoutModel.cs): cell and mount checks.
- [ShipEditorState](../../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs): campaign inventory and unlock context.
- [BattleSimulator: ship systems](../battle_simulator.md#ship-systems): activation and energy reserve behavior.
