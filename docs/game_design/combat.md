# Combat

[Game design index](index.md)

## Purpose

Combat tests both ship preparation and real-time piloting.
The player manages movement, facing, weapon range, energy, and defensive systems against an enemy fleet.

## Player actions

1. Choose an available ship when the encounter permits selection.
2. Steer, aim, fire weapons, and activate devices.
3. Manage range and energy while responding to enemy behavior.
4. Call another enemy early when reinforcements are available.
5. Withdraw the current ship when allowed, or surrender the encounter.
6. Review the result and adjust the fleet.

## Current rules

### Encounter setup and deployment

Each battle receives a player fleet, an enemy fleet, and a combat-rules record.
The rules control deployment, timers, rewards, skill bonuses, map size, and environmental objects.
Quests can choose custom rules. Otherwise the battle type selects its configured record.

Ordinary combat deploys one player ship at a time.
Enemies can appear together at the start or arrive as reinforcements.
When the active enemy force is gone, the manager introduces the next available enemy after a short delay.
The player can face several enemies if reinforcements arrive before earlier enemies die.

Drones, clones, satellites, and special devices add support according to the equipped ship.
Enemy AI controls steering, targeting, and system use.
The game supports behavior-driven enemy actions and player input modes through the battle simulator.

### Damage and resources

Ships have armor, optional shields, and energy.
Armor reaching zero normally destroys the combat ship, although specific devices can intervene.
Shields absorb eligible incoming damage before remaining damage reaches armor.
Some impacts bypass shields or apply additional effects.

Kinetic, energy, and heat damage use their corresponding armor resistances.
Corrosive damage uses a separate calculation based on the weakest of those resistances.
Shield damage, energy drain, repair, and impulse are distinct impact effects.
Movement and collisions therefore interact with weapons and defenses rather than functioning as purely visual effects.

Weapons and devices require their own activation conditions, energy, and cooldowns.
Energy recharge can be delayed by use.
Automatic systems can preserve a configured energy reserve.
The [fitting chapter](ship_building.md#activation-settings) owns control-group and reserve configuration.

Special equipment provides effects such as repair, acceleration, teleportation, concealment, temporary protection, missile interception, and drones.
Availability and exact strengths depend on the equipped content.
The device catalog is documented in [BattleSimulator](../battle_simulator.md#devices).

### Timers and pressure

The default rule uses a round time of `max(40, 100 - star level)` seconds.
A zero time limit disables timed reinforcements.
Round timing changes when enemies spawn or die, with exceptions for specific modes.

| Timeout mode | Outcome |
| --- | --- |
| Call next enemy | Introduces another available enemy if the active-enemy limit permits it |
| Call next enemy or draw | Introduces reinforcements, then permits a final timed phase that can end without victory |
| Drain player health | Applies escalating armor and shield loss after the limit |

The next-enemy control follows reinforcement availability.
The schema's `NextEnemyButton` field currently does not determine whether that control appears.
Do not use that unused field as evidence of an encounter's player options.

### Ship selection and withdrawal

| Selection mode | Player behavior |
| --- | --- |
| Default | Choose replacements and withdraw the current ship when another is available |
| No retreats | Choose replacements after destruction, but cannot voluntarily change ships during the fight |
| By order | Deploy the next available ship automatically |
| Only one ship | End the attempt when the first player ship is destroyed |

Default selection becomes No retreats when the player lacks the rescue-unit skill.
Rules that disable skill bonuses bypass that conversion, but explicit By order rules still prevent manual switching.
This makes withdrawal an encounter-dependent capability, not a universal action.

A permitted withdrawal takes seven seconds.
Returning a withdrawn ship during the same battle preserves its armor and shield damage.
Withdrawal differs from surrender and from campaign travel retreat.

### Victory, defeat, and rewards

Victory requires no surviving enemy ships and at least one surviving player ship.
Surrender marks the battle's player fleet destroyed and exits.
A timeout with surviving enemies is not a victory.

Loot and experience have separate conditions: victory only, always, or never.
Default rules require victory for both.
Survival rules allow both even without victory; arena and challenge rules disable experience.
Special activity rewards and random destroyed-ship loot are also separate.

Ordinary combat destruction does not delete the owned ship.
Rewards and quest callbacks apply when combat finishes, then the game returns to the star map.
Any additional loss or retreat is determined by the surrounding activity or quest.

## System connections

[Ship building](ship_building.md) supplies combat capabilities.
[Fleet](fleet.md) supplies the reserve roster.
[Activities](activities.md) select encounter variants.
[Economy](economy.md) owns reward delivery, while [Progression](progression.md) owns experience growth.

## Design intent and open questions

Combat should reward preparation and execution together.
Enemy behavior, terrain, reinforcement pressure, and objectives should create different tactical demands.
Losses should expose a reason that the player can address through piloting or construction.

Open questions:

1. Should the rescue-unit requirement be introduced before an encounter makes withdrawal strategically important?
2. Does each activity communicate its timeout and reward rules before entry?
3. Should the player receive clearer feedback about damage types, energy starvation, and resistance effects after defeat?

## Sources

- [CombatRules](../database/schema/CombatRules.md): rule fields, selection conversion, and timer behavior.
- [CombatManager](../../Assets/Scripts/Combat/Manager/CombatManager.cs), [CombatTimer](../../Assets/Scripts/Combat/Manager/CombatTimer.cs), and [ShipInfo](../../Assets/Scripts/Combat/Domain/ShipInfo.cs): deployment, withdrawal, and damage persistence.
- [ICombatModel](../../Assets/Scripts/Combat/Domain/ICombatModel.cs): victory and reward conditions.
- [CombatState](../../Assets/Scripts/GameStateMachine/States/CombatState.cs): completion and reward application.
- [BattleSimulator](../battle_simulator.md): damage, resources, devices, drones, AI, and input.
- [Built-in combat rules](../../Assets/Modules/Database/Resources/Database/Quests/CombatRules/): active encounter configurations.
