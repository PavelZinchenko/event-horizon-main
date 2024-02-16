using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class TargetMothershipNode : INode
	{
		public NodeState Evaluate(Context context)
		{
			var mothership = context.Mothership;
			if (!mothership.IsActive()) 
				return NodeState.Failure;

			context.TargetShip = mothership;
			return NodeState.Success;
		}
	}
}
