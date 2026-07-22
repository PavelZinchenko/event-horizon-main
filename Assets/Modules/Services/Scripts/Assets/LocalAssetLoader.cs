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
            var bundlePath = Path.Combine(Application.streamingAssetsPath, _bundleName);
            // Android StreamingAssets live inside the APK and use a jar: URI.
            // File.Exists cannot inspect that URI, while AssetBundle.LoadFromFile can.
            if (!bundlePath.Contains("://") && !File.Exists(bundlePath))
            {
                Debug.LogWarning($"Optional music AssetBundle is missing: {bundlePath}");
                onCompleteAction?.Invoke(null);
                return;
            }

            var assetBundle = AssetBundle.LoadFromFile(bundlePath);
            if (assetBundle == null)
            {
                Debug.LogWarning($"Optional music AssetBundle could not be loaded: {bundlePath}");
                onCompleteAction?.Invoke(null);
                return;
            }

            _musicAsset = assetBundle;
            var playlist = _musicAsset.LoadAsset<MusicPlaylist>(_assetName);
            onCompleteAction?.Invoke(playlist);
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
