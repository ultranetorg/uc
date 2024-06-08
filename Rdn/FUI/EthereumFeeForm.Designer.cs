namespace Uccs.Rdn.FUI
{
	partial class EthereumFeeForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EthereumFeeForm));
			this.gas = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.send = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.account = new System.Windows.Forms.TextBox();
			this.balance = new System.Windows.Forms.TextBox();
			this.eth = new System.Windows.Forms.TextBox();
			this.gasprice = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.cancel = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// gas
			// 
			this.gas.Location = new System.Drawing.Point(280, 234);
			this.gas.Margin = new System.Windows.Forms.Padding(6);
			this.gas.Name = "gas";
			this.gas.Size = new System.Drawing.Size(329, 39);
			this.gas.TabIndex = 6;
			this.gas.Text = "21000";
			this.gas.TextChanged += new System.EventHandler(this.gas_TextChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(197, 234);
			this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(52, 27);
			this.label2.TabIndex = 4;
			this.label2.Text = "Gas";
			// 
			// send
			// 
			this.send.Location = new System.Drawing.Point(762, 640);
			this.send.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
			this.send.Name = "send";
			this.send.Size = new System.Drawing.Size(202, 57);
			this.send.TabIndex = 7;
			this.send.Text = "Confirm";
			this.send.UseVisualStyleBackColor = true;
			this.send.Click += new System.EventHandler(this.send_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.account);
			this.groupBox1.Controls.Add(this.balance);
			this.groupBox1.Controls.Add(this.eth);
			this.groupBox1.Controls.Add(this.gasprice);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.gas);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Location = new System.Drawing.Point(310, 25);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
			this.groupBox1.Size = new System.Drawing.Size(890, 574);
			this.groupBox1.TabIndex = 8;
			this.groupBox1.TabStop = false;
			// 
			// account
			// 
			this.account.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.account.Location = new System.Drawing.Point(280, 71);
			this.account.Margin = new System.Windows.Forms.Padding(6);
			this.account.Name = "account";
			this.account.ReadOnly = true;
			this.account.Size = new System.Drawing.Size(576, 32);
			this.account.TabIndex = 6;
			this.account.TextChanged += new System.EventHandler(this.gas_TextChanged);
			// 
			// balance
			// 
			this.balance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.balance.Location = new System.Drawing.Point(280, 158);
			this.balance.Margin = new System.Windows.Forms.Padding(6);
			this.balance.Name = "balance";
			this.balance.ReadOnly = true;
			this.balance.Size = new System.Drawing.Size(329, 32);
			this.balance.TabIndex = 6;
			this.balance.TextChanged += new System.EventHandler(this.gas_TextChanged);
			// 
			// eth
			// 
			this.eth.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.eth.Location = new System.Drawing.Point(280, 463);
			this.eth.Margin = new System.Windows.Forms.Padding(6);
			this.eth.Name = "eth";
			this.eth.ReadOnly = true;
			this.eth.Size = new System.Drawing.Size(329, 32);
			this.eth.TabIndex = 6;
			this.eth.TextChanged += new System.EventHandler(this.gas_TextChanged);
			// 
			// gasprice
			// 
			this.gasprice.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.gasprice.Location = new System.Drawing.Point(280, 315);
			this.gasprice.Margin = new System.Windows.Forms.Padding(6);
			this.gasprice.Name = "gasprice";
			this.gasprice.Size = new System.Drawing.Size(329, 32);
			this.gasprice.TabIndex = 6;
			this.gasprice.Text = "20";
			this.gasprice.TextChanged += new System.EventHandler(this.gas_TextChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label5.Location = new System.Drawing.Point(74, 79);
			this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(166, 27);
			this.label5.TabIndex = 4;
			this.label5.Text = "From Account";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label3.Location = new System.Drawing.Point(46, 470);
			this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(193, 27);
			this.label3.TabIndex = 4;
			this.label3.Text = "Total Cost (ETH)";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label4.Location = new System.Drawing.Point(74, 160);
			this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(169, 27);
			this.label4.TabIndex = 4;
			this.label4.Text = "Balance (ETH)";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(43, 322);
			this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(196, 27);
			this.label1.TabIndex = 4;
			this.label1.Text = "Gas Price (Gwei)";
			// 
			// cancel
			// 
			this.cancel.Location = new System.Drawing.Point(998, 640);
			this.cancel.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(202, 57);
			this.cancel.TabIndex = 7;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(38, 42);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(249, 557);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 9;
			this.pictureBox1.TabStop = false;
			// 
			// EthereumFeeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1235, 731);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.send);
			this.Controls.Add(this.cancel);
			this.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
			this.Name = "EthereumFeeForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Confirm Ethereum Transaction";
			this.Load += new System.EventHandler(this.FeeForm_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button send;
		private System.Windows.Forms.GroupBox groupBox1;
		public System.Windows.Forms.TextBox gas;
		public System.Windows.Forms.TextBox gasprice;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Label label3;
		public System.Windows.Forms.TextBox balance;
		private System.Windows.Forms.Label label4;
		public System.Windows.Forms.TextBox account;
		private System.Windows.Forms.Label label5;
		public System.Windows.Forms.TextBox eth;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}