using GameDatabase.Controls;

namespace GameDatabase
{
    partial class MainWindow
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.DatabaseTreeView = new System.Windows.Forms.TreeView();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.structDataView1 = new GameDatabase.Controls.StructDataView();
            this.EditButton = new System.Windows.Forms.Button();
            this.ItemTypeText = new System.Windows.Forms.Label();
            this.ItemTypeLabel = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createModMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // DatabaseTreeView
            // 
            this.DatabaseTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DatabaseTreeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DatabaseTreeView.Location = new System.Drawing.Point(6, 3);
            this.DatabaseTreeView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.DatabaseTreeView.Name = "DatabaseTreeView";
            this.DatabaseTreeView.Size = new System.Drawing.Size(383, 568);
            this.DatabaseTreeView.TabIndex = 0;
            this.DatabaseTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.DatabaseTreeView_AfterSelect);
            this.DatabaseTreeView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.DatabaseTreeView_MouseDoubleClick);
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 24);
            this.splitContainer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.DatabaseTreeView);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.structDataView1);
            this.splitContainer.Panel2.Controls.Add(this.EditButton);
            this.splitContainer.Panel2.Controls.Add(this.ItemTypeText);
            this.splitContainer.Panel2.Controls.Add(this.ItemTypeLabel);
            this.splitContainer.Size = new System.Drawing.Size(800, 576);
            this.splitContainer.SplitterDistance = 393;
            this.splitContainer.SplitterWidth = 6;
            this.splitContainer.TabIndex = 1;
            // 
            // structDataView1
            // 
            this.structDataView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.structDataView1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.structDataView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.structDataView1.Data = null;
            this.structDataView1.Database = null;
            this.structDataView1.ForeColor = System.Drawing.Color.DimGray;
            this.structDataView1.Location = new System.Drawing.Point(4, 41);
            this.structDataView1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.structDataView1.Name = "structDataView1";
            this.structDataView1.Size = new System.Drawing.Size(381, 483);
            this.structDataView1.TabIndex = 5;
            // 
            // EditButton
            // 
            this.EditButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.EditButton.Enabled = false;
            this.EditButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EditButton.Location = new System.Drawing.Point(8, 534);
            this.EditButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.EditButton.Name = "EditButton";
            this.EditButton.Size = new System.Drawing.Size(112, 35);
            this.EditButton.TabIndex = 4;
            this.EditButton.TabStop = false;
            this.EditButton.Text = "Edit";
            this.EditButton.UseVisualStyleBackColor = true;
            this.EditButton.Click += new System.EventHandler(this.EditButton_Click);
            // 
            // ItemTypeText
            // 
            this.ItemTypeText.AutoSize = true;
            this.ItemTypeText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ItemTypeText.Location = new System.Drawing.Point(90, 3);
            this.ItemTypeText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ItemTypeText.Name = "ItemTypeText";
            this.ItemTypeText.Size = new System.Drawing.Size(14, 20);
            this.ItemTypeText.TabIndex = 3;
            this.ItemTypeText.Text = "-";
            // 
            // ItemTypeLabel
            // 
            this.ItemTypeLabel.AutoSize = true;
            this.ItemTypeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ItemTypeLabel.Location = new System.Drawing.Point(3, 3);
            this.ItemTypeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ItemTypeLabel.Name = "ItemTypeLabel";
            this.ItemTypeLabel.Size = new System.Drawing.Size(79, 20);
            this.ItemTypeLabel.TabIndex = 2;
            this.ItemTypeLabel.Text = "Item type:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsMenuItem,
            this.createModMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadMenuItem
            // 
            this.loadMenuItem.Name = "loadMenuItem";
            this.loadMenuItem.Size = new System.Drawing.Size(180, 22);
            this.loadMenuItem.Text = "Load";
            this.loadMenuItem.Click += new System.EventHandler(this.loadMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsMenuItem
            // 
            this.saveAsMenuItem.Name = "saveAsMenuItem";
            this.saveAsMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveAsMenuItem.Text = "Save As...";
            this.saveAsMenuItem.Click += new System.EventHandler(this.saveAsMenuItem_Click);
            // 
            // createModMenuItem
            // 
            this.createModMenuItem.Name = "createModMenuItem";
            this.createModMenuItem.Size = new System.Drawing.Size(180, 22);
            this.createModMenuItem.Text = "Create Mod...";
            this.createModMenuItem.Click += new System.EventHandler(this.createModMenuItem_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.AddExtension = false;
            this.saveFileDialog.FileName = "mod";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = global::DatabaseEditor.Properties.Settings.Default.MainSize;
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.menuStrip1);
            this.DataBindings.Add(new System.Windows.Forms.Binding("Location", global::DatabaseEditor.Properties.Settings.Default, "MainPosition", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.DataBindings.Add(new System.Windows.Forms.Binding("ClientSize", global::DatabaseEditor.Properties.Settings.Default, "MainSize", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Location = global::DatabaseEditor.Properties.Settings.Default.MainPosition;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainWindow";
            this.Text = "Game database";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView DatabaseTreeView;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Label ItemTypeText;
        private System.Windows.Forms.Label ItemTypeLabel;
        private System.Windows.Forms.Button EditButton;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsMenuItem;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private Controls.StructDataView structDataView1;
        private System.Windows.Forms.ToolStripMenuItem createModMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}

