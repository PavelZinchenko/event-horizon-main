using GameDatabase.Model;
using UnityEngine;

namespace Services.Audio
{
	public interface ISoundPlayer
	{
		float Volume { get; set; }
		void Play(AudioClip audioClip, int soundId = 0, bool loop = false);
        void Play(AudioClipId audioClip, int soundId = 0);
        void Stop(int soundId);
	}

	public class SoundPlayerStub : ISoundPlayer
	{
		public float Volume { get; set; }
		public void Play(AudioClip audioClip, int soundId = 0, bool loop = false) {}
		public void Play(AudioClipId audioClip, int soundId = 0) {}
		public void Stop(int soundId) {}
	}
}
