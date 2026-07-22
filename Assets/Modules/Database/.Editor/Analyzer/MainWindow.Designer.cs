namespace Analyzer
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.analyzeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deadLinksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cannonsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.console = new System.Windows.Forms.RichTextBox();
			this.shipsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.analyzeToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1184, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// loadToolStripMenuItem
			// 
			this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
			this.loadToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
			this.loadToolStripMenuItem.Text = "Reload";
			this.loadToolStripMenuItem.Click += new System.EventHandler(this.reloadToolStripMenuItem_Click);
			// 
			// analyzeToolStripMenuItem
			// 
			this.analyzeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deadLinksToolStripMenuItem,
            this.cannonsToolStripMenuItem,
            this.shipsToolStripMenuItem});
			this.analyzeToolStripMenuItem.Name = "analyzeToolStripMenuItem";
			this.analyzeToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
			this.analyzeToolStripMenuItem.Text = "Analyze";
			// 
			// deadLinksToolStripMenuItem
			// 
			this.deadLinksToolStripMenuItem.Name = "deadLinksToolStripMenuItem";
			this.deadLinksToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.deadLinksToolStripMenuItem.Text = "Dead links";
			this.deadLinksToolStripMenuItem.Click += new System.EventHandler(this.deadLinksToolStripMenuItem_Click);
			// 
			// cannonsToolStripMenuItem
			// 
			this.cannonsToolStripMenuItem.Name = "cannonsToolStripMenuItem";
			this.cannonsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.cannonsToolStripMenuItem.Text = "Ammunition";
			this.cannonsToolStripMenuItem.Click += new System.EventHandler(this.ammunitionToolStripMenuItem_Click);
			// 
			// console
			// 
			this.console.BackColor = System.Drawing.SystemColors.Window;
			this.console.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.console.Dock = System.Windows.Forms.DockStyle.Fill;
			this.console.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.console.Location = new System.Drawing.Point(0, 24);
			this.console.Name = "console";
			this.console.ReadOnly = true;
			this.console.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
			this.console.Size = new System.Drawing.Size(1184, 737);
			this.console.TabIndex = 2;
			this.console.Text = "";
			// 
			// shipsToolStripMenuItem
			// 
			this.shipsToolStripMenuItem.Name = "shipsToolStripMenuItem";
			this.shipsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.shipsToolStripMenuItem.Text = "Ships";
			this.shipsToolStripMenuItem.Click += new System.EventHandler(this.shipsToolStripMenuItem_Click);
			// 
			// MainWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1184, 761);
			this.Controls.Add(this.console);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "MainWindow";
			this.Text = "Database Analyzer";
			this.Load += new System.EventHandler(this.MainWindow_Load);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem loadToolStripMenuItem;
        private ToolStripMenuItem analyzeToolStripMenuItem;
        private RichTextBox console;
        private ToolStripMenuItem cannonsToolStripMenuItem;
        private ToolStripMenuItem deadLinksToolStripMenuItem;
        private ToolStripMenuItem shipsToolStripMenuItem;
    }
}