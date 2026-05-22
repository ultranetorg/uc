namespace Uccs.Nexus.Windows
{
	partial class AuthenticattionForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AuthenticattionForm));
			Reject = new Button();
			groupBox1 = new GroupBox();
			Allow = new Button();
			Application = new Label();
			Logo = new PictureBox();
			Accounts = new ComboBox();
			Wallets = new ComboBox();
			label1 = new Label();
			label2 = new Label();
			label3 = new Label();
			label4 = new Label();
			Ask = new Button();
			label6 = new Label();
			Net = new Label();
			Shield = new PictureBox();
			label5 = new Label();
			User = new Label();
			((System.ComponentModel.ISupportInitialize)Logo).BeginInit();
			((System.ComponentModel.ISupportInitialize)Shield).BeginInit();
			SuspendLayout();
			// 
			// Reject
			// 
			Reject.DialogResult = DialogResult.Cancel;
			Reject.Location = new Point(699, 676);
			Reject.Margin = new Padding(17, 19, 17, 19);
			Reject.Name = "Reject";
			Reject.Size = new Size(238, 68);
			Reject.TabIndex = 0;
			Reject.Text = "Reject";
			Reject.UseVisualStyleBackColor = true;
			Reject.Click += cancel_Click;
			// 
			// groupBox1
			// 
			groupBox1.Location = new Point(35, 618);
			groupBox1.Margin = new Padding(17, 19, 17, 19);
			groupBox1.Name = "groupBox1";
			groupBox1.Padding = new Padding(6);
			groupBox1.Size = new Size(902, 19);
			groupBox1.TabIndex = 5;
			groupBox1.TabStop = false;
			// 
			// Allow
			// 
			Allow.Location = new Point(35, 676);
			Allow.Margin = new Padding(17, 19, 17, 19);
			Allow.Name = "Allow";
			Allow.Size = new Size(238, 68);
			Allow.TabIndex = 2;
			Allow.Text = "Allow Always";
			Allow.UseVisualStyleBackColor = true;
			Allow.Click += Allow_Click;
			// 
			// Application
			// 
			Application.AutoSize = true;
			Application.ImageAlign = ContentAlignment.TopLeft;
			Application.Location = new Point(177, 290);
			Application.Margin = new Padding(6, 13, 6, 13);
			Application.Name = "Application";
			Application.Size = new Size(163, 32);
			Application.TabIndex = 4;
			Application.Text = "<application>";
			Application.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// Logo
			// 
			Logo.Image = (Image)resources.GetObject("Logo.Image");
			Logo.Location = new Point(35, 23);
			Logo.Margin = new Padding(6);
			Logo.Name = "Logo";
			Logo.Size = new Size(186, 213);
			Logo.SizeMode = PictureBoxSizeMode.Zoom;
			Logo.TabIndex = 6;
			Logo.TabStop = false;
			// 
			// Accounts
			// 
			Accounts.DropDownStyle = ComboBoxStyle.DropDownList;
			Accounts.FormattingEnabled = true;
			Accounts.Location = new Point(177, 538);
			Accounts.Margin = new Padding(6, 13, 6, 13);
			Accounts.Name = "Accounts";
			Accounts.Size = new Size(760, 40);
			Accounts.TabIndex = 1;
			Accounts.TextChanged += Accounts_TextChanged;
			// 
			// Wallets
			// 
			Wallets.DropDownStyle = ComboBoxStyle.DropDownList;
			Wallets.FormattingEnabled = true;
			Wallets.Location = new Point(177, 463);
			Wallets.Margin = new Padding(6, 13, 6, 13);
			Wallets.Name = "Wallets";
			Wallets.Size = new Size(294, 40);
			Wallets.TabIndex = 0;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label1.Location = new Point(79, 466);
			label1.Margin = new Padding(6, 0, 6, 0);
			label1.Name = "label1";
			label1.Size = new Size(86, 32);
			label1.TabIndex = 8;
			label1.Text = "Wallet";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label2.Location = new Point(56, 541);
			label2.Margin = new Padding(6, 0, 6, 0);
			label2.Name = "label2";
			label2.Size = new Size(109, 32);
			label2.TabIndex = 8;
			label2.Text = "Account";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label3.Location = new Point(38, 290);
			label3.Margin = new Padding(6, 0, 6, 0);
			label3.Name = "label3";
			label3.Size = new Size(146, 32);
			label3.TabIndex = 8;
			label3.Text = "Application";
			// 
			// label4
			// 
			label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
			label4.ImageAlign = ContentAlignment.TopLeft;
			label4.Location = new Point(262, 23);
			label4.Margin = new Padding(6, 0, 6, 0);
			label4.Name = "label4";
			label4.Size = new Size(568, 259);
			label4.TabIndex = 4;
			label4.Text = resources.GetString("label4.Text");
			// 
			// Ask
			// 
			Ask.Location = new Point(306, 676);
			Ask.Margin = new Padding(17, 19, 17, 19);
			Ask.Name = "Ask";
			Ask.Size = new Size(238, 68);
			Ask.TabIndex = 3;
			Ask.Text = "Ask Every Time";
			Ask.UseVisualStyleBackColor = true;
			Ask.Click += Ask_Click;
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label6.Location = new Point(114, 348);
			label6.Margin = new Padding(6, 0, 6, 0);
			label6.Name = "label6";
			label6.Size = new Size(55, 32);
			label6.TabIndex = 10;
			label6.Text = "Net";
			// 
			// Net
			// 
			Net.AutoSize = true;
			Net.ImageAlign = ContentAlignment.TopLeft;
			Net.Location = new Point(177, 348);
			Net.Margin = new Padding(6, 13, 6, 13);
			Net.Name = "Net";
			Net.Size = new Size(81, 32);
			Net.TabIndex = 9;
			Net.Text = "<net>";
			Net.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// Shield
			// 
			Shield.Image = (Image)resources.GetObject("Shield.Image");
			Shield.Location = new Point(24, 23);
			Shield.Margin = new Padding(6);
			Shield.Name = "Shield";
			Shield.Size = new Size(211, 232);
			Shield.SizeMode = PictureBoxSizeMode.Zoom;
			Shield.TabIndex = 11;
			Shield.TabStop = false;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label5.Location = new Point(105, 405);
			label5.Margin = new Padding(6, 0, 6, 0);
			label5.Name = "label5";
			label5.Size = new Size(65, 32);
			label5.TabIndex = 10;
			label5.Text = "User";
			// 
			// User
			// 
			User.AutoSize = true;
			User.ImageAlign = ContentAlignment.TopLeft;
			User.Location = new Point(177, 405);
			User.Margin = new Padding(6, 13, 6, 13);
			User.Name = "User";
			User.Size = new Size(91, 32);
			User.TabIndex = 9;
			User.Text = "<user>";
			User.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// AuthenticattionForm
			// 
			AutoScaleDimensions = new SizeF(13F, 32F);
			AutoScaleMode = AutoScaleMode.Font;
			AutoSize = true;
			CancelButton = Reject;
			ClientSize = new Size(978, 790);
			ControlBox = false;
			Controls.Add(Shield);
			Controls.Add(label5);
			Controls.Add(label6);
			Controls.Add(User);
			Controls.Add(Net);
			Controls.Add(label2);
			Controls.Add(label3);
			Controls.Add(label1);
			Controls.Add(Wallets);
			Controls.Add(Accounts);
			Controls.Add(Logo);
			Controls.Add(groupBox1);
			Controls.Add(label4);
			Controls.Add(Application);
			Controls.Add(Ask);
			Controls.Add(Allow);
			Controls.Add(Reject);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Margin = new Padding(6);
			Name = "AuthenticattionForm";
			Padding = new Padding(0, 0, 0, 19);
			SizeGripStyle = SizeGripStyle.Hide;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Authenticattion Required";
			((System.ComponentModel.ISupportInitialize)Logo).EndInit();
			((System.ComponentModel.ISupportInitialize)Shield).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		public System.Windows.Forms.TextBox gasprice;
		private System.Windows.Forms.Button Reject;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button Allow;
		private System.Windows.Forms.Label Application;
		private System.Windows.Forms.PictureBox Logo;
		private ComboBox Accounts;
		private ComboBox Wallets;
		private Label label1;
		private Label label2;
		private Label label3;
		private Label label4;
		private Button Ask;
		private Label label6;
		private Label Net;
		private PictureBox Shield;
		private Label label5;
		private Label User;
	}
}