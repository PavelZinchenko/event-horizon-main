using Combat.Ai.Calculations;
using Combat.Component.Ship;
using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class EscapeAttackRadiusNode : INode
	{
		private IShip _target;
		private float _targetAttackRadius;

		public NodeState Evaluate(Context context)
		{
			UpdateTarget(context.TargetShip);

			if (_target == null)
				return NodeState.Success;

			var minDistance = _targetAttackRadius + _target.Body.Scale + context.Ship.Body.Scale;
			if (ShipNavigationHandler.FlyAround(context.Ship, _target, minDistance, 1000f, 0, context.Controls))
				return NodeState.Success;

			return NodeState.Running;
		}

		private void UpdateTarget(IShip target)
		{
			if (_target == target) return;
			if (target == null || target.State != Unit.UnitState.Active)
			{
				_target = null;
				return;
			}

			_target = target;

			float rangeMin = 0, rangeMax = 0;
			var systems = target.Systems.All;
			for (int i = 0; i < systems.Count; ++i)
				systems.UpdateAttackRangeIfWeapon(i, ref rangeMin, ref rangeMax);

			_targetAttackRadius = rangeMax;
		}
	}
}
