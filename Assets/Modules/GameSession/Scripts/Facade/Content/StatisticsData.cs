using System.Linq;
using System.Collections.Generic;
using GameDatabase.DataModel;
using GameDatabase.Model;
using Session.Model;

namespace Session.Content
{
	public interface IStatisticsData
	{
		void UnlockShip(ItemId<Ship> id);
		IEnumerable<ItemId<Ship>> UnlockedShips { get; }
		int DefeatedEnemies { get; set; }
	}

	public class StatisticsData : IStatisticsData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public StatisticsData(SaveGameData sessionData) => _data = sessionData;

		public void UnlockShip(ItemId<Ship> id) => _data.Statistics.UnlockedShips.Add(id.Value);
		public IEnumerable<ItemId<Ship>> UnlockedShips => _data.Statistics.UnlockedShips.Select(id => new ItemId<Ship>(id));

		public int DefeatedEnemies
		{
			get => _data.Statistics.DefeatedEnemies;
			set => _data.Statistics.DefeatedEnemies = value;
		}
	}
}
