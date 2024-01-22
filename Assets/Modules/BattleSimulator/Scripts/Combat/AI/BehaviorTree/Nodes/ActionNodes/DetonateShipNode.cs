using UnityEngine;
using Combat.Component.Ship;
using Combat.Component.Systems.Devices;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class DetonateShipNode : INode
	{
		private readonly int _deviceId;
		private readonly bool _checkIfEnemyInRange;
		private readonly float _detonatorActivationTime;

		public DetonateShipNode(IShip ship, bool checkIfEnemyInRange)
		{
			_checkIfEnemyInRange = checkIfEnemyInRange;
			_deviceId = ship.Systems.All.FindFirst<DetonatorDevice>(out var detonator);

			if (detonator != null)
				_detonatorActivationTime = detonator.ActivationTime;
		}

		public NodeState Evaluate(Context context)
		{
			if (_deviceId < 0) 
				return NodeState.Failure;

			if (_checkIfEnemyInRange)
			{
				var ship = context.Ship;
				var enemy = context.TargetShip;

				if (enemy == null) 
					return NodeState.Failure;

				var shipPosition = ship.Body.Position + ship.Body.Velocity * _detonatorActivationTime;
				var enemyPosition = enemy.Body.Position + enemy.Body.Velocity * _detonatorActivationTime;

				if (Vector2.Distance(shipPosition, enemyPosition) > ship.Body.Scale/2 + enemy.Body.Scale/2)
					return NodeState.Failure;
			}

			context.Controls.ActivateSystem(_deviceId);
			return NodeState.Success;
		}
	}
}
