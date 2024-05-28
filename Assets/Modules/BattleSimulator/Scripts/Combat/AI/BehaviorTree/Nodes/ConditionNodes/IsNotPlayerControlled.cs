namespace Combat.Ai.BehaviorTree.Nodes
{
	public class IsNotPlayerControlled : INode
	{
        private float _cooldown;

        public IsNotPlayerControlled(float cooldown)
        {
            _cooldown = cooldown;
        }

		public NodeState Evaluate(Context context)
		{
            return context.TimeSinceLastPlayerInput <= _cooldown ? NodeState.Failure : NodeState.Success;
		}
	}
}
