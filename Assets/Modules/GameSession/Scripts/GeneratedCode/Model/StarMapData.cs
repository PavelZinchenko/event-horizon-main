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
	public class StarMapData : IDataChangedCallback
	{
		private readonly IDataChangedCallback _parent;
		private uint _playerPosition;
		private float _mapModeZoom;
		private float _starModeZoom;
		private ObservableMap<uint, uint> _starData;
		private ObservableMap<ulong, uint> _planetData;
		private ObservableMap<uint, string> _bookmarks;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public StarMapData(IDataChangedCallback parent)
		{
			_parent = parent;
			_playerPosition = default(uint);
			_mapModeZoom = default(float);
			_starModeZoom = default(float);
			_starData = new ObservableMap<uint, uint>(this);
			_planetData = new ObservableMap<ulong, uint>(this);
			_bookmarks = new ObservableMap<uint, string>(this);
		}

		public StarMapData(SessionDataReader reader, IDataChangedCallback parent)
		{
			_playerPosition = reader.ReadUint(EncodingType.EliasGamma);
			_mapModeZoom = reader.ReadFloat(EncodingType.EliasGamma);
			_starModeZoom = reader.ReadFloat(EncodingType.EliasGamma);
			int starDataItemCount;
			starDataItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_starData = new ObservableMap<uint, uint>(this);
			for (int i = 0; i < starDataItemCount; ++i)
			{
				uint key;
				uint value;
				key = reader.ReadUint(EncodingType.EliasGamma);
				value = reader.ReadUint(EncodingType.EliasGamma);
				_starData.Add(key,value);
			}
			int planetDataItemCount;
			planetDataItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_planetData = new ObservableMap<ulong, uint>(this);
			for (int i = 0; i < planetDataItemCount; ++i)
			{
				ulong key;
				uint value;
				key = reader.ReadUlong(EncodingType.EliasGamma);
				value = reader.ReadUint(EncodingType.EliasGamma);
				_planetData.Add(key,value);
			}
			int bookmarksItemCount;
			bookmarksItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_bookmarks = new ObservableMap<uint, string>(this);
			for (int i = 0; i < bookmarksItemCount; ++i)
			{
				uint key;
				string value;
				key = reader.ReadUint(EncodingType.EliasGamma);
				value = reader.ReadString(EncodingType.EliasGamma);
				_bookmarks.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public uint PlayerPosition
		{
			get => _playerPosition;
			set
			{
				if (_playerPosition == value) return;
				_playerPosition = value;
				OnDataChanged();
			}
		}
		public float MapModeZoom
		{
			get => _mapModeZoom;
			set
			{
				if (_mapModeZoom == value) return;
				_mapModeZoom = value;
			}
		}
		public float StarModeZoom
		{
			get => _starModeZoom;
			set
			{
				if (_starModeZoom == value) return;
				_starModeZoom = value;
			}
		}
		public ObservableMap<uint, uint> StarData => _starData;
		public ObservableMap<ulong, uint> PlanetData => _planetData;
		public ObservableMap<uint, string> Bookmarks => _bookmarks;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteUint(_playerPosition, EncodingType.EliasGamma);
			writer.WriteFloat(_mapModeZoom, EncodingType.EliasGamma);
			writer.WriteFloat(_starModeZoom, EncodingType.EliasGamma);
			writer.WriteInt(_starData.Count, EncodingType.EliasGamma);
			foreach (var item in _starData.Items)
			{
				writer.WriteUint(item.Key, EncodingType.EliasGamma);
				writer.WriteUint(item.Value, EncodingType.EliasGamma);
			}
			writer.WriteInt(_planetData.Count, EncodingType.EliasGamma);
			foreach (var item in _planetData.Items)
			{
				writer.WriteUlong(item.Key, EncodingType.EliasGamma);
				writer.WriteUint(item.Value, EncodingType.EliasGamma);
			}
			writer.WriteInt(_bookmarks.Count, EncodingType.EliasGamma);
			foreach (var item in _bookmarks.Items)
			{
				writer.WriteUint(item.Key, EncodingType.EliasGamma);
				writer.WriteString(item.Value, EncodingType.EliasGamma);
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
