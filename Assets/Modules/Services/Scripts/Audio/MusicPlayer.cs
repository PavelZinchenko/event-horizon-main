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
                Play(_activeTrack); 
            }  
        }

        public void Play(AudioTrackType track)
        {
			_activeTrack = track;
			if (Playlist == null) return;

			Mute(false);

			var audioClip = _customPlaylist?.GetAudioClip(track) ?? _defaultPlaylist?.GetAudioClip(track);

			if (_audioSource.clip == audioClip)
				return;

			_isPlaying = false;
			_audioSource.Stop();
			_clipToPlay = audioClip;
		}

        public float Volume
		{
			get
			{
				return _volume;
			}
			set
			{
				_volume = value;
				_audioSource.volume = _volume;
				_gameSettings.MusicVolume = _volume;

                if (Volume > 0 && _defaultPlaylist == null)
                    LoadMusicBundle();
            }
        }

		public void Mute(bool mute)
		{
			_muted = mute;
			OnMute();
		}

		private void OnMute()
		{
            _audioSource.mute = _muted || !_applicationActive;
		}

		//public void Pause()
		//{
		//	_audioSource.Pause();
		//}

		//public void Resume()
		//{
		//	if (!_audioSource.isPlaying)
		//		_audioSource.UnPause();
		//}

		public void OnAppActivated(bool active)
		{
			_applicationActive = active;
			OnMute();
		}

		private void Update()
		{
			if (!_isPlaying && _clipToPlay != null && _volume > 0f)
			{
				_isPlaying = true;
				_audioSource.clip = _clipToPlay;
				_audioSource.Play();
			}
			else if (_isPlaying && _volume <= 0f)
			{
				_isPlaying = false;
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

            if (!_isPlaying)
                Play(_activeTrack);
        }

        private bool _muted;
		private bool _applicationActive = true;

        private IMusicPlaylist _customPlaylist;
        private IMusicPlaylist _defaultPlaylist;
        private bool _bundleLoadStarted = false;
        private IAssetLoader _assetLoader;
		private AudioTrackType _activeTrack;
        private bool _isPlaying;
		private float _volume;
		private AudioClip _clipToPlay;
		private AudioSource _audioSource;
        private IGameSettings _gameSettings;
        private AppActivatedSignal _appActivatedSignal;
	}
}
