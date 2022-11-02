
namespace UC.Sun.FUI
{
	partial class ApplicationsPanel
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
			this.results = new System.Windows.Forms.ListView();
			this.cName = new System.Windows.Forms.ColumnHeader();
			this.cAuthor = new System.Windows.Forms.ColumnHeader();
			this.cVerification = new System.Windows.Forms.ColumnHeader();
			this.label5 = new System.Windows.Forms.Label();
			this.name = new System.Windows.Forms.ComboBox();
			this.search = new System.Windows.Forms.Button();
			this.chached = new System.Windows.Forms.ListView();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// results
			// 
			this.results.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.results.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cName,
            this.cAuthor,
            this.cVerification});
			this.results.FullRowSelect = true;
			this.results.HideSelection = false;
			this.results.Location = new System.Drawing.Point(0, 93);
			this.results.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.results.Name = "results";
			this.results.Size = new System.Drawing.Size(495, 674);
			this.results.TabIndex = 12;
			this.results.UseCompatibleStateImageBehavior = false;
			this.results.View = System.Windows.Forms.View.Details;
			// 
			// cName
			// 
			this.cName.Text = "Name";
			this.cName.Width = 150;
			// 
			// cAuthor
			// 
			this.cAuthor.Text = "Author";
			this.cAuthor.Width = 150;
			// 
			// cVerification
			// 
			this.cVerification.Text = "Verification";
			this.cVerification.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.cVerification.Width = 100;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label5.Location = new System.Drawing.Point(19, 18);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(105, 13);
			this.label5.TabIndex = 13;
			this.label5.Text = "Application Name";
			// 
			// name
			// 
			this.name.FormattingEnabled = true;
			this.name.Location = new System.Drawing.Point(152, 15);
			this.name.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.name.Name = "name";
			this.name.Size = new System.Drawing.Size(296, 23);
			this.name.TabIndex = 14;
			this.name.KeyDown += new System.Windows.Forms.KeyEventHandler(this.name_KeyDown);
			// 
			// search
			// 
			this.search.Location = new System.Drawing.Point(471, 14);
			this.search.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.search.Name = "search";
			this.search.Size = new System.Drawing.Size(117, 27);
			this.search.TabIndex = 9;
			this.search.Text = "Search";
			this.search.UseVisualStyleBackColor = true;
			this.search.Click += new System.EventHandler(this.search_Click);
			// 
			// chached
			// 
			this.chached.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.chached.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader1});
			this.chached.FullRowSelect = true;
			this.chached.HideSelection = false;
			this.chached.Location = new System.Drawing.Point(515, 93);
			this.chached.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.chached.Name = "chached";
			this.chached.Size = new System.Drawing.Size(508, 674);
			this.chached.TabIndex = 12;
			this.chached.UseCompatibleStateImageBehavior = false;
			this.chached.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Name";
			this.columnHeader3.Width = 200;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Author";
			this.columnHeader4.Width = 150;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Verrsion";
			this.columnHeader1.Width = 100;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(19, 65);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 13);
			this.label1.TabIndex = 13;
			this.label1.Text = "Search Results";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(515, 65);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(120, 13);
			this.label2.TabIndex = 13;
			this.label2.Text = "Cached Applications";
			// 
			// ApplicationsPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.name);
			this.Controls.Add(this.search);
			this.Controls.Add(this.chached);
			this.Controls.Add(this.results);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "ApplicationsPanel";
			this.Size = new System.Drawing.Size(1024, 768);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.ListView results;
		private System.Windows.Forms.ColumnHeader cName;
		private System.Windows.Forms.ColumnHeader cVerification;
		private System.Windows.Forms.ColumnHeader cAuthor;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox name;
		private System.Windows.Forms.Button search;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ListView chached;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ColumnHeader columnHeader1;
	}
}
