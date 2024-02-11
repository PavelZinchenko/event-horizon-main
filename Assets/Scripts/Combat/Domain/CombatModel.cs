using System.Collections.Generic;
using Constructor.Ships;
using Economy.Products;
using GameDatabase.DataModel;
using GameModel.Quests;
using GameServices.Economy;
using GameServices.Player;

namespace Combat.Domain
{
    public class CombatModel : ICombatModel
    {
        public CombatModel(
            FleetModel playerFleet, 
            FleetModel enemyFleet)
        {
            _playerFleet = playerFleet;
            _enemyFleet = enemyFleet;
        }

        public CombatRulesAdapter Rules { get; set; }

        public IReward GetReward(LootGenerator lootGenerator, PlayerSkills playerSkills, Galaxy.Star currentStar)
        {
            UpdateExperienceData(_playerFleet.LastActivated());
            return new CombatReward(this, playerSkills, lootGenerator, currentStar);
        }

        public IFleetModel PlayerFleet { get { return _playerFleet; } }
        public IFleetModel EnemyFleet { get { return _enemyFleet; } }

        public IEnumerable<KeyValuePair<IShip, long>> PlayerExperience { get { return _playerExperienceData; } }
        public IEnumerable<IProduct> SpecialRewards { get; set; }

        private readonly FleetModel _playerFleet;
        private readonly FleetModel _enemyFleet;

        private void UpdateExperienceData(IShipInfo ship)
        {
            if (ship == null)
                return;

            var total = _enemyFleet.GetExpForAllShips();
            if (total <= _totalExperience)
                return;

            long exp;
            if (!_playerExperienceData.TryGetValue(ship.ShipData, out exp))
                exp = 0;

            exp += (long)((total - _totalExperience) / (1f + ship.ShipData.Model.Layout.CellCount / 100f));
            _playerExperienceData[ship.ShipData] = exp;

            _totalExperience = total;
        }

        private long _totalExperience;
        private readonly Dictionary<IShip, long> _playerExperienceData = new Dictionary<IShip, long>();
    }
}


