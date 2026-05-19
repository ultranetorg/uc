namespace Uccs.Nexus.Windows
{
	partial class RdnTransferForm
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
			Cancel = new Button();
			Ok = new Button();
			Amount = new TextBox();
			label6 = new Label();
			label5 = new Label();
			BalanceLabel = new Label();
			Asset = new ComboBox();
			Entity = new TextBox();
			label2 = new Label();
			label3 = new Label();
			ToEntity = new Button();
			pictureBox1 = new PictureBox();
			ToNet = new ComboBox();
			label1 = new Label();
			label4 = new Label();
			comboBox1 = new ComboBox();
			label7 = new Label();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			SuspendLayout();
			// 
			// Cancel
			// 
			Cancel.DialogResult = DialogResult.Cancel;
			Cancel.Location = new Point(542, 375);
			Cancel.Margin = new Padding(6);
			Cancel.Name = "Cancel";
			Cancel.Size = new Size(240, 60);
			Cancel.TabIndex = 3;
			Cancel.Text = "Cancel";
			Cancel.UseVisualStyleBackColor = true;
			Cancel.Click += cancel_Click;
			// 
			// Ok
			// 
			Ok.Location = new Point(794, 375);
			Ok.Margin = new Padding(6);
			Ok.Name = "Ok";
			Ok.Size = new Size(240, 60);
			Ok.TabIndex = 2;
			Ok.Text = "OK";
			Ok.UseVisualStyleBackColor = true;
			Ok.Click += ok_Click;
			// 
			// Amount
			// 
			Amount.Location = new Point(138, 282);
			Amount.Margin = new Padding(6, 13, 6, 13);
			Amount.Name = "Amount";
			Amount.Size = new Size(566, 39);
			Amount.TabIndex = 21;
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label6.Location = new Point(19, 285);
			label6.Margin = new Padding(6, 0, 6, 0);
			label6.Name = "label6";
			label6.Size = new Size(107, 32);
			label6.TabIndex = 18;
			label6.Text = "Amount";
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label5.Location = new Point(51, 161);
			label5.Margin = new Padding(6, 0, 6, 0);
			label5.Name = "label5";
			label5.Size = new Size(75, 32);
			label5.TabIndex = 19;
			label5.Text = "Asset";
			// 
			// BalanceLabel
			// 
			BalanceLabel.AutoSize = true;
			BalanceLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			BalanceLabel.Location = new Point(24, 224);
			BalanceLabel.Margin = new Padding(6, 0, 6, 0);
			BalanceLabel.Name = "BalanceLabel";
			BalanceLabel.Size = new Size(102, 32);
			BalanceLabel.TabIndex = 20;
			BalanceLabel.Text = "Balance";
			// 
			// Asset
			// 
			Asset.DropDownStyle = ComboBoxStyle.DropDownList;
			Asset.FormattingEnabled = true;
			Asset.Location = new Point(138, 158);
			Asset.Margin = new Padding(6, 13, 6, 13);
			Asset.Name = "Asset";
			Asset.Size = new Size(566, 40);
			Asset.TabIndex = 17;
			Asset.SelectionChangeCommitted += Asset_SelectionChangeCommitted;
			// 
			// Entity
			// 
			Entity.Location = new Point(325, 95);
			Entity.Margin = new Padding(6, 13, 6, 13);
			Entity.Name = "Entity";
			Entity.Size = new Size(379, 39);
			Entity.TabIndex = 21;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label2.Location = new Point(190, 98);
			label2.Margin = new Padding(6, 0, 6, 0);
			label2.Name = "label2";
			label2.Size = new Size(123, 32);
			label2.TabIndex = 19;
			label2.Text = "Sender Id";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new Font("Segoe UI", 9F);
			label3.Location = new Point(138, 224);
			label3.Margin = new Padding(6, 13, 6, 13);
			label3.Name = "label3";
			label3.Size = new Size(157, 32);
			label3.TabIndex = 20;
			label3.Text = "01234566789";
			// 
			// ToEntity
			// 
			ToEntity.Location = new Point(850, 91);
			ToEntity.Margin = new Padding(6);
			ToEntity.Name = "ToEntity";
			ToEntity.Size = new Size(533, 230);
			ToEntity.TabIndex = 2;
			ToEntity.Text = "Choose Recipient";
			ToEntity.UseVisualStyleBackColor = true;
			ToEntity.Click += ok_Click;
			// 
			// pictureBox1
			// 
			pictureBox1.Image = Properties.Resources.right_arrow;
			pictureBox1.Location = new Point(716, 29);
			pictureBox1.Margin = new Padding(6);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new Size(122, 292);
			pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
			pictureBox1.TabIndex = 22;
			pictureBox1.TabStop = false;
			// 
			// ToNet
			// 
			ToNet.FormattingEnabled = true;
			ToNet.Location = new Point(850, 29);
			ToNet.Margin = new Padding(6, 13, 6, 13);
			ToNet.Name = "ToNet";
			ToNet.Size = new Size(385, 40);
			ToNet.TabIndex = 16;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label1.Location = new Point(1247, 35);
			label1.Margin = new Padding(6, 0, 6, 0);
			label1.Name = "label1";
			label1.Size = new Size(55, 32);
			label1.TabIndex = 19;
			label1.Text = "Net";
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label4.Location = new Point(1395, 91);
			label4.Margin = new Padding(6, 0, 6, 0);
			label4.Name = "label4";
			label4.Size = new Size(120, 32);
			label4.TabIndex = 19;
			label4.Text = "Recipient";
			// 
			// comboBox1
			// 
			comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox1.FormattingEnabled = true;
			comboBox1.Location = new Point(325, 29);
			comboBox1.Margin = new Padding(6, 13, 6, 13);
			comboBox1.Name = "comboBox1";
			comboBox1.Size = new Size(379, 40);
			comboBox1.TabIndex = 17;
			comboBox1.SelectionChangeCommitted += Asset_SelectionChangeCommitted;
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label7.Location = new Point(156, 32);
			label7.Margin = new Padding(6, 0, 6, 0);
			label7.Name = "label7";
			label7.Size = new Size(157, 32);
			label7.TabIndex = 19;
			label7.Text = "Sender Class";
			// 
			// RdnTransferForm
			// 
			AcceptButton = Ok;
			AutoScaleDimensions = new SizeF(13F, 32F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = Cancel;
			ClientSize = new Size(1548, 483);
			ControlBox = false;
			Controls.Add(pictureBox1);
			Controls.Add(Entity);
			Controls.Add(Amount);
			Controls.Add(label6);
			Controls.Add(label4);
			Controls.Add(label1);
			Controls.Add(label7);
			Controls.Add(label2);
			Controls.Add(label5);
			Controls.Add(label3);
			Controls.Add(BalanceLabel);
			Controls.Add(ToNet);
			Controls.Add(comboBox1);
			Controls.Add(Asset);
			Controls.Add(ToEntity);
			Controls.Add(Ok);
			Controls.Add(Cancel);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Margin = new Padding(6);
			Name = "RdnTransferForm";
			SizeGripStyle = SizeGripStyle.Hide;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Transfer from RDN";
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			ResumeLayout(false);
			PerformLayout();

		}

		#endregion
		public System.Windows.Forms.TextBox gasprice;
		private System.Windows.Forms.Button Cancel;
		private System.Windows.Forms.Button Ok;
		private TextBox Amount;
		private Label label6;
		private Label label5;
		private Label BalanceLabel;
		private ComboBox Asset;
		private TextBox Entity;
		private Label label2;
		private Label label3;
		private Button ToEntity;
		private PictureBox pictureBox1;
		private ComboBox ToNet;
		private Label label1;
		private Label label4;
		private ComboBox comboBox1;
		private Label label7;
	}
}