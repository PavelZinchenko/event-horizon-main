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
	public partial class MusicPlaylist
	{
		partial void OnDataDeserialized(MusicPlaylistSerializable serializable, Database database);
		partial void OnDataSerialized(ref MusicPlaylistSerializable serializable);

		public static MusicPlaylist Create(MusicPlaylistSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new MusicPlaylist(serializable, database);
		}

		private MusicPlaylist(MusicPlaylistSerializable serializable, Database database)
		{
			MainMenuMusic = serializable.MainMenuMusic?.Select(item => SoundTrack.Create(item, database)).ToArray();
			GalaxyMapMusic = serializable.GalaxyMapMusic?.Select(item => SoundTrack.Create(item, database)).ToArray();
			CombatMusic = serializable.CombatMusic?.Select(item => SoundTrack.Create(item, database)).ToArray();
			ExplorationMusic = serializable.ExplorationMusic?.Select(item => SoundTrack.Create(item, database)).ToArray();
			OnDataDeserialized(serializable, database);
		}

		public void Save(MusicPlaylistSerializable serializable)
		{
			if (MainMenuMusic == null || MainMenuMusic.Length == 0)
			    serializable.MainMenuMusic = null;
			else
			    serializable.MainMenuMusic = MainMenuMusic.Select(item => item.Serialize()).ToArray();
			if (GalaxyMapMusic == null || GalaxyMapMusic.Length == 0)
			    serializable.GalaxyMapMusic = null;
			else
			    serializable.GalaxyMapMusic = GalaxyMapMusic.Select(item => item.Serialize()).ToArray();
			if (CombatMusic == null || CombatMusic.Length == 0)
			    serializable.CombatMusic = null;
			else
			    serializable.CombatMusic = CombatMusic.Select(item => item.Serialize()).ToArray();
			if (ExplorationMusic == null || ExplorationMusic.Length == 0)
			    serializable.ExplorationMusic = null;
			else
			    serializable.ExplorationMusic = ExplorationMusic.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public SoundTrack[] MainMenuMusic;
		public SoundTrack[] GalaxyMapMusic;
		public SoundTrack[] CombatMusic;
		public SoundTrack[] ExplorationMusic;

		public static MusicPlaylist DefaultValue { get; private set; }
	}
}
