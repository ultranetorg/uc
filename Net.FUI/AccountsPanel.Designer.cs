
namespace Uccs.Net.FUI
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
			this.Accounts = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// Remove
			// 
			this.Remove.Enabled = false;
			this.Remove.Location = new System.Drawing.Point(349, 77);
			this.Remove.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Remove.Name = "Remove";
			this.Remove.Size = new System.Drawing.Size(277, 60);
			this.Remove.TabIndex = 1;
			this.Remove.Text = "Remove Account";
			this.Remove.UseVisualStyleBackColor = true;
			this.Remove.Click += new System.EventHandler(this.remove_Click);
			// 
			// add
			// 
			this.add.Location = new System.Drawing.Point(45, 77);
			this.add.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.add.Name = "add";
			this.add.Size = new System.Drawing.Size(277, 60);
			this.add.TabIndex = 1;
			this.add.Text = "Add Account";
			this.add.UseVisualStyleBackColor = true;
			this.add.Click += new System.EventHandler(this.add_Click);
			// 
			// Showprivate
			// 
			this.Showprivate.Enabled = false;
			this.Showprivate.Location = new System.Drawing.Point(45, 254);
			this.Showprivate.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Showprivate.Name = "Showprivate";
			this.Showprivate.Size = new System.Drawing.Size(581, 60);
			this.Showprivate.TabIndex = 1;
			this.Showprivate.Text = "Show Private Key";
			this.Showprivate.UseVisualStyleBackColor = true;
			this.Showprivate.Click += new System.EventHandler(this.showprivate_Click);
			// 
			// Backup
			// 
			this.Backup.Enabled = false;
			this.Backup.Location = new System.Drawing.Point(45, 341);
			this.Backup.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Backup.Name = "Backup";
			this.Backup.Size = new System.Drawing.Size(581, 60);
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
			this.groupBox2.Location = new System.Drawing.Point(0, 1167);
			this.groupBox2.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.groupBox2.Size = new System.Drawing.Size(683, 469);
			this.groupBox2.TabIndex = 4;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Manage Account";
			// 
			// CopyAddress
			// 
			this.CopyAddress.Enabled = false;
			this.CopyAddress.Location = new System.Drawing.Point(45, 164);
			this.CopyAddress.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.CopyAddress.Name = "CopyAddress";
			this.CopyAddress.Size = new System.Drawing.Size(581, 60);
			this.CopyAddress.TabIndex = 1;
			this.CopyAddress.Text = "Copy Address To Clipboard";
			this.CopyAddress.UseVisualStyleBackColor = true;
			this.CopyAddress.Click += new System.EventHandler(this.CopyAddress_Click);
			// 
			// accounts
			// 
			this.Accounts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Accounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.Accounts.FullRowSelect = true;
			this.Accounts.Location = new System.Drawing.Point(0, 0);
			this.Accounts.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Accounts.Name = "accounts";
			this.Accounts.Size = new System.Drawing.Size(1896, 1147);
			this.Accounts.TabIndex = 5;
			this.Accounts.UseCompatibleStateImageBehavior = false;
			this.Accounts.View = System.Windows.Forms.View.Details;
			this.Accounts.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.accounts_ItemSelectionChanged);
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
			// AccountsPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Accounts);
			this.Controls.Add(this.groupBox2);
			this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Name = "AccountsPanel";
			this.Size = new System.Drawing.Size(1902, 1638);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button Remove;
		private System.Windows.Forms.Button add;
		private System.Windows.Forms.Button Showprivate;
		private System.Windows.Forms.Button Backup;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ListView Accounts;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button CopyAddress;
	}
}
