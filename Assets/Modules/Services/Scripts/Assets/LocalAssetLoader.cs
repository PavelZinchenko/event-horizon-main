using System;
using System.IO;
using UnityEngine;
using Services.Audio;

namespace Services.Assets
{
    public class LocalAssetLoader : IAssetLoader, IDisposable
    {
        public void LoadMusicBundle(Action<IMusicPlaylist> onCompleteAction) 
        {
            var assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, _bundleName));
            if (assetBundle == null)
            {
                Debug.LogError("Failed to load AssetBundle!");
                return;
            }

            _musicAsset = assetBundle;
            var playlist = _musicAsset.LoadAsset<MusicPlaylist>(_assetName);
            onCompleteAction.Invoke(playlist);
        }

        public Status Status => Status.Idle;

        public void Dispose()
        {
            if (_musicAsset) _musicAsset.Unload(true);
        }

        private AssetBundle _musicAsset;

        private const string _assetName = "Playlist";
        private const string _bundleName = "musicbundle";
    }
}
