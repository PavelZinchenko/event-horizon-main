namespace Combat.Ai.BehaviorTree.Nodes
{
	public class IsLowOnHp : INode
	{
		private readonly float _minValue;

		public IsLowOnHp(float minValue)
		{
			_minValue = minValue;
		}

		public NodeState Evaluate(Context context)
		{
			var hp = context.Ship.Stats.Armor.Percentage;
			return hp < _minValue ? NodeState.Success : NodeState.Failure;
		}
	}
}
