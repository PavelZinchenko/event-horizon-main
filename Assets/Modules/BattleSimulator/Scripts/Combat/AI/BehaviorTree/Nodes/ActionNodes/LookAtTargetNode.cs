namespace Combat.Ai.BehaviorTree.Nodes
{
	public class LookAtTargetNode : INode
	{
		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null)
				return NodeState.Failure;

			var direction = context.Ship.Body.Position.Direction(context.TargetShip.Body.Position);
			context.Controls.Course = RotationHelpers.Angle(direction);
			return NodeState.Running;
		}
	}
}
