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
	public class GameData : IDataChangedCallback
	{
		private readonly IDataChangedCallback _parent;
		private string _gameId;
		private int _seed;
		private long _startTime;
		private long _totalPlayTime;
		private long _supplyShipStartTime;
		private int _counter;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public GameData(IDataChangedCallback parent)
		{
			_parent = parent;
			_gameId = string.Empty;
			_seed = (int)System.DateTime.UtcNow.Ticks;
			_startTime = default(long);
			_totalPlayTime = default(long);
			_supplyShipStartTime = default(long);
			_counter = default(int);
		}

		public GameData(SessionDataReader reader, IDataChangedCallback parent)
		{
			_gameId = reader.ReadString(EncodingType.EliasGamma);
			_seed = reader.ReadInt(EncodingType.EliasGamma);
			_startTime = reader.ReadLong(EncodingType.EliasGamma);
			_totalPlayTime = reader.ReadLong(EncodingType.EliasGamma);
			_supplyShipStartTime = reader.ReadLong(EncodingType.EliasGamma);
			_counter = reader.ReadInt(EncodingType.EliasGamma);
			_parent = parent;
			DataChanged = false;
		}

		public string GameId
		{
			get => _gameId;
			set
			{
				if (_gameId == value) return;
				_gameId = value;
				OnDataChanged();
			}
		}
		public int Seed
		{
			get => _seed;
			set
			{
				if (_seed == value) return;
				_seed = value;
				OnDataChanged();
			}
		}
		public long StartTime
		{
			get => _startTime;
			set
			{
				if (_startTime == value) return;
				_startTime = value;
				OnDataChanged();
			}
		}
		public long TotalPlayTime
		{
			get => _totalPlayTime;
			set
			{
				if (_totalPlayTime == value) return;
				_totalPlayTime = value;
			}
		}
		public long SupplyShipStartTime
		{
			get => _supplyShipStartTime;
			set
			{
				if (_supplyShipStartTime == value) return;
				_supplyShipStartTime = value;
				OnDataChanged();
			}
		}
		public int Counter
		{
			get => _counter;
			set
			{
				if (_counter == value) return;
				_counter = value;
				OnDataChanged();
			}
		}

		public void Serialize(SessionDataWriter writer)
		{
				writer.WriteString(_gameId, EncodingType.EliasGamma);
				writer.WriteInt(_seed, EncodingType.EliasGamma);
				writer.WriteLong(_startTime, EncodingType.EliasGamma);
				writer.WriteLong(_totalPlayTime, EncodingType.EliasGamma);
				writer.WriteLong(_supplyShipStartTime, EncodingType.EliasGamma);
				writer.WriteInt(_counter, EncodingType.EliasGamma);
			DataChanged = false;
		}

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
