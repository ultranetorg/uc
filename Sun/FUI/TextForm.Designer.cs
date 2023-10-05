namespace Uccs.Sun.FUI
{
	partial class TextForm
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
			send = new System.Windows.Forms.Button();
			text = new System.Windows.Forms.TextBox();
			message = new System.Windows.Forms.Label();
			SuspendLayout();
			// 
			// send
			// 
			send.DialogResult = System.Windows.Forms.DialogResult.OK;
			send.Location = new System.Drawing.Point(689, 414);
			send.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
			send.Name = "send";
			send.Size = new System.Drawing.Size(202, 57);
			send.TabIndex = 7;
			send.Text = "OK";
			send.UseVisualStyleBackColor = true;
			send.Click += send_Click;
			// 
			// text
			// 
			text.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			text.Location = new System.Drawing.Point(26, 229);
			text.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
			text.Multiline = true;
			text.Name = "text";
			text.ReadOnly = true;
			text.Size = new System.Drawing.Size(860, 134);
			text.TabIndex = 6;
			text.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// message
			// 
			message.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			message.Location = new System.Drawing.Point(24, 32);
			message.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			message.Name = "message";
			message.Size = new System.Drawing.Size(865, 190);
			message.TabIndex = 4;
			message.Text = "From Account dgdfg;dfkg;lkd;flgk;dlfkg;ldfk;gk;dflkg;ldfkg;lkdf;lgkd\r\nsdf's;df;lsdf\r\nsdf";
			// 
			// TextForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(921, 505);
			ControlBox = false;
			Controls.Add(message);
			Controls.Add(text);
			Controls.Add(send);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "TextForm";
			SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Confirm Transaction";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.Button send;
		public System.Windows.Forms.TextBox account;
		private System.Windows.Forms.Label message;
		public System.Windows.Forms.TextBox text;
	}
}