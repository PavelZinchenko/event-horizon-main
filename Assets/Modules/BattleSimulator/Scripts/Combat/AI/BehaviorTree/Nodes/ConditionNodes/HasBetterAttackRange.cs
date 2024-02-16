using Combat.Component.Ship;
using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class HasBetterAttackRange : INode
	{
		private IShip _target;
		private float _targetAttackRadius;

        private readonly float _targetRangeMultiplier;

        public HasBetterAttackRange(float targetRangeMultiplier)
        {
            _targetRangeMultiplier = targetRangeMultiplier;
        }

        public NodeState Evaluate(Context context)
		{
			UpdateTarget(context.TargetShip);

			if (_target == null)
				return NodeState.Failure;

			return context.AttackRangeMax > _targetAttackRadius*_targetRangeMultiplier ? NodeState.Success : NodeState.Failure;
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
            target.Systems.All.CalculateAttackRange(out var rangeMin, out var rangeMax);
			_targetAttackRadius = rangeMax;
		}
	}
}
