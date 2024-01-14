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
		private IDataChangedCallback _parent;

		private int _playerPosition;
		private float _mapModeZoom;
		private float _starModeZoom;
		private ObservableMap<int, int> _starData;
		private ObservableMap<long, int> _planetData;
		private ObservableMap<int, string> _bookmarks;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public StarMapData(IDataChangedCallback parent)
		{
			_parent = parent;
			_playerPosition = default(int);
			_mapModeZoom = default(float);
			_starModeZoom = default(float);
			_starData = new ObservableMap<int, int>(this);
			_planetData = new ObservableMap<long, int>(this);
			_bookmarks = new ObservableMap<int, string>(this);
		}

		public StarMapData(SessionDataReader reader, IDataChangedCallback parent)
		{
			_playerPosition = reader.ReadInt(EncodingType.EliasGamma);
			_mapModeZoom = reader.ReadFloat(EncodingType.EliasGamma);
			_starModeZoom = reader.ReadFloat(EncodingType.EliasGamma);
			int starDataItemCount;
			starDataItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_starData = new ObservableMap<int, int>(this);
			for (int i = 0; i < starDataItemCount; ++i)
			{
				int key;
				int value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadInt(EncodingType.EliasGamma);
				_starData.Add(key,value);
			}
			int planetDataItemCount;
			planetDataItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_planetData = new ObservableMap<long, int>(this);
			for (int i = 0; i < planetDataItemCount; ++i)
			{
				long key;
				int value;
				key = reader.ReadLong(EncodingType.EliasGamma);
				value = reader.ReadInt(EncodingType.EliasGamma);
				_planetData.Add(key,value);
			}
			int bookmarksItemCount;
			bookmarksItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_bookmarks = new ObservableMap<int, string>(this);
			for (int i = 0; i < bookmarksItemCount; ++i)
			{
				int key;
				string value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadString(EncodingType.EliasGamma);
				_bookmarks.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public int PlayerPosition
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
		public ObservableMap<int, int> StarData => _starData;
		public ObservableMap<long, int> PlanetData => _planetData;
		public ObservableMap<int, string> Bookmarks => _bookmarks;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_playerPosition, EncodingType.EliasGamma);
			writer.WriteFloat(_mapModeZoom, EncodingType.EliasGamma);
			writer.WriteFloat(_starModeZoom, EncodingType.EliasGamma);
			writer.WriteInt(_starData.Count, EncodingType.EliasGamma);
			foreach (var item in _starData.Items)
			{
				writer.WriteInt(item.Key, EncodingType.EliasGamma);
				writer.WriteInt(item.Value, EncodingType.EliasGamma);
			}
			writer.WriteInt(_planetData.Count, EncodingType.EliasGamma);
			foreach (var item in _planetData.Items)
			{
				writer.WriteLong(item.Key, EncodingType.EliasGamma);
				writer.WriteInt(item.Value, EncodingType.EliasGamma);
			}
			writer.WriteInt(_bookmarks.Count, EncodingType.EliasGamma);
			foreach (var item in _bookmarks.Items)
			{
				writer.WriteInt(item.Key, EncodingType.EliasGamma);
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
