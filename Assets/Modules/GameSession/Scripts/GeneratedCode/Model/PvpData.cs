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
	public class PvpData : IDataChangedCallback
	{
		private readonly IDataChangedCallback _parent;
		private int _arenaFightsFromTimerStart;
		private long _arenaLastFightTime;
		private long _arenaTimerStartTime;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public PvpData(IDataChangedCallback parent)
		{
			_parent = parent;
			_arenaFightsFromTimerStart = default(int);
			_arenaLastFightTime = default(long);
			_arenaTimerStartTime = default(long);
		}

		public PvpData(SessionDataReader reader, IDataChangedCallback parent)
		{
			_arenaFightsFromTimerStart = reader.ReadInt(EncodingType.EliasGamma);
			_arenaLastFightTime = reader.ReadLong(EncodingType.EliasGamma);
			_arenaTimerStartTime = reader.ReadLong(EncodingType.EliasGamma);
			_parent = parent;
			DataChanged = false;
		}

		public int ArenaFightsFromTimerStart
		{
			get => _arenaFightsFromTimerStart;
			set
			{
				if (_arenaFightsFromTimerStart == value) return;
				_arenaFightsFromTimerStart = value;
				OnDataChanged();
			}
		}
		public long ArenaLastFightTime
		{
			get => _arenaLastFightTime;
			set
			{
				if (_arenaLastFightTime == value) return;
				_arenaLastFightTime = value;
				OnDataChanged();
			}
		}
		public long ArenaTimerStartTime
		{
			get => _arenaTimerStartTime;
			set
			{
				if (_arenaTimerStartTime == value) return;
				_arenaTimerStartTime = value;
				OnDataChanged();
			}
		}

		public void Serialize(SessionDataWriter writer)
		{
				writer.WriteInt(_arenaFightsFromTimerStart, EncodingType.EliasGamma);
				writer.WriteLong(_arenaLastFightTime, EncodingType.EliasGamma);
				writer.WriteLong(_arenaTimerStartTime, EncodingType.EliasGamma);
			DataChanged = false;
		}

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
