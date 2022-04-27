using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UC.Net.Node.FUI
{
	public partial class AuthorPanel : MainPanel
	{
		public AuthorPanel(Dispatcher d, Vault vault) : base(d, vault)
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
				BindAuthors(AuthorSearch);
				BindAccounts(RegisrationSigner);
				BindAccounts(AuctionSigner);
				BindAuthors(TransferingAuthor);
		
				AuthorTitle_TextChanged(null, null);

				Fields.Text = null;
				Values.Text = null;
			}
		}

		private void Search_Click(object sender, EventArgs e)
		{
			Fields.Text = null;
			Values.Text = null;

			void add(string f, object v)
			{
				Fields.Text += f + "\n";
				Values.Text += v + "\n";
			}

			void sub(string f, object v)
			{
				Fields.Text += "   " + f + "\n";
				Values.Text += v + "\n";
			}

			lock(Dispatcher.Lock)
			{
				var ai = Dispatcher.GetAuthorInfo(AuthorSearch.Text, false);

				if(ai != null)
					ai.Dump(add, sub);
				else 
					Fields.Text = "Not found";
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
			
			lock(Dispatcher.Lock)
			{
				foreach(var i in FindAuthors(a))
				{
					var ar = i.LastRegistrationOperation;

					var li = new ListViewItem(ar.Author);
					li.Tag = ar;
					li.SubItems.Add(ar.Title);
					li.SubItems.Add(ar != null ? new AdmsTime(ar.Transaction.Payload.Round.Time.Ticks + ar.Years * AdmsTime.TicksPerYear).ToString(Dispatcher.DateFormat) : null);
				
					Authors.Items.Add(li);
				}
			}
		}*/

		private void AuthorTitle_TextChanged(object sender, EventArgs e)
		{
			if(AuthorTitle.Text.Length > 0)
			{
				AuthorName.Text = Operation.TitleToName(AuthorTitle.Text);
			}
		}

		private void AuthorName_TextChanged(object sender, EventArgs e)
		{
			RegistrationStatus.Text = null;

			if(AuthorName.Text.Length > AuthorRegistration.LengthMaxForAuction)
			{
				lock(Dispatcher.Lock)
				{
					Cost.Coins = AuthorRegistration.GetCost(Chain.LastConfirmedRound, (byte)Years.Value);
				}
			}
			else
				RegistrationStatus.Text = $"Author Name must be longer than {AuthorRegistration.LengthMaxForAuction} characters";
		}

		private void register_Click(object sender, EventArgs e)
		{
			try
			{
				if(!Operation.IsValid(AuthorName.Text, AuthorTitle.Text))
					throw new ArgumentException("Invalid author name");

				var a = GetPrivate(RegisrationSigner.SelectedItem as Account);

				if(a == null)
					return;

				Dispatcher.Enqueue(new AuthorRegistration(	a,
															AuthorName.Text,  
															AuthorTitle.Text,  
															(byte)Years.Value));
			}
			catch(Exception ex) when (ex is RequirementException || ex is ArgumentException)
			{
				ShowError(ex.Message);
			}
		}

		private void AuctionAuthor_TextChanged(object sender, EventArgs e)
		{
			AuctionStatus.Text = null;

			if(AuctionAuthor.Text.Length <= AuthorRegistration.LengthMaxForAuction)
			{
				var a = Chain.Authors.Find(AuctionAuthor.Text, Chain.LastConfirmedRound.Id);
				//var r = a?.FindRegistration(Chain.LastConfirmedRound);

				if(a != null && !a.IsOngoingAuction(Chain.LastConfirmedRound))
				{
					AuctionStatus.Text = $"Auction is over";
				}
				else
				{
					var l = a?.FindLastBid(Chain.LastConfirmedRound);

					Bid.Coins = l != null ? l.Bid : AuthorBid.GetMinCost(AuctionAuthor.Text);
					AuctionStatus.Text = $"Ongoing auction: " + (l != null ? $"more than {Bid.Coins}" : $"minimum {Bid.Coins}") + " UNT required";
				}
			}
			else
				AuctionStatus.Text = $"Author Name must be less than {AuthorRegistration.LengthMaxForAuction} characters"; 
		}

		private void transfer_Click(object sender, EventArgs e)
		{
			try
			{
				if(string.IsNullOrWhiteSpace(TransferingAuthor.Text))
					throw new ArgumentException("The author is not selected");

				var a = Chain.Authors.Find(TransferingAuthor.Text, int.MaxValue);

				Dispatcher.Enqueue(new AuthorTransfer(GetPrivate(a.FindOwner(Chain.LastConfirmedRound)), TransferingAuthor.Text, Account.Parse(NewOwner.Text)));
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
				var s = GetPrivate(AuctionSigner.SelectedItem as Account);

				if(s == null)
					return;

				Dispatcher.Enqueue(new AuthorBid(s, AuctionAuthor.Text, Bid.Coins));
			}
			catch(Exception ex)
			{
				ShowError(ex.Message);
			}
		}
	}
}
