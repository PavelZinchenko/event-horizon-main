namespace Combat.Ai.BehaviorTree.Nodes
{
	public class SaveTargetNode : INode
	{
		private readonly int _id;

		public SaveTargetNode(int id)
		{
			_id = id;
		}

		public NodeState Evaluate(Context context)
		{
			return context.TrySaveTarget(_id, context.TargetShip) ? NodeState.Success : NodeState.Failure;
		}
	}
}
