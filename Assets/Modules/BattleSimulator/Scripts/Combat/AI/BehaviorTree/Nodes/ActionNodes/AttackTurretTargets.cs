using Combat.Ai.Calculations;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class AttackTurretTargets : INode
	{
		public NodeState Evaluate(Context context)
		{
            return AimAndAttackHandler.AttackTurretTargets(context.Ship, context.SelectedWeapons, context.Controls) ?
                NodeState.Success : NodeState.Failure;
		}
	}
}
