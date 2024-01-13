using Session.Model;

namespace Session.Content
{
	public interface ICommonObjectData
	{
		long GetUseTime(int id);
		void SetUseTime(int id, long time);
		int GetIntValue(int id);
		void SetIntValue(int id, int value);
	}

	public class CommonObjectData : ICommonObjectData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public CommonObjectData(SaveGameData sessionData) => _data = sessionData;

		public long GetUseTime(int id) => _data.Common.LongValues.TryGetValue(id, out var time) ? time : 0;
		public void SetUseTime(int id, long time) => _data.Common.LongValues.SetValue(id, time);
		public int GetIntValue(int id) => _data.Common.IntValues.TryGetValue(id, out var value) ? value : 0;
		public void SetIntValue(int id, int value) => _data.Common.IntValues.SetValue(id, value);
	}
}
