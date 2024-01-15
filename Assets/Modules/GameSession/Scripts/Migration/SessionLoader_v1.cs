using Session.Extensions;

namespace Session
{
	public partial class SessionLoader
	{
		partial void Upgrage_v1_0_to_v2_0(v1.SaveGameData oldData, Model.SaveGameData newData)
		{
			GameDiagnostics.Trace.LogWarning("Upgrading savegame from v1.0 to v2.0");

			foreach(var item in oldData.Bosses.Bosses.Items)
			{
				var defeatTime = newData.TicksToGameTime(item.Value.LastDefeatTime, TimeUnits.Hours);
				newData.Bosses.Bosses.Add(item.Key, new Model.BossInfo(item.Value.DefeatCount, defeatTime));
			}

			var starData = oldData.StarMap.StarData;
			foreach (var item in starData.Items)
			{
				newData.StarMap.DiscoveredStars.Add((uint)item.Key);

				if (item.Value == 1 || item.Value == 2)
					newData.StarMap.SecuredStars.Add((uint)item.Key);
				if (item.Value == 2 || item.Value == 3)
					newData.StarMap.EnemiesOnStars.Add((uint)item.Key);
			}
		}
	}
}
