using Combat.Ai.Calculations;
using Combat.Component.Ship;
using Combat.Component.Systems.Devices;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class ChaseNode : INode
	{
        private int _accelerationDeviceId = -1;
        private int _warpDriveDeviceId = -1;
        private float _maxWarpDistance;

        public ChaseNode(IShip ship, bool useSystems)
        {
            if (useSystems)
            {
                _accelerationDeviceId = ship.Systems.All.FindFirst<AcceleratorDevice>();
                _warpDriveDeviceId = ship.Systems.All.FindFirst<WarpDrive>(out WarpDrive warpDrive);
                if (warpDrive != null) _maxWarpDistance = warpDrive.MaxRange;
            }
        }

        public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null)
				return NodeState.Failure;

			var status = ShipNavigationHandler.ChaseAndRam(context.Ship, context.TargetShip, context.Controls, out var timeToHit, out var deltaAngle);

            if (status == ShipNavigationHandler.Status.FullThrottle)
            {
                if (_accelerationDeviceId >= 0) UseAfterburner(context, deltaAngle);
                if (_warpDriveDeviceId >= 0) UseWarpDrive(context, deltaAngle);
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

            var minAttackRange = context.SelectedWeapons.RangeMin;
            var distance = Helpers.Distance(context.Ship, context.TargetShip) - minAttackRange;
            if (distance <= minDistance) return;

            context.Controls.ActivateSystem(_accelerationDeviceId);
        }

        private void UseWarpDrive(Context context, float deltaAngle)
        {
            const float maxDeltaAngle = 5f;
            const float minJumpDistance = 5f;

            var isActive = context.Ship.Systems.All[_warpDriveDeviceId].Active;
            if (!isActive && deltaAngle > maxDeltaAngle) return;

            var minAttackRange = context.SelectedWeapons.RangeMin;
            var distance = Helpers.Distance(context.Ship, context.TargetShip) - minAttackRange;
            if (distance <= 0 || distance > _maxWarpDistance || !isActive && distance < minJumpDistance) return;

            context.Controls.ActivateSystem(_warpDriveDeviceId);
        }
    }
}
