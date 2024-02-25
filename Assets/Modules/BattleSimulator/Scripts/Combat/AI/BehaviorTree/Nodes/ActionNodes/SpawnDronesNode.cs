using Combat.Component.Systems.Devices;
using Combat.Component.Systems.DroneBays;
using Combat.Component.Ship;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class SpawnDronesNode : INode
	{
		private Utils.ShipSystemList<IDroneBay> _droneBays;
		private Utils.ShipSystemList<ClonningDevice> _clonningDevices;

		public SpawnDronesNode(IShip ship)
		{
			var systems = ship.Systems.All;
			for (var i = 0; i < systems.Count; i++)
			{
				var system = systems[i];
                if (system.IsAutomatic) continue;

				if (system is IDroneBay droneBay)
					_droneBays.Add(droneBay, i);
				if (system is ClonningDevice clonningDevice)
					_clonningDevices.Add(clonningDevice, i);
			}
		}

		public NodeState Evaluate(Context context)
		{
			int dronesLeft = 0;
			int activeDrones = 0;
			int clonesLeft = 0;
			int activeClones = 0;

			for (var i = 0; i < _droneBays.Count; ++i)
			{
				var dronebay = _droneBays[i];
				context.Controls.ActivateSystem(dronebay.Id);
				dronesLeft += dronebay.Value.DronesInHangar;
				activeDrones += dronebay.Value.ActiveDrones;
			}

			for (var i = 0; i < _clonningDevices.Count; ++i)
			{
				var device = _clonningDevices[i];
				context.Controls.ActivateSystem(device.Id);
				if (device.Value.Clone == null)
					clonesLeft++;
				else
					activeClones++;
			}

			if (activeDrones == 0 && dronesLeft == 0 && activeClones == 0 && clonesLeft == 0) 
				return NodeState.Failure;

			return dronesLeft + clonesLeft > 0 ? NodeState.Running : NodeState.Success;
		}
	}
}
