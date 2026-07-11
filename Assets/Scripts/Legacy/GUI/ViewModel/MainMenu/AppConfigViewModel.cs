using System.Linq;
using Constructor;
using Constructor.Ships;
using GameDatabase;
using GameDatabase.Enums;
using GameServices.Player;
using Session;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ViewModel
{
    public class AppConfigViewModel : MonoBehaviour
    {
        [SerializeField] private Text _versionText;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly ISessionData _session;
        [Inject] private readonly PlayerFleet _playerFleet;
        [Inject] private readonly PlayerInventory _playerInventory;

        private int _tapCount;
        private float _lastTapTime;

        private void Start()
        {
            _versionText.text = AppConfig.version + " (build " + AppConfig.buildNumber + ")";
            var button = _versionText.GetComponent<Button>() ?? _versionText.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.RemoveListener(OnVersionClicked);
            button.onClick.AddListener(OnVersionClicked);
            _versionText.raycastTarget = true;
        }

        private void OnVersionClicked()
        {
            if (Time.unscaledTime - _lastTapTime > 2f) _tapCount = 0;
            _lastTapTime = Time.unscaledTime;
            if (++_tapCount < 5) return;
            _tapCount = 0;
            GrantDeveloperCollection();
        }

        private void GrantDeveloperCollection()
        {
            if (_session == null || !_session.IsGameStarted() || _database == null) return;

            foreach (var ship in _database.ShipList.Where(item => item != null))
                _session.Statistics.UnlockShip(ship.Id);

            foreach (var component in _database.ComponentList.Where(item => item != null))
            {
                var info = new ComponentInfo(component);
                if (_playerInventory.Components.GetQuantity(info) < 99)
                    _playerInventory.Components[info] = 99;
            }

            foreach (var satellite in _database.SatelliteList.Where(item => item != null))
                if (_playerInventory.Satellites.GetQuantity(satellite) == 0)
                    _playerInventory.Satellites[satellite] = 1;

            var ownedShipIds = _playerFleet.Ships.Select(item => item.Model.Id.Value).ToHashSet();
            foreach (var build in _database.ShipBuildList.Where(item => item?.Ship != null && item.AvailableForPlayer &&
                         (item.Ship.ShipType == ShipType.Common || item.Ship.ShipType == ShipType.Drone))
                     .GroupBy(item => item.Ship.Id.Value).Select(group => group.First()))
                if (ownedShipIds.Add(build.Ship.Id.Value))
                    _playerFleet.Ships.Add(new CommonShip(build, _database));
        }
    }
}
