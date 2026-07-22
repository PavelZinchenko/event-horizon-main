using System.Collections.Generic;
using System.Linq;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Economy.Products;
using GameDatabase;
using GameDatabase.DataModel;
using GameServices.Player;
using Model.Military;
using Constructor.Ships;
using Zenject;

namespace Combat.Domain
{
    public interface ICombatModelBuilder
    {
        ICombatModel Build(IEnumerable<IProduct> specialLoot = null);
        IFleet PlayerFleet { get; }
        IFleet EnemyFleet { get; }
    }

    public class CombatModelBuilder : ICombatModelBuilder
    {
        public CombatModelBuilder(IDatabase database, PlayerSkills playerSkills, ShipDestroyedSignal shipDestroyedSignal)
        {
            _database = database;
            _playerSkills = playerSkills;
            _shipDestroyedSignal = shipDestroyedSignal;

            Rules = database.CombatSettings.DefaultCombatRules;
        }

        public IFleet EnemyFleet { get; set; }
        public IFleet PlayerFleet { get; set; }
        public IFleet AllyFleet { get; set; }

        public CombatRules Rules { get; set; }
        public int StarLevel { get; set; }
        public ShipBuild DefenseStarbaseBuild { get; set; }
        public int DefenseStarbaseLevel { get; set; }
        public int? EnemyFactionIdOverride { get; set; }

        public void AddSpecialReward(IProduct item)
        {
            _specialReward.Add(item);
        }

        public void AddSpecialReward(IEnumerable<IProduct> items)
        {
            _specialReward.AddRange(items);
        }

        public ICombatModel Build(IEnumerable<IProduct> specialLoot = null)
        {
            var playerFleet = PlayerFleet ?? Model.Factories.Fleet.Empty;
            var enemyFleet = EnemyFleet ?? Model.Factories.Fleet.Empty;
            var allyFleet = AllyFleet ?? Model.Factories.Fleet.Empty;
            var playerShips = playerFleet.Ships.ToArray();
            var collaborativeShips = ThreeBodySkillState.CollaborativeCombatUnlocked && playerFleet is Model.Military.PlayerFleet
                ? playerShips
                : Enumerable.Empty<Constructor.Ships.IShip>();
            var allAllyShips = allyFleet.Ships.Concat(collaborativeShips).ToArray();
            var useBonuses = !Rules.DisableSkillBonuses;

            var model = new CombatModel(
                new FleetModel(playerShips, UnitSide.Player, _database, playerFleet.AiLevel, useBonuses ? _playerSkills : null),
                new FleetModel(allAllyShips, UnitSide.Ally, _database, allyFleet.AiLevel, null, collaborativeShips),
                new FleetModel(enemyFleet.Ships, UnitSide.Enemy, _database, enemyFleet.AiLevel,
                    factionIdOverride: EnemyFactionIdOverride), _shipDestroyedSignal);

			var rules = Rules.Create(StarLevel, _playerSkills.HasRescueUnit);

			model.SpecialRewards = specialLoot != null ? _specialReward.Concat(specialLoot) : _specialReward;
			model.Rules = rules;

            if (DefenseStarbaseBuild != null && DefenseStarbaseBuild != ShipBuild.DefaultValue)
            {
                var station = new CommonShip(DefenseStarbaseBuild, _database);
                if (DefenseStarbaseLevel > 0)
                    station.Experience = Maths.Experience.FromLevel(
                        _database.GalaxySettings.EnemyLevel(DefenseStarbaseLevel));
                var stationSpec = station.CreateBuilder().Build(_database.ShipSettings);
                model.DefenseStarbase = new ShipInfo(station, stationSpec, UnitSide.Player);
                model.IsStarbaseDefense = true;
            }

            return model;
        }

        private readonly IDatabase _database;
        private readonly List<IProduct> _specialReward = new List<IProduct>();
        private readonly ShipDestroyedSignal _shipDestroyedSignal;
        private readonly PlayerSkills _playerSkills;

        public class Factory : Factory<CombatModelBuilder> { }
    }
}
