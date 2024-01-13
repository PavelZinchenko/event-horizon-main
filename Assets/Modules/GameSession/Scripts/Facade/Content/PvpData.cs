using Session.Model;

namespace Session.Content
{
	public interface IPvpData
	{
		int FightsFromTimerStart { get; set; }
		long LastFightTime { get; set; }
		long TimerStartTime { get; set; }
	}

	public class PvpData : IPvpData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public PvpData(SaveGameData sessionData) => _data = sessionData;

		public int FightsFromTimerStart
		{
			get => _data.Pvp.ArenaFightsFromTimerStart;
			set => _data.Pvp.ArenaFightsFromTimerStart = value;
		}

		public long LastFightTime
		{
			get => _data.Pvp.ArenaLastFightTime;
			set => _data.Pvp.ArenaLastFightTime = value;
		}

		public long TimerStartTime
		{
			get => _data.Pvp.ArenaTimerStartTime;
			set => _data.Pvp.ArenaTimerStartTime = value;
		}
	}
}
