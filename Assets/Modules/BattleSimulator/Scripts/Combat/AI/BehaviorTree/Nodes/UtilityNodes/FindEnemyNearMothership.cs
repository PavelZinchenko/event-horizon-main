using Combat.Unit;
using Combat.Scene;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class FindEnemyNearMothership : FindEnemyNodeBase
	{
        private readonly ShipTargetLocator _targetLocator;
        private const float _minDistance = 5f;
		private readonly float _distance;
		private readonly bool _ignoreDrones;

		public FindEnemyNearMothership(ShipTargetLocator targetLocator, float findEnemyCooldown, float changeEnemyCooldown, float distance, bool ignoreDrones)
			: base(findEnemyCooldown, changeEnemyCooldown)
		{
            _targetLocator = targetLocator;
			_ignoreDrones = ignoreDrones;
            _distance = distance < _minDistance ? _minDistance : distance;
		}

		protected override IShip FindNewEnemy(Context context)
		{
            if (context.Mothership.IsActive())
            {
                if (context.IsDrone)
                    return _targetLocator.FindEnemyForDrone(context.Ship, context.Mothership, _ignoreDrones, _distance + context.AttackRangeMax);
                else
                    return _targetLocator.FindEnemyForDefenderShip(context.Ship, context.Mothership, _ignoreDrones, _distance + context.AttackRangeMax);
            }
            else if (context.IsDrone)
                return _targetLocator.FindEnemyForHomelessDrone(context.Ship, _ignoreDrones);
            else
                return _targetLocator.FindEnemyForShip(context.Ship, _ignoreDrones);
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
