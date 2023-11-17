using System;
using System.Collections.Generic;
using System.Linq;
using Combat.Component.Unit.Classification;
using Combat.Domain;
using Constructor.Ships;
using GameDatabase;
using GameServices.Player;
using Services.Account;
using UniRx;

namespace GameServices.Multiplayer
{
    public class PlayerInfo : IPlayerInfo
    {
        public PlayerInfo(PlayerFleet playerFleet, PlayerSkills playerSkills, IDatabase database, IAccount account, ArenaTimeManager timeManager)
        {
            _playerFleet = playerFleet;
            _playerSkills = playerSkills;
            _database = database;
            _account = account;
            _timeManager = timeManager;

            Reset();
        }

        public void Reset()
        {
            _rating = 100;
            _timeManager.Reset();
        }

        public int Id { get { return -1; } }
        public string Name { get { return _account.DisplayName; } }
        public int Rating { get { return _rating; } }

        public IEnumerable<IShip> Fleet { get { return _playerFleet.ActiveShipGroup.Ships; } }
        public float PowerMultiplier { get { return 1.0f; } }

        public bool CheckFleet()
        {
            return Fleet.Any() && Fleet.All(ship => ShipValidator.IsAllowedOnArena(ship, _database.ShipSettings));
        }

        public IObservable<bool> GetInfoObservable()
        {
            return Security.RequestFactory.CreateGetPlayerInfoRequest(_account.Id).Select(TryUpdate).Catch((WWWErrorException ex) =>
            {
                UnityEngine.Debug.LogError("LoadPlayerInfo: " + ex.RawErrorMessage);
                return Observable.Return(false);
            });
        }

        public IObservable<IPlayerInfo> FindOpponentObservable()
        {
#if !UNITY_EDITOR
            if (_timeManager.ShouldUpdatePlayerData)
                return RegisterPlayerObservable().SelectMany(result => result ? CreateFindOpponentObservable() : Observable.Return<IPlayerInfo>(null));
#else
            var data = FleetWebSerializer.SerializeFleet(Fleet);
#endif
            return CreateFindOpponentObservable();
        }

        public IObservable<bool> CombatCompletedObservable(IPlayerInfo opponent, ICombatModel combatModel)
        {
            if (opponent.Id <= 0) return Observable.Return(true);

            var value = combatModel.IsVictory() ? 1 : -1;
            var request = Security.RequestFactory.CreateUpdateStatsRequest(_account.Id, opponent.Id, value);
            return request.Select(TryUpdateRating).Catch((WWWErrorException ex) =>
            {
                UnityEngine.Debug.LogError("UpdateStats: " + ex.RawErrorMessage);
                return Observable.Return(false);
            });
        }

        private IObservable<IPlayerInfo> CreateFindOpponentObservable()
        {
            var request = Security.RequestFactory.CreateFindOpponentRequest(_account.Id, Rating);
            var observable = request.Select(CreateOpponent).Catch((WWWErrorException ex) =>
            {
                UnityEngine.Debug.LogError("FindOpponent: " + ex.RawErrorMessage);
                return Observable.Return<IPlayerInfo>(null);
            });

            return CheckAvailableBattlesObservable().SelectMany(result => result ? observable : Observable.Return<IPlayerInfo>(null));
        }

        private IObservable<bool> CheckAvailableBattlesObservable()
        {
            var request = Security.RequestFactory.CreateGetTimeFromLastFightRequest(_account.Id);
            return request.Select(CheckAvailableBattles).Catch((WWWErrorException ex) =>
            {
                UnityEngine.Debug.LogError("GetTimeFromLastFight: " + ex.RawErrorMessage);
                return Observable.Return(false);
            });
        }

        private bool CheckAvailableBattles(string text)
        {
            try
            {
                var timeFromLastFight = Convert.ToInt32(text) * TimeSpan.TicksPerSecond;
                _timeManager.OnLastFightTimeReceived(timeFromLastFight);
                return _timeManager.AvailableBattles > 0;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("CheckAvailableBattles.Failed: " + text);
                return false;
            }
        }

        private IObservable<bool> RegisterPlayerObservable()
        {
            var fleet = Fleet.ToArray();
            var fleetPower = RatingCalculator.CalculateFleetPower(fleet);
            var request = Security.RequestFactory.CreateRegisterPlayerRequest(_account.Id, 
                _account.DisplayName, Rating, fleetPower, FleetWebSerializer.SerializeFleet(fleet));

            return request.Select(TryUpdate).Catch((WWWErrorException ex) =>
            {
                UnityEngine.Debug.LogError("RegisterPlayer: " + ex.RawErrorMessage);
                return Observable.Return<bool>(false);
            });
        }

        public IObservable<bool> LoadFleetObservable()
        {
            return Observable.Return(true);
        }

        private IPlayerInfo CreateOpponent(string text)
        {
            try
            {
                UnityEngine.Debug.Log("CreateOpponent: " + text);

                var data = text.Split('\n');
                var id = int.Parse(data[0]);
                var rating = Convert.ToInt32(data[1]);
                var name = data[2];

                return new EnemyPlayerInfo(_database, id, rating, name);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("CreateOpponent.Failed: " + text);
                return null;
            }
        }

        private bool TryUpdateRating(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text)) return false;

                _rating = Convert.ToInt32(text);
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("UpdatePlayerRating.Failed: " + text);
                return false;
            }
        }

        private bool TryUpdate(string text)
        {
            try
            {
                Reset();

                if (string.IsNullOrEmpty(text)) return true;

                var data = text.Split('\n');
                var rating = Convert.ToInt32(data[0]);
                var airating = Convert.ToInt32(data[1]);
                var timeFromLastUpdate = Convert.ToInt32(data[2]) * TimeSpan.TicksPerSecond;
                var timeFromLastFight = Convert.ToInt32(data[3]) * TimeSpan.TicksPerSecond;

                UnityEngine.Debug.Log("player AI rating - " + airating);

                _rating = rating;
                _timeManager.OnLastFightTimeReceived(timeFromLastFight);
                _timeManager.OnLastUpdateTimeReceived(timeFromLastUpdate);

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("CreatePlayer.Failed: " + text);
                return false;
            }
        }

        private int _rating;
        private readonly ArenaTimeManager _timeManager;
        private readonly IAccount _account;
        private readonly IDatabase _database;
        private readonly PlayerSkills _playerSkills;
        private readonly PlayerFleet _playerFleet;
    }
}
