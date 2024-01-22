using UnityEngine;
using Combat.Component.Ship;
using Combat.Unit;
using Combat.Component.Body;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class UseControllableWeapon : INode
	{
		private readonly int _weaponId;
		private readonly bool _secondaryTargets;

		public UseControllableWeapon(int weaponId, bool secondaryTargets)
		{
			_weaponId = weaponId;
			_secondaryTargets = secondaryTargets;
		}

		public NodeState Evaluate(Context context)
		{
			var ship = context.Ship;
			var enemy = context.TargetShip;

			if (enemy == null)
				return NodeState.Failure;

			if (_secondaryTargets && context.SecondaryTargets.Count > 0)
			{
				var targets = context.SecondaryTargets;
				var shouldActivate = false;
				for (var i = 0; i < targets.Count; i++)
					shouldActivate |= ShouldActivate(ship, targets[i]);

				context.Controls.ActivateSystem(_weaponId, shouldActivate);
				return NodeState.Running;
			}

			context.Controls.ActivateSystem(_weaponId, ShouldActivate(ship, enemy));
			return NodeState.Running;
		}

		public bool ShouldActivate(IShip ship, IShip enemy)
		{
			var weapon = ship.Systems.All.Weapon(_weaponId);
			if (!weapon.Active)
				return false;

			var bullet = weapon.ActiveBullet;
			if (!bullet.IsActive())
				return false;

			var dir = bullet.Body.WorldPosition().Direction(enemy.Body.Position).normalized;
			var delta = Vector2.Dot(bullet.Body.WorldVelocity(), dir) - Vector2.Dot(enemy.Body.WorldVelocity(), dir);

			return delta >= 0;
		}
	}
}
