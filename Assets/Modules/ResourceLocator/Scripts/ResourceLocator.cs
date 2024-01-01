using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameDatabase;
using GameDatabase.Model;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Services.Reources
{
    public class ResourceLocator : MonoBehaviour, IResourceLocator
    {
        [Inject] private readonly IDatabase _database;

        [SerializeField] private Sprite[] _shipSprites;
        [SerializeField] private Sprite[] _shipIconSprites;
        [SerializeField] private Sprite[] _componentSprites;
        [SerializeField] private Sprite[] _satelliteSprites;
        [SerializeField] private Sprite[] _controlButtonSprites;
        [SerializeField] private Sprite[] _guiIconSprites;
        [SerializeField] private AudioClip[] _audioClips;

		private readonly Dictionary<string, Sprite> _cache = new(StringComparer.OrdinalIgnoreCase);
		
		private Dictionary<string, Sprite> _ships;
        private Dictionary<string, Sprite> _shipIcons;
        private Dictionary<string, Sprite> _components;
        private Dictionary<string, Sprite> _satellites;
        private Dictionary<string, Sprite> _controlButtons;
        private Dictionary<string, Sprite> _guiIcons;
        private Dictionary<string, AudioClip> _audio;

		private Dictionary<string, Sprite> Ships => _ships ??= new Dictionary<string, Sprite>(_shipSprites.ToDictionary(item => item.name));
		private Dictionary<string, Sprite> ShipIcons => _shipIcons ??= new Dictionary<string, Sprite>(_shipIconSprites.ToDictionary(item => item.name));
		private Dictionary<string, Sprite> Components => _components ??= new Dictionary<string, Sprite>(_componentSprites.ToDictionary(item => item.name));
		private Dictionary<string, Sprite> Satellites => _satellites ??= new Dictionary<string, Sprite>(_satelliteSprites.ToDictionary(item => item.name));
		private Dictionary<string, Sprite> ControlButtons => _controlButtons ??= new Dictionary<string, Sprite>(_controlButtonSprites.ToDictionary(item => item.name));
		private Dictionary<string, Sprite> GuiIcons => _guiIcons ??= new Dictionary<string, Sprite>(_guiIconSprites.ToDictionary(item => item.name));
		private Dictionary<string, AudioClip> Audio => _audio ??= new Dictionary<string, AudioClip>(_audioClips.ToDictionary(item => item.name));
		
		public Sprite GetSprite(SpriteId spriteId)
        {
            if (!spriteId) return null;

            Sprite sprite;

            switch (spriteId.Category)
            {
                case SpriteId.Type.Component:
                    sprite = GetComponentSprite(spriteId.Id);
                    break;
                case SpriteId.Type.Ship:
                    sprite = GetShipSprite(spriteId.Id);
                    break;
                case SpriteId.Type.ShipIcon:
                    sprite = GetShipIconSprite(spriteId.Id);
                    break;
                case SpriteId.Type.Satellite:
                    sprite = GetSatelliteSprite(spriteId.Id);
                    break;
                case SpriteId.Type.ActionButton:
                    sprite = GetControlButtonSprite(spriteId.Id);
                    break;
                case SpriteId.Type.GuiIcon:
                    sprite = GetGuiIcon(spriteId.Id);
                    break;
                case SpriteId.Type.AvatarIcon:
                    sprite = GetSprite("Textures/Avatars/" + spriteId.Id);
                    break;
                case SpriteId.Type.ArtifactIcon:
                    sprite = GetSprite("Textures/Artifacts/" + spriteId.Id);
                    break;
                case SpriteId.Type.Ammunition:
                    sprite = GetSprite("Textures/Bullets/" + spriteId.Id);
                    break;
                case SpriteId.Type.Effect:
                    sprite = GetSprite("Textures/Effects/" + spriteId.Id);
                    break;
                default:
                    return GetSprite(spriteId.Id);
            }

            if (sprite == null && _database != null)
                sprite = _database.GetImage(spriteId.Id).Sprite;

            return sprite;
        }

        public AudioClip GetAudioClip(AudioClipId id)
        {
            if (!id) return null;
            return Audio.TryGetValue(id.Id, out var audioClip) ? audioClip : _database.GetAudioClip(id.Id).AudioClip;
        }

        public Sprite GetSprite(string name)
        {
            if (!_cache.TryGetValue(name, out var sprite))
            {
                sprite = Resources.Load<Sprite>(name);
                if (!sprite) return null;

                _cache.Add(name, sprite);
            }

            return sprite;
        }

#if UNITY_EDITOR
		[ContextMenu("Reload")]
		private void Reload()
        {
            var prefab = Resources.Load<ResourceLocator>("ResourceLocator");

            _shipSprites = prefab._shipSprites = LoadAllAssets<Sprite>("/Sprites/Ships").ToArray();
            _shipIconSprites = prefab._shipIconSprites = LoadAllAssets<Sprite>("/Sprites/ShipIcons").ToArray();
            _componentSprites = prefab._componentSprites = LoadAllAssets<Sprite>("/Sprites/Components").ToArray();
            _satelliteSprites = prefab._satelliteSprites = LoadAllAssets<Sprite>("/Sprites/Satellites").ToArray();
            _controlButtonSprites = prefab._controlButtonSprites = LoadAllAssets<Sprite>("/Resources/Textures/GUI/Controls").ToArray();
            _audioClips = prefab._audioClips = LoadAllAssets<AudioClip>("/Audio").ToArray();

            PrefabUtility.SavePrefabAsset(prefab.gameObject);
		}

        private IEnumerable<T> LoadAllAssets<T>(string path) where T : UnityEngine.Object
        {
            var files =
                Directory.GetFiles(Application.dataPath + path, "*", SearchOption.AllDirectories)
                    .Where(file => !file.EndsWith(".meta"));
            foreach (var file in files)
            {
                var assetPath = "Assets" + file.Replace(Application.dataPath, "").Replace('\\', '/');
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                    yield return asset;
            }
        }
#endif

		private Sprite GetShipSprite(string id) => Ships.TryGetValue(id, out var sprite) ? sprite : null;
		private Sprite GetShipIconSprite(string id) => ShipIcons.TryGetValue(id, out var sprite) || Ships.TryGetValue(id, out sprite) ? sprite : null;
		private Sprite GetComponentSprite(string id) => Components.TryGetValue(id, out var sprite) ? sprite : null;
		private Sprite GetSatelliteSprite(string id) => Satellites.TryGetValue(id, out var sprite) ? sprite : null;
		private Sprite GetControlButtonSprite(string id) => ControlButtons.TryGetValue(id, out var sprite) ? sprite : null;
		private Sprite GetGuiIcon(string id) => GuiIcons.TryGetValue(id, out var sprite) ? sprite : null;
	}
}
