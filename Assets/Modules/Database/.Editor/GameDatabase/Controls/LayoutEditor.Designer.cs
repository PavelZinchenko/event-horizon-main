namespace GameDatabase
{
    partial class LayoutEditor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.horizontalSymmetryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verticalSymmetryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showGridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showBarrelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.horizontalSymmetryToolStripMenuItem,
            this.verticalSymmetryToolStripMenuItem,
            this.showGridToolStripMenuItem,
            this.showLayoutToolStripMenuItem,
            this.showBarrelsToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(198, 134);
            // 
            // horizontalSymmetryToolStripMenuItem
            // 
            this.horizontalSymmetryToolStripMenuItem.CheckOnClick = true;
            this.horizontalSymmetryToolStripMenuItem.Name = "horizontalSymmetryToolStripMenuItem";
            this.horizontalSymmetryToolStripMenuItem.ShowShortcutKeys = false;
            this.horizontalSymmetryToolStripMenuItem.Size = new System.Drawing.Size(197, 26);
            this.horizontalSymmetryToolStripMenuItem.Text = "Horizontal symmetry";
            this.horizontalSymmetryToolStripMenuItem.Click += new System.EventHandler(this.horizontalSymmetryToolStripMenuItem_Click);
            // 
            // verticalSymmetryToolStripMenuItem
            // 
            this.verticalSymmetryToolStripMenuItem.CheckOnClick = true;
            this.verticalSymmetryToolStripMenuItem.Name = "verticalSymmetryToolStripMenuItem";
            this.verticalSymmetryToolStripMenuItem.ShowShortcutKeys = false;
            this.verticalSymmetryToolStripMenuItem.Size = new System.Drawing.Size(197, 26);
            this.verticalSymmetryToolStripMenuItem.Text = "Vertical symmetry";
            this.verticalSymmetryToolStripMenuItem.Click += new System.EventHandler(this.verticalSymmetryToolStripMenuItem_Click);
            // 
            // showGridToolStripMenuItem
            // 
            this.showGridToolStripMenuItem.Checked = true;
            this.showGridToolStripMenuItem.CheckOnClick = true;
            this.showGridToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showGridToolStripMenuItem.Name = "showGridToolStripMenuItem";
            this.showGridToolStripMenuItem.ShowShortcutKeys = false;
            this.showGridToolStripMenuItem.Size = new System.Drawing.Size(197, 26);
            this.showGridToolStripMenuItem.Text = "Show grid";
            this.showGridToolStripMenuItem.Click += new System.EventHandler(this.showGridToolStripMenuItem_Click);
            // 
            // showLayoutToolStripMenuItem
            // 
            this.showLayoutToolStripMenuItem.Checked = true;
            this.showLayoutToolStripMenuItem.CheckOnClick = true;
            this.showLayoutToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showLayoutToolStripMenuItem.Name = "showLayoutToolStripMenuItem";
            this.showLayoutToolStripMenuItem.ShowShortcutKeys = false;
            this.showLayoutToolStripMenuItem.Size = new System.Drawing.Size(197, 26);
            this.showLayoutToolStripMenuItem.Text = "Show layout";
            this.showLayoutToolStripMenuItem.Click += new System.EventHandler(this.showLayoutToolStripMenuItem_Click);
            // 
            // showBarrelsToolStripMenuItem
            // 
            this.showBarrelsToolStripMenuItem.Checked = true;
            this.showBarrelsToolStripMenuItem.CheckOnClick = true;
            this.showBarrelsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showBarrelsToolStripMenuItem.Name = "showBarrelsToolStripMenuItem";
            this.showBarrelsToolStripMenuItem.ShowShortcutKeys = false;
            this.showBarrelsToolStripMenuItem.Size = new System.Drawing.Size(197, 26);
            this.showBarrelsToolStripMenuItem.Text = "Show barrels";
            this.showBarrelsToolStripMenuItem.Click += new System.EventHandler(this.showBarrelsToolStripMenuItem_Click);
            // 
            // LayoutEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "LayoutEditor";
            this.Size = new System.Drawing.Size(497, 450);
            this.SizeChanged += new System.EventHandler(this.LayoutEditor_SizeChanged);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LayoutEditor_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LayoutEditor_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LayoutEditor_MouseUp);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem horizontalSymmetryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem verticalSymmetryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showGridToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showLayoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showBarrelsToolStripMenuItem;
    }
}
