using Session.Model;

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

		public long CompletedTime(int starId) => _data.Events.CompletedTime.TryGetValue(starId, out var value) ? value : 0;
		public void Complete(int starId) => _data.Events.CompletedTime.SetValue(starId, System.DateTime.UtcNow.Ticks);
	}
}
