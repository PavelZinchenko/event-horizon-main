using Session.Model;
using Session.Extensions;

namespace Session.Content
{
	public interface IBossData
	{
		long CompletedTime(int id);
		int DefeatCount(int id);
		void SetCompleted(int id);
	}

	public class BossData : IBossData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public BossData(SaveGameData sessionData) => _data = sessionData;

		public long CompletedTime(int id) => _data.Bosses.Bosses.TryGetValue(id, out var info) ? 
			_data.GameTimeToTicks(info.LastDefeatTime, TimeUnits.Hours) : 0;

		public int DefeatCount(int id) => _data.Bosses.Bosses.TryGetValue(id, out var info) ? info.DefeatCount : 0;

		public void SetCompleted(int id)
		{
			var time = _data.CurrentGameTime(TimeUnits.Hours);

			if (!_data.Bosses.Bosses.TryGetValue(id, out var info))
				_data.Bosses.Bosses.Add(id, new BossInfo(1, time));
			else
				_data.Bosses.Bosses.SetValue(id, new BossInfo(info.DefeatCount + 1, time));
		}
	}
}
