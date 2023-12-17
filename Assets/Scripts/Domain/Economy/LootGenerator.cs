using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Constructor;
using Economy;
using Economy.ItemType;
using Economy.Products;
using GameServices.Random;
using Constructor.Ships;
using Game;
using Game.Exploration;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using GameDatabase.Model;
using GameDatabase.Query;
using GameModel;
using GameServices.Player;
using Zenject;
using Component = GameDatabase.DataModel.Component;

namespace GameServices.Economy
{
    public class LootGenerator
    {
        [Inject] private readonly ItemTypeFactory _factory;
        [Inject] private readonly Research.Research _research;
        [Inject] private readonly IRandom _random;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly HolidayManager _holidayManager;
        [Inject] private readonly PlayerSkills _playerSkills;

        public ItemTypeFactory Factory { get { return _factory; } }

        public IEnumerable<IProduct> GetCommonReward(IEnumerable<IShip> ships, int distance, Faction faction, int seed)
        {
            var random = _random.CreateRandom(seed);

            var scraps = 0;
            var money = 0;

            var moduleLevel = Maths.Distance.ComponentLevel(distance);

            foreach (var ship in ships)
            {
                scraps += ship.Scraps();
                money += ship.Price()/20;

                if (ship.Model.ShipType == ShipType.Flagship)
                {
                    var bossFaction = ship.Model.Faction;
                    foreach (var item in RandomComponents(moduleLevel + 35, random.Next(1, 2), bossFaction, random, false))
                        yield return new Product(item);

                    if (ship.ExtraThreatLevel >= DifficultyClass.Class2)
                    {
                        yield return Price.Premium(1).GetProduct(_factory);
                        foreach (var item in RandomComponents(moduleLevel + 75, random.Next(1, 3), bossFaction, random, false))
                            yield return new Product(item);
                    }
                }
                else
                {
                    foreach (var item in RandomComponents(moduleLevel, random.Next(-10, 2), faction, random, false))
                        yield return new Product(item);
                }
            }

            if (money > 0)
                yield return Price.Common(money).GetProduct(_factory);

            var toxicWaste = random.Next2(scraps/2);
            if (toxicWaste > 0)
                yield return new Product(CreateArtifact(CommodityType.ToxicWaste), toxicWaste);

            scraps -= toxicWaste;
            if (scraps > 0)
                yield return new Product(CreateArtifact(CommodityType.Scraps), scraps);

            foreach (var item in GetHolidayLoot(random))
                yield return item;
        }

        public IEnumerable<IProduct> GetSocialShareReward()
        {
            yield return Price.Premium(10).GetProduct(_factory);
        }

        public IEnumerable<IProduct> GetAdReward()
        {
            yield return Price.Premium(1).GetProduct(_factory);
        }

        public IEnumerable<IProduct> GetHolidayLoot(System.Random random)
        {
            if (_holidayManager.IsChristmas)
            {
                if (random.Percentage(33))
                    yield return new Product(_factory.CreateCurrencyItem(Currency.Snowflakes));
            }
        }

        public IEnumerable<IProduct> GetMeteoriteLoot(Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(_playerSkills.PlanetaryScanner*100);

            yield return new Product(CreateArtifact(CommodityType.Minerals), 1 + random.Next2(20*quality/100));
            if (random.Percentage(5))
                yield return new Product(CreateArtifact(CommodityType.Gems), 1 + random.Next2(5 * quality / 100));
            if (random.Percentage(5))
                yield return new Product(CreateArtifact(CommodityType.PreciousMetals), 1 + random.Next2(5 * quality / 100));
        }

        public IEnumerable<IProduct> GetOutpostLoot(Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(_playerSkills.PlanetaryScanner * 100);

            yield return new Product(CreateArtifact(CommodityType.Scraps), 1 + random.Next2(20 * quality / 100));

            if (random.Percentage(quality/5))
            {
                var tech = _research.GetAvailableTechs(faction).Where(item => item.Hidden || item.Price <= 10).RandomElement(random);
                if (tech != null)
                    yield return new Product(_factory.CreateBlueprintItem(tech));
            }

            for (var i = 0; i < random.Next(quality/40); ++i)
                yield return new Product(RandomComponent(level, faction, random, true));

            if (random.Percentage(quality/5))
                yield return new Product(_factory.CreateResearchItem(faction));

            yield return Price.Premium(1 + random.Next(2 + quality + level) / 100).GetProduct(_factory);
        }

