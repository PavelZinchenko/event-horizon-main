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
	public class UpgradesData : IDataChangedCallback
	{
		private readonly IDataChangedCallback _parent;
		private ObscuredInt _resetCounter;
		private ObscuredLong _playerExperience;
		private ObservableSet<int> _skills;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public UpgradesData(IDataChangedCallback parent)
		{
			_parent = parent;
			_resetCounter = default(int);
			_playerExperience = default(long);
			_skills = new ObservableSet<int>(this);
		}

		public UpgradesData(SessionDataReader reader, IDataChangedCallback parent)
		{
			_resetCounter = reader.ReadInt(EncodingType.EliasGamma);
			_playerExperience = reader.ReadLong(EncodingType.EliasGamma);
			int skillsItemCount;
			skillsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_skills = new ObservableSet<int>(this);
			for (int i = 0; i < skillsItemCount; ++i)
			{
				int item;
				item = reader.ReadInt(EncodingType.EliasGamma);
				_skills.Add(item);
			}
			_parent = parent;
			DataChanged = false;
		}

		public int ResetCounter
		{
			get => _resetCounter;
			set
			{
				if (_resetCounter == value) return;
				_resetCounter = value;
				OnDataChanged();
			}
		}
		public long PlayerExperience
		{
			get => _playerExperience;
			set
			{
				if (_playerExperience == value) return;
				_playerExperience = value;
				OnDataChanged();
			}
		}
		public ObservableSet<int> Skills => _skills;

		public void Serialize(SessionDataWriter writer)
		{
				writer.WriteInt(_resetCounter, EncodingType.EliasGamma);
				writer.WriteLong(_playerExperience, EncodingType.EliasGamma);
				writer.WriteInt(_skills.Count, EncodingType.EliasGamma);
				foreach (var item in _skills.Items)
				{
					writer.WriteInt(item, EncodingType.EliasGamma);
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
