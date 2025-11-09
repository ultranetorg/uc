namespace Uccs.Nexus.Windows;

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
		components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WalletsPage));
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
		Accounts = new ListView();
		columnHeader1 = new ColumnHeader();
		columnHeader2 = new ColumnHeader();
		imageList1 = new ImageList(components);
		Wallets = new ListView();
		columnHeader3 = new ColumnHeader();
		columnHeader4 = new ColumnHeader();
		AccountsPanel = new Panel();
		AccountsPanel.SuspendLayout();
		SuspendLayout();
		// 
		// DeleteWallet
		// 
		DeleteWallet.Location = new Point(381, 200);
		DeleteWallet.Name = "DeleteWallet";
		DeleteWallet.Size = new Size(124, 32);
		DeleteWallet.TabIndex = 8;
		DeleteWallet.Text = "Delete";
		DeleteWallet.UseVisualStyleBackColor = true;
		DeleteWallet.Click += DeleteWallet_Click;
		// 
		// LockUnlock
		// 
		LockUnlock.Location = new Point(251, 162);
		LockUnlock.Name = "LockUnlock";
		LockUnlock.Size = new Size(254, 32);
		LockUnlock.TabIndex = 8;
		LockUnlock.Text = "Unlock";
		LockUnlock.UseVisualStyleBackColor = true;
		LockUnlock.EnabledChanged += LockUnlock_EnabledChanged;
		LockUnlock.Click += LockUnlock_Click;
		// 
		// CreateWallet
		// 
		CreateWallet.Location = new Point(251, 48);
		CreateWallet.Name = "CreateWallet";
		CreateWallet.Size = new Size(254, 32);
		CreateWallet.TabIndex = 13;
		CreateWallet.Text = "Create";
		CreateWallet.UseVisualStyleBackColor = true;
		CreateWallet.Click += CreateWallet_Click;
		// 
		// CreateAccount
		// 
		CreateAccount.Location = new Point(520, 49);
		CreateAccount.Name = "CreateAccount";
		CreateAccount.Size = new Size(211, 32);
		CreateAccount.TabIndex = 12;
		CreateAccount.Text = "Create";
		CreateAccount.UseVisualStyleBackColor = true;
		CreateAccount.Click += CreateAccount_Click;
		// 
		// ShowSecret
		// 
		ShowSecret.Location = new Point(520, 255);
		ShowSecret.Name = "ShowSecret";
		ShowSecret.Size = new Size(211, 32);
		ShowSecret.TabIndex = 10;
		ShowSecret.Text = "Show Secret (Private Key)";
		ShowSecret.UseVisualStyleBackColor = true;
		ShowSecret.Click += ShowSecret_Click;
		// 
		// DeleteAccount
		// 
		DeleteAccount.Location = new Point(520, 293);
		DeleteAccount.Name = "DeleteAccount";
		DeleteAccount.Size = new Size(211, 32);
		DeleteAccount.TabIndex = 11;
		DeleteAccount.Text = "Delete";
		DeleteAccount.UseVisualStyleBackColor = true;
		DeleteAccount.Click += DeleteAccount_Click;
		// 
		// CopyAddress
		// 
		CopyAddress.Location = new Point(520, 179);
		CopyAddress.Name = "CopyAddress";
		CopyAddress.Size = new Size(211, 32);
		CopyAddress.TabIndex = 9;
		CopyAddress.Text = "Copy Address to Clipboard";
		CopyAddress.UseVisualStyleBackColor = true;
		CopyAddress.Click += CopyAddress_Click;
		// 
		// ImportWallet
		// 
		ImportWallet.Location = new Point(251, 86);
		ImportWallet.Name = "ImportWallet";
		ImportWallet.Size = new Size(254, 32);
		ImportWallet.TabIndex = 13;
		ImportWallet.Text = "Import";
		ImportWallet.UseVisualStyleBackColor = true;
		ImportWallet.Click += ImportWallet_Click;
		// 
		// RenameWallet
		// 
		RenameWallet.Location = new Point(251, 200);
		RenameWallet.Name = "RenameWallet";
		RenameWallet.Size = new Size(124, 32);
		RenameWallet.TabIndex = 8;
		RenameWallet.Text = "Rename";
		RenameWallet.UseVisualStyleBackColor = true;
		RenameWallet.Click += RenameWallet_Click;
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
		label1.Location = new Point(0, 0);
		label1.Name = "label1";
		label1.Padding = new Padding(8, 0, 0, 0);
		label1.Size = new Size(800, 32);
		label1.TabIndex = 14;
		label1.Text = "Accounts";
		label1.TextAlign = ContentAlignment.MiddleLeft;
		// 
		// ImportAccount
		// 
		ImportAccount.Location = new Point(520, 87);
		ImportAccount.Name = "ImportAccount";
		ImportAccount.Size = new Size(211, 32);
		ImportAccount.TabIndex = 11;
		ImportAccount.Text = "Import";
		ImportAccount.UseVisualStyleBackColor = true;
		ImportAccount.Click += ImportAccount_Click;
		// 
		// RenameAccount
		// 
		RenameAccount.Location = new Point(520, 217);
		RenameAccount.Name = "RenameAccount";
		RenameAccount.Size = new Size(211, 32);
		RenameAccount.TabIndex = 8;
		RenameAccount.Text = "Rename";
		RenameAccount.UseVisualStyleBackColor = true;
		RenameAccount.Click += RenameAccount_Click;
		// 
		// Accounts
		// 
		Accounts.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
		Accounts.FullRowSelect = true;
		Accounts.Location = new Point(3, 49);
		Accounts.Name = "Accounts";
		Accounts.Size = new Size(505, 276);
		Accounts.TabIndex = 15;
		Accounts.UseCompatibleStateImageBehavior = false;
		Accounts.View = View.Details;
		Accounts.ItemSelectionChanged += Accounts_ItemSelectionChanged;
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
		// imageList1
		// 
		imageList1.ColorDepth = ColorDepth.Depth32Bit;
		imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
		imageList1.TransparentColor = Color.Transparent;
		imageList1.Images.SetKeyName(0, "lock");
		// 
		// Wallets
		// 
		Wallets.Columns.AddRange(new ColumnHeader[] { columnHeader3, columnHeader4 });
		Wallets.FullRowSelect = true;
		Wallets.Location = new Point(3, 48);
		Wallets.Name = "Wallets";
		Wallets.Size = new Size(234, 184);
		Wallets.TabIndex = 16;
		Wallets.UseCompatibleStateImageBehavior = false;
		Wallets.View = View.Details;
		Wallets.ItemSelectionChanged += Wallets_ItemSelectionChanged;
		// 
		// columnHeader3
		// 
		columnHeader3.Text = "";
		columnHeader3.Width = 30;
		// 
		// columnHeader4
		// 
		columnHeader4.Text = "Name";
		columnHeader4.Width = 150;
		// 
		// AccountsPanel
		// 
		AccountsPanel.Controls.Add(Accounts);
		AccountsPanel.Controls.Add(DeleteAccount);
		AccountsPanel.Controls.Add(ImportAccount);
		AccountsPanel.Controls.Add(label1);
		AccountsPanel.Controls.Add(ShowSecret);
		AccountsPanel.Controls.Add(CreateAccount);
		AccountsPanel.Controls.Add(RenameAccount);
		AccountsPanel.Controls.Add(CopyAddress);
		AccountsPanel.Location = new Point(0, 272);
		AccountsPanel.Name = "AccountsPanel";
		AccountsPanel.Size = new Size(800, 328);
		AccountsPanel.TabIndex = 17;
		// 
		// WalletsPage
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		Controls.Add(AccountsPanel);
		Controls.Add(Wallets);
		Controls.Add(label3);
		Controls.Add(DeleteWallet);
		Controls.Add(LockUnlock);
		Controls.Add(RenameWallet);
		Controls.Add(ImportWallet);
		Controls.Add(CreateWallet);
		Name = "WalletsPage";
		Size = new Size(800, 600);
		AccountsPanel.ResumeLayout(false);
		ResumeLayout(false);
	}

	#endregion
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
	private ListView Accounts;
	private ColumnHeader columnHeader1;
	private ColumnHeader columnHeader2;
	private ImageList imageList1;
	private ListView Wallets;
	private ColumnHeader columnHeader3;
	private ColumnHeader columnHeader4;
	private Panel AccountsPanel;
}
