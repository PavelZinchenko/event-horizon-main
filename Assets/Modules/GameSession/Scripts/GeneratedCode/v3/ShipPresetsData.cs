//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using Session.Utils;

namespace Session.v3
{
	public class ShipPresetsData : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableList<v3.ShipPresetInfo> _presets;

		public const int VersionMinor = 0;
		public const int VersionMajor = 3;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public ShipPresetsData(IDataChangedCallback parent)
		{
			_parent = parent;
			_presets = new ObservableList<v3.ShipPresetInfo>(this);
		}

		public ShipPresetsData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int presetsItemCount;
			presetsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_presets = new ObservableList<v3.ShipPresetInfo>(presetsItemCount, this);
			for (int i = 0; i < presetsItemCount; ++i)
			{
				v3.ShipPresetInfo item;
				item = new v3.ShipPresetInfo(reader, this);
				_presets.Add(item);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableList<v3.ShipPresetInfo> Presets => _presets;

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
