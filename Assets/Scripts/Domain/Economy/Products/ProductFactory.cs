using Economy.ItemType;
using GameDatabase;
using GameDatabase.DataModel;
using GameServices.Player;
using Session;
using Zenject;

namespace Economy.Products
{
    public class ProductFactory
    {
        [Inject] private readonly PlayerResources _playerResources;
        [Inject] private readonly ISessionData _session;
        [Inject] private readonly ItemTypeFactory _itemTypeFactory;
        [Inject] private readonly IDatabase _database;

        public IProduct CreateLootItem(IItemType itemType, int quantity)
        {
            return new CommonProduct(itemType, new QuantityProvider(quantity), new NullPriceProvider());
        }

        public IProduct CreatePlayerProduct(IItemType itemType, int quantity = 1, int inversedPriceScale = 2)
        {
            var quantityProvider = new QuantityProvider(quantity);
            var priceProvider = new PlayerInventoryPriceProvider(_playerResources, itemType, inversedPriceScale);
            return new CommonProduct(itemType, quantityProvider, priceProvider);
        }

        public IProduct CreateCargoHoldProduct(IItemType itemType, int quantity = 1)
        {
            var quantityProvider = new QuantityProvider(quantity);
            var priceProvider = new PlayerInventoryPriceProvider(_playerResources, itemType, 2);
            return new CommonProduct(itemType, quantityProvider, priceProvider);
        }

        public IProduct CreateMarketProduct(IItemType itemType, int quantity = 1, float priceScale = 2f)
        {
            var quantityProvider = new QuantityProvider(quantity);
            var priceProvider = new MarketPriceProvider(_playerResources, itemType, priceScale);
            return new CommonProduct(itemType, quantityProvider, priceProvider);
        }

        public IProduct CreateIapProduct(IItemType itemType)
        {
            var quantityProvider = new ConstantQuantityProvider();
            var priceProvider = new MarketPriceProvider(_playerResources, itemType.Price);
            return new CommonProduct(itemType, quantityProvider, priceProvider);
        }

        public IProduct CreateArenaProduct(IItemType itemType, Price price)
        {
            var quantityProvider = new ConstantQuantityProvider();
            var priceProvider = new MarketPriceProvider(_playerResources, price);
            return new CommonProduct(itemType, quantityProvider, priceProvider);
        }

        public IProduct CreateRenewableMarketProduct(IItemType itemType, int quantity, int marketId, long renewalTime, float priceScale = 2f)
        {
            var quantityProvider = new MarketQuantityProvider(_session, marketId, quantity, itemType.Id, renewalTime);
            var priceProvider = new MarketPriceProvider(_playerResources, itemType, priceScale);
            return new CommonProduct(itemType, quantityProvider, priceProvider);
        }

        public IProduct CreateSpecial(IItemType itemType, Price price)
        {
            var quantityProvider = new ConstantQuantityProvider();
            var priceProvider = new MarketPriceProvider(_playerResources, price);
            return new CommonProduct(itemType, quantityProvider, priceProvider);
        }

        public bool TryCreateRandomComponentProduct(int marketId, int itemId, int itemLevel, Constructor.ComponentQuality maxQuality, Faction itemFaction, bool allowRare,
            long renewalTime, out IProduct product, bool premium = false, float priceScale = 2f)
        {
            var itemName = "component" + itemId;
            var purchase = _session.Shop.GetPurchase(marketId, itemName);
            var random = new System.Random(marketId + 123456789 * itemId + (int)purchase.Time);
            if (!Constructor.ComponentInfo.TryCreateRandomComponent(_database, itemLevel, itemFaction, random, allowRare, maxQuality, out var componentInfo))
            {
                product = null;
                return false;
            }

            var itemType = _itemTypeFactory.CreateComponentItem(componentInfo, premium);
            var quantityProvider = new MarketQuantityProvider(_session, marketId, 1, "component" + itemId, renewalTime);
            var priceProvider = new MarketPriceProvider(_playerResources, itemType, priceScale);
            product = new CommonProduct(itemType, quantityProvider, priceProvider);
            return true;
        }
    }
}
