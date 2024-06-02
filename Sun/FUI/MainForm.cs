using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Uccs.Sun.FUI
{
	public partial class MainForm : Form
	{
		public readonly Net.Sun		Sun;
		readonly Timer				Timer = new Timer();

		public MainForm(Net.Sun sun)
		{
			AutoScaleMode = AutoScaleMode.Inherit;

			InitializeComponent();

			MinimumSize = Size;
			Sun = sun;
			Sun.MainStarted += c =>	{ 	
										BeginInvoke((MethodInvoker) delegate
													{ 
														LoadUI(sun);
													});
									};

			if(sun.MainThread != null)
			{
				LoadUI(sun);
			}
		}

		public void BeginClose()
		{
			BeginInvoke(Close);
		}

		void LoadUI(Net.Sun sun)
		{
			var dashboard = new TreeNode("Dashboard"){Tag = new DashboardPanel(Sun)};
			navigator.Nodes.Add(dashboard);

			var net = new TreeNode("Network"){Tag = new NetworkPanel(Sun) };
			navigator.Nodes.Add(net);

			var accs = new TreeNode("Accounts"){Tag = new AccountsPanel(Sun)};
			navigator.Nodes.Add(accs);

			foreach(var i in sun.Mcvs)
			{
				var mcv = new TreeNode("Mcv"){};
				navigator.Nodes.Add(mcv);
			
				var m = new TreeNode("Members"){Tag = new MembersPanel(i) };
				mcv.Nodes.Add(m);
	
				if(i.Roles.HasFlag(Role.Chain))
				{
					var t = new TreeNode("Transactions"){ Tag = new TransactionsPanel(i) };
					mcv.Nodes.Add(t);
	
					var c = new TreeNode("Chain"){Tag = new ChainPanel(i) };
					mcv.Nodes.Add(c);
				}
	
				if(i is Rds rds)
				{
					var d = new TreeNode("Domains"){Tag = new DomainPanel(rds) };
					mcv.Nodes.Add(d);
		
					var r = new TreeNode("Resources"){Tag = new ResourcesPanel(rds) };				
					mcv.Nodes.Add(r);
	
					var e = new TreeNode("Emission"){Tag = new EmissionPanel(rds) };
					mcv.Nodes.Add(e);
	
					if(rds.SeedHub != null)
					{
						var s = new TreeNode("Seed Hub"){Tag = new HubPanel(rds) };
						mcv.Nodes.Add(s);
					}
				}
			}

			navigator.SelectedNode = dashboard;
			navigator.ExpandAll();
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
			lock(Sun.Lock)
			{
				Text = $"Ultranet Node - {Sun}";
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
