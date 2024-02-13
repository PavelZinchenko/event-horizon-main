using GameServices.Player;
using Services.Localization;
using GameDatabase.Model;
using UnityEngine;
using Zenject;
using Gui.Theme;

namespace Economy.ItemType
{
    public class SnowflakesItem : IItemType
    {
        [Inject]
        public SnowflakesItem(PlayerResources playerResources, ILocalization localization)
        {
            _localization = localization;
            _playerResources = playerResources;
        }

        public string Id => "snowflake";
        public string Name => _localization.GetString("$Snowflakes");
        public string Description => string.Empty;
        public SpriteId Icon => new("Textures/Artifacts/snowflake", SpriteId.Type.Default);
        public Color Color => UiTheme.Current.GetCurrencyColor(Currency.Snowflakes);
        public Price Price => new(1, Currency.Snowflakes);
        public ItemQuality Quality => ItemQuality.Common;

        public bool IsEqual(IItemType other) { return other.GetType() == GetType(); }

        public void Consume(int amount)
        {
            _playerResources.Snowflakes += amount;
        }

        public void Withdraw(int amount)
        {
            _playerResources.Snowflakes -= amount;
        }

        public int MaxItemsToConsume => int.MaxValue;

        public int MaxItemsToWithdraw => _playerResources.Snowflakes;

        private readonly PlayerResources _playerResources;
        private readonly ILocalization _localization;
    }
}
