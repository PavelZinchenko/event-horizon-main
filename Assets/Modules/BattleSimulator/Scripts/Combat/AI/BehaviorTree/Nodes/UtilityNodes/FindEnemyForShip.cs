using Combat.Unit;
using Combat.Scene;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class FindEnemyForShip : FindEnemyNodeBase
	{
		private readonly bool _ignoreDrones;
		private readonly bool _inAttackRange;

		public FindEnemyForShip(float findEnemyCooldown, float changeEnemyCooldown, bool inAttackRange, bool ignoreDrones)
			: base(findEnemyCooldown, changeEnemyCooldown)
		{
			_ignoreDrones = ignoreDrones;
			_inAttackRange = inAttackRange;
		}

		protected override IShip FindNewEnemy(Context context)
		{
			var options = EnemyMatchingOptions.EnemyForShip(_inAttackRange ? context.AttackRangeMax : 0);
			options.IgnoreDrones = _ignoreDrones;
			return context.Scene.Ships.GetEnemy(context.Ship, options);
		}

		protected override bool IsValidEnemy(IShip enemy, Context context)
		{
			var ship = context.Ship;
			if (!enemy.IsActive()) return false;
			if (enemy.Type.Side.IsAlly(ship.Type.Side)) return false;
			if (_ignoreDrones && enemy.Type.Class != UnitClass.Ship) return false;
			return !_inAttackRange || Helpers.Distance(ship, enemy) <= context.AttackRangeMax;
		}
	}
}
