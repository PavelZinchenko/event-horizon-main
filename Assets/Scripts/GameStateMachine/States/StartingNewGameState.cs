using System.Linq;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Session;
using GameServices.LevelManager;
using GameServices.Player;
using Domain.Quests;
using Constructor.Ships;
using Zenject;

namespace GameStateMachine.States
{
    public class StartingNewGameState : BaseState
    {
        [Inject]
        public StartingNewGameState(
            IStateMachine stateMachine,
            GameStateFactory stateFactory,
            ILevelLoader levelLoader,
            ISessionData session,
            IDatabase database,
            ILootItemFactory lootItemFactory,
            PlayerFleet playerFleet,
            MotherShip motherShip)
            : base(stateMachine, stateFactory, levelLoader)
        {
            _motherShip = motherShip;
            _session = session;
            _database = database;
            _playerFleet = playerFleet;
            _lootItemFactory = lootItemFactory;
        }

        public override StateType Type { get { return StateType.StartingNewGame; } }

        protected override void OnActivate()
        {
            InitializeGame();
            InitializeResources();
            InitializeInventory();
            InitializeFleet();

            StateMachine.LoadState(StateFactory.CreateStarMapState());
        }

        private void InitializeGame()
        {
            _session.Game.GameStartTime = System.DateTime.UtcNow.Ticks;
            _motherShip.ViewMode = ViewMode.StarSystem;
        }

        private void InitializeResources()
        {
            _session.Resources.Fuel = _database.SkillSettings.BaseFuelCapacity;
        }

        private void InitializeInventory()
        {
            if (_database.GalaxySettings.StartingInventory == null) return;

            var inventory = new Loot(new LootModel(_database.GalaxySettings.StartingInventory.Loot), new QuestInfo(0), _lootItemFactory, _database);
            foreach (var item in inventory.Items)
                item.Type.Consume(item.Quantity);
        }

        private void InitializeFleet()
        {
            var startingBuilds = _database.GalaxySettings.StartingShipBuilds;
            foreach (var build in startingBuilds)
            {
                var ship = new CommonShip(build);
                _playerFleet.Ships.Add(ship);
                _playerFleet.ActiveShipGroup.Add(ship);
            }

            if (_session.Purchases.SupporterPack)
                _playerFleet.AddSupporterPack();

            foreach (var ship in _playerFleet.Ships)
                foreach (var item in ship.Components)
                    item.Locked = false;

            _playerFleet.ExplorationShip = _playerFleet.Ships.FirstOrDefault(ship => ship.Model.SizeClass == SizeClass.Frigate);
        }

        private readonly ILootItemFactory _lootItemFactory;
        private readonly ISessionData _session;
        private readonly MotherShip _motherShip;
        private readonly IDatabase _database;
        private readonly PlayerFleet _playerFleet;

        public class Factory : Factory<StartingNewGameState> { }
    }
}