        public IEnumerable<IProduct> GetHiveLoot(int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(_playerSkills.PlanetaryScanner * 100);

            yield return new Product(CreateArtifact(CommodityType.Artifacts), 1 + random.Next2(5 * quality / 100));
            if (random.Percentage(20 + quality / 10))
                yield return new Product(RandomComponent(level, _database.ExplorationSettings.InfectedPlanetFaction, random, true));
            if (random.Percentage(20 + quality / 10))
                yield return new Product(RandomComponent(level, _database.ExplorationSettings.InfectedPlanetFaction, random, true));
            if (random.Percentage(20 + quality / 10))
                yield return new Product(RandomComponent(level, _database.ExplorationSettings.InfectedPlanetFaction, random, true));
            
            if (random.Percentage(quality/5))
                yield return new Product(RandomFactionShip(level, _database.ExplorationSettings.InfectedPlanetFaction, random));

            if (random.Percentage(quality/5))
            {
                var tech = _research.GetAvailableTechs((_database.ExplorationSettings.InfectedPlanetFaction)).Where(item => item.Hidden || item.Price <= 10).RandomElement(random);
                if (tech != null)
                    yield return new Product(_factory.CreateBlueprintItem(tech));
            }

            yield return Price.Premium(1 + random.Next(2 + quality + level) / 100).GetProduct(_factory);
        }

        public IEnumerable<IProduct> GetPlanetResources(PlanetType planetType, Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(_playerSkills.PlanetaryScanner * 100);

            if (planetType == PlanetType.Gas)
            {
                yield return new Product(CreateArtifact(CommodityType.ToxicWaste), 1 + random.Next2(100 * quality / 100));
                if (random.Percentage(30))
                    yield return new Product(_factory.CreateFuelItem(), 1 + random.Next2(5 * quality / 100));
            }
            else
            {
                yield return new Product(CreateArtifact(CommodityType.Minerals), 1 + random.Next2(20 * quality / 100));
                if (random.Percentage(5))
                    yield return new Product(CreateArtifact(CommodityType.Gems), 1 + random.Next2(5 * quality / 100));
                if (random.Percentage(5))
                    yield return new Product(CreateArtifact(CommodityType.PreciousMetals), 1 + random.Next2(5 * quality / 100));
            }
        }

        public IEnumerable<IProduct> GetPlanetRareResources(PlanetType planetType, Faction faction, int level, int seed)
        {
            return GetPlanetResources(planetType, faction, level, seed);
        }

        public IEnumerable<IProduct> GetContainerLoot(Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(_playerSkills.PlanetaryScanner * 100);

            yield return new Product(_factory.CreateCurrencyItem(Currency.Credits), Maths.Distance.Credits(level)/2 + random.Next2(Maths.Distance.Credits(level)*quality/200));

            if (random.Percentage(30))
                yield return new Product(CreateArtifact(CommodityType.Alloys), 1 + random.Next2(20 * quality / 100));
            if (random.Percentage(30))
                yield return new Product(CreateArtifact(CommodityType.Polymers), 1 + random.Next2(20 * quality / 100));
            if (random.Percentage(10))
                yield return new Product(CreateArtifact(CommodityType.Artifacts), 1 + random.Next2(10 * quality / 100));

            for (var i = 0; i < random.Next(quality/50); ++i)
                yield return new Product(RandomComponent(level, faction, random, true));
        }

        public IEnumerable<IProduct> GetShipWreckLoot(Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(_playerSkills.PlanetaryScanner * 100);

            yield return new Product(CreateArtifact(CommodityType.Scraps), 1 + random.Next2(50*quality/100));

            if (random.Percentage(30))
                yield return new Product(CreateArtifact(CommodityType.Alloys), 1 + random.Next2(20 * quality / 100));
            if (random.Percentage(30))
                yield return new Product(CreateArtifact(CommodityType.Polymers), 1 + random.Next2(20 * quality / 100));
            if (random.Percentage(20))
                yield return new Product(_factory.CreateFuelItem(), 1 + random.Next2(10 * quality / 100));

            if (random.Percentage(quality/5))
                yield return new Product(_factory.CreateResearchItem(faction));

            for (var i = 0; i < random.Next(quality / 50); ++i)
                yield return new Product(RandomComponent(level, faction, random, true));
        }

