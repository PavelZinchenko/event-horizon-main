using Combat.Unit;
using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class MothershipRetreated : INode
	{
		public NodeState Evaluate(Context context)
		{
			if (context.Ship.Type.Class != UnitClass.Drone)
				return NodeState.Failure;

			var mothership = context.Ship.Type.Owner;
			return mothership != null && mothership.State == UnitState.Inactive ? NodeState.Success : NodeState.Failure;
		}
	}
}
