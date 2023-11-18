using UnityEngine;

namespace GameDatabase.Model
{
    public interface IAudioClipData
    {
        public AudioClip AudioClip { get; }
    }

    public class AudioClipData : IAudioClipData
    {
        public static AudioClipData Empty = new();

        public AudioClip AudioClip { get; }

        public AudioClipData(byte[] data)
        {
            AudioClip = OpenWavParser.ByteArrayToAudioClip(data);
        }

        private AudioClipData() { }
    }
}
