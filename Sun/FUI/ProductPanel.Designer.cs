
namespace Uccs.Sun.FUI
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
			label1 = new System.Windows.Forms.Label();
			SearchAccount = new System.Windows.Forms.ComboBox();
			search = new System.Windows.Forms.Button();
			products = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			columnHeader3 = new System.Windows.Forms.ColumnHeader();
			columnHeader4 = new System.Windows.Forms.ColumnHeader();
			PublisherAccount = new System.Windows.Forms.ComboBox();
			label7 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			RemovePublisher = new System.Windows.Forms.Button();
			AddPublisher = new System.Windows.Forms.Button();
			Change = new System.Windows.Forms.Button();
			Publishers = new System.Windows.Forms.ListBox();
			checkBox1 = new System.Windows.Forms.CheckBox();
			Management = new System.Windows.Forms.GroupBox();
			ProductName = new System.Windows.Forms.TextBox();
			ProductTitle = new System.Windows.Forms.TextBox();
			Author = new System.Windows.Forms.ComboBox();
			label3 = new System.Windows.Forms.Label();
			label9 = new System.Windows.Forms.Label();
			label8 = new System.Windows.Forms.Label();
			Register = new System.Windows.Forms.Button();
			registerGroup = new System.Windows.Forms.GroupBox();
			Management.SuspendLayout();
			registerGroup.SuspendLayout();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label1.Location = new System.Drawing.Point(32, 32);
			label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(102, 27);
			label1.TabIndex = 16;
			label1.Text = "Account";
			// 
			// SearchAccount
			// 
			SearchAccount.FormattingEnabled = true;
			SearchAccount.Location = new System.Drawing.Point(165, 25);
			SearchAccount.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			SearchAccount.Name = "SearchAccount";
			SearchAccount.Size = new System.Drawing.Size(602, 40);
			SearchAccount.TabIndex = 17;
			// 
			// search
			// 
			search.Location = new System.Drawing.Point(801, 15);
			search.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			search.Name = "search";
			search.Size = new System.Drawing.Size(217, 58);
			search.TabIndex = 15;
			search.Text = "Search";
			search.UseVisualStyleBackColor = true;
			search.Click += search_Click;
			// 
			// products
			// 
			products.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			products.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4 });
			products.FullRowSelect = true;
			products.Location = new System.Drawing.Point(0, 139);
			products.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			products.Name = "products";
			products.Size = new System.Drawing.Size(1896, 832);
			products.TabIndex = 18;
			products.UseCompatibleStateImageBehavior = false;
			products.View = System.Windows.Forms.View.Details;
			products.ItemSelectionChanged += products_ItemSelectionChanged;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Name";
			columnHeader1.Width = 200;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Title";
			columnHeader2.Width = 200;
			// 
			// columnHeader3
			// 
			columnHeader3.Text = "Author";
			columnHeader3.Width = 200;
			// 
			// columnHeader4
			// 
			columnHeader4.Text = "Status";
			columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			columnHeader4.Width = 100;
			// 
			// PublisherAccount
			// 
			PublisherAccount.FormattingEnabled = true;
			PublisherAccount.Location = new System.Drawing.Point(197, 354);
			PublisherAccount.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			PublisherAccount.Name = "PublisherAccount";
			PublisherAccount.Size = new System.Drawing.Size(517, 40);
			PublisherAccount.TabIndex = 10;
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label7.Location = new System.Drawing.Point(56, 98);
			label7.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label7.Name = "label7";
			label7.Size = new System.Drawing.Size(127, 27);
			label7.TabIndex = 8;
			label7.Text = "Publishers";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label2.Location = new System.Drawing.Point(97, 437);
			label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(81, 27);
			label2.TabIndex = 8;
			label2.Text = "Active";
			// 
			// RemovePublisher
			// 
			RemovePublisher.Location = new System.Drawing.Point(743, 98);
			RemovePublisher.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			RemovePublisher.Name = "RemovePublisher";
			RemovePublisher.Size = new System.Drawing.Size(308, 58);
			RemovePublisher.TabIndex = 9;
			RemovePublisher.Text = "Remove Publisher";
			RemovePublisher.UseVisualStyleBackColor = true;
			RemovePublisher.Click += RemovePublisher_Click;
			// 
			// AddPublisher
			// 
			AddPublisher.Location = new System.Drawing.Point(743, 348);
			AddPublisher.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			AddPublisher.Name = "AddPublisher";
			AddPublisher.Size = new System.Drawing.Size(308, 58);
			AddPublisher.TabIndex = 9;
			AddPublisher.Text = "Add Publisher";
			AddPublisher.UseVisualStyleBackColor = true;
			AddPublisher.Click += AddPublisher_Click;
			// 
			// Change
			// 
			Change.Location = new System.Drawing.Point(743, 422);
			Change.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			Change.Name = "Change";
			Change.Size = new System.Drawing.Size(308, 58);
			Change.TabIndex = 9;
			Change.Text = "Update";
			Change.UseVisualStyleBackColor = true;
			Change.Click += Change_Click;
			// 
			// Publishers
			// 
			Publishers.FormattingEnabled = true;
			Publishers.ItemHeight = 32;
			Publishers.Location = new System.Drawing.Point(197, 98);
			Publishers.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			Publishers.Name = "Publishers";
			Publishers.Size = new System.Drawing.Size(517, 228);
			Publishers.TabIndex = 11;
			// 
			// checkBox1
			// 
			checkBox1.AutoSize = true;
			checkBox1.Location = new System.Drawing.Point(197, 437);
			checkBox1.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			checkBox1.Name = "checkBox1";
			checkBox1.Size = new System.Drawing.Size(28, 27);
			checkBox1.TabIndex = 12;
			checkBox1.UseVisualStyleBackColor = true;
			// 
			// Management
			// 
			Management.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			Management.Controls.Add(checkBox1);
			Management.Controls.Add(Publishers);
			Management.Controls.Add(Change);
			Management.Controls.Add(AddPublisher);
			Management.Controls.Add(RemovePublisher);
			Management.Controls.Add(label2);
			Management.Controls.Add(label7);
			Management.Controls.Add(PublisherAccount);
			Management.Enabled = false;
			Management.Location = new System.Drawing.Point(782, 990);
			Management.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Management.Name = "Management";
			Management.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Management.Size = new System.Drawing.Size(1120, 646);
			Management.TabIndex = 19;
			Management.TabStop = false;
			Management.Text = "Product Management";
			// 
			// ProductName
			// 
			ProductName.Location = new System.Drawing.Point(253, 250);
			ProductName.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			ProductName.Name = "ProductName";
			ProductName.ReadOnly = true;
			ProductName.Size = new System.Drawing.Size(429, 39);
			ProductName.TabIndex = 11;
			// 
			// ProductTitle
			// 
			ProductTitle.Location = new System.Drawing.Point(253, 166);
			ProductTitle.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			ProductTitle.Name = "ProductTitle";
			ProductTitle.Size = new System.Drawing.Size(429, 39);
			ProductTitle.TabIndex = 11;
			ProductTitle.TextChanged += registeringProduct_TextChanged;
			// 
			// Author
			// 
			Author.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			Author.FormattingEnabled = true;
			Author.Location = new System.Drawing.Point(253, 85);
			Author.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			Author.Name = "Author";
			Author.Size = new System.Drawing.Size(429, 40);
			Author.TabIndex = 10;
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label3.Location = new System.Drawing.Point(48, 256);
			label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(169, 27);
			label3.TabIndex = 8;
			label3.Text = "Product Name";
			// 
			// label9
			// 
			label9.AutoSize = true;
			label9.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label9.Location = new System.Drawing.Point(58, 175);
			label9.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label9.Name = "label9";
			label9.Size = new System.Drawing.Size(154, 27);
			label9.TabIndex = 8;
			label9.Text = "Product Title";
			// 
			// label8
			// 
			label8.AutoSize = true;
			label8.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label8.Location = new System.Drawing.Point(130, 94);
			label8.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label8.Name = "label8";
			label8.Size = new System.Drawing.Size(88, 27);
			label8.TabIndex = 8;
			label8.Text = "Author";
			// 
			// Register
			// 
			Register.Location = new System.Drawing.Point(379, 354);
			Register.Margin = new System.Windows.Forms.Padding(17, 19, 17, 19);
			Register.Name = "Register";
			Register.Size = new System.Drawing.Size(308, 58);
			Register.TabIndex = 9;
			Register.Text = "Register";
			Register.UseVisualStyleBackColor = true;
			Register.Click += register_Click;
			// 
			// registerGroup
			// 
			registerGroup.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			registerGroup.Controls.Add(Register);
			registerGroup.Controls.Add(label8);
			registerGroup.Controls.Add(label9);
			registerGroup.Controls.Add(label3);
			registerGroup.Controls.Add(Author);
			registerGroup.Controls.Add(ProductTitle);
			registerGroup.Controls.Add(ProductName);
			registerGroup.Location = new System.Drawing.Point(0, 990);
			registerGroup.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			registerGroup.Name = "registerGroup";
			registerGroup.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			registerGroup.Size = new System.Drawing.Size(748, 646);
			registerGroup.TabIndex = 19;
			registerGroup.TabStop = false;
			registerGroup.Text = "Product Registration";
			// 
			// ProductPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(Management);
			Controls.Add(registerGroup);
			Controls.Add(products);
			Controls.Add(label1);
			Controls.Add(SearchAccount);
			Controls.Add(search);
			Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Name = "ProductPanel";
			Size = new System.Drawing.Size(1902, 1638);
			Management.ResumeLayout(false);
			Management.PerformLayout();
			registerGroup.ResumeLayout(false);
			registerGroup.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox SearchAccount;
		private System.Windows.Forms.Button search;
		private System.Windows.Forms.ListView products;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ComboBox PublisherAccount;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button RemovePublisher;
		private System.Windows.Forms.Button AddPublisher;
		private System.Windows.Forms.Button Change;
		private System.Windows.Forms.ListBox Publishers;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.GroupBox Management;
		private System.Windows.Forms.TextBox ProductName;
		private System.Windows.Forms.TextBox ProductTitle;
		private System.Windows.Forms.ComboBox Author;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button Register;
		private System.Windows.Forms.GroupBox registerGroup;
	}
}
