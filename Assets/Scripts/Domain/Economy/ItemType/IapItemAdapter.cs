using System;
using GameDatabase.Model;
using Gui.Theme;
using Services.InAppPurchasing;
using UnityEngine;

namespace Economy.ItemType
{
    public class IapItemAdapter : IItemType
    {
        private readonly IIapItem _iapItem;

        public IapItemAdapter(IIapItem iapItem)
        {
            _iapItem = iapItem;
        }

        public string Id => _iapItem.Id;
        public string Name => _iapItem.Name;
        public string Description => _iapItem.Description;
        public SpriteId Icon => _iapItem.Icon;
        public Color Color => UiTheme.Current.GetCurrencyColor(Currency.Money);
        public Price Price => new Price(0, Currency.Money);
        public string PriceText => _iapItem.PriceText;
        public ItemQuality Quality => ItemQuality.Common;
        public int MaxItemsToConsume => 1;
        public int MaxItemsToWithdraw => 0;
        public void Consume(int amount) => _iapItem.Buy();
        public void Withdraw(int amount) => throw new InvalidOperationException();
    }
}
