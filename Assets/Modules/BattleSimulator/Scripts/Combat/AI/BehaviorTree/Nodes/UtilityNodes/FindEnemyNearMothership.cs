using Combat.Unit;
using Combat.Scene;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class FindEnemyNearMothership : FindEnemyNodeBase
	{
		private const float _minDistance = 5f;
		private readonly float _distance;
		private readonly bool _ignoreDrones;

		public FindEnemyNearMothership(float findEnemyCooldown, float changeEnemyCooldown, float distance, bool ignoreDrones)
			: base(findEnemyCooldown, changeEnemyCooldown)
		{
			_ignoreDrones = ignoreDrones;
            _distance = distance < _minDistance ? _minDistance : distance;
		}

		protected override IShip FindNewEnemy(Context context)
		{
            EnemyMatchingOptions options;
            if (context.Mothership.IsActive())
                options = context.IsDrone ? EnemyMatchingOptions.EnemyForDrone(context.AttackRangeMax + _distance) :
                    EnemyMatchingOptions.EnemyForShip(context.AttackRangeMax + _distance);
            else
                options = context.IsDrone ? EnemyMatchingOptions.EnemyForDrone(0) : EnemyMatchingOptions.EnemyForShip(0);

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
			if (!mothership.IsActive()) return true;

			return Helpers.Distance(mothership, enemy) <= context.AttackRangeMax + _distance;
		}
	}
}
