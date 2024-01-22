using Combat.Ai.Calculations;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class ChaseNode : INode
	{
		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null)
				return NodeState.Failure;

			ShipNavigationHandler.ChaseAndRam(context.Ship, context.TargetShip, context.Controls);
			return NodeState.Running;
		}
	}
}
