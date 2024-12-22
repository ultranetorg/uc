namespace Uccs.Net.FUI;

public partial class DashboardPanel : MainPanel
{
	Uos.Uos Uos;

	public DashboardPanel()
	{
		InitializeComponent();
	}

	public DashboardPanel(Uos.Uos uos)
	{
		InitializeComponent();

		Uos = uos;
	}

	public override void Open(bool first)
	{
		if(first)
		{
			Logbox.Log = Uos.Flow.Log;

			var m = Uos.Nodes.Select(i => i.Node).OfType<McvTcpPeering>().FirstOrDefault();
			
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
		fields.Text = "";
		values.Text = "";
	
		foreach(var i in Uos.Nodes.Select(i => i.Node).OfType<McvNode>())
		{
			//(new SummaryApc() { Limit = panel1.Height/(int)panel1.Font.Size}.Execute(Uos., null, null, null) as SummaryApc.Return).Summary
			var	m = (new McvSummaryApc() { Limit = panel1.Height/(int)panel1.Font.Size}.Execute(i, null, null, null) as McvSummaryApc.Return).Summary;

			fields.Text += Environment.NewLine + string.Join('\n', m.Select(j => j[0]));
			values.Text += Environment.NewLine + string.Join('\n', m.Select(j => j[1]));
		}

		Monitor.Invalidate();
	}
}
