namespace Combat.Ai.BehaviorTree.Nodes
{
	public class ForgetTargetNode : INode
	{
		private int _targetId = -1;

		public ForgetTargetNode(int targetId)
		{
			_targetId = targetId;
		}

		public ForgetTargetNode() {}

		public NodeState Evaluate(Context context)
		{
			if (_targetId >= 0)
				context.TrySaveTarget(_targetId, null);
			else
				context.TargetShip = null;

			return NodeState.Success;
		}
	}
}
