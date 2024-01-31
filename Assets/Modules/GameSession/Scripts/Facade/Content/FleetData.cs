using System.Linq;
using Constructor.Ships;
using Session.Model;
using Session.Utils;
using System.Collections.Generic;

namespace Session.Content
{
	public interface IFleetData
	{
		int ExplorationShipId { get; set; }
		ObservableList<ShipInfo> Ships { get; }
		ObservableList<HangarSlotInfo> Hangar { get; }

        void UpdateShips(IEnumerable<IShip> ships);
    }

    public class FleetData : IFleetData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public FleetData(SaveGameData sessionData) => _data = sessionData;

		public int ExplorationShipId { get => _data.Fleet.ExplorationShipId; set => _data.Fleet.ExplorationShipId = value; }
		public ObservableList<ShipInfo> Ships => _data.Fleet.Ships;
		public ObservableList<HangarSlotInfo> Hangar => _data.Fleet.HangarSlots;

        public void UpdateShips(IEnumerable<IShip> ships)
        {
            _data.Fleet.Ships.Assign(ships.Select(item => new ShipInfo(item, _data.Fleet)), new ShipInfo.EqualityComparer());
        }
    }
}
