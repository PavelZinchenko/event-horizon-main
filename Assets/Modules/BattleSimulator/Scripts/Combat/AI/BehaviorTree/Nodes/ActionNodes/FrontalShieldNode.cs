using UnityEngine;
using GameDatabase.Enums;
using Combat.Component.Ship;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class FrontalShieldNode : INode
	{
		private readonly int _deviceId;

		public static INode Create(IShip ship)
		{
			var deviceId = ship.Systems.All.FindFirstDevice(DeviceClass.PartialShield);
			return deviceId >= 0 ? new FrontalShieldNode(deviceId) : EmptyNode.Failure;
		}

		public NodeState Evaluate(Context context)
		{
			if (context.Threats.Count == 0) 
				return NodeState.Failure;

			var ship = context.Ship;

			var target = Vector2.zero;
			for (var i = 0; i < context.Threats.Count; ++i)
			{
				var dir = ship.Body.Position.Direction(context.Threats[i].Body.Position).normalized;
				target += dir;
			}

			var course = RotationHelpers.Angle(target);
			context.Controls.Course = course;
			if (Mathf.Abs(Mathf.DeltaAngle(course, ship.Body.Rotation)) < 90)
				context.Controls.ActivateSystem(_deviceId);

			return NodeState.Running;
		}

		private FrontalShieldNode(int deviceId)
		{
			_deviceId = deviceId;
		}
	}
}
