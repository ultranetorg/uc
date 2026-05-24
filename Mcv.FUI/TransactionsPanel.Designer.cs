
namespace Uccs.Mcv.FUI
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
			Transactions = new ListView();
			ChTag = new ColumnHeader();
			ChStatus = new ColumnHeader();
			ChUser = new ColumnHeader();
			ChNonce = new ColumnHeader();
			ChExpiration = new ColumnHeader();
			ChOpsn = new ColumnHeader();
			ChFirstOp = new ColumnHeader();
			Operations = new ListView();
			columnHeader5 = new ColumnHeader();
			label7 = new Label();
			label1 = new Label();
			Refresh = new Button();
			SuspendLayout();
			// 
			// Transactions
			// 
			Transactions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			Transactions.Columns.AddRange(new ColumnHeader[] { ChTag, ChStatus, ChUser, ChNonce, ChExpiration, ChOpsn, ChFirstOp });
			Transactions.FullRowSelect = true;
			Transactions.Location = new Point(0, 80);
			Transactions.Margin = new Padding(6, 12, 6, 6);
			Transactions.Name = "Transactions";
			Transactions.Size = new Size(1895, 789);
			Transactions.TabIndex = 1;
			Transactions.UseCompatibleStateImageBehavior = false;
			Transactions.View = View.Details;
			Transactions.ItemSelectionChanged += Transactions_ItemSelectionChanged;
			// 
			// ChTag
			// 
			ChTag.Text = "Tag";
			ChTag.Width = 200;
			// 
			// ChStatus
			// 
			ChStatus.Text = "Status";
			ChStatus.TextAlign = HorizontalAlignment.Center;
			// 
			// ChUser
			// 
			ChUser.Text = "User";
			ChUser.Width = 100;
			// 
			// ChNonce
			// 
			ChNonce.Text = "Nonce";
			ChNonce.TextAlign = HorizontalAlignment.Right;
			// 
			// ChExpiration
			// 
			ChExpiration.Text = "Expiration";
			ChExpiration.TextAlign = HorizontalAlignment.Right;
			// 
			// ChOpsn
			// 
			ChOpsn.Text = "Ops;(n)";
			ChOpsn.TextAlign = HorizontalAlignment.Right;
			// 
			// ChFirstOp
			// 
			ChFirstOp.Text = "First Operation";
			ChFirstOp.Width = 300;
			// 
			// Operations
			// 
			Operations.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Operations.Columns.AddRange(new ColumnHeader[] { columnHeader5 });
			Operations.FullRowSelect = true;
			Operations.Location = new Point(7, 979);
			Operations.Margin = new Padding(6, 12, 6, 6);
			Operations.Name = "Operations";
			Operations.Size = new Size(1888, 653);
			Operations.TabIndex = 24;
			Operations.UseCompatibleStateImageBehavior = false;
			Operations.View = View.Details;
			// 
			// columnHeader5
			// 
			columnHeader5.Text = "Id";
			columnHeader5.Width = 600;
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label7.Location = new Point(7, 18);
			label7.Margin = new Padding(6, 12, 6, 12);
			label7.Name = "label7";
			label7.Size = new Size(273, 32);
			label7.TabIndex = 34;
			label7.Text = "Outgoing Transactions";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label1.Location = new Point(6, 923);
			label1.Margin = new Padding(6, 12, 6, 12);
			label1.Name = "label1";
			label1.Size = new Size(140, 32);
			label1.TabIndex = 34;
			label1.Text = "Operations";
			// 
			// Refresh
			// 
			Refresh.Location = new Point(1655, 6);
			Refresh.Margin = new Padding(6);
			Refresh.Name = "Refresh";
			Refresh.Size = new Size(240, 56);
			Refresh.TabIndex = 35;
			Refresh.Text = "Refresh";
			Refresh.UseVisualStyleBackColor = true;
			Refresh.Click += Refresh_Click;
			// 
			// TransactionsPanel
			// 
			AutoScaleDimensions = new SizeF(13F, 32F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(Refresh);
			Controls.Add(label1);
			Controls.Add(label7);
			Controls.Add(Operations);
			Controls.Add(Transactions);
			Margin = new Padding(7, 6, 7, 6);
			Name = "TransactionsPanel";
			Size = new Size(1902, 1638);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.ListView Transactions;
		private System.Windows.Forms.ColumnHeader ChNonce;
		private System.Windows.Forms.ColumnHeader ChStatus;
		private System.Windows.Forms.ListView Operations;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader ChUser;
		private ColumnHeader ChOpsn;
		private ColumnHeader ChExpiration;
		private ColumnHeader ChTag;
		private ColumnHeader ChFirstOp;
		private Label label7;
		private Label label1;
		private Button Refresh;
	}
}
