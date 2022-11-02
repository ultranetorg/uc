
namespace UC.Sun.FUI
{
	partial class ProductPanel
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
			this.SearchAccount = new System.Windows.Forms.ComboBox();
			this.search = new System.Windows.Forms.Button();
			this.products = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.registerGroup = new System.Windows.Forms.GroupBox();
			this.Register = new System.Windows.Forms.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.Author = new System.Windows.Forms.ComboBox();
			this.ProductTitle = new System.Windows.Forms.TextBox();
			this.ProductName = new System.Windows.Forms.TextBox();
			this.Management = new System.Windows.Forms.GroupBox();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.Publishers = new System.Windows.Forms.ListBox();
			this.Change = new System.Windows.Forms.Button();
			this.AddPublisher = new System.Windows.Forms.Button();
			this.RemovePublisher = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.PublisherAccount = new System.Windows.Forms.ComboBox();
			this.registerGroup.SuspendLayout();
			this.Management.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(32, 32);
			this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(102, 27);
			this.label1.TabIndex = 16;
			this.label1.Text = "Account";
			// 
			// SearchAccount
			// 
			this.SearchAccount.FormattingEnabled = true;
			this.SearchAccount.Location = new System.Drawing.Point(165, 25);
			this.SearchAccount.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.SearchAccount.Name = "SearchAccount";
			this.SearchAccount.Size = new System.Drawing.Size(602, 40);
			this.SearchAccount.TabIndex = 17;
			// 
			// search
			// 
			this.search.Location = new System.Drawing.Point(801, 15);
			this.search.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.search.Name = "search";
			this.search.Size = new System.Drawing.Size(217, 58);
			this.search.TabIndex = 15;
			this.search.Text = "Search";
			this.search.UseVisualStyleBackColor = true;
			this.search.Click += new System.EventHandler(this.search_Click);
			// 
			// products
			// 
			this.products.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.products.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
			this.products.FullRowSelect = true;
			this.products.HideSelection = false;
			this.products.Location = new System.Drawing.Point(0, 139);
			this.products.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.products.Name = "products";
			this.products.Size = new System.Drawing.Size(1896, 832);
			this.products.TabIndex = 18;
			this.products.UseCompatibleStateImageBehavior = false;
			this.products.View = System.Windows.Forms.View.Details;
			this.products.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.products_ItemSelectionChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 200;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Title";
			this.columnHeader2.Width = 200;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Author";
			this.columnHeader3.Width = 200;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Status";
			this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader4.Width = 100;
			// 
			// registerGroup
			// 
			this.registerGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.registerGroup.Controls.Add(this.Register);
			this.registerGroup.Controls.Add(this.label8);
			this.registerGroup.Controls.Add(this.label9);
			this.registerGroup.Controls.Add(this.label3);
			this.registerGroup.Controls.Add(this.Author);
			this.registerGroup.Controls.Add(this.ProductTitle);
			this.registerGroup.Controls.Add(this.ProductName);
			this.registerGroup.Location = new System.Drawing.Point(0, 990);
			this.registerGroup.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.registerGroup.Name = "registerGroup";
			this.registerGroup.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.registerGroup.Size = new System.Drawing.Size(748, 646);
			this.registerGroup.TabIndex = 19;
			this.registerGroup.TabStop = false;
			this.registerGroup.Text = "Product Registration";
			// 
			// Register
			// 
			this.Register.Location = new System.Drawing.Point(379, 354);
			this.Register.Margin = new System.Windows.Forms.Padding(17, 19, 17, 19);
			this.Register.Name = "Register";
			this.Register.Size = new System.Drawing.Size(308, 58);
			this.Register.TabIndex = 9;
			this.Register.Text = "Register";
			this.Register.UseVisualStyleBackColor = true;
			this.Register.Click += new System.EventHandler(this.register_Click);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label8.Location = new System.Drawing.Point(130, 94);
			this.label8.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(88, 27);
			this.label8.TabIndex = 8;
			this.label8.Text = "Author";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label9.Location = new System.Drawing.Point(58, 175);
			this.label9.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(154, 27);
			this.label9.TabIndex = 8;
			this.label9.Text = "Product Title";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label3.Location = new System.Drawing.Point(48, 256);
			this.label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(169, 27);
			this.label3.TabIndex = 8;
			this.label3.Text = "Product Name";
			// 
			// Author
			// 
			this.Author.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.Author.FormattingEnabled = true;
			this.Author.Location = new System.Drawing.Point(253, 85);
			this.Author.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Author.Name = "Author";
			this.Author.Size = new System.Drawing.Size(429, 40);
			this.Author.TabIndex = 10;
			// 
			// ProductTitle
			// 
			this.ProductTitle.Location = new System.Drawing.Point(253, 166);
			this.ProductTitle.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.ProductTitle.Name = "ProductTitle";
			this.ProductTitle.Size = new System.Drawing.Size(429, 39);
			this.ProductTitle.TabIndex = 11;
			this.ProductTitle.TextChanged += new System.EventHandler(this.registeringProduct_TextChanged);
			// 
			// ProductName
			// 
			this.ProductName.Location = new System.Drawing.Point(253, 250);
			this.ProductName.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.ProductName.Name = "ProductName";
			this.ProductName.ReadOnly = true;
			this.ProductName.Size = new System.Drawing.Size(429, 39);
			this.ProductName.TabIndex = 11;
			// 
			// Management
			// 
			this.Management.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.Management.Controls.Add(this.checkBox1);
			this.Management.Controls.Add(this.Publishers);
			this.Management.Controls.Add(this.Change);
			this.Management.Controls.Add(this.AddPublisher);
			this.Management.Controls.Add(this.RemovePublisher);
			this.Management.Controls.Add(this.label2);
			this.Management.Controls.Add(this.label7);
			this.Management.Controls.Add(this.PublisherAccount);
			this.Management.Enabled = false;
			this.Management.Location = new System.Drawing.Point(782, 990);
			this.Management.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Management.Name = "Management";
			this.Management.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Management.Size = new System.Drawing.Size(1120, 646);
			this.Management.TabIndex = 19;
			this.Management.TabStop = false;
			this.Management.Text = "Product Management";
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new System.Drawing.Point(197, 437);
			this.checkBox1.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(28, 27);
			this.checkBox1.TabIndex = 12;
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// Publishers
			// 
			this.Publishers.FormattingEnabled = true;
			this.Publishers.ItemHeight = 32;
			this.Publishers.Location = new System.Drawing.Point(197, 98);
			this.Publishers.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Publishers.Name = "Publishers";
			this.Publishers.Size = new System.Drawing.Size(517, 228);
			this.Publishers.TabIndex = 11;
			// 
			// Change
			// 
			this.Change.Location = new System.Drawing.Point(743, 422);
			this.Change.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Change.Name = "Change";
			this.Change.Size = new System.Drawing.Size(308, 58);
			this.Change.TabIndex = 9;
			this.Change.Text = "Update";
			this.Change.UseVisualStyleBackColor = true;
			this.Change.Click += new System.EventHandler(this.Change_Click);
			// 
			// AddPublisher
			// 
			this.AddPublisher.Location = new System.Drawing.Point(743, 348);
			this.AddPublisher.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.AddPublisher.Name = "AddPublisher";
			this.AddPublisher.Size = new System.Drawing.Size(308, 58);
			this.AddPublisher.TabIndex = 9;
			this.AddPublisher.Text = "Add Publisher";
			this.AddPublisher.UseVisualStyleBackColor = true;
			this.AddPublisher.Click += new System.EventHandler(this.AddPublisher_Click);
			// 
			// RemovePublisher
			// 
			this.RemovePublisher.Location = new System.Drawing.Point(743, 98);
			this.RemovePublisher.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.RemovePublisher.Name = "RemovePublisher";
			this.RemovePublisher.Size = new System.Drawing.Size(308, 58);
			this.RemovePublisher.TabIndex = 9;
			this.RemovePublisher.Text = "Remove Publisher";
			this.RemovePublisher.UseVisualStyleBackColor = true;
			this.RemovePublisher.Click += new System.EventHandler(this.RemovePublisher_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(97, 437);
			this.label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(81, 27);
			this.label2.TabIndex = 8;
			this.label2.Text = "Active";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label7.Location = new System.Drawing.Point(56, 98);
			this.label7.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(127, 27);
			this.label7.TabIndex = 8;
			this.label7.Text = "Publishers";
			// 
			// PublisherAccount
			// 
			this.PublisherAccount.FormattingEnabled = true;
			this.PublisherAccount.Location = new System.Drawing.Point(197, 354);
			this.PublisherAccount.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.PublisherAccount.Name = "PublisherAccount";
			this.PublisherAccount.Size = new System.Drawing.Size(517, 40);
			this.PublisherAccount.TabIndex = 10;
			// 
			// ProductPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Management);
			this.Controls.Add(this.registerGroup);
			this.Controls.Add(this.products);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.SearchAccount);
			this.Controls.Add(this.search);
			this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Name = "ProductPanel";
			this.Size = new System.Drawing.Size(1902, 1638);
			this.registerGroup.ResumeLayout(false);
			this.registerGroup.PerformLayout();
			this.Management.ResumeLayout(false);
			this.Management.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox SearchAccount;
		private System.Windows.Forms.Button search;
		private System.Windows.Forms.ListView products;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.GroupBox registerGroup;
		private System.Windows.Forms.Button Register;
		private System.Windows.Forms.Label label3;
		private new System.Windows.Forms.TextBox ProductName;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.ComboBox Author;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox ProductTitle;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.GroupBox Management;
		private System.Windows.Forms.ListBox Publishers;
		private System.Windows.Forms.Button RemovePublisher;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox PublisherAccount;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button AddPublisher;
		private System.Windows.Forms.Button Change;
	}
}
