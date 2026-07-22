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
        IFleetModel AllyFleet { get; }
        IFleetModel EnemyFleet { get; }
        IShipInfo DefenseStarbase { get; }
        bool IsStarbaseDefense { get; }
    }

    public static class CombatModelExtensions
    {
        public static bool IsCompleted(this ICombatModel combatModel)
        {
            return !combatModel.EnemyFleet.IsAnyShipAlive() || !IsPlayerForceAlive(combatModel);
        }

        public static bool IsVictory(this ICombatModel combatModel)
        {
            if (!IsPlayerForceAlive(combatModel))
                return false;
            if (combatModel.EnemyFleet.IsAnyShipAlive())
                return false;

            return true;
        }

        private static bool IsPlayerForceAlive(ICombatModel combatModel)
        {
            return combatModel.PlayerFleet.IsAnyShipAlive() ||
                   combatModel.DefenseStarbase != null && combatModel.DefenseStarbase.Status != ShipStatus.Destroyed;
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
