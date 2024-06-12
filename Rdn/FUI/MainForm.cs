using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Uccs.Rdn.FUI
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

			Uos.IcnStarted += c =>	{ 	
										BeginInvoke((MethodInvoker) delegate
													{ 
														LoadIcn();
													});
									};
			
			Uos.McvStarted += c =>	{ 	
										BeginInvoke((MethodInvoker) delegate
													{ 
														LoadMcv(c);
													});
									};
			if(Uos.Icn != null)
				LoadIcn();

			foreach(var i in Uos.Mcvs)
				LoadMcv(i);
		}

		public void BeginClose()
		{
			BeginInvoke(Close);
		}

		void LoadIcn()
		{
			var dashboard = new TreeNode("Dashboard"){Tag = Dashboard = new DashboardPanel(Uos.Icn)};
			Navigator.Nodes.Add(dashboard);

			var net = new TreeNode("Network"){Tag = new NetworkPanel(Uos.Icn) };
			Navigator.Nodes.Add(net);

			var accs = new TreeNode("Accounts"){Tag = new AccountsPanel(Uos.Icn)};
			Navigator.Nodes.Add(accs);

			Navigator.SelectedNode = dashboard;
			Navigator.ExpandAll();
		}

		void LoadMcv(Mcv mcv)
		{
			Dashboard.Monitor.Mcv = mcv;
			Dashboard.Mcvs.Add(mcv);

			var root = new TreeNode("Mcv"){};
			Navigator.Nodes.Add(root);
			
			var m = new TreeNode("Members"){Tag = new MembersPanel(mcv) };
			root.Nodes.Add(m);
	
			if(mcv.Settings.Base?.Chain != null)
			{
				var t = new TreeNode("Transactions"){ Tag = new TransactionsPanel(mcv) };
				root.Nodes.Add(t);
	
				var c = new TreeNode("Chain"){Tag = new ChainPanel(mcv) };
				root.Nodes.Add(c);
			}
	
			if(mcv is Net.Rdn rds)
			{
				var d = new TreeNode("Domains"){ Tag = new DomainPanel(rds) };
				root.Nodes.Add(d);
		
				var r = new TreeNode("Resources"){ Tag = new ResourcesPanel(rds) };				
				root.Nodes.Add(r);
	
				var e = new TreeNode("Emission"){ Tag = new EmissionPanel(rds) };
				root.Nodes.Add(e);
	
				if(rds.SeedHub != null)
				{
					var s = new TreeNode("Seed Hub"){ Tag = new HubPanel(rds) };
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
