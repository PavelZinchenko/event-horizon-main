using GameDatabase.Enums;
using GameDatabase.Model;

namespace Constructor.Model
{
    public interface IShipLayout
    {
        int CellCount { get; }
        ref readonly LayoutRect Rect { get; }
        CellType this[int x, int y] { get; }

        [System.Obsolete] int Size { get; }
    }

    public readonly struct LayoutRect
    {
        public LayoutRect(int xmin, int ymin, int xmax, int ymax)
        {
            if (xmax < xmin || ymax < ymin)
                throw new System.InvalidOperationException();

            xMin = xmin;
            yMin = ymin;
            xMax = xmax;
            yMax = ymax;
        }

        public readonly int xMin;
        public readonly int xMax;
        public readonly int yMin;
        public readonly int yMax;

        public int Width => xMax - xMin + 1;
        public int Height => yMax - yMin + 1;
        public int Square => Width*Height;

        public bool IsInsideRect(int x, int y) => x >= xMin && y >= yMin && x <= xMax && y <= yMax;
    }

    public static class ShipLayoutExtensison
    {
        public static int ToArrayIndex(this in LayoutRect rect, int x, int y)
        {
            return x - rect.xMin + (y - rect.yMin) * rect.Width;
        }

        public static void ArrayIndexToXY(this in LayoutRect rect, int index, out int x, out int y)
        {
            x = rect.xMin + index % rect.Width;
            y = rect.yMin + index / rect.Width;
        }
    }

    public class ShipLayoutAdapter : IShipLayout
    {
        private readonly Layout _layout;
        private readonly LayoutRect _rect;

        public CellType this[int x, int y] => (CellType)_layout[x,y];
        public int CellCount => _layout.CellCount;
        public int Size => _layout.Size;

        public ref readonly LayoutRect Rect => ref _rect;

        public ShipLayoutAdapter(Layout layout)
        {
            _layout = layout;
            _rect = new LayoutRect(0,0,Size-1,Size-1);
        }
    }
}
