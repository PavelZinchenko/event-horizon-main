using Combat.Component.Systems.Devices;
using Combat.Component.Ship;
using Combat.Ai.Calculations;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class RamNode : INode
	{
		private int _accelerationDeviceId = -1;
		private int _fortificationDeviceId = -1;

		public RamNode(IShip ship, bool useSystems)
		{
			if (useSystems)
			{
				_accelerationDeviceId = ship.Systems.All.FindFirst<AcceleratorDevice>();
				_fortificationDeviceId = ship.Systems.All.FindFirst<FortificationDevice>();
			}
		}

		public NodeState Evaluate(Context context)
		{
			var status = ShipNavigationHandler.ChaseAndRam(context.Ship, context.TargetShip, context.Controls);

			switch (status)
			{
				case ShipNavigationHandler.Status.NoTarget:
				case ShipNavigationHandler.Status.Following:
					return NodeState.Failure;
				case ShipNavigationHandler.Status.FullThrottle:
					if (_accelerationDeviceId >= 0)
						context.Controls.ActivateSystem(_accelerationDeviceId);
					break;
				case ShipNavigationHandler.Status.AboutToHit:
					if (_fortificationDeviceId >= 0)
						context.Controls.ActivateSystem(_fortificationDeviceId);
					break;
			}

			return NodeState.Running;
		}
	}
}
