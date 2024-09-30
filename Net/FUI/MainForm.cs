using System.Windows.Forms;
using Uccs.Rdn;
using Timer = System.Windows.Forms.Timer;

namespace Uccs.Net.FUI
{
	public partial class MainForm : Form
	{
		public readonly Uos.Uos		Uos;
		readonly Timer				Timer = new Timer();

		DashboardPanel				Dashboard;

		public MainForm(Uos.Uos uos)
		{
			Uos = uos;
			AutoScaleMode = AutoScaleMode.Inherit;

			InitializeComponent();

			MinimumSize = Size;

			Uos.IznStarted += c =>	{ 	
										BeginInvoke((MethodInvoker) delegate
													{ 
														LoadIcn();
													});
									};
			
			Uos.NodeStarted += c =>	{ 	
										BeginInvoke((MethodInvoker) delegate
													{ 
														LoadMcv(c as McvNode);
													});
									};
			if(Uos.Izn != null)
				LoadIcn();

			foreach(var i in Uos.Nodes)
				LoadMcv(i.Node as McvNode);
		}

		public void BeginClose()
		{
			BeginInvoke(Close);
		}

		void LoadIcn()
		{
			var dashboard = new TreeNode("Dashboard"){Tag = Dashboard = new DashboardPanel(Uos)};
			Navigator.Nodes.Add(dashboard);

			var net = new TreeNode("Nexus"){Tag = new NexusNetworkPanel(Uos.Izn)};
			Navigator.Nodes.Add(net);

			var accs = new TreeNode("Accounts"){Tag = new AccountsPanel(Uos)};
			Navigator.Nodes.Add(accs);

			Navigator.SelectedNode = dashboard;
			Navigator.ExpandAll();
		}

		void LoadMcv(McvNode node)
		{
			Dashboard.Monitor.Mcv = node.Mcv;

			var root = new TreeNode(node.GetType().Name);
			Navigator.Nodes.Add(root);

			var net = new TreeNode("Network"){Tag = new NetworkPanel(node)};
			root.Nodes.Add(net);
			
			var m = new TreeNode("Members"){Tag = new MembersPanel(node)};
			root.Nodes.Add(m);
	
			if(node.Mcv?.Settings.Base?.Chain != null)
			{
				var t = new TreeNode("Transactions"){ Tag = new TransactionsPanel(node)};
				root.Nodes.Add(t);
	
				var c = new TreeNode("Chain"){Tag = new ChainPanel(node)};
				root.Nodes.Add(c);
			}
	
			if(node is RdnNode rdn)
			{
				var d = new TreeNode("Domains"){ Tag = new DomainPanel(rdn)};
				root.Nodes.Add(d);
		
				var r = new TreeNode("Resources"){ Tag = new ResourcesPanel(rdn)};
				root.Nodes.Add(r);
	
				//var e = new TreeNode("Emission"){ Tag = new EmissionPanel(rdn)};
				//root.Nodes.Add(e);
	
				if(rdn.SeedHub != null)
				{
					var s = new TreeNode("Seed Hub"){ Tag = new HubPanel(rdn)};
					root.Nodes.Add(s);
				}
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

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			foreach(Control i in Controls)
				if(i is MainPanel j)
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
				Text = $"Uos - {Uos}";
			}

			foreach(var i in Controls)
				if(i is MainPanel p)
					p.PeriodicalRefresh();
		}

		private void navigator_AfterSelect(object sender, TreeViewEventArgs e)
		{
			var p = e.Node.Tag as MainPanel;

			foreach(Control i in Controls)
				if(i is MainPanel j)
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
}
