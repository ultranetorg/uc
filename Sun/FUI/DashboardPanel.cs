using System.Windows.Forms;

namespace Uccs.Sun.FUI
{
	public partial class DashboardPanel : MainPanel
	{

		public DashboardPanel()
		{
			InitializeComponent();
		}

		public DashboardPanel(Net.Sun d) : base(d)
		{
			InitializeComponent();

			monitor.Mcv	= Mcv;

			//d.MainStarted += c =>	{
			//							if(Core.Database != null && !Core.Database.BlockAdded.GetInvocationList().Any(i => i == monitor.OnBlockAdded))
			//								Core.Database.BlockAdded +=  monitor.OnBlockAdded;
			//						};
		}

		public override void Open(bool first)
		{
			//if(Core.Database != null && !Core.Database.BlockAdded.GetInvocationList().Any(i => i == monitor.OnBlockAdded))
			//	Core.Database.BlockAdded +=  monitor.OnBlockAdded;

			if(first)
			{
				logbox.Log = Sun.Flow.Log;

				BindAccounts(source);
				BindAccounts(destination);

				if(destination.Items.Count > 1)
					destination.SelectedIndex = 1;

				if(Mcv != null)
				{
					Mcv.VoteAdded += (b) => BeginInvoke(monitor.Invalidate);
				}
			}
		}

		public override void Close()
		{
			base.Close();

			//if(Core.Database != null)
			//	Core.Database.BlockAdded -= monitor.OnBlockAdded;
		}

		public override void PeriodicalRefresh()
		{
			var	s = (new SummaryApc() { Limit = panel1.Height/(int)panel1.Font.Size}.Execute(Sun, null, null, null) as SummaryApc.Return).Summary;

			fields.Text = string.Join('\n', s.Select(j => j[0]));
			values.Text = string.Join('\n', s.Select(j => j[1]));
		
			 //monitor.OnBlockAdded(null);
		}

		private void all_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if(source.SelectedItem is AccountAddress a)
			{
				amount.Coins = Mcv.Accounts.Find(a, Mcv.LastConfirmedRound.Id).Balance;
			}
		}

		private void source_SelectionChangeCommitted(object sender, EventArgs e)
		{
			//amount.Coins = Core.Database.GetConfirmedBalance(source.SelectedItem as Account);
		}

		private void send_Click(object sender, EventArgs e)
		{
			var signer = GetPrivate(source.SelectedItem as AccountAddress);

			if(signer != null)
			{
				try
				{
					Mcv.Transact(new UntTransfer(AccountAddress.Parse(destination.Text), amount.Coins), signer, TransactionStatus.None, new Flow("UntTransfer"));
				}
				catch(RequirementException ex)
				{
					MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
	}
}
