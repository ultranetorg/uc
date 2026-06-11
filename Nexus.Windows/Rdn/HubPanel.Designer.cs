
namespace Uccs.Nexus.Windows
{
	partial class HubPanel
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
			label1 = new Label();
			Seeds = new ListView();
			columnHeader5 = new ColumnHeader();
			columnHeader2 = new ColumnHeader();
			columnHeader1 = new ColumnHeader();
			label2 = new Label();
			Packages = new ListView();
			columnHeader14 = new ColumnHeader();
			AuthorSearch = new ComboBox();
			Search = new Button();
			namelabel = new Label();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label1.Location = new Point(4, 65);
			label1.Margin = new Padding(0, 0, 3, 0);
			label1.Name = "label1";
			label1.Size = new Size(66, 13);
			label1.TabIndex = 2;
			label1.Text = "Resources";
			label1.TextAlign = ContentAlignment.TopRight;
			// 
			// Seeds
			// 
			Seeds.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
			Seeds.Columns.AddRange(new ColumnHeader[] { columnHeader5, columnHeader2, columnHeader1 });
			Seeds.FullRowSelect = true;
			Seeds.Location = new Point(656, 87);
			Seeds.Margin = new Padding(3, 9, 3, 3);
			Seeds.Name = "Seeds";
			Seeds.Size = new Size(366, 680);
			Seeds.TabIndex = 4;
			Seeds.UseCompatibleStateImageBehavior = false;
			Seeds.View = View.Details;
			// 
			// columnHeader5
			// 
			columnHeader5.Text = "IP";
			columnHeader5.Width = 100;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Availability";
			columnHeader2.Width = 100;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Arrived";
			columnHeader1.Width = 100;
			// 
			// label2
			// 
			label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			label2.AutoSize = true;
			label2.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label2.Location = new Point(656, 65);
			label2.Margin = new Padding(4, 0, 4, 0);
			label2.Name = "label2";
			label2.Size = new Size(41, 13);
			label2.TabIndex = 2;
			label2.Text = "Seeds";
			label2.TextAlign = ContentAlignment.TopRight;
			// 
			// Packages
			// 
			Packages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Packages.Columns.AddRange(new ColumnHeader[] { columnHeader14 });
			Packages.FullRowSelect = true;
			Packages.Location = new Point(4, 87);
			Packages.Margin = new Padding(3, 9, 3, 3);
			Packages.Name = "Packages";
			Packages.Size = new Size(640, 680);
			Packages.TabIndex = 6;
			Packages.UseCompatibleStateImageBehavior = false;
			Packages.View = View.Details;
			Packages.ItemSelectionChanged += Packages_ItemSelectionChanged;
			// 
			// columnHeader14
			// 
			columnHeader14.Text = "Resource";
			columnHeader14.Width = 300;
			// 
			// AuthorSearch
			// 
			AuthorSearch.FormattingEnabled = true;
			AuthorSearch.Location = new Point(103, 10);
			AuthorSearch.Margin = new Padding(4, 3, 4, 3);
			AuthorSearch.Name = "AuthorSearch";
			AuthorSearch.Size = new Size(355, 23);
			AuthorSearch.TabIndex = 23;
			// 
			// Search
			// 
			Search.Location = new Point(469, 7);
			Search.Margin = new Padding(7, 14, 7, 7);
			Search.Name = "Search";
			Search.Size = new Size(166, 28);
			Search.TabIndex = 22;
			Search.Text = "Search";
			Search.UseVisualStyleBackColor = true;
			Search.Click += Search_Click;
			// 
			// namelabel
			// 
			namelabel.AutoSize = true;
			namelabel.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			namelabel.Location = new Point(4, 16);
			namelabel.Margin = new Padding(4, 0, 4, 0);
			namelabel.Name = "namelabel";
			namelabel.Size = new Size(91, 13);
			namelabel.TabIndex = 21;
			namelabel.Text = "Resource Hash";
			// 
			// HubPanel
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(AuthorSearch);
			Controls.Add(Search);
			Controls.Add(namelabel);
			Controls.Add(Packages);
			Controls.Add(Seeds);
			Controls.Add(label1);
			Controls.Add(label2);
			Margin = new Padding(4, 3, 4, 3);
			Name = "HubPanel";
			Size = new Size(1024, 768);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView Seeds;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListView Packages;
		private System.Windows.Forms.ColumnHeader columnHeader14;
		private System.Windows.Forms.ComboBox AuthorSearch;
		private System.Windows.Forms.Button Search;
		private System.Windows.Forms.Label namelabel;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
	}
}
