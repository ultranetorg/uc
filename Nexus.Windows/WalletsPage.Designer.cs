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
		CreateKey = new Button();
		ShowSecret = new Button();
		DeleteKey = new Button();
		CopyPublic = new Button();
		ImportWallet = new Button();
		label3 = new Label();
		label1 = new Label();
		ImportKey = new Button();
		Keys = new ListView();
		columnHeader1 = new ColumnHeader();
		columnHeader2 = new ColumnHeader();
		imageList1 = new ImageList(components);
		Wallets = new ListView();
		columnHeader4 = new ColumnHeader();
		KeysPanel = new Panel();
		Locked = new Label();
		ExportWallet = new Button();
		KeysPanel.SuspendLayout();
		SuspendLayout();
		// 
		// DeleteWallet
		// 
		DeleteWallet.Location = new Point(586, 210);
		DeleteWallet.Name = "DeleteWallet";
		DeleteWallet.Size = new Size(211, 32);
		DeleteWallet.TabIndex = 8;
		DeleteWallet.Text = "Delete";
		DeleteWallet.UseVisualStyleBackColor = true;
		DeleteWallet.Click += DeleteWallet_Click;
		// 
		// LockUnlock
		// 
		LockUnlock.Location = new Point(586, 48);
		LockUnlock.Name = "LockUnlock";
		LockUnlock.Size = new Size(211, 32);
		LockUnlock.TabIndex = 8;
		LockUnlock.Text = "Unlock";
		LockUnlock.UseVisualStyleBackColor = true;
		LockUnlock.EnabledChanged += LockUnlock_EnabledChanged;
		LockUnlock.Click += LockUnlock_Click;
		// 
		// CreateWallet
		// 
		CreateWallet.Location = new Point(3, 48);
		CreateWallet.Name = "CreateWallet";
		CreateWallet.Size = new Size(260, 32);
		CreateWallet.TabIndex = 13;
		CreateWallet.Text = "Create";
		CreateWallet.UseVisualStyleBackColor = true;
		CreateWallet.Click += CreateWallet_Click;
		// 
		// CreateKey
		// 
		CreateKey.Location = new Point(586, 49);
		CreateKey.Name = "CreateKey";
		CreateKey.Size = new Size(211, 32);
		CreateKey.TabIndex = 12;
		CreateKey.Text = "Create";
		CreateKey.UseVisualStyleBackColor = true;
		CreateKey.Click += CreateKey_Click;
		// 
		// ShowSecret
		// 
		ShowSecret.Location = new Point(586, 255);
		ShowSecret.Name = "ShowSecret";
		ShowSecret.Size = new Size(211, 32);
		ShowSecret.TabIndex = 10;
		ShowSecret.Text = "Show Secret (Private Key)";
		ShowSecret.UseVisualStyleBackColor = true;
		ShowSecret.Click += ShowSecret_Click;
		// 
		// DeleteKey
		// 
		DeleteKey.Location = new Point(586, 293);
		DeleteKey.Name = "DeleteKey";
		DeleteKey.Size = new Size(211, 32);
		DeleteKey.TabIndex = 11;
		DeleteKey.Text = "Delete";
		DeleteKey.UseVisualStyleBackColor = true;
		DeleteKey.Click += DeleteKey_Click;
		// 
		// CopyPublic
		// 
		CopyPublic.Location = new Point(586, 217);
		CopyPublic.Name = "CopyPublic";
		CopyPublic.Size = new Size(211, 32);
		CopyPublic.TabIndex = 9;
		CopyPublic.Text = "Copy Public Key to Clipboard";
		CopyPublic.UseVisualStyleBackColor = true;
		CopyPublic.Click += CopyAddress_Click;
		// 
		// ImportWallet
		// 
		ImportWallet.Location = new Point(3, 86);
		ImportWallet.Name = "ImportWallet";
		ImportWallet.Size = new Size(260, 32);
		ImportWallet.TabIndex = 13;
		ImportWallet.Text = "Import";
		ImportWallet.UseVisualStyleBackColor = true;
		ImportWallet.Click += ImportWallet_Click;
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
		label1.Text = "Keys";
		label1.TextAlign = ContentAlignment.MiddleLeft;
		// 
		// ImportKey
		// 
		ImportKey.Location = new Point(586, 87);
		ImportKey.Name = "ImportKey";
		ImportKey.Size = new Size(211, 32);
		ImportKey.TabIndex = 11;
		ImportKey.Text = "Import";
		ImportKey.UseVisualStyleBackColor = true;
		ImportKey.Click += ImportKey_Click;
		// 
		// Keys
		// 
		Keys.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
		Keys.FullRowSelect = true;
		Keys.Location = new Point(3, 49);
		Keys.Name = "Keys";
		Keys.Size = new Size(568, 276);
		Keys.TabIndex = 15;
		Keys.UseCompatibleStateImageBehavior = false;
		Keys.View = View.Details;
		Keys.ItemSelectionChanged += Keys_ItemSelectionChanged;
		// 
		// columnHeader1
		// 
		columnHeader1.Text = "Name";
		columnHeader1.Width = 150;
		// 
		// columnHeader2
		// 
		columnHeader2.Text = "Public Key";
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
		Wallets.Columns.AddRange(new ColumnHeader[] { columnHeader4 });
		Wallets.FullRowSelect = true;
		Wallets.LabelEdit = true;
		Wallets.Location = new Point(278, 48);
		Wallets.Margin = new Padding(12, 3, 12, 3);
		Wallets.Name = "Wallets";
		Wallets.Size = new Size(293, 194);
		Wallets.TabIndex = 16;
		Wallets.UseCompatibleStateImageBehavior = false;
		Wallets.View = View.Details;
		Wallets.AfterLabelEdit += Wallets_AfterLabelEdit;
		Wallets.ItemSelectionChanged += Wallets_ItemSelectionChanged;
		// 
		// columnHeader4
		// 
		columnHeader4.Text = "Name";
		columnHeader4.Width = 150;
		// 
		// KeysPanel
		// 
		KeysPanel.Controls.Add(Locked);
		KeysPanel.Controls.Add(Keys);
		KeysPanel.Controls.Add(DeleteKey);
		KeysPanel.Controls.Add(ImportKey);
		KeysPanel.Controls.Add(label1);
		KeysPanel.Controls.Add(ShowSecret);
		KeysPanel.Controls.Add(CreateKey);
		KeysPanel.Controls.Add(CopyPublic);
		KeysPanel.Location = new Point(0, 272);
		KeysPanel.Name = "KeysPanel";
		KeysPanel.Size = new Size(800, 328);
		KeysPanel.TabIndex = 17;
		// 
		// Locked
		// 
		Locked.BorderStyle = BorderStyle.FixedSingle;
		Locked.Location = new Point(3, 49);
		Locked.Name = "Locked";
		Locked.Size = new Size(794, 276);
		Locked.TabIndex = 18;
		Locked.Text = "The Wallet is Locked";
		Locked.TextAlign = ContentAlignment.MiddleCenter;
		// 
		// ExportWallet
		// 
		ExportWallet.Location = new Point(586, 172);
		ExportWallet.Name = "ExportWallet";
		ExportWallet.Size = new Size(211, 32);
		ExportWallet.TabIndex = 8;
		ExportWallet.Text = "Export";
		ExportWallet.UseVisualStyleBackColor = true;
		ExportWallet.EnabledChanged += LockUnlock_EnabledChanged;
		ExportWallet.Click += ExportWallet_Click;
		// 
		// WalletsPage
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		Controls.Add(KeysPanel);
		Controls.Add(Wallets);
		Controls.Add(label3);
		Controls.Add(DeleteWallet);
		Controls.Add(ExportWallet);
		Controls.Add(LockUnlock);
		Controls.Add(ImportWallet);
		Controls.Add(CreateWallet);
		Name = "WalletsPage";
		Size = new Size(800, 600);
		KeysPanel.ResumeLayout(false);
		ResumeLayout(false);
	}

	#endregion
	private Button DeleteWallet;
	private Button LockUnlock;
	private Button CreateWallet;
	private Button CreateKey;
	private Button ShowSecret;
	private Button DeleteKey;
	private Button CopyPublic;
	private Button ImportWallet;
	private Label label3;
	private Label label1;
	private Button ImportKey;
	private ListView Keys;
	private ColumnHeader columnHeader1;
	private ColumnHeader columnHeader2;
	private ImageList imageList1;
	private ListView Wallets;
	private ColumnHeader columnHeader4;
	private Panel KeysPanel;
	private Button ExportWallet;
	private Label Locked;
}
