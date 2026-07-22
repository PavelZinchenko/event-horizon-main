namespace GameDatabase
{
    partial class EditorDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorDialog));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.structDataEditor1 = new StructDataEditor();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 378);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(400, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // structDataEditor1
            // 
            this.structDataEditor1.AutoSize = true;
            this.structDataEditor1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None;
            this.structDataEditor1.ContentAutoScroll = true;
            this.structDataEditor1.Data = null;
            this.structDataEditor1.Database = null;
            this.structDataEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.structDataEditor1.Exclusions = ((System.Collections.Generic.List<string>)(resources.GetObject("structDataEditor1.Exclusions")));
            this.structDataEditor1.Location = new System.Drawing.Point(0, 0);
            this.structDataEditor1.Name = "structDataEditor1";
            this.structDataEditor1.Size = new System.Drawing.Size(400, 378);
            this.structDataEditor1.TabIndex = 3;
            // 
            // EditorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = global::DatabaseEditor.Properties.Settings.Default.EditorSize;
            this.Controls.Add(this.structDataEditor1);
            this.Controls.Add(this.statusStrip1);
            this.DataBindings.Add(new System.Windows.Forms.Binding("Location", global::DatabaseEditor.Properties.Settings.Default, "EditorPosition", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.DataBindings.Add(new System.Windows.Forms.Binding("ClientSize", global::DatabaseEditor.Properties.Settings.Default, "EditorSize", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Location = global::DatabaseEditor.Properties.Settings.Default.EditorPosition;
            this.Name = "EditorDialog";
            this.Text = "EditorDialog";
            this.Load += new System.EventHandler(this.EditorDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private StructDataEditor structDataEditor1;
    }
}