namespace Uccs.Nexus.Windows
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
			send.Location = new System.Drawing.Point(412, 177);
			send.Name = "send";
			send.Size = new System.Drawing.Size(109, 27);
			send.TabIndex = 7;
			send.Text = "OK";
			send.UseVisualStyleBackColor = true;
			send.Click += OK_Click;
			// 
			// text
			// 
			text.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
			text.Location = new System.Drawing.Point(12, 68);
			text.Margin = new System.Windows.Forms.Padding(0);
			text.Multiline = true;
			text.Name = "text";
			text.ReadOnly = true;
			text.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			text.Size = new System.Drawing.Size(508, 91);
			text.TabIndex = 6;
			text.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// message
			// 
			message.AutoSize = true;
			message.Font = new System.Drawing.Font("Tahoma", 8.25F);
			message.Location = new System.Drawing.Point(12, 17);
			message.Margin = new System.Windows.Forms.Padding(0);
			message.Name = "message";
			message.Size = new System.Drawing.Size(328, 39);
			message.TabIndex = 4;
			message.Text = "From Account dgdfg;dfkg;lkd;flgk;dlfkg;ldfk;gk;dflkg;ldfkg;lkdf;lgkd\r\nsdf's;df;lsdf\r\nsdf";
			// 
			// TextForm
			// 
			AcceptButton = send;
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			AutoSize = true;
			ClientSize = new System.Drawing.Size(532, 215);
			Controls.Add(message);
			Controls.Add(text);
			Controls.Add(send);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "TextForm";
			Padding = new System.Windows.Forms.Padding(8);
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