        public IEnumerable<IProduct> GetStarBaseSpecialReward(Region region)
        {
            yield return new Product(_factory.CreateResearchItem(region.Faction), Mathf.FloorToInt(3f + region.BaseDefensePower / 4f));

            if (region.IsPirateBase)
            {
                var random = _random.CreateRandom(region.Id);

                yield return Price.Premium(Mathf.Min(10, 1 + region.MilitaryPower / 30)).GetProduct(_factory);
                foreach (var faction in _database.FactionsWithEmpty.ValidForMerchants().RandomUniqueElements(4, random))
                    yield return new Product(_factory.CreateResearchItem(faction), Mathf.Min(10, 1 + region.MilitaryPower / 30));

                if (random.Percentage(30))
                {
                    var tech = _research.GetAvailableTechs(region.Faction).Where(item => item.Hidden || item.Price <= 10).RandomElement(random);
                    if (tech != null)
                        yield return new Product(_factory.CreateBlueprintItem(tech));
                }
            }
        }

        //public IEnumerable<IProduct> GetCommonPlanetReward(Faction faction, int level, System.Random random, float successChances)
        //{
        //    if (random.NextFloat() < successChances * successChances && random.Percentage(7))
        //        yield return Price.Premium(1).GetProduct(_factory);

        //    if (random.NextFloat() < successChances * successChances && random.Percentage(2))
        //    {
        //        var tech = _research.GetAvailableTechs(faction).Where(item => item.Hidden || item.Price <= 10).RandomElement(random);
        //        if (tech != null)
        //            yield return new Product(_factory.CreateBlueprintItem(tech));
        //    }

        //    if (System.DateTime.UtcNow.IsEaster())
        //        if (random.NextFloat() < successChances * successChances && random.Percentage(2))
        //            yield return new Product(_factory.CreateShipItem(new CommonShip(_database.GetShipBuild(LegacyShipBuildNames.GetId("fns3_mk2"))).Unlocked()));
        //}

        public IEnumerable<IProduct> GetSpaceWormLoot(int level, int seed)
        {
            var random = _random.CreateRandom(seed);
            yield return new Product(CreateArtifact(CommodityType.Artifacts), 1 + random.Next2(level));
            yield return Price.Premium(5 + random.Next2(level / 20)).GetProduct(_factory);

            if (random.Percentage(30))
            {
                var tech = _research.GetAvailableTechs().Where(item => item.Price <= 15).RandomElement(random);
                if (tech != null)
                    yield return new Product(_factory.CreateBlueprintItem(tech));
            }
        }

        public IEnumerable<IProduct> GetRuinsRewards(int level, int seed)
        {
            var random = _random.CreateRandom(seed);

            yield return Price.Common(5 * Maths.Distance.Credits(level)).GetProduct(_factory);
            yield return new Product(_factory.CreateFuelItem(), random.Next(5,15));

            if (random.Next(3) == 0)
            {
                var itemLevel = Mathf.Max(6, level / 2);
                var companions = _database.SatelliteList.Where(item => item.Layout.CellCount <= itemLevel && item.SizeClass != SizeClass.Titan);
                foreach (var item in companions.Where(item => item.SizeClass != SizeClass.Titan).RandomUniqueElements(1, random))
                    yield return new Product(_factory.CreateSatelliteItem(item));
            }

            foreach (var item in RandomComponents(Maths.Distance.ComponentLevel(level) + 35, random.Next(1, 3), null, random, false))
                yield return new Product(item);

            var quantity = random.Next(3);
            if (quantity > 0)
                yield return Price.Premium(quantity).GetProduct(_factory);

            yield return new Product(_factory.CreateResearchItem(_database.GalaxySettings.AbandonedStarbaseFaction));
        }

