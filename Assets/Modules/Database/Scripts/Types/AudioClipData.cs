namespace GameDatabase.Model
{
    public enum AudioFormat
    {
        None,
        Wav,
        Ogg,
    }

    // Encoded audio. Database.Unity turns it into an AudioClip with ToAudioClip().
    public interface IAudioClipData
    {
        AudioFormat Format { get; }

        // Returns the encoded bytes, or null when there is no audio.
        // May release the bytes, so read it once and cache the result (ToAudioClip() does).
        byte[] LoadData();
    }

    public class AudioClipData : IAudioClipData
    {
        public static AudioClipData Empty = new(null, AudioFormat.None);

        private byte[] _data;

        public AudioFormat Format { get; }

        public AudioClipData(byte[] data, AudioFormat format)
        {
            _data = data;
            Format = format;
        }

        public byte[] LoadData()
        {
            var data = _data;
            _data = null;
            return data;
        }
    }
}
