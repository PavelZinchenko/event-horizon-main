using GameServices.Player;
using Services.Localization;
using GameDatabase.Model;
using UnityEngine;
using Zenject;
using Gui.Theme;

namespace Economy.ItemType
{
    public class TokensItem : IItemType
    {
        [Inject]
        public TokensItem(PlayerResources playerResources, ILocalization localization)
        {
            _playerResources = playerResources;
            _localization = localization;
        }

        public string Id => "token";
        public string Name => _localization.GetString("$TokenCurrency");
        public string Description => string.Empty;
        public SpriteId Icon => new("Textures/Currency/tokens", SpriteId.Type.Default);
        public Color Color => UiTheme.Current.GetCurrencyColor(Currency.Tokens);
        public Price Price => Price.Tokens(1);
        public ItemQuality Quality => ItemQuality.Perfect;

        public void Consume(int amount)
        {
            _playerResources.Tokens += amount;
        }

        public void Withdraw(int amount)
        {
            _playerResources.Tokens -= amount;
        }

        public int MaxItemsToConsume => int.MaxValue;
        public int MaxItemsToWithdraw => _playerResources.Tokens;

        private readonly PlayerResources _playerResources;
        private readonly ILocalization _localization;
    }
}
