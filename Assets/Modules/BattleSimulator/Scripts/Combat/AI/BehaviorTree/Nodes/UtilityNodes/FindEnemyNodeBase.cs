using Combat.Component.Ship;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public abstract class FindEnemyNodeBase : INode
	{
		private const float _maxCooldown = 24*60*60;
		private readonly float _findEnemyCooldown;
		private readonly float _changeEnemyCooldown;

		private IShip _lastEnemy;
		private float _lastEnemyUpdateTime = -_maxCooldown;

		public FindEnemyNodeBase(float findEnemyCooldown, float changeEnemyCooldown)
		{
			_findEnemyCooldown = findEnemyCooldown > 0 ? findEnemyCooldown : _maxCooldown;
			_changeEnemyCooldown = changeEnemyCooldown > 0 ? changeEnemyCooldown : _maxCooldown;
		}

		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip != null && IsValidEnemy(context.TargetShip, context))
			{
				_lastEnemy = context.TargetShip;
				_lastEnemyUpdateTime = context.LastTargetUpdateTime;
			}
			else if (_lastEnemy != null && IsValidEnemy(_lastEnemy, context))
			{
				context.TargetShip = _lastEnemy;
				context.LastTargetUpdateTime = _lastEnemyUpdateTime;
			}
			else
			{
				context.TargetShip = _lastEnemy = null;
			}

			var elapsedTime = context.Time - _lastEnemyUpdateTime;
			if (_lastEnemy == null && elapsedTime < _findEnemyCooldown)
				return NodeState.Failure;
			if (_lastEnemy != null && elapsedTime < _changeEnemyCooldown)
				return NodeState.Success;

			var newEnemy = FindNewEnemy(context);
			context.LastTargetUpdateTime = _lastEnemyUpdateTime = context.Time;
			
			if (IsValidEnemy(newEnemy, context))
			{
				context.TargetShip = _lastEnemy = newEnemy;
				return NodeState.Success;
			}

			return _lastEnemy != null ? NodeState.Success : NodeState.Failure;
		}

		protected abstract bool IsValidEnemy(IShip enemy, Context context);
		protected abstract IShip FindNewEnemy(Context context);
	}
}
