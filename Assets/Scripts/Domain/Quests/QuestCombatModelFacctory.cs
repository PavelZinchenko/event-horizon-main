using System.Linq;
using Economy.Products;
using Combat.Domain;
using GameDatabase;
using GameDatabase.Enums;
using GameServices.Player;

namespace Domain.Quests
{
    public class QuestCombatModelFacctory
    {
		private readonly IDatabase _database;
		private readonly CombatModelBuilder.Factory _combatModelBuilderFactory;
		private readonly PlayerFleet _playerFleet;

		public QuestCombatModelFacctory(
			IDatabase database,
			PlayerFleet playerFleet,
			CombatModelBuilder.Factory combatModelBuilderFactory)
        {
			_database = database;
			_playerFleet = playerFleet;
			_combatModelBuilderFactory = combatModelBuilderFactory;
        }

		public Model.Military.IFleet CreateEnemyFleet(QuestEnemyData enemyData)
        {
			if (enemyData.EnemyFleet == null) return null;
			return new Model.Military.QuestFleet(_database, enemyData.EnemyFleet, enemyData.Level, enemyData.Seed);
		}

        public ICombatModel CreateCombatModel(QuestEnemyData enemyData, ILoot specialLoot)
        {
			var builder = _combatModelBuilderFactory.Create();
			var rules = new Model.Military.CombatRules
			{
				StarLevel = enemyData.Level,
				RewardType = Model.Military.RewardType.Default,
				LootCondition = RewardCondition.Default,
				ExpCondition = RewardCondition.Default,
				TimeLimit = Maths.Distance.CombatTime(enemyData.Level),
				TimeoutBehaviour = Model.Military.TimeoutBehaviour.NextEnemy,
				CanSelectShips = true,
				CanCallEnemies = true,
				AsteroidsEnabled = true,
				PlanetEnabled = true,
				InitialEnemies = 1,
				MaxEnemies = 3
			};

			if (enemyData.TimeLimit > 0) rules.TimeLimit = enemyData.TimeLimit;
			rules.LootCondition = enemyData.LootCondition;
			rules.ExpCondition = enemyData.ExpCondition;
			rules.NoRetreats = enemyData.NoRetreats;
			rules.PlayerHasOneShip = enemyData.PlayerHasOneShip;

			builder.EnemyFleet = CreateEnemyFleet(enemyData);
			builder.PlayerFleet = Model.Factories.Fleet.Player(_playerFleet, _database);
			builder.Rules = rules;

			var loot = specialLoot?.Items.Select(item => CommonProduct.Create(item.Type, item.Quantity));

			return builder.Build(loot);
		}
	}
}
