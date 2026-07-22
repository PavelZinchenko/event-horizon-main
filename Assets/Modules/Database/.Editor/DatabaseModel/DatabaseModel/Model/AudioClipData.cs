namespace EditorDatabase.Model
{
    public interface IAudioClipData
    {
    }

    public class AudioClipData : IAudioClipData
    {
        public static AudioClipData Empty = new AudioClipData();
    }
}
