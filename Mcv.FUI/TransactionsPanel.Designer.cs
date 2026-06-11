
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
			ChError = new ColumnHeader();
			ChMemberPeer = new ColumnHeader();
			Operations = new ListView();
			columnHeader5 = new ColumnHeader();
			label7 = new Label();
			label1 = new Label();
			Reload = new Button();
			label2 = new Label();
			Log = new TextBox();
			SuspendLayout();
			// 
			// Transactions
			// 
			Transactions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			Transactions.Columns.AddRange(new ColumnHeader[] { ChTag, ChStatus, ChUser, ChNonce, ChExpiration, ChOpsn, ChFirstOp, ChError, ChMemberPeer });
			Transactions.FullRowSelect = true;
			Transactions.Location = new Point(4, 43);
			Transactions.Margin = new Padding(3, 9, 3, 3);
			Transactions.Name = "Transactions";
			Transactions.Size = new Size(1016, 367);
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
			// ChError
			// 
			ChError.Text = "Error";
			// 
			// ChMemberPeer
			// 
			ChMemberPeer.Text = "MemberPeer";
			// 
			// Operations
			// 
			Operations.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Operations.Columns.AddRange(new ColumnHeader[] { columnHeader5 });
			Operations.FullRowSelect = true;
			Operations.Location = new Point(4, 459);
			Operations.Margin = new Padding(3, 9, 3, 3);
			Operations.Name = "Operations";
			Operations.Size = new Size(1016, 139);
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
			label7.Location = new Point(4, 19);
			label7.Margin = new Padding(0, 0, 3, 0);
			label7.Name = "label7";
			label7.Size = new Size(130, 15);
			label7.TabIndex = 34;
			label7.Text = "Outgoing Transactions";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label1.Location = new Point(4, 435);
			label1.Margin = new Padding(0, 0, 3, 0);
			label1.Name = "label1";
			label1.Size = new Size(68, 15);
			label1.TabIndex = 34;
			label1.Text = "Operations";
			// 
			// Refresh
			// 
			Reload.Location = new Point(891, 3);
			Reload.Name = "Refresh";
			Reload.Size = new Size(129, 28);
			Reload.TabIndex = 35;
			Reload.Text = "Refresh";
			Reload.UseVisualStyleBackColor = true;
			Reload.Click += Refresh_Click;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label2.Location = new Point(4, 620);
			label2.Margin = new Padding(0, 0, 3, 0);
			label2.Name = "label2";
			label2.Size = new Size(27, 15);
			label2.TabIndex = 34;
			label2.Text = "Log";
			// 
			// Log
			// 
			Log.Location = new Point(4, 644);
			Log.Margin = new Padding(3, 9, 3, 3);
			Log.Multiline = true;
			Log.Name = "Log";
			Log.ReadOnly = true;
			Log.Size = new Size(1016, 121);
			Log.TabIndex = 36;
			// 
			// TransactionsPanel
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(Log);
			Controls.Add(Reload);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(label7);
			Controls.Add(Operations);
			Controls.Add(Transactions);
			Margin = new Padding(4, 3, 4, 3);
			Name = "TransactionsPanel";
			Size = new Size(1024, 768);
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
		private Button Reload;
		private ColumnHeader ChError;
		private ColumnHeader ChMemberPeer;
		private Label label2;
		private TextBox Log;
	}
}
