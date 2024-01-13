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
	public readonly partial struct ShipInfo
	{
		private readonly int _id;
		private readonly string _name;
		private readonly long _colorScheme;
		private readonly ObscuredLong _experience;
		private readonly ObservableList<ShipComponentInfo> _components;
		private readonly ObservableList<byte> _layoutModifications;
		private readonly ObservableList<long> _modifications;
		private readonly SatelliteInfo _satellite1;
		private readonly SatelliteInfo _satellite2;

		public ShipInfo(IDataChangedCallback parent)
		{
			_id = default(int);
			_name = string.Empty;
			_colorScheme = default(long);
			_experience = default(long);
			_components = new ObservableList<ShipComponentInfo>(parent);
			_layoutModifications = new ObservableList<byte>(parent);
			_modifications = new ObservableList<long>(parent);
			_satellite1 = new SatelliteInfo(parent);
			_satellite2 = new SatelliteInfo(parent);
		}

		public ShipInfo(SessionDataReader reader, IDataChangedCallback parent)
		{
			_id = reader.ReadInt(EncodingType.EliasGamma);
			_name = reader.ReadString(EncodingType.EliasGamma);
			_colorScheme = reader.ReadLong(EncodingType.EliasGamma);
			_experience = reader.ReadLong(EncodingType.EliasGamma);
			int componentsItemCount;
			componentsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_components = new ObservableList<ShipComponentInfo>(componentsItemCount, parent);
			for (int i = 0; i < componentsItemCount; ++i)
			{
				ShipComponentInfo item;
				item = new ShipComponentInfo(reader, parent);
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
			_satellite1 = new SatelliteInfo(reader, parent);
			_satellite2 = new SatelliteInfo(reader, parent);
		}

		public int Id => _id;
		public string Name => _name;
		public long ColorScheme => _colorScheme;
		public long Experience => _experience;
		public ObservableList<ShipComponentInfo> Components => _components;
		public ObservableList<byte> LayoutModifications => _layoutModifications;
		public ObservableList<long> Modifications => _modifications;
		public SatelliteInfo Satellite1 => _satellite1;
		public SatelliteInfo Satellite2 => _satellite2;

		public void Serialize(SessionDataWriter writer)
		{
				writer.WriteInt(_id, EncodingType.EliasGamma);
				writer.WriteString(_name, EncodingType.EliasGamma);
				writer.WriteLong(_colorScheme, EncodingType.EliasGamma);
				writer.WriteLong(_experience, EncodingType.EliasGamma);
				writer.WriteInt(_components.Count, EncodingType.EliasGamma);
				for (int i = 0; i < _components.Count; ++i)
				{
					_components[i].Serialize(writer);
				}
				writer.WriteInt(_layoutModifications.Count, EncodingType.EliasGamma);
				for (int i = 0; i < _layoutModifications.Count; ++i)
				{
					writer.WriteByte(_layoutModifications[i], EncodingType.EliasGamma);
				}
				writer.WriteInt(_modifications.Count, EncodingType.EliasGamma);
				for (int i = 0; i < _modifications.Count; ++i)
				{
					writer.WriteLong(_modifications[i], EncodingType.EliasGamma);
				}
				_satellite1.Serialize(writer);
				_satellite2.Serialize(writer);
		}
	}
}
