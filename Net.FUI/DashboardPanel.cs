namespace Uccs.Net.FUI;

public partial class DashboardPanel : MainPanel
{
	McvNode Node;

	public DashboardPanel()
	{
		InitializeComponent();
	}

	public DashboardPanel(McvNode uos)
	{
		InitializeComponent();

		Node = uos;
	}

	public override void Open(bool first)
	{
		if(first)
		{
			Logbox.Log = Node.Flow.Log;

			if(Node?.Mcv != null)
			{
				Monitor.Mcv = Node.Mcv;
				Node.Mcv.VoteAdded += (b) => BeginInvoke(Monitor.Invalidate);
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
	
		var	m = (new McvSummaryApc() { Limit = panel1.Height/(int)panel1.Font.Size}.Execute(Node, null, null, null) as McvSummaryApc.Return).Summary;

		fields.Text += Environment.NewLine + string.Join('\n', m.Select(j => j[0]));
		values.Text += Environment.NewLine + string.Join('\n', m.Select(j => j[1]));

		Monitor.Invalidate();
	}
}
