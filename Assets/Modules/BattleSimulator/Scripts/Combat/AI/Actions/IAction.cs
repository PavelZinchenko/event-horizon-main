using Combat.Component.Ship;
using Combat.Component.Controls;

namespace Combat.Ai
{
	public class ShipControls
	{
		public int SystemCount { get; }
		public bool RotationLocked => _courseLocked;
		public bool MovementLocked => _thrustLocked;

		public void Apply(IShip ship)
        {
			ship.Controls.Throttle = _thrust;
			ship.Controls.Course = _course;
			ship.Controls.Systems.Assign(_systems);
			_courseLocked = false;
			_thrustLocked = false;
			_systems.Clear();
			_systemsMask.Clear();
		}

		public float Course
		{
			set
			{
				if (!_courseLocked)
				{
					_course = value;
					_courseLocked = true;
				}
			} 
		}

		public float Thrust
		{
			set
			{
				if (!_thrustLocked)
				{
					_thrust = value;
					_thrustLocked = true;
				}
			}
            get
            {
				return _thrust;
			}
		}

		public bool IsSystemLocked(int id) => _systemsMask[id];

	    public void ActivateSystem(int index, bool active = true)
	    {
			if (_systemsMask[index]) return;
			_systems[index] = active;
			_systemsMask[index] = true;
	    }

		private bool _thrustLocked;
		private float _thrust;
		private bool _courseLocked;
		private float? _course;
		private SystemsState _systems = SystemsState.Create();
		private SystemsState _systemsMask = SystemsState.Create();
    }

    public struct Context
	{
	    public Context(IShip ship, IShip target, TargetList secondaryTargets, ThreatList threats, float currentTime)
	    {
	        Ship = ship;
	        Enemy = target;
	        Threats = threats;
	        CurrentTime = currentTime;
	        UnusedEnergy = new FloatReference { Value = Ship.Stats.Energy.Value };
	        Targets = secondaryTargets;
	    }

		public float CurrentTime;
		public IShip Ship;
        public ThreatList Threats;
        public TargetList Targets;
        public IShip Enemy;
	    public FloatReference UnusedEnergy;

        public class FloatReference
        {
            public float Value { get; set; }
        }
	}

	public interface IAction
	{
		void Perform(Context context, ShipControls controls);
	}
}
