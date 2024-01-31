using System.Collections.Generic;
using GameDatabase.DataModel;
using GameDatabase.Model;
using Session.Model;

namespace Session.Content
{
	public interface IResearchData
	{
		IEnumerable<int> Technologies { get; }
		void AddTechnology(ItemId<Technology> id);
		void RemoveTechnology(ItemId<Technology> id);
		int GetResearchPoints(Faction faction);
		void SetResearchPoints(Faction faction, int points);
	}

	public class ResearchData : IResearchData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public ResearchData(SaveGameData sessionData) => _data = sessionData;

		public IEnumerable<int> Technologies => _data.Research.Technologies;

		public void AddTechnology(ItemId<Technology> id) => _data.Research.Technologies.Add(id.Value);
		public void RemoveTechnology(ItemId<Technology> id) => _data.Research.Technologies.Remove(id.Value);
		public int GetResearchPoints(Faction faction) => _data.Research.ResearchPoints.TryGetValue(faction.Id.Value, out var points) ? points : 0;
		public void SetResearchPoints(Faction faction, int points) => _data.Research.ResearchPoints.SetValue(faction.Id.Value, points);
	}
}
