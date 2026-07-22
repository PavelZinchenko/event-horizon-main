using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using EditorDatabase;

namespace GameDatabase.Controls
{
    public partial class CollectionEditor : UserControl
    {
        [Description("Database"), Category("Data")]
        public Database Database
        {
            get { return _database; }
            set { _database = value; }
        }

        [Description("ContentAutoScroll"), Category("Layout")]
        public bool ContentAutoScroll
        {
            get { return tableLayoutPanel.AutoScroll; }
            set { tableLayoutPanel.AutoScroll = value; }
        }

        [Description("Collection"), Category("Data")]
        public Array Data
        {
            get { return _collection; }
            set
            {
                _collection = value;
                BuildLayout();
            }
        }

		public event EventHandler SelectionChanged;
		public event EventHandler CollectionChanged;
        public event EventHandler DataChanged;

		public int SelectedItemId => _selectedItemId;

		public CollectionEditor()
        {
            InitializeComponent();
        }

        private void Cleanup()
        {
            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.RowStyles.Clear();
            _radioButtons.Clear();
            _controls.Clear();
            _selectedItemId = -1;
        }

        private void BuildLayout()
        {
            Cleanup();

            if (_collection == null || _collection.Length == 0)
                return;

            tableLayoutPanel.SuspendLayout();

            var rowCount = _collection.Length;
            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.RowCount = rowCount;

            for (var i = 0; i < rowCount; ++i)
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            for (var i = 0; i < rowCount; ++i)
                AddRow(i, _collection.GetValue(i));

            tableLayoutPanel.ResumeLayout();
        }

        private void AddRow(int rowId, object data)
        {
            tableLayoutPanel.SuspendLayout();

            var radioButton = new RadioButton { Text = string.Empty, AutoSize = true };
            _radioButtons.Add(radioButton);
            radioButton.CheckedChanged += OnRadioButtonSelected;

            tableLayoutPanel.Controls.Add(radioButton, 0, rowId);

            var editor = new StructDataEditor
            {
                Dock = DockStyle.Fill,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                AutoSize = true,
                Database = _database,
                Data = data as IDataAdapter ?? new DataAdapter(data),
                ContentAutoScroll = false,
            };

            _controls.Add(editor);
            editor.DataChanged += OnDataChanged;

            tableLayoutPanel.Controls.Add(editor, 1, rowId);

            tableLayoutPanel.ResumeLayout();
        }

        private void OnDataChanged(object sender, EventArgs args)
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnRadioButtonSelected(object sender, EventArgs args)
        {
            _selectedItemId = _radioButtons.IndexOf((Control)sender);
			SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            if (_collection == null || _collection.Length < 2)
                return;

            if (_selectedItemId <= 0 || _selectedItemId >= _collection.Length)
                return;

            SwapControls(_selectedItemId, _selectedItemId - 1);
			_radioButtons[_selectedItemId - 1].Select();
		}

		private void moveDownButton_Click(object sender, EventArgs e)
        {
            if (_collection == null || _collection.Length < 2)
                return;

            if (_selectedItemId < 0 || _selectedItemId + 1 >= _collection.Length)
                return;

            SwapControls(_selectedItemId, _selectedItemId + 1);
			_radioButtons[_selectedItemId + 1].Select();
		}

		private void addButton_Click(object sender, EventArgs args)
        {
            if (_collection == null)
                return;

            var rowId = tableLayoutPanel.RowCount;
            tableLayoutPanel.RowCount++;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            //var collection = (Array)Activator.CreateInstance(_collection.GetType(), new [] {_collection.Length + 1});
            //Array.Copy(_collection, collection, _collection.Length);
            //collection.SetValue(_collection.Length, Activator.CreateInstance(_collection.GetType().));
            //_collection = collection;

            var type = _collection.GetType().GetElementType();
            var method = typeof(Array).GetMethod("Resize");
            var generic = method.MakeGenericMethod(type);
            var arguments = new object[] { _collection, _collection.Length + 1 };
            generic.Invoke(null, arguments);
            _collection = (Array)arguments[0];
            var value = Activator.CreateInstance(type);
            _collection.SetValue(value, _collection.Length - 1);

            AddRow(rowId, value);

            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (_collection == null || _selectedItemId < 0 || _selectedItemId >= _collection.Length)
                return;

            var radioButton = _radioButtons[_selectedItemId];
            var control = _controls[_selectedItemId];

            _radioButtons.RemoveAt(_selectedItemId);
            _controls.RemoveAt(_selectedItemId);

            var collection = Array.CreateInstance(_collection.GetType().GetElementType(), _collection.Length - 1);
            if (_selectedItemId > 0)
                Array.Copy(_collection, collection, _selectedItemId);
            if (_selectedItemId + 1 < _collection.Length)
                Array.Copy(_collection, _selectedItemId+1, collection, _selectedItemId, _collection.Length - _selectedItemId - 1);

            _collection = collection;
            
            RebuildLayout();

            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SwapControls(int index1, int index2)
        { 
            var first = _collection.GetValue(index1);
            var second = _collection.GetValue(index2);
            _collection.SetValue(second, index1);
            _collection.SetValue(first, index2);

            var control1 = _controls[index1];
            var control2 = _controls[index2];
            _controls[index1] = control2;
            _controls[index2] = control1;

            RebuildLayout();
        }

        private void RebuildLayout()
        {
            var count = _collection.Length;

            if (_radioButtons.Count != count || _controls.Count != count)
                throw new InvalidOperationException();

            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.RowCount = count;
            while (tableLayoutPanel.RowStyles.Count < count)
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            while (tableLayoutPanel.RowStyles.Count > count)
                tableLayoutPanel.RowStyles.RemoveAt(tableLayoutPanel.RowStyles.Count - 1);

            for (var i = 0; i < count; ++i)
            {
                tableLayoutPanel.Controls.Add(_radioButtons[i], 0, i);
                tableLayoutPanel.Controls.Add(_controls[i], 1, i);
            }

            tableLayoutPanel.ResumeLayout();
        }

        private int _selectedItemId = -1;
        private Array _collection;
        private Database _database;
        private readonly List<Control> _radioButtons = new List<Control>();
        private readonly List<Control> _controls = new List<Control>();
    }
}
