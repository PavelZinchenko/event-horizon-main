using UnityEngine;

namespace Combat.Unit.HitPoints
{
	public interface IResourcePoints
	{
		float Value { get; }
		float MaxValue { get; }
		float Percentage { get; }
		float RechargeRate { get; }

		void Update(float elapsedTime);

        bool Exists { get; }

		bool TryGet(float value);
		float Get(float value);
	}

    public class EmptyResources : IResourcePoints
    {
        public float Value => 0.0f;
        public float MaxValue => 1.0f;
        public float Percentage => 0.0f;
        public float RechargeRate => 0.0f;
        public void Update(float elapsedTime) { }
        public bool TryGet(float value) => true;
        public float Get(float value) => 0.0f;
        public bool Exists => false;
    }

    public class UnlimitedEnergy : IResourcePoints
	{
		public float Value => 100f;
		public float MaxValue => 100f;
		public float Percentage => 1.0f;
		public float RechargeRate => 0.0f;
		public void Update(float elapsedTime) {}
		public bool TryGet(float value) => true;
		public float Get(float value) => value;
        public bool Exists => true;
    }

	public class Energy : IResourcePoints
	{
		public Energy(float max, float rechargeRate, float rechargeDelay)
		{
			_rechargeRate = rechargeRate;
			MaxValue = max;
			_rechargeDelay = rechargeDelay;
			_value = 1.0f;
			_delay = 0.0f;
		}
		
		public float Value => MaxValue*_value;
		public float Percentage => _value;
		public float MaxValue { get; private set; }
		public float RechargeRate => _rechargeRate;

		public void Update(float elapsedTime)
		{
			if (_delay > elapsedTime)
			{
				_delay -= elapsedTime;
				return;
			}

            if (MaxValue > 0)
			    ThreadSafe.AddClamp(ref _value, (elapsedTime - _delay)*_rechargeRate/MaxValue, 0, 1);
			_delay = 0;
		}
		
		public bool TryGet(float how)
		{
			ThreadSafe.Function<float> func = (ref float value) =>
			{
				if (value > 0 && value*MaxValue >= how && MaxValue > 0)
				{
					value -= how/MaxValue;
					return true;
				}
				return false;
			};

			if (ThreadSafe.ChangeValue(ref _value, func))
			{
				if (how > 0 && _rechargeRate > 0)
					_delay = _rechargeDelay;
				return true;
			}

			return false;
		}

		public float Get(float how)
		{
			if (how > 0 && _rechargeRate > 0)
				_delay = _rechargeDelay;

            return MaxValue > 0 ? -ThreadSafe.AddClamp(ref _value, -how / MaxValue, 0, 1)*MaxValue : 0f;
        }

        public readonly static Energy Zero = new Energy(0,1,1);

	    public bool Exists => MaxValue > 0;

		private float _value;
		private float _delay;
		
		private readonly float _rechargeRate;
		private readonly float _rechargeDelay;
	}

	public class HitPoints : IResourcePoints
	{
		public HitPoints(float max)
		{
			_value = -1.0f;
			_max = Mathf.Max(max, 1);
		}
		
		public float Value => -_value*_max;
		public float MaxValue => _max;
		public float Percentage => -_value;
		public float RechargeRate => 0;

		public void Update(float elapsedTime) {}

		public float Get(float how)
		{
			return ThreadSafe.AddClamp(ref _value, how/_max, -1, 0) * _max;
		}
		
		public bool TryGet(float how)
		{
			throw new System.NotImplementedException();
		}

        public bool Exists { get { return true; } }

        private float _value;
		private readonly float _max;
	}
}
