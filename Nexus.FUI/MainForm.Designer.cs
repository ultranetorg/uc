namespace Uccs.Nexus.FUI
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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			fileToolStripMenuItem = new ToolStripMenuItem();
			newToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator = new ToolStripSeparator();
			exitToolStripMenuItem = new ToolStripMenuItem();
			imageList1 = new ImageList(components);
			listView1 = new ListView();
			columnHeader1 = new ColumnHeader();
			columnHeader2 = new ColumnHeader();
			columnHeader3 = new ColumnHeader();
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
			// imageList1
			// 
			imageList1.ColorDepth = ColorDepth.Depth32Bit;
			imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
			imageList1.TransparentColor = Color.Transparent;
			imageList1.Images.SetKeyName(0, "Icon1.ico");
			// 
			// listView1
			// 
			listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3 });
			listView1.Location = new Point(19, 21);
			listView1.Margin = new Padding(6, 6, 6, 6);
			listView1.Name = "listView1";
			listView1.Size = new Size(1360, 900);
			listView1.TabIndex = 14;
			listView1.UseCompatibleStateImageBehavior = false;
			listView1.View = View.Details;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Net";
			columnHeader1.Width = 200;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Protocol";
			columnHeader2.Width = 100;
			// 
			// columnHeader3
			// 
			columnHeader3.Text = "Status";
			columnHeader3.TextAlign = HorizontalAlignment.Center;
			columnHeader3.Width = 100;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(13F, 32F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1400, 947);
			Controls.Add(listView1);
			Margin = new Padding(7, 6, 7, 6);
			Name = "MainForm";
			Padding = new Padding(13, 15, 13, 15);
			Text = "Connectivity";
			FormClosing += MainForm_FormClosing;
			ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private ImageList imageList1;
		private ListView listView1;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private ColumnHeader columnHeader3;
	}
}

