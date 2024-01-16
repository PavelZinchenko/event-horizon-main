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

	public static class GameTimeExtensions
	{
		public static int CurrentGameTime(this Model.SaveGameData saveGame, TimeUnits timeUnits)
		{
			var time = (DateTime.UtcNow.Ticks - saveGame.Game.StartTime) / TicksPerUnit(timeUnits);
			return time > 0 ? (int)time : 0;
		}

		public static int TicksToGameTime(this Model.SaveGameData saveGame, long ticks, TimeUnits timeUnits)
		{
			var time = (ticks - saveGame.Game.StartTime) / TicksPerUnit(timeUnits);
			return time > 0 ? (int)time : 0;
		}

		public static long GameTimeToTicks(this Model.SaveGameData saveGame, int time, TimeUnits timeUnits)
		{
			return time * TicksPerUnit(timeUnits) + saveGame.Game.StartTime;
		}

		public static int TicksToGameTime(long ticks, long gameStartTime, TimeUnits timeUnits)
		{
			var time = (ticks - gameStartTime) / TicksPerUnit(timeUnits);
			return time > 0 ? (int)time : 0;
		}

		public static long GameTimeToTicks(int time, long gameStartTime, TimeUnits timeUnits)
		{
			return time * TicksPerUnit(timeUnits) + gameStartTime;
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
