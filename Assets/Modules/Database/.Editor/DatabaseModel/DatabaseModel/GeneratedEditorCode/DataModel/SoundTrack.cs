//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using EditorDatabase.Enums;
using EditorDatabase.Serializable;
using EditorDatabase.Model;

namespace EditorDatabase.DataModel
{
	public partial class SoundTrack
	{
		partial void OnDataDeserialized(SoundTrackSerializable serializable, Database database);
		partial void OnDataSerialized(ref SoundTrackSerializable serializable);

		public static SoundTrack Create(SoundTrackSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new SoundTrack(serializable, database);
		}

		public SoundTrack() {}

		private SoundTrack(SoundTrackSerializable serializable, Database database)
		{
			Audio = serializable.Audio;
			OnDataDeserialized(serializable, database);
		}

		public SoundTrackSerializable Serialize()
		{
			var serializable = new SoundTrackSerializable();
			serializable.Audio = Audio;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public string Audio;

		public static SoundTrack DefaultValue { get; private set; }
	}
}
