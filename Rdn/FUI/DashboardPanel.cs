using System.Windows.Forms;

namespace Uccs.Rdn.FUI
{
	public partial class DashboardPanel : MainPanel
	{
		public ChainMonitor Monitor => monitor;
		public List<Mcv>	Mcvs = new();

		public DashboardPanel()
		{
			InitializeComponent();
		}

		public DashboardPanel(Net.Node d) : base(d)
		{
			InitializeComponent();
		}

		public override void Open(bool first)
		{
			if(first)
			{
				logbox.Log = Node.Flow.Log;

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
		}

		public override void PeriodicalRefresh()
		{
			var	s = (new SummaryApc() { Limit = panel1.Height/(int)panel1.Font.Size}.Execute(Node, null, null, null) as SummaryApc.Return).Summary;

			fields.Text = string.Join('\n', s.Select(j => j[0]));
			values.Text = string.Join('\n', s.Select(j => j[1]));
		
			foreach(var i in Mcvs)
			{
				var	m = (new McvSummaryApc() { Limit = panel1.Height/(int)panel1.Font.Size}.Execute(i, null, null, null) as SummaryApc.Return).Summary;

				fields.Text += Environment.NewLine + string.Join('\n', m.Select(j => j[0]));
				values.Text += Environment.NewLine + string.Join('\n', m.Select(j => j[1]));
			}

			monitor.Invalidate();
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
