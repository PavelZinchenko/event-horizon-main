using Session.Model;

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

		public long CompletedTime(int id) => _data.Bosses.Bosses.TryGetValue(id, out var info) ? info.LastDefeatTime : 0;
		public int DefeatCount(int id) => _data.Bosses.Bosses.TryGetValue(id, out var info) ? info.DefeatCount : 0;

		public void SetCompleted(int id)
		{
			var time = System.DateTime.UtcNow.Ticks;
			if (!_data.Bosses.Bosses.TryGetValue(id, out var info))
				_data.Bosses.Bosses.Add(id, new BossInfo(1, time));
			else
				_data.Bosses.Bosses.SetValue(id, new BossInfo(info.DefeatCount + 1, time));
		}
	}
}
