using Combat.Component.Ship;
using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class MothershipLowShieldNode : INode
	{
		private readonly float _minValue;
        private IShip _mothership;
        private bool _hasShieldDevice;

		public MothershipLowShieldNode(float minValue)
		{
			_minValue = minValue;
		}

		public NodeState Evaluate(Context context)
		{
			var mothership = context.Mothership;
			if (!mothership.IsActive())
				return NodeState.Failure;

            UpdateMothership(mothership);
			var shield = _hasShieldDevice ? mothership.Stats.Energy.Percentage : mothership.Stats.Shield.Percentage;
			return shield < _minValue ? NodeState.Success : NodeState.Failure;
		}

        private void UpdateMothership(IShip mothership)
        {
            if (mothership == _mothership) return;
            _mothership = mothership;
            _hasShieldDevice = mothership.Systems.All.FindFirstDevice(GameDatabase.Enums.DeviceClass.EnergyShield) >= 0;
        }
    }
}
