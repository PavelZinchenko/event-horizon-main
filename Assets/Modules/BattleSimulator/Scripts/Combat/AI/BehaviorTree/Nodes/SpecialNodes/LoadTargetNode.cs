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
			context.TargetShip = context.LoadTarget(_id);
			return context.TargetShip != null ? NodeState.Success : NodeState.Failure;
		}
	}
}
