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
	public class SocialData : IDataChangedCallback
	{
		private readonly IDataChangedCallback _parent;
		private int _firstDailyRewardDate;
		private int _lastDailyRewardDate;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public SocialData(IDataChangedCallback parent)
		{
			_parent = parent;
			_firstDailyRewardDate = default(int);
			_lastDailyRewardDate = default(int);
		}

		public SocialData(SessionDataReader reader, IDataChangedCallback parent)
		{
			_firstDailyRewardDate = reader.ReadInt(EncodingType.EliasGamma);
			_lastDailyRewardDate = reader.ReadInt(EncodingType.EliasGamma);
			_parent = parent;
			DataChanged = false;
		}

		public int FirstDailyRewardDate
		{
			get => _firstDailyRewardDate;
			set
			{
				if (_firstDailyRewardDate == value) return;
				_firstDailyRewardDate = value;
				OnDataChanged();
			}
		}
		public int LastDailyRewardDate
		{
			get => _lastDailyRewardDate;
			set
			{
				if (_lastDailyRewardDate == value) return;
				_lastDailyRewardDate = value;
				OnDataChanged();
			}
		}

		public void Serialize(SessionDataWriter writer)
		{
				writer.WriteInt(_firstDailyRewardDate, EncodingType.EliasGamma);
				writer.WriteInt(_lastDailyRewardDate, EncodingType.EliasGamma);
			DataChanged = false;
		}

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
