
namespace UC.Net.Node.FUI
{
	partial class AccountsPanel
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
			this.Remove = new System.Windows.Forms.Button();
			this.add = new System.Windows.Forms.Button();
			this.Showprivate = new System.Windows.Forms.Button();
			this.Backup = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.CopyAddress = new System.Windows.Forms.Button();
			this.accounts = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// Remove
			// 
			this.Remove.Enabled = false;
			this.Remove.Location = new System.Drawing.Point(188, 36);
			this.Remove.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.Remove.Name = "Remove";
			this.Remove.Size = new System.Drawing.Size(149, 28);
			this.Remove.TabIndex = 1;
			this.Remove.Text = "Remove Account";
			this.Remove.UseVisualStyleBackColor = true;
			this.Remove.Click += new System.EventHandler(this.remove_Click);
			// 
			// add
			// 
			this.add.Location = new System.Drawing.Point(24, 36);
			this.add.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.add.Name = "add";
			this.add.Size = new System.Drawing.Size(149, 28);
			this.add.TabIndex = 1;
			this.add.Text = "Add Account";
			this.add.UseVisualStyleBackColor = true;
			this.add.Click += new System.EventHandler(this.add_Click);
			// 
			// Showprivate
			// 
			this.Showprivate.Enabled = false;
			this.Showprivate.Location = new System.Drawing.Point(24, 119);
			this.Showprivate.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.Showprivate.Name = "Showprivate";
			this.Showprivate.Size = new System.Drawing.Size(313, 28);
			this.Showprivate.TabIndex = 1;
			this.Showprivate.Text = "Show Private Key";
			this.Showprivate.UseVisualStyleBackColor = true;
			this.Showprivate.Click += new System.EventHandler(this.showprivate_Click);
			// 
			// Backup
			// 
			this.Backup.Enabled = false;
			this.Backup.Location = new System.Drawing.Point(24, 160);
			this.Backup.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.Backup.Name = "Backup";
			this.Backup.Size = new System.Drawing.Size(313, 28);
			this.Backup.TabIndex = 1;
			this.Backup.Text = "Back up Wallet File";
			this.Backup.UseVisualStyleBackColor = true;
			this.Backup.Click += new System.EventHandler(this.backup_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox2.Controls.Add(this.Backup);
			this.groupBox2.Controls.Add(this.add);
			this.groupBox2.Controls.Add(this.CopyAddress);
			this.groupBox2.Controls.Add(this.Remove);
			this.groupBox2.Controls.Add(this.Showprivate);
			this.groupBox2.Location = new System.Drawing.Point(0, 547);
			this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.groupBox2.Size = new System.Drawing.Size(368, 220);
			this.groupBox2.TabIndex = 4;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Manage Account";
			// 
			// CopyAddress
			// 
			this.CopyAddress.Enabled = false;
			this.CopyAddress.Location = new System.Drawing.Point(24, 77);
			this.CopyAddress.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.CopyAddress.Name = "CopyAddress";
			this.CopyAddress.Size = new System.Drawing.Size(313, 28);
			this.CopyAddress.TabIndex = 1;
			this.CopyAddress.Text = "Copy Address To Clipboard";
			this.CopyAddress.UseVisualStyleBackColor = true;
			this.CopyAddress.Click += new System.EventHandler(this.CopyAddress_Click);
			// 
			// accounts
			// 
			this.accounts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.accounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
			this.accounts.FullRowSelect = true;
			this.accounts.HideSelection = false;
			this.accounts.Location = new System.Drawing.Point(0, 0);
			this.accounts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.accounts.Name = "accounts";
			this.accounts.Size = new System.Drawing.Size(1023, 540);
			this.accounts.TabIndex = 5;
			this.accounts.UseCompatibleStateImageBehavior = false;
			this.accounts.View = System.Windows.Forms.View.Details;
			this.accounts.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.accounts_ItemSelectionChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Account";
			this.columnHeader1.Width = 300;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Balance";
			this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader2.Width = 200;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Balance (Unconfirmed)";
			this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader3.Width = 200;
			// 
			// AccountsPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.accounts);
			this.Controls.Add(this.groupBox2);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "AccountsPanel";
			this.Size = new System.Drawing.Size(1024, 768);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button Remove;
		private System.Windows.Forms.Button add;
		private System.Windows.Forms.Button Showprivate;
		private System.Windows.Forms.Button Backup;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ListView accounts;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button CopyAddress;
		private System.Windows.Forms.ColumnHeader columnHeader3;
	}
}
