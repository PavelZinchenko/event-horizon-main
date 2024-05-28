using System.ComponentModel;
using Constructor;
using Constructor.Ships;
using DataModel.Technology;
using GameDatabase;
using GameDatabase.DataModel;
using GameModel;
using GameServices.Player;
using GameServices.Random;
using Session;
using Zenject;

namespace Economy.ItemType
{
    public class ItemTypeFactory
    {
        [Inject] private readonly DiContainer _container;
        [Inject] private readonly ISessionData _session;
        [Inject] private readonly RegionMap _regionMap;
        [Inject] private readonly IRandom _random;
        [Inject] private readonly PlayerResources _playerResources;
        [Inject] private readonly IDatabase _database;

        public IItemType CreateEmptyShipItem(IShipModel ship)
        {
            return _container.Instantiate<EmptyShipItem>(new object[] { ship });
        }

        public IItemType CreatePlayerShipItem(IShip ship)
        {
            return _container.Instantiate<PlayerShipItem>(new object[] { ship });
        }

        public IItemType CreateMarketShipItem(IShip ship, bool premium = false)
        {
            return _container.Instantiate<MarketShipItem>(new object[] { ship, premium });
        }

        public IItemType CreateQuestShipItem(IShip ship)
        {
            return _container.Instantiate<QuestShipItem>(new object[] { ship });
        }

        public IItemType CreateQuestShipItem(ShipBuild build)
        {
            return _container.Instantiate<QuestShipItem>(new object[] { new CommonShip(build, _database) });;
        }

        public IItemType CreateDamagedShipItem(ShipBuild build, int seed)
        {
            return _container.Instantiate<DamagedShipItem>(new object[] { build, seed });
        }

        public IItemType CreateCurrencyItem(Currency currency)
        {
            switch (currency)
            {
                case Currency.Credits:
                    return _container.Instantiate<MoneyItem>();
                case Currency.Stars:
                    return _container.Instantiate<StarsItem>();
                case Currency.Tokens:
                    return _container.Instantiate<TokensItem>();
                case Currency.Snowflakes:
                    return _container.Instantiate<SnowflakesItem>();
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public IItemType CreateArtifactItem(QuestItem questItem)
        {
            return _container.Instantiate<ArtifactItem>(new object[] { questItem });
        }

        public IItemType CreateFuelItem()
        {
            return _container.Instantiate<FuelItem>();
        }

        public IItemType CreateBlueprintItem(ITechnology technology)
        {
            return _container.Instantiate<BlueprintItem>(new object[] { technology });
        }

        public IItemType CreateStarMapItem(int starId)
        {
            return _container.Instantiate<StarMapItem>(new object[] { starId });
        }

        public IItemType TryCreateFactionMapItem(int starId)
        {
            int x, y;
            StarLayout.IdToPosition(starId, out x, out y);
            x += _random.RandomInt(starId + 10, -25, 25);
            y += _random.RandomInt(starId + 20, -25, 25);
            int id = StarLayout.PositionToId(x, y);
            var region = _regionMap.GetStarRegion(id);
            if (region.Id == Region.UnoccupiedRegionId || region.Id == Region.PlayerHomeRegionId)
                return null;
            if (_session.StarMap.IsVisited(starId))
                return null;

            return _container.Instantiate<FactionMapItem>(new object[] {starId});
        }

        //public IItemType CreateComponentItem(string component, bool premium = false)
        //{
        //    return CreateComponentItem(ComponentInfo.FormString(_database, component), premium);
        //}

        public IItemType CreateComponentItem(ComponentInfo component, bool premium = false)
        {
            return _container.Instantiate<ComponentItem>(new object[] { component, premium });
        }

        public IItemType CreateSatelliteItem(Satellite satellite, bool premium = false)
        {
            return _container.Instantiate<SatelliteItem>(new object[] { satellite, premium });
        }

        public IItemType CreateResearchItem(Faction faction)
        {
            return _container.Instantiate<ResearchItem>(new object[] { faction });
        }
        public IItemType CreatePurchasedStarsItem()
        {
            return _container.Instantiate<PurchasedStarsItem>();
        }

        public IItemType CreateSupporterPackItem()
        {
            return _container.Instantiate<SupporterPackItem>();
        }

        public IItemType CreateRewardedAdItem()
        {
            return _container.Instantiate<RewardedAdItem>();
        }

        public IItemType CreateXmasBoxItem(int seed = -1)
        {
			if (seed <= 0)
				seed = (int)_playerResources.Money + (int)_playerResources.Stars + _random.Seed;

			return _container.Instantiate<XmaxBoxItem>(new object[] { seed });
        }
    }
}
