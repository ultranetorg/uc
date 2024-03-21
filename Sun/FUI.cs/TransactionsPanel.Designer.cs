
namespace Uccs.Sun.FUI
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
			Transactions = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnPlacedBy = new System.Windows.Forms.ColumnHeader();
			label1 = new System.Windows.Forms.Label();
			Account = new System.Windows.Forms.ComboBox();
			SearchButton = new System.Windows.Forms.Button();
			Operations = new System.Windows.Forms.ListView();
			columnHeader5 = new System.Windows.Forms.ColumnHeader();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			SuspendLayout();
			// 
			// Transactions
			// 
			Transactions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			Transactions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader2, columnHeader1, columnPlacedBy });
			Transactions.FullRowSelect = true;
			Transactions.Location = new System.Drawing.Point(0, 139);
			Transactions.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Transactions.Name = "Transactions";
			Transactions.Size = new System.Drawing.Size(761, 1499);
			Transactions.TabIndex = 1;
			Transactions.UseCompatibleStateImageBehavior = false;
			Transactions.View = System.Windows.Forms.View.Details;
			Transactions.ItemSelectionChanged += Transactions_ItemSelectionChanged;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Round";
			columnHeader1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// columnPlacedBy
			// 
			columnPlacedBy.Text = "Generator";
			columnPlacedBy.Width = 300;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label1.Location = new System.Drawing.Point(35, 38);
			label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(102, 27);
			label1.TabIndex = 19;
			label1.Text = "Account";
			// 
			// Account
			// 
			Account.FormattingEnabled = true;
			Account.Location = new System.Drawing.Point(163, 32);
			Account.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Account.Name = "Account";
			Account.Size = new System.Drawing.Size(598, 40);
			Account.TabIndex = 20;
			Account.SelectedIndexChanged += account_SelectedIndexChanged;
			Account.SelectionChangeCommitted += account_SelectionChangeCommitted;
			// 
			// SearchButton
			// 
			SearchButton.Location = new System.Drawing.Point(802, 28);
			SearchButton.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			SearchButton.Name = "SearchButton";
			SearchButton.Size = new System.Drawing.Size(217, 58);
			SearchButton.TabIndex = 18;
			SearchButton.Text = "Search";
			SearchButton.UseVisualStyleBackColor = true;
			SearchButton.Click += search_Click;
			// 
			// Operations
			// 
			Operations.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Operations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader5 });
			Operations.FullRowSelect = true;
			Operations.Location = new System.Drawing.Point(802, 139);
			Operations.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Operations.Name = "Operations";
			Operations.Size = new System.Drawing.Size(1100, 1499);
			Operations.TabIndex = 24;
			Operations.UseCompatibleStateImageBehavior = false;
			Operations.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader5
			// 
			columnHeader5.Text = "Id";
			columnHeader5.Width = 400;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Id";
			// 
			// TransactionsPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(Operations);
			Controls.Add(label1);
			Controls.Add(Account);
			Controls.Add(SearchButton);
			Controls.Add(Transactions);
			Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Name = "TransactionsPanel";
			Size = new System.Drawing.Size(1902, 1638);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.ListView Transactions;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox Account;
		private System.Windows.Forms.Button SearchButton;
		private System.Windows.Forms.ColumnHeader columnPlacedBy;
		private System.Windows.Forms.ListView Operations;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader2;
	}
}
