namespace Uccs.Nexus.Windows;

partial class IamForm
{
	/// <summary>
	///  Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	///  Clean up any resources being used.
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

	#region Windows Form Designer generated code

	/// <summary>
	///  Required method for Designer support - do not modify
	///  the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IamForm));
		Sessions = new RadioButton();
		radioButton3 = new RadioButton();
		Assets = new RadioButton();
		Wallets = new RadioButton();
		Place = new Panel();
		Logo = new PictureBox();
		panel1 = new Panel();
		pictureBox1 = new PictureBox();
		Transfer = new RadioButton();
		((System.ComponentModel.ISupportInitialize)Logo).BeginInit();
		panel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
		SuspendLayout();
		// 
		// Sessions
		// 
		Sessions.Appearance = Appearance.Button;
		Sessions.Font = new Font("Segoe UI", 12F);
		Sessions.Image = Properties.Resources.checked_figure;
		Sessions.ImageAlign = ContentAlignment.MiddleLeft;
		Sessions.Location = new Point(22, 333);
		Sessions.Margin = new Padding(6, 6, 6, 6);
		Sessions.Name = "Sessions";
		Sessions.Padding = new Padding(19, 0, 0, 0);
		Sessions.Size = new Size(427, 137);
		Sessions.TabIndex = 2;
		Sessions.Text = " Sessions";
		Sessions.TextImageRelation = TextImageRelation.ImageBeforeText;
		Sessions.UseVisualStyleBackColor = true;
		Sessions.CheckedChanged += radioButton_CheckedChanged;
		// 
		// radioButton3
		// 
		radioButton3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		radioButton3.Appearance = Appearance.Button;
		radioButton3.Font = new Font("Segoe UI", 12F);
		radioButton3.Image = Properties.Resources.setting_2_1_;
		radioButton3.ImageAlign = ContentAlignment.MiddleLeft;
		radioButton3.Location = new Point(22, 1327);
		radioButton3.Margin = new Padding(6, 6, 6, 6);
		radioButton3.Name = "radioButton3";
		radioButton3.Padding = new Padding(19, 0, 0, 0);
		radioButton3.Size = new Size(427, 137);
		radioButton3.TabIndex = 2;
		radioButton3.Text = " Settings";
		radioButton3.TextImageRelation = TextImageRelation.ImageBeforeText;
		radioButton3.UseVisualStyleBackColor = true;
		radioButton3.CheckedChanged += radioButton_CheckedChanged;
		// 
		// Assets
		// 
		Assets.Appearance = Appearance.Button;
		Assets.Font = new Font("Segoe UI", 12F);
		Assets.Image = Properties.Resources.coins;
		Assets.ImageAlign = ContentAlignment.MiddleLeft;
		Assets.Location = new Point(22, 482);
		Assets.Margin = new Padding(6, 6, 6, 6);
		Assets.Name = "Assets";
		Assets.Padding = new Padding(19, 0, 0, 0);
		Assets.Size = new Size(427, 137);
		Assets.TabIndex = 2;
		Assets.Text = " Assets && Tokens";
		Assets.TextImageRelation = TextImageRelation.ImageBeforeText;
		Assets.UseVisualStyleBackColor = true;
		Assets.CheckedChanged += radioButton_CheckedChanged;
		// 
		// Wallets
		// 
		Wallets.Appearance = Appearance.Button;
		Wallets.Font = new Font("Segoe UI", 12F);
		Wallets.Image = Properties.Resources.users;
		Wallets.ImageAlign = ContentAlignment.MiddleLeft;
		Wallets.Location = new Point(22, 183);
		Wallets.Margin = new Padding(6, 6, 6, 6);
		Wallets.Name = "Wallets";
		Wallets.Padding = new Padding(19, 0, 0, 0);
		Wallets.Size = new Size(427, 137);
		Wallets.TabIndex = 2;
		Wallets.Text = " Wallets && Keys";
		Wallets.TextImageRelation = TextImageRelation.ImageBeforeText;
		Wallets.UseVisualStyleBackColor = true;
		Wallets.CheckedChanged += radioButton_CheckedChanged;
		// 
		// Place
		// 
		Place.Location = new Point(487, 183);
		Place.Margin = new Padding(6, 6, 6, 6);
		Place.Name = "Place";
		Place.Size = new Size(1486, 1280);
		Place.TabIndex = 3;
		// 
		// Logo
		// 
		Logo.BackColor = Color.Transparent;
		Logo.BackgroundImageLayout = ImageLayout.None;
		Logo.Image = (Image)resources.GetObject("Logo.Image");
		Logo.InitialImage = null;
		Logo.Location = new Point(32, 32);
		Logo.Margin = new Padding(6, 6, 6, 6);
		Logo.Name = "Logo";
		Logo.Size = new Size(64, 64);
		Logo.SizeMode = PictureBoxSizeMode.Zoom;
		Logo.TabIndex = 4;
		Logo.TabStop = false;
		// 
		// panel1
		// 
		panel1.BackColor = SystemColors.ControlText;
		panel1.Controls.Add(pictureBox1);
		panel1.Controls.Add(Logo);
		panel1.Location = new Point(0, 0);
		panel1.Margin = new Padding(0);
		panel1.Name = "panel1";
		panel1.Size = new Size(2006, 137);
		panel1.TabIndex = 5;
		// 
		// pictureBox1
		// 
		pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
		pictureBox1.Location = new Point(119, 32);
		pictureBox1.Margin = new Padding(6, 6, 6, 6);
		pictureBox1.Name = "pictureBox1";
		pictureBox1.Size = new Size(484, 64);
		pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
		pictureBox1.TabIndex = 5;
		pictureBox1.TabStop = false;
		// 
		// Transfer
		// 
		Transfer.Appearance = Appearance.Button;
		Transfer.Font = new Font("Segoe UI", 12F);
		Transfer.Image = Properties.Resources.money_send;
		Transfer.ImageAlign = ContentAlignment.MiddleLeft;
		Transfer.Location = new Point(84, 631);
		Transfer.Margin = new Padding(6, 6, 6, 6);
		Transfer.Name = "Transfer";
		Transfer.Padding = new Padding(19, 0, 0, 0);
		Transfer.Size = new Size(366, 137);
		Transfer.TabIndex = 2;
		Transfer.Text = "Net-to-Net Transfer";
		Transfer.TextImageRelation = TextImageRelation.ImageBeforeText;
		Transfer.UseVisualStyleBackColor = true;
		Transfer.CheckedChanged += radioButton_CheckedChanged;
		// 
		// IamForm
		// 
		AutoScaleDimensions = new SizeF(13F, 32F);
		AutoScaleMode = AutoScaleMode.Font;
		ClientSize = new Size(1995, 1485);
		Controls.Add(panel1);
		Controls.Add(Place);
		Controls.Add(radioButton3);
		Controls.Add(Transfer);
		Controls.Add(Assets);
		Controls.Add(Wallets);
		Controls.Add(Sessions);
		Margin = new Padding(6, 6, 6, 6);
		Name = "IamForm";
		StartPosition = FormStartPosition.CenterScreen;
		Text = "Identity and Activity Management";
		((System.ComponentModel.ISupportInitialize)Logo).EndInit();
		panel1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
		ResumeLayout(false);
	}

	#endregion
	private RadioButton Sessions;
	private RadioButton radioButton3;
	private RadioButton Assets;
	private RadioButton Wallets;
	private Panel Place;
	private PictureBox Logo;
	private Panel panel1;
	private PictureBox pictureBox1;
	private RadioButton Transfer;
}
