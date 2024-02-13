using System;
using Services.InAppPurchasing;
using GameDatabase.Model;
using Session;
using UnityEngine;
using Zenject;
using Gui.Theme;

namespace Economy.ItemType
{
    public class PurchasedStarsItem : IItemType
    {
        [Inject]
        public PurchasedStarsItem(ISessionData session, InAppPurchaseCompletedSignal.Trigger iapCompletedTrigger)
        {
            _session = session;
            _iapCompletedTrigger = iapCompletedTrigger;
        }

        public string Id => "iapstar";
        public string Name => "stars";
        public string Description => string.Empty;
        public SpriteId Icon => new("Textures/Currency/star", SpriteId.Type.Default);
        public Color Color => UiTheme.Current.GetCurrencyColor(Currency.Money);
        public Price Price => Price.Common(15000);
        public ItemQuality Quality => ItemQuality.Perfect;

        public void Consume(int amount)
        {
            _session.Purchases.PurchasedStars += amount;
            _session.Resources.Stars += amount;
            _iapCompletedTrigger.Fire();
        }

        public void Withdraw(int amount)
        {
            throw new InvalidOperationException();
        }

        public int MaxItemsToConsume => int.MaxValue;

        public int MaxItemsToWithdraw => 0;

        private readonly ISessionData _session;
        private readonly InAppPurchaseCompletedSignal.Trigger _iapCompletedTrigger;
    }

    public class SupporterPackItem : IItemType
    {
        [Inject]
        public SupporterPackItem(IapPurchaseProcessor purchaseProcessor)
        {
            _purchaseProcessor = purchaseProcessor;
        }

        public string Id => "iap_pack1";
        public string Name => "supporter pack";
        public string Description => string.Empty;
        public SpriteId Icon => new("Textures/GUI/shopping_cart.png", SpriteId.Type.Default);
        public Color Color => UiTheme.Current.GetCurrencyColor(Currency.Money);
        public Price Price => Price.Common(15000);
        public ItemQuality Quality => ItemQuality.Perfect;

        public void Consume(int amount)
        {
            _purchaseProcessor.TryProcessPurchase(IapPurchaseProcessor.SupporterPack_Id);
        }

        public void Withdraw(int amount)
        {
            throw new InvalidOperationException();
        }

        public int MaxItemsToConsume => 1;

        public int MaxItemsToWithdraw => 0;

        private readonly IapPurchaseProcessor _purchaseProcessor;
    }
}
