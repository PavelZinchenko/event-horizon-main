using Combat.Component.Ship;
using Combat.Component.Unit;

namespace Combat.Ai
{
	public interface IThreatAnalyzer
	{
		bool IsThreat(IShip ship, IUnit unit);
	}

	public interface IStrategy : IThreatAnalyzer
	{
		void Apply(Context context, ShipControls controls);
	}
}
