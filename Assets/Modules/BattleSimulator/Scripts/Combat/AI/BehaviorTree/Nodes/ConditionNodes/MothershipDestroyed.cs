using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class MothershipDestroyed : INode
	{
		public NodeState Evaluate(Context context)
		{
			var mothership = context.Mothership;
			return mothership != null && mothership.State == UnitState.Destroyed ? NodeState.Success : NodeState.Failure;
		}
	}
}
