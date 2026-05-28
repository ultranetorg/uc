
using Uccs.Mcv.FUI;

namespace Uccs.Nexus.Windows
{
	partial class DomainPanel
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
			Transfering = new GroupBox();
			transfer = new Button();
			label5 = new Label();
			NewOwner = new TextBox();
			Registration = new GroupBox();
			Cost = new CoinEdit();
			Years = new NumericUpDown();
			register = new Button();
			RegistrationStatus = new Label();
			label7 = new Label();
			label2 = new Label();
			label9 = new Label();
			label4 = new Label();
			RegisrationSigner = new ComboBox();
			DomainTitle = new TextBox();
			Auction = new GroupBox();
			Bid = new CoinEdit();
			MakeBid = new Button();
			label11 = new Label();
			AuctionStatus = new Label();
			label14 = new Label();
			AuctionSigner = new ComboBox();
			groupBox2 = new GroupBox();
			Fields = new Label();
			Values = new Label();
			Search = new Button();
			namelabel = new Label();
			DomainSearch = new ComboBox();
			Transfering.SuspendLayout();
			Registration.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)Years).BeginInit();
			Auction.SuspendLayout();
			groupBox2.SuspendLayout();
			SuspendLayout();
			// 
			// Transfering
			// 
			Transfering.Controls.Add(transfer);
			Transfering.Controls.Add(label5);
			Transfering.Controls.Add(NewOwner);
			Transfering.Location = new Point(630, 357);
			Transfering.Margin = new Padding(7, 7, 7, 7);
			Transfering.Name = "Transfering";
			Transfering.Padding = new Padding(4, 3, 4, 3);
			Transfering.Size = new Size(616, 161);
			Transfering.TabIndex = 4;
			Transfering.TabStop = false;
			Transfering.Text = "Transfer Domain to other Account";
			Transfering.Visible = false;
			// 
			// transfer
			// 
			transfer.Location = new Point(344, 89);
			transfer.Margin = new Padding(7, 14, 7, 7);
			transfer.Name = "transfer";
			transfer.Size = new Size(166, 27);
			transfer.TabIndex = 9;
			transfer.Text = "Transfer";
			transfer.UseVisualStyleBackColor = true;
			transfer.Click += Transfer_Click;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label5.Location = new Point(83, 53);
			label5.Margin = new Padding(4, 0, 4, 0);
			label5.Name = "label5";
			label5.Size = new Size(127, 13);
			label5.TabIndex = 8;
			label5.Text = "New Owner's Account";
			// 
			// NewOwner
			// 
			NewOwner.Location = new Point(232, 50);
			NewOwner.Margin = new Padding(7, 7, 7, 7);
			NewOwner.Name = "NewOwner";
			NewOwner.Size = new Size(280, 23);
			NewOwner.TabIndex = 11;
			// 
			// Registration
			// 
			Registration.Controls.Add(Cost);
			Registration.Controls.Add(Years);
			Registration.Controls.Add(register);
			Registration.Controls.Add(RegistrationStatus);
			Registration.Controls.Add(label7);
			Registration.Controls.Add(label2);
			Registration.Controls.Add(label9);
			Registration.Controls.Add(label4);
			Registration.Controls.Add(RegisrationSigner);
			Registration.Controls.Add(DomainTitle);
			Registration.Location = new Point(0, 351);
			Registration.Margin = new Padding(7, 7, 7, 7);
			Registration.Name = "Registration";
			Registration.Padding = new Padding(4, 3, 4, 3);
			Registration.Size = new Size(616, 291);
			Registration.TabIndex = 19;
			Registration.TabStop = false;
			Registration.Text = "Register a new Domain";
			Registration.Visible = false;
			// 
			// Cost
			// 
			Cost.Location = new Point(206, 161);
			Cost.Margin = new Padding(7, 7, 7, 7);
			Cost.Name = "Cost";
			Cost.ReadOnly = true;
			Cost.Size = new Size(116, 23);
			Cost.TabIndex = 13;
			Cost.Text = "0.000000";
			Cost.TextAlign = HorizontalAlignment.Right;
			// 
			// Years
			// 
			Years.Location = new Point(206, 123);
			Years.Margin = new Padding(7, 7, 7, 7);
			Years.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
			Years.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			Years.Name = "Years";
			Years.ReadOnly = true;
			Years.Size = new Size(70, 23);
			Years.TabIndex = 12;
			Years.TextAlign = HorizontalAlignment.Right;
			Years.Value = new decimal(new int[] { 1, 0, 0, 0 });
			Years.ValueChanged += DomainName_TextChanged;
			// 
			// register
			// 
			register.Location = new Point(320, 241);
			register.Margin = new Padding(4, 3, 4, 3);
			register.Name = "register";
			register.Size = new Size(166, 27);
			register.TabIndex = 9;
			register.Text = "Register";
			register.UseVisualStyleBackColor = true;
			register.Click += Register_Click;
			// 
			// RegistrationStatus
			// 
			RegistrationStatus.AutoSize = true;
			RegistrationStatus.Font = new Font("Tahoma", 8.25F);
			RegistrationStatus.Location = new Point(57, 241);
			RegistrationStatus.Margin = new Padding(4, 0, 4, 0);
			RegistrationStatus.MaximumSize = new Size(321, 0);
			RegistrationStatus.Name = "RegistrationStatus";
			RegistrationStatus.Size = new Size(219, 26);
			RegistrationStatus.TabIndex = 8;
			RegistrationStatus.Text = "Ongoing auction: more than 0 UNT required\r\nOngoing auction: more than 0 UNT required ";
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label7.Location = new Point(156, 126);
			label7.Margin = new Padding(4, 0, 4, 0);
			label7.Name = "label7";
			label7.Size = new Size(39, 13);
			label7.TabIndex = 8;
			label7.Text = "Years";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label2.Location = new Point(90, 165);
			label2.Margin = new Padding(4, 0, 4, 0);
			label2.Name = "label2";
			label2.Size = new Size(99, 13);
			label2.TabIndex = 8;
			label2.Text = "Total Cost (UNT)";
			// 
			// label9
			// 
			label9.AutoSize = true;
			label9.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label9.Location = new Point(118, 84);
			label9.Margin = new Padding(4, 0, 4, 0);
			label9.Name = "label9";
			label9.Size = new Size(78, 13);
			label9.TabIndex = 8;
			label9.Text = "Domain Title";
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label4.Location = new Point(90, 46);
			label4.Margin = new Padding(4, 0, 4, 0);
			label4.Name = "label4";
			label4.Size = new Size(101, 13);
			label4.TabIndex = 8;
			label4.Text = "Owner's Account";
			// 
			// RegisrationSigner
			// 
			RegisrationSigner.DropDownStyle = ComboBoxStyle.DropDownList;
			RegisrationSigner.FormattingEnabled = true;
			RegisrationSigner.Location = new Point(206, 43);
			RegisrationSigner.Margin = new Padding(7, 7, 7, 7);
			RegisrationSigner.Name = "RegisrationSigner";
			RegisrationSigner.Size = new Size(280, 23);
			RegisrationSigner.TabIndex = 10;
			// 
			// DomainTitle
			// 
			DomainTitle.Location = new Point(206, 81);
			DomainTitle.Margin = new Padding(7, 7, 7, 7);
			DomainTitle.Name = "DomainTitle";
			DomainTitle.Size = new Size(280, 23);
			DomainTitle.TabIndex = 11;
			DomainTitle.TextChanged += DomainTitle_TextChanged;
			// 
			// Auction
			// 
			Auction.Controls.Add(Bid);
			Auction.Controls.Add(MakeBid);
			Auction.Controls.Add(label11);
			Auction.Controls.Add(AuctionStatus);
			Auction.Controls.Add(label14);
			Auction.Controls.Add(AuctionSigner);
			Auction.Location = new Point(630, 56);
			Auction.Margin = new Padding(7, 7, 7, 7);
			Auction.Name = "Auction";
			Auction.Padding = new Padding(4, 3, 4, 3);
			Auction.Size = new Size(616, 231);
			Auction.TabIndex = 19;
			Auction.TabStop = false;
			Auction.Text = "Auction for Names with 4 and less Letters Long";
			Auction.Visible = false;
			// 
			// Bid
			// 
			Bid.Location = new Point(170, 86);
			Bid.Margin = new Padding(7, 7, 7, 7);
			Bid.Name = "Bid";
			Bid.Size = new Size(116, 23);
			Bid.TabIndex = 13;
			Bid.Text = "0.000000";
			Bid.TextAlign = HorizontalAlignment.Right;
			// 
			// MakeBid
			// 
			MakeBid.Location = new Point(283, 174);
			MakeBid.Margin = new Padding(4, 3, 4, 3);
			MakeBid.Name = "MakeBid";
			MakeBid.Size = new Size(166, 27);
			MakeBid.TabIndex = 9;
			MakeBid.Text = "Make a Bid";
			MakeBid.UseVisualStyleBackColor = true;
			MakeBid.Click += MakeBid_Click;
			// 
			// label11
			// 
			label11.AutoSize = true;
			label11.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label11.Location = new Point(95, 92);
			label11.Margin = new Padding(4, 0, 4, 0);
			label11.Name = "label11";
			label11.Size = new Size(59, 13);
			label11.TabIndex = 8;
			label11.Text = "Bid (UNT)";
			// 
			// AuctionStatus
			// 
			AuctionStatus.AutoSize = true;
			AuctionStatus.Font = new Font("Tahoma", 8.25F);
			AuctionStatus.Location = new Point(170, 123);
			AuctionStatus.Margin = new Padding(4, 0, 4, 0);
			AuctionStatus.MaximumSize = new Size(233, 0);
			AuctionStatus.Name = "AuctionStatus";
			AuctionStatus.Size = new Size(219, 26);
			AuctionStatus.TabIndex = 8;
			AuctionStatus.Text = "Ongoing auction: more than 0 UNT required Ongoing auction: more than 0 UNT required ";
			// 
			// label14
			// 
			label14.AutoSize = true;
			label14.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label14.Location = new Point(52, 46);
			label14.Margin = new Padding(4, 0, 4, 0);
			label14.Name = "label14";
			label14.Size = new Size(101, 13);
			label14.TabIndex = 8;
			label14.Text = "Bidder's Account";
			// 
			// AuctionSigner
			// 
			AuctionSigner.DropDownStyle = ComboBoxStyle.DropDownList;
			AuctionSigner.FormattingEnabled = true;
			AuctionSigner.Location = new Point(170, 43);
			AuctionSigner.Margin = new Padding(7, 7, 7, 7);
			AuctionSigner.Name = "AuctionSigner";
			AuctionSigner.Size = new Size(280, 23);
			AuctionSigner.TabIndex = 10;
			// 
			// groupBox2
			// 
			groupBox2.Controls.Add(Fields);
			groupBox2.Controls.Add(Values);
			groupBox2.Location = new Point(0, 56);
			groupBox2.Margin = new Padding(7, 7, 7, 7);
			groupBox2.Name = "groupBox2";
			groupBox2.Padding = new Padding(4, 3, 4, 3);
			groupBox2.Size = new Size(616, 287);
			groupBox2.TabIndex = 4;
			groupBox2.TabStop = false;
			groupBox2.Text = "Domain Info";
			// 
			// Fields
			// 
			Fields.AutoSize = true;
			Fields.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			Fields.Location = new Point(19, 37);
			Fields.Margin = new Padding(4, 0, 4, 0);
			Fields.Name = "Fields";
			Fields.Padding = new Padding(0, 0, 19, 0);
			Fields.Size = new Size(64, 13);
			Fields.TabIndex = 8;
			Fields.Text = "Fieds...";
			// 
			// Values
			// 
			Values.AutoSize = true;
			Values.Font = new Font("Tahoma", 8.25F);
			Values.Location = new Point(195, 37);
			Values.Margin = new Padding(4, 0, 4, 0);
			Values.Name = "Values";
			Values.Size = new Size(50, 13);
			Values.TabIndex = 8;
			Values.Text = "Values...";
			// 
			// Search
			// 
			Search.Location = new Point(450, 13);
			Search.Margin = new Padding(7, 14, 7, 7);
			Search.Name = "Search";
			Search.Size = new Size(166, 26);
			Search.TabIndex = 9;
			Search.Text = "Search";
			Search.UseVisualStyleBackColor = true;
			Search.Click += Search_Click;
			// 
			// namelabel
			// 
			namelabel.AutoSize = true;
			namelabel.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			namelabel.Location = new Point(19, 18);
			namelabel.Margin = new Padding(4, 0, 4, 0);
			namelabel.Name = "namelabel";
			namelabel.Size = new Size(85, 13);
			namelabel.TabIndex = 8;
			namelabel.Text = "Domain Name";
			// 
			// DomainSearch
			// 
			DomainSearch.FormattingEnabled = true;
			DomainSearch.Location = new Point(124, 15);
			DomainSearch.Margin = new Padding(4, 3, 4, 3);
			DomainSearch.Name = "DomainSearch";
			DomainSearch.Size = new Size(316, 23);
			DomainSearch.TabIndex = 20;
			DomainSearch.KeyDown += DomainSearch_KeyDown;
			// 
			// DomainPanel
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(Registration);
			Controls.Add(DomainSearch);
			Controls.Add(Search);
			Controls.Add(Auction);
			Controls.Add(namelabel);
			Controls.Add(groupBox2);
			Controls.Add(Transfering);
			Margin = new Padding(4, 3, 4, 3);
			Name = "DomainPanel";
			Size = new Size(1286, 768);
			Transfering.ResumeLayout(false);
			Transfering.PerformLayout();
			Registration.ResumeLayout(false);
			Registration.PerformLayout();
			((System.ComponentModel.ISupportInitialize)Years).EndInit();
			Auction.ResumeLayout(false);
			Auction.PerformLayout();
			groupBox2.ResumeLayout(false);
			groupBox2.PerformLayout();
			ResumeLayout(false);
			PerformLayout();

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
		private System.Windows.Forms.TextBox DomainTitle;
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
		private System.Windows.Forms.ComboBox DomainSearch;
	}
}
