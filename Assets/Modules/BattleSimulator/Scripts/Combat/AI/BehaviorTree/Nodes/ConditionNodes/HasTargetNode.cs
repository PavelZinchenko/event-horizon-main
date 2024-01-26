namespace Combat.Ai.BehaviorTree.Nodes
{
	public class HasTargetNode : INode
	{
		private int _targetId = -1;

		public HasTargetNode(int targetId)
		{
			_targetId = targetId;
		}

		public HasTargetNode() {}

		public NodeState Evaluate(Context context)
		{
			var mainTarget = _targetId >= 0 ? context.LoadTarget(_targetId) : context.TargetShip;
			return mainTarget == null || mainTarget.State != Unit.UnitState.Active ? NodeState.Failure : NodeState.Success;
		}
	}
}
