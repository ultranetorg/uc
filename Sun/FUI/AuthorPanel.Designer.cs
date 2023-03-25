
namespace Uccs.Sun.FUI
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
			this.Transfering = new System.Windows.Forms.GroupBox();
			this.transfer = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.NewOwner = new System.Windows.Forms.TextBox();
			this.Registration = new System.Windows.Forms.GroupBox();
			this.Cost = new Uccs.Sun.FUI.CoinEdit();
			this.Years = new System.Windows.Forms.NumericUpDown();
			this.register = new System.Windows.Forms.Button();
			this.RegistrationStatus = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.RegisrationSigner = new System.Windows.Forms.ComboBox();
			this.AuthorTitle = new System.Windows.Forms.TextBox();
			this.Auction = new System.Windows.Forms.GroupBox();
			this.Bid = new Uccs.Sun.FUI.CoinEdit();
			this.MakeBid = new System.Windows.Forms.Button();
			this.label11 = new System.Windows.Forms.Label();
			this.AuctionStatus = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.AuctionSigner = new System.Windows.Forms.ComboBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.Fields = new System.Windows.Forms.Label();
			this.Values = new System.Windows.Forms.Label();
			this.Search = new System.Windows.Forms.Button();
			this.namelabel = new System.Windows.Forms.Label();
			this.AuthorSearch = new System.Windows.Forms.ComboBox();
			this.Transfering.SuspendLayout();
			this.Registration.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Years)).BeginInit();
			this.Auction.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// Transfering
			// 
			this.Transfering.Controls.Add(this.transfer);
			this.Transfering.Controls.Add(this.label5);
			this.Transfering.Controls.Add(this.NewOwner);
			this.Transfering.Location = new System.Drawing.Point(1170, 761);
			this.Transfering.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Transfering.Name = "Transfering";
			this.Transfering.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Transfering.Size = new System.Drawing.Size(1144, 344);
			this.Transfering.TabIndex = 4;
			this.Transfering.TabStop = false;
			this.Transfering.Text = "Transfer Author to other Account";
			this.Transfering.Visible = false;
			// 
			// transfer
			// 
			this.transfer.Location = new System.Drawing.Point(639, 190);
			this.transfer.Margin = new System.Windows.Forms.Padding(13, 30, 13, 15);
			this.transfer.Name = "transfer";
			this.transfer.Size = new System.Drawing.Size(308, 58);
			this.transfer.TabIndex = 9;
			this.transfer.Text = "Transfer";
			this.transfer.UseVisualStyleBackColor = true;
			this.transfer.Click += new System.EventHandler(this.Transfer_Click);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label5.Location = new System.Drawing.Point(155, 113);
			this.label5.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(255, 27);
			this.label5.TabIndex = 8;
			this.label5.Text = "New Owner\'s Account";
			// 
			// NewOwner
			// 
			this.NewOwner.Location = new System.Drawing.Point(430, 106);
			this.NewOwner.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.NewOwner.Name = "NewOwner";
			this.NewOwner.Size = new System.Drawing.Size(517, 39);
			this.NewOwner.TabIndex = 11;
			// 
			// Registration
			// 
			this.Registration.Controls.Add(this.Cost);
			this.Registration.Controls.Add(this.Years);
			this.Registration.Controls.Add(this.register);
			this.Registration.Controls.Add(this.RegistrationStatus);
			this.Registration.Controls.Add(this.label7);
			this.Registration.Controls.Add(this.label2);
			this.Registration.Controls.Add(this.label9);
			this.Registration.Controls.Add(this.label4);
			this.Registration.Controls.Add(this.RegisrationSigner);
			this.Registration.Controls.Add(this.AuthorTitle);
			this.Registration.Location = new System.Drawing.Point(0, 749);
			this.Registration.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Registration.Name = "Registration";
			this.Registration.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Registration.Size = new System.Drawing.Size(1144, 621);
			this.Registration.TabIndex = 19;
			this.Registration.TabStop = false;
			this.Registration.Text = "Register a new Author";
			this.Registration.Visible = false;
			// 
			// Cost
			// 
			this.Cost.Location = new System.Drawing.Point(383, 344);
			this.Cost.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Cost.Name = "Cost";
			this.Cost.ReadOnly = true;
			this.Cost.Size = new System.Drawing.Size(212, 39);
			this.Cost.TabIndex = 13;
			this.Cost.Text = "0.000000";
			this.Cost.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// Years
			// 
			this.Years.Location = new System.Drawing.Point(383, 263);
			this.Years.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
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
			this.Years.Size = new System.Drawing.Size(130, 39);
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
			this.register.Location = new System.Drawing.Point(594, 514);
			this.register.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.register.Name = "register";
			this.register.Size = new System.Drawing.Size(308, 58);
			this.register.TabIndex = 9;
			this.register.Text = "Register";
			this.register.UseVisualStyleBackColor = true;
			this.register.Click += new System.EventHandler(this.Register_Click);
			// 
			// RegistrationStatus
			// 
			this.RegistrationStatus.AutoSize = true;
			this.RegistrationStatus.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.RegistrationStatus.Location = new System.Drawing.Point(106, 514);
			this.RegistrationStatus.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.RegistrationStatus.MaximumSize = new System.Drawing.Size(596, 0);
			this.RegistrationStatus.Name = "RegistrationStatus";
			this.RegistrationStatus.Size = new System.Drawing.Size(448, 54);
			this.RegistrationStatus.TabIndex = 8;
			this.RegistrationStatus.Text = "Ongoing auction: more than 0 UNT required\r\nOngoing auction: more than 0 UNT requi" +
    "red ";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label7.Location = new System.Drawing.Point(289, 269);
			this.label7.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(74, 27);
			this.label7.TabIndex = 8;
			this.label7.Text = "Years";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(168, 351);
			this.label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(195, 27);
			this.label2.TabIndex = 8;
			this.label2.Text = "Total Cost (UNT)";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label9.Location = new System.Drawing.Point(220, 180);
			this.label9.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(143, 27);
			this.label9.TabIndex = 8;
			this.label9.Text = "Author Title";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label4.Location = new System.Drawing.Point(168, 99);
			this.label4.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(199, 27);
			this.label4.TabIndex = 8;
			this.label4.Text = "Owner\'s Account";
			// 
			// RegisrationSigner
			// 
			this.RegisrationSigner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.RegisrationSigner.FormattingEnabled = true;
			this.RegisrationSigner.Location = new System.Drawing.Point(383, 92);
			this.RegisrationSigner.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.RegisrationSigner.Name = "RegisrationSigner";
			this.RegisrationSigner.Size = new System.Drawing.Size(517, 40);
			this.RegisrationSigner.TabIndex = 10;
			// 
			// AuthorTitle
			// 
			this.AuthorTitle.Location = new System.Drawing.Point(383, 173);
			this.AuthorTitle.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.AuthorTitle.Name = "AuthorTitle";
			this.AuthorTitle.Size = new System.Drawing.Size(517, 39);
			this.AuthorTitle.TabIndex = 11;
			this.AuthorTitle.TextChanged += new System.EventHandler(this.AuthorTitle_TextChanged);
			// 
			// Auction
			// 
			this.Auction.Controls.Add(this.Bid);
			this.Auction.Controls.Add(this.MakeBid);
			this.Auction.Controls.Add(this.label11);
			this.Auction.Controls.Add(this.AuctionStatus);
			this.Auction.Controls.Add(this.label14);
			this.Auction.Controls.Add(this.AuctionSigner);
			this.Auction.Location = new System.Drawing.Point(1170, 119);
			this.Auction.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Auction.Name = "Auction";
			this.Auction.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Auction.Size = new System.Drawing.Size(1144, 493);
			this.Auction.TabIndex = 19;
			this.Auction.TabStop = false;
			this.Auction.Text = "Auction for Names with 4 and less Letters Long";
			this.Auction.Visible = false;
			// 
			// Bid
			// 
			this.Bid.Location = new System.Drawing.Point(316, 184);
			this.Bid.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Bid.Name = "Bid";
			this.Bid.Size = new System.Drawing.Size(212, 39);
			this.Bid.TabIndex = 13;
			this.Bid.Text = "0.000000";
			this.Bid.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MakeBid
			// 
			this.MakeBid.Location = new System.Drawing.Point(525, 371);
			this.MakeBid.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.MakeBid.Name = "MakeBid";
			this.MakeBid.Size = new System.Drawing.Size(308, 58);
			this.MakeBid.TabIndex = 9;
			this.MakeBid.Text = "Make a Bid";
			this.MakeBid.UseVisualStyleBackColor = true;
			this.MakeBid.Click += new System.EventHandler(this.MakeBid_Click);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label11.Location = new System.Drawing.Point(176, 196);
			this.label11.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(120, 27);
			this.label11.TabIndex = 8;
			this.label11.Text = "Bid (UNT)";
			// 
			// AuctionStatus
			// 
			this.AuctionStatus.AutoSize = true;
			this.AuctionStatus.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.AuctionStatus.Location = new System.Drawing.Point(316, 263);
			this.AuctionStatus.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.AuctionStatus.MaximumSize = new System.Drawing.Size(433, 0);
			this.AuctionStatus.Name = "AuctionStatus";
			this.AuctionStatus.Size = new System.Drawing.Size(399, 81);
			this.AuctionStatus.TabIndex = 8;
			this.AuctionStatus.Text = "Ongoing auction: more than 0 UNT required Ongoing auction: more than 0 UNT requir" +
    "ed ";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label14.Location = new System.Drawing.Point(97, 99);
			this.label14.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(198, 27);
			this.label14.TabIndex = 8;
			this.label14.Text = "Bidder\'s Account";
			// 
			// AuctionSigner
			// 
			this.AuctionSigner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.AuctionSigner.FormattingEnabled = true;
			this.AuctionSigner.Location = new System.Drawing.Point(316, 92);
			this.AuctionSigner.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.AuctionSigner.Name = "AuctionSigner";
			this.AuctionSigner.Size = new System.Drawing.Size(517, 40);
			this.AuctionSigner.TabIndex = 10;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.Fields);
			this.groupBox2.Controls.Add(this.Values);
			this.groupBox2.Location = new System.Drawing.Point(0, 119);
			this.groupBox2.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.groupBox2.Size = new System.Drawing.Size(1144, 613);
			this.groupBox2.TabIndex = 4;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Author Info";
			// 
			// Fields
			// 
			this.Fields.AutoSize = true;
			this.Fields.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.Fields.Location = new System.Drawing.Point(35, 79);
			this.Fields.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.Fields.Name = "Fields";
			this.Fields.Padding = new System.Windows.Forms.Padding(0, 0, 35, 0);
			this.Fields.Size = new System.Drawing.Size(126, 27);
			this.Fields.TabIndex = 8;
			this.Fields.Text = "Fieds...";
			// 
			// Values
			// 
			this.Values.AutoSize = true;
			this.Values.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Values.Location = new System.Drawing.Point(362, 79);
			this.Values.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.Values.Name = "Values";
			this.Values.Size = new System.Drawing.Size(97, 27);
			this.Values.TabIndex = 8;
			this.Values.Text = "Values...";
			// 
			// Search
			// 
			this.Search.Location = new System.Drawing.Point(836, 28);
			this.Search.Margin = new System.Windows.Forms.Padding(13, 30, 13, 15);
			this.Search.Name = "Search";
			this.Search.Size = new System.Drawing.Size(308, 58);
			this.Search.TabIndex = 9;
			this.Search.Text = "Search";
			this.Search.UseVisualStyleBackColor = true;
			this.Search.Click += new System.EventHandler(this.Search_Click);
			// 
			// namelabel
			// 
			this.namelabel.AutoSize = true;
			this.namelabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.namelabel.Location = new System.Drawing.Point(35, 38);
			this.namelabel.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.namelabel.Name = "namelabel";
			this.namelabel.Size = new System.Drawing.Size(158, 27);
			this.namelabel.TabIndex = 8;
			this.namelabel.Text = "Author Name";
			// 
			// AuthorSearch
			// 
			this.AuthorSearch.FormattingEnabled = true;
			this.AuthorSearch.Location = new System.Drawing.Point(230, 32);
			this.AuthorSearch.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.AuthorSearch.Name = "AuthorSearch";
			this.AuthorSearch.Size = new System.Drawing.Size(583, 40);
			this.AuthorSearch.TabIndex = 20;
			this.AuthorSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AuthorSearch_KeyDown);
			// 
			// AuthorPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Registration);
			this.Controls.Add(this.AuthorSearch);
			this.Controls.Add(this.Search);
			this.Controls.Add(this.Auction);
			this.Controls.Add(this.namelabel);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.Transfering);
			this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Name = "AuthorPanel";
			this.Size = new System.Drawing.Size(2388, 1638);
			this.Transfering.ResumeLayout(false);
			this.Transfering.PerformLayout();
			this.Registration.ResumeLayout(false);
			this.Registration.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Years)).EndInit();
			this.Auction.ResumeLayout(false);
			this.Auction.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox Transfering;
		private System.Windows.Forms.Button transfer;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.GroupBox Registration;
		private System.Windows.Forms.Button register;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox RegisrationSigner;
		private System.Windows.Forms.TextBox NewOwner;
		private System.Windows.Forms.NumericUpDown Years;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox AuthorTitle;
		private CoinEdit Cost;
		private System.Windows.Forms.GroupBox Auction;
		private CoinEdit Bid;
		private System.Windows.Forms.Button MakeBid;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.ComboBox AuctionSigner;
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
