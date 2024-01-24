namespace Combat.Ai.BehaviorTree.Nodes
{
	public class IsFaterThanTarget : INode
	{
		private readonly float _targetSpeedMultiplier;

		public IsFaterThanTarget(float targetSpeedMultiplier)
		{
			_targetSpeedMultiplier = targetSpeedMultiplier;
		}

		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null)
				return NodeState.Failure;

			var targetSpeed = _targetSpeedMultiplier * context.TargetShip.Engine.MaxVelocity;
			return context.Ship.Engine.MaxVelocity > targetSpeed ? NodeState.Success : NodeState.Failure;
		}
	}
}
