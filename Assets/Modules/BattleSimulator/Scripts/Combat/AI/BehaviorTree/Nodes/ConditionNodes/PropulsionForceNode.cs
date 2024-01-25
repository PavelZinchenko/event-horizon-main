namespace Combat.Ai.BehaviorTree.Nodes
{
	public class PropulsionForceNode : INode
	{
		private readonly float _minValue;

		public PropulsionForceNode(float minValue)
		{
			_minValue = minValue;
		}

		public NodeState Evaluate(Context context)
		{
			var propulsion = context.Ship.Engine.Propulsion;
			var forwardAcceleration = context.Ship.Engine.ForwardAcceleration;
			if (propulsion <= 0) 
				return NodeState.Failure;

			return forwardAcceleration / propulsion >= _minValue ? NodeState.Success : NodeState.Failure;
		}
	}
}
