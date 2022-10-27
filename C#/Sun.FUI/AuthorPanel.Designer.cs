
namespace UC.Sun.FUI
{
	partial class AuthorPanel
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
			this.transferGroup = new System.Windows.Forms.GroupBox();
			this.transfer = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.TransferingAuthor = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.NewOwner = new System.Windows.Forms.TextBox();
			this.registerGroup = new System.Windows.Forms.GroupBox();
			this.Cost = new UC.Sun.FUI.CoinEdit();
			this.Years = new System.Windows.Forms.NumericUpDown();
			this.register = new System.Windows.Forms.Button();
			this.RegistrationStatus = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.RegisrationSigner = new System.Windows.Forms.ComboBox();
			this.AuthorTitle = new System.Windows.Forms.TextBox();
			this.AuthorName = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.Bid = new UC.Sun.FUI.CoinEdit();
			this.MakeBid = new System.Windows.Forms.Button();
			this.label11 = new System.Windows.Forms.Label();
			this.AuctionStatus = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.AuctionSigner = new System.Windows.Forms.ComboBox();
			this.AuctionAuthor = new System.Windows.Forms.TextBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.Fields = new System.Windows.Forms.Label();
			this.Values = new System.Windows.Forms.Label();
			this.Search = new System.Windows.Forms.Button();
			this.namelabel = new System.Windows.Forms.Label();
			this.AuthorSearch = new System.Windows.Forms.ComboBox();
			this.transferGroup.SuspendLayout();
			this.registerGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Years)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// transferGroup
			// 
			this.transferGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.transferGroup.Controls.Add(this.transfer);
			this.transferGroup.Controls.Add(this.label6);
			this.transferGroup.Controls.Add(this.TransferingAuthor);
			this.transferGroup.Controls.Add(this.label5);
			this.transferGroup.Controls.Add(this.NewOwner);
			this.transferGroup.Location = new System.Drawing.Point(554, 577);
			this.transferGroup.Margin = new System.Windows.Forms.Padding(7);
			this.transferGroup.Name = "transferGroup";
			this.transferGroup.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.transferGroup.Size = new System.Drawing.Size(470, 191);
			this.transferGroup.TabIndex = 4;
			this.transferGroup.TabStop = false;
			this.transferGroup.Text = "Transfer Author to other Account";
			// 
			// transfer
			// 
			this.transfer.Location = new System.Drawing.Point(284, 125);
			this.transfer.Margin = new System.Windows.Forms.Padding(7, 14, 7, 7);
			this.transfer.Name = "transfer";
			this.transfer.Size = new System.Drawing.Size(166, 27);
			this.transfer.TabIndex = 9;
			this.transfer.Text = "Transfer";
			this.transfer.UseVisualStyleBackColor = true;
			this.transfer.Click += new System.EventHandler(this.transfer_Click);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label6.Location = new System.Drawing.Point(104, 46);
			this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(46, 13);
			this.label6.TabIndex = 8;
			this.label6.Text = "Author";
			// 
			// TransferingAuthor
			// 
			this.TransferingAuthor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.TransferingAuthor.FormattingEnabled = true;
			this.TransferingAuthor.Location = new System.Drawing.Point(170, 43);
			this.TransferingAuthor.Margin = new System.Windows.Forms.Padding(7);
			this.TransferingAuthor.Name = "TransferingAuthor";
			this.TransferingAuthor.Size = new System.Drawing.Size(280, 23);
			this.TransferingAuthor.TabIndex = 10;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label5.Location = new System.Drawing.Point(12, 84);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(127, 13);
			this.label5.TabIndex = 8;
			this.label5.Text = "New Owner\'s Account";
			// 
			// NewOwner
			// 
			this.NewOwner.Location = new System.Drawing.Point(170, 81);
			this.NewOwner.Margin = new System.Windows.Forms.Padding(7);
			this.NewOwner.Name = "NewOwner";
			this.NewOwner.Size = new System.Drawing.Size(280, 23);
			this.NewOwner.TabIndex = 11;
			// 
			// registerGroup
			// 
			this.registerGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.registerGroup.Controls.Add(this.Cost);
			this.registerGroup.Controls.Add(this.Years);
			this.registerGroup.Controls.Add(this.register);
			this.registerGroup.Controls.Add(this.RegistrationStatus);
			this.registerGroup.Controls.Add(this.label7);
			this.registerGroup.Controls.Add(this.label2);
			this.registerGroup.Controls.Add(this.label9);
			this.registerGroup.Controls.Add(this.label3);
			this.registerGroup.Controls.Add(this.label4);
			this.registerGroup.Controls.Add(this.RegisrationSigner);
			this.registerGroup.Controls.Add(this.AuthorTitle);
			this.registerGroup.Controls.Add(this.AuthorName);
			this.registerGroup.Location = new System.Drawing.Point(554, 54);
			this.registerGroup.Margin = new System.Windows.Forms.Padding(7);
			this.registerGroup.Name = "registerGroup";
			this.registerGroup.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.registerGroup.Size = new System.Drawing.Size(470, 291);
			this.registerGroup.TabIndex = 19;
			this.registerGroup.TabStop = false;
			this.registerGroup.Text = "Register a new Author";
			// 
			// Cost
			// 
			this.Cost.Location = new System.Drawing.Point(170, 195);
			this.Cost.Margin = new System.Windows.Forms.Padding(7);
			this.Cost.Name = "Cost";
			this.Cost.ReadOnly = true;
			this.Cost.Size = new System.Drawing.Size(116, 23);
			this.Cost.TabIndex = 13;
			this.Cost.Text = "0.000000";
			this.Cost.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// Years
			// 
			this.Years.Location = new System.Drawing.Point(170, 157);
			this.Years.Margin = new System.Windows.Forms.Padding(7);
			this.Years.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
			this.Years.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.Years.Name = "Years";
			this.Years.ReadOnly = true;
			this.Years.Size = new System.Drawing.Size(70, 23);
			this.Years.TabIndex = 12;
			this.Years.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.Years.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.Years.ValueChanged += new System.EventHandler(this.AuthorName_TextChanged);
			// 
			// register
			// 
			this.register.Location = new System.Drawing.Point(284, 241);
			this.register.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.register.Name = "register";
			this.register.Size = new System.Drawing.Size(166, 27);
			this.register.TabIndex = 9;
			this.register.Text = "Register";
			this.register.UseVisualStyleBackColor = true;
			this.register.Click += new System.EventHandler(this.register_Click);
			// 
			// RegistrationStatus
			// 
			this.RegistrationStatus.AutoSize = true;
			this.RegistrationStatus.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.RegistrationStatus.Location = new System.Drawing.Point(21, 241);
			this.RegistrationStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.RegistrationStatus.MaximumSize = new System.Drawing.Size(321, 0);
			this.RegistrationStatus.Name = "RegistrationStatus";
			this.RegistrationStatus.Size = new System.Drawing.Size(219, 26);
			this.RegistrationStatus.TabIndex = 8;
			this.RegistrationStatus.Text = "Ongoing auction: more than 0 UNT required\r\nOngoing auction: more than 0 UNT requi" +
    "red ";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label7.Location = new System.Drawing.Point(112, 159);
			this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(39, 13);
			this.label7.TabIndex = 8;
			this.label7.Text = "Years";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(44, 198);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(99, 13);
			this.label2.TabIndex = 8;
			this.label2.Text = "Total Cost (UNT)";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label9.Location = new System.Drawing.Point(71, 84);
			this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(74, 13);
			this.label9.TabIndex = 8;
			this.label9.Text = "Author Title";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label3.Location = new System.Drawing.Point(65, 122);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(81, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Author Name";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label4.Location = new System.Drawing.Point(40, 46);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(101, 13);
			this.label4.TabIndex = 8;
			this.label4.Text = "Owner\'s Account";
			// 
			// RegisrationSigner
			// 
			this.RegisrationSigner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.RegisrationSigner.FormattingEnabled = true;
			this.RegisrationSigner.Location = new System.Drawing.Point(170, 43);
			this.RegisrationSigner.Margin = new System.Windows.Forms.Padding(7);
			this.RegisrationSigner.Name = "RegisrationSigner";
			this.RegisrationSigner.Size = new System.Drawing.Size(280, 23);
			this.RegisrationSigner.TabIndex = 10;
			// 
			// AuthorTitle
			// 
			this.AuthorTitle.Location = new System.Drawing.Point(170, 81);
			this.AuthorTitle.Margin = new System.Windows.Forms.Padding(7);
			this.AuthorTitle.Name = "AuthorTitle";
			this.AuthorTitle.Size = new System.Drawing.Size(280, 23);
			this.AuthorTitle.TabIndex = 11;
			this.AuthorTitle.TextChanged += new System.EventHandler(this.AuthorTitle_TextChanged);
			// 
			// AuthorName
			// 
			this.AuthorName.Location = new System.Drawing.Point(170, 119);
			this.AuthorName.Margin = new System.Windows.Forms.Padding(7);
			this.AuthorName.Name = "AuthorName";
			this.AuthorName.ReadOnly = true;
			this.AuthorName.Size = new System.Drawing.Size(280, 23);
			this.AuthorName.TabIndex = 11;
			this.AuthorName.TextChanged += new System.EventHandler(this.AuthorName_TextChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.Bid);
			this.groupBox1.Controls.Add(this.MakeBid);
			this.groupBox1.Controls.Add(this.label11);
			this.groupBox1.Controls.Add(this.AuctionStatus);
			this.groupBox1.Controls.Add(this.label13);
			this.groupBox1.Controls.Add(this.label14);
			this.groupBox1.Controls.Add(this.AuctionSigner);
			this.groupBox1.Controls.Add(this.AuctionAuthor);
			this.groupBox1.Location = new System.Drawing.Point(554, 351);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(7);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.groupBox1.Size = new System.Drawing.Size(470, 220);
			this.groupBox1.TabIndex = 19;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Auction for Names with 4 and less Letters Long";
			// 
			// Bid
			// 
			this.Bid.Location = new System.Drawing.Point(173, 119);
			this.Bid.Margin = new System.Windows.Forms.Padding(7);
			this.Bid.Name = "Bid";
			this.Bid.Size = new System.Drawing.Size(116, 23);
			this.Bid.TabIndex = 13;
			this.Bid.Text = "0.000000";
			this.Bid.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MakeBid
			// 
			this.MakeBid.Location = new System.Drawing.Point(284, 172);
			this.MakeBid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MakeBid.Name = "MakeBid";
			this.MakeBid.Size = new System.Drawing.Size(166, 27);
			this.MakeBid.TabIndex = 9;
			this.MakeBid.Text = "Make a Bid";
			this.MakeBid.UseVisualStyleBackColor = true;
			this.MakeBid.Click += new System.EventHandler(this.MakeBid_Click);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label11.Location = new System.Drawing.Point(89, 122);
			this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(59, 13);
			this.label11.TabIndex = 8;
			this.label11.Text = "Bid (UNT)";
			// 
			// AuctionStatus
			// 
			this.AuctionStatus.AutoSize = true;
			this.AuctionStatus.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.AuctionStatus.Location = new System.Drawing.Point(21, 172);
			this.AuctionStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.AuctionStatus.MaximumSize = new System.Drawing.Size(233, 0);
			this.AuctionStatus.Name = "AuctionStatus";
			this.AuctionStatus.Size = new System.Drawing.Size(219, 26);
			this.AuctionStatus.TabIndex = 8;
			this.AuctionStatus.Text = "Ongoing auction: more than 0 UNT required Ongoing auction: more than 0 UNT requir" +
    "ed ";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label13.Location = new System.Drawing.Point(63, 84);
			this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(81, 13);
			this.label13.TabIndex = 8;
			this.label13.Text = "Author Name";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label14.Location = new System.Drawing.Point(40, 46);
			this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(101, 13);
			this.label14.TabIndex = 8;
			this.label14.Text = "Owner\'s Account";
			// 
			// AuctionSigner
			// 
			this.AuctionSigner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.AuctionSigner.FormattingEnabled = true;
			this.AuctionSigner.Location = new System.Drawing.Point(170, 43);
			this.AuctionSigner.Margin = new System.Windows.Forms.Padding(7);
			this.AuctionSigner.Name = "AuctionSigner";
			this.AuctionSigner.Size = new System.Drawing.Size(280, 23);
			this.AuctionSigner.TabIndex = 10;
			// 
			// AuctionAuthor
			// 
			this.AuctionAuthor.Location = new System.Drawing.Point(170, 81);
			this.AuctionAuthor.Margin = new System.Windows.Forms.Padding(7);
			this.AuctionAuthor.Name = "AuctionAuthor";
			this.AuctionAuthor.Size = new System.Drawing.Size(280, 23);
			this.AuctionAuthor.TabIndex = 11;
			this.AuctionAuthor.TextChanged += new System.EventHandler(this.AuctionAuthor_TextChanged);
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.Fields);
			this.groupBox2.Controls.Add(this.Values);
			this.groupBox2.Location = new System.Drawing.Point(0, 53);
			this.groupBox2.Margin = new System.Windows.Forms.Padding(7);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.groupBox2.Size = new System.Drawing.Size(540, 715);
			this.groupBox2.TabIndex = 4;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Author Info";
			// 
			// Fields
			// 
			this.Fields.AutoSize = true;
			this.Fields.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.Fields.Location = new System.Drawing.Point(19, 37);
			this.Fields.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.Fields.Name = "Fields";
			this.Fields.Padding = new System.Windows.Forms.Padding(0, 0, 19, 0);
			this.Fields.Size = new System.Drawing.Size(64, 13);
			this.Fields.TabIndex = 8;
			this.Fields.Text = "Fieds...";
			// 
			// Values
			// 
			this.Values.AutoSize = true;
			this.Values.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Values.Location = new System.Drawing.Point(195, 37);
			this.Values.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.Values.Name = "Values";
			this.Values.Size = new System.Drawing.Size(50, 13);
			this.Values.TabIndex = 8;
			this.Values.Text = "Values...";
			// 
			// Search
			// 
			this.Search.Location = new System.Drawing.Point(450, 13);
			this.Search.Margin = new System.Windows.Forms.Padding(7, 14, 7, 7);
			this.Search.Name = "Search";
			this.Search.Size = new System.Drawing.Size(166, 27);
			this.Search.TabIndex = 9;
			this.Search.Text = "Search";
			this.Search.UseVisualStyleBackColor = true;
			this.Search.Click += new System.EventHandler(this.Search_Click);
			// 
			// namelabel
			// 
			this.namelabel.AutoSize = true;
			this.namelabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.namelabel.Location = new System.Drawing.Point(19, 18);
			this.namelabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.namelabel.Name = "namelabel";
			this.namelabel.Size = new System.Drawing.Size(81, 13);
			this.namelabel.TabIndex = 8;
			this.namelabel.Text = "Author Name";
			// 
			// AuthorSearch
			// 
			this.AuthorSearch.FormattingEnabled = true;
			this.AuthorSearch.Location = new System.Drawing.Point(124, 15);
			this.AuthorSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.AuthorSearch.Name = "AuthorSearch";
			this.AuthorSearch.Size = new System.Drawing.Size(316, 23);
			this.AuthorSearch.TabIndex = 20;
			this.AuthorSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AuthorSearch_KeyDown);
			// 
			// AuthorPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.AuthorSearch);
			this.Controls.Add(this.Search);
			this.Controls.Add(this.registerGroup);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.namelabel);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.transferGroup);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "AuthorPanel";
			this.Size = new System.Drawing.Size(1024, 768);
			this.transferGroup.ResumeLayout(false);
			this.transferGroup.PerformLayout();
			this.registerGroup.ResumeLayout(false);
			this.registerGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Years)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox transferGroup;
		private System.Windows.Forms.Button transfer;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.GroupBox registerGroup;
		private System.Windows.Forms.Button register;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox RegisrationSigner;
		private System.Windows.Forms.TextBox AuthorName;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox TransferingAuthor;
		private System.Windows.Forms.TextBox NewOwner;
		private System.Windows.Forms.NumericUpDown Years;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox AuthorTitle;
		private CoinEdit Cost;
		private System.Windows.Forms.GroupBox groupBox1;
		private CoinEdit Bid;
		private System.Windows.Forms.Button MakeBid;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.ComboBox AuctionSigner;
		private System.Windows.Forms.TextBox AuctionAuthor;
		private System.Windows.Forms.Label AuctionStatus;
		private System.Windows.Forms.Label RegistrationStatus;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button Search;
		private System.Windows.Forms.Label namelabel;
		private System.Windows.Forms.Label Fields;
		private System.Windows.Forms.Label Values;
		private System.Windows.Forms.ComboBox AuthorSearch;
	}
}
