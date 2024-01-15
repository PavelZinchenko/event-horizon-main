using System;

namespace Session.Extensions
{
	public enum TimeUnits
	{
		Seconds,
		Minutes,
		Hours,
		Days,
	}

	public static class SaveGameDataExtensions
	{
		public static int TicksToGameTime(this Model.SaveGameData saveGame, long ticks, TimeUnits timeUnits)
		{
			var time = (ticks - saveGame.Game.StartTime) / TicksPerUnit(timeUnits);
			return time > 0 ? (int)time : 0;
		}

		public static long GameTimeToTicks(this Model.SaveGameData saveGame, int time, TimeUnits timeUnits)
		{
			return time * TicksPerUnit(timeUnits) + saveGame.Game.StartTime;
		}

		private static long TicksPerUnit(TimeUnits units)
		{
			switch (units)
			{
				case TimeUnits.Seconds: return TimeSpan.TicksPerSecond;
				case TimeUnits.Minutes: return TimeSpan.TicksPerMinute;
				case TimeUnits.Hours: return TimeSpan.TicksPerHour;
				case TimeUnits.Days: return TimeSpan.TicksPerDay;
				default: throw new System.InvalidOperationException();
			}
		}
	}
}
