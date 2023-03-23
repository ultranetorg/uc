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
using UC.Net;

namespace UC.Sun.FUI
{
	public partial class AuthorPanel : MainPanel
	{
		public AuthorPanel(Core d, Vault vault) : base(d, vault)
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

				var a = Core.Call(Role.Base, p => p.GetAuthorInfo(AuthorSearch.Text), Core.Workflow).Entry;
	
				if(a != null)
				{	
					a.ToXon(new XonTextValueSerializator()).Dump((n, t) => 
																 {
																	Fields.Text += new string(' ', t * 3) + n.Name + "\n";
																	Values.Text += (n.Value != null ? n.Serializator.Get<String>(n, n.Value) : null) + "\n";
																 });

				}
				else 
					Fields.Text = "Not found";

				var t = Core.Call(Role.Base, p => p.GetTime(), Core.Workflow);

				if(AuthorEntry.IsExclusive(AuthorSearch.Text) && AuthorBid.CanBid(a, t.Time))
				{
					if(a != null)
					{
						AuctionStatus.Text = $"Current bid is {a.LastBid}. Send higher than this amount to outbid.";
					}

					Switch(Auction);
				}
				else if(AuthorRegistration.CanRegister(AuthorSearch.Text, a, t.Time, Core.Vault.Accounts))
					Switch(Registration);
				else if(Core.Vault.Accounts.Any(i => i == a.Owner))
					Switch(Transfering);

			}
			catch(RdcException ex)
			{
				Fields.Text = "Error : " + ex.Error.ToString();
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
				AuthorSearch.Text = Operation.TitleToName(AuthorTitle.Text);
			}
		}

		private void AuthorName_TextChanged(object sender, EventArgs e)
		{
			RegistrationStatus.Text = null;

			lock(Core.Lock)
			{
				Cost.Coins = AuthorRegistration.GetCost(Database.LastConfirmedRound, (byte)Years.Value);
			}
		}

		private void Register_Click(object sender, EventArgs e)
		{
			try
			{
				if(!Operation.IsValid(AuthorSearch.Text, AuthorTitle.Text))
					throw new ArgumentException("Invalid author name");

				var a = GetPrivate(RegisrationSigner.SelectedItem as AccountAddress);

				if(a == null)
					return;

				Core.Enqueue(new AuthorRegistration(a, AuthorSearch.Text, AuthorTitle.Text, (byte)Years.Value), PlacingStage.Null, new Workflow());
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

				if(a != null && !a.IsOngoingAuction(Database.LastConfirmedRound))
				{
					AuctionStatus.Text = $"Auction is over";
				}
				else
				{
					Bid.Coins = a?.LastWinner != null ? a.LastBid : AuthorBid.GetMinCost(AuthorSearch.Text);
					AuctionStatus.Text = $"Ongoing auction: " + (a?.LastWinner != null ? $"more than {a.LastBid}" : 
																						 $"minimum {AuthorBid.GetMinCost(AuthorSearch.Text)}") + " UNT required";
				}
			}
			else
				AuctionStatus.Text = $"Author Name must be less than {AuthorEntry.ExclusiveLengthMax} characters"; 
		}

		private void Transfer_Click(object sender, EventArgs e)
		{
			try
			{
				if(string.IsNullOrWhiteSpace(AuthorSearch.Text))
					throw new ArgumentException("The author is not selected");

				var a = Database.Authors.Find(AuthorSearch.Text, int.MaxValue);

				Core.Enqueue(new AuthorTransfer(GetPrivate(a.Owner), AuthorSearch.Text, AccountAddress.Parse(NewOwner.Text)), PlacingStage.Null, new Workflow());
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

				Core.Enqueue(new AuthorBid(s, AuthorSearch.Text, Bid.Coins), PlacingStage.Null, new Workflow());
			}
			catch(Exception ex)
			{
				ShowError(ex.Message);
			}
		}
	}
}
