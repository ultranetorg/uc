
namespace UC.Net.Node.FUI
{
	partial class TransactionsPanel
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
			this.Transactions = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnId = new System.Windows.Forms.ColumnHeader();
			this.columnRound = new System.Windows.Forms.ColumnHeader();
			this.columnStatus = new System.Windows.Forms.ColumnHeader();
			this.columnPlacedBy = new System.Windows.Forms.ColumnHeader();
			this.label1 = new System.Windows.Forms.Label();
			this.Account = new System.Windows.Forms.ComboBox();
			this.SearchButton = new System.Windows.Forms.Button();
			this.Operations = new System.Windows.Forms.ListView();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// Transactions
			// 
			this.Transactions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Transactions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnId,
            this.columnRound,
            this.columnStatus,
            this.columnPlacedBy});
			this.Transactions.FullRowSelect = true;
			this.Transactions.HideSelection = false;
			this.Transactions.Location = new System.Drawing.Point(0, 65);
			this.Transactions.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Transactions.Name = "Transactions";
			this.Transactions.Size = new System.Drawing.Size(1024, 511);
			this.Transactions.TabIndex = 1;
			this.Transactions.UseCompatibleStateImageBehavior = false;
			this.Transactions.View = System.Windows.Forms.View.Details;
			this.Transactions.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.Transactions_ItemSelectionChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "";
			this.columnHeader1.Width = 10;
			// 
			// columnId
			// 
			this.columnId.Text = "Id";
			this.columnId.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnId.Width = 50;
			// 
			// columnRound
			// 
			this.columnRound.Text = "Round";
			this.columnRound.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// columnStatus
			// 
			this.columnStatus.Text = "Status";
			this.columnStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnStatus.Width = 100;
			// 
			// columnPlacedBy
			// 
			this.columnPlacedBy.Text = "Placed By";
			this.columnPlacedBy.Width = 300;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(19, 18);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(53, 13);
			this.label1.TabIndex = 19;
			this.label1.Text = "Account";
			// 
			// Account
			// 
			this.Account.FormattingEnabled = true;
			this.Account.Location = new System.Drawing.Point(88, 15);
			this.Account.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Account.Name = "Account";
			this.Account.Size = new System.Drawing.Size(324, 23);
			this.Account.TabIndex = 20;
			this.Account.SelectedIndexChanged += new System.EventHandler(this.account_SelectedIndexChanged);
			this.Account.SelectionChangeCommitted += new System.EventHandler(this.account_SelectionChangeCommitted);
			// 
			// SearchButton
			// 
			this.SearchButton.Location = new System.Drawing.Point(432, 13);
			this.SearchButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.SearchButton.Name = "SearchButton";
			this.SearchButton.Size = new System.Drawing.Size(117, 27);
			this.SearchButton.TabIndex = 18;
			this.SearchButton.Text = "Search";
			this.SearchButton.UseVisualStyleBackColor = true;
			this.SearchButton.Click += new System.EventHandler(this.search_Click);
			// 
			// Operations
			// 
			this.Operations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Operations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6});
			this.Operations.FullRowSelect = true;
			this.Operations.HideSelection = false;
			this.Operations.Location = new System.Drawing.Point(0, 594);
			this.Operations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Operations.Name = "Operations";
			this.Operations.Size = new System.Drawing.Size(1023, 173);
			this.Operations.TabIndex = 24;
			this.Operations.UseCompatibleStateImageBehavior = false;
			this.Operations.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Operation";
			this.columnHeader5.Width = 300;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Status";
			this.columnHeader6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader6.Width = 130;
			// 
			// TransactionsPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Operations);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.Account);
			this.Controls.Add(this.SearchButton);
			this.Controls.Add(this.Transactions);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "TransactionsPanel";
			this.Size = new System.Drawing.Size(1024, 768);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView Transactions;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnId;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox Account;
		private System.Windows.Forms.Button SearchButton;
		private System.Windows.Forms.ColumnHeader columnStatus;
		private System.Windows.Forms.ColumnHeader columnPlacedBy;
		private System.Windows.Forms.ListView Operations;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnRound;
	}
}
