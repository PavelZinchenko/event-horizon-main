using System.Linq;
using Combat.Domain;
using Economy;
using Economy.ItemType;
using Economy.Products;
using GameDatabase;
using GameServices.Player;
using GameStateMachine.States;
using Services.Account;
using Services.Unity;
using Session;
using CommonComponents.Signals;
using Zenject;
using UniRx;

namespace GameServices.Multiplayer
{
    public enum Status
    {
        Unknown,
        NotLoggedIn,
        ShipNotAllowed,
        Ready,
        Connecting,
        ConnectionError,
    }

    public class OfflineMultiplayer : GameServiceBase
    {
        [Inject] private readonly ICoroutineManager _coroutineManager;
        [Inject] private readonly MultiplayerStatusChangedSignal.Trigger _multiplayerStatusChangedTrigger;
        [Inject] private readonly EnemyFleetLoadedSignal.Trigger _enemyFleetLoadedTrigger;
        [Inject] private readonly EnemyFoundSignal.Trigger _enemyFoundTrigger;
        [Inject] private readonly StartBattleSignal.Trigger _startBattleTrigger;
        [Inject] private readonly CombatModelBuilder.Factory _combatModelBuilderFactory;
        [Inject] private readonly ItemTypeFactory _factory;

        [Inject]
        public OfflineMultiplayer(
            SessionDataLoadedSignal dataLoadedSignal,
            SessionCreatedSignal sessionCreatedSignal,
            AccountStatusChangedSignal accountStatusChangedSignal,
            IAccount account,
            PlayerFleet playerFleet,
            PlayerSkills playerSkills,
            IDatabase database,
            ISessionData session)
            : base(dataLoadedSignal, sessionCreatedSignal)
        {
            _account = account;
            _accountStatusChangedSignal = accountStatusChangedSignal;
            _accountStatusChangedSignal.Event += OnAccountStatusChanged;
            _timeManager = new ArenaTimeManager(session);
            _player = new PlayerInfo(playerFleet, playerSkills, database, account, _timeManager);
        }

        public IPlayerInfo Player { get { return _player; } }

        public Status Status
        {
            get { return _status; }
            set
            {
                if (_status == value)
                    return;

                _status = value;
                _multiplayerStatusChangedTrigger.Fire(_status);
            }
        }

        public int AvailableBattles { get { return _timeManager.AvailableBattles; } }
        public long TimeToNextBattle { get { return _timeManager.TimeToNextBattle; } }

        public void BuyMoreTickets(int count = 0)
        {
            _timeManager.IncreaseAvailableBattles(count);
        }

        public void UpdateStatus()
        {
            if (Status == Status.Connecting) return;

            if (_account.Status != Services.Account.Status.Connected)
            {
                Status = Status.NotLoggedIn;
                return;
            }

#if !UNITY_EDITOR
            if (!_player.CheckFleet())
            {
                Status = Status.ShipNotAllowed;
                return;
            }
#endif

            Status = Status.Connecting;
            _player.GetInfoObservable().Subscribe(result => Status = result ? Status.Ready : Status.ConnectionError);
        }

        public void FindOpponent()
        {
            if (_account.Status != Services.Account.Status.Connected || Status != Status.Ready) return;

            Status = Status.Connecting;
            _player.FindOpponentObservable().Subscribe(result =>
            {
                if (result == null)
                {
                    Status = Status.ConnectionError;
                    return;
                }

                Status = Status.Ready;
                _enemyFoundTrigger.Fire(result);
            });
        }

        public void PrepareToFight(IPlayerInfo player)
        {
            if (_account.Status != Services.Account.Status.Connected || _status != Status.Ready || player == null)
                return;

            Status = Status.Connecting;
            player.LoadFleetObservable().Subscribe(result =>
            {
                if (result)
                {
                    if (player.Id > 0) _timeManager.DecreaseAvailableBattles();
                    Status = Status.Ready;
                    _enemyFleetLoadedTrigger.Fire(player);
                }
                else
                {
                    Status = Status.ConnectionError;
                }
            });
        }

        public void Fight(IPlayerInfo enemy)
        {
            var random = new System.Random();
            var playerFleet = new Model.Military.ArenaFleet(_player.Fleet.OrderBy(item => random.Next()));
            var enemyFleet = new Model.Military.ArenaFleet(enemy.Fleet.OrderBy(item => random.Next()), enemy.PowerMultiplier);

            var price = RatingCalculator.GetTokensForBattle(enemy.Rating);

            var builder = _combatModelBuilderFactory.Create();
            builder.PlayerFleet = playerFleet;
            builder.EnemyFleet = enemyFleet;
            builder.Rules = Model.Factories.CombatRules.Arena();

            if (enemy.Id > 0)
                builder.AddSpecialReward(new Product(_factory.CreateCurrencyItem(Currency.Tokens), price));

            _startBattleTrigger.Fire(builder.Build(), model => OnCombatCompleted(enemy, model));
            Status = Status.Ready;
        }


        protected override void OnSessionDataLoaded()
        {
            _player.Reset();
            _status = Status.Unknown;
        }

        protected override void OnSessionCreated()
        {
        }

        private void OnAccountStatusChanged(Services.Account.Status status)
        {
            if (status != Services.Account.Status.Connected)
            {
                _player.Reset();
                _status = Status.NotLoggedIn;
            }
        }

        private void OnCombatCompleted(IPlayerInfo enemy, ICombatModel combatModel)
        {
            Status = Status.Connecting;
            _player.CombatCompletedObservable(enemy, combatModel).Subscribe(result =>
            {
                if (!result)
                {
                    Status = Status.ConnectionError;
                    return;
                }

                Status = Status.Ready;
            });
        }

        private Status _status = Status.Unknown;
        private readonly ArenaTimeManager _timeManager;
        private readonly PlayerInfo _player;
        private readonly IAccount _account;
        private readonly AccountStatusChangedSignal _accountStatusChangedSignal;
    }

    public class MultiplayerStatusChangedSignal : SmartWeakSignal<MultiplayerStatusChangedSignal, Status> {}
    public class EnemyFleetLoadedSignal : SmartWeakSignal<EnemyFleetLoadedSignal, IPlayerInfo> {}
    public class EnemyFoundSignal : SmartWeakSignal<EnemyFoundSignal, IPlayerInfo> {}
}
