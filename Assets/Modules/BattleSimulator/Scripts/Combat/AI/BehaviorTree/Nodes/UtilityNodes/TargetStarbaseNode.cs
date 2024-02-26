using Combat.Component.Ship;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class TargetStarbaseNode : INode
	{
        private const float _cooldown = 1.0f;
        private readonly bool _ally;
        private float _lastSearchTime = -_cooldown;
        private IShip _starbase;

        public TargetStarbaseNode(bool ally)
        {
            _ally = ally;
        }

        public NodeState Evaluate(Context context)
		{
            if (IsStarbase(context.Ship, context.TargetShip, _ally))
                return NodeState.Success;

            if (IsStarbase(context.Ship, _starbase, _ally))
            {
                context.TargetShip = _starbase;
                return NodeState.Success;
            }

            if (context.Time - _lastSearchTime < _cooldown) 
                return NodeState.Failure;

            _lastSearchTime = context.Time;
            _starbase = FindStarbase(context.Ship, context.Scene.Ships, _ally);
            if (_starbase == null || _starbase.State != UnitState.Active)
                return NodeState.Failure;

            context.TargetShip = _starbase;
			return NodeState.Success;
		}

        private static IShip FindStarbase(IShip ship, IUnitList<IShip> shipList, bool ally)
        {
            lock (shipList.LockObject)
            {
                foreach (var item in shipList.Items)
                    if (IsStarbase(ship, item, ally))
                        return item;
            }

            return null;
        }

        private static bool IsStarbase(IShip ship, IShip target, bool ally)
        {
            if (target == null) return false;
            if (target.Specification == null) return false;
            if (target.State != UnitState.Active) return false;
            if (target.Specification.Stats.ShipModel.SizeClass != GameDatabase.Enums.SizeClass.Starbase) return false;
            if (ally == target.Type.Side.IsEnemy(ship.Type.Side)) return false;
            return true;
        }
    }
}
