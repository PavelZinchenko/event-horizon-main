using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class TargetIsEnemyNode : INode
	{
		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null || context.TargetShip.State != Unit.UnitState.Active)
				return NodeState.Failure;

			return CombatRelations.AreEnemies(context.TargetShip.Type, context.Ship.Type) ? NodeState.Success : NodeState.Failure;
		}
	}
}
