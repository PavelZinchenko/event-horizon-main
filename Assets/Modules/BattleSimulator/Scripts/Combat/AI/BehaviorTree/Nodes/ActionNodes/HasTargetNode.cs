namespace Combat.Ai.BehaviorTree.Nodes
{
	public class HasTargetNode : INode
	{
		private bool _ignoreSecondary;

		public HasTargetNode(bool ignoreSecondary)
		{
			_ignoreSecondary = ignoreSecondary;
		}

		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null)
				if (_ignoreSecondary || context.SecondaryTargets.Count > 0)
					return NodeState.Failure;

			return NodeState.Success;
		}
	}
}
