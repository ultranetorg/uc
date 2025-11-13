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
		FromId = new ComboBox();
		Transfer = new Button();
		FromNet = new ComboBox();
		label4 = new Label();
		FromClass = new ComboBox();
		label1 = new Label();
		Asset = new ComboBox();
		label5 = new Label();
		label6 = new Label();
		Amount = new TextBox();
		ToNet = new ComboBox();
		ToId = new ComboBox();
		ToClass = new ComboBox();
		label7 = new Label();
		label8 = new Label();
		label9 = new Label();
		pictureBox1 = new PictureBox();
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
		label3.Text = "Assets";
		label3.TextAlign = ContentAlignment.MiddleLeft;
		// 
		// Transfers
		// 
		Transfers.Columns.AddRange(new ColumnHeader[] { CFromNet, CFromEntity, CToNet, CToEntity, CAsset, CAmount });
		Transfers.FullRowSelect = true;
		Transfers.Location = new Point(3, 303);
		Transfers.Name = "Transfers";
		Transfers.Size = new Size(794, 294);
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
		label2.Location = new Point(18, 124);
		label2.Name = "label2";
		label2.Size = new Size(64, 15);
		label2.TabIndex = 12;
		label2.Text = "Sender's Id";
		// 
		// FromId
		// 
		FromId.FormattingEnabled = true;
		FromId.Location = new Point(88, 121);
		FromId.Margin = new Padding(3, 6, 3, 6);
		FromId.Name = "FromId";
		FromId.Size = new Size(280, 23);
		FromId.TabIndex = 10;
		FromId.SelectionChangeCommitted += Nets_Changed;
		FromId.KeyDown += Accounts_KeyDown;
		// 
		// Transfer
		// 
		Transfer.Location = new Point(297, 240);
		Transfer.Margin = new Padding(3, 6, 3, 6);
		Transfer.Name = "Transfer";
		Transfer.Size = new Size(209, 32);
		Transfer.TabIndex = 9;
		Transfer.Text = "Transfer";
		Transfer.UseVisualStyleBackColor = true;
		// 
		// FromNet
		// 
		FromNet.FormattingEnabled = true;
		FromNet.Location = new Point(159, 51);
		FromNet.Margin = new Padding(3, 6, 3, 6);
		FromNet.Name = "FromNet";
		FromNet.Size = new Size(209, 23);
		FromNet.TabIndex = 11;
		FromNet.SelectionChangeCommitted += Nets_Changed;
		FromNet.KeyDown += Accounts_KeyDown;
		// 
		// label4
		// 
		label4.AutoSize = true;
		label4.Location = new Point(60, 54);
		label4.Name = "label4";
		label4.Size = new Size(93, 15);
		label4.TabIndex = 13;
		label4.Text = "Source Net/CCP";
		// 
		// FromClass
		// 
		FromClass.FormattingEnabled = true;
		FromClass.Location = new Point(159, 86);
		FromClass.Margin = new Padding(3, 6, 3, 6);
		FromClass.Name = "FromClass";
		FromClass.Size = new Size(209, 23);
		FromClass.TabIndex = 10;
		FromClass.SelectionChangeCommitted += Nets_Changed;
		FromClass.KeyDown += Accounts_KeyDown;
		// 
		// label1
		// 
		label1.AutoSize = true;
		label1.Location = new Point(72, 89);
		label1.Name = "label1";
		label1.Size = new Size(81, 15);
		label1.TabIndex = 13;
		label1.Text = "Sender's Class";
		// 
		// Asset
		// 
		Asset.FormattingEnabled = true;
		Asset.Location = new Point(297, 170);
		Asset.Margin = new Padding(3, 6, 3, 6);
		Asset.Name = "Asset";
		Asset.Size = new Size(209, 23);
		Asset.TabIndex = 10;
		Asset.SelectionChangeCommitted += Nets_Changed;
		Asset.KeyDown += Accounts_KeyDown;
		// 
		// label5
		// 
		label5.AutoSize = true;
		label5.Location = new Point(256, 173);
		label5.Name = "label5";
		label5.Size = new Size(35, 15);
		label5.TabIndex = 12;
		label5.Text = "Asset";
		// 
		// label6
		// 
		label6.AutoSize = true;
		label6.Location = new Point(240, 208);
		label6.Name = "label6";
		label6.Size = new Size(51, 15);
		label6.TabIndex = 12;
		label6.Text = "Amount";
		// 
		// Amount
		// 
		Amount.Location = new Point(297, 205);
		Amount.Margin = new Padding(3, 6, 3, 6);
		Amount.Name = "Amount";
		Amount.Size = new Size(209, 23);
		Amount.TabIndex = 15;
		// 
		// ToNet
		// 
		ToNet.FormattingEnabled = true;
		ToNet.Location = new Point(429, 51);
		ToNet.Margin = new Padding(3, 6, 3, 6);
		ToNet.Name = "ToNet";
		ToNet.Size = new Size(209, 23);
		ToNet.TabIndex = 11;
		ToNet.SelectionChangeCommitted += Nets_Changed;
		ToNet.KeyDown += Accounts_KeyDown;
		// 
		// ToId
		// 
		ToId.FormattingEnabled = true;
		ToId.Location = new Point(429, 121);
		ToId.Margin = new Padding(3, 6, 3, 6);
		ToId.Name = "ToId";
		ToId.Size = new Size(280, 23);
		ToId.TabIndex = 10;
		ToId.SelectionChangeCommitted += Nets_Changed;
		ToId.KeyDown += Accounts_KeyDown;
		// 
		// ToClass
		// 
		ToClass.FormattingEnabled = true;
		ToClass.Location = new Point(429, 86);
		ToClass.Margin = new Padding(3, 6, 3, 6);
		ToClass.Name = "ToClass";
		ToClass.Size = new Size(209, 23);
		ToClass.TabIndex = 10;
		ToClass.SelectionChangeCommitted += Nets_Changed;
		ToClass.KeyDown += Accounts_KeyDown;
		// 
		// label7
		// 
		label7.AutoSize = true;
		label7.Location = new Point(644, 54);
		label7.Name = "label7";
		label7.Size = new Size(117, 15);
		label7.TabIndex = 13;
		label7.Text = "Destination Net/CCP";
		// 
		// label8
		// 
		label8.AutoSize = true;
		label8.Location = new Point(644, 89);
		label8.Name = "label8";
		label8.Size = new Size(94, 15);
		label8.TabIndex = 13;
		label8.Text = "Recipient's Class";
		// 
		// label9
		// 
		label9.AutoSize = true;
		label9.Location = new Point(715, 124);
		label9.Name = "label9";
		label9.Size = new Size(77, 15);
		label9.TabIndex = 12;
		label9.Text = "Recipient's Id";
		// 
		// pictureBox1
		// 
		pictureBox1.Image = Properties.Resources.right_arrow;
		pictureBox1.Location = new Point(374, 49);
		pictureBox1.Name = "pictureBox1";
		pictureBox1.Size = new Size(49, 95);
		pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
		pictureBox1.TabIndex = 16;
		pictureBox1.TabStop = false;
		// 
		// TransferPage
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		Controls.Add(pictureBox1);
		Controls.Add(Amount);
		Controls.Add(label3);
		Controls.Add(Transfers);
		Controls.Add(label6);
		Controls.Add(label5);
		Controls.Add(label9);
		Controls.Add(label2);
		Controls.Add(label8);
		Controls.Add(label1);
		Controls.Add(label7);
		Controls.Add(label4);
		Controls.Add(ToClass);
		Controls.Add(FromClass);
		Controls.Add(Asset);
		Controls.Add(ToId);
		Controls.Add(FromId);
		Controls.Add(ToNet);
		Controls.Add(FromNet);
		Controls.Add(Transfer);
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
	private ComboBox FromId;
	private Button Transfer;
	private ComboBox FromNet;
	private Label label4;
	private ColumnHeader CFromNet;
	private ColumnHeader CFromEntity;
	private ColumnHeader CAmount;
	private ComboBox FromClass;
	private Label label1;
	private ComboBox Asset;
	private Label label5;
	private Label label6;
	private TextBox Amount;
	private ComboBox ToNet;
	private ComboBox ToId;
	private ComboBox ToClass;
	private Label label7;
	private Label label8;
	private Label label9;
	private PictureBox pictureBox1;
}
