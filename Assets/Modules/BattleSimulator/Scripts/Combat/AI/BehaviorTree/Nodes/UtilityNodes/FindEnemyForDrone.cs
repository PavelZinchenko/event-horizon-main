using Combat.Unit;
using Combat.Scene;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class FindEnemyForDrone : FindEnemyNodeBase
	{
		private readonly float _droneRange;
		private readonly bool _ignoreDrones;

		public FindEnemyForDrone(float findEnemyCooldown, float changeEnemyCooldown, float droneRange, bool ignoreDrones)
			: base(findEnemyCooldown, changeEnemyCooldown)
		{
			_droneRange = droneRange;
			_ignoreDrones = ignoreDrones;
		}

		protected override IShip FindNewEnemy(Context context)
		{
			var options = EnemyMatchingOptions.EnemyForDrone(context.AttackRangeMax + _droneRange);
			options.IgnoreDrones = _ignoreDrones;
			return context.Scene.Ships.GetEnemy(context.Ship, options);
		}

		protected override bool IsValidEnemy(IShip enemy, Context context)
		{
			var ship = context.Ship;
			if (!enemy.IsActive()) return false;
			if (enemy.Type.Side.IsAlly(ship.Type.Side)) return false;
			if (_ignoreDrones && enemy.Type.Class != UnitClass.Ship) return false;

			var mothership = context.Mothership;
			if (mothership == null) return true;
			return Helpers.Distance(mothership, enemy) <= context.AttackRangeMax + _droneRange;
		}
	}
}
