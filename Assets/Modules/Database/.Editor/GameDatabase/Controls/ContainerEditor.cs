using System.ComponentModel;
using EditorDatabase;
using EditorDatabase.Model;

namespace GameDatabase.Controls
{
    public partial class ContainerEditor : UserControl
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
            get { return tableLayoutOuterPanel.AutoScroll; }
            set { tableLayoutOuterPanel.AutoScroll = value; }
        }

        [Description("Container"), Category("Data")]
        public IObjectWrapper Data
        {
            get { return _wrapper; }
            set
            {
                _wrapper = value;
                BuildLayout();
            }
        }

        public event EventHandler DataChanged;

		public ContainerEditor()
        {
            InitializeComponent();
        }

        private void Cleanup()
        {
			if (_contentControl == null) return;
			
			tableLayoutOuterPanel.Controls.Remove(_contentControl);
			tableLayoutOuterPanel.RowCount = 1;
			_contentControl.Dispose();
			_contentControl = null;
        }

        private void BuildLayout()
        {
            if (_wrapper == null)
                return;

			Cleanup();

			if (_wrapper.CurrentValue != _wrapper.DefaultValue)
			{
				var data = _wrapper.CurrentValue;
				var editor = new StructDataEditor
				{
					Dock = DockStyle.Fill,
					CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
					AutoSize = true,
					Database = _database,
					Data = data as IDataAdapter ?? new DataAdapter(data),
					ContentAutoScroll = false,
				};

				editor.DataChanged += OnDataChanged;
				tableLayoutOuterPanel.RowCount = 2;
				tableLayoutOuterPanel.Controls.Add(editor, 0, 1);
				_contentControl = editor;

				addButton.Visible = false;
				deleteButton.Visible = true;
			}
			else
			{
				addButton.Visible = true;
				deleteButton.Visible = false;
			}
		}

        private void OnDataChanged(object sender, EventArgs args)
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void addButton_Click(object sender, EventArgs args)
        {
			_wrapper.CreateNew();
			RebuildLayout();
		}

		private void deleteButton_Click(object sender, EventArgs e)
        {
			_wrapper.Clear();
			RebuildLayout();
        }

        private void RebuildLayout() => BuildLayout();

        private IObjectWrapper _wrapper;
        private Database _database;
		private Control _contentControl;
    }
}
