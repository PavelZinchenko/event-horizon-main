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
	public class IapData : IDataChangedCallback
	{
		private readonly IDataChangedCallback _parent;
		private bool _removeAds;
		private bool _supporterPack;
		private ObscuredInt _stars;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public IapData(IDataChangedCallback parent)
		{
			_parent = parent;
			_removeAds = default(bool);
			_supporterPack = default(bool);
			_stars = default(int);
		}

		public IapData(SessionDataReader reader, IDataChangedCallback parent)
		{
			_removeAds = reader.ReadBool(EncodingType.EliasGamma);
			_supporterPack = reader.ReadBool(EncodingType.EliasGamma);
			_stars = reader.ReadInt(EncodingType.EliasGamma);
			_parent = parent;
			DataChanged = false;
		}

		public bool RemoveAds
		{
			get => _removeAds;
			set
			{
				if (_removeAds == value) return;
				_removeAds = value;
				OnDataChanged();
			}
		}
		public bool SupporterPack
		{
			get => _supporterPack;
			set
			{
				if (_supporterPack == value) return;
				_supporterPack = value;
				OnDataChanged();
			}
		}
		public int Stars
		{
			get => _stars;
			set
			{
				if (_stars == value) return;
				_stars = value;
				OnDataChanged();
			}
		}

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteBool(_removeAds, EncodingType.EliasGamma);
			writer.WriteBool(_supporterPack, EncodingType.EliasGamma);
			writer.WriteInt(_stars, EncodingType.EliasGamma);
			DataChanged = false;
		}

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
