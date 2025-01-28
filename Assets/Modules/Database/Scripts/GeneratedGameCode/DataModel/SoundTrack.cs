//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
	public partial class SoundTrack 
	{
		partial void OnDataDeserialized(SoundTrackSerializable serializable, Database.Loader loader);

		public static SoundTrack Create(SoundTrackSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new SoundTrack(serializable, loader);
		}

		private SoundTrack(SoundTrackSerializable serializable, Database.Loader loader)
		{
			Audio = new AudioClipId(serializable.Audio);

			OnDataDeserialized(serializable, loader);
		}

		public AudioClipId Audio { get; private set; }

		public static SoundTrack DefaultValue { get; private set; }= new(new(), null);
	}
}
