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
		WalletsAndAccounts = new RadioButton();
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
		Sessions.Location = new Point(12, 156);
		Sessions.Name = "Sessions";
		Sessions.Padding = new Padding(10, 0, 0, 0);
		Sessions.Size = new Size(230, 64);
		Sessions.TabIndex = 2;
		Sessions.TabStop = true;
		Sessions.Text = "Sessions";
		Sessions.UseVisualStyleBackColor = true;
		Sessions.CheckedChanged += radioButton_CheckedChanged;
		// 
		// radioButton3
		// 
		radioButton3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		radioButton3.Appearance = Appearance.Button;
		radioButton3.Font = new Font("Segoe UI", 12F);
		radioButton3.Location = new Point(12, 622);
		radioButton3.Name = "radioButton3";
		radioButton3.Padding = new Padding(10, 0, 0, 0);
		radioButton3.Size = new Size(230, 64);
		radioButton3.TabIndex = 2;
		radioButton3.TabStop = true;
		radioButton3.Text = "Settings";
		radioButton3.UseVisualStyleBackColor = true;
		radioButton3.CheckedChanged += radioButton_CheckedChanged;
		// 
		// Assets
		// 
		Assets.Appearance = Appearance.Button;
		Assets.Font = new Font("Segoe UI", 12F);
		Assets.Location = new Point(12, 226);
		Assets.Name = "Assets";
		Assets.Padding = new Padding(10, 0, 0, 0);
		Assets.Size = new Size(230, 64);
		Assets.TabIndex = 2;
		Assets.TabStop = true;
		Assets.Text = "Assets && Tokens";
		Assets.UseVisualStyleBackColor = true;
		Assets.CheckedChanged += radioButton_CheckedChanged;
		// 
		// WalletsAndAccounts
		// 
		WalletsAndAccounts.Appearance = Appearance.Button;
		WalletsAndAccounts.Font = new Font("Segoe UI", 12F);
		WalletsAndAccounts.ImageAlign = ContentAlignment.MiddleLeft;
		WalletsAndAccounts.Location = new Point(12, 86);
		WalletsAndAccounts.Name = "WalletsAndAccounts";
		WalletsAndAccounts.Padding = new Padding(10, 0, 0, 0);
		WalletsAndAccounts.Size = new Size(230, 64);
		WalletsAndAccounts.TabIndex = 2;
		WalletsAndAccounts.TabStop = true;
		WalletsAndAccounts.Text = "Wallets && Accounts";
		WalletsAndAccounts.TextImageRelation = TextImageRelation.ImageBeforeText;
		WalletsAndAccounts.UseVisualStyleBackColor = true;
		WalletsAndAccounts.CheckedChanged += radioButton_CheckedChanged;
		// 
		// Place
		// 
		Place.Location = new Point(262, 86);
		Place.Name = "Place";
		Place.Size = new Size(800, 600);
		Place.TabIndex = 3;
		// 
		// Logo
		// 
		Logo.BackColor = Color.Transparent;
		Logo.BackgroundImageLayout = ImageLayout.None;
		Logo.Image = (Image)resources.GetObject("Logo.Image");
		Logo.InitialImage = (Image)resources.GetObject("Logo.InitialImage");
		Logo.Location = new Point(20, 9);
		Logo.Name = "Logo";
		Logo.Size = new Size(48, 48);
		Logo.SizeMode = PictureBoxSizeMode.AutoSize;
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
		panel1.Size = new Size(1080, 71);
		panel1.TabIndex = 5;
		// 
		// pictureBox1
		// 
		pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
		pictureBox1.Location = new Point(74, -2);
		pictureBox1.Name = "pictureBox1";
		pictureBox1.Size = new Size(300, 70);
		pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
		pictureBox1.TabIndex = 5;
		pictureBox1.TabStop = false;
		// 
		// Transfer
		// 
		Transfer.Appearance = Appearance.Button;
		Transfer.Font = new Font("Segoe UI", 12F);
		Transfer.Location = new Point(45, 296);
		Transfer.Name = "Transfer";
		Transfer.Padding = new Padding(10, 0, 0, 0);
		Transfer.Size = new Size(197, 64);
		Transfer.TabIndex = 2;
		Transfer.TabStop = true;
		Transfer.Text = "Transfer";
		Transfer.UseVisualStyleBackColor = true;
		Transfer.CheckedChanged += radioButton_CheckedChanged;
		// 
		// IamForm
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		ClientSize = new Size(1074, 696);
		Controls.Add(panel1);
		Controls.Add(Place);
		Controls.Add(radioButton3);
		Controls.Add(Transfer);
		Controls.Add(Assets);
		Controls.Add(WalletsAndAccounts);
		Controls.Add(Sessions);
		Name = "IamForm";
		StartPosition = FormStartPosition.CenterScreen;
		Text = "Identity and Activity Management";
		((System.ComponentModel.ISupportInitialize)Logo).EndInit();
		panel1.ResumeLayout(false);
		panel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
		ResumeLayout(false);
	}

	#endregion
	private RadioButton Sessions;
	private RadioButton radioButton3;
	private RadioButton Assets;
	private RadioButton WalletsAndAccounts;
	private Panel Place;
	private PictureBox Logo;
	private Panel panel1;
	private PictureBox pictureBox1;
	private RadioButton Transfer;
}
