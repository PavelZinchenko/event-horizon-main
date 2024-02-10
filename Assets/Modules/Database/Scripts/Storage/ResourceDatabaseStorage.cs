using System;
using System.IO;
using UnityEngine;

namespace GameDatabase.Storage
{
    public class ResourceDatabaseStorage : IDataStorage
    {
        public ResourceDatabaseStorage(string path)
        {
            var info = new DirectoryInfo(path);
            Name = info.Name;
            Id = string.Empty;
            _path = path;
#if UNITY_EDITOR
            if (TryFindDatabaseVersion(out var version))
                Version = version;
#endif
        }

        public void LoadContent(IContentLoader loader)
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
            foreach (var asset in Resources.LoadAll<TextAsset>(_path))
            {
#if UNITY_EDITOR
                var name = UnityEditor.AssetDatabase.GetAssetPath(asset);
				try
				{
					loader.LoadJson(name, asset.text);
				}
				catch (System.Exception)
				{
					GameDiagnostics.Trace.LogError("JSON parse error: " + name);
				}
#else
                loader.LoadJson(string.Empty, asset.text);
#endif
			}

#if UNITY_EDITOR
			foreach (var texture in Resources.LoadAll<Texture2D>(_path))
			{
				var fullname = UnityEditor.AssetDatabase.GetAssetPath(texture);
				loader.LoadImage(Path.GetFileName(fullname), new TextureAdapter(texture));
			}
#endif
		}

		public string Name { get; }
        public string Id { get; }
        public Version Version { get; } = new Version(Database.VersionMajor, Database.VersionMinor);
        public bool IsEditable
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public void UpdateItem(string name, string content)
        {
#if UNITY_EDITOR
            try
            {
                File.WriteAllText(name, content);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
#endif
        }

#if UNITY_EDITOR
        private bool TryFindDatabaseVersion(out Version version)
        {
            var serializer = new UnityJsonSerializer();
            foreach (var asset in Resources.LoadAll<TextAsset>(_path))
            {
				try
				{
					var settings = serializer.FromJson<Serializable.DatabaseSettingsSerializable>(asset.text);
					if (settings.ItemType != Enums.ItemType.DatabaseSettings) continue;
					version = new Version(settings.DatabaseVersion, settings.DatabaseVersionMinor);
					return true;
				}
				catch (Exception)
				{
                    GameDiagnostics.Trace.LogError("JSON parse error: " + asset.name);
				}

			}

            version = new();
            return false;
        }
#endif

        private readonly string _path;
    }

	public class TextureAdapter : Model.IImageData
	{
		private readonly Texture2D _texture;
		private Sprite _sprite;

		public Sprite Sprite
		{
			get
			{
				if (_sprite == null)
					_sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f), _texture.width);

				return _sprite;
			}
		}

		public TextureAdapter(Texture2D texture)
		{
			_texture = texture;
		}
	}
}
