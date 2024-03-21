
namespace Uccs.Sun.FUI
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
			label1 = new System.Windows.Forms.Label();
			Seeds = new System.Windows.Forms.ListView();
			columnHeader5 = new System.Windows.Forms.ColumnHeader();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			label2 = new System.Windows.Forms.Label();
			Packages = new System.Windows.Forms.ListView();
			columnHeader14 = new System.Windows.Forms.ColumnHeader();
			AuthorSearch = new System.Windows.Forms.ComboBox();
			Search = new System.Windows.Forms.Button();
			namelabel = new System.Windows.Forms.Label();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label1.Location = new System.Drawing.Point(7, 128);
			label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(126, 27);
			label1.TabIndex = 2;
			label1.Text = "Resources";
			label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Seeds
			// 
			Seeds.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			Seeds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader5, columnHeader2, columnHeader1 });
			Seeds.FullRowSelect = true;
			Seeds.Location = new System.Drawing.Point(1219, 186);
			Seeds.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Seeds.Name = "Seeds";
			Seeds.Size = new System.Drawing.Size(676, 1446);
			Seeds.TabIndex = 4;
			Seeds.UseCompatibleStateImageBehavior = false;
			Seeds.View = System.Windows.Forms.View.Details;
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
			label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			label2.AutoSize = true;
			label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label2.Location = new System.Drawing.Point(1219, 128);
			label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(77, 27);
			label2.TabIndex = 2;
			label2.Text = "Seeds";
			label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Packages
			// 
			Packages.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Packages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader14 });
			Packages.FullRowSelect = true;
			Packages.Location = new System.Drawing.Point(7, 186);
			Packages.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Packages.Name = "Packages";
			Packages.Size = new System.Drawing.Size(1186, 1446);
			Packages.TabIndex = 6;
			Packages.UseCompatibleStateImageBehavior = false;
			Packages.View = System.Windows.Forms.View.Details;
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
			AuthorSearch.Location = new System.Drawing.Point(209, 25);
			AuthorSearch.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			AuthorSearch.Name = "AuthorSearch";
			AuthorSearch.Size = new System.Drawing.Size(656, 40);
			AuthorSearch.TabIndex = 23;
			// 
			// Search
			// 
			Search.Location = new System.Drawing.Point(885, 15);
			Search.Margin = new System.Windows.Forms.Padding(13, 30, 13, 15);
			Search.Name = "Search";
			Search.Size = new System.Drawing.Size(308, 58);
			Search.TabIndex = 22;
			Search.Text = "Search";
			Search.UseVisualStyleBackColor = true;
			Search.Click += Search_Click;
			// 
			// namelabel
			// 
			namelabel.AutoSize = true;
			namelabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			namelabel.Location = new System.Drawing.Point(7, 32);
			namelabel.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			namelabel.Name = "namelabel";
			namelabel.Size = new System.Drawing.Size(176, 27);
			namelabel.TabIndex = 21;
			namelabel.Text = "Resource Hash";
			// 
			// HubPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(AuthorSearch);
			Controls.Add(Search);
			Controls.Add(namelabel);
			Controls.Add(Packages);
			Controls.Add(Seeds);
			Controls.Add(label1);
			Controls.Add(label2);
			Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Name = "HubPanel";
			Size = new System.Drawing.Size(1902, 1638);
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
		private System.Windows.Forms.ColumnHeader columnHeader16;
		private System.Windows.Forms.ComboBox AuthorSearch;
		private System.Windows.Forms.Button Search;
		private System.Windows.Forms.Label namelabel;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
	}
}
