using Constructor.Ships;
using Session.Model;
using Session.Utils;

namespace Session.Content
{
	public interface IFleetData
	{
		int ExplorationShipId { get; set; }
		ObservableList<ShipInfo> Ships { get; }
		ObservableList<HangarSlotInfo> Hangar { get; }

		ShipInfo CreateShipInfo(IShip ship); // TODO: make ShipInfo internal
	}

	public class FleetData : IFleetData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public FleetData(SaveGameData sessionData) => _data = sessionData;

		public int ExplorationShipId { get => _data.Fleet.ExplorationShipId; set => _data.Fleet.ExplorationShipId = value; }
		public ObservableList<ShipInfo> Ships => _data.Fleet.Ships;
		public ObservableList<HangarSlotInfo> Hangar => _data.Fleet.HangarSlots;

		public ShipInfo CreateShipInfo(IShip ship) => new ShipInfo(ship, _data.Fleet);
	}
}
