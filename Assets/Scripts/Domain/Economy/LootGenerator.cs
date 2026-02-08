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
using Constructor.Extensions;

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
                money += ship.Price()/5; // 4 times the money from defeating ships

                if (ship.Model.ShipType == ShipType.Flagship)
                {
                    var bossFaction = ship.Model.Faction;
                    var extraThreatLevel = (int)ship.ExtraThreatLevel;

                    yield return CommonProduct.Create(_factory.CreateResearchItem(bossFaction), 1 + extraThreatLevel); // Recieve alien technologies (research point) of the corresponding faction for every defeated flag ship based on its ExtraThreatLevel
                    yield return Price.Premium(1 + extraThreatLevel).GetProduct(_factory); // Recieve stars for every defeated flag ship based on its ExtraThreatLevel
                    foreach (var item in RandomComponents(moduleLevel + 35 + extraThreatLevel * 20, random.Next(1, 2 + extraThreatLevel), bossFaction, random, false, ComponentQuality.N3))
                        yield return CommonProduct.Create(item); // Recieve ship components for every defeated flag ship based on its ExtraThreatLevel
                }
                else
                {
                    foreach (var item in RandomComponents(moduleLevel, random.Next(-10, 2), faction, random, false, ComponentQuality.N3))
                        yield return CommonProduct.Create(item);
                }
            }

            if (money > 0)
                yield return Price.Common(money).GetProduct(_factory);

            var toxicWaste = random.Next2(scraps/2);
            if (toxicWaste > 0)
                yield return CommonProduct.Create(CreateArtifact(CommodityType.ToxicWaste), toxicWaste);

            scraps -= toxicWaste;
            if (scraps > 0)
                yield return CommonProduct.Create(CreateArtifact(CommodityType.Scraps), scraps);

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
                if (random.Percentage(67)) // Now you get twice the snowflakes
                    yield return CommonProduct.Create(_factory.CreateCurrencyItem(Currency.Snowflakes));
            }
        }

        public IEnumerable<IProduct> GetMeteoriteLoot(Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt((1f/(1f+Mathf.Exp(-10f*(_playerSkills.PlanetaryScanner-1.5f)))+1)*100); // Change how the player feel when leveling up PlanetaryScanner (Mapping PlanetaryScanner from [1, 2] to [1, 2] using sigmoid function)

            yield return CommonProduct.Create(CreateArtifact(CommodityType.Minerals), 10 + random.Next2(200*quality/100));
            if (random.Percentage(10*quality/100))
                yield return CommonProduct.Create(CreateArtifact(CommodityType.Gems), 5 + random.Next2(25 * quality / 100));
            if (random.Percentage(10*quality/100))
                yield return CommonProduct.Create(CreateArtifact(CommodityType.PreciousMetals), 5 + random.Next2(25 * quality / 100));
            // about 10 times the original amount of loot
        }

        public IEnumerable<IProduct> GetOutpostLoot(Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt((1f/(1f+Mathf.Exp(-10f*(_playerSkills.PlanetaryScanner-1.5f)))+1)*100);
            
            yield return CommonProduct.Create(CreateArtifact(CommodityType.Scraps), 10 + random.Next2(200 * quality / 100)); // 10 times the original amount of scraps from destroying a outpost in planet exploration

            if (random.Percentage(quality/2)) // Guaranties to recive a blueprint/research items if your PlanetaryScanner skill is at the highest level (min(PlanetaryScanner)=1f, max(PlanetaryScanner)=2f) 
            {
                var tech = _research.GetAvailableTechs(faction).Where(item => item.Hidden || item.Price <= 25).RandomElement(random); // Now you can get the blueprint of a tech if that tech requires less than or equal to 25 alien technologies to research
                if (tech != null)
                    yield return CommonProduct.Create(_factory.CreateBlueprintItem(tech));
                else
                {
                    yield return CommonProduct.Create(_factory.CreateResearchItem(faction),
                    5 + random.Next(quality/40+1)); // You will be rewarded with 5~10 alien technologies instead if there are no blueprints that meet the requirements above
                }
            }

            for (var i = 0; i < random.Next(1, quality/40); ++i)
                if (TryCreateRandomComponent(level, faction, random, true, ComponentQuality.N3, ComponentQuality.P3, out var itemType))
                    yield return CommonProduct.Create(itemType);

            yield return Price.Premium(3 + random.Next(2 + quality + level) / 100).GetProduct(_factory);
        }

        public IEnumerable<IProduct> GetHiveLoot(int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(Mathf.RoundToInt((1f/(1f+Mathf.Exp(-10f*(_playerSkills.PlanetaryScanner-1.5f)))+1)*100));

            yield return CommonProduct.Create(CreateArtifact(CommodityType.Artifacts), 10 + random.Next2(50 * quality / 100));

            for (int i = 0; i < 3; ++i)
               if (random.Percentage(25 + quality / 4))
                    if (TryCreateRandomComponent(level, _database.ExplorationSettings.InfectedPlanetFaction, random, true, ComponentQuality.N3, ComponentQuality.P3, out var itemType))
                        yield return CommonProduct.Create(itemType);

            if (random.Percentage(quality/5))
                yield return CommonProduct.Create(RandomFactionShip(level, _database.ExplorationSettings.InfectedPlanetFaction, random));

            if (random.Percentage(quality/2))
            {
                var tech = _research.GetAvailableTechs((_database.ExplorationSettings.InfectedPlanetFaction)).Where(item => item.Hidden || item.Price <= 25).RandomElement(random);
                if (tech != null)
                    yield return CommonProduct.Create(_factory.CreateBlueprintItem(tech));
            }

            yield return Price.Premium(3 + random.Next(2 + quality + level) / 100).GetProduct(_factory);
        }

        public IEnumerable<IProduct> GetPlanetResources(PlanetType planetType, Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(Mathf.RoundToInt((1f/(1f+Mathf.Exp(-10f*(_playerSkills.PlanetaryScanner-1.5f)))+1)*100));

            if (planetType == PlanetType.Gas)
            {
                yield return CommonProduct.Create(CreateArtifact(CommodityType.ToxicWaste), 10 + random.Next2(1000 * quality / 100));
                if (random.Percentage(30*quality/100))
                    yield return CommonProduct.Create(_factory.CreateFuelItem(), 5 + random.Next2(25 * quality / 100));
            }
            else
            {
                yield return CommonProduct.Create(CreateArtifact(CommodityType.Minerals), 10 + random.Next2(200 * quality / 100));
                if (random.Percentage(5*quality/100))
                    yield return CommonProduct.Create(CreateArtifact(CommodityType.Gems), 5 + random.Next2(25 * quality / 100));
                if (random.Percentage(5*quality/100))
                    yield return CommonProduct.Create(CreateArtifact(CommodityType.PreciousMetals), 5 + random.Next2(25 * quality / 100));
            }
        }

        public IEnumerable<IProduct> GetPlanetRareResources(PlanetType planetType, Faction faction, int level, int seed)
        {
            return GetPlanetResources(planetType, faction, level, seed);
        }

        public IEnumerable<IProduct> GetContainerLoot(Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(Mathf.RoundToInt((1f/(1f+Mathf.Exp(-10f*(_playerSkills.PlanetaryScanner-1.5f)))+1)*100));

            yield return CommonProduct.Create(_factory.CreateCurrencyItem(Currency.Credits), 5*Maths.Distance.Credits(level) + random.Next2(Maths.Distance.Credits(level)*quality/20));
            yield return Price.Premium(random.Next(1, quality / 40 + 1)).GetProduct(_factory);

            if (random.Percentage(30*quality/100))
                yield return CommonProduct.Create(CreateArtifact(CommodityType.Alloys), 5 + random.Next2(100 * quality / 100));
            if (random.Percentage(30*quality/100))
                yield return CommonProduct.Create(CreateArtifact(CommodityType.Polymers), 5 + random.Next2(100 * quality / 100));
            if (random.Percentage(10*quality/100))
                yield return CommonProduct.Create(CreateArtifact(CommodityType.Artifacts), 5 + random.Next2(50 * quality / 100));

            for (var i = 0; i < random.Next(1, quality/50); ++i)
                if (TryCreateRandomComponent(level, faction, random, true, ComponentQuality.N3, ComponentQuality.P3, out var itemType))
                    yield return CommonProduct.Create(itemType);
                    
            if (random.Percentage(quality/2)) // Player now can to recieve a P3 (best quality; quality rank: N3-N2-N1-N0-P1-P2-P3) component if PlanetaryScanner skill is at max level.
            {
                var component = _database.ComponentList.CommonAndRare()
                    .FilterByFactionOrEmpty(faction)
                    .LevelLessOrEqual(3 * level / 2)
                    .RandomElement(random);

                if (component != null)
                {
                    var componentInfo = ComponentInfo.CreateRandomModification(
                        component,
                        random,
                        ModificationQuality.P3,
                        ModificationQuality.P3);

                    yield return CommonProduct.Create(_factory.CreateComponentItem(componentInfo));
                }
            }
        }

        public IEnumerable<IProduct> GetShipWreckLoot(Faction planetFaction, Faction wreckFaction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(Mathf.RoundToInt((1f/(1f+Mathf.Exp(-10f*(_playerSkills.PlanetaryScanner-1.5f)))+1)*100));
            var faction = wreckFaction ?? planetFaction;

            yield return CommonProduct.Create(CreateArtifact(CommodityType.Scraps), 10 + random.Next2(500*quality/100));

            if (random.Percentage(30*quality/100))
                yield return CommonProduct.Create(CreateArtifact(CommodityType.Alloys), 5 + random.Next2(100 * quality / 100));
            if (random.Percentage(30*quality/100))
                yield return CommonProduct.Create(CreateArtifact(CommodityType.Polymers), 5 + random.Next2(100 * quality / 100));
            if (random.Percentage(20*quality/100))
                yield return CommonProduct.Create(_factory.CreateFuelItem(), 5 + random.Next2(50 * quality / 100));

            if (random.Percentage(quality/2))
                yield return CommonProduct.Create(_factory.CreateResearchItem(faction), 1 + random.Next(quality/66));

            for (var i = 0; i < random.Next(quality / 50); ++i)
                if (TryCreateRandomComponent(level, faction, random, true, ComponentQuality.N3, ComponentQuality.P3, out var itemType))
                    yield return CommonProduct.Create(itemType);
        }

        public IEnumerable<IProduct> GetStarBaseSpecialReward(Region region)
        {
            yield return CommonProduct.Create(_factory.CreateResearchItem(region.Faction), Mathf.FloorToInt(3f + region.BaseDefensePower / 100f) + region.HomeStarLevel / 60); // More alien technologies from capturing a starbase

            if (region.IsPirateBase)
            {
                var random = _random.CreateRandom(region.Id);

                yield return Price.Premium(Mathf.Min(20, 5 + region.HomeStarLevel / 30)).GetProduct(_factory);
                foreach (var faction in _database.FactionsWithEmpty.ValidForMerchants().RandomUniqueElements(4, random))
                    yield return CommonProduct.Create(_factory.CreateResearchItem(faction), Mathf.Min(20, 1 + region.HomeStarLevel / 30));

                if (random.Percentage(30))
                {
                    var tech = _research.GetAvailableTechs(region.Faction).Where(item => item.Hidden || item.Price <= 25).RandomElement(random);
                    if (tech != null)
                        yield return CommonProduct.Create(_factory.CreateBlueprintItem(tech));
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
            yield return CommonProduct.Create(CreateArtifact(CommodityType.Artifacts), 1 + random.Next2(level));
            yield return Price.Premium(5 + random.Next2(level / 20)).GetProduct(_factory);

            if (random.Percentage(30))
            {
                var tech = _research.GetAvailableTechs().Where(item => item.Price <= 15).RandomElement(random);
                if (tech != null)
                    yield return CommonProduct.Create(_factory.CreateBlueprintItem(tech));
            }
        }

        public IEnumerable<IProduct> GetRuinsRewards(int level, int seed)
        {
            var random = _random.CreateRandom(seed);

            yield return Price.Common(25 * Maths.Distance.Credits(level)).GetProduct(_factory);
            yield return CommonProduct.Create(_factory.CreateFuelItem(), random.Next(5,15));

            if (random.Next(3) == 0)
            {
                var itemLevel = Mathf.Max(6, level / 2);
                var companions = _database.SatelliteList.Where(item => item.Layout.CellCount <= itemLevel && item.SizeClass != SizeClass.Titan);
                foreach (var item in companions.Where(item => item.SizeClass != SizeClass.Titan).RandomUniqueElements(1, random))
                    yield return CommonProduct.Create(_factory.CreateSatelliteItem(item));
            }

            foreach (var item in RandomComponents(Maths.Distance.ComponentLevel(level) + 35, random.Next(1, 3), null, random, false, ComponentQuality.N3))
                yield return CommonProduct.Create(item);

            var quantity = random.Next(3);
            if (quantity > 0)
                yield return Price.Premium(quantity).GetProduct(_factory);

            yield return CommonProduct.Create(_factory.CreateResearchItem(_database.GalaxySettings.AbandonedStarbaseFaction), random.Next(3, 7));
        }

        public IEnumerable<IProduct> GetXmasRewards(int level, int seed)
        {
            var random = _random.CreateRandom(seed);

            yield return new Price(random.Range(level/5 + 15, level/5 + 30), Currency.Snowflakes).GetProduct(_factory);

            var items = _database.ComponentList.CommonAndRare().LevelLessOrEqual(level + 50)
                .RandomElements(random.Range(5, 10), random).Select(item =>
                    ComponentInfo.CreateRandomModification(item, random, ModificationQuality.P2));

            if (random.Percentage(10))
                yield return CommonProduct.Create(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(96))))); // xmas bomb
            if (random.Percentage(5) && level > 50)
                yield return CommonProduct.Create(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(215))))); // drone bay
            if (random.Percentage(5) && level > 50)
                yield return CommonProduct.Create(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(220))))); // drone bay
            if (random.Percentage(5) && level > 50)
                yield return CommonProduct.Create(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(219))))); // drone bay
            if (random.Percentage(5) && level > 100)
                yield return CommonProduct.Create(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(213))))); // holy cannon

            foreach (var item in items)
                yield return CommonProduct.Create(_factory.CreateComponentItem(item));
        }

        public IEnumerable<IProduct> GetDailyReward(int day, int level, int seed)
        {
            if (day <= 0)
                yield break;

            yield return new Price(Mathf.Min(day*100, 1000), Currency.Credits).GetProduct(_factory);

            if (day % 2 == 0)
                yield return CommonProduct.Create(_factory.CreateFuelItem(), Mathf.Min(30, 10*day/2));
            else if (day % 3 == 0)
                yield return CommonProduct.Create(_factory.CreateResearchItem(_database.FactionsWithEmpty.CanGiveTechPoints(level).RandomElement(new System.Random(seed))), Mathf.Min(5,day/3));
            else if (day % 5 == 0)
                yield return Price.Premium(Mathf.Min(5,day/5)).GetProduct(_factory);

            if (day > 3)
            {
                var quality = (ComponentQuality)Mathf.Min(day/3, (int)ComponentQuality.P3);
                if (ComponentInfo.TryCreateRandomComponent(_database, level, null, _random.CreateRandom(seed), false, quality, out var componentInfo))
                    yield return CommonProduct.Create(_factory.CreateComponentItem(componentInfo));
            }
        }

        public bool TryGetRandomComponent(int distance, int seed, bool allowRare, out IProduct product)
        {
            var random = _random.CreateRandom(seed);
            if (TryCreateRandomComponent(distance, null, random, allowRare, ComponentQuality.N3, ComponentQuality.P3, out var item))
            {
                product = CommonProduct.Create(item);
                return true;
            }

            product = null;
            return false;
        }

        public IEnumerable<IItemType> GetRandomComponents(int distance, int count, Faction faction, int seed, bool allowRare, ComponentQuality maxQuality = ComponentQuality.P3)
        {
            var random = _random.CreateRandom(seed);
            return RandomComponents(distance, count, faction, random, allowRare, ComponentQuality.N3, maxQuality);
        }

        public IEnumerable<IItemType> GetRandomComponents(int distance, int count, int seed, bool allowRare, ComponentQuality maxQuality = ComponentQuality.P3)
        {
            var random = _random.CreateRandom(seed);
            return RandomComponents(distance, count, null, random, allowRare, ComponentQuality.N3, maxQuality);
        }

        public IEnumerable<IItemType> GetRandomComponents(int distance, int count, int seed, bool allowRare, ComponentQuality minQuality, ComponentQuality maxQuality)
        {
            var random = _random.CreateRandom(seed);
            return RandomComponents(distance, count, null, random, allowRare, minQuality, maxQuality);
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
            var ship = ships.FilterByStarDistance(distance/2, ShipBuildQuery.FilterMode.SizeAndFaction).Random(random);

            return (DamagedShipItem)_factory.CreateDamagedShipItem(ship, random.Next());
        }

        private IItemType RandomFactionShip(int distance, Faction faction, System.Random random)
        {
            var ship = ShipBuildQuery.PlayerShips(_database).Common().BelongToFaction(faction).
				FilterByStarDistance(distance, ShipBuildQuery.FilterMode.Size).Random(random);
            return ship != null ? _factory.CreateMarketShipItem(new CommonShip(ship, _database)) : null;
        }

        private IEnumerable<IItemType> RandomComponents(int distance, int count, Faction faction, System.Random random, bool allowRare, ComponentQuality minQuality, ComponentQuality maxQuality = ComponentQuality.P3)
        {
            var produced = 0;
            var attempts = 0;
            var maxAttempts = count * 20;

            while (produced < count && attempts < maxAttempts)
            {
                ++attempts;
                if (TryCreateRandomComponent(distance, faction, random, allowRare, minQuality, maxQuality, out var component))
                {
                    ++produced;
                    yield return component;
                }
            }
        }

        // Backward compatibility for existing callers
        private bool TryCreateRandomComponent(int distance, Faction faction, System.Random random, bool allowRare, ComponentQuality maxQuality, out IItemType itemType)
        {
            return TryCreateRandomComponent(distance, faction, random, allowRare, ComponentQuality.N3, maxQuality, out itemType);
        }

        private bool TryCreateRandomComponent(int distance, Faction faction, System.Random random, bool allowRare, ComponentQuality minQuality, ComponentQuality maxQuality, out IItemType itemType)
        {
            // sample until we meet the quality floor or exhaust attempts
            for (int i = 0; i < 20; ++i)
            {
                if (!ComponentInfo.TryCreateRandomComponent(_database, distance, faction, random, allowRare, maxQuality, out var componentInfo))
                    continue;

                // componentInfo.ModificationQuality is N3..P3; P0 comes back as N3 with empty mod
                var modQuality = componentInfo.ModificationQuality;
                var requiredQuality = minQuality switch
                {
                    ComponentQuality.N3 => ModificationQuality.N3,
                    ComponentQuality.N2 => ModificationQuality.N2,
                    ComponentQuality.N1 => ModificationQuality.N1,
                    ComponentQuality.P1 => ModificationQuality.P1,
                    ComponentQuality.P2 => ModificationQuality.P2,
                    ComponentQuality.P3 => ModificationQuality.P3,
                    _ => ModificationQuality.N3,
                };

                if (modQuality < requiredQuality)
                    continue;

                itemType = _factory.CreateComponentItem(componentInfo);
                return true;
            }

            itemType = null;
            return false;
        }

        private IItemType CreateArtifact(CommodityType commodityType)
        {
            var artifact = _database.GetQuestItem(new ItemId<QuestItem>((int)commodityType));
            return _factory.CreateArtifactItem(artifact);
        }
    }

    public static class ProductListExtensions
    {
        public static Domain.Quests.ILoot ToLoot(this IEnumerable<IProduct> products)
        {
            var loot = new Loot();
            foreach (var item in products)
                loot.Add(item.Type, item.Quantity);

            return loot;
        }

        private class Loot : Domain.Quests.ILoot
        {
            private List<Domain.Quests.LootItem> _items = new();

            public void Add(IItemType item, int quantity)
            {
                _items.Add(new Domain.Quests.LootItem(item, quantity));
            }

            public IEnumerable<Domain.Quests.LootItem> Items => _items;
            public bool CanBeRemoved => false;
        }
    }
}
