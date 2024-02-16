using System.Collections.Generic;
using Combat.Component.Unit.Classification;
using Combat.Scene;
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
        private readonly Dictionary<IShip, long> _playerExperienceData = new Dictionary<IShip, long>();
        private readonly ShipDestroyedSignal _shipDestroyedSignal;
        private readonly FleetModel _playerFleet;
        private readonly FleetModel _enemyFleet;
        private long _totalExperience;

        public CombatModel(
            FleetModel playerFleet, 
            FleetModel enemyFleet,
            ShipDestroyedSignal shipDestroyedSignal)
        {
            _playerFleet = playerFleet;
            _enemyFleet = enemyFleet;
            _shipDestroyedSignal = shipDestroyedSignal;
            _shipDestroyedSignal.Event += OnShipDestroyed;
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

        private void OnShipDestroyed(Component.Ship.IShip ship)
        {
            IShipInfo shipInfo;
            if (ship.Type.Class == UnitClass.Ship && ship.Type.Side == UnitSide.Player && _playerFleet.TryGetInfo(ship, out shipInfo))
                UpdateExperienceData(shipInfo);
        }

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
    }
}


