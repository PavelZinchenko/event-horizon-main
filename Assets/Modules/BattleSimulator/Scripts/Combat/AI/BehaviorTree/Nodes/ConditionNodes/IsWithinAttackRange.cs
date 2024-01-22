using UnityEngine;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class IsWithinAttackRange : INode
	{
		private readonly float _lerpMinMax;

		public IsWithinAttackRange(float lerpMinMax)
		{
			_lerpMinMax = lerpMinMax;
		}

		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null)
				return NodeState.Failure;

			var min = context.SelectedWeapons.RangeMin;
			var max = context.SelectedWeapons.RangeMax;
			float distance = Mathf.Lerp(min, max, _lerpMinMax);

			if (distance <= 0)
				return NodeState.Failure;

			var currentDistance = Helpers.Distance(context.Ship, context.TargetShip);
			return currentDistance <= distance ? NodeState.Success : NodeState.Failure;
		}
	}
}
