using System.Collections.Generic;
using Economy.ItemType;
using Utils;

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

    public class InAppPurchaseCompletedSignal : SmartWeakSignal { public class Trigger : TriggerBase { } }
    public class InAppPurchaseFailedSignal : SmartWeakSignal<string> { public class Trigger : TriggerBase { } }
    public class SupporterPackPurchasedSignal : SmartWeakSignal { public class Trigger : TriggerBase { } }
    public class InAppItemListUpdatedSignal : SmartWeakSignal { public class Trigger : TriggerBase { } }
}
