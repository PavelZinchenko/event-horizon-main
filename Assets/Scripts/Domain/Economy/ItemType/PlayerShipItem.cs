using GameServices.Player;
using Services.Localization;
using Constructor.Ships;
using Zenject;

namespace Economy.ItemType
{
    public class PlayerShipItem : ShipItemBase
    {
        [Inject]
        public PlayerShipItem(PlayerFleet playerFleet, ILocalization localization, IShip ship, bool premium = false)
            : base(localization, ship)
        {
            _playerFleet = playerFleet;
            _premium = premium;
        }

        public override void Consume(int amount)
        {
            throw new System.InvalidOperationException();
        }

        public override void Withdraw(int amount)
        {
            _playerFleet.Ships.Remove(Ship);
        }

        public override int MaxItemsToConsume => 0;
        public override int MaxItemsToWithdraw => _playerFleet.Ships.Contains(Ship) ? 1 : 0;

        private readonly bool _premium;
        private readonly PlayerFleet _playerFleet;
    }
}
