using UnityEngine;
using Combat.Component.Ship;
using Combat.Unit;
using Combat.Component.Body;
using Combat.Ai.BehaviorTree.Utils;
using Combat.Component.Systems.Weapons;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class TrackControllableAmmo : INode
	{
		private readonly ShipSystemList<IWeapon> _weapons;
		private readonly bool _secondaryTargets;

		public static INode Create(IShip ship, bool secondaryTargets)
		{
			var weapons = ship.Systems.All.FindWeaponsByType(WeaponType.Manageable);
			if (weapons.Count == 0) return EmptyNode.Failure;
			return new TrackControllableAmmo(weapons, secondaryTargets);
		}

		private TrackControllableAmmo(ShipSystemList<IWeapon> weapons, bool secondaryTargets)
		{
			_weapons = weapons;
			_secondaryTargets = secondaryTargets;
		}

		public NodeState Evaluate(Context context)
		{
			var enemy = context.TargetShip;
			var result = NodeState.Failure;

			for (int i = 0; i < _weapons.Count; ++i)
			{
				var weapon = _weapons[i];

				if (enemy != null && ShouldActivate(weapon.Value, enemy))
				{
					context.Controls.ActivateSystem(weapon.Id);
					result = NodeState.Running;
					continue;
				}

				var targets = context.SecondaryTargets;
				if (!_secondaryTargets || targets.Count == 0) 
					continue;

				var shouldActivate = false;
				for (int j = 0; j < targets.Count; ++j)
					shouldActivate |= ShouldActivate(weapon.Value, targets[j]);

				if (!shouldActivate) continue;
				context.Controls.ActivateSystem(weapon.Id, shouldActivate);
				result = NodeState.Running;
			}

			return result;
		}

		public bool ShouldActivate(IWeapon weapon, IShip enemy)
		{
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
