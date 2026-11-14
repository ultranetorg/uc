namespace Uccs.Nexus.Windows
{
	partial class CreateWalletForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateWalletForm));
			cancel = new Button();
			label2 = new Label();
			passwordConfirmLabel = new Label();
			password = new TextBox();
			passwordConfirm = new TextBox();
			ok = new Button();
			suggestions = new Label();
			label1 = new Label();
			walletname = new TextBox();
			SuspendLayout();
			// 
			// cancel
			// 
			cancel.DialogResult = DialogResult.Cancel;
			cancel.Location = new Point(206, 427);
			cancel.Name = "cancel";
			cancel.Size = new Size(93, 28);
			cancel.TabIndex = 3;
			cancel.Text = "Cancel";
			cancel.UseVisualStyleBackColor = true;
			cancel.Click += cancel_Click;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(26, 72);
			label2.Name = "label2";
			label2.Size = new Size(57, 15);
			label2.TabIndex = 4;
			label2.Text = "Password";
			// 
			// passwordConfirmLabel
			// 
			passwordConfirmLabel.AutoSize = true;
			passwordConfirmLabel.Location = new Point(26, 101);
			passwordConfirmLabel.Name = "passwordConfirmLabel";
			passwordConfirmLabel.Size = new Size(131, 15);
			passwordConfirmLabel.TabIndex = 4;
			passwordConfirmLabel.Text = "Password Confirmation";
			// 
			// password
			// 
			password.Location = new Point(168, 69);
			password.Name = "password";
			password.PasswordChar = '*';
			password.Size = new Size(230, 23);
			password.TabIndex = 0;
			// 
			// passwordConfirm
			// 
			passwordConfirm.Location = new Point(168, 98);
			passwordConfirm.Name = "passwordConfirm";
			passwordConfirm.PasswordChar = '*';
			passwordConfirm.Size = new Size(230, 23);
			passwordConfirm.TabIndex = 1;
			// 
			// ok
			// 
			ok.Location = new Point(305, 427);
			ok.Name = "ok";
			ok.Size = new Size(93, 28);
			ok.TabIndex = 2;
			ok.Text = "OK";
			ok.UseVisualStyleBackColor = true;
			ok.Click += ok_Click;
			// 
			// suggestions
			// 
			suggestions.Location = new Point(26, 148);
			suggestions.Name = "suggestions";
			suggestions.Size = new Size(372, 276);
			suggestions.TabIndex = 4;
			suggestions.Text = resources.GetString("suggestions.Text");
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(26, 27);
			label1.Name = "label1";
			label1.Size = new Size(75, 15);
			label1.TabIndex = 4;
			label1.Text = "Wallet Name";
			// 
			// walletname
			// 
			walletname.Location = new Point(168, 24);
			walletname.Name = "walletname";
			walletname.Size = new Size(230, 23);
			walletname.TabIndex = 0;
			// 
			// CreateWalletForm
			// 
			AcceptButton = ok;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = cancel;
			ClientSize = new Size(419, 475);
			ControlBox = false;
			Controls.Add(passwordConfirm);
			Controls.Add(walletname);
			Controls.Add(password);
			Controls.Add(passwordConfirmLabel);
			Controls.Add(suggestions);
			Controls.Add(label1);
			Controls.Add(label2);
			Controls.Add(ok);
			Controls.Add(cancel);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Name = "CreateWalletForm";
			SizeGripStyle = SizeGripStyle.Hide;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Create Wallet";
			ResumeLayout(false);
			PerformLayout();

		}

		#endregion
		public System.Windows.Forms.TextBox gasprice;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label passwordConfirmLabel;
		public System.Windows.Forms.TextBox password;
		public System.Windows.Forms.TextBox passwordConfirm;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Label suggestions;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.TextBox walletname;
	}
}