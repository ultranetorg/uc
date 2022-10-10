
namespace UC.Sun.FUI
{
	partial class ReleasePanel
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
			if(disposing && (components != null))
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
			this.manifest = new System.Windows.Forms.TextBox();
			this.Releases = new System.Windows.Forms.ListView();
			this.cVersion = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.label5 = new System.Windows.Forms.Label();
			this.Author = new System.Windows.Forms.ComboBox();
			this.search = new System.Windows.Forms.Button();
			this.Platform = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.Product = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// manifest
			// 
			this.manifest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.manifest.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.manifest.Location = new System.Drawing.Point(7, 7);
			this.manifest.Multiline = true;
			this.manifest.Name = "manifest";
			this.manifest.ReadOnly = true;
			this.manifest.Size = new System.Drawing.Size(1002, 307);
			this.manifest.TabIndex = 11;
			// 
			// Releases
			// 
			this.Releases.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Releases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cVersion,
            this.columnHeader3,
            this.columnHeader4});
			this.Releases.FullRowSelect = true;
			this.Releases.Location = new System.Drawing.Point(0, 65);
			this.Releases.Name = "Releases";
			this.Releases.Size = new System.Drawing.Size(1024, 349);
			this.Releases.TabIndex = 4;
			this.Releases.UseCompatibleStateImageBehavior = false;
			this.Releases.View = System.Windows.Forms.View.Details;
			this.Releases.SelectedIndexChanged += new System.EventHandler(this.releases_SelectedIndexChanged);
			// 
			// cVersion
			// 
			this.cVersion.Text = "Address";
			this.cVersion.Width = 300;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Manifest Hash";
			this.columnHeader3.Width = 500;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Channel";
			this.columnHeader4.Width = 100;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label5.Location = new System.Drawing.Point(19, 18);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(46, 13);
			this.label5.TabIndex = 13;
			this.label5.Text = "Author";
			// 
			// Author
			// 
			this.Author.FormattingEnabled = true;
			this.Author.Location = new System.Drawing.Point(85, 15);
			this.Author.Name = "Author";
			this.Author.Size = new System.Drawing.Size(233, 23);
			this.Author.TabIndex = 0;
			this.Author.SelectedIndexChanged += new System.EventHandler(this.Author_SelectedIndexChanged);
			this.Author.KeyDown += new System.Windows.Forms.KeyEventHandler(this.all_KeyDown);
			// 
			// search
			// 
			this.search.Location = new System.Drawing.Point(882, 11);
			this.search.Name = "search";
			this.search.Size = new System.Drawing.Size(117, 27);
			this.search.TabIndex = 3;
			this.search.Text = "Search";
			this.search.UseVisualStyleBackColor = true;
			this.search.Click += new System.EventHandler(this.search_Click);
			// 
			// Platform
			// 
			this.Platform.FormattingEnabled = true;
			this.Platform.Location = new System.Drawing.Point(686, 14);
			this.Platform.Name = "Platform";
			this.Platform.Size = new System.Drawing.Size(170, 23);
			this.Platform.TabIndex = 2;
			this.Platform.KeyDown += new System.Windows.Forms.KeyEventHandler(this.all_KeyDown);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(624, 19);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 13);
			this.label1.TabIndex = 13;
			this.label1.Text = "Platform";
			// 
			// Product
			// 
			this.Product.FormattingEnabled = true;
			this.Product.Location = new System.Drawing.Point(405, 15);
			this.Product.Name = "Product";
			this.Product.Size = new System.Drawing.Size(192, 23);
			this.Product.TabIndex = 1;
			this.Product.KeyDown += new System.Windows.Forms.KeyEventHandler(this.all_KeyDown);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(338, 18);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(51, 13);
			this.label2.TabIndex = 13;
			this.label2.Text = "Product";
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Location = new System.Drawing.Point(0, 420);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(1023, 348);
			this.tabControl1.TabIndex = 14;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.manifest);
			this.tabPage1.Location = new System.Drawing.Point(4, 24);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(1015, 320);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Manifest";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.listView1);
			this.tabPage2.Location = new System.Drawing.Point(4, 24);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(1015, 320);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Anti-malware Verification Reports";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// listView1
			// 
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.listView1.FullRowSelect = true;
			this.listView1.Location = new System.Drawing.Point(7, 7);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(1002, 307);
			this.listView1.TabIndex = 5;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Malware Analizer";
			this.columnHeader1.Width = 200;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Report";
			this.columnHeader2.Width = 150;
			// 
			// ReleasePanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.Platform);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.Product);
			this.Controls.Add(this.Author);
			this.Controls.Add(this.search);
			this.Controls.Add(this.Releases);
			this.Name = "ReleasePanel";
			this.Size = new System.Drawing.Size(1024, 768);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.TextBox manifest;
		private System.Windows.Forms.ListView Releases;
		private System.Windows.Forms.ColumnHeader cVersion;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox Author;
		private System.Windows.Forms.Button search;
		private System.Windows.Forms.ComboBox Platform;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox Product;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
	}
}
