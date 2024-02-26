using System.Collections.Generic;
using Combat.Ai.BehaviorTree.Utils;
using Combat.Component.Ship;
using Combat.Component.Unit;

namespace Combat.Ai
{
	public abstract class StrategyBase : IStrategy
	{
        public virtual bool IsThreat(IShip ship, IUnit unit) => ThreatAnalyzer.IsThreat(ship, unit);

        public void Apply(Context context, ShipControls controls)
		{
			foreach (var policy in _policyList)
			    policy.Perform(context, controls);
		}

		public void AddPolicy(ICondition condition, IAction action, IAction oppositeAction = null)
		{
			_policyList.Add(new Policy(condition, action, oppositeAction));
		}

		private readonly List<Policy> _policyList = new List<Policy>();
	}
}
