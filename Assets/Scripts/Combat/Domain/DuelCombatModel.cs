using System;
using System.Collections.Generic;
using GameDatabase.DataModel;
using GameModel.Quests;
using GameServices.Economy;
using GameServices.Player;

namespace Combat.Domain
{
    // The combat model of an online duel (phase 7, step 6): both fleets as built for the match, and no loot and no
    // experience, whatever the rules record says. The duel states never ask for a reward.
    public sealed class DuelCombatModel : ICombatModel
    {
        public DuelCombatModel(CombatRulesAdapter rules, IList<IShipInfo> playerShips, IList<IShipInfo> enemyShips)
        {
            Rules = rules;
            PlayerFleet = new Fleet(playerShips);
            EnemyFleet = new Fleet(enemyShips);
        }

        public CombatRulesAdapter Rules { get; }
        public IFleetModel PlayerFleet { get; }
        public IFleetModel EnemyFleet { get; }

        public IReward GetReward(LootGenerator lootGenerator, PlayerSkills playerSkills, Galaxy.Star currentStar) =>
            throw new InvalidOperationException("An online duel gives no reward.");

        public void AddExperienceCheckpoint(Constructor.Ships.IShip ship) { }

        private sealed class Fleet : IFleetModel
        {
            public Fleet(IList<IShipInfo> ships) => Ships = ships;

            public IList<IShipInfo> Ships { get; }
            public int AiLevel => 0;
        }
    }
}
