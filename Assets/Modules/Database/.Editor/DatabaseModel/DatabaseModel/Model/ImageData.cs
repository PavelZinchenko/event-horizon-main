using System;
using System.Drawing;
using System.IO;

namespace EditorDatabase.Model
{
    public interface IImageData 
    {
        Image Image { get; }
    }

    public class ImageData : IImageData
    {
        public ImageData(string fileName)
        {
            _fileName = fileName;
            _image = null;
        }

        public string Name => string.IsNullOrEmpty(_fileName) ? string.Empty : Path.GetFileNameWithoutExtension(_fileName);
        public Image Image
        {
            get
            {
                if (string.IsNullOrEmpty(_fileName)) return null;

                if (_image == null)
                    _image = LoadImage(_fileName);

                return _image;
            }
        }

        private static Image LoadImage(string name)
        {
            Image image;
            try
            {
                image = Image.FromFile(name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

            if (image.Width != image.Height)
                return null;

            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            return image;
        }

        private readonly string _fileName;
        private Image _image;

        public static ImageData Empty = new ImageData(string.Empty);
    }
}
