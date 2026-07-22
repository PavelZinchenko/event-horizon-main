using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameDatabase;
using GameDatabase.Model;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Services.Resources
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
        private readonly Dictionary<string, Sprite> _correctedSprites = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Texture2D> _correctedTextures = new(StringComparer.OrdinalIgnoreCase);
		
		private Dictionary<string, Sprite> _ships;
        private Dictionary<string, Sprite> _shipIcons;
        private Dictionary<string, Sprite> _components;
        private Dictionary<string, Sprite> _satellites;
        private Dictionary<string, Sprite> _controlButtons;
        private Dictionary<string, Sprite> _guiIcons;
        private Dictionary<string, AudioClip> _audio;

		private Dictionary<string, Sprite> Ships => _ships ??= CreateSpriteDictionary(_shipSprites, true);
        private Dictionary<string, Sprite> ShipIcons => _shipIcons ??= CreateSpriteDictionary(_shipIconSprites, true);
		private Dictionary<string, Sprite> Components => _components ??= CreateSpriteDictionary(_componentSprites);
		private Dictionary<string, Sprite> Satellites => _satellites ??= CreateSpriteDictionary(_satelliteSprites);
		private Dictionary<string, Sprite> ControlButtons => _controlButtons ??= CreateSpriteDictionary(_controlButtonSprites);
		private Dictionary<string, Sprite> GuiIcons => _guiIcons ??= CreateSpriteDictionary(_guiIconSprites);
		private Dictionary<string, AudioClip> Audio => _audio ??= new Dictionary<string, AudioClip>(_audioClips.ToDictionary(item => item.name));
		
        public Sprite GetSprite(SpriteId spriteId)
        {
            if (!spriteId) return null;

            Sprite sprite;

            if (spriteId.Category == SpriteId.Type.Ammunition &&
                string.Equals(spriteId.Id, "dual_vector_foil_projectile", StringComparison.OrdinalIgnoreCase))
                return GetDualVectorFoilPaperSprite();

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
                    sprite = GetSprite("Textures/Bullets/" + spriteId.Id) ?? GetComponentSprite(spriteId.Id);
                    break;
                case SpriteId.Type.Effect:
                    sprite = GetSprite("Textures/Effects/" + spriteId.Id);
                    break;
                default:
                    sprite = GetSprite(spriteId.Id);
                    break;
            }

            if (sprite == null && _database != null)
                sprite = _database.GetImage(spriteId.Id).Sprite;

            if (sprite != null &&
                (spriteId.Category == SpriteId.Type.Ship || spriteId.Category == SpriteId.Type.ShipIcon) &&
                string.Equals(spriteId.Id, "sophon_launcher", StringComparison.OrdinalIgnoreCase))
                sprite = GetClockwiseRotatedSprite(spriteId, sprite);

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
                sprite = UnityEngine.Resources.Load<Sprite>(name);
                if (!sprite) return null;

                _cache.Add(name, sprite);
            }

            return sprite;
        }

#if UNITY_EDITOR
		[ContextMenu("Reload")]
		public void Reload()
        {
            var prefab = UnityEngine.Resources.Load<ResourceLocator>("ResourceLocator");

            _shipSprites = prefab._shipSprites = LoadAllAssets<Sprite>("/Sprites/Ships").Concat(LoadAllAssets<Sprite>("/Sprites/Starbases")).ToArray();
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
                foreach (var asset in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<T>())
                    yield return asset;
            }
        }
