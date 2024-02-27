using Combat.Ai.BehaviorTree.Utils;
using Combat.Component.Unit;
using UnityEngine;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class BypassObstaclesNode : INode
	{
        private const float _cooldown = 0.5f;
        private const float _initialTurnAngle = 60f;
        private float _elapsedTime = _cooldown;
        private IUnit _obstacle;
        private float _turnAngle = _initialTurnAngle;

		public NodeState Evaluate(Context context)
		{
            var ship = context.Ship;
            _elapsedTime += context.DeltaTime;

            var obstacle = ship.Collider.LastCollision;
            if (ThreatAnalyzer.IsObstacle(ship, obstacle))
            {
                if (_obstacle != obstacle)
                    _turnAngle = _initialTurnAngle;

                _obstacle = obstacle;
                _elapsedTime = 0;
            }

            if (_elapsedTime > _cooldown)
                _obstacle = null;

            if (_obstacle == null)
                return NodeState.Failure;

            if (ship.Collider.ActiveCollision == _obstacle)
                _turnAngle = Mathf.Min(180, _turnAngle + _initialTurnAngle * context.DeltaTime);

            var angle = RotationHelpers.Angle(_obstacle.Body.Position - ship.Body.Position);
            var shipRotation = ship.Body.Rotation;
            var delta = Mathf.DeltaAngle(shipRotation, angle);

            angle = delta > 0 ? angle - _turnAngle : angle + _turnAngle;
            context.Controls.Course = angle;
    	    context.Controls.Thrust = Mathf.Abs(Mathf.DeltaAngle(shipRotation, angle)) < _turnAngle/3 ? 1 : 0;
			return NodeState.Running;
		}
    }
}
