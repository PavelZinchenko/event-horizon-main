namespace Combat.Ai.BehaviorTree.Nodes
{
	public class MovingForwardNode : INode
	{
		private readonly float _minValue;
        private readonly float _maxDeltaAngle;

		public MovingForwardNode(float minThrottle, float maxDeltaAngle)
		{
			_minValue = minThrottle;
            _maxDeltaAngle = maxDeltaAngle;
		}

		public NodeState Evaluate(Context context)
		{
            var ship = context.Ship;
            if (ship.Controls.Throttle < _minValue) return NodeState.Failure;
            if (!ship.Controls.Course.HasValue) return NodeState.Success;
            var delta = UnityEngine.Mathf.DeltaAngle(ship.Controls.Course.Value, ship.Body.Rotation);
            if (delta > _maxDeltaAngle || delta < -_maxDeltaAngle) return NodeState.Failure;
            return NodeState.Success;
		}
	}
}
