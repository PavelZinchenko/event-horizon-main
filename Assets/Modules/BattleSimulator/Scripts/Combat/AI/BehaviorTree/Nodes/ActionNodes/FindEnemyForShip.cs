using Combat.Unit;
using Combat.Scene;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class FindEnemyForShip : INode
	{
		private readonly float _findEnemyCooldown;
		private readonly float _changeEnemyCooldown;
		private readonly bool _inAttackRange;
		private readonly bool _ignoreDrones;

		public FindEnemyForShip(float findEnemyCooldown, float changeEnemyCooldown, bool inAttackRange, bool ignoreDrones)
		{
			_ignoreDrones = ignoreDrones;
			_inAttackRange = inAttackRange;
			_findEnemyCooldown = findEnemyCooldown > 0 ? findEnemyCooldown : float.MaxValue;
			_changeEnemyCooldown = changeEnemyCooldown > 0 ? changeEnemyCooldown : float.MaxValue;
		}

		public NodeState Evaluate(Context context)
		{
			var ship = context.Ship;
			var enemy = context.TargetShip;
			var elapsedTime = context.TimeSinceTargetUpdate;
			var range = _inAttackRange ? context.AttackRangeMax : 0;

			var haveEnemy = IsValidEnemy(ship, enemy, range);
			var cooldown = haveEnemy ? _changeEnemyCooldown : _findEnemyCooldown;

			if (elapsedTime < cooldown)
				return haveEnemy ? NodeState.Success : NodeState.Failure;

			var options = EnemyMatchingOptions.EnemyForShip(range);
			options.IgnoreDrones = _ignoreDrones;
			var newEnemy = context.Scene.Ships.GetEnemy(context.Ship, options);
			context.UpdateTarget(newEnemy);

			if (newEnemy != null)
				GameDiagnostics.Debug.Log($"FindEnemy: {newEnemy.Specification.Stats.ShipModel.Name}");

			return IsValidEnemy(ship, newEnemy, range) ? NodeState.Success : NodeState.Failure;
		}

		private bool IsValidEnemy(IShip ship, IShip enemy, float range)
		{
			if (!enemy.IsActive()) return false;
			if (_ignoreDrones && enemy.Type.Class != UnitClass.Ship) return false;
			if (enemy.Type.Side.IsAlly(ship.Type.Side)) return false;
			return !_inAttackRange || Helpers.Distance(ship, enemy) <= range;
		}
	}
}
