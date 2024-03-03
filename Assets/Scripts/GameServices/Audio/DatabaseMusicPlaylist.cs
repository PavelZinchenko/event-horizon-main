using GameDatabase;
using GameDatabase.Model;
using Services.Audio;
using UnityEngine;
using Utilites;

namespace GameServices.Audio
{
    public class DatabaseMusicPlaylist : IMusicPlaylist
    {
        private readonly PcgRandom _random = new();
        private readonly IDatabase _database;
        private ImmutableCollection<GameDatabase.DataModel.SoundTrack> _customCombatPlaylist;

        public DatabaseMusicPlaylist(IDatabase database)
        {
            _database = database;
        }

        public void SetCustomCombatPlaylist(ImmutableCollection<GameDatabase.DataModel.SoundTrack> customCombatPlaylist)
        {
            _customCombatPlaylist = customCombatPlaylist;
        }

        public AudioClip GetAudioClip(AudioTrackType type)
        {
            switch (type)
            {
                case AudioTrackType.Menu: return GetRandomTrack(_database.MusicPlaylist.MainMenuMusic);
                case AudioTrackType.Game: return GetRandomTrack(_database.MusicPlaylist.GalaxyMapMusic);
                case AudioTrackType.Exploration: return GetRandomTrack(_database.MusicPlaylist.ExplorationMusic);
                case AudioTrackType.Combat: return GetRandomTrack(_customCombatPlaylist.Count > 0 ? 
                    _customCombatPlaylist : _database.MusicPlaylist.CombatMusic);
            }

            return null;
        }

        private AudioClip GetRandomTrack(ImmutableCollection<GameDatabase.DataModel.SoundTrack> list)
        {
            if (list.Count == 0) return null;
            var soundTrack = list[_random.Next() % list.Count].Audio.Id;
            var audioClip = _database.GetAudioClip(soundTrack);
            return audioClip?.AudioClip;
        }
    }
}
