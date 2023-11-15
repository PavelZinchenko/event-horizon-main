using System.Collections.Generic;
using System.Linq;
using Economy.ItemType;
using Economy.Products;
using Game;
using Services.Advertisements;
using Services.InAppPurchasing;

namespace GameModel.Quests
{
    public class IapInventory : IInventory
    {
        public IapInventory(ProductFactory productFactory, IInAppPurchasing iapPurchasing, IAdsManager adsManager, ItemTypeFactory itemTypeFactory, HolidayManager holidayManager)
        {
            _productFactory = productFactory;
            _inAppPurchasing = iapPurchasing;
            _adsManager = adsManager;
            _itemTypeFactory = itemTypeFactory;
            _holidayManager = holidayManager;
        }

        public void Refresh() {}

        public IEnumerable<IProduct> Items
        {
            get
            {
                var items = new List<IProduct>(_inAppPurchasing.GetAvailableProducts().Select(item => _productFactory.CreateMarketProduct(item, 1, 0)));

                if (_holidayManager.IsChristmas)
                    items.Add(_productFactory.CreateMarketProduct(_itemTypeFactory.CreateXmasBoxItem()));

                if (_adsManager.AdsEnabled)
                    items.Add(_productFactory.CreateMarketProduct(_itemTypeFactory.CreateRewardedAdItem()));

                return items;
            }
        }

        private readonly IInAppPurchasing _inAppPurchasing;
        private readonly ProductFactory _productFactory;
        private readonly IAdsManager _adsManager;
        private readonly ItemTypeFactory _itemTypeFactory;
        private readonly HolidayManager _holidayManager;
    }
}
