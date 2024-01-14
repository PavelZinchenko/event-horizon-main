using System.Linq;
using Session.Model;

namespace Session.Content
{
	public interface IStarMapData
	{
		public enum Occupant : uint
		{
			Unknown = 0,
			Empty,
			Passive,
			Agressive,
		}

		void Reset();
		int PlayerPosition { get; set; }
		int LastPlayerPosition { get; }
		float MapScaleFactor { get; set; }
		float StarScaleFactor { get; set; }
		bool IsVisited(int starId);
		int VisitedStarsCount { get; }
		int FurthestVisitedStar { get; }
		uint GetPlanetData(int starId, int planetId);
		void SetPlanetData(int starId, int planetId, uint value);
		void SetVisited(int starId);
		Occupant GetEnemy(int starId);
		void SetEnemy(int starId, Occupant enemy);
		string GetBookmark(int starId);
		bool HasBookmark(int starId);
		void SetBookmark(int starId, string value);
	}

	public class StarMapData : IStarMapData, ISessionDataContent
	{
		private readonly SaveGameData _data;
		private readonly PlayerPositionChangedSignal.Trigger _playerPositionChangedTrigger;
		private readonly NewStarSecuredSignal.Trigger _newStarSecuredTrigger;

		public StarMapData(
			SaveGameData sessionData,
			PlayerPositionChangedSignal.Trigger playerPositionChangedTrigger,
			NewStarSecuredSignal.Trigger newStarExploredTrigger)
		{
			_data = sessionData;
			_playerPositionChangedTrigger = playerPositionChangedTrigger;
			_newStarSecuredTrigger = newStarExploredTrigger;
		}

		public void Reset()
		{
			_data.StarMap.PlayerPosition = 0;
			LastPlayerPosition = 0;
			_data.StarMap.StarData.Clear();
			_data.StarMap.Bookmarks.Clear();
			_data.StarMap.PlanetData.Clear();
		}

		public int PlayerPosition 
		{
			get => _data.StarMap.PlayerPosition; 
			set
			{
				if (value == _data.StarMap.PlayerPosition)
					return;

				LastPlayerPosition = PlayerPosition;
				_data.StarMap.PlayerPosition = value;
				SetVisited(value);

				_playerPositionChangedTrigger.Fire(value);
			}
		}

		public int LastPlayerPosition { get; private set; }
		public float MapScaleFactor { get => _data.StarMap.MapModeZoom; set => _data.StarMap.MapModeZoom = value; }
		public float StarScaleFactor { get => _data.StarMap.StarModeZoom; set => _data.StarMap.StarModeZoom = value; }
		public bool IsVisited(int starId) => _data.StarMap.StarData.ContainsKey(starId);
		public int VisitedStarsCount => _data.StarMap.StarData.Count;
		public int FurthestVisitedStar => (int)_data.StarMap.StarData.Keys.Max();
		
		public uint GetPlanetData(int starId, int planetId)
		{
			var key = (((long)starId) << 32) + planetId;
			return _data.StarMap.PlanetData.TryGetValue(key, out var value) ? (uint)value : 0;
		}

		public void SetPlanetData(int starId, int planetId, uint value)
		{
			var key = (((long)starId) << 32) + planetId;
			_data.StarMap.PlanetData.SetValue(key, (int)value);
		}

		public void SetVisited(int starId) => _data.StarMap.StarData.TryAdd(starId, 0);

		public IStarMapData.Occupant GetEnemy(int starId)
		{
			if (!_data.StarMap.StarData.TryGetValue(starId, out var value))
				return IStarMapData.Occupant.Unknown;

			return (IStarMapData.Occupant)value;
		}

		public void SetEnemy(int starId, IStarMapData.Occupant enemy)
		{
			if (_data.StarMap.StarData.TryGetValue(starId, out var oldValue) && oldValue == (int)enemy)
				return;

			_data.StarMap.StarData.SetValue(starId, (int)enemy);

			if (oldValue == (int)IStarMapData.Occupant.Agressive || oldValue == (int)IStarMapData.Occupant.Unknown)
				if (enemy == IStarMapData.Occupant.Empty || enemy == IStarMapData.Occupant.Passive)
					_newStarSecuredTrigger.Fire(starId);
		}

		public string GetBookmark(int starId) => _data.StarMap.Bookmarks.TryGetValue(starId, out var value) ? value : string.Empty;
		public bool HasBookmark(int starId) => _data.StarMap.Bookmarks.ContainsKey(starId);

		public void SetBookmark(int starId, string value)
		{
			if (string.IsNullOrEmpty(value))
				_data.StarMap.Bookmarks.Remove(starId);
			else
				_data.StarMap.Bookmarks.SetValue(starId, value);
		}
	}
}
