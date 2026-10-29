using System;
using System.Runtime.CompilerServices;
using GameDatabase.Model;
using GameDatabase.Storage;
using UnityEngine;

// Converts engine-free database images and audio to Unity assets. Global namespace, so call sites need no using.
public static class DatabaseAssetConverters
{
    private sealed class Cached<T> { public T Value; public byte[] Data; }

    private static readonly ConditionalWeakTable<IImageData, Cached<Sprite>> _sprites = new();
    private static readonly ConditionalWeakTable<IAudioClipData, Cached<AudioClip>> _audioClips = new();

    // Creates the sprite on first use and caches it per image.
    public static Sprite ToSprite(this IImageData image)
    {
        if (image == null) return null;
        if (image is TextureAdapter texture) return texture.Sprite;

        var cached = _sprites.GetValue(image, data =>
        {
            var bytes = data.LoadData();
            return new Cached<Sprite> { Value = CreateSprite(bytes), Data = bytes };
        });

        if (cached.Value != null && cached.Value)
            return cached.Value;

        var sourceBytes = cached.Data;
        if (sourceBytes == null)
            sourceBytes = image.LoadData();
        if (sourceBytes == null)
            return cached.Value;

        var sprite = CreateSprite(sourceBytes);
        cached.Value = sprite;
        cached.Data = sourceBytes;
        return sprite;
    }

    // Creates the audio clip on first use and caches it per clip.
    public static AudioClip ToAudioClip(this IAudioClipData audioClip)
    {
        if (audioClip == null) return null;
        return _audioClips.GetValue(audioClip, data => new Cached<AudioClip> { Value = CreateAudioClip(data.Format, data.LoadData()) }).Value;
    }

    private static Sprite CreateSprite(byte[] data)
    {
        if (data == null) return null;

        var texture = new Texture2D(2, 2);
        if (!texture.LoadImage(data))
        {
            GameDiagnostics.Trace.LogError("Invalid texture format");
            return null;
        }

        if (IsPowerOfTwo(texture.width) && IsPowerOfTwo(texture.height))
            texture.Compress(true);
        else
            GameDiagnostics.Trace.LogError($"Texture dimensions({texture.width},{texture.height}) are not powers of two. Compression will be disabled. Consider resizing it for improved performance and reduced memory usage.");

        texture.hideFlags = HideFlags.HideAndDontSave;
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private static AudioClip CreateAudioClip(AudioFormat format, byte[] data)
    {
        if (data == null) return null;

        try
        {
            switch (format)
            {
                case AudioFormat.Wav: return OpenWavParser.ByteArrayToAudioClip(data);
                case AudioFormat.Ogg: return OggAudioClip.Create(data);
                default: throw new InvalidOperationException("Unknown audio format: " + format);
            }
        }
        catch (Exception e)
        {
            GameDiagnostics.Trace.LogException(e);
            return null;
        }
    }

    private static bool IsPowerOfTwo(int n) => n > 0 && (n & (n - 1)) == 0;
}
