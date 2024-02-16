using Combat.Component.Ship;
using Combat.Component.Systems.Weapons;
using GameDatabase.Enums;

namespace Combat.Ai
{
	public static class ActionFactory
	{
		public static IAction CreateAttackAction(IShip ship, int weaponIndex, int level)
		{
			var weapon = ship.Systems.All.Weapon(weaponIndex);
			
			switch (weapon.Info.BulletType)
			{
			case AiBulletBehavior.Projectile:
                if (weapon.Info.WeaponType == WeaponType.Manageable)
                    return new ProjectileAttackAction(weaponIndex);
                return level < 100 ? (IAction)(new DirectAttackAction(weaponIndex)) : (IAction)(new ProjectileAttackAction(weaponIndex));
			case AiBulletBehavior.Homing:
				return new HomingAttackAction(weaponIndex);
			case AiBulletBehavior.Beam:
				return new DirectAttackAction(weaponIndex);
			case AiBulletBehavior.AreaOfEffect:
				return new ExplosionAttackAction(weaponIndex);
			default:
				throw new System.NotSupportedException();
			}			
		}		
	}
}
