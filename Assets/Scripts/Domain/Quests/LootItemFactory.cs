using System.Collections.Generic;
using System.Linq;
using Constructor;
using Constructor.Ships;
using Economy;
using Economy.ItemType;
using GameDatabase.DataModel;

namespace Domain.Quests
{
    public class LootItemFactory : ILootItemFactory
    {
        private readonly ItemTypeFactory _itemTypeFactory;
        private readonly GameServices.Database.ITechnologies _technologies;

        public LootItemFactory(ItemTypeFactory itemTypeFactory, GameServices.Database.ITechnologies technologies)
        {
            _itemTypeFactory = itemTypeFactory;
            _technologies = technologies;
        }

        public LootItem CreateBlueprint(Technology technology)
        {
            return new LootItem(_itemTypeFactory.CreateBlueprintItem(_technologies.Get(technology.Id)));
        }

        public LootItem CreateComponent(Component component, int amount)
        {
            return new LootItem(_itemTypeFactory.CreateComponentItem(new ComponentInfo(component)), amount);
        }

        public LootItem CreateFuel(int amount)
        {
            return new LootItem(_itemTypeFactory.CreateFuelItem(), amount);
        }

        public LootItem CreateMoney(int amount)
        {
            return new LootItem(_itemTypeFactory.CreateCurrencyItem(Currency.Credits), amount);
        }

        public LootItem CreateQuestItem(QuestItem questItem, int amount)
        {
            return new LootItem(_itemTypeFactory.CreateArtifactItem(questItem), amount);
        }

        public IEnumerable<LootItem> CreateRandomComponents(IEnumerable<Component> components, int amount, int level, System.Random random)
        {
            return ComponentInfo.CreateRandom(components, amount, level, random).
                Select(item => new LootItem(_itemTypeFactory.CreateComponentItem(item)));
        }

        public LootItem CreateRandomMoney(int starLevel, System.Random random)
        {
            var amount = Maths.Distance.Credits(starLevel);
            amount = 9 * amount / 10 + random.Next(1 + amount / 5);
            return new LootItem(_itemTypeFactory.CreateCurrencyItem(Currency.Credits), amount);
        }

        public LootItem CreateResearchPoints(Faction faction, int amount)
        {
            return new LootItem(_itemTypeFactory.CreateResearchItem(faction), amount);
        }

        public LootItem CreateShip(ShipBuild shipBuild)
        {
            return new LootItem(_itemTypeFactory.CreateQuestShipItem(new CommonShip(shipBuild)));
        }

        public LootItem CreateShip(Ship ship)
        {
            return new LootItem(_itemTypeFactory.CreateQuestShipItem(new CommonShip(ship, Enumerable.Empty<IntegratedComponent>())));
        }

        public LootItem CreateStarMap(int starId)
        {
            return new LootItem(_itemTypeFactory.CreateStarMapItem(starId));
        }

        public LootItem CreateStars(int amount)
        {
            return new LootItem(Price.Premium(amount).GetProduct(_itemTypeFactory).Type, amount);
        }
    }
}
