using System.Collections.Generic;
using Economy.ItemType;
using CommonComponents.Signals;

namespace Services.InAppPurchasing
{
	public interface IInAppPurchasing
	{
		IEnumerable<IIapItem> GetAvailableProducts();
        void RestorePurchases();
        void ConfirmRecentPurchases();
    }

    public interface IIapItem : IItemType
	{
		string PriceText { get; }
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
