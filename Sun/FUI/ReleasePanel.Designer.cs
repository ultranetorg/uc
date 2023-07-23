
namespace Uccs.Sun.FUI
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
			manifest = new System.Windows.Forms.TextBox();
			Releases = new System.Windows.Forms.ListView();
			cVersion = new System.Windows.Forms.ColumnHeader();
			columnHeader3 = new System.Windows.Forms.ColumnHeader();
			label5 = new System.Windows.Forms.Label();
			Address = new System.Windows.Forms.ComboBox();
			search = new System.Windows.Forms.Button();
			tabControl1 = new System.Windows.Forms.TabControl();
			tabPage1 = new System.Windows.Forms.TabPage();
			tabPage2 = new System.Windows.Forms.TabPage();
			listView1 = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			tabControl1.SuspendLayout();
			tabPage1.SuspendLayout();
			tabPage2.SuspendLayout();
			SuspendLayout();
			// 
			// manifest
			// 
			manifest.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			manifest.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			manifest.Location = new System.Drawing.Point(13, 15);
			manifest.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			manifest.Multiline = true;
			manifest.Name = "manifest";
			manifest.ReadOnly = true;
			manifest.Size = new System.Drawing.Size(1857, 650);
			manifest.TabIndex = 11;
			// 
			// Releases
			// 
			Releases.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Releases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { cVersion, columnHeader3 });
			Releases.FullRowSelect = true;
			Releases.Location = new System.Drawing.Point(0, 139);
			Releases.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			Releases.Name = "Releases";
			Releases.Size = new System.Drawing.Size(1898, 740);
			Releases.TabIndex = 4;
			Releases.UseCompatibleStateImageBehavior = false;
			Releases.View = System.Windows.Forms.View.Details;
			Releases.SelectedIndexChanged += releases_SelectedIndexChanged;
			// 
			// cVersion
			// 
			cVersion.Text = "Address";
			cVersion.Width = 300;
			// 
			// columnHeader3
			// 
			columnHeader3.Text = "Data";
			columnHeader3.Width = 500;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label5.Location = new System.Drawing.Point(35, 43);
			label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(100, 27);
			label5.TabIndex = 13;
			label5.Text = "Address";
			// 
			// Author
			// 
			Address.FormattingEnabled = true;
			Address.Location = new System.Drawing.Point(147, 33);
			Address.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			Address.Name = "Author";
			Address.Size = new System.Drawing.Size(1459, 40);
			Address.TabIndex = 0;
			Address.KeyDown += all_KeyDown;
			// 
			// search
			// 
			search.Location = new System.Drawing.Point(1638, 23);
			search.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			search.Name = "search";
			search.Size = new System.Drawing.Size(217, 58);
			search.TabIndex = 3;
			search.Text = "Search";
			search.UseVisualStyleBackColor = true;
			search.Click += search_Click;
			// 
			// tabControl1
			// 
			tabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tabControl1.Controls.Add(tabPage1);
			tabControl1.Controls.Add(tabPage2);
			tabControl1.Location = new System.Drawing.Point(0, 896);
			tabControl1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			tabControl1.Name = "tabControl1";
			tabControl1.SelectedIndex = 0;
			tabControl1.Size = new System.Drawing.Size(1900, 742);
			tabControl1.TabIndex = 14;
			// 
			// tabPage1
			// 
			tabPage1.Controls.Add(manifest);
			tabPage1.Location = new System.Drawing.Point(8, 46);
			tabPage1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			tabPage1.Name = "tabPage1";
			tabPage1.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
			tabPage1.Size = new System.Drawing.Size(1884, 688);
			tabPage1.TabIndex = 0;
			tabPage1.Text = "Manifest";
			tabPage1.UseVisualStyleBackColor = true;
			// 
			// tabPage2
			// 
			tabPage2.Controls.Add(listView1);
			tabPage2.Location = new System.Drawing.Point(8, 46);
			tabPage2.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			tabPage2.Name = "tabPage2";
			tabPage2.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
			tabPage2.Size = new System.Drawing.Size(1884, 688);
			tabPage2.TabIndex = 1;
			tabPage2.Text = "Anti-malware Verification Reports";
			tabPage2.UseVisualStyleBackColor = true;
			// 
			// listView1
			// 
			listView1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1, columnHeader2 });
			listView1.FullRowSelect = true;
			listView1.Location = new System.Drawing.Point(13, 15);
			listView1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			listView1.Name = "listView1";
			listView1.Size = new System.Drawing.Size(1857, 650);
			listView1.TabIndex = 5;
			listView1.UseCompatibleStateImageBehavior = false;
			listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Malware Analizer";
			columnHeader1.Width = 200;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Report";
			columnHeader2.Width = 150;
			// 
			// ReleasePanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(tabControl1);
			Controls.Add(label5);
			Controls.Add(Address);
			Controls.Add(search);
			Controls.Add(Releases);
			Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			Name = "ReleasePanel";
			Size = new System.Drawing.Size(1902, 1638);
			tabControl1.ResumeLayout(false);
			tabPage1.ResumeLayout(false);
			tabPage1.PerformLayout();
			tabPage2.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.TextBox manifest;
		private System.Windows.Forms.ListView Releases;
		private System.Windows.Forms.ColumnHeader cVersion;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox Address;
		private System.Windows.Forms.Button search;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
	}
}
