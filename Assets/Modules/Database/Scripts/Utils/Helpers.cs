using GameDatabase.Enums;

namespace GameDatabase.Utils
{
	public static class Helpers // TODO: move formulas to database
	{
		public static int StarLevelToShipSize(int level)
		{
			return 25 + 2*level;
		}

        public static DifficultyClass StarLevelToMaxDifficulty(int level) { return (DifficultyClass)(level/25); }
		public static DifficultyClass StarLevelToMinDifficulty(int level) { return level < 50 ? DifficultyClass.Default : DifficultyClass.Class1; }
	}
}