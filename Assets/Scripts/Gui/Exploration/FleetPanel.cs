using System;
using System.Collections.Generic;
using System.Linq;
using GameServices.Player;
using Services.Localization;
using Constructor.Ships;
using GameDatabase;
using GameDatabase.Enums;
using Gui.StarMap;
using Services.Resources;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Gui.Exploration
{
    public class FleetPanel : MonoBehaviour
    {
        [Inject] private readonly PlayerFleet _playerFleet;
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly IResourceLocator _resourceLocator;

        [SerializeField] private ListScrollRect _shipList;
        [SerializeField] private ShipListContentFiller _shipListContentFiller;

        [SerializeField] private ShipSelectedEvent _shipSelectedEvent = new ShipSelectedEvent();

        [Serializable]
        public class ShipSelectedEvent : UnityEvent<IShip> { }

        public void Initialize()
        {
            var availableShips = _playerFleet.Ships.Where(IsShipAllowed).OrderBy(ship => ship.Id.Value).ToList();
            var selected = availableShips.Contains(_playerFleet.ExplorationShip)
                ? _playerFleet.ExplorationShip
                : availableShips.FirstOrDefault();
            _playerFleet.ExplorationShip = selected;
            _shipListContentFiller.SelectedShip = selected;
            _shipListContentFiller.InitializeShips(availableShips);
            _shipList.RefreshContent();

            var selectedIndex = _shipListContentFiller.SelectedShipIndex;
            if (selectedIndex >= 0)
                _shipList.ScrollToListItem(selectedIndex);
        }

        public void OnItemSelected(ShipListItem ship)
        {
            _shipListContentFiller.SelectedShip = ship.Ship;
            _shipSelectedEvent.Invoke(ship.Ship);
        }

        private static bool IsShipAllowed(IShip ship)
        {
            // Exploration used to silently require a frigate. That leaves the
            // infected hive panel with no selectable ship for many valid fleets.
            // Any mobile owned ship can carry an exploration team; stations cannot.
            return ship != null && ship.Model.SizeClass != SizeClass.Starbase;
        }
    }
}
