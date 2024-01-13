//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using Session.Utils;

namespace Session.Model
{
	public class FleetData : IDataChangedCallback
	{
		private readonly IDataChangedCallback _parent;
		private int _explorationShipId;
		private ObservableList<ShipInfo> _ships;
		private ObservableList<HangarSlotInfo> _hangarSlots;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public FleetData(IDataChangedCallback parent)
		{
			_parent = parent;
			_explorationShipId = -1;
			_ships = new ObservableList<ShipInfo>(this);
			_hangarSlots = new ObservableList<HangarSlotInfo>(this);
		}

		public FleetData(SessionDataReader reader, IDataChangedCallback parent)
		{
			_explorationShipId = reader.ReadInt(EncodingType.EliasGamma);
			int shipsItemCount;
			shipsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_ships = new ObservableList<ShipInfo>(shipsItemCount, this);
			for (int i = 0; i < shipsItemCount; ++i)
			{
				ShipInfo item;
				item = new ShipInfo(reader, this);
				_ships.Add(item);
			}
			int hangarSlotsItemCount;
			hangarSlotsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_hangarSlots = new ObservableList<HangarSlotInfo>(hangarSlotsItemCount, this);
			for (int i = 0; i < hangarSlotsItemCount; ++i)
			{
				HangarSlotInfo item;
				item = new HangarSlotInfo(reader, this);
				_hangarSlots.Add(item);
			}
			_parent = parent;
			DataChanged = false;
		}

		public int ExplorationShipId
		{
			get => _explorationShipId;
			set
			{
				if (_explorationShipId == value) return;
				_explorationShipId = value;
				OnDataChanged();
			}
		}
		public ObservableList<ShipInfo> Ships => _ships;
		public ObservableList<HangarSlotInfo> HangarSlots => _hangarSlots;

		public void Serialize(SessionDataWriter writer)
		{
				writer.WriteInt(_explorationShipId, EncodingType.EliasGamma);
				writer.WriteInt(_ships.Count, EncodingType.EliasGamma);
				for (int i = 0; i < _ships.Count; ++i)
				{
					_ships[i].Serialize(writer);
				}
				writer.WriteInt(_hangarSlots.Count, EncodingType.EliasGamma);
				for (int i = 0; i < _hangarSlots.Count; ++i)
				{
					_hangarSlots[i].Serialize(writer);
				}
			DataChanged = false;
		}

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
