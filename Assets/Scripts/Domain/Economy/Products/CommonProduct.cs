using Economy.ItemType;
using GameServices.Player;
using Session;

namespace Economy.Products
{
    public interface IQuantityProvider
    {
        int Quantity { get; }
        bool TryConsume(int amount);
        bool TryWithdraw(int amount);
    }

    public interface IPriceProvider
    {
        Price Price { get; }
        bool TryBuy(int amount);
        bool TrySell(int amount);
    }

    public class CommonProduct : IProduct
    {
        private readonly IItemType _itemType;
        private readonly IPriceProvider _priceProvider;
        private readonly IQuantityProvider _quantityProvider;

        public static IProduct Create(IItemType itemType, int amount = 1) => 
            new CommonProduct(itemType, new QuantityProvider(amount), NullPriceProvider.Instance);

        public CommonProduct(IItemType itemType, IQuantityProvider quantityProvider, IPriceProvider priceProvider)
        {
            _itemType = itemType;
            _priceProvider = priceProvider;
            _quantityProvider = quantityProvider;
        }

        public IItemType Type => _itemType;
        public int Quantity => _quantityProvider.Quantity;
        public Price Price => _priceProvider.Price;

        public void Buy(int amount = 1)
        {
            if (!_priceProvider.TryBuy(amount))
            {
                GameDiagnostics.Trace.LogError("Not enough money");
                return;
            }

            if (!_quantityProvider.TryConsume(amount))
            {
                GameDiagnostics.Trace.LogError("Item can't be purchases");
                return;
            }

            _itemType.Consume(amount);
        }

        public void Sell(int amount = 1)
        {
            if (!_priceProvider.TrySell(amount))
            {
                GameDiagnostics.Trace.LogError("Not enough money");
                return;
            }

            if (!_quantityProvider.TryWithdraw(amount))
            {
                GameDiagnostics.Trace.LogError("Item can't be sold");
                return;
            }

            _itemType.Withdraw(amount);
        }
    }

    public class MarketPriceProvider : IPriceProvider
    {
        private readonly PlayerResources _playerResources;
        private readonly Price _price;

        public MarketPriceProvider(PlayerResources playerResources, IItemType itemType, float priceScale)
        {
            _playerResources = playerResources;
            _price = itemType.Price * priceScale;
        }

        public MarketPriceProvider(PlayerResources playerResources, Price price)
        {
            _playerResources = playerResources;
            _price = price;
        }

        public Price Price => _price;

        public bool TryBuy(int amount)
        {
            if (amount <= 0) return false;
            return _price.TryWithdraw(_playerResources);
        }

        public bool TrySell(int amount)
        {
            if (amount <= 0) return false;
            _price.Consume(_playerResources);
            return true;
        }
    }

    public class PlayerInventoryPriceProvider : IPriceProvider
    {
        private readonly PlayerResources _playerResources;
        private readonly Price _price;

        public PlayerInventoryPriceProvider(PlayerResources playerResources, IItemType itemType, int inversedPriceScale)
        {
            _playerResources = playerResources;
            _price = itemType.Price / inversedPriceScale;
        }

        public Price Price => _price;

        public bool TryBuy(int amount) => false;

        public bool TrySell(int amount)
        {
            if (amount <= 0) return false;
            _price.Consume(_playerResources);
            return true;
        }
    }

    public class QuantityProvider : IQuantityProvider
    {
        private ObscuredInt _quantity;

        public int Quantity => _quantity;

        public QuantityProvider(int quantity)
        {
            _quantity = quantity;
        }

        public bool TryConsume(int amount)
        {
            if (amount < 0) return false;
            _quantity -= amount;
            return true;
        }

        public bool TryWithdraw(int amount)
        {
            if (amount < 0) return false;
            if (_quantity < amount) return false;
            _quantity += amount;
            return true;
        }
    }

    public class MarketQuantityProvider : IQuantityProvider
    {
        private readonly ISessionData _session;
        private readonly string _itemId;
        private readonly int _marketId;
        private readonly int _maxQuantity;
        private ObscuredInt _purchasedAmount;

        public int Quantity => _maxQuantity > _purchasedAmount ? _maxQuantity - _purchasedAmount : 0;

        public MarketQuantityProvider(ISessionData session, int marketId, int maxQuantity, string itemId, long renewalTime)
        {
            _session = session;
            _maxQuantity = maxQuantity;
            _itemId = itemId;
            _marketId = marketId;

            var purchase = session.Shop.GetPurchase(marketId, itemId);
            _purchasedAmount = _session.Shop.NumberOfPurchasedItems(purchase, renewalTime);
        }

        public bool TryConsume(int amount)
        {
            if (amount < 0) return false;
            if (Quantity < amount) return false;
            _purchasedAmount += amount;
            _session.Shop.SetPurchase(_marketId, _itemId, _purchasedAmount);
            return true;
        }

        public bool TryWithdraw(int amount) => false;
    }

    public class NullPriceProvider : IPriceProvider
    {
        public Price Price => Price.Common(0);
        public bool TryBuy(int amount) => true;
        public bool TrySell(int amount) => true;

        public static readonly IPriceProvider Instance = new NullPriceProvider();
    }

    public class ConstantQuantityProvider : IQuantityProvider
    {
        public int Quantity { get; }

        public ConstantQuantityProvider(int quantity = 1)
        {
            Quantity = quantity;
        }

        public bool TryConsume(int amount)
        {
            if (amount < 0 || Quantity < amount) return false;
            return true;
        }

        public bool TryWithdraw(int amount) => false;
    }
}
