using Session.Model;

namespace Session.Content
{
	public interface IShopData
	{
		PurchaseInfo GetPurchase(int starId, string itemId);
		void SetPurchase(int starId, string itemId, int quantity);
	}

	public class ShopData : IShopData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public ShopData(SaveGameData sessionData) => _data = sessionData;

		public PurchaseInfo GetPurchase(int starId, string itemId)
		{
			if (!_data.Shop.Purchases.TryGetValue(starId, out var purchases))
				return new PurchaseInfo(_data.Shop);

			PurchaseInfo purchase;
			if (!purchases.Purchases.TryGetValue(itemId, out purchase))
				return new PurchaseInfo(_data.Shop);

			return purchase;
		}

		public void SetPurchase(int starId, string itemId, int quantity)
		{
			if (!_data.Shop.Purchases.TryGetValue(starId, out var purchases))
			{
				purchases = new PurchasesMap(_data.Shop);
				_data.Shop.Purchases.Add(starId, purchases);
			}

			purchases.Purchases.Add(itemId, new PurchaseInfo(quantity, System.DateTime.UtcNow.Ticks));
		}
	}
}
