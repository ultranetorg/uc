namespace Uccs.Mcv.FUI
{
	partial class MainForm
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
			if(disposing && (components != null))
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
			fileToolStripMenuItem = new ToolStripMenuItem();
			newToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator = new ToolStripSeparator();
			exitToolStripMenuItem = new ToolStripMenuItem();
			Navigator = new TreeView();
			place = new Panel();
			SuspendLayout();
			// 
			// fileToolStripMenuItem
			// 
			fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			fileToolStripMenuItem.Size = new Size(58, 24);
			fileToolStripMenuItem.Text = "&Account";
			// 
			// newToolStripMenuItem
			// 
			newToolStripMenuItem.ImageTransparentColor = Color.Magenta;
			newToolStripMenuItem.Name = "newToolStripMenuItem";
			newToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
			newToolStripMenuItem.Size = new Size(176, 22);
			newToolStripMenuItem.Text = "&New Account";
			// 
			// toolStripSeparator
			// 
			toolStripSeparator.Name = "toolStripSeparator";
			toolStripSeparator.Size = new Size(173, 6);
			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.Size = new Size(176, 22);
			exitToolStripMenuItem.Text = "E&xit";
			// 
			// Navigator
			// 
			Navigator.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			Navigator.HideSelection = false;
			Navigator.Location = new Point(10, 10);
			Navigator.Margin = new Padding(4, 3, 4, 3);
			Navigator.Name = "Navigator";
			Navigator.Size = new Size(174, 768);
			Navigator.TabIndex = 9;
			Navigator.AfterSelect += navigator_AfterSelect;
			// 
			// place
			// 
			place.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			place.Location = new Point(197, 9);
			place.Margin = new Padding(4, 3, 4, 3);
			place.Name = "place";
			place.Size = new Size(1024, 768);
			place.TabIndex = 10;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1234, 788);
			Controls.Add(place);
			Controls.Add(Navigator);
			Margin = new Padding(4, 3, 4, 3);
			Name = "MainForm";
			Padding = new Padding(7);
			StartPosition = FormStartPosition.Manual;
			Text = "Node";
			FormClosing += MainForm_FormClosing;
			ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.TreeView Navigator;
		private System.Windows.Forms.Panel place;
	}
}

