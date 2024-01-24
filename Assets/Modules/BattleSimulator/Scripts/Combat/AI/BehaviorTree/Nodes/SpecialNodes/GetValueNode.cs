namespace Combat.Ai.BehaviorTree.Nodes
{
	public class GetValueNode : INode
	{
		private readonly string _name;

		public GetValueNode(string name)
		{
			_name = name;
		}

		public NodeState Evaluate(Context context)
		{
			return context.GetValue(_name) ? NodeState.Success : NodeState.Failure;
		}
	}
}
