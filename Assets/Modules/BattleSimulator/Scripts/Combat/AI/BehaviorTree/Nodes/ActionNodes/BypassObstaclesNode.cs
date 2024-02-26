using Combat.Ai.BehaviorTree.Utils;
using Combat.Component.Unit;
using UnityEngine;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class BypassObstaclesNode : INode
	{
        private const float _cooldown = 0.5f;
        private float _elapsedTime = _cooldown;
        private IUnit _obstacle;

		public NodeState Evaluate(Context context)
		{
            var ship = context.Ship;
            _elapsedTime += context.DeltaTime;

            if (_obstacle == null || _elapsedTime > _cooldown)
            {
                var obstacle = context.Obstacle;
                if (obstacle == null || !ThreatAnalyzer.IsObstacle(ship, obstacle))
                    return NodeState.Failure;

                _elapsedTime = 0;
                _obstacle = obstacle;
            }

            var angle = RotationHelpers.Angle(_obstacle.Body.Position - ship.Body.Position);
            var shipRotation = ship.Body.Rotation;
            var delta = Mathf.DeltaAngle(shipRotation, angle);

            angle = delta > 0 ? angle - 90 : angle + 90;
            context.Controls.Course = angle;
    	    context.Controls.Thrust = Mathf.Abs(Mathf.DeltaAngle(shipRotation, angle)) < 30 ? 1 : 0;
			return NodeState.Running;
		}
    }
}
