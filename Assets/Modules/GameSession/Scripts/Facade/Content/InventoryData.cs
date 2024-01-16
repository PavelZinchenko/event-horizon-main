using Session.Model;
using Session.Utils;

namespace Session.Content
{
	public interface IInventoryData
	{
		ObservableInventory<InventoryComponentInfo> Components { get; }
		ObservableInventory<int> Satellites { get; }
	}

	public class InventoryData : IInventoryData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public InventoryData(SaveGameData sessionData) => _data = sessionData;

		public ObservableInventory<InventoryComponentInfo> Components => _data.Inventory.Components;
		public ObservableInventory<int> Satellites => _data.Inventory.Satellites;
	}
}
