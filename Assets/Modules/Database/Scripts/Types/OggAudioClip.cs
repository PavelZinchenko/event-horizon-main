using UnityEngine;
using NVorbis;

namespace GameDatabase.Model
{
    public class OggAudioClip : IAudioClipData
    {
        public static IAudioClipData Create(byte[] data)
        {
            const long maxSize = 1024*100; // clips bigger than 100 Kb will be streamed

            if (data.Length > maxSize)
                return new OggAudioStream(data);
            else
                return new OggAudioClip(data);
        }

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
                if (offset >= length)
                {
                    GameDiagnostics.Trace.LogError($"Audioclip contains more data than expected, truncating");
                    break;
                }

                audioClip.SetData(readBuffer, offset);
                offset += count / channels;
            }

            AudioClip = audioClip;
        }
    }

    public class OggAudioStream : IAudioClipData
    {
        private readonly VorbisReader _vorbisReader;
        private readonly AudioClip _audioClip;

        public AudioClip AudioClip => _audioClip;

        void OnAudioRead(float[] data)
        {
            _vorbisReader.ReadSamples(data, 0, data.Length);
        }

        void OnAudioSetPosition(int newPosition)
        {
            _vorbisReader.SeekTo(newPosition);
        }

        public OggAudioStream(byte[] data)
        {
            _vorbisReader = new VorbisReader(new System.IO.MemoryStream(data));
            var channels = _vorbisReader.Channels;
            var sampleRate = _vorbisReader.SampleRate;
            var length = _vorbisReader.TotalSamples;

            _audioClip = AudioClip.Create(string.Empty, (int)length, channels, sampleRate, true, OnAudioRead, OnAudioSetPosition);
        }
    }
}
