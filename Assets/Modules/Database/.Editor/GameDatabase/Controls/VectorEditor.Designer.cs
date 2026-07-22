namespace GameDatabase.Controls
{
    partial class VectorEditor
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.X = new System.Windows.Forms.NumericUpDown();
            this.Y = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.X)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Y)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.X, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.Y, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(252, 150);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // X
            // 
            this.X.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.X.DecimalPlaces = 3;
            this.X.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.X.Location = new System.Drawing.Point(3, 3);
            this.X.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.X.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.X.Name = "X";
            this.X.Size = new System.Drawing.Size(120, 20);
            this.X.TabIndex = 0;
            this.X.ValueChanged += new System.EventHandler(this.X_ValueChanged);
            // 
            // Y
            // 
            this.Y.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Y.DecimalPlaces = 3;
            this.Y.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.Y.Location = new System.Drawing.Point(129, 3);
            this.Y.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.Y.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.Y.Name = "Y";
            this.Y.Size = new System.Drawing.Size(120, 20);
            this.Y.TabIndex = 1;
            this.Y.ValueChanged += new System.EventHandler(this.Y_ValueChanged);
            // 
            // VectorEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "VectorEditor";
            this.Size = new System.Drawing.Size(252, 150);
            this.Load += new System.EventHandler(this.VectorEditor_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.X)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Y)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.NumericUpDown X;
        private System.Windows.Forms.NumericUpDown Y;
    }
}
