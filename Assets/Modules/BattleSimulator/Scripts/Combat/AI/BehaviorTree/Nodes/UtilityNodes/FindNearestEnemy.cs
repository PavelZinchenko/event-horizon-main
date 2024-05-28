using Combat.Unit;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class FindNearestEnemy : FindEnemyNodeBase
	{
        private readonly ShipTargetLocator _targetLocator;
		private readonly bool _ignoreDrones;

        public FindNearestEnemy(ShipTargetLocator targetLocator, float findEnemyCooldown, float changeEnemyCooldown, bool ignoreDrones)
			: base(findEnemyCooldown, changeEnemyCooldown)
		{
            _targetLocator = targetLocator;
			_ignoreDrones = ignoreDrones;
        }

        protected override IShip FindNewEnemy(Context context)
		{
            if (context.IsDrone)
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
			return true;
		}
	}
}
