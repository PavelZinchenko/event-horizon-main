//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using Session.Utils;

namespace Session.v1
{
	public class Inventory : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableInventory<long> _components;
		private ObservableInventory<int> _satellites;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public Inventory(IDataChangedCallback parent)
		{
			_parent = parent;
			_components = new ObservableInventory<long>(this);
			_satellites = new ObservableInventory<int>(this);
		}

		public Inventory(SessionDataReader reader, IDataChangedCallback parent)
		{
			int componentsItemCount;
			componentsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_components = new ObservableInventory<long>(this);
			for (int i = 0; i < componentsItemCount; ++i)
			{
				long value;
				int quantity;
				value = reader.ReadLong(EncodingType.EliasGamma);
				quantity = reader.ReadInt(EncodingType.EliasGamma);
				_components.Add(value,quantity);
			}
			int satellitesItemCount;
			satellitesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_satellites = new ObservableInventory<int>(this);
			for (int i = 0; i < satellitesItemCount; ++i)
			{
				int value;
				int quantity;
				value = reader.ReadInt(EncodingType.EliasGamma);
				quantity = reader.ReadInt(EncodingType.EliasGamma);
				_satellites.Add(value,quantity);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableInventory<long> Components => _components;
		public ObservableInventory<int> Satellites => _satellites;

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
