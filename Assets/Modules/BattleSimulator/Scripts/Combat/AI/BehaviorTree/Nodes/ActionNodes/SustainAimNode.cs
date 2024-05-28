using Combat.Component.Platform;
using Combat.Component.Systems.Weapons;
using GameDatabase.Enums;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class SustainAimNode : INode
	{
		private readonly bool _onlyDirectWeapon;

		public SustainAimNode(bool onlyDirectWeapon)
		{
			_onlyDirectWeapon = onlyDirectWeapon;
		}

		public NodeState Evaluate(Context context)
		{
			var ship = context.Ship;

			for (int i = 0; i < context.SelectedWeapons.List.Count; ++i)
			{
				var weapon = context.SelectedWeapons.List[i].Weapon;
				if (!ShouldTrack(weapon)) continue;

				if (context.TargetShip != null)
					if (AttackHelpers.TryGetTarget(weapon, ship, context.TargetShip, out var target))
					{
						context.Controls.Course = weapon.Platform.OptimalShipCourse(target);
						return NodeState.Running;
					}

				for (int j = 0; j < context.SecondaryTargets.Count; ++j)
				{
					var enemy = context.SecondaryTargets[j];
					if (AttackHelpers.TryGetTarget(weapon, ship, enemy, out var target))
					{
						context.Controls.Course = weapon.Platform.OptimalShipCourse(target);
						return NodeState.Running;
					}
				}
			}

			return NodeState.Failure;
		}

		private bool ShouldTrack(IWeapon weapon)
		{
			var type = weapon.Info.WeaponType;
			switch (type)
			{
				case WeaponType.Common:
				case WeaponType.Continuous:
					if (!weapon.Active) return false;
					break;
				case WeaponType.Manageable:
                    return false;
                case WeaponType.RequiredCharging:
                    if (weapon.ActiveBullet == null) return false;
                    break;
			}

			var bulletType = weapon.Info.BulletType;
			switch (bulletType)
			{
				case AiBulletBehavior.Beam:
					return true;
				case AiBulletBehavior.Projectile:
					return !_onlyDirectWeapon;
				default:
					return false;
			}
		}
	}
}
