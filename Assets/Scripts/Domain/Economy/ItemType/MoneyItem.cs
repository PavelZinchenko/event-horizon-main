using GameServices.Player;
using Services.Localization;
using GameDatabase.Model;
using UnityEngine;
using Zenject;
using Gui.Theme;

namespace Economy.ItemType
{
    public class MoneyItem : IItemType
    {
        [Inject]
        public MoneyItem(PlayerResources playerResources, ILocalization localization)
        {
            _playerResources = playerResources;
            _localization = localization;
        }

        public string Id => "m";
        public string Name => _localization.GetString("$Credits");
        public string Description => string.Empty;
        public SpriteId Icon => new("Textures/GUI/credit", SpriteId.Type.Default);
        public Price Price => Price.Common(1);
        public Color Color => UiTheme.Current.GetCurrencyColor(Currency.Credits);
        public ItemQuality Quality => ItemQuality.Common;

        public void Consume(int amount)
        {
            _playerResources.Money += amount;
        }

        public void Withdraw(int amount)
        {
            _playerResources.Money -= amount;
        }

        public int MaxItemsToConsume => int.MaxValue;
        public int MaxItemsToWithdraw => (int)_playerResources.Money;

        private readonly PlayerResources _playerResources;
        private readonly ILocalization _localization;
	}
}
