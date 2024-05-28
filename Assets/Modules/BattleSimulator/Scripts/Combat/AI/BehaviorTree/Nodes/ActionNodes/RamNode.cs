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
			var status = ShipNavigationHandler.ChaseAndRam(context.Ship, context.TargetShip, context.Controls, out var timeToHit, out var deltaAngle);

            if (timeToHit < 1.0f)
            {
                if (_fortificationDeviceId >= 0)
                    context.Controls.ActivateSystem(_fortificationDeviceId);
            }
            else switch (status)
			{
				case ShipNavigationHandler.Status.NoTarget:
				case ShipNavigationHandler.Status.Following:
					return NodeState.Failure;
				case ShipNavigationHandler.Status.FullThrottle:
					if (_accelerationDeviceId >= 0) UseAfterburner(context, deltaAngle);
					break;
			}

			return NodeState.Running;
		}

        private void UseAfterburner(Context context, float deltaAngle)
        {
            const float maxDeltaAngle = 15f;
            const float minDistance = 10f;
            const float minEnergy = 0.5f;

            if (deltaAngle > maxDeltaAngle) return;
            var isActive = context.Ship.Systems.All[_accelerationDeviceId].Active;
            if (!isActive && context.Ship.Stats.Energy.Percentage < minEnergy) return;

            var distance = Helpers.Distance(context.Ship, context.TargetShip);
            if (distance < minDistance) return;

            context.Controls.ActivateSystem(_accelerationDeviceId);
        }
    }
}
