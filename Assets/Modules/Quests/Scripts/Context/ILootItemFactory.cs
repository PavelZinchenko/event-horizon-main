using System.Collections.Generic;
using GameDatabase.DataModel;

namespace Domain.Quests
{
    public interface ILootItemFactory
    {
        LootItem CreateStars(int amount);
        LootItem CreateMoney(int amount);
        LootItem CreateRandomMoney(int starLevel, System.Random random);
        LootItem CreateFuel(int amount);
        LootItem CreateStarMap(int starId);
        LootItem CreateResearchPoints(Faction faction, int amount);
        LootItem CreateQuestItem(QuestItem questItem, int amount);
        LootItem CreateShip(ShipBuild shipBuild);
        LootItem CreateShip(Ship ship);
        LootItem CreateComponent(Component component, int amount);
        LootItem CreateBlueprint(Technology technology);
        LootItem CreateSatellite(Satellite satellite, int amount);
        IEnumerable<LootItem> CreateRandomComponents(IEnumerable<Component> components, int amount, int level, System.Random random);
    }
}
