
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
			this.label1 = new System.Windows.Forms.Label();
			this.Seeds = new System.Windows.Forms.ListView();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.label2 = new System.Windows.Forms.Label();
			this.Packages = new System.Windows.Forms.ListView();
			this.columnHeader13 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader14 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader15 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
			this.AuthorSearch = new System.Windows.Forms.ComboBox();
			this.Search = new System.Windows.Forms.Button();
			this.namelabel = new System.Windows.Forms.Label();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(17, 60);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(61, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Packages";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Seeds
			// 
			this.Seeds.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Seeds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader2,
            this.columnHeader1});
			this.Seeds.FullRowSelect = true;
			this.Seeds.Location = new System.Drawing.Point(663, 87);
			this.Seeds.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Seeds.Name = "Seeds";
			this.Seeds.Size = new System.Drawing.Size(363, 683);
			this.Seeds.TabIndex = 4;
			this.Seeds.UseCompatibleStateImageBehavior = false;
			this.Seeds.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "IP";
			this.columnHeader5.Width = 100;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Arrived";
			this.columnHeader1.Width = 100;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(692, 60);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(41, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Seeds";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Packages
			// 
			this.Packages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Packages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader13,
            this.columnHeader14,
            this.columnHeader15,
            this.columnHeader7});
			this.Packages.FullRowSelect = true;
			this.Packages.Location = new System.Drawing.Point(0, 87);
			this.Packages.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Packages.Name = "Packages";
			this.Packages.Size = new System.Drawing.Size(644, 683);
			this.Packages.TabIndex = 6;
			this.Packages.UseCompatibleStateImageBehavior = false;
			this.Packages.View = System.Windows.Forms.View.Details;
			this.Packages.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.Packages_ItemSelectionChanged);
			// 
			// columnHeader13
			// 
			this.columnHeader13.Text = "Author";
			this.columnHeader13.Width = 150;
			// 
			// columnHeader14
			// 
			this.columnHeader14.Text = "Product";
			this.columnHeader14.Width = 150;
			// 
			// columnHeader15
			// 
			this.columnHeader15.Text = "Platform";
			this.columnHeader15.Width = 150;
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "Version";
			this.columnHeader7.Width = 150;
			// 
			// AuthorSearch
			// 
			this.AuthorSearch.FormattingEnabled = true;
			this.AuthorSearch.Location = new System.Drawing.Point(142, 12);
			this.AuthorSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.AuthorSearch.Name = "AuthorSearch";
			this.AuthorSearch.Size = new System.Drawing.Size(316, 23);
			this.AuthorSearch.TabIndex = 23;
			// 
			// Search
			// 
			this.Search.Location = new System.Drawing.Point(478, 7);
			this.Search.Margin = new System.Windows.Forms.Padding(7, 14, 7, 7);
			this.Search.Name = "Search";
			this.Search.Size = new System.Drawing.Size(166, 27);
			this.Search.TabIndex = 22;
			this.Search.Text = "Search";
			this.Search.UseVisualStyleBackColor = true;
			this.Search.Click += new System.EventHandler(this.Search_Click);
			// 
			// namelabel
			// 
			this.namelabel.AutoSize = true;
			this.namelabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.namelabel.Location = new System.Drawing.Point(17, 15);
			this.namelabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.namelabel.Name = "namelabel";
			this.namelabel.Size = new System.Drawing.Size(96, 13);
			this.namelabel.TabIndex = 21;
			this.namelabel.Text = "Author/Product";
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Distributives";
			this.columnHeader2.Width = 100;
			// 
			// HubPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.AuthorSearch);
			this.Controls.Add(this.Search);
			this.Controls.Add(this.namelabel);
			this.Controls.Add(this.Packages);
			this.Controls.Add(this.Seeds);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label2);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "HubPanel";
			this.Size = new System.Drawing.Size(1024, 768);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView Seeds;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListView Packages;
		private System.Windows.Forms.ColumnHeader columnHeader13;
		private System.Windows.Forms.ColumnHeader columnHeader14;
		private System.Windows.Forms.ColumnHeader columnHeader15;
		private System.Windows.Forms.ColumnHeader columnHeader16;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.ComboBox AuthorSearch;
		private System.Windows.Forms.Button Search;
		private System.Windows.Forms.Label namelabel;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
	}
}
