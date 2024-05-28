using UnityEngine;
using Combat.Ai.Calculations;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class MaintainAttackRange : INode
	{
		private readonly float _lerpMinMax;
		private readonly float _tolerance;
        private readonly float _rangeLimit;

		public MaintainAttackRange(float lerpMinMax, float tolerance)
		{
			_lerpMinMax = lerpMinMax;
			_tolerance = tolerance;
		}

		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null)
				return NodeState.Failure;

			var min = context.SelectedWeapons.RangeMin;
			var max = context.SelectedWeapons.RangeMax;
			float maxDistance = Mathf.Lerp(min, max, _lerpMinMax);

            var distanceLimit = 0.5f * context.Scene.Settings.AreaWidth;

            if (maxDistance > distanceLimit)
                maxDistance = distanceLimit;

			float minDistance = maxDistance - maxDistance*_tolerance;

			var ship = context.Ship;
			var target = context.TargetShip;

			if (ShipNavigationHandler.FlyAround(ship, target, minDistance, maxDistance, 0, context.Controls))
				return NodeState.Success;

			return NodeState.Running;
		}
	}
}
