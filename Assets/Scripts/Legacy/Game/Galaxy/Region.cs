using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Extensions;
using Session;
using UnityEngine;

namespace GameModel
{
	public class Region
	{
		public Region(int id, bool isPirateBase, ISessionData session, IDatabase database, BaseCapturedSignal.Trigger baseCapturedTrigger, RegionFleetDefeatedSignal.Trigger regionFleetDefeatedTrigger)
        {
            _session = session;
            _database = database;
            _baseCapturedTrigger = baseCapturedTrigger;
            _regionFleetDefeatedTrigger = regionFleetDefeatedTrigger;

			Id = Mathf.Max(id, 0);
			OwnerId = Id;

			if (Id == 0 || Id == PlayerHomeRegionId || isPirateBase)
			{
				_faction = Faction.Empty;
			    Size = 0;
			}
			else
			{
				Size = RegionLayout.RegionFourthSize*2 - 1;// Game.Data.RandomInt(id, 1, RegionLayout.RegionMaxSize);
			}

			_isCaptured = Id == PlayerHomeRegionId || _session.Regions.IsRegionCaptured(Id);
		}

		public void OnFleetDefeated()
		{
			UnityEngine.Debug.Log("RegionFleetDefeated: " + Id);

			if (Id == UnoccupiedRegionId)
				return;

            BaseDefensePower -= _database.FactionsSettings.DefenseLossPerEnemyDefeated;
            _regionFleetDefeatedTrigger.Fire(this);
		}

		public int Id { get; private set; }
		public int OwnerId { get; private set; }
		public int Size { get; private set; }

		public int PlayerReputation => _session.Quests.GetFactionRelations(HomeStar);

	    public bool IsPirateBase => _faction == Faction.Empty;

        public int Relations
        {
            get
            {
                if (Id == PlayerHomeRegionId || IsCaptured) return 100;
                return _session.Quests.GetFactionRelations(HomeStar);
            }
        }

        public bool IsCaptured 
		{
			get
			{
				return _isCaptured;
			}
			set
			{
				if (Id == UnoccupiedRegionId || _isCaptured == value)
					return;

				_isCaptured = value;
                _session.Regions.SetRegionCaptured(Id, _isCaptured);
                _baseCapturedTrigger.Fire(this);
			}
		}

		public bool IsVisited => IsCaptured || _session.StarMap.IsVisited(HomeStar);

		public int BaseDefensePower 
		{
			get
			{
                if (!_session.Regions.TryGetStarbaseDefensePower(Id, out var power))
                    power = (uint)_database.FactionsSettings.StarbaseInitialDefense(HomeStarLevel);

				return (int)power;
			}
            set
            {
                var min = _database.FactionsSettings.StarbaseMinDefense;
                _session.Regions.SetStarbaseDefensePower(Id, value > min ? (uint)value : (uint)min);
            }
		}

        public int BaseDefendersLevel => HomeStarLevel * BaseDefensePower / 100;
		public int HomeStarLevel => Mathf.RoundToInt(StarLayout.GetStarPosition(HomeStar, _session.Game.Seed).magnitude);

		public Faction Faction
		{
			get
			{
			    if (_faction != null)
			        return _faction;

				if (_session.Regions.TryGetRegionFactionId(Id, out var factionId))
                {
					_faction = _database.GetFaction(factionId);
					return _faction;
				}

			    _faction = _database.FactionList.WithStarbases(HomeStarLevel).RandomElement(new System.Random(HomeStar + _session.Game.Seed));
			    _session.Regions.SetRegionFactionId(Id, _faction.Id);

                return _faction;
			}
            set
            {
				if (value == _faction) return;
                _faction = value;
				_session.Regions.SetRegionFactionId(Id, value.Id);
				_baseCapturedTrigger.Fire(this);
            }
		}

		public int HomeStar
		{
			get
			{
				if (_homeStar < 0)
					_homeStar = RegionLayout.GetRegionHomeStar(Id);

				return _homeStar;
			}
		}

	    private Region()
	    {
	        Id = UnoccupiedRegionId;
	        OwnerId = 0;
	        Size = 0;
			_faction = Faction.Empty;
	        _isCaptured = true;
	    }

        private bool _isCaptured;
		private int _homeStar = -1;
		private Faction _faction;

	    private readonly IDatabase _database;
	    private readonly ISessionData _session;
	    private readonly BaseCapturedSignal.Trigger _baseCapturedTrigger;
	    private readonly RegionFleetDefeatedSignal.Trigger _regionFleetDefeatedTrigger;

        public const int UnoccupiedRegionId = 0;
		public const int PlayerHomeRegionId = 1;

	    public static readonly Region Empty = new Region();
	}
}
