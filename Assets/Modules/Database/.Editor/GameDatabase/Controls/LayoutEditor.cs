using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Controls;
using EditorDatabase.Enums;
using GameDatabase.ShipLayout;

namespace GameDatabase
{
    public partial class LayoutEditor : UserControl
    {
        public LayoutEditor()
        {
            InitializeComponent();

			_layout = new LayoutModel();
            _layout.DataChangedEvent += OnLayoutChanged;

            _categories = new Dictionary<char, Color> { { '0', Color.LightGray }, { '1', Color.Blue } };
            SelectedCategory = _categories.Last().Key;

            _selectedBrush = _defaultBrush;
        }

        [Description("Layout"), Category("Margins")]
        public int BorderSize { get; set; }

        [Description("Image"), Category("Data")]
        public Image Image { get; set; }

        [Description("Layout"), Category("Data")]
        public new string Layout
        {
            get { return _layout.Data; }
            set 
			{
				_barrelMapOutdated = true;
				_layout.Data = value; 
			}
        }

        [Serializable]
        public struct BarrelData
        {
            public float X;
            public float Y;
            public float Rotation;
            public float Offset;
            public string Text;
        }

        [Description("Barrels"), Category("Data")]
        public IList<BarrelData> Barrels
        {
            get { return _barrels; }
            set
            {
                _barrels = value;
                Invalidate();
            }
        }

        [Description("Colors"), Category("Data")]
        public IDictionary<char, Color> Colors
        {
            get { return _categories; }
        }

        [Description("SelectedCategory"), Category("Data")]
        public char SelectedCategory { get; set; }

        public event EventHandler ValueChanged;

