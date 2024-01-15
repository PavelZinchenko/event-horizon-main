using Session.Model;

namespace Session.Content
{
	public interface IStarMapData
	{
		public enum Occupant : uint
		{
			Unknown = 0, // !Secured && !Enemy
			Empty,       // Secured && !Enemy
			Passive,     // Secured && Enemy
			Agressive,   // !Secured && Enemy
		}

		void Reset();
		int PlayerPosition { get; set; }
		int LastPlayerPosition { get; }
		float MapScaleFactor { get; set; }
		float StarScaleFactor { get; set; }
		bool IsVisited(int starId);
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
			_data.StarMap.SecuredStars.Clear();
			_data.StarMap.EnemiesOnStars.Clear();
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
		public bool IsVisited(int starId) => _data.StarMap.DiscoveredStars.Get((uint)starId);
		public int FurthestVisitedStar => _data.StarMap.DiscoveredStars.LastIndex;
		
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

		public void SetVisited(int starId) => _data.StarMap.DiscoveredStars.Add((uint)starId);

		public IStarMapData.Occupant GetEnemy(int starId)
		{
			var secured = _data.StarMap.SecuredStars.Get((uint)starId);
			var enemy = _data.StarMap.EnemiesOnStars.Get((uint)starId);

			if (secured)
				return enemy ? IStarMapData.Occupant.Passive : IStarMapData.Occupant.Empty;
			else
				return enemy ? IStarMapData.Occupant.Agressive : IStarMapData.Occupant.Unknown;
		}

		public void SetEnemy(int starId, IStarMapData.Occupant enemy)
		{
			var oldValue = GetEnemy(starId);
			if (oldValue == enemy) return;

			_data.StarMap.SecuredStars.Set((uint)starId, enemy == IStarMapData.Occupant.Empty || enemy == IStarMapData.Occupant.Passive);
			_data.StarMap.EnemiesOnStars.Set((uint)starId, enemy == IStarMapData.Occupant.Passive || enemy == IStarMapData.Occupant.Agressive);

			if (oldValue == IStarMapData.Occupant.Agressive || oldValue == IStarMapData.Occupant.Unknown)
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
