using Combat.Unit;
using Combat.Scene;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class FindEnemyForDrone : INode
	{
		private readonly float _findEnemyCooldown;
		private readonly float _changeEnemyCooldown;
		private readonly float _droneRange;
		private readonly bool _ignoreDrones;
		private readonly bool _inAttackRange;

		public FindEnemyForDrone(float findEnemyCooldown, float changeEnemyCooldown, float droneRange, bool inAttackRange, bool ignoreDrones)
		{
			_droneRange = droneRange;
			_inAttackRange = inAttackRange;
			_ignoreDrones = ignoreDrones;
			_findEnemyCooldown = findEnemyCooldown > 0 ? findEnemyCooldown : float.MaxValue;
			_changeEnemyCooldown = changeEnemyCooldown > 0 ? changeEnemyCooldown : float.MaxValue;
		}

		public NodeState Evaluate(Context context)
		{
			if (!context.HaveWeapons)
				return NodeState.Failure;

			float range = _inAttackRange ? context.AttackRangeMax : context.AttackRangeMax + _droneRange;
			var ship = context.Ship;
			var enemy = context.TargetShip;
			var elapsedTime = context.TimeSinceTargetUpdate;

			var haveEnemy = IsValidEnemy(ship, enemy, context.AttackRangeMax);
			var cooldown = haveEnemy ? _changeEnemyCooldown : _findEnemyCooldown;

			if (elapsedTime < cooldown)
				return haveEnemy ? NodeState.Success : NodeState.Failure;

			var options = EnemyMatchingOptions.EnemyForDrone(range);
			options.IgnoreDrones = _ignoreDrones;
			var newEnemy = context.Scene.Ships.GetEnemy(ship, options);
			context.UpdateTarget(newEnemy);

			if (newEnemy != null)
				GameDiagnostics.Debug.Log($"FindDroneEnemy: {newEnemy.Specification.Stats.ShipModel.Name}");

			return IsValidEnemy(ship, newEnemy, range) ? NodeState.Success : NodeState.Failure;
		}

		private bool IsValidEnemy(IShip ship, IShip enemy, float attackRange)
		{
			if (!enemy.IsActive()) return false;
			if (_ignoreDrones && enemy.Type.Class != UnitClass.Ship) return false;
			if (enemy.Type.Side.IsAlly(ship.Type.Side)) return false;

			if (_inAttackRange)
				return Helpers.Distance(ship, enemy) <= attackRange;

			var mothership = ship.Type.Owner;
			return mothership == null || Helpers.Distance(mothership, enemy) <= attackRange + _droneRange;
		}
	}
}