        public void OnValueChanged(object sender, EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

		public void HighlightBarrel(int barredId)
		{
			_selectedBarredId = barredId;
			Invalidate();
		}

		private void UpdateBarrelMap()
		{
			if (_selectedBarredId >= 0 && _barrelMapOutdated)
			{
				_barrelMapOutdated = false;
				_barrelMapBuilder.Build(_layout);
			}
		}

		private bool TryGetCellColor(int x, int y, out Color color)
		{
			color = Color.Black;
			var cell = _layout[x, y];

			if (cell == (char)CellType.Empty)
				return false;

			if (!_categories.TryGetValue(cell, out color))
				return false;

			if (cell == (char)CellType.Weapon)
				if (_selectedBarredId >= 0 && _barrelMapBuilder[x, y] == _selectedBarredId)
					color = Color.FromArgb(255, color.R/4 + 192, color.G/4 + 192, color.B/4 + 192);

			return true;
		}

		private void DrawLayout(PaintData data)
        {
			UpdateBarrelMap();

            data.Graphics.FillRectangle(GetBrush(_categories[(char)CellType.Empty]), new Rectangle(BorderSize, BorderSize, data.CanvasSize, data.CanvasSize));

            if (Image != null)
                data.Graphics.DrawImage(Image, new Rectangle(BorderSize, BorderSize, data.CanvasSize, data.CanvasSize));

            if (!showLayoutToolStripMenuItem.Checked) return;

            for (var i = 0; i < data.LayoutSize; ++i)
            {
                for (var j = 0; j < data.LayoutSize; ++j)
                {
                    var x = BorderSize + j * data.CanvasSize / data.LayoutSize;
                    var y = BorderSize + i * data.CanvasSize / data.LayoutSize;

					if (TryGetCellColor(j, i, out var color))
						data.Graphics.FillRectangle(GetBrush(color), x, y, data.CellSize, data.CellSize);
                }
            }
        }

        private void DrawGrid(PaintData data)
        {
            if (!showGridToolStripMenuItem.Checked) return;

            var pen = new Pen(Color.Black);
            for (var i = 0; i <= data.LayoutSize; ++i)
            {
                var x = BorderSize + i * data.CanvasSize / data.LayoutSize;
                data.Graphics.DrawLine(pen, x, BorderSize, x, BorderSize + data.CanvasSize);
                data.Graphics.DrawLine(pen, BorderSize, x, BorderSize + data.CanvasSize, x);
            }
        }

        private void DrawAxis(PaintData data)
        {
            if (!data.MirrorHorizontal && !data.MirrorVertical) return;

            var pen = new Pen(Color.Purple, 3);

            if (data.MirrorVertical)
                data.Graphics.DrawLine(pen, BorderSize + data.CanvasSize / 2, BorderSize, BorderSize + data.CanvasSize / 2, BorderSize + data.CanvasSize);
            if (data.MirrorHorizontal)
                data.Graphics.DrawLine(pen, BorderSize, BorderSize + data.CanvasSize / 2, BorderSize + data.CanvasSize, BorderSize + data.CanvasSize / 2);
        }

        private void DrawBarrels(PaintData data)
        {
            if (!showBarrelsToolStripMenuItem.Checked || _barrels == null) return;

			var defaultColor = Color.Black;
			var selectedColor = Color.Purple;
            var pen = new Pen(defaultColor, 2);
            var widePen = new Pen(defaultColor, 3);

			for (var i = 0; i < _barrels.Count; ++i)
            {
				pen.Color = i == _selectedBarredId ? selectedColor : defaultColor;
				widePen.Color = i == _selectedBarredId ? selectedColor : defaultColor;

				var barrel = _barrels[i];
                var x = BorderSize + (1 - barrel.X) * data.CanvasSize / 2;
                var y = BorderSize + (1 - barrel.Y) * data.CanvasSize / 2;

                var offset = barrel.Offset * data.CanvasSize / 2;
                var radius = Math.Max(data.CellSize / 4, offset);

                data.Graphics.DrawEllipse(widePen, x - radius, y - radius, radius * 2, radius * 2);

                var length = radius + data.CellSize / 2;
                data.Graphics.DrawLine(pen, x - length, y, x + length, y);
                data.Graphics.DrawLine(pen, x, y - length, x, y + length);

                var dir = RotationHelpers.Direction(-barrel.Rotation);
                var p0 = new Point((int)(x + dir.x * offset), (int)(y + dir.y * offset));
                var p1 = new Point((int)(x + dir.x * (offset + data.CellSize)), (int)(y + dir.y * (offset + data.CellSize)));
                var d = new Point((int)(dir.x * data.CellSize / 2f), (int)(dir.y * data.CellSize / 2f));
                var p2 = new Point(p1.X + d.X, p1.Y + d.Y);

                data.Graphics.DrawLine(widePen, p0, p2);
                data.Graphics.DrawLine(pen, p2.X, p2.Y, p1.X - d.Y, p1.Y + d.X);
                data.Graphics.DrawLine(pen, p2.X, p2.Y, p1.X + d.Y, p1.Y - d.X);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var layoutSize = _layout.Size;
            var canvasSize = Math.Min(Width, Height) - BorderSize*2 - 1;
            var cellSize = canvasSize / layoutSize + (canvasSize % layoutSize > 0 ? 1 : 0);
            var paintData = new PaintData(e.Graphics, layoutSize, canvasSize, BorderSize, cellSize, _layout.MirrorHorizontal, _layout.MirrorVertical);

            DrawLayout(paintData);
            _selectedBrush.OnDraw(paintData);

            DrawGrid(paintData);
            DrawAxis(paintData);
            DrawBarrels(paintData);
        }

        public Brush GetBrush(Color color)
        {
            Brush brush;
            if (!_brushes.TryGetValue(color, out brush))
            {
                brush = new SolidBrush(color);
                _brushes.Add(color, brush);
            }

            return brush;
        }

        private void LayoutEditor_MouseDown(object sender, MouseEventArgs e)
        {
            if (IsDragging) return;

            var cellIndex = PointToCellIndex(e.X, e.Y);
            if (cellIndex < 0) return;

            if (e.Button.HasFlag(MouseButtons.Middle))
            {
                SelectedCategory = _layout[cellIndex];
                return;
            }

            var leftButtonPressed = e.Button.HasFlag(MouseButtons.Left);
            var rightButtonPressed = e.Button.HasFlag(MouseButtons.Right);

            if (!leftButtonPressed && !rightButtonPressed) return;

            _mouseButton = leftButtonPressed ? MouseButtons.Left : MouseButtons.Right;
            var celltype = leftButtonPressed ? SelectedCategory : EraseCategory;
            var position = new MousePosition(e.X, e.Y, cellIndex);

            switch (ModifierKeys)
            {
                case Keys.Shift:
                    _selectedBrush = _lineBrush; break;
                case Keys.Control:
                    _selectedBrush = _rectangleBrush; break;
                case Keys.Alt:
                    _selectedBrush = _circleBrush; break;
                case Keys.Shift | Keys.Control:
                    _selectedBrush = _fillBrush; break;
                default:
                    _selectedBrush = _defaultBrush; break;
            }

            _selectedBrush.OnMouseDown(position, _layout, celltype);
        }

        private void LayoutEditor_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDragging) return;
            var celltype = _mouseButton == MouseButtons.Left ? SelectedCategory : EraseCategory;
            var cellIndex = PointToCellIndex(e.X, e.Y);
            var position = new MousePosition(e.X, e.Y, cellIndex);

            _selectedBrush.OnMouseDrag(position, _layout, celltype);
            Invalidate();
        }

