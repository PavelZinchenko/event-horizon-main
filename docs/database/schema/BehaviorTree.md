# BehaviorTree

A behavior tree record is a combat AI program: a tree of nodes that the AI evaluates every AI step to choose a target, move, fire, and use devices. [CombatSettings](CombatSettings.md) sets the default tree for each role (enemy, starbase, clone, drone, autopilot). A [ShipBuild](ShipBuild.md) can override it with `CustomAI`, and a [DroneBay](DroneBay.md) with its drone trees. Trees can include other trees through `SubTree` nodes.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `28` |
| Kind | collection |
| Folder | [`Resources/Database/Ai/`](../../../Assets/Modules/Database/Resources/Database/Ai/) (25 role trees), `Ai/Behaviors/` (45 subtrees), `Ai/Conditions/` (4 subtrees) |
| Records in the built-in catalog | 74. `CombatSettings` uses 6, ship builds 290 and 343 use 9 and 20, `SubTree` nodes make 125 references. Trees 6, 8, 15, 16, 18, 19, 21, 23, 25, and 106 are not referenced. No drone bay sets a tree. |
| Schema | [BehaviorTree.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Ai/BehaviorTree.xml), [BehaviorTreeNode.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Ai/BehaviorTreeNode.xml), [BehaviorNodeRequirement.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Ai/BehaviorNodeRequirement.xml), node list [BehaviorNodeType.xml](../../../Assets/Modules/Database/.Schema/v1/Enums/Ai/BehaviorNodeType.xml) |
| Generated class | `GameDatabase.DataModel.BehaviorTreeModel` (alias of `BehaviorTree`), with switch classes `BehaviorTreeNode_*` and `BehaviorNodeRequirement_*` ([BehaviorTree.cs](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/BehaviorTree.cs), [BehaviorTreeNode.cs](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/BehaviorTreeNode.cs), [BehaviorNodeRequirement.cs](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/BehaviorNodeRequirement.cs)) |
| Runtime | [`BehaviorTreeBuilder`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/BehaviorTreeBuilder.cs), [`NodeBuilder`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/NodeBuilder.cs), [`RequirementChecker`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs), nodes in [`Nodes/`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/Nodes/). Build and run steps: [BattleSimulator: Behaviour tree](../../battle_simulator.md#behaviour-tree). |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `RootNode` | struct [BehaviorTreeNode](#behaviortreenode-switch) | `Success` | The node that the AI evaluates every step. |

## Structures

### BehaviorTreeNode (switch)

`Type` selects the node. The JSON field names are generic; the *Model name* column gives the name in the generated class. A node returns `Success`, `Failure`, or `Running`.

| Field | Type | Case | Model name | Default / range | Meaning |
| --- | --- | --- | --- | --- | --- |
| `Type` | enum `BehaviorNodeType` | | | `Success` | The node. See [Nodes](#nodes). |
| `Requirement` | struct [BehaviorNodeRequirement](#behaviornoderequirement-switch) | | | `Empty` | Checked once when the tree is built. If it fails, the node becomes a constant `Failure` ([NodeBuilder.cs:41](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/NodeBuilder.cs#L41)). |
| `Nodes` | struct list `BehaviorTreeNode` | composites | | | Child nodes. |
| `Node` | struct `BehaviorTreeNode` | `Invertor`, `Cooldown`, `Execute`, `PreserveTarget` | | | The one child node. |
| `ItemId` | ref [BehaviorTree](BehaviorTree.md) | `SubTree` | `BehaviourTree` | | The included tree. `0` or a missing tree makes a `Failure` node. |
| `WeaponType` | enum `AiWeaponCategory` | `SelectWeapon` | | `All` | Weapon filter ([enums: AI](enums.md#ai)). |
| `ExecutionMode` | enum `NodeExecutionMode` | `Execute`, `Cooldown` | | `UntilSucceeds` | When the child counts as done. See [Execute and Cooldown](#execute-and-cooldown). |
| `Result` | bool | `Execute`, `Cooldown` | `Result` | | The constant result after the child is done (`true` = `Success`). |
| `Result` | bool | `SetValue` | `Value` | | The value to store. |
| `MinValue` | float | `HasEnoughEnergy`, `RechargeEnergy` | `FailIfLess` | 0.1, 0…1 | Energy fraction. |
| `MaxValue` | float | `RechargeEnergy` | `RestoreUntil` | 0.9, 0…1 | Energy fraction that ends recharging. |
| `MinValue` | float | `IsLowOnHp`, `MainTargetLowHp`, `MothershipLowHp`, `MothershipLowEnergy`, `MothershipLowShield`, `EnginePropulsionForce` | | 0…1 | Threshold fraction. The default 0 never succeeds for the "low" checks. |
| `Cooldown` | float | `RandomSelector`, `Wait`, `Cooldown` | | 0… | Seconds. |
| `Cooldown` | float | `LookForAdditionalTargets`, `LookForThreats`, `IsNotControledByPlayer` | `Cooldown` | 0.1… | Seconds between updates, or since the last player input. |
| `Cooldown` | float | `HasIncomingThreat` | `TimeToCollision` | 5, 0… | Seconds. |
| `MinValue` | float | `FindEnemy`, `FindDamagedAlly` | `MinCooldown` | 0.5… | Minimum seconds between target changes. |
| `MaxValue` | float | `FindEnemy`, `FindDamagedAlly` | `MaxCooldown` | 5, 0… | Maximum seconds between target changes. |
| `InRange` | bool | `FindEnemy`, `FindDamagedAlly`, `DetonateShip` | `InAttackRange` | | Only targets within attack range. For drones: search near the mother ship within a quarter of the drone range. `DetonateShip`: only when the target touches the ship at detonation time. |
| `InRange` | bool | `Wait` | `ResetIfInterrupted` | | Restart the wait when the node was not evaluated in the previous step. |
| `InRange` | bool | `AttackMainTarget`, `AttackAdditionalTargets` | `NotMoving` | | Aim by turning only; do not move the ship. |
| `NoDrones` | bool | `FindEnemy` | `IgnoreDrones` | | Skip drones. |
| `UseSystems` | bool | `Ram` | `UseShipSystems` | | Use afterburner and fortification while ramming. |
| `MinValue` | float | `MoveToAttackRange`, `MaintainAttackRange`, `MainTargetWithinAttackRange` | `MinMaxLerp` | 1, 0…1 | Attack range between the shortest (0) and the longest (1) range of the selected weapons. |
| `MaxValue` | float | `MoveToAttackRange` | `Multiplier` | 1, 0…1 | Multiplies the attack range. `0` means 1. |
| `MaxValue` | float | `MaintainAttackRange` | `Tolerance` | 0.2, 0…1 | Valid distance: `range × (1 − Tolerance)` … `range`. |
| `MaxValue` | float | `SlowDown`, `MatchVelocityWithTarget` | `Tolerance` | 0.2, 0…1 | Accepted speed deviation. |
| `MinValue`, `MaxValue` | float | `FlyAroundMothership`, `KeepDistance` | `MinDistance`, `MaxDistance` | 2.5, 3.5, 0…1000 | Distance band. `FlyAroundMothership` picks one random distance in the band when the tree is built. |
| `MaxValue` | float | `TargetDistance`, `MothershipDistanceExceeded` | `MaxDistance` | 0… | Distance. `0` uses the drone range. A non-drone has no drone range, so `MothershipDistanceExceeded` with `0` is a constant `Failure`. |
| `MinValue` | float | `IsFasterThanTarget`, `HasLongerAttackRange` | `Multiplier` | 1…10 | Margin over the target's value. |
| `DeviceClass` | enum [DeviceClass](enums.md#deviceclass) | `ActivateDevice` | | | The device class to activate. Automatic devices are not found ([Device: Activation](Device.md#activation)). |
| `Text` | string | `ShowMessage`, `DebugLog` | | | Message. `ShowMessage` localizes it ([NodeBuilder.cs:254](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/NodeBuilder.cs#L254)). |
| `Text` | string | `SetValue`, `GetValue`, `SaveTarget`, `LoadTarget`, `HasSavedTarget`, `ForgetSavedTarget`, `SendMessage`, `MessageReceived` | `Name` | | Variable, slot, or message name. See [Variables and messages](#variables-and-messages). |
| `Color` | color | `ShowMessage` | | white | Text color. |

*Composites* are `Selector`, `Sequence`, `Parallel`, `RandomSelector`, `ParallelSequence`, and `IfThenElse`.

### BehaviorNodeRequirement (switch)

`Type` selects the check. [`RequirementChecker`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs) evaluates it once, against the ship as it is created.

| Field | Type | Case | Model name | Default / range | Meaning |
| --- | --- | --- | --- | --- | --- |
| `Type` | enum `BehaviorRequirementType` | | | `Empty` | The check. See [Requirements](#requirements). |
| `DeviceClass` | enum [DeviceClass](enums.md#deviceclass) | `HasDevice` | | | |
| `DifficultyLevel` | enum `AiDifficultyLevel` | `AiLevel`, `MinAiLevel` | | `Easy` | See [AI level](#ai-level). |
| `SizeClass` | enum [SizeClass](enums.md#sizeclass) | `SizeClass` | | | |
| `Value` | float | `HasHighManeuverability`, `HasKineticResistance` | | 1, 0… | Threshold. |
| `Value` | float | `HasHighRammingDamage` | | 0… | Threshold. |
| `Value` | float | `HasLongRangeWeapon` | `Range` | 0… | Minimum attack range. |
| `Requirements` | struct list `BehaviorNodeRequirement` | `Any`, `All`, `None` | | | Child checks. |

## Notes

### Nodes

The schema enum carries a description of every node ([BehaviorNodeType.xml](../../../Assets/Modules/Database/.Schema/v1/Enums/Ai/BehaviorNodeType.xml)). The editor shows it. The table lists the value, the parameters, and rules that the descriptions do not state. Node classes: [`Nodes/`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/Nodes/).

| Group | Nodes (value) | Notes |
| --- | --- | --- |
| Constants | `Success` (0), `Failure` (9) | |
| Composites | `Selector` (2), `Sequence` (3), `Parallel` (4), `ParallelSequence` (10) | `Parallel` evaluates all children and returns `Success` if any succeeds. `ParallelSequence` evaluates all children until one fails. |
| | `RandomSelector` (5) | Every `Cooldown` seconds it shuffles the children and keeps the first one that does not fail. Between picks it runs only that child. |
| | `IfThenElse` (12) | Needs exactly 3 children; otherwise building the tree throws `IfThenElse node requires 3 child nodes` ([NodeBuilder.cs:109](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/NodeBuilder.cs#L109)). |
| Decorators | `Invertor` (6), `Cooldown` (7), `Execute` (8), `PreserveTarget` (11), `SubTree` (1) | `SubTree` inserts the root of `ItemId` in place. |
| Conditions | `HasEnoughEnergy` (50), `IsLowOnHp` (51), `IsNotControledByPlayer` (52), `HasIncomingThreat` (53), `HasAdditionalTargets` (54), `IsFasterThanTarget` (55), `HasMainTarget` (56), `MainTargetIsAlly` (57), `MainTargetIsEnemy` (58), `MainTargetLowHp` (59), `MainTargetWithinAttackRange` (60), `HasMothership` (61), `TargetDistance` (62), `HasLongerAttackRange` (63), `EnginePropulsionForce` (150), `MotherShipRetreated` (200), `MotherShipDestroyed` (201), `MothershipLowHp` (205), `MothershipDistanceExceeded` (206), `MothershipLowEnergy` (208), `MothershipLowShield` (209) | "Low" checks succeed when the armor, energy, or shield fraction is below `MinValue`. `HasEnoughEnergy` fails below `FailIfLess`. |
| Targeting | `FindEnemy` (100), `LookForAdditionalTargets` (111), `LookForThreats` (112), `ForgetMainTarget` (125), `TargetAllyStarbase` (128), `TargetEnemyStarbase` (129), `FindDamagedAlly` (133), `TargetMothership` (204), `MakeTargetMothership` (207), `TargetMessageSender` (306), `SaveTarget` (307), `LoadTarget` (308), `HasSavedTarget` (309), `ForgetSavedTarget` (310) | A drone's `FindEnemy` searches near the mother ship within the drone range ([NodeBuilder.cs:186](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/NodeBuilder.cs#L186)). |
| Movement | `MoveToAttackRange` (101), `Ram` (105), `MaintainAttackRange` (108), `LookAtTarget` (110), `MatchVelocityWithTarget` (113), `Chase` (118), `AvoidThreats` (119), `SlowDown` (120), `KeepDistance` (124), `EscapeTargetAttackRadius` (126), `BypassObstacles` (130), `FlyAroundMothership` (202) | |
| Weapons | `AttackMainTarget` (102), `SelectWeapon` (103), `SustainAim` (116), `ChargeWeapons` (117), `UseRecoil` (121), `TrackControllableAmmo` (123), `AttackAdditionalTargets` (127), `AttackTurretTargets` (131), `HoldHarpoon` (132) | A ship that cannot move (starbase, or engine or turn rate below 0.1) always attacks without moving ([NodeBuilder.cs:61](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/NodeBuilder.cs#L61)). |
| Ship and devices | `SpawnDrones` (104), `DetonateShip` (106), `Vanish` (107), `ActivateDevice` (114), `RechargeEnergy` (115), `DefendWithFronalShield` (122), `GoBerserk` (203) | `ActivateDevice`, `ChargeWeapons`, `DefendWithFronalShield`, `HoldHarpoon`, and `TrackControllableAmmo` become `Failure` when the tree is built if the ship lacks the device or weapon. `DetonateShip` fails at run time without a detonator. |
| Wait | `Wait` (109) | Fails until it has been evaluated for `Cooldown` seconds in total, then succeeds on every later step. Only `ResetIfInterrupted` restarts it. |
| Special | `ShowMessage` (300), `DebugLog` (301), `SetValue` (302), `GetValue` (303), `SendMessage` (304), `MessageReceived` (305) | `ShowMessage` returns `Running` for 0.5 s after the last message of this ship. |

### Build-time simplification

The builder evaluates requirements and drops constant children ([NodeBuilder.cs:90](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/NodeBuilder.cs#L90)):

1. A `Selector` or `Parallel` drops `Failure` children. A `Selector` stops at the first `Success` child.
2. A `Sequence` or `ParallelSequence` drops `Success` children and stops at the first `Failure` child.
3. A composite with no children left is `Failure` (`Selector`, `Parallel`) or `Success` (`Sequence`, `ParallelSequence`). A composite with one child is that child.
4. An `IfThenElse` whose condition is constant becomes its *then* or *else* branch.

Requirements therefore describe the ship at its creation. A ship that later loses a device keeps the branch for it.

### Execute and Cooldown

`ExecutionMode` decides when the child is done: `UntilSucceeds` on `Success`, `UntilFails` on `Failure`, `UntilFinishes` on either, `OneTime` after the first evaluation, `Infinitely` never ([NodeExecutionModeExtensions.cs:8](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/Utils/NodeExecutionModeExtensions.cs#L8)).

- `Execute` returns the child's result until the child is done, then `Result` forever. With `Infinitely`, it runs the child every step and always returns `Result`.
- `Cooldown` returns the child's result. When the child is done, the node returns `Result` without running the child for `Cooldown` seconds, then starts again.

### Requirements

| `Type` | Passes when | Code |
| --- | --- | --- |
| `Empty` | always | |
| `Any`, `All`, `None` | any, all, or none of `Requirements` pass. An empty `Any` fails; an empty `All` or `None` passes. | [line 27](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs#L27) |
| `AiLevel`, `MinAiLevel` | the AI level equals, or is at least, `DifficultyLevel` | [line 92](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs#L92) |
| `SizeClass` | the hull has this size class | [line 99](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs#L99) |
| `HasDevice` | a device of `DeviceClass` is installed, automatic devices included | [line 51](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs#L51) |
| `HasDrones` | the ship has a drone bay or a cloning device | [line 59](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs#L59) |
| `HasAnyWeapon`, `CanRepairAllies`, `CanRechargeAllies`, `HasHarpoon`, `HasChargeableWeapon`, `HasRemotelyControlledWeapon` | a weapon of this kind exists. Repair and recharge come from the ammunition effects; harpoon from `AiBulletBehavior`; chargeable and remote from the weapon class. | [line 91](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs#L91) |
| `HasHighRecoilWeapon` | a weapon's recoil per ship weight is above 0.1 (hard-coded) | [line 67](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs#L67) |
| `HasLongRangeWeapon` | the longest attack range is at least `Range` | [line 77](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs#L77) |
| `HasEngine` | not a starbase, and engine power and turn rate above 0.1 | [line 83](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs#L83) |
| `IsDrone` | the ship is a drone | [line 94](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs#L94) |
| `HasKineticResistance` | kinetic resistance ≥ `Value`. The default 1 needs full immunity. | [line 100](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs#L100) |
| `HasHighRammingDamage` | ramming damage multiplier ≥ `Value`. The default 0 always passes. | [line 101](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs#L101) |
| `HasHighManeuverability` | forward acceleration > `Value` | [line 102](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs#L102) |

### AI level

The AI level is a number that becomes `Easy` below 50, `Medium` below 80, and `Hard` from 80 ([BehaviorTreeBuilder.cs:51](../../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/BehaviorTreeBuilder.cs#L51)).

| Ship | AI level |
| --- | --- |
| Enemy fleets and exploration enemies | 2 × star level ([Distance.cs:15](../../../Assets/Modules/ShipConstructor/Scripts/Maths/Distance.cs#L15)): `Easy` below level 25, `Medium` below 40, `Hard` from 40 |
| Arena opponents | 100 (`Hard`) |
| Starbases, clones, drones, autopilot | `Hard` |

### Variables and messages

- `SetValue` and `GetValue` use boolean variables of the ship. `SaveTarget`, `LoadTarget`, `HasSavedTarget`, and `ForgetSavedTarget` use target slots of the ship. A ship has 64 variables.
- `SendMessage` sends to all allied ships in the battle that have a `MessageReceived` node for the name. `MessageReceived` then succeeds and remembers the sender for `TargetMessageSender`.
- The name-to-number maps are broken: every name maps to number 0, so all variables share one value, all target slots share one target, and all messages are one message ([DB15.20](../../issues/database.md#db15-smaller-defects)). Built-in trees use one variable name (`wait_for_help`) and one message name (`need_repair`), so they are not affected.

### Data rules

1. Do not include a tree in itself through `SubTree`, directly or indirectly. Building such a tree overflows the stack ([DB12](../../issues/database.md#db12-the-loader-does-not-detect-reference-cycles)).
2. Give `IfThenElse` exactly three children.
3. Use only one variable name and one message name per battle until DB15.20 is fixed.
4. Set `MinValue` on "low" checks; the default 0 never succeeds.
5. Requirements are checked once at ship creation. Use condition nodes for anything that changes during battle.
6. A behavior tree change applies to every ship that uses the tree or includes it.
