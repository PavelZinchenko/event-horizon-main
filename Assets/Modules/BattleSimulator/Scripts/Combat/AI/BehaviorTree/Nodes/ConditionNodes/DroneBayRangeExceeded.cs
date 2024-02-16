using Combat.Component.Unit.Classification;
using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class DroneBayRangeExceeded : INode
	{
		private readonly float _maxRange;

        public static INode Create(float maxRange)
        {
            if (maxRange <= 0) 
                return EmptyNode.Failure;

            return new DroneBayRangeExceeded(maxRange);
        }

		public NodeState Evaluate(Context context)
		{
			if (context.Ship.Type.Class != UnitClass.Drone)
				return NodeState.Failure;

			var mothership = context.Ship.Type.Owner;
			if (!mothership.IsActive()) 
				return NodeState.Failure;

			var currentDistance = Helpers.Distance(context.Ship, mothership);
			return currentDistance > _maxRange ? NodeState.Success : NodeState.Failure;
		}

        private DroneBayRangeExceeded(float maxRange) { _maxRange = maxRange; }
    }
}
