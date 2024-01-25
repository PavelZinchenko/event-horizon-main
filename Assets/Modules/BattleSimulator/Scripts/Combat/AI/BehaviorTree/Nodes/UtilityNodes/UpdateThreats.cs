using Combat.Component.Ship;
using Combat.Component.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class UpdateThreats : INode, IThreatAnalyzer
	{
		private readonly float _cooldown;

		public UpdateThreats(float cooldown)
		{
			_cooldown = cooldown;
		}

		public NodeState Evaluate(Context context)
		{
			context.UpdateThreatList(this, _cooldown);
			return context.Threats.Count > 0 ? NodeState.Success : NodeState.Failure;
		}

		public bool IsThreat(IShip ship, IUnit unit) => true;
	}
}