        private void LayoutEditor_MouseUp(object sender, MouseEventArgs e)
        {
            if (!IsDragging)
            {
                if (e.Button.HasFlag(MouseButtons.Right))
                {
                    var p = PointToScreen(new Point(e.X, e.Y));
                    contextMenuStrip1.Show(p);
                }
                return;
            }

            var celltype = _mouseButton == MouseButtons.Left ? SelectedCategory : EraseCategory;
            var cellIndex = PointToCellIndex(e.X, e.Y);
            var position = new MousePosition(e.X, e.Y, cellIndex);
            _selectedBrush.OnMouseUp(position, _layout, celltype);
            _mouseButton = MouseButtons.None;
            Invalidate();
        }

        private int PointToCellIndex(int mouseX, int mouseY)
        {
            var layoutSize = _layout.Size;
            var size = Math.Min(Width, Height) - 1 - BorderSize * 2;

            var x = layoutSize * (mouseX - BorderSize) / size;
            var y = layoutSize * (mouseY - BorderSize) / size;

            if (x < 0 || x >= layoutSize)
                return -1;
            if (y < 0 || y >= layoutSize)
                return -1;

            return x + y * layoutSize;
        }

        private void LayoutEditor_SizeChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private char EraseCategory { get { return _categories.First().Key; } }

        private void OnLayoutChanged()
        {
			_barrelMapOutdated = true;
            Invalidate();
            OnValueChanged(this, EventArgs.Empty);
        }

        private void horizontalSymmetryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _layout.MirrorHorizontal = horizontalSymmetryToolStripMenuItem.Checked;            
            Invalidate();
        }

        private void verticalSymmetryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _layout.MirrorVertical = verticalSymmetryToolStripMenuItem.Checked;
            Invalidate();
        }

        private void showGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void showLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void showBarrelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Invalidate();
        }

        private bool IsDragging { get { return _mouseButton != MouseButtons.None; } }

        private MouseButtons _mouseButton;

		private bool _barrelMapOutdated = true;
		private int _selectedBarredId = -1;
		private BarrelMapBuilder _barrelMapBuilder = new BarrelMapBuilder();

        private ILayoutBrush _selectedBrush;
        private IList<BarrelData> _barrels;

        private readonly Dictionary<Color, Brush> _brushes = new Dictionary<Color, Brush>();
        private readonly Dictionary<char, Color> _categories;
        private readonly LayoutModel _layout;

        private static readonly ILayoutBrush _defaultBrush = new DefaultBrush();
        private static readonly ILayoutBrush _rectangleBrush = new RectangleBrush();
        private static readonly ILayoutBrush _lineBrush = new LineBrush();
        private static readonly ILayoutBrush _circleBrush = new CircleBrush();
        private static readonly ILayoutBrush _fillBrush = new FillBrush();

        [Serializable]
        public class CellCategory
        {
            public char Id { get; set; }
            public Color Color { get; set; }
        }
    }

	public class BarrelMapBuilder
	{
		private string _layout;
		private int[] _map;
		private readonly Queue<int> _mapCells = new Queue<int>();

		public int Size { get; private set; }
		public int BarrelCount { get; private set; }
		public int this[int x, int y]
		{
			get
			{
				if (x < 0 || x >= Size) return -1;
				if (y < 0 || y >= Size) return -1;
				return _map[x + y * Size] - 1;
			}
		}

		public void Build(LayoutModel layout)
		{
			BarrelCount = 0;
			Size = layout.Size;
			_layout = layout.Data;
			_map = new int[Size * Size];

			for (int i = 0; i < Size; ++i)
			{
				for (int j = 0; j < Size; ++j)
				{
					if (TryAssignBarrel(j, i, BarrelCount + 1))
					{
						BarrelCount++;
						ProcessCells();
					}
				}
			}
		}

		private void ProcessCells()
		{
			while (_mapCells.Count > 0)
			{
				var index = _mapCells.Dequeue();
				var y = index / Size;
				var x = index % Size;
				var barrel = _map[index];

				TryAssignBarrel(x - 1, y, barrel);
				TryAssignBarrel(x + 1, y, barrel);
				TryAssignBarrel(x, y + 1, barrel);
				TryAssignBarrel(x, y - 1, barrel);
			}
		}

		private bool TryAssignBarrel(int x, int y, int barrelId)
		{
			if (x < 0 || x >= Size) return false;
			if (y < 0 || y >= Size) return false;

			int index = x + y * Size;
			if (_map[index] > 0) return false;
			if (_layout[index] != (char)CellType.Weapon) return false;

			_map[index] = barrelId;
			_mapCells.Enqueue(index);
			return true;
		}
	}
}
