namespace Combat.Ai.BehaviorTree.Nodes
{
	public class WaitNode : INode
	{
		private readonly float _timeInterval;
		private readonly bool _resetIfInterrupted;
		private float _elapsedTime;
		private int _lastFrameId;

		public WaitNode(float timeInterval, bool resetIfInterrupted)
		{
			_timeInterval = timeInterval;
			_resetIfInterrupted = resetIfInterrupted;
		}

		public NodeState Evaluate(Context context)
		{
			if (_resetIfInterrupted && context.FrameId - _lastFrameId != 1)
				_elapsedTime = 0;

			_lastFrameId = context.FrameId;
			_elapsedTime += context.DeltaTime;

			return _elapsedTime >= _timeInterval ? NodeState.Success : NodeState.Running;
		}
	}
}
