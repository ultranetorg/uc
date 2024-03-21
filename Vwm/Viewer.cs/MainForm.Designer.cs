namespace UC.Vwm.Viewer
{
	partial class MainForm
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
			this._splitMain = new System.Windows.Forms.SplitContainer();
			this._treeFiles = new System.Windows.Forms.TreeView();
			this._gbImage = new System.Windows.Forms.GroupBox();
			this._imageBox = new System.Windows.Forms.PictureBox();
			this._mainMenu = new System.Windows.Forms.MenuStrip();
			this._miFile = new System.Windows.Forms.ToolStripMenuItem();
			this._openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._reloadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._misepMRU = new System.Windows.Forms.ToolStripSeparator();
			this._misepExit = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._statusMain = new System.Windows.Forms.StatusStrip();
			this._lblLoading = new System.Windows.Forms.ToolStripStatusLabel();
			((System.ComponentModel.ISupportInitialize)(this._splitMain)).BeginInit();
			this._splitMain.Panel1.SuspendLayout();
			this._splitMain.Panel2.SuspendLayout();
			this._splitMain.SuspendLayout();
			this._gbImage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._imageBox)).BeginInit();
			this._mainMenu.SuspendLayout();
			this._statusMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// _splitMain
			// 
			this._splitMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._splitMain.Location = new System.Drawing.Point(0, 24);
			this._splitMain.Name = "_splitMain";
			// 
			// _splitMain.Panel1
			// 
			this._splitMain.Panel1.Controls.Add(this._treeFiles);
			// 
			// _splitMain.Panel2
			// 
			this._splitMain.Panel2.Controls.Add(this._gbImage);
			this._splitMain.Size = new System.Drawing.Size(1016, 717);
			this._splitMain.SplitterDistance = 176;
			this._splitMain.TabIndex = 1;
			// 
			// _treeFiles
			// 
			this._treeFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._treeFiles.BackColor = System.Drawing.SystemColors.Window;
			this._treeFiles.HideSelection = false;
			this._treeFiles.Indent = 19;
			this._treeFiles.Location = new System.Drawing.Point(0, 0);
			this._treeFiles.Margin = new System.Windows.Forms.Padding(1, 1, 1, 13);
			this._treeFiles.Name = "_treeFiles";
			this._treeFiles.Size = new System.Drawing.Size(176, 716);
			this._treeFiles.TabIndex = 6;
			// 
			// _gbImage
			// 
			this._gbImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._gbImage.BackColor = System.Drawing.SystemColors.Control;
			this._gbImage.Controls.Add(this._imageBox);
			this._gbImage.Location = new System.Drawing.Point(0, 0);
			this._gbImage.Name = "_gbImage";
			this._gbImage.Size = new System.Drawing.Size(832, 716);
			this._gbImage.TabIndex = 13;
			this._gbImage.TabStop = false;
			this._gbImage.Text = "Image";
			// 
			// _imageBox
			// 
			this._imageBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(2)))));
			this._imageBox.BackgroundImage = global::UC.Vwm.Viewer.Properties.Resources.pattern;
			this._imageBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this._imageBox.Location = new System.Drawing.Point(8, 16);
			this._imageBox.Name = "_imageBox";
			this._imageBox.Size = new System.Drawing.Size(816, 692);
			this._imageBox.TabIndex = 12;
			this._imageBox.TabStop = false;
			// 
			// _mainMenu
			// 
			this._mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._miFile});
			this._mainMenu.Location = new System.Drawing.Point(0, 0);
			this._mainMenu.Name = "_mainMenu";
			this._mainMenu.Padding = new System.Windows.Forms.Padding(0);
			this._mainMenu.Size = new System.Drawing.Size(1016, 24);
			this._mainMenu.TabIndex = 2;
			this._mainMenu.Text = "menuStrip1";
			// 
			// _miFile
			// 
			this._miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openMenuItem,
            this._reloadMenuItem,
            this._misepMRU,
            this._misepExit,
            this.exitToolStripMenuItem});
			this._miFile.Name = "_miFile";
			this._miFile.Size = new System.Drawing.Size(35, 24);
			this._miFile.Text = "File";
			// 
			// _openMenuItem
			// 
			this._openMenuItem.Name = "_openMenuItem";
			this._openMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this._openMenuItem.Size = new System.Drawing.Size(140, 22);
			this._openMenuItem.Text = "Open";
			this._openMenuItem.Click += new System.EventHandler(this._openMenuItem_Click);
			// 
			// _reloadMenuItem
			// 
			this._reloadMenuItem.Name = "_reloadMenuItem";
			this._reloadMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
			this._reloadMenuItem.Size = new System.Drawing.Size(140, 22);
			this._reloadMenuItem.Text = "Reload";
			this._reloadMenuItem.Click += new System.EventHandler(this._reloadMenuItem_Click);
			// 
			// _misepMRU
			// 
			this._misepMRU.Name = "_misepMRU";
			this._misepMRU.Size = new System.Drawing.Size(137, 6);
			// 
			// _misepExit
			// 
			this._misepExit.Name = "_misepExit";
			this._misepExit.Size = new System.Drawing.Size(137, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
			this.exitToolStripMenuItem.Text = "Exit";
			// 
			// _statusMain
			// 
			this._statusMain.GripMargin = new System.Windows.Forms.Padding(0);
			this._statusMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._lblLoading});
			this._statusMain.Location = new System.Drawing.Point(0, 744);
			this._statusMain.Name = "_statusMain";
			this._statusMain.Size = new System.Drawing.Size(1016, 22);
			this._statusMain.TabIndex = 3;
			this._statusMain.Text = "statusStrip1";
			// 
			// _lblLoading
			// 
			this._lblLoading.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this._lblLoading.Name = "_lblLoading";
			this._lblLoading.Size = new System.Drawing.Size(1001, 17);
			this._lblLoading.Spring = true;
			this._lblLoading.Text = "Ready";
			this._lblLoading.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1016, 766);
			this.Controls.Add(this._statusMain);
			this.Controls.Add(this._splitMain);
			this.Controls.Add(this._mainMenu);
			this.MainMenuStrip = this._mainMenu;
			this.Name = "MainForm";
			this.Text = "Vwm/Mwx Viewer";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
			this._splitMain.Panel1.ResumeLayout(false);
			this._splitMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._splitMain)).EndInit();
			this._splitMain.ResumeLayout(false);
			this._gbImage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._imageBox)).EndInit();
			this._mainMenu.ResumeLayout(false);
			this._mainMenu.PerformLayout();
			this._statusMain.ResumeLayout(false);
			this._statusMain.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.SplitContainer _splitMain;
		private System.Windows.Forms.TreeView _treeFiles;
		private System.Windows.Forms.MenuStrip _mainMenu;
		private System.Windows.Forms.ToolStripMenuItem _miFile;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem _openMenuItem;
		private System.Windows.Forms.ToolStripSeparator _misepExit;
		private System.Windows.Forms.ToolStripMenuItem _reloadMenuItem;
		private System.Windows.Forms.StatusStrip _statusMain;
		private System.Windows.Forms.ToolStripStatusLabel _lblLoading;
		private System.Windows.Forms.PictureBox _imageBox;
		private System.Windows.Forms.GroupBox _gbImage;
		private System.Windows.Forms.ToolStripSeparator _misepMRU;

	}
}

