namespace Uccs.Iam.FUI;

partial class WalletsPage
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
		Wallets = new ListBox();
		DeleteWallet = new Button();
		LockUnlock = new Button();
		CreateWallet = new Button();
		CreateAccount = new Button();
		ShowSecret = new Button();
		DeleteAccount = new Button();
		CopyAddress = new Button();
		ImportWallet = new Button();
		RenameWallet = new Button();
		label3 = new Label();
		label1 = new Label();
		ImportAccount = new Button();
		RenameAccount = new Button();
		listView1 = new ListView();
		columnHeader1 = new ColumnHeader();
		columnHeader2 = new ColumnHeader();
		SuspendLayout();
		// 
		// Wallets
		// 
		Wallets.FormattingEnabled = true;
		Wallets.Location = new Point(0, 48);
		Wallets.Name = "Wallets";
		Wallets.Size = new Size(233, 184);
		Wallets.TabIndex = 1;
		// 
		// DeleteWallet
		// 
		DeleteWallet.Location = new Point(381, 200);
		DeleteWallet.Name = "DeleteWallet";
		DeleteWallet.Size = new Size(124, 32);
		DeleteWallet.TabIndex = 8;
		DeleteWallet.Text = "Delete";
		DeleteWallet.UseVisualStyleBackColor = true;
		// 
		// LockUnlock
		// 
		LockUnlock.Location = new Point(251, 162);
		LockUnlock.Name = "LockUnlock";
		LockUnlock.Size = new Size(254, 32);
		LockUnlock.TabIndex = 8;
		LockUnlock.Text = "Unlock";
		LockUnlock.UseVisualStyleBackColor = true;
		// 
		// CreateWallet
		// 
		CreateWallet.Location = new Point(251, 48);
		CreateWallet.Name = "CreateWallet";
		CreateWallet.Size = new Size(254, 32);
		CreateWallet.TabIndex = 13;
		CreateWallet.Text = "Create";
		CreateWallet.UseVisualStyleBackColor = true;
		// 
		// CreateAccount
		// 
		CreateAccount.Location = new Point(520, 323);
		CreateAccount.Name = "CreateAccount";
		CreateAccount.Size = new Size(211, 32);
		CreateAccount.TabIndex = 12;
		CreateAccount.Text = "Create";
		CreateAccount.UseVisualStyleBackColor = true;
		// 
		// ShowSecret
		// 
		ShowSecret.Location = new Point(520, 527);
		ShowSecret.Name = "ShowSecret";
		ShowSecret.Size = new Size(211, 32);
		ShowSecret.TabIndex = 10;
		ShowSecret.Text = "Show Secret (Private Key)";
		ShowSecret.UseVisualStyleBackColor = true;
		// 
		// DeleteAccount
		// 
		DeleteAccount.Location = new Point(520, 565);
		DeleteAccount.Name = "DeleteAccount";
		DeleteAccount.Size = new Size(211, 32);
		DeleteAccount.TabIndex = 11;
		DeleteAccount.Text = "Delete";
		DeleteAccount.UseVisualStyleBackColor = true;
		// 
		// CopyAddress
		// 
		CopyAddress.Location = new Point(520, 451);
		CopyAddress.Name = "CopyAddress";
		CopyAddress.Size = new Size(211, 32);
		CopyAddress.TabIndex = 9;
		CopyAddress.Text = "Copy Address to Clipboard";
		CopyAddress.UseVisualStyleBackColor = true;
		// 
		// ImportWallet
		// 
		ImportWallet.Location = new Point(251, 86);
		ImportWallet.Name = "ImportWallet";
		ImportWallet.Size = new Size(254, 32);
		ImportWallet.TabIndex = 13;
		ImportWallet.Text = "Import";
		ImportWallet.UseVisualStyleBackColor = true;
		// 
		// RenameWallet
		// 
		RenameWallet.Location = new Point(251, 200);
		RenameWallet.Name = "RenameWallet";
		RenameWallet.Size = new Size(124, 32);
		RenameWallet.TabIndex = 8;
		RenameWallet.Text = "Rename";
		RenameWallet.UseVisualStyleBackColor = true;
		// 
		// label3
		// 
		label3.BackColor = SystemColors.ControlDarkDark;
		label3.Font = new Font("Segoe UI", 11F);
		label3.ForeColor = SystemColors.Control;
		label3.Location = new Point(0, 0);
		label3.Name = "label3";
		label3.Padding = new Padding(8, 0, 0, 0);
		label3.Size = new Size(800, 32);
		label3.TabIndex = 14;
		label3.Text = "Wallets";
		label3.TextAlign = ContentAlignment.MiddleLeft;
		// 
		// label1
		// 
		label1.BackColor = SystemColors.ControlDarkDark;
		label1.Font = new Font("Segoe UI", 12F);
		label1.ForeColor = SystemColors.Control;
		label1.Location = new Point(0, 273);
		label1.Name = "label1";
		label1.Padding = new Padding(8, 0, 0, 0);
		label1.Size = new Size(800, 32);
		label1.TabIndex = 14;
		label1.Text = "Accounts";
		label1.TextAlign = ContentAlignment.MiddleLeft;
		// 
		// ImportAccount
		// 
		ImportAccount.Location = new Point(520, 361);
		ImportAccount.Name = "ImportAccount";
		ImportAccount.Size = new Size(211, 32);
		ImportAccount.TabIndex = 11;
		ImportAccount.Text = "Import";
		ImportAccount.UseVisualStyleBackColor = true;
		// 
		// RenameAccount
		// 
		RenameAccount.Location = new Point(520, 489);
		RenameAccount.Name = "RenameAccount";
		RenameAccount.Size = new Size(211, 32);
		RenameAccount.TabIndex = 8;
		RenameAccount.Text = "Rename";
		RenameAccount.UseVisualStyleBackColor = true;
		// 
		// listView1
		// 
		listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
		listView1.Location = new Point(3, 323);
		listView1.Name = "listView1";
		listView1.Size = new Size(502, 274);
		listView1.TabIndex = 15;
		listView1.UseCompatibleStateImageBehavior = false;
		listView1.View = View.Details;
		// 
		// columnHeader1
		// 
		columnHeader1.Text = "Name";
		columnHeader1.Width = 150;
		// 
		// columnHeader2
		// 
		columnHeader2.Text = "Address";
		columnHeader2.Width = 300;
		// 
		// WalletsPage
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		Controls.Add(listView1);
		Controls.Add(label1);
		Controls.Add(label3);
		Controls.Add(Wallets);
		Controls.Add(DeleteWallet);
		Controls.Add(CopyAddress);
		Controls.Add(LockUnlock);
		Controls.Add(RenameAccount);
		Controls.Add(RenameWallet);
		Controls.Add(CreateAccount);
		Controls.Add(ImportWallet);
		Controls.Add(CreateWallet);
		Controls.Add(ShowSecret);
		Controls.Add(ImportAccount);
		Controls.Add(DeleteAccount);
		Name = "WalletsPage";
		Size = new Size(800, 600);
		ResumeLayout(false);
	}

	#endregion
	private ListBox Wallets;
	private Button DeleteWallet;
	private Button LockUnlock;
	private Button CreateWallet;
	private Button CreateAccount;
	private Button ShowSecret;
	private Button DeleteAccount;
	private Button CopyAddress;
	private Button ImportWallet;
	private Button RenameWallet;
	private Label label3;
	private Label label1;
	private Button ImportAccount;
	private Button RenameAccount;
	private ListView listView1;
	private ColumnHeader columnHeader1;
	private ColumnHeader columnHeader2;
}
