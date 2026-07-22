//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

namespace EditorDatabase.Enums
{
	public enum BehaviorNodeType
	{
		[TooltipText("Always returs SUCCESS")]
		Success = 0,
		[TooltipText("Always returs FAILURE")]
		Failure = 9,
		SubTree = 1,
		[TooltipText("Executes nodes sequentially, stops when the first node returns SUCCESS or RUNNING")]
		Selector = 2,
		[TooltipText("Executes nodes sequentially, stops when the first node returns FAILURE or RUNNING")]
		Sequence = 3,
		[TooltipText("Executes nodes in parallel. Returns SUCCESS if at least one succeeds, RUNNING if any still running, and FAILURE if all fail")]
		Parallel = 4,
		[TooltipText("Executes random node, selects another after 'Cooldown' sec")]
		RandomSelector = 5,
		[TooltipText("Executes node, swaps SUCCESS and FAILURE")]
		Invertor = 6,
		[TooltipText("Executes node until condition met, then waits a specific amount of time, returning 'Result', repeats")]
		Cooldown = 7,
		[TooltipText("Executes node until condition met, returns 'Result' after that")]
		Execute = 8,
		[TooltipText("Executes nodes sequentially, stops when the first node returns FAILURE")]
		ParallelSequence = 10,
		[TooltipText("Memorizes the primary target, runs the child node, then restores the original target. Returns the result of the node's execution")]
		PreserveTarget = 11,
		[TooltipText("If the first node returns SUCCESS, executes the second node, executes the thirid node otherwise")]
		IfThenElse = 12,
		HasEnoughEnergy = 50,
		IsLowOnHp = 51,
		[TooltipText("Returns FAILURE if the time since the last player action is under the Cooldown, SUCCESS otherwise")]
		IsNotControledByPlayer = 52,
		[TooltipText("Requires threat list. Returns SUCCESS if time to collision less than value")]
		HasIncomingThreat = 53,
		[TooltipText("Returns SUCCESS if has any targets found by LookForAdditionalTargets node")]
		HasAdditionalTargets = 54,
		[TooltipText("Returns SUCCESS if [engine power] > Multiplier * [target engine power]")]
		IsFasterThanTarget = 55,
		[TooltipText("Returns SUCCESS if the main is selected and alive")]
		HasMainTarget = 56,
		MainTargetIsAlly = 57,
		MainTargetIsEnemy = 58,
		MainTargetLowHp = 59,
		MainTargetWithinAttackRange = 60,
		[TooltipText("Returns SUCCESS if mothership exists and is alive")]
		HasMothership = 61,
		[TooltipText("Returns SUCCESS if the distance to the target does not exceed the MaxDistance")]
		TargetDistance = 62,
		[TooltipText("Returns SUCCESS if the attack range of any weapon surpasses [enemy attack range] * Multiplier")]
		HasLongerAttackRange = 63,
		[TooltipText("Looks for nearest enemy. Changes target every MaxCooldown sec if it's defined. SUCCESS if found, FAILURE otherwise")]
		FindEnemy = 100,
		[TooltipText("Moves towards target until inside attack radius. Requires main target. Uses attack range of selected weapons. SUCCESS if inside attack range, FAILURE if any error, RUNNING otherwise")]
		MoveToAttackRange = 101,
		[TooltipText("Attacks main target with selected weapons, returning SUCCESS upon firing, RUNNING while aiming, and FAILURE for any inability to initiate the attack")]
		AttackMainTarget = 102,
		[TooltipText("Returns SUCCESS if any weapon found, FAILURE otherwise. If no weapon selected all weapons will be used")]
		SelectWeapon = 103,
		[TooltipText("Spawns drones and clones. Returns RUNNING until there are no more drones left to spawn, then returns SUCCESS if any drones active, FAILURE otherwise")]
		SpawnDrones = 104,
		[TooltipText("Tries to ram the target, can use Afterburner and Fortification. Returns FAILURE if there is no target or it's moving too fast, RUNNING otherwise")]
		Ram = 105,
		DetonateShip = 106,
		[TooltipText("Makes ship disappear completely, without a trace")]
		Vanish = 107,
		[TooltipText("Keeps ship at required distance. Requires main target. Uses attack range of selected weapons. SUCCESS if inside valid range, FAILURE if any error, RUNNING otherwise")]
		MaintainAttackRange = 108,
		Wait = 109,
		LookAtTarget = 110,
		[TooltipText("Updates the list of enemies other than the main target. Returns SUCCESS if any found, FAILURE otherwise")]
		LookForAdditionalTargets = 111,
		[TooltipText("Updates threat list. Returns SUCCESS if any found, FAILURE otherwise")]
		LookForThreats = 112,
		[TooltipText("Aligns ship's movement speed and direction to the target's. Returns SUCCESS on reaching, FAILURE on errors, and RUNNING otherwise")]
		MatchVelocityWithTarget = 113,
		[TooltipText("Activates device. Returns SUCCESS if activated, FAILURE otherwise")]
		ActivateDevice = 114,
		[TooltipText("If energy level drops below FailIfLess, enters recharging state and returns FAILURE until energy level reaches RestoreUntil, otherwise returns SUCCESS")]
		RechargeEnergy = 115,
		[TooltipText("If any directional weapon active, maintain focus on target and returns RUNNING. Returns FAILURE otherwise")]
		SustainAim = 116,
		[TooltipText("Charges all weapons that require charging, reserves energy needed to fully charge. Returns FAILURE if not possible, SUCCESS if fully charged, RUNNING otherwise")]
		ChargeWeapons = 117,
		[TooltipText("Follows the target. Returns FAILURE if there is no target, RUNNING otherwise")]
		Chase = 118,
		[TooltipText("Changes the ship's trajectory to avoid collision. Requires threat list. Returns SUCCESS if there are no threats, RUNNING otherwise")]
		AvoidThreats = 119,
		[TooltipText("Reduces ship's speed. Returns SUCCESS if it completely stopped, RUNNING otherwise")]
		SlowDown = 120,
		[TooltipText("Activates weapons with high recoil to get impulse. Returns FAILURE if doesn't have such weapons, RUNNING otherwise")]
		UseRecoil = 121,
		[TooltipText("Turns ship towards threats and activates shield. Returns FAILURE if there are no shields or threats, RUNNING otherwise")]
		DefendWithFronalShield = 122,
		[TooltipText("Tracks controllable bullets. Detonates them if near the target. Returns FAILURE if there are no bullets left. RUNNING otherwise")]
		TrackControllableAmmo = 123,
		[TooltipText("Keeps ship within a specified distance from main target. Returns SUCCESS if inside valid range, FAILURE if any error, RUNNING otherwise")]
		KeepDistance = 124,
		[TooltipText("Removes main target. Returns SUCCESS")]
		ForgetMainTarget = 125,
		[TooltipText("Flies away from target until outside its attack radius. Returns SUCCESS when safe, RUNNING otherwise")]
		EscapeTargetAttackRadius = 126,
		[TooltipText("Attacks additional targets with selected weapons, returning SUCCESS upon firing, RUNNING while aiming, and FAILURE for any inability to initiate the attack")]
		AttackAdditionalTargets = 127,
		[TooltipText("Targets an ally starbase. Returns SUCCESS if it exists and alive, otherwise FAILURE")]
		TargetAllyStarbase = 128,
		[TooltipText("Targets an enemy starbase. Returns SUCCESS if it exists and alive, otherwise FAILURE")]
		TargetEnemyStarbase = 129,
		[TooltipText("Tries to bypass obstacle if any. Returns FAILURE if there are no obstacles, RUNNING otherwise")]
		BypassObstacles = 130,
		[TooltipText("Attacks targets that tracked by auto-aiming turrets, returning SUCCESS upon firing, FAILURE otherwise")]
		AttackTurretTargets = 131,
		[TooltipText("Keeps harpoon active upon hitting the enemy. Returns SUCCESS if it remains attached to the enemy, returns FAILURE otherwise")]
		HoldHarpoon = 132,
		[TooltipText("Looks for nearest damaged ally. Changes target every MaxCooldown sec if it's defined. SUCCESS if found, FAILURE otherwise")]
		FindDamagedAlly = 133,
		[TooltipText("Returns SUCCESS when [forward acceleration]/[max acceleration] > MinValue")]
		EnginePropulsionForce = 150,
		MotherShipRetreated = 200,
		MotherShipDestroyed = 201,
		FlyAroundMothership = 202,
		[TooltipText("Detaches drone from the mothership. Starts treating all ships as enemies. Returns SUCCESS once activated, FAILURE after that")]
		GoBerserk = 203,
		[TooltipText("Sets the mothership as a main target. Returns FAILURE if it's absent, otherwise SUCCESS")]
		TargetMothership = 204,
		MothershipLowHp = 205,
		[TooltipText("Returns SUCCESS if ship is too far from the mothership")]
		MothershipDistanceExceeded = 206,
		[TooltipText("Makes primary target a new mothership. Returns FAILURE if target is absent or already a mothership, otherwise SUCCESS")]
		MakeTargetMothership = 207,
		MothershipLowEnergy = 208,
		MothershipLowShield = 209,
		[TooltipText("Spawns text over the ship sprite that disappears shortly. Retruns SUCCESS if spawned, RUNNING if on cooldown")]
		ShowMessage = 300,
		[TooltipText("Writes message to the log file (https://docs.unity3d.com/Manual/LogFiles.html). Retruns SUCCESS")]
		DebugLog = 301,
		[TooltipText("Sets a boolean variable for this ship. Returns SUCCESS if the variable's value changes, and FAILURE if the value remains unchanged")]
		SetValue = 302,
		[TooltipText("Verifies the state of the variable. Returns SUCCESS if the variable is set, and FAILURE if not set or undefined")]
		GetValue = 303,
		[TooltipText("Sends a message to all allies, returning SUCCESS if any ally receives it")]
		SendMessage = 304,
		[TooltipText("Monitors the radio, waiting for a specific message. Remembers message sender. If received returns SUCCESS, otherwise FAILURE")]
		MessageReceived = 305,
		[TooltipText("Targets the sender of the last received message. Returns SUCCESS if it's still alive, otherwise FAILURE")]
		TargetMessageSender = 306,
		[TooltipText("Saves the main target for this ship. It returns SUCCESS if the target is successfully updated, and FAILURE if the new target is identical to the previously saved target")]
		SaveTarget = 307,
		[TooltipText("Loads saved target. Returns SUCCESS if it exists and alive, otherwise FAILURE")]
		LoadTarget = 308,
		[TooltipText("Returns SUCCESS if specific target exists and still alive")]
		HasSavedTarget = 309,
		[TooltipText("Removes saved target. Returns SUCCESS")]
		ForgetSavedTarget = 310,
	}
}
