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
		bool IsRegionCaptured(int regionId);
		IEnumerable<ItemId<Faction>> GetCapturedFactions();
		void SetRegionCaptured(int regionId, bool caputured);
		IEnumerable<int> DiscoveredRegions { get; }
        void SetStarbaseDefensePower(int regionId, uint power);
        bool TryGetStarbaseDefensePower(int regionId, out uint power);
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
		//public int GetDefeatedFleetCount(int regionId) => _data.Regions.DefeatedFleetCount.TryGetValue(regionId, out var value) && value > 0 ? value : 0;
		public bool IsRegionCaptured(int regionId) => _data.Regions.CapturedBases.Get((uint)regionId);

		public void Reset()
		{
			_data.Regions.Factions.Clear();
			_data.Regions.CapturedBases.Clear();
		}

		public IEnumerable<ItemId<Faction>> GetCapturedFactions()
		{
			foreach (var item in _data.Regions.CapturedBases)
			{
				if (!_data.Regions.Factions.TryGetValue((int)item, out var faction)) continue;
				yield return new ItemId<Faction>(faction);
			}
		}

        public void SetRegionCaptured(int regionId, bool caputured) => _data.Regions.CapturedBases.Set((uint)regionId, caputured);
		public IEnumerable<int> DiscoveredRegions => _data.Regions.MilitaryPower.Keys;

		//public void SetDefeatedFleetCount(int regionId, int count)
		//{
		//	if (_data.Regions.DefeatedFleetCount.TryGetValue(regionId, out var value) && value < 0) return;
		//	_data.Regions.DefeatedFleetCount.SetValue(regionId, count > 0 ? count : 0);
		//}

        public void SetStarbaseDefensePower(int regionId, uint power) => _data.Regions.MilitaryPower.SetValue(regionId, power);
        public bool TryGetStarbaseDefensePower(int regionId, out uint power) => _data.Regions.MilitaryPower.TryGetValue(regionId, out power);
    }
}
