namespace Uccs.Net.FUI
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
			cancel = new System.Windows.Forms.Button();
			password = new System.Windows.Forms.TextBox();
			groupBox1 = new System.Windows.Forms.GroupBox();
			ok = new System.Windows.Forms.Button();
			info = new System.Windows.Forms.Label();
			pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			SuspendLayout();
			// 
			// cancel
			// 
			cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			cancel.Location = new System.Drawing.Point(289, 128);
			cancel.Margin = new System.Windows.Forms.Padding(9);
			cancel.Name = "cancel";
			cancel.Size = new System.Drawing.Size(93, 28);
			cancel.TabIndex = 3;
			cancel.Text = "Cancel";
			cancel.UseVisualStyleBackColor = true;
			cancel.Click += cancel_Click;
			// 
			// password
			// 
			password.Location = new System.Drawing.Point(85, 66);
			password.Name = "password";
			password.PasswordChar = '*';
			password.Size = new System.Drawing.Size(297, 23);
			password.TabIndex = 1;
			// 
			// groupBox1
			// 
			groupBox1.Location = new System.Drawing.Point(14, 101);
			groupBox1.Margin = new System.Windows.Forms.Padding(9);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new System.Drawing.Size(368, 9);
			groupBox1.TabIndex = 5;
			groupBox1.TabStop = false;
			// 
			// ok
			// 
			ok.Location = new System.Drawing.Point(178, 129);
			ok.Margin = new System.Windows.Forms.Padding(9);
			ok.Name = "ok";
			ok.Size = new System.Drawing.Size(93, 28);
			ok.TabIndex = 3;
			ok.Text = "OK";
			ok.UseVisualStyleBackColor = true;
			ok.Click += ok_Click;
			// 
			// info
			// 
			info.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
			info.Location = new System.Drawing.Point(84, 21);
			info.Name = "info";
			info.Size = new System.Drawing.Size(298, 40);
			info.TabIndex = 4;
			info.Text = "0x3405034759347598347953495349";
			info.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// pictureBox1
			// 
			pictureBox1.Image = Uccs.Net.FUI.Properties.Resources.Golden_key_icon_svg;
			pictureBox1.Location = new System.Drawing.Point(14, 21);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new System.Drawing.Size(64, 68);
			pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			pictureBox1.TabIndex = 6;
			pictureBox1.TabStop = false;
			// 
			// EnterPasswordForm
			// 
			AcceptButton = ok;
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			AutoSize = true;
			CancelButton = cancel;
			ClientSize = new System.Drawing.Size(417, 174);
			ControlBox = false;
			Controls.Add(pictureBox1);
			Controls.Add(groupBox1);
			Controls.Add(password);
			Controls.Add(info);
			Controls.Add(ok);
			Controls.Add(cancel);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			Name = "EnterPasswordForm";
			Padding = new System.Windows.Forms.Padding(0, 0, 0, 9);
			SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Password Required";
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		public System.Windows.Forms.TextBox gasprice;
		private System.Windows.Forms.Button cancel;
		public System.Windows.Forms.TextBox password;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Label info;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}