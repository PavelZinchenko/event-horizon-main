using System;
using System.Collections.Generic;
using System.Linq;
using GameDatabase;
using GameDatabase.DataModel;
using GameDiagnostics;
using Assert = UnityEngine.Assertions.Assert;

namespace Domain.Quests
{
    public interface IRequirementCache
    {
        INodeRequirements Get(Requirement requirement, QuestInfo context);
    }

    public interface IEnemyCache
    {
        QuestEnemyData Get(Fleet enemy, QuestInfo questInfo);
    }

    public interface ILootCache
    {
        ILoot Get(LootModel model, QuestInfo questInfo);
    }

    public struct Key<T> : IEquatable<Key<T>>
        where T : class
    {
        public Key(QuestInfo questInfo, T item)
        {
            Assert.IsNotNull(item);

            StarId = questInfo.StarId;
            Seed = questInfo.Seed;
            Item = item;
        }

        public bool Equals(Key<T> other)
        {
            return StarId == other.StarId && Seed == other.Seed && ReferenceEquals(Item, other.Item);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Key<T> && Equals((Key<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StarId;
                hashCode = (hashCode * 397) ^ Seed;
                hashCode = (hashCode * 397) ^ Item.GetHashCode();
                return hashCode;
            }
        }

        public readonly int StarId;
        public readonly int Seed;
        public readonly T Item;
    }

    public class RequirementCache : IRequirementCache
    {
        public RequirementCache(IQuestBuilderContext context, ILootCache lootCache)
        {
            _lootCache = lootCache;
            _context = context;
        }

        public INodeRequirements Get(Requirement requirement, QuestInfo questInfo)
        {
            if (requirement == null) return null;

            var key = new Key<Requirement>(questInfo, requirement);

            INodeRequirements value;
            if (!_cache.TryGetValue(key, out value))
            {
                var builder = new RequirementsBuilder(questInfo, _lootCache, _context);
                value = builder.Build(requirement);
                _cache.Add(key, value);
            }

            return value;
        }

        private readonly IQuestBuilderContext _context;
        private readonly Dictionary<Key<Requirement>, INodeRequirements> _cache = new Dictionary<Key<Requirement>, INodeRequirements>();
        private readonly ILootCache _lootCache;
    }

    public class EnemyCache : IEnemyCache
    {
        public EnemyCache(IDatabase database)
        {
            _factory = new FleetFactory(database);
        }

        public QuestEnemyData Get(Fleet enemy, QuestInfo questInfo)
        {
            if (enemy == null) return new();

            var key = new Key<Fleet>(questInfo, enemy);

            QuestEnemyData value;
            if (!_cache.TryGetValue(key, out value))
            {
                value = _factory.CreateCombatPlan(enemy, questInfo);
                _cache.Add(key, value);
            }

            return value;
        }

        private readonly Dictionary<Key<Fleet>, QuestEnemyData> _cache = new();
        private readonly FleetFactory _factory;
    }

    public class LootCache : ILootCache
    {
        public LootCache(ILootItemFactory lootItemFactory, IDatabase database)
        {
            _lootItemFactory = lootItemFactory;
            _database = database;
        }

        public void SetRequirementCache(IRequirementCache requirementCache)
        {
            _requirementCache = requirementCache;
        }

        public ILoot Get(LootModel loot, QuestInfo questInfo)
        {
            if (loot == null) return null;

            var key = new Key<LootModel>(questInfo, loot);

            ILoot value;
            if (!_cache.TryGetValue(key, out value))
            {
                value = new Loot(loot, questInfo, _lootItemFactory,  _requirementCache, _database);
                _cache.Add(key, value);
            }

            return value;
        }

        private readonly IDatabase _database;
        private readonly ILootItemFactory _lootItemFactory;
        private readonly Dictionary<Key<LootModel>, ILoot> _cache = new Dictionary<Key<LootModel>, ILoot>();
        private IRequirementCache _requirementCache;
    }
}
