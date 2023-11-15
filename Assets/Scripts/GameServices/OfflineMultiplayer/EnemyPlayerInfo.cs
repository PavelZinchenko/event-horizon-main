using System;
using System.Collections.Generic;
using System.Linq;
using Constructor.Ships;
using GameDatabase;
using UniRx;

namespace GameServices.Multiplayer
{
    public class EnemyPlayerInfo : IPlayerInfo
    {
        public EnemyPlayerInfo(IDatabase database, int id, int rating, string name)
        {
            _name = name;
            _rating = rating;
            _id = id;
            _database = database;
        }

        public int Id { get { return _id; } }
        public string Name { get { return _name; } }
        public int Rating { get { return _rating; } }
        public IEnumerable<IShip> Fleet { get { return _fleet; } }
        public float PowerMultiplier { get { return 1.0f + UnityEngine.Mathf.Min(5.0f, _rating / 20000f); } }

        public IObservable<bool> LoadFleetObservable()
        {
            return _fleet.Count > 0
                ? Observable.Return<bool>(true)
                : Security.RequestFactory.CreateGetFleetRequest(_id).Select(TryLoadEnemyFleet).Catch((WWWErrorException ex) =>
                {
                    UnityEngine.Debug.LogError("LoadFleet: " + ex.RawErrorMessage);
                    return Observable.Return<bool>(false);
                });
        }

        private bool TryLoadEnemyFleet(string text)
        {
            try
            {
                _fleet.Clear();

                try
                {
                    var ships = FleetWebSerializer.DeserializeFleet(_database, text);
                    _fleet.AddRange(Id == 1 ? ships : ships.Where(ship => ShipValidator.IsAllowedOnArena(ship, _database.ShipSettings)));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Fleet deserialization failed - " + e.Message);
                }

                if (_fleet.Count == 0)
                {
                    UnityEngine.Debug.LogError("TryLoadEnemyFleet: fleet is empty - " + Id);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("TryLoadEnemyFleet.Failed: " + text);
                return false;
            }
        }

        private readonly int _id;
        private readonly int _rating;
        private readonly string _name;
        private readonly IDatabase _database;
        private readonly List<IShip> _fleet = new List<IShip>();
    }
}
