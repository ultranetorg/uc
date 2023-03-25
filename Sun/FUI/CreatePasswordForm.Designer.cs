namespace Uccs.Sun.FUI
{
	partial class CreatePasswordForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreatePasswordForm));
			this.cancel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.passwordConfirmLabel = new System.Windows.Forms.Label();
			this.password = new System.Windows.Forms.TextBox();
			this.passwordConfirm = new System.Windows.Forms.TextBox();
			this.ok = new System.Windows.Forms.Button();
			this.suggestions = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(358, 834);
			this.cancel.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(173, 59);
			this.cancel.TabIndex = 3;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(43, 662);
			this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(111, 32);
			this.label2.TabIndex = 4;
			this.label2.Text = "Password";
			// 
			// passwordConfirmLabel
			// 
			this.passwordConfirmLabel.AutoSize = true;
			this.passwordConfirmLabel.Location = new System.Drawing.Point(43, 743);
			this.passwordConfirmLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.passwordConfirmLabel.Name = "passwordConfirmLabel";
			this.passwordConfirmLabel.Size = new System.Drawing.Size(258, 32);
			this.passwordConfirmLabel.TabIndex = 4;
			this.passwordConfirmLabel.Text = "Password Confirmation";
			// 
			// password
			// 
			this.password.Location = new System.Drawing.Point(310, 655);
			this.password.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
			this.password.Name = "password";
			this.password.PasswordChar = '*';
			this.password.Size = new System.Drawing.Size(424, 39);
			this.password.TabIndex = 0;
			// 
			// passwordConfirm
			// 
			this.passwordConfirm.Location = new System.Drawing.Point(310, 736);
			this.passwordConfirm.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
			this.passwordConfirm.Name = "passwordConfirm";
			this.passwordConfirm.PasswordChar = '*';
			this.passwordConfirm.Size = new System.Drawing.Size(424, 39);
			this.passwordConfirm.TabIndex = 1;
			// 
			// ok
			// 
			this.ok.Location = new System.Drawing.Point(561, 834);
			this.ok.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
			this.ok.Name = "ok";
			this.ok.Size = new System.Drawing.Size(173, 59);
			this.ok.TabIndex = 2;
			this.ok.Text = "OK";
			this.ok.UseVisualStyleBackColor = true;
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// suggestions
			// 
			this.suggestions.Location = new System.Drawing.Point(43, 22);
			this.suggestions.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.suggestions.Name = "suggestions";
			this.suggestions.Size = new System.Drawing.Size(696, 593);
			this.suggestions.TabIndex = 4;
			this.suggestions.Text = resources.GetString("suggestions.Text");
			// 
			// CreatePasswordForm
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(785, 948);
			this.ControlBox = false;
			this.Controls.Add(this.passwordConfirm);
			this.Controls.Add(this.password);
			this.Controls.Add(this.passwordConfirmLabel);
			this.Controls.Add(this.suggestions);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.cancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
			this.Name = "CreatePasswordForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Create Account Password";
			this.ResumeLayout(false);
			this.PerformLayout();

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
	}
}