#endif

        private static Dictionary<string, Sprite> CreateSpriteDictionary(IEnumerable<Sprite> sprites,
            bool preferLargestAlias = false)
        {
            var result = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            foreach (var sprite in sprites.Where(item => item != null))
            {
                result[sprite.name] = sprite;

                // A sliced ship texture can contain engines, turrets and the hull.
                // The old first-slice-wins alias frequently selected a small accessory
                // (or the complete source sheet) as the database-facing ship sprite.
                // The hull is consistently the largest slice, so retain the largest
                // candidate for an unsuffixed id.
                if (sprite.name.EndsWith("_0", StringComparison.OrdinalIgnoreCase))
                {
                    var id = sprite.name.Substring(0, sprite.name.Length - 2);
                    if (preferLargestAlias) SetLargestAlias(result, id, sprite);
                    else result.TryAdd(id, sprite);
                }
                else
                {
                    var separator = sprite.name.LastIndexOf('_');
                    if (separator > 0 && int.TryParse(sprite.name.Substring(separator + 1), out _))
                    {
                        var id = sprite.name.Substring(0, separator);
                        if (preferLargestAlias) SetLargestAlias(result, id, sprite);
                        else result.TryAdd(id, sprite);
                    }
                }
            }

            return result;
        }

        private static void SetLargestAlias(IDictionary<string, Sprite> sprites, string id, Sprite candidate)
        {
            if (!sprites.TryGetValue(id, out var current) ||
                candidate.rect.width * candidate.rect.height > current.rect.width * current.rect.height)
                sprites[id] = candidate;
        }

        private Sprite GetClockwiseRotatedSprite(SpriteId spriteId, Sprite source)
        {
            var key = spriteId.Category + ":" + spriteId.Id + ":" + source.GetInstanceID();
            if (_correctedSprites.TryGetValue(key, out var cached) && cached != null)
                return cached;

            var sourceTexture = CopySpritePixels(source);
            if (sourceTexture == null)
                return source;

            var sourceWidth = sourceTexture.width;
            var sourceHeight = sourceTexture.height;
            var sourcePixels = sourceTexture.GetPixels32();
            var targetPixels = new Color32[sourcePixels.Length];

            // Unity texture coordinates start at the lower-left. Clockwise rotation
            // maps source (x,y) to target (y, width-1-x).
            for (var y = 0; y < sourceHeight; ++y)
            for (var x = 0; x < sourceWidth; ++x)
            {
                var targetX = y;
                var targetY = sourceWidth - 1 - x;
                targetPixels[targetY * sourceHeight + targetX] = sourcePixels[y * sourceWidth + x];
            }

            var rotated = new Texture2D(sourceHeight, sourceWidth, TextureFormat.RGBA32, false)
            {
                name = source.name + "_Clockwise90",
                filterMode = source.texture.filterMode,
                wrapMode = TextureWrapMode.Clamp,
            };
            rotated.SetPixels32(targetPixels);
            rotated.Apply(false, true);
            UnityEngine.Object.Destroy(sourceTexture);

            var normalizedPivot = new Vector2(
                source.pivot.y / Mathf.Max(1f, source.rect.height),
                1f - source.pivot.x / Mathf.Max(1f, source.rect.width));
            var corrected = Sprite.Create(rotated,
                new Rect(0f, 0f, rotated.width, rotated.height),
                normalizedPivot,
                Mathf.Max(1f, source.pixelsPerUnit),
                0,
                SpriteMeshType.Tight);
            corrected.name = source.name + "_Clockwise90";

            _correctedTextures[key] = rotated;
            _correctedSprites[key] = corrected;
            return corrected;
        }

        private Sprite GetDualVectorFoilPaperSprite()
        {
            const string key = "Generated:DualVectorFoilPaper";
            if (_correctedSprites.TryGetValue(key, out var cached) && cached != null)
                return cached;

            const int width = 48;
            const int height = 18;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "dual_vector_foil_projectile_Paper",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                bool edge = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                pixels[y * width + x] = edge
                    ? new Color32(218, 226, 234, 255)
                    : new Color32(255, 255, 255, 255);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            var sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f), 64f, 0, SpriteMeshType.FullRect);
            sprite.name = "dual_vector_foil_projectile_Paper";
            _correctedTextures[key] = texture;
            _correctedSprites[key] = sprite;
            return sprite;
        }

        private static Texture2D CopySpritePixels(Sprite sprite)
        {
            Rect rect;
            try { rect = sprite.textureRect; }
            catch { rect = sprite.rect; }

            try
            {
                var copy = new Texture2D(Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height),
                    TextureFormat.RGBA32, false);
                copy.SetPixels(sprite.texture.GetPixels(
                    Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y),
                    Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height)));
                copy.Apply(false, false);
                return copy;
            }
            catch
            {
                var width = Mathf.Max(1, Mathf.RoundToInt(rect.width));
                var height = Mathf.Max(1, Mathf.RoundToInt(rect.height));
                var target = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
                var previous = RenderTexture.active;
                try
                {
                    var scale = new Vector2(rect.width / sprite.texture.width, rect.height / sprite.texture.height);
                    var offset = new Vector2(rect.x / sprite.texture.width, rect.y / sprite.texture.height);
                    Graphics.Blit(sprite.texture, target, scale, offset);
                    RenderTexture.active = target;
                    var copy = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    copy.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    copy.Apply(false, false);
                    return copy;
                }
                finally
                {
                    RenderTexture.active = previous;
                    RenderTexture.ReleaseTemporary(target);
                }
            }
        }

		private Sprite GetShipSprite(string id)
        {
            if (Ships.TryGetValue(id, out var sprite))
                return sprite;

            // Project-owned ship artwork lives in Resources so it remains
            // available even when third-party mods replace the locator prefab.
            return GetSprite("Textures/ThreeBody/" + id);
        }
		private Sprite GetShipIconSprite(string id) => ShipIcons.TryGetValue(id, out var sprite) || Ships.TryGetValue(id, out sprite)
            ? sprite
            : GetShipSprite(id);
        private Sprite GetComponentSprite(string id) => Components.TryGetValue(id, out var sprite)
            ? sprite
            : GetSprite("Textures/ThreeBody/" + id);
		private Sprite GetSatelliteSprite(string id) => Satellites.TryGetValue(id, out var sprite) ? sprite : null;
		private Sprite GetControlButtonSprite(string id)
        {
            if (ControlButtons.TryGetValue(id, out var sprite))
                return sprite;

            // Custom control-button sprites added under Resources should work
            // without requiring a manual refresh of the serialized locator prefab.
            return GetSprite("Textures/GUI/Controls/" + id);
        }
		private Sprite GetGuiIcon(string id) => GuiIcons.TryGetValue(id, out var sprite) ? sprite : null;
	}
}
