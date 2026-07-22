using System;
using System.ComponentModel;
using System.Windows.Forms;
using EditorDatabase.Model;

namespace GameDatabase.Controls
{
    public partial class VectorEditor : UserControl
    {
        [Description("Vector"), Category("Data")]
        public Vector2 Value
        {
            get { return _vector; }
            set
            {
                _ignoreEvents = true;
                X.Value = (decimal)value.x;
                Y.Value = (decimal)value.y;
                _ignoreEvents = false;
                _vector = value;
            }
        }

        public event EventHandler ValueChanged;

        public VectorEditor()
        {
            InitializeComponent();
        }

        private Vector2 _vector;

        private void X_ValueChanged(object sender, System.EventArgs e)
        {
            if (_ignoreEvents)
                return;

            UpdateData();
        }

        private void Y_ValueChanged(object sender, System.EventArgs e)
        {
            if (_ignoreEvents)
                return;

            UpdateData();
        }

        private void UpdateData()
        {
            _vector.x = (float)X.Value;
            _vector.y = (float)Y.Value;

            if (ValueChanged != null)
                ValueChanged.Invoke(this, EventArgs.Empty);
        }

        private void VectorEditor_Load(object sender, EventArgs e)
        {
            X.MouseWheel += DisableMouseWheel;
            Y.MouseWheel += DisableMouseWheel;
        }

        private static void DisableMouseWheel(object sender, EventArgs args)
        {
            ((HandledMouseEventArgs)args).Handled = true;
        }
        
        private bool _ignoreEvents = false;
    }
}
