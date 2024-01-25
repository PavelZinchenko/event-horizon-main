namespace Combat.Ai.BehaviorTree.Nodes
{
	public class GetValueNode : INode
	{
		private readonly int _id;

		public GetValueNode(int id)
		{
			_id = id;
		}

		public NodeState Evaluate(Context context)
		{
			return context.GetValue(_id) ? NodeState.Success : NodeState.Failure;
		}
	}
}
