namespace Services.Audio
{
    public enum AudioTrackType
    {
        None,
        Menu,
        Game,
        Combat,
        Exploration,
    }

    public interface IMusicPlayer
    {
        float Volume { get; set; }
        void Stop();
        void Play(AudioTrackType track);
        IMusicPlaylist Playlist { get; set; }
    }

	public class MusicPlayerStub : IMusicPlayer
	{
		public float Volume { get; set; }
        public IMusicPlaylist Playlist { get; set; }
        public void Stop() { }
        public void Play(AudioTrackType track) {}
	}
}
