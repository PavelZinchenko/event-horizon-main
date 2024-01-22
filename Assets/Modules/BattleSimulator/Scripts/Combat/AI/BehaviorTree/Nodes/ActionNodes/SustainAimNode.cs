using Combat.Component.Systems.Weapons;

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
			var enemy = context.TargetShip;

			if (enemy == null)
				return NodeState.Failure;

			for (int i = 0; i < context.SelectedWeapons.Count; ++i)
			{
				var weapon = context.SelectedWeapons.GetWeaponByIndex(i);
				if (!weapon.Active) continue;
				if (!IsSuitableWeaponType(weapon.Info.BulletType)) continue;

				if (!AttackHelpers.TryGetTarget(weapon, ship, enemy, out var target)) continue;
				context.Controls.Course = Helpers.TargetCourse(ship, target, weapon.Platform);
				return NodeState.Running;
			}

			return NodeState.Failure;
		}

		private bool IsSuitableWeaponType(BulletType bulletType)
		{
			switch (bulletType)
			{
				case BulletType.Direct:
					return true;
				case BulletType.Projectile:
					return !_onlyDirectWeapon;
				default:
					return false;
			}
		}
	}
}
