using System;
using System.Windows.Forms;
using EditorDatabase;
using EditorDatabase.DataModel;

namespace GameDatabase
{
    public partial class ComponentEditorDialog : Form
    {
        public ComponentEditorDialog(Database database, Component component)
        {
            _component = component;
            _database = database;
            
            InitializeComponent();
        }

        private void ComponentEditorDialog_Load(object sender, EventArgs e)
        {
            Text = _component.Id.Name;
            structDataEditor1.Database = _database;
            structDataEditor1.Data = new DataAdapter(_component);
        }

        private readonly Component _component;
        private readonly Database _database;
    }
}