        public IEnumerable<IProduct> GetXmasRewards(int level, int seed)
        {
            var random = _random.CreateRandom(seed);

            yield return new Price(random.Range(level/5 + 15, level/5 + 30), Currency.Snowflakes).GetProduct(_factory);

            var items = _database.ComponentList.CommonAndRare().LevelLessOrEqual(level + 50)
                .RandomElements(random.Range(5, 10), random).Select(item =>
                    ComponentInfo.CreateRandomModification(item, random, ModificationQuality.P2));

            if (random.Percentage(10))
                yield return new Product(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(96))))); // xmas bomb
            if (random.Percentage(5) && level > 50)
                yield return new Product(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(215))))); // drone bay
            if (random.Percentage(5) && level > 50)
                yield return new Product(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(220))))); // drone bay
            if (random.Percentage(5) && level > 50)
                yield return new Product(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(219))))); // drone bay
            if (random.Percentage(5) && level > 100)
                yield return new Product(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(213))))); // holy cannon

            foreach (var item in items)
                yield return new Product(_factory.CreateComponentItem(item));
        }

        public IEnumerable<IProduct> GetDailyReward(int day, int level, int seed)
        {
            if (day <= 0)
                yield break;

            yield return new Price(Mathf.Min(day*100, 1000), Currency.Credits).GetProduct(_factory);

            if (day % 2 == 0)
                yield return new Product(_factory.CreateFuelItem(), Mathf.Min(30, 10*day/2));
            else if (day % 3 == 0)
                yield return new Product(_factory.CreateResearchItem(_database.FactionsWithEmpty.CanGiveTechPoints(level).RandomElement(new System.Random(seed))), Mathf.Min(5,day/3));
            else if (day % 5 == 0)
                yield return Price.Premium(Mathf.Min(5,day/5)).GetProduct(_factory);

            if (day > 3)
            {
                var quality = (ComponentQuality)Mathf.Min(day/3, (int)ComponentQuality.P3);
                var component = ComponentInfo.CreateRandom(_database, level, null, _random.CreateRandom(seed), false, quality);
                yield return new Product(_factory.CreateComponentItem(component));
            }
        }

        public IItemType GetRandomComponent(int distance, Faction faction, int seed, bool allowRare)
        {
            var random = _random.CreateRandom(seed);
            return RandomComponent(distance, faction, random, allowRare);
        }

        public IItemType GetRandomComponent(int distance, int seed, bool allowRare)
        {
            var random = _random.CreateRandom(seed);
            return RandomComponent(distance, null, random, allowRare);
        }

        public IEnumerable<IItemType> GetRandomComponents(int distance, int count, Faction faction, int seed, bool allowRare, ComponentQuality maxQuality = ComponentQuality.P3)
        {
            var random = _random.CreateRandom(seed);
            return RandomComponents(distance, count, faction, random, allowRare, maxQuality);
        }

        public IEnumerable<IItemType> GetRandomComponents(int distance, int count, int seed, bool allowRare, ComponentQuality maxQuality = ComponentQuality.P3)
        {
            var random = _random.CreateRandom(seed);
            return RandomComponents(distance, count, null, random, allowRare, maxQuality);
        }

        public IItemType GetRandomFactionShip(int distance, Faction faction, int seed)
        {
            var random = _random.CreateRandom(seed);
            return RandomFactionShip(distance, faction, random);
        }

        public DamagedShipItem GetRandomDamagedShip(int distance, int seed)
        {
            var random = _random.CreateRandom(seed);

            var value = random.Next(distance);
            var ships = value > 20 ? ShipBuildQuery.PlayerShips(_database).CommonAndRare() : ShipBuildQuery.PlayerShips(_database).Common();
            var ship = ships.FilterByStarDistance(distance/2).Random(random);

            return (DamagedShipItem)_factory.CreateDamagedShipItem(ship, random.Next());
        }

        private IItemType RandomFactionShip(int distance, Faction faction, System.Random random)
        {
            var ship = ShipBuildQuery.PlayerShips(_database).Common().BelongToFaction(faction).
				FilterByStarDistance(distance, ShipBuildQuery.FilterMode.Size).Random(random);
            return ship != null ? _factory.CreateMarketShipItem(new CommonShip(ship)) : null;
        }

        private IEnumerable<IItemType> RandomComponents(int distance, int count, Faction faction, System.Random random, bool allowRare, ComponentQuality maxQuality = ComponentQuality.P3)
        {
            for (var i = 0; i < count; ++i)
                yield return _factory.CreateComponentItem(ComponentInfo.CreateRandom(_database, distance, faction, random, allowRare, maxQuality));
        }

        private IItemType RandomComponent(int distance, Faction faction, System.Random random, bool allowRare, ComponentQuality maxQuality = ComponentQuality.P3)
        {
            return _factory.CreateComponentItem(ComponentInfo.CreateRandom(_database, distance, faction, random, allowRare, maxQuality));
        }

        private IItemType CreateArtifact(CommodityType commodityType)
        {
            var artifact = _database.GetQuestItem(new ItemId<QuestItem>((int)commodityType));
            return _factory.CreateArtifactItem(artifact);
        }
    }
}
