using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameModel.Quests;
using GameServices.Economy;
using GameServices.Player;

namespace Combat.Domain
{
    public interface ICombatModel
    {
        CombatRulesAdapter Rules { get; }

        IReward GetReward(LootGenerator lootGenerator, PlayerSkills playerSkills, Galaxy.Star currentStar);

        IFleetModel PlayerFleet { get; }
        IFleetModel EnemyFleet { get; }
    }

    public static class CombatModelExtensions
    {
        public static bool IsCompleted(this ICombatModel combatModel)
        {
            return !combatModel.EnemyFleet.IsAnyShipAlive() || !combatModel.PlayerFleet.IsAnyShipAlive();
        }

        public static bool IsVictory(this ICombatModel combatModel)
        {
            if (!combatModel.PlayerFleet.IsAnyShipAlive())
                return false;
            if (combatModel.EnemyFleet.IsAnyShipAlive())
                return false;

            return true;
        }

        public static bool IsLootAllowed(this ICombatModel combatModel)
        {
            switch (combatModel.Rules.LootCondition)
            {
                case RewardCondition.Always:
                    return true;
                case RewardCondition.Default:
                    return combatModel.IsVictory();
                case RewardCondition.Never:
                default:
                    return false;
            }
        }

        public static bool IsExpAllowed(this ICombatModel combatModel)
        {
            switch (combatModel.Rules.ExpCondition)
            {
                case RewardCondition.Always:
                    return true;
                case RewardCondition.Default:
                    return combatModel.IsVictory();
                case RewardCondition.Never:
                default:
                    return false;
            }
        }
    }
}
