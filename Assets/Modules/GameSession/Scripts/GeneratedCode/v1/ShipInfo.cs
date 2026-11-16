//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Collections.Generic;
using Session.Utils;

namespace Session.v1
{
	public readonly partial struct ShipInfo
	{
		private readonly int _id;
		private readonly string _name;
		private readonly long _colorScheme;
		private readonly ObscuredLong _experience;
		private readonly ObservableList<Model.ShipComponentInfo> _components;
		private readonly ObservableList<byte> _layoutModifications;
		private readonly ObservableList<long> _modifications;
		private readonly Model.SatelliteInfo _satellite1;
		private readonly Model.SatelliteInfo _satellite2;

		public ShipInfo(IDataChangedCallback parent)
		{
			_id = default(int);
			_name = string.Empty;
			_colorScheme = default(long);
			_experience = default(long);
			_components = new ObservableList<Model.ShipComponentInfo>(parent);
			_layoutModifications = new ObservableList<byte>(parent);
			_modifications = new ObservableList<long>(parent);
			_satellite1 = new Model.SatelliteInfo(parent);
			_satellite2 = new Model.SatelliteInfo(parent);
		}

		public ShipInfo(SessionDataReader reader, IDataChangedCallback parent)
		{
			_id = reader.ReadInt(EncodingType.EliasGamma);
			_name = reader.ReadString(EncodingType.EliasGamma);
			_colorScheme = reader.ReadLong(EncodingType.EliasGamma);
			_experience = reader.ReadLong(EncodingType.EliasGamma);
			int componentsItemCount;
			componentsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_components = new ObservableList<Model.ShipComponentInfo>(componentsItemCount, parent);
			for (int i = 0; i < componentsItemCount; ++i)
			{
				Model.ShipComponentInfo item;
				item = new Model.ShipComponentInfo(reader, parent);
				_components.Add(item);
			}
			int layoutModificationsItemCount;
			layoutModificationsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_layoutModifications = new ObservableList<byte>(layoutModificationsItemCount, parent);
			for (int i = 0; i < layoutModificationsItemCount; ++i)
			{
				byte item;
				item = reader.ReadByte(EncodingType.EliasGamma);
				_layoutModifications.Add(item);
			}
			int modificationsItemCount;
			modificationsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_modifications = new ObservableList<long>(modificationsItemCount, parent);
			for (int i = 0; i < modificationsItemCount; ++i)
			{
				long item;
				item = reader.ReadLong(EncodingType.EliasGamma);
				_modifications.Add(item);
			}
			_satellite1 = new Model.SatelliteInfo(reader, parent);
			_satellite2 = new Model.SatelliteInfo(reader, parent);
		}

		public int Id => _id;
		public string Name => _name;
		public long ColorScheme => _colorScheme;
		public long Experience => _experience;
		public ObservableList<Model.ShipComponentInfo> Components => _components;
		public ObservableList<byte> LayoutModifications => _layoutModifications;
		public ObservableList<long> Modifications => _modifications;
		public Model.SatelliteInfo Satellite1 => _satellite1;
		public Model.SatelliteInfo Satellite2 => _satellite2;
	}
}
