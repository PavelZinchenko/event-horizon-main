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

            texture.Compress(true);
            Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
        }

        private ImageData() { }
    }
}
