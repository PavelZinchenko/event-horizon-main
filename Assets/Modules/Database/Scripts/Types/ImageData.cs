namespace GameDatabase.Model
{
    // An encoded image (png or jpg). Database.Unity turns it into a sprite with ToSprite().
    public interface IImageData
    {
        // Returns the encoded bytes, or null when there is no image.
        // May release the bytes, so read it once and cache the result (ToSprite() does).
        byte[] LoadData();
    }

    public class ImageData : IImageData
    {
        public static ImageData Empty = new(null);

        private byte[] _data;

        public ImageData(byte[] data)
        {
            _data = data;
        }

        public byte[] LoadData()
        {
            var data = _data;
            _data = null;
            return data;
        }
    }
}
