using GameServices.Player;
using Services.Localization;
using GameDatabase.Model;
using UnityEngine;
using Zenject;
using Gui.Theme;

namespace Economy.ItemType
{
    public class StarsItem : IItemType
    {
        [Inject]
        public StarsItem(PlayerResources playerResources, ILocalization localization)
        {
            _playerResources = playerResources;
            _localization = localization;
        }

        public string Id => "star";
        public string Name => _localization.GetString("$StarCurrency");
        public string Description => string.Empty;
        public SpriteId Icon => new("Textures/Currency/star", SpriteId.Type.Default);
        public Color Color => UiTheme.Current.GetCurrencyColor(Currency.Stars);
        public Price Price => Price.Common(15000);
        public ItemQuality Quality => ItemQuality.Perfect;

        public void Consume(int amount)
        {
            _playerResources.Stars += amount;
        }

        public void Withdraw(int amount)
        {
            _playerResources.Stars -= amount;
        }

        public int MaxItemsToConsume => int.MaxValue;
        public int MaxItemsToWithdraw => (int)_playerResources.Stars;

        private readonly PlayerResources _playerResources;
        private readonly ILocalization _localization;
    }
}
