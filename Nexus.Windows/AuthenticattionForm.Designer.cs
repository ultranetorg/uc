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
			Keys = new ComboBox();
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
			Reject.Location = new Point(376, 317);
			Reject.Margin = new Padding(9, 9, 9, 9);
			Reject.Name = "Reject";
			Reject.Size = new Size(128, 32);
			Reject.TabIndex = 0;
			Reject.Text = "Reject";
			Reject.UseVisualStyleBackColor = true;
			Reject.Click += cancel_Click;
			// 
			// groupBox1
			// 
			groupBox1.Location = new Point(19, 290);
			groupBox1.Margin = new Padding(9, 9, 9, 9);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(486, 9);
			groupBox1.TabIndex = 5;
			groupBox1.TabStop = false;
			// 
			// Allow
			// 
			Allow.Location = new Point(19, 317);
			Allow.Margin = new Padding(9, 9, 9, 9);
			Allow.Name = "Allow";
			Allow.Size = new Size(128, 32);
			Allow.TabIndex = 2;
			Allow.Text = "Allow Always";
			Allow.UseVisualStyleBackColor = true;
			Allow.Click += Allow_Click;
			// 
			// Application
			// 
			Application.AutoSize = true;
			Application.ImageAlign = ContentAlignment.TopLeft;
			Application.Location = new Point(95, 136);
			Application.Margin = new Padding(3, 6, 3, 6);
			Application.Name = "Application";
			Application.Size = new Size(82, 15);
			Application.TabIndex = 4;
			Application.Text = "<application>";
			Application.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// Logo
			// 
			Logo.Image = (Image)resources.GetObject("Logo.Image");
			Logo.Location = new Point(19, 11);
			Logo.Name = "Logo";
			Logo.Size = new Size(100, 100);
			Logo.SizeMode = PictureBoxSizeMode.Zoom;
			Logo.TabIndex = 6;
			Logo.TabStop = false;
			// 
			// Accounts
			// 
			Keys.DropDownStyle = ComboBoxStyle.DropDownList;
			Keys.FormattingEnabled = true;
			Keys.Location = new Point(95, 252);
			Keys.Margin = new Padding(3, 6, 3, 6);
			Keys.Name = "Keys";
			Keys.Size = new Size(411, 23);
			Keys.TabIndex = 1;
			Keys.TextChanged += Keys_TextChanged;
			// 
			// Wallets
			// 
			Wallets.DropDownStyle = ComboBoxStyle.DropDownList;
			Wallets.FormattingEnabled = true;
			Wallets.Location = new Point(95, 217);
			Wallets.Margin = new Padding(3, 6, 3, 6);
			Wallets.Name = "Wallets";
			Wallets.Size = new Size(160, 23);
			Wallets.TabIndex = 0;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label1.Location = new Point(46, 220);
			label1.Name = "label1";
			label1.Size = new Size(43, 15);
			label1.TabIndex = 8;
			label1.Text = "Wallet";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label2.Location = new Point(61, 255);
			label2.Name = "label2";
			label2.Size = new Size(28, 15);
			label2.TabIndex = 8;
			label2.Text = "Key";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label3.Location = new Point(20, 136);
			label3.Name = "label3";
			label3.Size = new Size(69, 15);
			label3.TabIndex = 8;
			label3.Text = "Application";
			// 
			// label4
			// 
			label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
			label4.ImageAlign = ContentAlignment.TopLeft;
			label4.Location = new Point(141, 11);
			label4.Name = "label4";
			label4.Size = new Size(363, 109);
			label4.TabIndex = 4;
			label4.Text = resources.GetString("label4.Text");
			// 
			// Ask
			// 
			Ask.Location = new Point(165, 317);
			Ask.Margin = new Padding(9, 9, 9, 9);
			Ask.Name = "Ask";
			Ask.Size = new Size(128, 32);
			Ask.TabIndex = 3;
			Ask.Text = "Ask Every Time";
			Ask.UseVisualStyleBackColor = true;
			Ask.Click += Ask_Click;
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label6.Location = new Point(61, 163);
			label6.Name = "label6";
			label6.Size = new Size(28, 15);
			label6.TabIndex = 10;
			label6.Text = "Net";
			// 
			// Net
			// 
			Net.AutoSize = true;
			Net.ImageAlign = ContentAlignment.TopLeft;
			Net.Location = new Point(95, 163);
			Net.Margin = new Padding(3, 6, 3, 6);
			Net.Name = "Net";
			Net.Size = new Size(40, 15);
			Net.TabIndex = 9;
			Net.Text = "<net>";
			Net.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// Shield
			// 
			Shield.Image = (Image)resources.GetObject("Shield.Image");
			Shield.Location = new Point(13, 11);
			Shield.Name = "Shield";
			Shield.Size = new Size(114, 109);
			Shield.SizeMode = PictureBoxSizeMode.Zoom;
			Shield.TabIndex = 11;
			Shield.TabStop = false;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label5.Location = new Point(57, 190);
			label5.Name = "label5";
			label5.Size = new Size(33, 15);
			label5.TabIndex = 10;
			label5.Text = "User";
			// 
			// User
			// 
			User.AutoSize = true;
			User.ImageAlign = ContentAlignment.TopLeft;
			User.Location = new Point(95, 190);
			User.Margin = new Padding(3, 6, 3, 6);
			User.Name = "User";
			User.Size = new Size(45, 15);
			User.TabIndex = 9;
			User.Text = "<user>";
			User.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// AuthenticattionForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			AutoSize = true;
			CancelButton = Reject;
			ClientSize = new Size(527, 370);
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
			Controls.Add(Keys);
			Controls.Add(Logo);
			Controls.Add(groupBox1);
			Controls.Add(label4);
			Controls.Add(Application);
			Controls.Add(Ask);
			Controls.Add(Allow);
			Controls.Add(Reject);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Name = "AuthenticattionForm";
			Padding = new Padding(0, 0, 0, 9);
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
		private ComboBox Keys;
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