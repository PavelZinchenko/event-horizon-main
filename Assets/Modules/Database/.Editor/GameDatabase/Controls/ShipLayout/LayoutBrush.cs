using System;
using System.Linq;
using System.Drawing;

namespace GameDatabase.ShipLayout
{
    public interface ILayoutBrush
    {
        void OnMouseDown(MousePosition position, LayoutModel layout, char cellType);
        void OnMouseDrag(MousePosition position, LayoutModel layout, char cellType);
        void OnMouseUp(MousePosition position, LayoutModel layout, char cellType);
        void OnDraw(PaintData paintData);
    }

    public abstract class GeometryBrush : ILayoutBrush
    {
        public virtual void OnDraw(PaintData paintData)
        {
            if (!IsButtonPressed) return;

            var mirrorCells1 = paintData.GetMirrorCells(StartPosition.CellIndex).ToArray();
            var mirrorCells2 = paintData.GetMirrorCells(CurrentPosition.CellIndex).ToArray();
            if (mirrorCells1.Length != mirrorCells2.Length)
                throw new InvalidOperationException();

            for (var i = 0; i < mirrorCells1.Length; ++i)
                DoDraw(paintData, mirrorCells1[i], mirrorCells2[i]);
        }

        public virtual void OnMouseDown(MousePosition position, LayoutModel layout, char cellType)
        {
            IsButtonPressed = true;
            StartPosition = CurrentPosition = position;
            CellType = cellType;
        }

        public virtual void OnMouseDrag(MousePosition position, LayoutModel layout, char cellType)
        {
            if (!IsButtonPressed)
                throw new InvalidOperationException();

            CurrentPosition = position;
            CellType = cellType;
        }

        public virtual void OnMouseUp(MousePosition position, LayoutModel layout, char cellType)
        {
            if (!IsButtonPressed)
                throw new InvalidOperationException();

            IsButtonPressed = false;
            CurrentPosition = position;
            CellType = cellType;
        }

        protected abstract void DoDraw(PaintData data, int cell1, int cell2);

        protected bool IsButtonPressed { get; private set; }
        protected char CellType { get; private set; }
        protected MousePosition StartPosition { get; private set; }
        protected MousePosition CurrentPosition { get; private set; }
    }

    public class DefaultBrush : ILayoutBrush
    {
        public void OnDraw(PaintData paintData) { }

        public void OnMouseDown(MousePosition position, LayoutModel layout, char cellType)
        {
            layout[position.CellIndex] = cellType;
        }

        public void OnMouseDrag(MousePosition position, LayoutModel layout, char cellType)
        {
            layout[position.CellIndex] = cellType;
        }

        public void OnMouseUp(MousePosition position, LayoutModel layout, char cellType) { }
    }

    public class RectangleBrush : GeometryBrush
    {
        public override void OnMouseUp(MousePosition position, LayoutModel layout, char cellType)
        {
            base.OnMouseUp(position, layout, cellType);
            layout.DrawRectangle(StartPosition.CellIndex, position.CellIndex, cellType);
        }

        protected override void DoDraw(PaintData data, int cell1, int cell2)
        {
            int x1 = Math.Min(cell1 % data.LayoutSize, cell2 % data.LayoutSize);
            int x2 = Math.Max(cell1 % data.LayoutSize, cell2 % data.LayoutSize);
            int y1 = Math.Min(cell1 / data.LayoutSize, cell2 / data.LayoutSize);
            int y2 = Math.Max(cell1 / data.LayoutSize, cell2 / data.LayoutSize);

            var p1= data.Align(x1 + y1 * data.LayoutSize, PaintData.AlignSide.TopLeft);
            var p2 = data.Align(x2 + y2 * data.LayoutSize, PaintData.AlignSide.BottomRight);

            var pen = new Pen(Color.Red, 3f);
            data.Graphics.DrawRectangle(pen, p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
        }
    }

    public class LineBrush : GeometryBrush
    {
        public override void OnMouseUp(MousePosition position, LayoutModel layout, char cellType)
        {
            base.OnMouseUp(position, layout, cellType);
            layout.DrawLine(StartPosition.CellIndex, CurrentPosition.CellIndex, cellType);
        }

        protected override void DoDraw(PaintData data, int cell1, int cell2)
        {
            var p1 = data.Align(cell1, PaintData.AlignSide.Center);
            var p2 = data.Align(cell2, PaintData.AlignSide.Center);
            var pen = new Pen(Color.Red, 3f);
            data.Graphics.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
        }
    }

    public class FillBrush : ILayoutBrush
    {
        public void OnDraw(PaintData paintData) {}

        public void OnMouseDown(MousePosition position, LayoutModel layout, char cellType)
        {
            if (position.CellIndex < 0) return;

            char oldValue = layout[position.CellIndex];
            if (oldValue == cellType) return;

            int x = position.CellIndex % layout.Size;
            int y = position.CellIndex / layout.Size;
            
            RecursiveFill(x, y, layout, oldValue, cellType);
        }

        public void OnMouseDrag(MousePosition position, LayoutModel layout, char cellType) {}
        public void OnMouseUp(MousePosition position, LayoutModel layout, char cellType) {}

        private void RecursiveFill(int x, int y, LayoutModel layout, char oldValue, char newValue, int iteration = 0)
        {
            var index = x + layout.Size * y;
            if (x < 0 || y < 0 || x >= layout.Size || y >= layout.Size || layout[index] != oldValue)
            {
                return;
            }
            if (iteration > 10000)
            {
                throw new InvalidOperationException("10,000th iteration reached");
            }

            layout[index] = newValue;

            if (x > 0)
                RecursiveFill(x - 1, y, layout, oldValue, newValue);
            if (x < layout.Size - 1)
                RecursiveFill(x + 1, y, layout, oldValue, newValue);
            if (y > 0)
                RecursiveFill(x, y - 1, layout, oldValue, newValue);
            if (y < layout.Size -1)
                RecursiveFill(x, y + 1, layout, oldValue, newValue);
        }
    }

    public class CircleBrush : GeometryBrush
    {
        public override void OnMouseUp(MousePosition position, LayoutModel layout, char cellType)
        {
            base.OnMouseUp(position, layout, cellType);
            layout.DrawEllipse(StartPosition.CellIndex, CurrentPosition.CellIndex, cellType);
        }

        protected override void DoDraw(PaintData data, int cell1, int cell2)
        {
            int x1 = Math.Min(cell1 % data.LayoutSize, cell2 % data.LayoutSize);
            int x2 = Math.Max(cell1 % data.LayoutSize, cell2 % data.LayoutSize);
            int y1 = Math.Min(cell1 / data.LayoutSize, cell2 / data.LayoutSize);
            int y2 = Math.Max(cell1 / data.LayoutSize, cell2 / data.LayoutSize);
            var p1 = data.Align(x1 + y1 * data.LayoutSize, PaintData.AlignSide.TopLeft);
            var p2 = data.Align(x2 + y2 * data.LayoutSize, PaintData.AlignSide.BottomRight);
            var pen = new Pen(Color.Red, 3f);
            data.Graphics.DrawEllipse(pen, p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
        }
    }
}
