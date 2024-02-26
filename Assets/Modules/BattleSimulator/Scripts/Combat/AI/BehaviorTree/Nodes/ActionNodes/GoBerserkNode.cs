namespace Combat.Ai.BehaviorTree.Nodes
{
	public class GoBerserkNode : INode
	{
		public NodeState Evaluate(Context context)
		{
			if (!context.IsDrone)
				return NodeState.Failure;

			if (context.Ship.Type.Owner == null)
				return NodeState.Failure;

			context.Ship.Type.Owner = null;
			return NodeState.Success;
		}
	}
}
