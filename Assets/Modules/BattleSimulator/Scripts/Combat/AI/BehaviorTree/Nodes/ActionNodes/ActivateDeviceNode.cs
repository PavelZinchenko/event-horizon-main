using GameDatabase.Enums;
using Combat.Component.Ship;
using Combat.Component.Systems.Devices;
using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class ActivateDeviceNode : INode
	{
		private readonly ShipSystemList<IDevice> _devices;

		public static INode Create(IShip ship, DeviceClass deviceClass)
		{
			var devices = ship.Systems.All.FindDevices(deviceClass);
			return devices.Count > 0 ? new ActivateDeviceNode(devices) : EmptyNode.Failure;
		}

		public NodeState Evaluate(Context context)
		{
			int activated = 0;
			for (int i = 0; i < _devices.Count; ++i)
			{
				if (_devices[i].Value.CanBeActivated)
				{
					context.Controls.ActivateSystem(_devices[i].Id);
					activated++;
				}
			}

			return activated > 0 ? NodeState.Success : NodeState.Failure;
		}

		private ActivateDeviceNode(ShipSystemList<IDevice> devices)
		{
			_devices = devices;
		}
	}
}
