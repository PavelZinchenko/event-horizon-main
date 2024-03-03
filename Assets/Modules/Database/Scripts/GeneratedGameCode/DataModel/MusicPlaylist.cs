


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
	public partial class MusicPlaylist 
	{
		partial void OnDataDeserialized(MusicPlaylistSerializable serializable, Database.Loader loader);

		public static MusicPlaylist Create(MusicPlaylistSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new MusicPlaylist(serializable, loader);
		}

		private MusicPlaylist(MusicPlaylistSerializable serializable, Database.Loader loader)
		{
			MainMenuMusic = new ImmutableCollection<SoundTrack>(serializable.MainMenuMusic?.Select(item => SoundTrack.Create(item, loader)));
			GalaxyMapMusic = new ImmutableCollection<SoundTrack>(serializable.GalaxyMapMusic?.Select(item => SoundTrack.Create(item, loader)));
			CombatMusic = new ImmutableCollection<SoundTrack>(serializable.CombatMusic?.Select(item => SoundTrack.Create(item, loader)));
			ExplorationMusic = new ImmutableCollection<SoundTrack>(serializable.ExplorationMusic?.Select(item => SoundTrack.Create(item, loader)));

			OnDataDeserialized(serializable, loader);
		}

		public ImmutableCollection<SoundTrack> MainMenuMusic { get; private set; }
		public ImmutableCollection<SoundTrack> GalaxyMapMusic { get; private set; }
		public ImmutableCollection<SoundTrack> CombatMusic { get; private set; }
		public ImmutableCollection<SoundTrack> ExplorationMusic { get; private set; }

		public static MusicPlaylist DefaultValue { get; private set; }
	}
}
