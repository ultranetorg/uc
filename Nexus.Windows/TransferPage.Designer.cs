namespace Uccs.Nexus.Windows;

partial class TransferPage
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
		label3 = new Label();
		Transfers = new ListView();
		CFromNet = new ColumnHeader();
		CFromEntity = new ColumnHeader();
		CToNet = new ColumnHeader();
		CToEntity = new ColumnHeader();
		CAsset = new ColumnHeader();
		CAmount = new ColumnHeader();
		label2 = new Label();
		Transfer = new Button();
		FromNet = new ComboBox();
		label4 = new Label();
		FromClass = new ComboBox();
		Asset = new ComboBox();
		label5 = new Label();
		label6 = new Label();
		Amount = new TextBox();
		ToNet = new ComboBox();
		ToClass = new ComboBox();
		label7 = new Label();
		label8 = new Label();
		label9 = new Label();
		pictureBox1 = new PictureBox();
		FromAccount = new ComboBox();
		label10 = new Label();
		Balance = new Label();
		FromId = new TextBox();
		ToId = new TextBox();
		label11 = new Label();
		label1 = new Label();
		label12 = new Label();
		Wallets = new ComboBox();
		label13 = new Label();
		((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
		SuspendLayout();
		// 
		// label3
		// 
		label3.BackColor = SystemColors.ControlDarkDark;
		label3.Font = new Font("Segoe UI", 12F);
		label3.ForeColor = SystemColors.Control;
		label3.Location = new Point(0, 0);
		label3.Name = "label3";
		label3.Padding = new Padding(8, 0, 0, 0);
		label3.Size = new Size(800, 32);
		label3.TabIndex = 14;
		label3.Text = "Transfer";
		label3.TextAlign = ContentAlignment.MiddleLeft;
		// 
		// Transfers
		// 
		Transfers.Columns.AddRange(new ColumnHeader[] { CFromNet, CFromEntity, CToNet, CToEntity, CAsset, CAmount });
		Transfers.FullRowSelect = true;
		Transfers.Location = new Point(3, 383);
		Transfers.Name = "Transfers";
		Transfers.Size = new Size(794, 214);
		Transfers.TabIndex = 8;
		Transfers.UseCompatibleStateImageBehavior = false;
		Transfers.View = View.Details;
		// 
		// CFromNet
		// 
		CFromNet.Text = "From Net";
		CFromNet.Width = 100;
		// 
		// CFromEntity
		// 
		CFromEntity.Text = "From Entity";
		CFromEntity.Width = 100;
		// 
		// CToNet
		// 
		CToNet.Text = "To Net";
		CToNet.Width = 100;
		// 
		// CToEntity
		// 
		CToEntity.Text = "To Entity";
		CToEntity.Width = 100;
		// 
		// CAsset
		// 
		CAsset.Text = "Asset";
		CAsset.Width = 100;
		// 
		// CAmount
		// 
		CAmount.Text = "Amount";
		CAmount.Width = 100;
		// 
		// label2
		// 
		label2.AutoSize = true;
		label2.Font = new Font("Segoe UI", 9F);
		label2.Location = new Point(206, 81);
		label2.Name = "label2";
		label2.Size = new Size(17, 15);
		label2.TabIndex = 12;
		label2.Text = "Id";
		// 
		// Transfer
		// 
		Transfer.Location = new Point(282, 342);
		Transfer.Margin = new Padding(3, 6, 3, 6);
		Transfer.Name = "Transfer";
		Transfer.Size = new Size(209, 32);
		Transfer.TabIndex = 9;
		Transfer.Text = "Transfer";
		Transfer.UseVisualStyleBackColor = true;
		Transfer.Click += Transfer_Click;
		// 
		// FromNet
		// 
		FromNet.FormattingEnabled = true;
		FromNet.Location = new Point(149, 45);
		FromNet.Margin = new Padding(3, 6, 3, 6);
		FromNet.Name = "FromNet";
		FromNet.Size = new Size(209, 23);
		FromNet.TabIndex = 11;
		FromNet.DropDown += Open_DropDown;
		FromNet.TextUpdate += FromNet_TextUpdate;
		FromNet.TextChanged += Any_Changed;
		// 
		// label4
		// 
		label4.AutoSize = true;
		label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
		label4.Location = new Point(89, 48);
		label4.Name = "label4";
		label4.Size = new Size(54, 15);
		label4.TabIndex = 13;
		label4.Text = "Net/CCP";
		// 
		// FromClass
		// 
		FromClass.DropDownStyle = ComboBoxStyle.DropDownList;
		FromClass.FormattingEnabled = true;
		FromClass.Location = new Point(66, 97);
		FromClass.Margin = new Padding(3, 6, 3, 6);
		FromClass.Name = "FromClass";
		FromClass.Size = new Size(134, 23);
		FromClass.TabIndex = 10;
		FromClass.DropDown += Open_DropDown;
		FromClass.TextChanged += Any_Changed;
		// 
		// Asset
		// 
		Asset.DropDownStyle = ComboBoxStyle.DropDownList;
		Asset.FormattingEnabled = true;
		Asset.Location = new Point(282, 272);
		Asset.Margin = new Padding(3, 6, 3, 6);
		Asset.Name = "Asset";
		Asset.Size = new Size(209, 23);
		Asset.TabIndex = 10;
		Asset.DropDown += Open_DropDown;
		Asset.SelectionChangeCommitted += Asset_SelectionChangeCommitted;
		Asset.TextChanged += Any_Changed;
		// 
		// label5
		// 
		label5.AutoSize = true;
		label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
		label5.Location = new Point(239, 275);
		label5.Name = "label5";
		label5.Size = new Size(37, 15);
		label5.TabIndex = 12;
		label5.Text = "Asset";
		// 
		// label6
		// 
		label6.AutoSize = true;
		label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
		label6.Location = new Point(224, 310);
		label6.Name = "label6";
		label6.Size = new Size(52, 15);
		label6.TabIndex = 12;
		label6.Text = "Amount";
		// 
		// Amount
		// 
		Amount.Location = new Point(282, 307);
		Amount.Margin = new Padding(3, 6, 3, 6);
		Amount.Name = "Amount";
		Amount.Size = new Size(209, 23);
		Amount.TabIndex = 15;
		Amount.TextChanged += Any_Changed;
		// 
		// ToNet
		// 
		ToNet.FormattingEnabled = true;
		ToNet.Location = new Point(419, 45);
		ToNet.Margin = new Padding(3, 6, 3, 6);
		ToNet.Name = "ToNet";
		ToNet.Size = new Size(209, 23);
		ToNet.TabIndex = 11;
		ToNet.DropDown += Open_DropDown;
		ToNet.TextUpdate += ToNet_TextUpdate;
		ToNet.TextChanged += Any_Changed;
		// 
		// ToClass
		// 
		ToClass.DropDownStyle = ComboBoxStyle.DropDownList;
		ToClass.FormattingEnabled = true;
		ToClass.Location = new Point(419, 97);
		ToClass.Margin = new Padding(3, 6, 3, 6);
		ToClass.Name = "ToClass";
		ToClass.Size = new Size(144, 23);
		ToClass.TabIndex = 10;
		ToClass.DropDown += Open_DropDown;
		ToClass.TextChanged += Any_Changed;
		// 
		// label7
		// 
		label7.AutoSize = true;
		label7.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
		label7.Location = new Point(634, 48);
		label7.Name = "label7";
		label7.Size = new Size(54, 15);
		label7.TabIndex = 13;
		label7.Text = "Net/CCP";
		// 
		// label8
		// 
		label8.AutoSize = true;
		label8.Font = new Font("Segoe UI", 9F);
		label8.Location = new Point(419, 81);
		label8.Name = "label8";
		label8.Size = new Size(34, 15);
		label8.TabIndex = 13;
		label8.Text = "Class";
		// 
		// label9
		// 
		label9.AutoSize = true;
		label9.Font = new Font("Segoe UI", 9F);
		label9.Location = new Point(569, 81);
		label9.Name = "label9";
		label9.Size = new Size(17, 15);
		label9.TabIndex = 12;
		label9.Text = "Id";
		// 
		// pictureBox1
		// 
		pictureBox1.Image = Properties.Resources.right_arrow;
		pictureBox1.Location = new Point(365, 45);
		pictureBox1.Name = "pictureBox1";
		pictureBox1.Size = new Size(48, 126);
		pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
		pictureBox1.TabIndex = 16;
		pictureBox1.TabStop = false;
		// 
		// FromAccount
		// 
		FromAccount.DropDownStyle = ComboBoxStyle.DropDownList;
		FromAccount.FormattingEnabled = true;
		FromAccount.Location = new Point(66, 183);
		FromAccount.Margin = new Padding(3, 6, 3, 6);
		FromAccount.Name = "FromAccount";
		FromAccount.Size = new Size(291, 23);
		FromAccount.TabIndex = 11;
		FromAccount.DropDown += Open_DropDown;
		FromAccount.TextChanged += Any_Changed;
		// 
		// label10
		// 
		label10.AutoSize = true;
		label10.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
		label10.Location = new Point(17, 186);
		label10.Name = "label10";
		label10.Size = new Size(43, 15);
		label10.TabIndex = 13;
		label10.Text = "Signer";
		// 
		// Balance
		// 
		Balance.AutoSize = true;
		Balance.Location = new Point(507, 211);
		Balance.Name = "Balance";
		Balance.Size = new Size(48, 15);
		Balance.TabIndex = 13;
		Balance.Text = "Balance";
		// 
		// FromId
		// 
		FromId.Location = new Point(206, 97);
		FromId.Margin = new Padding(3, 6, 3, 6);
		FromId.Name = "FromId";
		FromId.Size = new Size(152, 23);
		FromId.TabIndex = 17;
		FromId.TextChanged += Any_Changed;
		// 
		// ToId
		// 
		ToId.Location = new Point(569, 97);
		ToId.Margin = new Padding(3, 6, 3, 6);
		ToId.Name = "ToId";
		ToId.Size = new Size(152, 23);
		ToId.TabIndex = 17;
		ToId.TextChanged += Any_Changed;
		// 
		// label11
		// 
		label11.AutoSize = true;
		label11.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
		label11.Location = new Point(13, 100);
		label11.Name = "label11";
		label11.Size = new Size(47, 15);
		label11.TabIndex = 13;
		label11.Text = "Sender";
		// 
		// label1
		// 
		label1.AutoSize = true;
		label1.Font = new Font("Segoe UI", 9F);
		label1.Location = new Point(67, 81);
		label1.Name = "label1";
		label1.Size = new Size(34, 15);
		label1.TabIndex = 12;
		label1.Text = "Class";
		// 
		// label12
		// 
		label12.AutoSize = true;
		label12.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
		label12.Location = new Point(727, 100);
		label12.Name = "label12";
		label12.Size = new Size(60, 15);
		label12.TabIndex = 12;
		label12.Text = "Recipient";
		// 
		// Wallets
		// 
		Wallets.DropDownStyle = ComboBoxStyle.DropDownList;
		Wallets.FormattingEnabled = true;
		Wallets.Location = new Point(66, 148);
		Wallets.Margin = new Padding(3, 6, 3, 6);
		Wallets.Name = "Wallets";
		Wallets.Size = new Size(134, 23);
		Wallets.TabIndex = 11;
		// 
		// label13
		// 
		label13.AutoSize = true;
		label13.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
		label13.Location = new Point(17, 151);
		label13.Name = "label13";
		label13.Size = new Size(43, 15);
		label13.TabIndex = 13;
		label13.Text = "Wallet";
		// 
		// TransferPage
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		Controls.Add(ToId);
		Controls.Add(FromId);
		Controls.Add(pictureBox1);
		Controls.Add(Amount);
		Controls.Add(label3);
		Controls.Add(Transfers);
		Controls.Add(label6);
		Controls.Add(label5);
		Controls.Add(label12);
		Controls.Add(label9);
		Controls.Add(label1);
		Controls.Add(label2);
		Controls.Add(label8);
		Controls.Add(label11);
		Controls.Add(label7);
		Controls.Add(label13);
		Controls.Add(label10);
		Controls.Add(Balance);
		Controls.Add(label4);
		Controls.Add(ToClass);
		Controls.Add(FromClass);
		Controls.Add(Asset);
		Controls.Add(ToNet);
		Controls.Add(Wallets);
		Controls.Add(FromAccount);
		Controls.Add(FromNet);
		Controls.Add(Transfer);
		Font = new Font("Segoe UI", 9F);
		Name = "TransferPage";
		Size = new Size(800, 600);
		((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private Label label3;
	private ListView Transfers;
	private ColumnHeader CToNet;
	private ColumnHeader CToEntity;
	private ColumnHeader CAsset;
	private Label label2;
	private Button Transfer;
	private ComboBox FromNet;
	private Label label4;
	private ColumnHeader CFromNet;
	private ColumnHeader CFromEntity;
	private ColumnHeader CAmount;
	private ComboBox FromClass;
	private ComboBox Asset;
	private Label label5;
	private Label label6;
	private TextBox Amount;
	private ComboBox ToNet;
	private ComboBox ToClass;
	private Label label7;
	private Label label8;
	private Label label9;
	private PictureBox pictureBox1;
	private ComboBox FromAccount;
	private Label label10;
	private Label Balance;
	private TextBox FromId;
	private TextBox ToId;
	private Label label11;
	private Label label1;
	private Label label12;
	private ComboBox Wallets;
	private Label label13;
}
