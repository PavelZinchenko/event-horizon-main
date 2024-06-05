using UnityEngine;

namespace GameDatabase.Model
{
    public interface IImageData
    {
        Sprite Sprite { get; }
    }

    public class ImageData : IImageData
    {
        public static ImageData Empty = new();

        public Sprite Sprite { get; }

        public ImageData(byte[] data)
        {
            var texture = new Texture2D(2, 2);

            if (!texture.LoadImage(data))
            {
                GameDiagnostics.Trace.LogError("Invalid texture format");
                return;
            }

            if (IsPowerOfTwo(texture.width) && IsPowerOfTwo(texture.height))
                texture.Compress(true);
            else
                GameDiagnostics.Trace.LogError($"Texture dimensions({texture.width},{texture.height}) are not powers of two. Compression will be disabled. Consider resizing it for improved performance and reduced memory usage.") ;

            Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
        }

        private static bool IsPowerOfTwo(int n) => n > 0 && (n & (n - 1)) == 0;

        private ImageData() { }
    }
}
