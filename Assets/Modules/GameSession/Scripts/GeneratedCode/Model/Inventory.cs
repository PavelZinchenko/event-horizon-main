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
	public class Inventory : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableInventory<Model.InventoryComponentInfo> _components;
		private ObservableInventory<int> _satellites;

		public const int VersionMinor = 0;
		public const int VersionMajor = 2;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public Inventory(IDataChangedCallback parent)
		{
			_parent = parent;
			_components = new ObservableInventory<Model.InventoryComponentInfo>(this);
			_satellites = new ObservableInventory<int>(this);
		}

		public Inventory(SessionDataReader reader, IDataChangedCallback parent)
		{
			int componentsItemCount;
			componentsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_components = new ObservableInventory<Model.InventoryComponentInfo>(this);
			for (int i = 0; i < componentsItemCount; ++i)
			{
				Model.InventoryComponentInfo value;
				int quantity;
				value = new Model.InventoryComponentInfo(reader, this);
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

		public ObservableInventory<Model.InventoryComponentInfo> Components => _components;
		public ObservableInventory<int> Satellites => _satellites;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_components.Count, EncodingType.EliasGamma);
			foreach (var item in _components)
			{
				item.Key.Serialize(writer);
				writer.WriteInt(item.Value, EncodingType.EliasGamma);
			}
			writer.WriteInt(_satellites.Count, EncodingType.EliasGamma);
			foreach (var item in _satellites)
			{
				writer.WriteInt(item.Key, EncodingType.EliasGamma);
				writer.WriteInt(item.Value, EncodingType.EliasGamma);
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
