namespace Combat.Ai.BehaviorTree.Nodes
{
	public class HasIncomingThreatNode : INode
	{
		private readonly float _maxTimeToHit;

		public HasIncomingThreatNode(float maxTimeToHit)
		{
			_maxTimeToHit = maxTimeToHit;
		}

		public NodeState Evaluate(Context context)
		{
			return context.TimeToCollision > _maxTimeToHit ? NodeState.Failure : NodeState.Success;
		}
	}
}
