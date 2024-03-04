using UnityEngine;
using Services.Settings;
using Services.Assets;
using Services.GameApplication;
using Zenject;

namespace Services.Audio
{	
	public class MusicPlayer : MonoBehaviour, IMusicPlayer
	{
		[Inject]
	    private void Initialize(
			IGameSettings gameSettings, 
			IAssetLoader assetLoader,
			AppActivatedSignal appActivatedSignal)
	    {
            _appActivatedSignal = appActivatedSignal;
			_appActivatedSignal.Event += OnAppActivated;

            _assetLoader = assetLoader;
	        _gameSettings = gameSettings;
            _audioSource = GetComponent<AudioSource>();
            Volume = _gameSettings.MusicVolume;

			if (Volume > 0) LoadMusicBundle();
        }

        public IMusicPlaylist Playlist 
        {
            get => _defaultPlaylist ?? _customPlaylist;
            set 
            { 
                _customPlaylist = value;
                var track = _activeTrack;
                Stop();
                Play(track);
            }  
        }

        public void Stop()
        {
            _activeTrack = AudioTrackType.None;
            _audioSource.Stop();
        }

        public void Play(AudioTrackType track)
        {
            if (_activeTrack == track) return;
            _audioSource.Stop();
            _activeTrack = track;
		}

        public float Volume
		{
			get => _volume;
			set
			{
				_volume = value;
				_audioSource.volume = _volume;
				_gameSettings.MusicVolume = _volume;

                if (Volume > 0 && _defaultPlaylist == null)
                    LoadMusicBundle();
            }
        }

        private AudioClip NextAudioClip => _customPlaylist?.GetAudioClip(_activeTrack) ?? _defaultPlaylist?.GetAudioClip(_activeTrack);

        public void OnAppActivated(bool active)
		{
            _audioSource.mute = !active;
		}

		private void Update()
		{
            if (_audioSource.mute) return;

			if (!_audioSource.isPlaying && _activeTrack != AudioTrackType.None && Volume > 0f)
			{
                var clip = NextAudioClip;
                if (clip != null)
                {
                    _audioSource.clip = clip;
                    _audioSource.Play();
                }
			}
			else if (_audioSource.isPlaying && Volume <= 0f)
			{
				_audioSource.Stop();
			}
		}

		private void LoadMusicBundle()
		{
			if (_bundleLoadStarted) return;
            _bundleLoadStarted = true;
            _assetLoader.LoadMusicBundle(SetPlaylist);
        }

        private void SetPlaylist(IMusicPlaylist playlist)
        {
            _defaultPlaylist = playlist;

            if (!_audioSource.isPlaying)
                Play(_activeTrack);
        }

        private IMusicPlaylist _customPlaylist;
        private IMusicPlaylist _defaultPlaylist;
        private bool _bundleLoadStarted = false;
        private IAssetLoader _assetLoader;
		private AudioTrackType _activeTrack;
		private float _volume;
		private AudioSource _audioSource;
        private IGameSettings _gameSettings;
        private AppActivatedSignal _appActivatedSignal;
	}
}
