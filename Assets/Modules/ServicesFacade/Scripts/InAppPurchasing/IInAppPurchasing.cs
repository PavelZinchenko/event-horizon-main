using System.Collections.Generic;
using CommonComponents.Signals;
using GameDatabase.Model;

namespace Services.InAppPurchasing
{
	public interface IInAppPurchasing
	{
		IEnumerable<IIapItem> GetAvailableProducts();
        void RestorePurchases();
        void ConfirmRecentPurchases();
    }

    public interface IIapItem
	{
        string Id { get; }
        string Name { get; }
        string Description { get; }
        string PriceText { get; }
        SpriteId Icon { get; }
        void Buy();
    }

    public class IapException : System.Exception
    {
        public IapException() { }
        public IapException(string message) : base(message) { }
        public IapException(string message, System.Exception inner) : base(message, inner) { }
    }

    public class InAppPurchaseCompletedSignal : SmartWeakSignal<InAppPurchaseCompletedSignal> {}
    public class InAppPurchaseFailedSignal : SmartWeakSignal<InAppPurchaseFailedSignal, string> {}
    public class SupporterPackPurchasedSignal : SmartWeakSignal<SupporterPackPurchasedSignal> {}
    public class InAppItemListUpdatedSignal : SmartWeakSignal<InAppItemListUpdatedSignal> {}
}
