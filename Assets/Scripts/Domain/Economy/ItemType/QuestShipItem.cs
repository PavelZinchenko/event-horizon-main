using System.Linq;
using GameServices.Player;
using Services.Localization;
using Constructor.Ships;
using Zenject;

namespace Economy.ItemType
{
    public class QuestShipItem : ShipItemBase
    {
        [Inject]
        public QuestShipItem(PlayerFleet playerFleet, PlayerInventory playerInventory, ILocalization localization, IShip ship)
            : base(localization, ship)
        {
            _playerFleet = playerFleet;
            _playerInventory = playerInventory;
        }

        public override void Consume(int amount)
        {
            for (var i = 0; i < amount; ++i)
                _playerFleet.Ships.Add(new CommonShip(Ship.Model, Ship.Components) { Experience = Ship.Experience });
        }

        public override void Withdraw(int amount)
        {
            var id = Ship.Id;
            var ships = _playerFleet.Ships.Where(item => item.Id == id).OrderBy(item => item.Experience.Level).Take(amount);
            foreach (var ship in ships)
            {
                Strip(ship);
                _playerFleet.Ships.Remove(ship);
            }
        }

        public override int MaxItemsToConsume => int.MaxValue;

        public override int MaxItemsToWithdraw 
        {
            get
            {
                int count = 0;
                var id = Ship.Id;

                for (int i = 0; i < _playerFleet.Ships.Count; ++i)
                    if (_playerFleet.Ships[i].Id == id) count++;

                return count;
            }
        }

        private void Strip(IShip ship)
        {
            int index = 0;
            var components = ship.Components;
            while (index < components.Count)
            {
                var component = components[index];
                if (!component.Locked)
                {
                    _playerInventory.Components.Add(component.Info);
                    components.RemoveAt(index);
                    continue;
                }

                index++;
            }
        }

        private readonly PlayerInventory _playerInventory;
        private readonly PlayerFleet _playerFleet;
    }
}
