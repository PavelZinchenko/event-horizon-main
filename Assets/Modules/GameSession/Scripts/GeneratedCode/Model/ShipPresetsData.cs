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
	public class ShipPresetsData : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableList<Model.ShipPresetInfo> _presets;

		public const int VersionMinor = 0;
		public const int VersionMajor = 3;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public ShipPresetsData(IDataChangedCallback parent)
		{
			_parent = parent;
			_presets = new ObservableList<Model.ShipPresetInfo>(this);
		}

		public ShipPresetsData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int presetsItemCount;
			presetsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_presets = new ObservableList<Model.ShipPresetInfo>(presetsItemCount, this);
			for (int i = 0; i < presetsItemCount; ++i)
			{
				Model.ShipPresetInfo item;
				item = new Model.ShipPresetInfo(reader, this);
				_presets.Add(item);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableList<Model.ShipPresetInfo> Presets => _presets;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_presets.Count, EncodingType.EliasGamma);
			for (int i = 0; i < _presets.Count; ++i)
			{
				_presets[i].Serialize(writer);
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
