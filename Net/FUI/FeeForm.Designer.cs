namespace Uccs.Net.FUI
{
	partial class FeeForm
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.from = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.cancel = new System.Windows.Forms.Button();
			this.fee = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// send
			// 
			this.send.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.send.Location = new System.Drawing.Point(236, 136);
			this.send.Name = "send";
			this.send.Size = new System.Drawing.Size(93, 23);
			this.send.TabIndex = 1;
			this.send.Text = "Confirm";
			this.send.UseVisualStyleBackColor = true;
			this.send.Click += new System.EventHandler(this.send_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.fee);
			this.groupBox1.Controls.Add(this.from);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(429, 106);
			this.groupBox1.TabIndex = 8;
			this.groupBox1.TabStop = false;
			// 
			// from
			// 
			this.from.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.from.Location = new System.Drawing.Point(125, 29);
			this.from.Margin = new System.Windows.Forms.Padding(6);
			this.from.Name = "from";
			this.from.ReadOnly = true;
			this.from.Size = new System.Drawing.Size(280, 20);
			this.from.TabIndex = 3;
			this.from.TabStop = false;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label5.Location = new System.Drawing.Point(19, 32);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(85, 13);
			this.label5.TabIndex = 4;
			this.label5.Text = "From Account";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(242, 64);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(27, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "UNT";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label3.Location = new System.Drawing.Point(17, 64);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(99, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Estimanted Cost";
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(348, 136);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(93, 23);
			this.cancel.TabIndex = 2;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// fee
			// 
			this.fee.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.fee.Location = new System.Drawing.Point(125, 61);
			this.fee.Margin = new System.Windows.Forms.Padding(6);
			this.fee.Name = "fee";
			this.fee.ReadOnly = true;
			this.fee.Size = new System.Drawing.Size(108, 20);
			this.fee.TabIndex = 3;
			this.fee.TabStop = false;
			this.fee.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FeeForm
			// 
			this.AcceptButton = this.send;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(459, 175);
			this.ControlBox = false;
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.send);
			this.Controls.Add(this.cancel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FeeForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Confirm Transaction";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button send;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.TextBox from;
		public System.Windows.Forms.TextBox fee;
	}
}