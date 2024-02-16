using System;
using System.Collections.Generic;
using System.Linq;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Query;

namespace Domain.Quests
{
    public struct QuestEnemyData
    {
        public IEnumerable<ShipBuild> EnemyFleet;
        public CombatRules Rules;
        public int StarLevel;
        public int EnemyLevel;
        public int Seed;
    }

    public class FleetFactory
    {
        private readonly IDatabase _database;

        public FleetFactory(IDatabase database)
        {
            _database = database;
        }

        public QuestEnemyData CreateCombatPlan(Fleet enemy, QuestInfo questInfo)
        {
            var enemyLevel = Math.Max(0, questInfo.Level + enemy.LevelBonus);

            return new QuestEnemyData
            {
                EnemyFleet = CreateFleet(enemy, questInfo.Faction, enemyLevel, questInfo.Seed),
                Rules = enemy.CombatRules,
                StarLevel = questInfo.Level,
                EnemyLevel = enemyLevel,
                Seed = questInfo.Seed
            };
        }

        public QuestEnemyData CreateCombatPlan(Fleet enemy, Faction faction, int level, int seed)
        {
            var enemyLevel = Math.Max(0, level + enemy.LevelBonus);
            return new QuestEnemyData
            {
                EnemyFleet = CreateFleet(enemy, faction, enemyLevel, seed),
                Rules = enemy.CombatRules,
                StarLevel = level,
                EnemyLevel = enemyLevel,
                Seed = seed
            };
        }

        private IEnumerable<ShipBuild> CreateFleet(Fleet enemy, Faction faction, int level, int seed)
        {
            var random = new Random(seed);
            var factionFilter = new FactionFilter(enemy.Factions, level, faction);

            var numberOfShips = enemy.NoRandomShips ? 0 : FleetSize(level, random);

            var ships = ShipBuildQuery.EnemyShips(_database).
				Common().
				FilterByStarDistance(level, ShipBuildQuery.FilterMode.SizeAndDifficulty).
				Where(item => factionFilter.IsSuitableForFleet(item.Ship.Faction)).
				SelectRandom(numberOfShips, random).
				Concat(enemy.SpecificShips);

            return ships.All.ToList().Shuffle(random);
        }

        // TODO: move to database
        private static int FleetSize(int distance, System.Random random)
        {
            var max = 3 + random.Next(100) * random.Next(100) / 2000;
            return 1 + System.Math.Min(max, distance / 3);
        }
    }
}
