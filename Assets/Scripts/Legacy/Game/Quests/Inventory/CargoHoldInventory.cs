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
        public class CargoHoldInventory : IInventory
        {
            public CargoHoldInventory(
                ProductFactory productFactory,
                GameServices.Player.PlayerInventory inventory, 
                PlayerFleet fleet, 
                PlayerResources playerResources, 
                ItemTypeFactory factory, 
                IDatabase database)
            {
                _productFactory = productFactory;
                _inventory = inventory;
                _playerResources = playerResources;
                _database = database;
                _fleet = fleet;
                _factory = factory;
            }

            public void Refresh() { }

            public IEnumerable<IProduct> Items
            {
                get
                {
                    foreach (var item in _playerResources.Resources)
                    {
                        var resource = _database.GetQuestItem(item);
                        if (resource != null)
                            yield return _productFactory.CreatePlayerProduct(_factory.CreateArtifactItem(resource), _playerResources.GetResource(item), _priceScale);
                    }

                    foreach (var item in _inventory.Components.Items)
                        yield return _productFactory.CreatePlayerProduct(_factory.CreateComponentItem(item.Key), item.Value, _priceScale);
                    foreach (var item in _inventory.Satellites.Items)
                        yield return _productFactory.CreatePlayerProduct(_factory.CreateSatelliteItem(item.Key), item.Value, _priceScale);

                    foreach (var item in _fleet.Ships.Where(ShipExtensions.CanBeSold).Except(_fleet.GetAllHangarShips()))
                        yield return _productFactory.CreatePlayerProduct(_factory.CreatePlayerShipItem(item));
                }
            }

            private readonly ProductFactory _productFactory;
            private readonly GameServices.Player.PlayerInventory _inventory;
            private readonly PlayerFleet _fleet;
            private readonly PlayerResources _playerResources;
            private readonly ItemTypeFactory _factory;
            private readonly IDatabase _database;

            private const int _priceScale = 5;
        }
    }
}
