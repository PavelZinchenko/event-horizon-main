﻿using System;
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
        public RewardCondition LootCondition;
        public RewardCondition ExpCondition;
        public bool NoRetreats;
        public bool PlayerHasOneShip;
        public int TimeLimit;
        public int Level;
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
            var level = Math.Max(0, questInfo.Level + enemy.LevelBonus);

            return new QuestEnemyData
            {
                EnemyFleet = CreateFleet(enemy, questInfo, level),
                LootCondition = enemy.LootCondition,
                ExpCondition = enemy.ExpCondition,
                NoRetreats = enemy.NoShipChanging,
                PlayerHasOneShip = enemy.PlayerHasOneShip,
                TimeLimit = enemy.CombatTimeLimit,
                Level = level,
                Seed = questInfo.Seed
            };
        }

        private IEnumerable<ShipBuild> CreateFleet(Fleet enemy, QuestInfo questInfo, int level)
        {
            var random = new Random(questInfo.Seed);
            var factionFilter = new FactionFilter(enemy.Factions, level, questInfo.Faction);

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
