using System.Collections.Generic;
using System.Linq;
using Economy.ItemType;
using Economy.Products;
using GameServices.Player;
using GameDatabase;
using Constructor.Extensions;

namespace GameModel
{
    namespace Quests
	{
		public class PlayerInventory : IInventory
		{
		    public PlayerInventory(
                ProductFactory productFactory,
                GameServices.Player.PlayerInventory inventory, 
                PlayerFleet fleet, 
                PlayerResources playerResources, 
                ItemTypeFactory factory, 
                IDatabase database)
		    {
		        _inventory = inventory;
                _productFactory = productFactory;
                _playerResources = playerResources;
		        _fleet = fleet;
		        _factory = factory;
		        _database = database;
		    }

			public void Refresh() {}

			public IEnumerable<IProduct> Items
			{
				get
				{
				    foreach (var item in _playerResources.Resources)
				    {
				        var resource = _database.GetQuestItem(item);
				        if (resource == null || resource.Price == 0) continue;
				        var quantity = _playerResources.GetResource(item);
				        yield return _productFactory.CreatePlayerProduct(_factory.CreateArtifactItem(resource), quantity);
				    }

					foreach (var item in _inventory.Components.Items)
						yield return _productFactory.CreatePlayerProduct(_factory.CreateComponentItem(item.Key), item.Value);
					foreach (var item in _inventory.Satellites.Items)
						yield return _productFactory.CreatePlayerProduct(_factory.CreateSatelliteItem(item.Key), item.Value);

					foreach (var item in _fleet.Ships.Where(ShipExtensions.CanBeSold).Except(_fleet.GetAllHangarShips()))
						yield return _productFactory.CreatePlayerProduct(_factory.CreatePlayerShipItem(item));
				}
			}

            private readonly ProductFactory _productFactory;
		    private readonly GameServices.Player.PlayerInventory _inventory;
		    private readonly IDatabase _database;
            private readonly PlayerFleet _fleet;
            private readonly PlayerResources _playerResources;
            private readonly ItemTypeFactory _factory;
        }
	}
}
