﻿namespace Services.Audio
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
        void Play(AudioTrackType track);
        float Volume { get; set; }
		void Mute(bool mute);
		//void Pause();
		//void Resume();
    }

	public class MusicPlayerStub : IMusicPlayer
	{
		public float Volume { get; set; }
		public void Mute(bool mute) {}
		public void Play(AudioTrackType track) {}
	}
}
