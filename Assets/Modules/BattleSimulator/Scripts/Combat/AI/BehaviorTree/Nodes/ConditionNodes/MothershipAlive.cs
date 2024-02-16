using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class MothershipAlive : INode
	{
		public NodeState Evaluate(Context context)
		{
			var mothership = context.Mothership;
			return mothership != null && mothership.State == UnitState.Active ? NodeState.Success : NodeState.Failure;
		}
	}
}
