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
				var a = Core.Connect(Role.Base, null, Core.Workflow).GetAuthorInfo(AuthorSearch.Text).Entry;

				Registration.Visible	= false;
				Auction.Visible			= false;
				Transfering.Visible		= false;
	
				if(a != null)
				{	
					a.ToXon(new XonTextValueSerializator()).Dump((n, t) => 
																 {
																	Fields.Text += new string(' ', t * 3) + n.Name + "\n";
																	Values.Text += (n.Value != null ? n.Serializator.Get<String>(n, n.Value) : null) + "\n";
																 });

					if(a.Owner == null)
					{ 
						if(AuthorEntry.IsExclusive(AuthorSearch.Text))
						{
							Switch(Auction);
						} 
						else
						{
							Switch(Registration);
						}
					}
					else if(Core.Vault.Accounts.Any(i => i == a.Owner))
					{
						Switch(Transfering);
					}
				}
				else 
				{
					Fields.Text = "Not found";

					if(AuthorEntry.IsExclusive(AuthorSearch.Text))
						Switch(Auction);
					else
						Switch(Registration);
				}
			}
			catch(Exception ex) when(!Debugger.IsAttached)
			{
				ShowException("Falied", ex);
			}
		}

		void Switch(GroupBox group)
		{
					//= 

			group.Location = Registration.Location;
			group.Visible = true;
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

			if(!AuthorEntry.IsExclusive(AuthorSearch.Text))
			{
				lock(Core.Lock)
				{
					Cost.Coins = AuthorRegistration.GetCost(Database.LastConfirmedRound, (byte)Years.Value);
				}
			}
			else
				RegistrationStatus.Text = $"Author Name must be longer than {AuthorEntry.LengthMaxForAuction} characters";
		}

		private void register_Click(object sender, EventArgs e)
		{
			try
			{
				if(!Operation.IsValid(AuthorSearch.Text, AuthorTitle.Text))
					throw new ArgumentException("Invalid author name");

				var a = GetPrivate(RegisrationSigner.SelectedItem as Account);

				if(a == null)
					return;

				Core.Enqueue(new AuthorRegistration(a, AuthorSearch.Text, AuthorTitle.Text, (byte)Years.Value), PlacingStage.Null, null);
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
				AuctionStatus.Text = $"Author Name must be less than {AuthorEntry.LengthMaxForAuction} characters"; 
		}

		private void transfer_Click(object sender, EventArgs e)
		{
			try
			{
				if(string.IsNullOrWhiteSpace(AuthorSearch.Text))
					throw new ArgumentException("The author is not selected");

				var a = Database.Authors.Find(AuthorSearch.Text, int.MaxValue);

				Core.Enqueue(new AuthorTransfer(GetPrivate(a.Owner), AuthorSearch.Text, Account.Parse(NewOwner.Text)), PlacingStage.Null, null);
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
				var log = new Log();
				log.Report(this, "Initiated");

				var s = GetPrivate(AuctionSigner.SelectedItem as Account);

				if(s == null)
					return;

				var v = new Workflow(log);

				var f = new FlowControlForm(Core, v);
				f.StartPosition = FormStartPosition.CenterParent;
				f.Show(ParentForm);

				Core.Enqueue(new AuthorBid(s, AuthorSearch.Text, Bid.Coins), PlacingStage.Null, v);
			}
			catch(Exception ex)
			{
				ShowError(ex.Message);
			}
		}
	}
}
