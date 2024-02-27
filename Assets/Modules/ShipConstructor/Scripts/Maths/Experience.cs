using System;
using Constructor.Ships;

namespace Maths
{
	public struct Experience
	{
		public Experience(long value)
		{
		    if (value < 0)
		        value = 0;

			_value = value;
		}

		public int Level
		{
			get
			{
				var exp = Value;
				var level = (int)Math.Pow(exp/100, 1.0/3.0);

				if (LevelToExp(level+1) <= exp)
					return level+1;

				return level;
			}
		}

		public float PowerMultiplier { get { return LevelToPowerMultiplier(Level); } }

		public long ExpFromLastLevel { get { return Value - LevelToExp(Level); } }

	    public long NextLevelCost
		{
			get
			{
				var level = Level;
				return LevelToExp(level+1) - LevelToExp(level);
			}
		}

		public static Experience FromLevel(int level)
		{
			return new Experience(LevelToExp(level));
		}

		public static float LevelToPowerMultiplier(int level)
		{
			return (float)Math.Pow(11, (0.01*level));
		}

        public static long TotalExpForShip(IShip ship)
        {
            return (1L + (long)Math.Pow(ship.Experience.Level + 1, 1.2)) * (long)Math.Pow(ship.Model.Layout.CellCount, 1.2) * (1 + (int)ship.ExtraThreatLevel);
        }

        public static implicit operator Experience(long value)
		{
			return new Experience(value);
		}
		
		public static implicit operator long(Experience data)
		{
			return data.Value;
		}
		
		public override string ToString ()
		{
			return Value.ToString();
		}

		private long Value { get { return _value; } }

		public static long LevelToExp(int level) { return 100L*level*level*level; }

		private readonly ObscuredLong _value;
    }
}
