using Combat.Unit;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class FindDamagedAlly : FindEnemyNodeBase
	{
        private const float _maxHp = 0.99f;
        private readonly ShipTargetLocator _targetLocator;
        private const float _minDistance = 5f;
        private readonly float _distance;

        public FindDamagedAlly(ShipTargetLocator targetLocator, float findEnemyCooldown, float changeEnemyCooldown, float distance)
			: base(findEnemyCooldown, changeEnemyCooldown)
		{
            _targetLocator = targetLocator;
            _distance = distance < _minDistance ? _minDistance : distance;
        }

        protected override IShip FindNewEnemy(Context context)
		{
            return _targetLocator.FindTargetForRepairDrone(context.Ship, context.Mothership ?? context.Ship, _distance + context.AttackRangeMax);
		}

		protected override bool IsValidEnemy(IShip target, Context context)
		{
			var ship = context.Ship;
			if (!target.IsActive()) return false;
			if (target.Type.Side.IsEnemy(ship.Type.Side)) return false;
			if (target.Type.Class != UnitClass.Ship) return false;
            if (target.Stats.Armor.Percentage > _maxHp) return false;

            var mothership = context.Mothership;
            if (!mothership.IsActive()) return true;

            return Helpers.Distance(mothership, target) <= context.AttackRangeMax + _distance;
        }
    }
}
