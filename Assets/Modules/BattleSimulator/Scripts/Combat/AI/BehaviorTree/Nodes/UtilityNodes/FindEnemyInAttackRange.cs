﻿using Combat.Unit;
using Combat.Scene;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class FindEnemyInAttackRange : FindEnemyNodeBase
	{
        private readonly bool _ignoreDrones;

        public FindEnemyInAttackRange(float findEnemyCooldown, float changeEnemyCooldown, bool ignoreDrones)
			: base(findEnemyCooldown, changeEnemyCooldown)
		{
			_ignoreDrones = ignoreDrones;
        }

        protected override IShip FindNewEnemy(Context context)
		{
			var options = context.IsDrone ? EnemyMatchingOptions.EnemyForDrone(context.AttackRangeMax) :
                EnemyMatchingOptions.EnemyForShip(context.AttackRangeMax);

			options.IgnoreDrones = _ignoreDrones;
			return context.Scene.Ships.GetEnemy(context.Ship, options);
		}

		protected override bool IsValidEnemy(IShip enemy, Context context)
		{
			var ship = context.Ship;
			if (!enemy.IsActive()) return false;
			if (enemy.Type.Side.IsAlly(ship.Type.Side)) return false;
			if (_ignoreDrones && enemy.Type.Class != UnitClass.Ship) return false;
			return Helpers.Distance(ship, enemy) <= context.AttackRangeMax;
		}
	}
}
