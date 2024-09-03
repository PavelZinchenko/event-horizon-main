//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using GameDatabase.Model;
using DatabaseMigration.v1.Enums;

namespace DatabaseMigration.v1.Serializable
{
	[Serializable]
	public class MusicPlaylistSerializable : SerializableItem
	{
		public MusicPlaylistSerializable()
		{
			ItemType = ItemType.MusicPlaylist;
			FileName = "MusicPlaylist.json";
		}

		public SoundTrackSerializable[] MainMenuMusic;
		public SoundTrackSerializable[] GalaxyMapMusic;
		public SoundTrackSerializable[] CombatMusic;
		public SoundTrackSerializable[] ExplorationMusic;
	}
}
