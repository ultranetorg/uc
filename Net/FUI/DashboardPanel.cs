namespace Uccs.Net.FUI
{
	public partial class DashboardPanel : MainPanel
	{
		public DashboardPanel()
		{
			InitializeComponent();
		}

		public DashboardPanel(Uos.Uos d) : base(d)
		{
			InitializeComponent();
		}

		public override void Open(bool first)
		{
			if(first)
			{
				Logbox.Log = Uos.Flow.Log;

				var m = Uos.Nodes.OfType<McvNode>().FirstOrDefault();
				
				if(m?.Mcv != null)
				{
					Monitor.Mcv = m.Mcv;
					m.Mcv.VoteAdded += (b) => BeginInvoke(Monitor.Invalidate);
				}
			}
		}

		public override void Close()
		{
			base.Close();
		}

		public override void PeriodicalRefresh()
		{
			var	s = (new SummaryApc() { Limit = panel1.Height/(int)panel1.Font.Size}.Execute(Uos.Izn, null, null, null) as SummaryApc.Return).Summary;

			fields.Text = string.Join('\n', s.Select(j => j[0]));
			values.Text = string.Join('\n', s.Select(j => j[1]));
		
			foreach(var i in Uos.Nodes.OfType<McvNode>())
			{
				var	m = (new McvSummaryApc() { Limit = panel1.Height/(int)panel1.Font.Size}.Execute(i, null, null, null) as SummaryApc.Return).Summary;

				fields.Text += Environment.NewLine + string.Join('\n', m.Select(j => j[0]));
				values.Text += Environment.NewLine + string.Join('\n', m.Select(j => j[1]));
			}

			Monitor.Invalidate();
		}
	}
}
