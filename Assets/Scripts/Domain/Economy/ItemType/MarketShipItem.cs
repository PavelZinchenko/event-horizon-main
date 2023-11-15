using GameServices.Player;
using Services.Localization;
using Constructor.Ships;
using Zenject;

namespace Economy.ItemType
{
    public class MarketShipItem : ShipItemBase
    {
        [Inject]
        public MarketShipItem(PlayerFleet playerFleet, ILocalization localization, IShip ship, bool premium = false)
            : base(localization, ship)
        {
            _playerFleet = playerFleet;
            _premium = premium;
        }

        public override Price Price
        {
            get
            {
                if (!_premium)
                    return base.Price;

                var price = Ship.Model.Layout.CellCount / 5;
                if (Ship.Model.ShipRarity != GameDatabase.Enums.ShipRarity.Normal) price *= 2;

                return Price.Premium(price);
            }
        }

        public override void Consume(int amount)
        {
            for (var i = 0; i < amount; ++i)
                _playerFleet.Ships.Add(new CommonShip(Ship.Model, Ship.Components) { Experience = Ship.Experience } );
        }

        public override void Withdraw(int amount)
        {
            throw new System.InvalidOperationException();
        }

        public override int MaxItemsToConsume => int.MaxValue;
        public override int MaxItemsToWithdraw => 0;

        private readonly bool _premium;
        private readonly PlayerFleet _playerFleet;
    }
}
