using System;
using System.Collections.Generic;
using System.Linq;
using Economy.ItemType;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Extensions;

namespace Domain.Quests
{
    public interface ILoot
    {
        IEnumerable<LootItem> Items { get; }
        bool CanBeRemoved { get; }
    }

    public static class LootExtensions
    {
        public static void Consume(this ILoot loot)
        {
            foreach (var item in loot.Items)
                item.Type.Consume(item.Quantity);
        }

        public static void Remove(this ILoot loot)
        {
            foreach (var item in loot.Items)
                item.Type.Withdraw(item.Quantity);
        }
    }

    public class EmptyLoot : ILoot
    {
        public IEnumerable<LootItem> Items { get { return Enumerable.Empty<LootItem>(); } }
        public bool CanBeRemoved { get { return true; } }
        public static readonly EmptyLoot Instance = new EmptyLoot();
    }

    public struct LootItem
    {
        public LootItem(IItemType type, int quantity = 1)
        {
            Type = type;
            Quantity = quantity;
        }

        public readonly IItemType Type;
        public readonly int Quantity;
    }

    public class Loot : ILoot, ILootContentFactory<IEnumerable<LootItem>>
    {
        public Loot(LootModel loot, QuestInfo questInfo, ILootItemFactory lootItemFactory, IDatabase database)
        {
            _lootItemFactory = lootItemFactory;
            _database = database;
            _loot = loot;
            _questInfo = questInfo;
            _random = new Random(questInfo.Seed + loot.Id.Value);
        }

        public bool CanBeRemoved
        {
            get
            {
                if (_items == null)
                    Initialize();

                foreach (var item in _items)
                    if (item.Type.MaxItemsToWithdraw < item.Quantity)
                        return false;

                return true;
            }
        }

        public IEnumerable<LootItem> Items
        {
            get
            {
                if (_items == null)
                    Initialize();

                return _items;
            }
        }

        private void Initialize()
        {
            _items = new List<LootItem>();
            foreach (var item in _loot.Loot.Create(this))
                _items.Add(item);
        }

        #region ILootItemFactory

        public IEnumerable<LootItem> Create(LootContent_None content)
        {
            return Enumerable.Empty<LootItem>();
        }

        public IEnumerable<LootItem> Create(LootContent_SomeMoney content)
        {
            var level = UnityEngine.Mathf.Max((int)(_questInfo.Level * content.ValueRatio), 1);
            yield return _lootItemFactory.CreateRandomMoney(level, _random);
        }

        public IEnumerable<LootItem> Create(LootContent_Fuel content)
        {
            var amount = _random.Range(content.MinAmount, content.MaxAmount);
            yield return _lootItemFactory.CreateFuel(amount);
        }

        public IEnumerable<LootItem> Create(LootContent_Money content)
        {
            var amount = _random.Range(content.MinAmount, content.MaxAmount);
            yield return _lootItemFactory.CreateMoney(amount);
        }

        public IEnumerable<LootItem> Create(LootContent_Stars content)
        {
            var amount = _random.Range(content.MinAmount, content.MaxAmount);
            yield return _lootItemFactory.CreateStars(amount);
        }

        public IEnumerable<LootItem> Create(LootContent_StarMap content)
        {
            yield return _lootItemFactory.CreateStarMap(_questInfo.StarId);
        }

        public IEnumerable<LootItem> Create(LootContent_RandomComponents content)
        {
            var level = Math.Max(0, (int)(_questInfo.Level * content.ValueRatio));
            var amount = _random.Range(content.MinAmount, content.MaxAmount);
            var factionFilter = new FactionFilter(content.Factions, level, _questInfo.Faction);
            var components = _database.ComponentList.Available().LevelLessOrEqual(level * 3 / 2).Where(item => factionFilter.IsSuitableForLoot(item.Faction));
            return _lootItemFactory.CreateRandomComponents(components, amount, level, _random);
        }

        public IEnumerable<LootItem> Create(LootContent_ResearchPoints content)
        {
            var amount = _random.Range(content.MinAmount, content.MaxAmount);
            var factionFilter = new FactionFilter(content.Factions, _questInfo.Level, _questInfo.Faction);
            var faction = _database.FactionsWithEmpty.Where(factionFilter.IsSuitableForResearch).RandomElement(_random);

            if (faction != null)
                yield return _lootItemFactory.CreateResearchPoints(faction, amount);
        }

        public IEnumerable<LootItem> Create(LootContent_RandomItems content)
        {
            var amount = _random.Range(content.MinAmount, content.MaxAmount);
            var itemCount = content.Items.Count;

            if (itemCount == 0)
                yield break;

            var totalWeight = 0f;
            for (var i = 0; i < itemCount; ++i)
                totalWeight += content.Items[i].Weight;

            if (totalWeight < 0.0001f)
                foreach (var item in content.Items.RandomUniqueElements(amount, itemCount, _random).SelectMany(item => item.Loot.Create(this)))
                    yield return item;

            var itemsLeft = amount;
            foreach (var item in content.Items)
            {
                if (itemsLeft <= 0)
                    yield break;

                if (_random.NextFloat() * totalWeight >= item.Weight * itemsLeft)
                    continue;

                itemsLeft--;
                totalWeight = totalWeight > item.Weight ? totalWeight - item.Weight : 0f;

                foreach (var lootItem in item.Loot.Create(this))
                    yield return lootItem;
            }
        }

        public IEnumerable<LootItem> Create(LootContent_AllItems content)
        {
            return content.Items.SelectMany(item => item.Loot.Create(this));
        }

        public IEnumerable<LootItem> Create(LootContent_ItemsWithChance content)
        {
            foreach (var item in content.Items)
            {
                if (_random.NextFloat() > item.Weight)
                    continue;

                foreach (var lootItem in item.Loot.Create(this))
                    yield return lootItem;
            }
        }

        public IEnumerable<LootItem> Create(LootContent_QuestItem content)
        {
            var amount = _random.Range(content.MinAmount, content.MaxAmount);
            yield return _lootItemFactory.CreateQuestItem(content.QuestItem, amount);
        }

        public IEnumerable<LootItem> Create(LootContent_Ship content)
        {
            yield return _lootItemFactory.CreateShip(content.ShipBuild);
        }

        public IEnumerable<LootItem> Create(LootContent_EmptyShip content)
        {
            yield return _lootItemFactory.CreateShip(content.Ship);
        }

        public IEnumerable<LootItem> Create(LootContent_Component content)
        {
            var amount = _random.Range(content.MinAmount, content.MaxAmount);
            yield return _lootItemFactory.CreateComponent(content.Component, amount);
        }

        public IEnumerable<LootItem> Create(LootContent_Blueprint content)
        {
            yield return _lootItemFactory.CreateBlueprint(content.Blueprint);
        }

        public IEnumerable<LootItem> Create(LootContent_Satellite content)
        {
            var amount = _random.Range(content.MinAmount, content.MaxAmount);
            yield return _lootItemFactory.CreateSatellite(content.Satellite, amount);
        }

        #endregion

        private List<LootItem> _items;
        private readonly Random _random;
        private readonly QuestInfo _questInfo;
        private readonly LootModel _loot;
        private readonly IDatabase _database;
        private readonly ILootItemFactory _lootItemFactory;
    }
}
