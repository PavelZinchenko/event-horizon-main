using System.Linq;
using Constructor;
using Constructor.Ships;
using GameDatabase;
using GameDatabase.Enums;
using GameServices.Player;
using GameServices.Research;
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
        [Inject] private readonly PlayerResources _playerResources;
        [Inject] private readonly PlayerSkills _playerSkills;
        [Inject] private readonly Research _research;

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

            // The version label is also the developer unlock gesture.  Keep
            // the content unlock from the previous implementation, but make
            // the gesture useful on a fresh save as well by filling the
            // normal player resources and progression pools.  Values are
            // intentionally applied as additions (except fuel, which is
            // filled to its current capacity) so tapping again is harmless.
            _playerResources.Money += 5_000_000;
            _playerResources.Stars += 100_000;
            _playerResources.Tokens += 10_000;
            _playerResources.Fuel = _playerSkills.MainFuelCapacity;

            foreach (var item in _database.QuestItemList.Where(item => item != null))
                _playerResources.AddResource(item.Id, 10_000);

            foreach (var faction in _database.FactionsWithEmpty.Where(item => item != null))
                _research.AddResearchPoints(faction, 10_000);

            // Player skill points are represented by the player's skill
            // experience level.  Add a generous pool without resetting points
            // already spent by the player.
            _playerSkills.Experience = GameModel.Skills.Experience.FromLevel(
                _playerSkills.Experience.Level + 100);

            var ownedShipIds = _playerFleet.Ships.Select(item => item.Model.Id.Value).ToHashSet();
            foreach (var build in _database.ShipBuildList.Where(item => item?.Ship != null && item.AvailableForPlayer &&
                         (item.Ship.ShipType == ShipType.Common || item.Ship.ShipType == ShipType.Drone))
                     .GroupBy(item => item.Ship.Id.Value).Select(group => group.First()))
                if (ownedShipIds.Add(build.Ship.Id.Value))
                    _playerFleet.Ships.Add(new CommonShip(build, _database));
        }
    }
}
