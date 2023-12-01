using System.Text;
using System.Linq;
using UnityEngine;
using Services.Audio;

namespace Services.Audio
{
    public interface IMusicPlaylist
    {
        AudioClip GetAudioClip(AudioTrackType type);
    }

    [CreateAssetMenu(fileName = "New Playlist", menuName = "Music Playlist", order = 100)]
    public class MusicPlaylist : ScriptableObject, IMusicPlaylist
    {
        [SerializeField] private AudioClip[] _menuMusic = {};
        [SerializeField] private AudioClip[] _gameMusic = {};
        [SerializeField] private AudioClip[] _combatMusic = {};
        [SerializeField] private AudioClip[] _explorationMusic = {};

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var item in _menuMusic.Concat(_gameMusic).Concat(_combatMusic).Concat(_explorationMusic))
                sb.Append(item.name).Append(' ');

            return sb.ToString(); //(_menuMusic.Length + _gameMusic.Length + _combatMusic.Length + _explorationMusic.Length).ToString();
        }

        public AudioClip GetAudioClip(AudioTrackType type)
        {
            switch (type)
            {
                case AudioTrackType.Menu:
                    return _menuMusic.Length > 0 ? _menuMusic[_random.Next(_menuMusic.Length)] : null;
                case AudioTrackType.Game:
                    return _gameMusic.Length > 0 ? _gameMusic[_random.Next(_gameMusic.Length)] : null;
                case AudioTrackType.Combat:
                    return _combatMusic.Length > 0 ? _combatMusic[_random.Next(_combatMusic.Length)] : null;
                case AudioTrackType.Exploration:
                    return _explorationMusic.Length > 0 ? _explorationMusic[_random.Next(_explorationMusic.Length)] : null;
                default:
                    return null;
            }
        }

        private readonly System.Random _random = new System.Random();
    }
}
