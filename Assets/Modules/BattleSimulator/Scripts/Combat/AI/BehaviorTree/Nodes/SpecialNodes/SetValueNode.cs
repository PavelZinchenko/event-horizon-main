namespace Combat.Ai.BehaviorTree.Nodes
{
	public class SetValueNode : INode
	{
		private readonly string _name;
		private readonly bool _value;

		public SetValueNode(string name, bool value)
		{
			_name = name;
			_value = value;
		}

		public NodeState Evaluate(Context context)
		{
			return context.TrySetValue(_name, _value) ? NodeState.Success : NodeState.Failure;
		}
	}
}
