namespace Combat.Ai.BehaviorTree.Nodes
{
	public class SetValueNode : INode
	{
		private readonly int _id;
		private readonly bool _value;

		public SetValueNode(int id, bool value)
		{
			_id = id;
			_value = value;
		}

		public NodeState Evaluate(Context context)
		{
			return context.TrySetValue(_id, _value) ? NodeState.Success : NodeState.Failure;
		}
	}
}
