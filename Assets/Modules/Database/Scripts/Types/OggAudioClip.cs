using UnityEngine;
using NVorbis;

namespace GameDatabase.Model
{
    public class OggAudioClip : IAudioClipData
    {
        public AudioClip AudioClip { get; }

        public OggAudioClip(byte[] data)
        {
            using var vorbis = new VorbisReader(new System.IO.MemoryStream(data));
            var channels = vorbis.Channels;
            var sampleRate = vorbis.SampleRate;
            var length = vorbis.TotalSamples;

            var readBuffer = new float[channels * 1024];
            var audioClip = AudioClip.Create(string.Empty, (int)length, channels, sampleRate, false);

            int count;
            int offset = 0;
            while ((count = vorbis.ReadSamples(readBuffer, 0, readBuffer.Length)) > 0)
            {
                audioClip.SetData(readBuffer, offset);
                offset += count / channels;
            }

            AudioClip = audioClip;
        }
    }
}
