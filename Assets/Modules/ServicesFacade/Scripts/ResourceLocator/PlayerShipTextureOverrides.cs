using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Services.Resources
{
    /// <summary>
    /// Stores artwork overrides made by the player in the ship editor.  The
    /// original artwork is never modified: the override is a separate PNG
    /// under the save directory and deleting it restores the database sprite.
    /// </summary>
    public static class PlayerShipTextureOverrides
    {
        private static readonly Dictionary<int, Sprite> Cache = new Dictionary<int, Sprite>();
        private static readonly Dictionary<int, Texture2D> Textures = new Dictionary<int, Texture2D>();
        private static readonly Dictionary<int, byte[]> RemoteBytes = new Dictionary<int, byte[]>();
        private static readonly Dictionary<int, Sprite> RemoteCache = new Dictionary<int, Sprite>();
        private static readonly Dictionary<int, Texture2D> RemoteTextures = new Dictionary<int, Texture2D>();
        private const string FolderName = "PlayerShipTextures";

        public static bool HasConsent
        {
            get => PlayerPrefs.GetInt("ThreeBody.TextureCustomizationDisclaimer", 0) == 1;
            set
            {
                PlayerPrefs.SetInt("ThreeBody.TextureCustomizationDisclaimer", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static Sprite Get(int shipId, Sprite fallback)
        {
            if (shipId <= 0 || fallback == null)
                return fallback;

            if (Cache.TryGetValue(shipId, out var cached) && cached != null)
                return cached;

            var path = GetOverridePath(shipId);
            if (!File.Exists(path))
                return fallback;

            try
            {
                var bytes = File.ReadAllBytes(path);
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(bytes, true))
                {
                    UnityEngine.Object.Destroy(texture);
                    return fallback;
                }

                texture.name = "PlayerShipTexture_" + shipId;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = FilterMode.Bilinear;
                Textures[shipId] = texture;
                var ppu = Mathf.Max(1f, fallback.pixelsPerUnit);
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.Tight);
                sprite.name = "PlayerShipTexture_" + shipId;
                Cache[shipId] = sprite;
                return sprite;
            }
            catch (Exception error)
            {
                Debug.LogWarning("Unable to load player ship texture: " + error.Message);
                return fallback;
            }
        }

        public static bool Apply(int shipId, Sprite baseSprite, Texture2D overlay,
            bool sticker, float scale, Vector2 normalizedOffset, float rotationDegrees, out string error)
        {
            error = null;
            if (shipId <= 0 || baseSprite == null || overlay == null)
            {
                error = "缺少舰船贴图或导入图片";
                return false;
            }

            Texture2D source = null;
            Texture2D layer = null;
            try
            {
                source = CopySprite(baseSprite);
                layer = CopyTexture(overlay);
                if (source == null || layer == null)
                {
                    error = "图片不可读";
                    return false;
                }

                var result = Compose(source, layer, sticker, scale, normalizedOffset, rotationDegrees);
                SaveOverride(shipId, result);
                ReplaceCache(shipId, result, baseSprite.pixelsPerUnit);
                UnityEngine.Object.Destroy(result);
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
            finally
            {
                if (source != null) UnityEngine.Object.Destroy(source);
                if (layer != null) UnityEngine.Object.Destroy(layer);
            }
        }

        public static void Restore(int shipId)
        {
            Cache.Remove(shipId);
            if (Textures.TryGetValue(shipId, out var texture) && texture != null)
                UnityEngine.Object.Destroy(texture);
            Textures.Remove(shipId);

            var path = GetOverridePath(shipId);
            if (File.Exists(path))
                File.Delete(path);
        }

        public static bool HasOverride(int shipId) => File.Exists(GetOverridePath(shipId));

        public static bool TryGetOverrideBytes(int shipId, out byte[] bytes)
        {
            bytes = null;
            var path = GetOverridePath(shipId);
            if (!File.Exists(path)) return false;
            try { bytes = File.ReadAllBytes(path); return bytes.Length > 0; }
            catch (Exception error) { Debug.LogWarning("Unable to read player ship texture: " + error.Message); return false; }
        }

        public static void SetRemoteOverride(int shipId, byte[] bytes)
        {
            if (shipId <= 0 || bytes == null || bytes.Length == 0) return;
            RemoteBytes[shipId] = bytes;
            if (RemoteCache.TryGetValue(shipId, out var sprite) && sprite != null) UnityEngine.Object.Destroy(sprite);
            if (RemoteTextures.TryGetValue(shipId, out var texture) && texture != null) UnityEngine.Object.Destroy(texture);
            RemoteCache.Remove(shipId);
            RemoteTextures.Remove(shipId);
        }

        public static Sprite GetRemote(int shipId, Sprite fallback)
        {
            if (!RemoteBytes.TryGetValue(shipId, out var bytes) || fallback == null) return fallback;
            if (RemoteCache.TryGetValue(shipId, out var cached) && cached != null) return cached;
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(bytes, true)) { UnityEngine.Object.Destroy(texture); return fallback; }
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), Mathf.Max(1f, fallback.pixelsPerUnit), 0, SpriteMeshType.Tight);
            RemoteTextures[shipId] = texture;
            RemoteCache[shipId] = sprite;
            return sprite;
        }

        public static void ClearRemoteSession()
        {
            foreach (var sprite in RemoteCache.Values) if (sprite != null) UnityEngine.Object.Destroy(sprite);
            foreach (var texture in RemoteTextures.Values) if (texture != null) UnityEngine.Object.Destroy(texture);
            RemoteBytes.Clear();
            RemoteCache.Clear();
            RemoteTextures.Clear();
        }

        public static Texture2D CreatePreview(Sprite baseSprite, Texture2D overlay,
            bool sticker, float scale, Vector2 normalizedOffset, float rotationDegrees = 0f)
        {
            var source = CopySprite(baseSprite);
            var layer = CopyTexture(overlay);
            if (source == null || layer == null)
            {
                if (source != null) UnityEngine.Object.Destroy(source);
                if (layer != null) UnityEngine.Object.Destroy(layer);
                return null;
            }

            var result = Compose(source, layer, sticker, scale, normalizedOffset, rotationDegrees);
            UnityEngine.Object.Destroy(source);
            UnityEngine.Object.Destroy(layer);
            return result;
        }

        public static Texture2D CreateBasePreview(Sprite baseSprite)
        {
            return CopySprite(baseSprite);
        }

        private static Texture2D Compose(Texture2D source, Texture2D layer, bool sticker,
            float scale, Vector2 normalizedOffset, float rotationDegrees)
        {
            var width = source.width;
            var height = source.height;
            var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var basePixels = source.GetPixels32();
            var layerPixels = layer.GetPixels32();
            // Texture2D's initial contents are platform-dependent. Explicitly
            // clear every pixel so transparent parts of the original hull can
            // never become white in a saved player override.
            var resultPixels = new Color32[width * height];
            var sx = Mathf.Max(0.01f, scale) * layer.width;
            var sy = Mathf.Max(0.01f, scale) * layer.height;
            var centerX = width * (0.5f + normalizedOffset.x);
            var centerY = height * (0.5f + normalizedOffset.y);
            var radians = rotationDegrees * Mathf.Deg2Rad;
            var cosine = Mathf.Cos(radians);
            var sine = Mathf.Sin(radians);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var index = y * width + x;
                    var baseColor = basePixels[index];
                    if (baseColor.a == 0)
                        continue;

                    // Undo the preview rotation before sampling the imported
                    // image so the persisted PNG exactly matches the gesture.
                    var dx = x + 0.5f - centerX;
                    var dy = y + 0.5f - centerY;
                    var u = (cosine * dx + sine * dy) / sx + 0.5f;
                    var v = (-sine * dx + cosine * dy) / sy + 0.5f;
                    if (u < 0 || u >= 1 || v < 0 || v >= 1)
                    {
                        resultPixels[index] = baseColor;
                        continue;
                    }

                    var lx = Mathf.Clamp(Mathf.FloorToInt(u * layer.width), 0, layer.width - 1);
                    var ly = Mathf.Clamp(Mathf.FloorToInt(v * layer.height), 0, layer.height - 1);
                    var layerColor = layerPixels[ly * layer.width + lx];
                    if (layerColor.a == 0)
                    {
                        resultPixels[index] = baseColor;
                        continue;
                    }

                    if (sticker)
                    {
                        var alpha = layerColor.a / 255f;
                        var inv = 1f - alpha;
                        resultPixels[index] = new Color32(
                            (byte)(layerColor.r * alpha + baseColor.r * inv),
                            (byte)(layerColor.g * alpha + baseColor.g * inv),
                            (byte)(layerColor.b * alpha + baseColor.b * inv),
                            baseColor.a);
                    }
                    else
                    {
                        resultPixels[index] = new Color32(layerColor.r, layerColor.g, layerColor.b, baseColor.a);
                    }
                }
            }

            result.SetPixels32(resultPixels);
            result.Apply(false, false);
            result.wrapMode = TextureWrapMode.Clamp;
            result.filterMode = FilterMode.Bilinear;
            return result;
        }

        private static void SaveOverride(int shipId, Texture2D texture)
        {
            var directory = Path.Combine(Application.persistentDataPath, FolderName);
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(GetOverridePath(shipId), texture.EncodeToPNG());
        }

        private static void ReplaceCache(int shipId, Texture2D texture, float pixelsPerUnit)
        {
            if (Cache.TryGetValue(shipId, out var oldSprite) && oldSprite != null)
                UnityEngine.Object.Destroy(oldSprite);
            if (Textures.TryGetValue(shipId, out var oldTexture) && oldTexture != null)
                UnityEngine.Object.Destroy(oldTexture);

            var cachedTexture = UnityEngine.Object.Instantiate(texture);
            cachedTexture.name = "PlayerShipTexture_" + shipId;
            Textures[shipId] = cachedTexture;
            Cache[shipId] = Sprite.Create(cachedTexture,
                new Rect(0, 0, cachedTexture.width, cachedTexture.height),
                new Vector2(0.5f, 0.5f), Mathf.Max(1f, pixelsPerUnit), 0, SpriteMeshType.Tight);
        }

        private static string GetOverridePath(int shipId)
        {
            return Path.Combine(Application.persistentDataPath, FolderName, shipId + ".png");
        }

        private static Texture2D CopyTexture(Texture2D texture)
        {
            if (texture == null) return null;
            try
            {
                var copy = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
                copy.SetPixels32(texture.GetPixels32());
                copy.Apply(false, false);
                return copy;
            }
            catch
            {
                return CopyViaRenderTexture(texture, new Rect(0, 0, texture.width, texture.height));
            }
        }

        private static Texture2D CopySprite(Sprite sprite)
        {
            if (sprite == null || sprite.texture == null) return null;
            // sprite.rect is the untrimmed logical rectangle.  For sliced or
            // packed ship sheets it can address the complete source texture.
            // textureRect is the exact hull slice shown by the renderer.
            Rect rect;
            try
            {
                rect = sprite.textureRect;
            }
            catch
            {
                rect = sprite.rect;
            }
            try
            {
                var copy = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
                copy.SetPixels(sprite.texture.GetPixels(
                    Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y),
                    Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height)));
                copy.Apply(false, false);
                return copy;
            }
            catch
            {
                return CopyViaRenderTexture(sprite.texture, rect);
            }
        }

        private static Texture2D CopyViaRenderTexture(Texture2D texture, Rect sourceRect)
        {
            var width = Mathf.Max(1, Mathf.RoundToInt(sourceRect.width));
            var height = Mathf.Max(1, Mathf.RoundToInt(sourceRect.height));
            var target = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            try
            {
                var scale = new Vector2(sourceRect.width / texture.width, sourceRect.height / texture.height);
                var offset = new Vector2(sourceRect.x / texture.width, sourceRect.y / texture.height);
                Graphics.Blit(texture, target, scale, offset);
                RenderTexture.active = target;
                var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
                result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                result.Apply(false, false);
                return result;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
            }
        }
    }
}
