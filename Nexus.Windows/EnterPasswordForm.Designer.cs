namespace Uccs.Nexus.Windows
{
	partial class EnterPasswordForm
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
			password = new TextBox();
			ok = new Button();
			info = new Label();
			pictureBox1 = new PictureBox();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			SuspendLayout();
			// 
			// password
			// 
			password.Font = new Font("Segoe UI", 12F);
			password.Location = new Point(16, 96);
			password.Name = "password";
			password.PasswordChar = '*';
			password.Size = new Size(265, 29);
			password.TabIndex = 1;
			password.TextAlign = HorizontalAlignment.Center;
			// 
			// ok
			// 
			ok.Location = new Point(293, 93);
			ok.Margin = new Padding(9);
			ok.Name = "ok";
			ok.Size = new Size(107, 32);
			ok.TabIndex = 3;
			ok.Text = "Unlock";
			ok.UseVisualStyleBackColor = true;
			ok.Click += ok_Click;
			// 
			// info
			// 
			info.ImageAlign = ContentAlignment.TopLeft;
			info.Location = new Point(126, 3);
			info.Name = "info";
			info.Size = new Size(274, 81);
			info.TabIndex = 4;
			info.Text = "0x3405034759347598347953495349";
			info.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// pictureBox1
			// 
			pictureBox1.Image = Properties.Resources.key_13705428;
			pictureBox1.Location = new Point(16, 3);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new Size(104, 81);
			pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBox1.TabIndex = 6;
			pictureBox1.TabStop = false;
			// 
			// EnterPasswordForm
			// 
			AcceptButton = ok;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			AutoSize = true;
			ClientSize = new Size(416, 144);
			Controls.Add(password);
			Controls.Add(pictureBox1);
			Controls.Add(info);
			Controls.Add(ok);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "EnterPasswordForm";
			Padding = new Padding(0, 0, 0, 9);
			SizeGripStyle = SizeGripStyle.Hide;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Password Required";
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		public System.Windows.Forms.TextBox gasprice;
		public System.Windows.Forms.TextBox password;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Label info;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}