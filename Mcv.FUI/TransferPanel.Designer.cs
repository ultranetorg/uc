
namespace Uccs.Mcv.FUI
{
	partial class TransferPanel
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			pictureBox1 = new PictureBox();
			To = new TextBox();
			From = new TextBox();
			Amount = new TextBox();
			label6 = new Label();
			label4 = new Label();
			label1 = new Label();
			label7 = new Label();
			label2 = new Label();
			label5 = new Label();
			Balance = new Label();
			BalanceLabel = new Label();
			ToClass = new ComboBox();
			FromClass = new ComboBox();
			Asset = new ComboBox();
			Send = new Button();
			FromAs = new ComboBox();
			ToWhat = new ComboBox();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			SuspendLayout();
			// 
			// pictureBox1
			// 
			pictureBox1.Image = Properties.Resources.right_arrow;
			pictureBox1.Location = new Point(877, 52);
			pictureBox1.Margin = new Padding(6);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new Size(122, 105);
			pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
			pictureBox1.TabIndex = 39;
			pictureBox1.TabStop = false;
			// 
			// To
			// 
			To.Location = new Point(1011, 118);
			To.Margin = new Padding(6, 13, 6, 13);
			To.Name = "To";
			To.Size = new Size(379, 39);
			To.TabIndex = 38;
			To.TextChanged += Any_Changed;
			// 
			// From
			// 
			From.Location = new Point(486, 118);
			From.Margin = new Padding(6, 13, 6, 13);
			From.Name = "From";
			From.Size = new Size(379, 39);
			From.TabIndex = 37;
			From.TextChanged += FromId_TextChanged;
			// 
			// Amount
			// 
			Amount.Location = new Point(653, 326);
			Amount.Margin = new Padding(6, 13, 6, 13);
			Amount.Name = "Amount";
			Amount.Size = new Size(566, 39);
			Amount.TabIndex = 36;
			Amount.TextChanged += Any_Changed;
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label6.Location = new Point(534, 329);
			label6.Margin = new Padding(6, 0, 6, 0);
			label6.Name = "label6";
			label6.Size = new Size(107, 32);
			label6.TabIndex = 28;
			label6.Text = "Amount";
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label4.Location = new Point(1586, 121);
			label4.Margin = new Padding(6, 0, 6, 0);
			label4.Name = "label4";
			label4.Size = new Size(120, 32);
			label4.TabIndex = 32;
			label4.Text = "Recipient";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label1.Location = new Point(1402, 55);
			label1.Margin = new Padding(6, 0, 6, 0);
			label1.Name = "label1";
			label1.Size = new Size(184, 32);
			label1.TabIndex = 31;
			label1.Text = "Recipient Class";
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label7.Location = new Point(317, 55);
			label7.Margin = new Padding(6, 0, 6, 0);
			label7.Name = "label7";
			label7.Size = new Size(157, 32);
			label7.TabIndex = 33;
			label7.Text = "Sender Class";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label2.Location = new Point(197, 121);
			label2.Margin = new Padding(6, 0, 6, 0);
			label2.Name = "label2";
			label2.Size = new Size(93, 32);
			label2.TabIndex = 30;
			label2.Text = "Sender";
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label5.Location = new Point(566, 205);
			label5.Margin = new Padding(6, 0, 6, 0);
			label5.Name = "label5";
			label5.Size = new Size(75, 32);
			label5.TabIndex = 29;
			label5.Text = "Asset";
			// 
			// Balance
			// 
			Balance.AutoSize = true;
			Balance.Font = new Font("Segoe UI", 9F);
			Balance.Location = new Point(653, 268);
			Balance.Margin = new Padding(6, 13, 6, 13);
			Balance.Name = "Balance";
			Balance.Size = new Size(49, 32);
			Balance.TabIndex = 34;
			Balance.Text = ".......";
			// 
			// BalanceLabel
			// 
			BalanceLabel.AutoSize = true;
			BalanceLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			BalanceLabel.Location = new Point(453, 268);
			BalanceLabel.Margin = new Padding(6, 0, 6, 0);
			BalanceLabel.Name = "BalanceLabel";
			BalanceLabel.Size = new Size(188, 32);
			BalanceLabel.TabIndex = 35;
			BalanceLabel.Text = "Sender Balance";
			// 
			// ToClass
			// 
			ToClass.DropDownStyle = ComboBoxStyle.DropDownList;
			ToClass.FormattingEnabled = true;
			ToClass.Location = new Point(1011, 52);
			ToClass.Margin = new Padding(6, 13, 6, 13);
			ToClass.Name = "ToClass";
			ToClass.Size = new Size(379, 40);
			ToClass.TabIndex = 27;
			// 
			// FromClass
			// 
			FromClass.DropDownStyle = ComboBoxStyle.DropDownList;
			FromClass.FormattingEnabled = true;
			FromClass.Location = new Point(486, 52);
			FromClass.Margin = new Padding(6, 13, 6, 13);
			FromClass.Name = "FromClass";
			FromClass.Size = new Size(379, 40);
			FromClass.TabIndex = 26;
			// 
			// Asset
			// 
			Asset.DropDownStyle = ComboBoxStyle.DropDownList;
			Asset.FormattingEnabled = true;
			Asset.Location = new Point(653, 202);
			Asset.Margin = new Padding(6, 13, 6, 13);
			Asset.Name = "Asset";
			Asset.Size = new Size(566, 40);
			Asset.TabIndex = 25;
			Asset.SelectionChangeCommitted += Asset_SelectionChangeCommitted;
			Asset.TextChanged += Any_Changed;
			// 
			// Send
			// 
			Send.Location = new Point(826, 417);
			Send.Margin = new Padding(6);
			Send.Name = "Send";
			Send.Size = new Size(240, 60);
			Send.TabIndex = 23;
			Send.Text = "Send";
			Send.UseVisualStyleBackColor = true;
			// 
			// FromWhat
			// 
			FromAs.DropDownStyle = ComboBoxStyle.DropDownList;
			FromAs.FormattingEnabled = true;
			FromAs.Location = new Point(302, 118);
			FromAs.Margin = new Padding(6, 13, 6, 13);
			FromAs.Name = "FromWhat";
			FromAs.Size = new Size(172, 40);
			FromAs.TabIndex = 26;
			// 
			// ToWhat
			// 
			ToWhat.DropDownStyle = ComboBoxStyle.DropDownList;
			ToWhat.FormattingEnabled = true;
			ToWhat.Location = new Point(1402, 118);
			ToWhat.Margin = new Padding(6, 13, 6, 13);
			ToWhat.Name = "ToWhat";
			ToWhat.Size = new Size(172, 40);
			ToWhat.TabIndex = 26;
			// 
			// TransferPanel
			// 
			AutoScaleDimensions = new SizeF(13F, 32F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(pictureBox1);
			Controls.Add(To);
			Controls.Add(From);
			Controls.Add(Amount);
			Controls.Add(label6);
			Controls.Add(label4);
			Controls.Add(label1);
			Controls.Add(label7);
			Controls.Add(label2);
			Controls.Add(label5);
			Controls.Add(Balance);
			Controls.Add(BalanceLabel);
			Controls.Add(ToClass);
			Controls.Add(ToWhat);
			Controls.Add(FromAs);
			Controls.Add(FromClass);
			Controls.Add(Asset);
			Controls.Add(Send);
			Margin = new Padding(7, 6, 7, 6);
			Name = "TransferPanel";
			Size = new Size(1902, 1638);
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private PictureBox pictureBox1;
		private TextBox To;
		private TextBox From;
		private TextBox Amount;
		private Label label6;
		private Label label4;
		private Label label1;
		private Label label7;
		private Label label2;
		private Label label5;
		private Label Balance;
		private Label BalanceLabel;
		private ComboBox ToClass;
		private ComboBox FromClass;
		private ComboBox Asset;
		private Button Send;
		private ComboBox FromAs;
		private ComboBox ToWhat;
	}
}
