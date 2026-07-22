namespace GameDatabase.Controls
{
    partial class CollectionEditor
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
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutOuterPanel = new System.Windows.Forms.TableLayoutPanel();
            this.buttonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.addButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.moveUpButton = new System.Windows.Forms.Button();
            this.moveDownButton = new System.Windows.Forms.Button();
            this.tableLayoutOuterPanel.SuspendLayout();
            this.buttonsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.AutoScroll = true;
            this.tableLayoutPanel.AutoSize = true;
            this.tableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 1;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(820, 496);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // tableLayoutOuterPanel
            // 
            this.tableLayoutOuterPanel.AutoScroll = true;
            this.tableLayoutOuterPanel.AutoSize = true;
            this.tableLayoutOuterPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutOuterPanel.ColumnCount = 1;
            this.tableLayoutOuterPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutOuterPanel.Controls.Add(this.tableLayoutPanel, 0, 0);
            this.tableLayoutOuterPanel.Controls.Add(this.buttonsPanel, 0, 1);
            this.tableLayoutOuterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutOuterPanel.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutOuterPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutOuterPanel.Name = "tableLayoutOuterPanel";
            this.tableLayoutOuterPanel.RowCount = 2;
            this.tableLayoutOuterPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutOuterPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutOuterPanel.Size = new System.Drawing.Size(826, 537);
            this.tableLayoutOuterPanel.TabIndex = 1;
            // 
            // buttonsPanel
            // 
            this.buttonsPanel.AutoSize = true;
            this.buttonsPanel.Controls.Add(this.addButton);
            this.buttonsPanel.Controls.Add(this.deleteButton);
            this.buttonsPanel.Controls.Add(this.moveUpButton);
            this.buttonsPanel.Controls.Add(this.moveDownButton);
            this.buttonsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonsPanel.Location = new System.Drawing.Point(3, 505);
            this.buttonsPanel.Name = "buttonsPanel";
            this.buttonsPanel.Size = new System.Drawing.Size(820, 29);
            this.buttonsPanel.TabIndex = 1;
            // 
            // addButton
            // 
            this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addButton.Location = new System.Drawing.Point(3, 3);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(75, 23);
            this.addButton.TabIndex = 1;
            this.addButton.Text = "Add";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // deleteButton
            // 
            this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.deleteButton.Location = new System.Drawing.Point(84, 3);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 1;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // moveUpButton
            // 
            this.moveUpButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.moveUpButton.Location = new System.Drawing.Point(165, 3);
            this.moveUpButton.Name = "moveUpButton";
            this.moveUpButton.Size = new System.Drawing.Size(75, 23);
            this.moveUpButton.TabIndex = 1;
            this.moveUpButton.Text = "Move Up";
            this.moveUpButton.UseVisualStyleBackColor = true;
            this.moveUpButton.Click += new System.EventHandler(this.moveUpButton_Click);
            // 
            // moveDownButton
            // 
            this.moveDownButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.moveDownButton.Location = new System.Drawing.Point(246, 3);
            this.moveDownButton.Name = "moveDownButton";
            this.moveDownButton.Size = new System.Drawing.Size(75, 23);
            this.moveDownButton.TabIndex = 1;
            this.moveDownButton.Text = "MoveDown";
            this.moveDownButton.UseVisualStyleBackColor = true;
            this.moveDownButton.Click += new System.EventHandler(this.moveDownButton_Click);
            // 
            // CollectionEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.Controls.Add(this.tableLayoutOuterPanel);
            this.Name = "CollectionEditor";
            this.Size = new System.Drawing.Size(826, 537);
            this.tableLayoutOuterPanel.ResumeLayout(false);
            this.tableLayoutOuterPanel.PerformLayout();
            this.buttonsPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutOuterPanel;
        private System.Windows.Forms.FlowLayoutPanel buttonsPanel;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button moveUpButton;
        private System.Windows.Forms.Button moveDownButton;
    }
}
