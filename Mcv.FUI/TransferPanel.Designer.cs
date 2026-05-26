
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
			Transfer = new Button();
			FromAs = new ComboBox();
			ToWhat = new ComboBox();
			Transactions = new ListView();
			ChTag = new ColumnHeader();
			ChFromClass = new ColumnHeader();
			ChFromUser = new ColumnHeader();
			ChToClass = new ColumnHeader();
			ChToUser = new ColumnHeader();
			ChSpacetime = new ColumnHeader();
			ChEnergy = new ColumnHeader();
			ChEnergyNext = new ColumnHeader();
			ChStatus = new ColumnHeader();
			ChError = new ColumnHeader();
			label3 = new Label();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			SuspendLayout();
			// 
			// pictureBox1
			// 
			pictureBox1.Image = Properties.Resources.right_arrow;
			pictureBox1.Location = new Point(472, 24);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new Size(66, 54);
			pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
			pictureBox1.TabIndex = 39;
			pictureBox1.TabStop = false;
			// 
			// To
			// 
			To.Location = new Point(544, 55);
			To.Margin = new Padding(3, 6, 3, 6);
			To.Name = "To";
			To.Size = new Size(206, 23);
			To.TabIndex = 38;
			To.TextChanged += Any_Changed;
			// 
			// From
			// 
			From.Location = new Point(261, 55);
			From.Margin = new Padding(3, 6, 3, 6);
			From.Name = "From";
			From.Size = new Size(206, 23);
			From.TabIndex = 37;
			From.TextChanged += FromId_TextChanged;
			// 
			// Amount
			// 
			Amount.Location = new Point(351, 152);
			Amount.Margin = new Padding(3, 6, 3, 6);
			Amount.Name = "Amount";
			Amount.Size = new Size(307, 23);
			Amount.TabIndex = 36;
			Amount.TextChanged += Any_Changed;
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label6.Location = new Point(293, 155);
			label6.Name = "label6";
			label6.Size = new Size(52, 15);
			label6.TabIndex = 28;
			label6.Text = "Amount";
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label4.Location = new Point(855, 58);
			label4.Name = "label4";
			label4.Size = new Size(60, 15);
			label4.TabIndex = 32;
			label4.Text = "Recipient";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label1.Location = new Point(755, 27);
			label1.Name = "label1";
			label1.Size = new Size(89, 15);
			label1.TabIndex = 31;
			label1.Text = "Recipient Class";
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label7.Location = new Point(180, 27);
			label7.Name = "label7";
			label7.Size = new Size(76, 15);
			label7.TabIndex = 33;
			label7.Text = "Sender Class";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label2.Location = new Point(109, 58);
			label2.Name = "label2";
			label2.Size = new Size(47, 15);
			label2.TabIndex = 30;
			label2.Text = "Sender";
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label5.Location = new Point(304, 96);
			label5.Name = "label5";
			label5.Size = new Size(37, 15);
			label5.TabIndex = 29;
			label5.Text = "Asset";
			// 
			// Balance
			// 
			Balance.AutoSize = true;
			Balance.Font = new Font("Segoe UI", 9F);
			Balance.Location = new Point(351, 125);
			Balance.Margin = new Padding(3, 6, 3, 6);
			Balance.Name = "Balance";
			Balance.Size = new Size(28, 15);
			Balance.TabIndex = 34;
			Balance.Text = ".......";
			// 
			// BalanceLabel
			// 
			BalanceLabel.AutoSize = true;
			BalanceLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			BalanceLabel.Location = new Point(244, 125);
			BalanceLabel.Name = "BalanceLabel";
			BalanceLabel.Size = new Size(93, 15);
			BalanceLabel.TabIndex = 35;
			BalanceLabel.Text = "Sender Balance";
			// 
			// ToClass
			// 
			ToClass.DropDownStyle = ComboBoxStyle.DropDownList;
			ToClass.FormattingEnabled = true;
			ToClass.Location = new Point(544, 24);
			ToClass.Margin = new Padding(3, 6, 3, 6);
			ToClass.Name = "ToClass";
			ToClass.Size = new Size(206, 23);
			ToClass.TabIndex = 27;
			// 
			// FromClass
			// 
			FromClass.DropDownStyle = ComboBoxStyle.DropDownList;
			FromClass.FormattingEnabled = true;
			FromClass.Location = new Point(261, 24);
			FromClass.Margin = new Padding(3, 6, 3, 6);
			FromClass.Name = "FromClass";
			FromClass.Size = new Size(206, 23);
			FromClass.TabIndex = 26;
			// 
			// Asset
			// 
			Asset.DropDownStyle = ComboBoxStyle.DropDownList;
			Asset.FormattingEnabled = true;
			Asset.Location = new Point(351, 94);
			Asset.Margin = new Padding(3, 6, 3, 6);
			Asset.Name = "Asset";
			Asset.Size = new Size(307, 23);
			Asset.TabIndex = 25;
			Asset.SelectionChangeCommitted += Asset_SelectionChangeCommitted;
			Asset.TextChanged += Any_Changed;
			// 
			// Transfer
			// 
			Transfer.Location = new Point(444, 195);
			Transfer.Name = "Transfer";
			Transfer.Size = new Size(129, 28);
			Transfer.TabIndex = 23;
			Transfer.Text = "Transfer";
			Transfer.UseVisualStyleBackColor = true;
			Transfer.Click += Transfer_Click;
			// 
			// FromAs
			// 
			FromAs.DropDownStyle = ComboBoxStyle.DropDownList;
			FromAs.FormattingEnabled = true;
			FromAs.Location = new Point(162, 55);
			FromAs.Margin = new Padding(3, 6, 3, 6);
			FromAs.Name = "FromAs";
			FromAs.Size = new Size(94, 23);
			FromAs.TabIndex = 26;
			// 
			// ToWhat
			// 
			ToWhat.DropDownStyle = ComboBoxStyle.DropDownList;
			ToWhat.FormattingEnabled = true;
			ToWhat.Location = new Point(755, 55);
			ToWhat.Margin = new Padding(3, 6, 3, 6);
			ToWhat.Name = "ToWhat";
			ToWhat.Size = new Size(94, 23);
			ToWhat.TabIndex = 26;
			// 
			// Transactions
			// 
			Transactions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			Transactions.Columns.AddRange(new ColumnHeader[] { ChTag, ChFromClass, ChFromUser, ChToClass, ChToUser, ChSpacetime, ChEnergy, ChEnergyNext, ChStatus, ChError });
			Transactions.FullRowSelect = true;
			Transactions.Location = new Point(3, 259);
			Transactions.Margin = new Padding(3, 9, 3, 3);
			Transactions.Name = "Transactions";
			Transactions.Size = new Size(1018, 506);
			Transactions.TabIndex = 40;
			Transactions.UseCompatibleStateImageBehavior = false;
			Transactions.View = View.Details;
			// 
			// ChTag
			// 
			ChTag.Text = "Tag";
			ChTag.Width = 150;
			// 
			// ChFromClass
			// 
			ChFromClass.Text = "From Class";
			ChFromClass.TextAlign = HorizontalAlignment.Right;
			// 
			// ChFromUser
			// 
			ChFromUser.Text = "From User";
			// 
			// ChToClass
			// 
			ChToClass.Text = "To Class";
			ChToClass.TextAlign = HorizontalAlignment.Right;
			// 
			// ChToUser
			// 
			ChToUser.Text = "To User";
			// 
			// ChSpacetime
			// 
			ChSpacetime.Text = "Spacetime";
			ChSpacetime.TextAlign = HorizontalAlignment.Right;
			// 
			// ChEnergy
			// 
			ChEnergy.Text = "Energy";
			ChEnergy.TextAlign = HorizontalAlignment.Right;
			// 
			// ChEnergyNext
			// 
			ChEnergyNext.Text = "Energy Next";
			ChEnergyNext.TextAlign = HorizontalAlignment.Right;
			// 
			// ChStatus
			// 
			ChStatus.Text = "Status";
			ChStatus.TextAlign = HorizontalAlignment.Center;
			// 
			// ChError
			// 
			ChError.Text = "Error";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label3.Location = new Point(3, 235);
			label3.Margin = new Padding(0, 0, 3, 0);
			label3.Name = "label3";
			label3.Size = new Size(163, 15);
			label3.TabIndex = 33;
			label3.Text = "Recent Transfers (Updating)";
			// 
			// TransferPanel
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(Transactions);
			Controls.Add(pictureBox1);
			Controls.Add(To);
			Controls.Add(From);
			Controls.Add(Amount);
			Controls.Add(label6);
			Controls.Add(label4);
			Controls.Add(label1);
			Controls.Add(label3);
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
			Controls.Add(Transfer);
			Margin = new Padding(4, 3, 4, 3);
			Name = "TransferPanel";
			Size = new Size(1024, 768);
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
		private Button Transfer;
		private ComboBox FromAs;
		private ComboBox ToWhat;
		private ListView Transactions;
		private ColumnHeader ChStatus;
		private ColumnHeader ChFromUser;
		private ColumnHeader ChFromClass;
		private ColumnHeader ChError;
		private ColumnHeader ChToUser;
		private ColumnHeader ChToClass;
		private ColumnHeader ChSpacetime;
		private ColumnHeader ChEnergy;
		private ColumnHeader ChEnergyNext;
		private ColumnHeader ChTag;
		private Label label3;
	}
}
