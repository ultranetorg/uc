using System.Windows.Forms;

namespace Uccs.Sun.FUI
{
	public partial class DomainPanel : MainPanel
	{
		public DomainPanel(Mcv mcv) : base(mcv)
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
		
				DomainTitle_TextChanged(null, null);

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

				var a = Mcv.Call(() => new DomainRequest(DomainSearch.Text), Sun.Flow).Domain;
	
				if(a != null)
				{	
					Dump(a, Fields, Values);
				}
				else 
					Fields.Text = "Not found";

				var t = Mcv.Call(() => new TimeRequest(), Sun.Flow);

				if(Domain.IsWeb(DomainSearch.Text) && (a == null || Domain.CanBid(a, t.Time)))
				{
					if(a != null)
						AuctionStatus.Text = $"Current bid is {a.LastBid}. Send higher than this amount to outbid.";
					else
						AuctionStatus.Text = "Start the auction by sending first bid";

					Switch(Auction);
				}
				else if(Sun.Vault.Wallets.Keys.Any(i => Domain.CanRegister(DomainSearch.Text, a, t.Time, i)))
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
				DomainTitle.Text = DomainSearch.Text;
				DomainName_TextChanged(null, EventArgs.Empty);
				group.Visible = true;
			}
			else if(group == Transfering)
			{ 
				group.Visible = true;
			}
		}

		private void DomainSearch_KeyDown(object sender, KeyEventArgs e)
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
				a = Account.Parse(DomainSearch.Text.ToString());
			}
			catch(Exception ex)
			{
				ShowError(ex.Message);
				return;
			}

			Domains.Items.Clear();
			
			lock(Core.Lock)
			{
				foreach(var i in FindDomains(a))
				{
					var ar = i.LastRegistrationOperation;

					var li = new ListViewItem(ar.Domain);
					li.Tag = ar;
					li.SubItems.Add(ar.Title);
					li.SubItems.Add(ar != null ? new AdmsTime(ar.Transaction.Payload.Round.Time.Ticks + ar.Years * AdmsTime.TicksPerYear).ToString(Core.DateFormat) : null);
				
					Domains.Items.Add(li);
				}
			}
		}*/

		private void DomainTitle_TextChanged(object sender, EventArgs e)
		{
			if(DomainTitle.Text.Length > 0)
			{
				///DomainSearch.Text = Operation.TitleToName(DomainTitle.Text);
			}
		}

		private void DomainName_TextChanged(object sender, EventArgs e)
		{
			RegistrationStatus.Text = null;

			lock(Sun.Lock)
			{
				//Cost.Coins = DomainRegistration.GetCost(Database.LastConfirmedRound.Factor, (byte)Years.Value);
			}
		}

		private void Register_Click(object sender, EventArgs e)
		{
			//try
			//{
			//	if(!Domain.Valid(DomainSearch.Text))
			//		throw new ArgumentException("Invalid domain name");
			//
			//	var a = GetPrivate(RegisrationSigner.SelectedItem as AccountAddress);
			//
			//	Sun.Enqueue(new DomainRegistration(DomainSearch.Text, (byte)Years.Value), a, TransactionStatus.None, new Workflow("DomainRegistration"));
			//}
			//catch(Exception ex) when (ex is RequirementException || ex is ArgumentException)
			//{
			//	ShowError(ex.Message);
			//}

			throw new NotImplementedException();
		}

		private void AuctionDomain_TextChanged(object sender, EventArgs e)
		{
// 			AuctionStatus.Text = null;
// 
// 			if(Domain.IsWeb(DomainSearch.Text))
// 			{
// 				var a = Database.Domains.Find(DomainSearch.Text, Database.LastConfirmedRound.Id);
// 				//var r = a?.FindRegistration(Chain.LastConfirmedRound);
// 
// 				if(a != null && !Domain.CanBid(a, Database.LastConfirmedRound.ConsensusTime))
// 				{
// 					AuctionStatus.Text = $"Auction is over";
// 				}
// 				else
// 				{
// 					Bid.Coins = a.LastBid;
// 					AuctionStatus.Text = $"Ongoing auction:  more than {a.LastBid} UNT required";
// 				}
// 			}
// 			//else
// 			//	AuctionStatus.Text = $"Domain name must be less than {DomainEntry.ExclusiveLengthMax} characters"; 

			throw new NotImplementedException();
		}

		private void Transfer_Click(object sender, EventArgs e)
		{
// 			try
// 			{
// 				if(string.IsNullOrWhiteSpace(DomainSearch.Text))
// 					throw new ArgumentException("The domain is not selected");
// 
// 				var a = Database.Domains.Find(DomainSearch.Text, int.MaxValue);
// 
// 				//Sun.Enqueue(new DomainTransfer(DomainSearch.Text, AccountAddress.Parse(NewOwner.Text)), GetPrivate(a.Owner), TransactionStatus.None, new Workflow("Transfer_Click"));
// 			}
// 			catch(Exception ex) when (ex is RequirementException || ex is FormatException || ex is ArgumentException)
// 			{
// 				ShowError(ex.Message);
// 			}
			throw new NotImplementedException();
		}

		private void MakeBid_Click(object sender, EventArgs e)
		{
			try
			{
				var s = GetPrivate(AuctionSigner.SelectedItem as AccountAddress);

				if(s == null)
					return;

				Rds.Transact(new DomainBid(null, Bid.Coins), s, TransactionStatus.None, new Flow("MakeBid_Click"));
			}
			catch(Exception ex)
			{
				ShowError(ex.Message);
			}
		}
	}
}
