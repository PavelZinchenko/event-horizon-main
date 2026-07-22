using System.Drawing;
using System.Collections.Generic;

namespace GameDatabase.ShipLayout
{
    public struct PaintData
    {
        public PaintData(Graphics graphics, int layoutSize, int canvasSize, int borderSize, int cellSize, bool mirrorHorizontal, bool mirrorVertical)
        {
            Graphics = graphics;
            LayoutSize = layoutSize;
            CanvasSize = canvasSize;
            BorderSize = borderSize;
            CellSize = cellSize;
            MirrorHorizontal = mirrorHorizontal;
            MirrorVertical = mirrorVertical;
        }

        public enum MirrorMode
        {
            None,
            Horizontal,
            Vertical,
            Both,
        }

        public enum AlignSide
        {
            Center,
            TopLeft,
            BottomRight
        }

        public int PointToCellIndex(int mouseX, int mouseY)
        {
            var x = LayoutSize * (mouseX - BorderSize) / CanvasSize;
            var y = LayoutSize * (mouseY - BorderSize) / CanvasSize;

            if (x < 0 || x >= LayoutSize)
                return -1;
            if (y < 0 || y >= LayoutSize)
                return -1;

            return x + y * LayoutSize;
        }

        public IEnumerable<int> GetMirrorCells(int cell)
        {
            return GetMirrorCells(cell % LayoutSize, cell / LayoutSize);
        }

        public IEnumerable<int> GetMirrorCells(int x, int y)
        {
            yield return x + y * LayoutSize;

            if (MirrorVertical)
                yield return LayoutSize - x - 1 + y * LayoutSize;
            if (MirrorHorizontal)
                yield return x + (LayoutSize - y - 1) * LayoutSize;
            if (MirrorHorizontal && MirrorVertical)
                yield return LayoutSize - x - 1 + (LayoutSize - y - 1) * LayoutSize;
        }

        public Point Align(int mouseX, int mouseY, AlignSide side)
        {
            return Align(PointToCellIndex(mouseX, mouseY), side);
        }

        public Point Align(int cell, AlignSide side)
        {
            int cellY = cell / LayoutSize;
            int cellX = cell % LayoutSize;
            int x = BorderSize + cellX * CanvasSize / LayoutSize;
            int y = BorderSize + cellY * CanvasSize / LayoutSize;

            if (side == AlignSide.Center)
            {
                return new Point(x + CellSize / 2, y + CellSize / 2);
            }
            if (side != AlignSide.BottomRight)
            {
                return new Point(x, y);
            }

            return new Point(x + CellSize, y + CellSize);
        }

        public readonly bool MirrorHorizontal;
        public readonly bool MirrorVertical;
        public readonly int LayoutSize;
        public readonly int CanvasSize;
        public readonly int BorderSize;
        public readonly int CellSize;
        public readonly Graphics Graphics;
    }
}
