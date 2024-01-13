using Session.Model;

namespace Session.Content
{
	public interface IGameData
	{
		int Seed { get; }
		int Counter { get; }
		void Regenerate();
		long GameStartTime { get; set; }
		long TotalPlayTime { get; set; }
		long SupplyShipStartTime { get; }
		void StartSupplyShip();
	}

	public class GameData : IGameData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public GameData(SaveGameData sessionData) => _data = sessionData;

		public int Seed => _data.Game.Seed;
		public int Counter => _data.Game.Counter++;
		public void Regenerate() => _data.Game.Seed = (int)System.DateTime.Now.Ticks;
		public long GameStartTime { get => _data.Game.StartTime; set => _data.Game.StartTime = value; }
		public long TotalPlayTime { get => _data.Game.TotalPlayTime; set => _data.Game.TotalPlayTime = value; }
		public long SupplyShipStartTime { get => _data.Game.SupplyShipStartTime; }
		public void StartSupplyShip() => _data.Game.SupplyShipStartTime = System.DateTime.UtcNow.Ticks;
	}
}
