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
			this.send = new System.Windows.Forms.Button();
			this.text = new System.Windows.Forms.TextBox();
			this.message = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// send
			// 
			this.send.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.send.Location = new System.Drawing.Point(318, 168);
			this.send.Name = "send";
			this.send.Size = new System.Drawing.Size(93, 23);
			this.send.TabIndex = 7;
			this.send.Text = "OK";
			this.send.UseVisualStyleBackColor = true;
			this.send.Click += new System.EventHandler(this.send_Click);
			// 
			// text
			// 
			this.text.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.text.Location = new System.Drawing.Point(12, 93);
			this.text.Multiline = true;
			this.text.Name = "text";
			this.text.ReadOnly = true;
			this.text.Size = new System.Drawing.Size(399, 57);
			this.text.TabIndex = 6;
			this.text.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// message
			// 
			this.message.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.message.Location = new System.Drawing.Point(11, 13);
			this.message.Name = "message";
			this.message.Size = new System.Drawing.Size(399, 77);
			this.message.TabIndex = 4;
			this.message.Text = "From Account dgdfg;dfkg;lkd;flgk;dlfkg;ldfk;gk;dflkg;ldfkg;lkdf;lgkd\r\nsdf\'s;df;ls" +
    "df\r\nsdf";
			// 
			// TextForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(425, 205);
			this.ControlBox = false;
			this.Controls.Add(this.message);
			this.Controls.Add(this.text);
			this.Controls.Add(this.send);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TextForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Confirm Transaction";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button send;
		public System.Windows.Forms.TextBox account;
		private System.Windows.Forms.Label message;
		public System.Windows.Forms.TextBox text;
	}
}