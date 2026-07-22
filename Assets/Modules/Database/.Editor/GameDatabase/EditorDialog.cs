using System;
using System.Windows.Forms;
using EditorDatabase;

namespace GameDatabase
{
    public partial class EditorDialog : Form
    {
        public EditorDialog(Database database, object item, string name)
        {
            _item = item;
            _database = database;
            _dialogName = name;

            InitializeComponent();
        }

        private void EditorDialog_Load(object sender, EventArgs e)
        {
            Text = _dialogName;
            structDataEditor1.Database = _database;
            structDataEditor1.Data = _item as IDataAdapter ?? new DataAdapter(_item);
        }

        private readonly string _dialogName;
        private readonly object _item;
        private readonly Database _database;
    }
}
