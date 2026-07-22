using GameDatabase.Model;

namespace ShipEditor.Model
{
    /// <summary>
    /// Rotates a component's square database canvas while preserving occupied
    /// and transparent cells, so rendering and placement stay synchronized.
    /// </summary>
    public static class ComponentLayoutRotation
    {
        public static Layout Get(Layout source, int rotation)
        {
            rotation &= 3;
            if (rotation == 0)
                return source;

            var size = source.Size;
            var data = new char[size * size];
            for (var y = 0; y < size; ++y)
            for (var x = 0; x < size; ++x)
            {
                var targetX = x;
                var targetY = y;
                switch (rotation)
                {
                    case 1:
                        targetX = size - 1 - y;
                        targetY = x;
                        break;
                    case 2:
                        targetX = size - 1 - x;
                        targetY = size - 1 - y;
                        break;
                    case 3:
                        targetX = y;
                        targetY = size - 1 - x;
                        break;
                }

                data[targetY * size + targetX] = source[x, y];
            }

            return new Layout(new string(data));
        }
    }
}
