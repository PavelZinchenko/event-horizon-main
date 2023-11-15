using System.Collections.Generic;
using System.Linq;
using Economy;
using Economy.ItemType;
using Economy.Products;
using GameServices.Player;

namespace GameModel
{
    namespace Quests
	{
		public class BlackMarketPlayerInventory : IInventory
		{
		    public BlackMarketPlayerInventory(PlayerResources playerResources, ItemTypeFactory itemTypeFactory)
		    {
		        _playerResources = playerResources;
		        _itemTypeFactory = itemTypeFactory;
		    }

			public void Refresh() {}

			public IEnumerable<IProduct> Items
			{
				get
				{
#if IAP_DISABLED
					return Enumerable.Empty<IProduct>();
#else
				    var itemType = _itemTypeFactory.CreateCurrencyItem(Currency.Stars);
                    if (_playerResources.Stars > 0)
                        yield return new PlayerProduct(_playerResources, itemType, itemType.MaxItemsToWithdraw);
#endif
                }
            }
			
			//public int Money { get { return Game.Session.Player.Money; } }

		    private readonly PlayerResources _playerResources;
		    private readonly ItemTypeFactory _itemTypeFactory;

		}
	}
}
