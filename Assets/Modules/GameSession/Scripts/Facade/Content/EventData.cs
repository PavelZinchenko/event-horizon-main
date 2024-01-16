using Session.Model;
using Session.Extensions;

namespace Session.Content
{
	public interface IEventData
	{
		long CompletedTime(int starId);
		void Complete(int starId);
	}

	public class EventData : IEventData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public EventData(SaveGameData sessionData) => _data = sessionData;

		public long CompletedTime(int starId) => _data.Events.CompletedTime.TryGetValue(starId, out var value) ? _data.GameTimeToTicks(value, TimeUnits.Hours) : 0;
		public void Complete(int starId) => _data.Events.CompletedTime.SetValue(starId, _data.CurrentGameTime(TimeUnits.Hours));
	}
}
