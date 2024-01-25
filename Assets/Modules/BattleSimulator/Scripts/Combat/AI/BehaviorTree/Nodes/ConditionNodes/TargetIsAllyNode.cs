using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class TargetIsAllyNode : INode
	{
		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null || context.TargetShip.State != Unit.UnitState.Active)
				return NodeState.Failure;

			return context.TargetShip.Type.Side.IsAlly(context.Ship.Type.Side) ? NodeState.Success : NodeState.Failure;
		}
	}
}
