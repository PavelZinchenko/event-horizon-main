using GameDatabase.Enums;
using Combat.Ai.BehaviorTree.Nodes;

namespace Combat.Ai.BehaviorTree.Utils
{
	public static class NodeExecutionModeExtensions
	{
		public static bool IsConditionMet(this NodeExecutionMode mode, NodeState result)
		{
			switch (mode)
			{
				case NodeExecutionMode.UntilSucceeds:
					return result == NodeState.Success;
				case NodeExecutionMode.UntilFails:
					return result == NodeState.Failure;
				case NodeExecutionMode.UntilFinishes:
					return result == NodeState.Success || result == NodeState.Failure;
				case NodeExecutionMode.OneTime:
					return true;
				case NodeExecutionMode.Infinitely:
				default:
					return false;
			}
		}
	}
}
