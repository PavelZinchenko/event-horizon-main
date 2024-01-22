namespace Combat.Ai.BehaviorTree.Nodes
{
	public class VanishNode : INode
	{
		public NodeState Evaluate(Context context)
		{
			context.Ship.Vanish();
			return NodeState.Success;
		}
	}
}
