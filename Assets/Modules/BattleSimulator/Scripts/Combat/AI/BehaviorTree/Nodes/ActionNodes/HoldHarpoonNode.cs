using Combat.Component.Ship;
using Combat.Ai.BehaviorTree.Utils;
using Combat.Component.Systems.Weapons;
using Combat.Component.Unit.Classification;
using GameDatabase.Enums;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class HoldHarpoonNode : INode
	{
        private const float _releaseCooldown = 0.5f;
		private readonly ShipSystemList<IWeapon> _weapons;
        private float _timeLeft;

		public static INode Create(IShip ship)
		{
			var weapons = ship.Systems.All.FindWeaponsByBulletType(AiBulletBehavior.Harpoon);
			if (weapons.Count == 0) return EmptyNode.Failure;
			return new HoldHarpoonNode(weapons);
		}

		public NodeState Evaluate(Context context)
		{
            bool hasTarget = false;
			for (int i = 0; i < _weapons.Count; ++i)
			{
				var weapon = _weapons[i];
                var bullet = weapon.Value.ActiveBullet;
                if (bullet == null) continue;
                var target = bullet.Collider.ActiveTrigger;
                
                if (target == null)
                {
                    _timeLeft -= context.DeltaTime; // keep active for a while
                    if (_timeLeft < 0) continue;
                }
                else
                {
                    if (target.Type.Class != UnitClass.Ship) continue;
                    if (target.Type.Side.IsAlly(context.Ship.Type.Side)) continue;
                    _timeLeft = _releaseCooldown;
                }

                hasTarget = true;
                context.Controls.ActivateSystem(weapon.Id, true);
            }

            return hasTarget ? NodeState.Success : NodeState.Failure;
		}

		private HoldHarpoonNode(ShipSystemList<IWeapon> weapons)
		{
			_weapons = weapons;
		}
	}
}
