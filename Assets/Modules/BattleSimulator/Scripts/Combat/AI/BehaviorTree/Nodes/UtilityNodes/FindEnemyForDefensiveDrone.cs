using Combat.Unit;
using Combat.Scene;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class FindEnemyForDefensiveDrone : FindEnemyNodeBase
	{
		private const float _minRange = 5f;
		private readonly float _droneRange;
		private readonly bool _ignoreDrones;

		public FindEnemyForDefensiveDrone(float findEnemyCooldown, float changeEnemyCooldown, float droneRange, bool ignoreDrones)
			: base(findEnemyCooldown, changeEnemyCooldown)
		{
			_ignoreDrones = ignoreDrones;
			_droneRange = System.Math.Max(_minRange, droneRange / 4);
		}

		protected override IShip FindNewEnemy(Context context)
		{
			var range = context.AttackRangeMax + _droneRange;
			var options = EnemyMatchingOptions.EnemyForDrone(range);
			options.IgnoreDrones = _ignoreDrones;
			return context.Scene.Ships.GetEnemy(context.Mothership ?? context.Ship, options);
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
