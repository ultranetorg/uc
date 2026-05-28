using System.Windows.Forms;

namespace Uccs.Mcv.FUI;

public partial class McvForm : Form
{
	public readonly McvNode			Node;
	System.Windows.Forms.Timer		Timer = new ();

	DashboardPanel				Dashboard;

	public McvForm(McvNode node)
	{
		InitializeComponent();

		Node = node;
		MinimumSize = Size;

		var dashboard = new TreeNode("Dashboard"){Tag = Dashboard = new DashboardPanel(Node)};
		Navigator.Nodes.Add(dashboard);

		Navigator.SelectedNode = dashboard;
		Navigator.ExpandAll();
		
		LoadNode(Node);
	}

	protected virtual void LoadNode(McvNode node)
	{
		Dashboard.Monitor.Mcv = node.Mcv;

		var root = Navigator;

		var tf = new TreeNode("Transfer"){Tag = new TransferPanel(node)};
		root.Nodes.Add(tf);

		var net = new TreeNode("Network"){Tag = new NetworkPanel(node.Peering)};
		root.Nodes.Add(net);

		var t = new TreeNode("Transactions"){Tag = new TransactionsPanel(node)};
		root.Nodes.Add(t);

		if(node.Mcv?.Settings.Chain != null)
		{
			var c = new TreeNode("Chain"){Tag = new ChainPanel(node)};
			root.Nodes.Add(c);
		}

		root.ExpandAll();
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		Timer.Tick += RefreshInfo;
		Timer.Interval = 1000;
		Timer.Start();
	}

	public void BeginClose()
	{
		BeginInvoke(Close);
	}

	private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		foreach(Control i in Controls)
			if(i is McvPanel j)
			{
				j.Close();
			}

		Timer.Stop();
		Timer.Tick -= RefreshInfo;
	}

	void RefreshInfo(object myObject, EventArgs myEventArgs)
	{
		//lock(Uos.Lock)
		{
			Text = $"{Node}";
		}

		foreach(var i in Controls)
			if(i is McvPanel p)
				p.PeriodicalRefresh();
	}

	private void navigator_AfterSelect(object sender, TreeViewEventArgs e)
	{
		var p = e.Node.Tag as McvPanel;

		foreach(Control i in Controls)
			if(i is McvPanel j)
			{
				j.Close();
				Controls.Remove(i);
			}

		if(p != null)
		{
			p.Location	= place.Location;
			p.Size		= place.Size;
			p.Anchor	= AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right;
	
			Controls.Add(p);
			Controls.SetChildIndex(p, 0);

			p.Open(p.First);

			p.First = false;
		}
	}
}
