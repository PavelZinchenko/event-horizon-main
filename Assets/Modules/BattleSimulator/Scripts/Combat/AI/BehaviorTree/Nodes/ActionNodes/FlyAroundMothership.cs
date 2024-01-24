using Combat.Unit;
using Combat.Ai.Calculations;
using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class FlyAroundMothership : INode
	{
		private readonly float _distance;

		public FlyAroundMothership(float distance)
		{
			_distance = distance;
		}

		public NodeState Evaluate(Context context)
		{
			var ship = context.Ship;

			if (ship.Type.Class != UnitClass.Drone)
				return NodeState.Failure;

			var mothership = context.Ship.Type.Owner;
			if (!mothership.IsActive())
				return NodeState.Failure;

			if (ShipNavigationHandler.FlyAround(ship, mothership, _distance, _distance, 1, context.Controls))
				return NodeState.Success;

			return NodeState.Running;
		}
	}
}
