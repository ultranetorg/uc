using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Uccs.Net;

namespace Uccs.Sun.FUI
{
	public partial class AuthorPanel : MainPanel
	{
		public AuthorPanel(Net.Sun d, Vault vault) : base(d, vault)
		{
			InitializeComponent();

			Years.Maximum = 10;
			AuctionStatus.Text = null;
			RegistrationStatus.Text = null;
		}

		public override void Open(bool first)
		{
			if(first)
			{
				BindAccounts(RegisrationSigner);
				BindAccounts(AuctionSigner);
		
				AuthorTitle_TextChanged(null, null);

				Fields.Text = null;
				Values.Text = null;
			}
		}

		private void Search_Click(object sender, EventArgs e)
		{
			Fields.Text = null;
			Values.Text = null;

			try
			{
				Registration.Visible	= false;
				Auction.Visible			= false;
				Transfering.Visible		= false;

				var a = Sun.Call(p => p.GetAuthorInfo(AuthorSearch.Text), Sun.Workflow).Author;
	
				if(a != null)
				{	
					Dump(a, Fields, Values);
				}
				else 
					Fields.Text = "Not found";

				var t = Sun.Call(p => p.GetTime(), Sun.Workflow);

				if(Author.IsExclusive(AuthorSearch.Text) && (a == null || Author.CanBid(a.Name, a, t.Time)))
				{
					if(a != null)
						AuctionStatus.Text = $"Current bid is {a.LastBid}. Send higher than this amount to outbid.";
					else
						AuctionStatus.Text = "Start the auction by sending first bid";

					Switch(Auction);
				}
				else if(Sun.Vault.Wallets.Keys.Any(i => Author.CanRegister(AuthorSearch.Text, a, t.Time, i)))
					Switch(Registration);
				else if(Sun.Vault.Wallets.Keys.Any(i => i == a.Owner))
					Switch(Transfering);

			}
			catch(Exception ex)
			{
				Fields.Text = "Error : " + ex.Message;
			}
		}

		void Switch(GroupBox group)
		{
			group.Location = Registration.Location;
			
			if(group == Auction)
			{
				group.Visible = true;
			}
			else if(group == Registration)
			{
				AuthorTitle.Text = AuthorSearch.Text;
				AuthorName_TextChanged(null, EventArgs.Empty);
				group.Visible = true;
			}
			else if(group == Transfering)
			{ 
				group.Visible = true;
			}
		}

		private void AuthorSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Enter)
				Search_Click(sender, e);
		}

/*
		private void search_Click(object sender, EventArgs e)
		{
			Account a = null;

			try
			{
				a = Account.Parse(AuthorSearch.Text.ToString());
			}
			catch(Exception ex)
			{
				ShowError(ex.Message);
				return;
			}

			Authors.Items.Clear();
			
			lock(Core.Lock)
			{
				foreach(var i in FindAuthors(a))
				{
					var ar = i.LastRegistrationOperation;

					var li = new ListViewItem(ar.Author);
					li.Tag = ar;
					li.SubItems.Add(ar.Title);
					li.SubItems.Add(ar != null ? new AdmsTime(ar.Transaction.Payload.Round.Time.Ticks + ar.Years * AdmsTime.TicksPerYear).ToString(Core.DateFormat) : null);
				
					Authors.Items.Add(li);
				}
			}
		}*/

		private void AuthorTitle_TextChanged(object sender, EventArgs e)
		{
			if(AuthorTitle.Text.Length > 0)
			{
				///AuthorSearch.Text = Operation.TitleToName(AuthorTitle.Text);
			}
		}

		private void AuthorName_TextChanged(object sender, EventArgs e)
		{
			RegistrationStatus.Text = null;

			lock(Sun.Lock)
			{
				//Cost.Coins = AuthorRegistration.GetCost(Database.LastConfirmedRound.Factor, (byte)Years.Value);
			}
		}

		private void Register_Click(object sender, EventArgs e)
		{
			try
			{
				if(!Author.Valid(AuthorSearch.Text))
					throw new ArgumentException("Invalid author name");

				var a = GetPrivate(RegisrationSigner.SelectedItem as AccountAddress);

				Sun.Enqueue(new AuthorRegistration(AuthorSearch.Text, AuthorTitle.Text, (byte)Years.Value), a, PlacingStage.Null, new Workflow("AuthorRegistration"));
			}
			catch(Exception ex) when (ex is RequirementException || ex is ArgumentException)
			{
				ShowError(ex.Message);
			}
		}

		private void AuctionAuthor_TextChanged(object sender, EventArgs e)
		{
			AuctionStatus.Text = null;

			if(AuthorEntry.IsExclusive(AuthorSearch.Text))
			{
				var a = Database.Authors.Find(AuthorSearch.Text, Database.LastConfirmedRound.Id);
				//var r = a?.FindRegistration(Chain.LastConfirmedRound);

				if(a != null && !Author.CanBid(a.Name, a, Database.LastConfirmedRound.ConfirmedTime))
				{
					AuctionStatus.Text = $"Auction is over";
				}
				else
				{
					Bid.Coins = a.LastBid;
					AuctionStatus.Text = $"Ongoing auction:  more than {a.LastBid} UNT required";
				}
			}
			//else
			//	AuctionStatus.Text = $"Author name must be less than {AuthorEntry.ExclusiveLengthMax} characters"; 
		}

		private void Transfer_Click(object sender, EventArgs e)
		{
			try
			{
				if(string.IsNullOrWhiteSpace(AuthorSearch.Text))
					throw new ArgumentException("The author is not selected");

				var a = Database.Authors.Find(AuthorSearch.Text, int.MaxValue);

				Sun.Enqueue(new AuthorTransfer(AuthorSearch.Text, AccountAddress.Parse(NewOwner.Text)), GetPrivate(a.Owner), PlacingStage.Null, new Workflow("Transfer_Click"));
			}
			catch(Exception ex) when (ex is RequirementException || ex is FormatException || ex is ArgumentException)
			{
				ShowError(ex.Message);
			}
		}

		private void MakeBid_Click(object sender, EventArgs e)
		{
			try
			{
				var s = GetPrivate(AuctionSigner.SelectedItem as AccountAddress);

				if(s == null)
					return;

				Sun.Enqueue(new AuthorBid(AuthorSearch.Text, null, Bid.Coins), s, PlacingStage.Null, new Workflow("MakeBid_Click"));
			}
			catch(Exception ex)
			{
				ShowError(ex.Message);
			}
		}
	}
}
