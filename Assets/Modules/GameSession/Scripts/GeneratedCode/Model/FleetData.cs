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
		private IDataChangedCallback _parent;

		private int _explorationShipId;
		private ObservableList<Model.ShipInfo> _ships;
		private ObservableList<Model.HangarSlotInfo> _hangarSlots;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public FleetData(IDataChangedCallback parent)
		{
			_parent = parent;
			_explorationShipId = -1;
			_ships = new ObservableList<Model.ShipInfo>(this);
			_hangarSlots = new ObservableList<Model.HangarSlotInfo>(this);
		}

		public FleetData(SessionDataReader reader, IDataChangedCallback parent)
		{
			_explorationShipId = reader.ReadInt(EncodingType.EliasGamma);
			int shipsItemCount;
			shipsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_ships = new ObservableList<Model.ShipInfo>(shipsItemCount, this);
			for (int i = 0; i < shipsItemCount; ++i)
			{
				Model.ShipInfo item;
				item = new Model.ShipInfo(reader, this);
				_ships.Add(item);
			}
			int hangarSlotsItemCount;
			hangarSlotsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_hangarSlots = new ObservableList<Model.HangarSlotInfo>(hangarSlotsItemCount, this);
			for (int i = 0; i < hangarSlotsItemCount; ++i)
			{
				Model.HangarSlotInfo item;
				item = new Model.HangarSlotInfo(reader, this);
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
		public ObservableList<Model.ShipInfo> Ships => _ships;
		public ObservableList<Model.HangarSlotInfo> HangarSlots => _hangarSlots;

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
