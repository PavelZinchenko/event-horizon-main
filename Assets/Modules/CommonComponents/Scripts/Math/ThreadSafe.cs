using System.Threading;

public static class ThreadSafe
{
	public static void Add(ref float target, float value)
	{
		float newCurrentValue = target;
		while (true)
		{
			var currentValue = newCurrentValue;
			var newValue = currentValue + value;
			newCurrentValue = Interlocked.CompareExchange(ref target, newValue, currentValue);

			if (newCurrentValue == currentValue)
				break;
		}
	}

	public static float AddClamp(ref float target, float value, float min, float max)
	{
	    float addedValue;
		while (true)
		{
		    var newCurrentValue = target;
			var currentValue = newCurrentValue;
			var newValue = currentValue + value;

		    if (newValue < min) newValue = min;
            if (newValue > max) newValue = max;

		    addedValue = newValue - currentValue;
            newCurrentValue = Interlocked.CompareExchange(ref target, newValue, currentValue);

			if (newCurrentValue == currentValue)
				break;
		}

	    return addedValue;
	}

	public delegate bool Function<T>(ref T value);

	public static bool ChangeValue(ref float target, Function<float> function)
	{
		float newCurrentValue = target;
		while (true)
		{
			var currentValue = newCurrentValue;
			var newValue = currentValue;

			if (!function(ref newValue))
				return false;

			newCurrentValue = Interlocked.CompareExchange(ref target, newValue, currentValue);

			if (newCurrentValue == currentValue)
				return true;
		}
	}
}
