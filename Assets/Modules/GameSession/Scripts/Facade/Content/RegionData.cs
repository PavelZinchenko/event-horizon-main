using System.Collections.Generic;
using GameDatabase.Model;
using GameDatabase.DataModel;
using Session.Model;

namespace Session.Content
{
	public interface IRegionData
	{
		int ExploredRegionCount { get; }
		IEnumerable<int> Regions { get; }
		bool TryGetRegionFactionId(int regionId, out ItemId<Faction> faction);
		void SetRegionFactionId(int regionId, ItemId<Faction> factionId);
		int GetDefeatedFleetCount(int regionId);
		bool IsRegionCaptured(int regionId);
		IEnumerable<ItemId<Faction>> GetCapturedFactions();
		void SetRegionCaptured(int regionId, bool caputured);
		IEnumerable<int> DiscoveredRegions { get; }
		void SetDefeatedFleetCount(int regionId, int count);
		void Reset();
	}

	public class RegionData : IRegionData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public RegionData(SaveGameData sessionData) => _data = sessionData;

		public int ExploredRegionCount => _data.Regions.Factions.Count;
		public IEnumerable<int> Regions => _data.Regions.Factions.Keys;

		public bool TryGetRegionFactionId(int regionId, out ItemId<Faction> faction)
		{
			if (_data.Regions.Factions.TryGetValue(regionId, out var factionId))
			{
				faction = ItemId<Faction>.Create(factionId);
				return true;
			}

			faction = ItemId<Faction>.Empty;
			return false;
		}

		public void SetRegionFactionId(int regionId, ItemId<Faction> factionId) => _data.Regions.Factions.SetValue(regionId, factionId.Value);
		public int GetDefeatedFleetCount(int regionId) => _data.Regions.DefeatedFleetCount.TryGetValue(regionId, out var value) && value > 0 ? value : 0;
		public bool IsRegionCaptured(int regionId) => _data.Regions.DefeatedFleetCount.TryGetValue(regionId, out var value) && value < 0;

		public void Reset()
		{
			_data.Regions.Factions.Clear();
			_data.Regions.DefeatedFleetCount.Clear();
		}

		public IEnumerable<ItemId<Faction>> GetCapturedFactions()
		{
			foreach (var item in _data.Regions.DefeatedFleetCount)
			{
				if (item.Value >= 0) continue;
				if (!_data.Regions.Factions.TryGetValue(item.Key, out var faction)) continue;
				yield return new ItemId<Faction>(faction);
			}
		}

		public void SetRegionCaptured(int regionId, bool caputured) => _data.Regions.DefeatedFleetCount.SetValue(regionId, caputured ? -1 : 0);
		public IEnumerable<int> DiscoveredRegions => _data.Regions.DefeatedFleetCount.Keys;

		public void SetDefeatedFleetCount(int regionId, int count)
		{
			if (_data.Regions.DefeatedFleetCount.TryGetValue(regionId, out var value) && value < 0) return;
			_data.Regions.DefeatedFleetCount.SetValue(regionId, count > 0 ? count : 0);
		}
	}
}
