using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class SetMothershipNode : INode
	{
		public NodeState Evaluate(Context context)
		{
            var target = context.TargetShip;
            if (context.Mothership == target) 
                return NodeState.Failure;

            context.Mothership = target;
            return target != null && target.State == UnitState.Active ? NodeState.Success : NodeState.Failure;
		}
	}
}
