using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class TargetMessageSenderNode : INode
	{
		public NodeState Evaluate(Context context)
		{
			context.TargetShip = context.LastMessageSender;
			return context.TargetShip.IsActive() ? NodeState.Success : NodeState.Failure;
		}
	}
}
