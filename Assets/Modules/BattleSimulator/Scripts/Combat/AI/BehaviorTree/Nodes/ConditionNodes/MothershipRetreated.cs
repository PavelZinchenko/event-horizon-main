using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class MothershipRetreated : INode
	{
		public NodeState Evaluate(Context context)
		{
			var mothership = context.Mothership;
			return mothership != null && mothership.State == UnitState.Inactive ? NodeState.Success : NodeState.Failure;
		}
	}
}
