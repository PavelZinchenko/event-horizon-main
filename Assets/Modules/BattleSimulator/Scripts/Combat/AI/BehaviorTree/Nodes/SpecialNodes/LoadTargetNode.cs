namespace Combat.Ai.BehaviorTree.Nodes
{
	public class LoadTargetNode : INode
	{
		private readonly int _id;

		public LoadTargetNode(int id)
		{
			_id = id;
		}

		public NodeState Evaluate(Context context)
		{
			return context.LoadTarget(_id) ? NodeState.Success : NodeState.Failure;
		}
	}
}
