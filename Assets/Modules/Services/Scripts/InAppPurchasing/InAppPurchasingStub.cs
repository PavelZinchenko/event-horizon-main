using System.Collections.Generic;
using System.Linq;

namespace Services.InAppPurchasing
{
    public class InAppPurchasingStub : IInAppPurchasing
    {
        public IEnumerable<IIapItem> GetAvailableProducts() { return Enumerable.Empty<IIapItem>(); }

        public void RestorePurchases() {}
        public bool ProcessPurchase(string id) { return false; }
        public void ConfirmRecentPurchases() {}
    }
}
