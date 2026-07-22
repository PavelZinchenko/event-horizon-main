using System;
using EditorDatabase.Model;

namespace GameDatabase.ShipLayout
{
    public class LayoutModel
    {
        public event Action DataChangedEvent = () => { };

        public LayoutModel()
        {
            _layout = new Layout(_defaultLayout);
        }

        public string Data
        {
            get { return _layout.Data; }
            set
            {
                _layout.Data = string.IsNullOrEmpty(value) ? _defaultLayout : value;
                DataChangedEvent.Invoke();
            }
        }

        public int Size { get { return _layout.Size; } }

        public bool MirrorHorizontal { get; set; }
        public bool MirrorVertical { get; set; }

        public bool IsValidIndex(int index) { return index >= 0 && index < _layout.Data.Length; }

        public char this[int x, int y]
        {
            get { return _layout[x, y]; }
            set
            {
                SetValue(x, y, value);
                DataChangedEvent.Invoke();
            }
        }

        public char this[int index]
        {
            get { return _layout[index]; }
            set
            {
                SetValue(index % _layout.Size, index / _layout.Size, value);
                DataChangedEvent.Invoke();
            }
        }

        public void DrawRectangle(int cell1, int cell2, char value)
        {
            if (!IsValidIndex(cell1) || !IsValidIndex(cell2)) return;

            var x1 = cell1 % _layout.Size;
            var y1 = cell1 / _layout.Size;
            var x2 = cell2 % _layout.Size;
            var y2 = cell2 / _layout.Size;

            if (x1 > x2)
            {
                var t = x1;
                x1 = x2;
                x2 = t;
            }

            if (y1 > y2)
            {
                var t = y1;
                y1 = y2;
                y2 = t;
            }

            for (var x = x1; x <= x2; ++x)
                for (var y = y1; y <= y2; ++y)
                    SetValue(x, y, value);

            DataChangedEvent.Invoke();
        }

        public void DrawEllipse(int cell1, int cell2, char value)
        {
            if (!IsValidIndex(cell1) || !IsValidIndex(cell2)) return;

            var x1 = cell1 % _layout.Size;
            var x2 = cell2 % _layout.Size;
            var y1 = cell1 / _layout.Size;
            var y2 = cell2 / _layout.Size;

            if (x1 > x2)
            {
                var t = x1;
                x1 = x2;
                x2 = t;
            }

            if (y1 > y2)
            {
                var t = y1;
                y1 = y2;
                y2 = t;
            }

            var d = (x1 + x2) / 2f;
            var d2 = (y1 + y2) / 2f;
            var num = (x2 - x1) / 2f + 0.5f;
            var num2 = (y2 - y1) / 2f + 0.5f;
            for (int i = x1; i <= x2; i++)
                for (int j = y1; j <= y2; j++)
                    if ((i - d) * (i - d) / (num * num) + (j - d2) * (j - d2) / (num2 * num2) <= 1f)
                        SetValue(i, j, value);

            DataChangedEvent.Invoke();
        }

        public void DrawLine(int cell1, int cell2, char value)
        {
            if (!IsValidIndex(cell1) || !IsValidIndex(cell2)) return;

            if (cell1 == cell2)
            {
                this[cell1] = value;
                return;
            }

            var x1 = cell1 % _layout.Size;
            var y1 = cell1 / _layout.Size;
            var x2 = cell2 % _layout.Size;
            var y2 = cell2 / _layout.Size;

            var length = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
            for (var i = 0; i <= length; ++i)
            {
                var x = x1 + (x2-x1)*(2*i+1) / (length*2);
                var y = y1 + (y2-y1)*(2*i+1) / (length*2);
                SetValue(x, y, value);
            }

            DataChangedEvent.Invoke();
        }

        private void SetValue(int x, int y, char value)
        {
            var size = _layout.Size;

            _layout[x, y] = value;

            if (MirrorVertical) _layout[size - x - 1, y] = value;
            if (MirrorHorizontal) _layout[x, size - y - 1] = value;
            if (MirrorVertical && MirrorHorizontal) _layout[size - x - 1, size - y - 1] = value;
        }

        private readonly Layout _layout;
        private const string _defaultLayout = "000000000";
    }
}
