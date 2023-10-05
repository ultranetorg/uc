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
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;

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

		void LoadUI(Net.Sun sun)
		{
			var dashboard = new TreeNode("Dashboard"){Tag = new DashboardPanel(Sun, sun.Vault)};
			navigator.Nodes.Add(dashboard);

			var accs = new TreeNode("Accounts"){ Tag = new AccountsPanel(Sun, sun.Vault)};
			navigator.Nodes.Add(accs);
	
			var auth = new TreeNode("Authors"){ Tag = new AuthorPanel(Sun, sun.Vault) };
			navigator.Nodes.Add(auth);
	
			var rel = new TreeNode("Resources"){ Tag = new ResourcesPanel(Sun, sun.Vault) };				
			navigator.Nodes.Add(rel);

			var transfer = new TreeNode("Emission"){ Tag = new EmissionPanel(Sun, sun.Vault) };
			navigator.Nodes.Add(transfer);

			var net = new TreeNode("Network"){ Tag = new NetworkPanel(Sun, sun.Vault) };
			navigator.Nodes.Add(net);

			if(sun.Mcv != null)
			{
				var mcv = new TreeNode("Mcv"){ Tag = new AccountsPanel(Sun, sun.Vault)};
				navigator.Nodes.Add(mcv);
			
				var gens = new TreeNode("Members"){ Tag = new MembersPanel(Sun, sun.Vault) };
				mcv.Nodes.Add(gens);

				if(sun.Roles.HasFlag(Role.Chain))
				{
					var txs = new TreeNode("Transactions"){ Tag = new TransactionsPanel(Sun, sun.Vault) };
					mcv.Nodes.Add(txs);

					var exp = new TreeNode("Chain"){ Tag = new ChainPanel(Sun, sun.Vault) };
					mcv.Nodes.Add(exp);
				}

				///var memb = new TreeNode("Membership"){ Tag = new MembershipPanel(Core, sun.Vault) };
				///navigator.Nodes.Add(memb);
			}

			if(Sun.SeedHub != null)
			{
				var hub = new TreeNode("Hub"){ Tag = new HubPanel(Sun, sun.Vault) };
				navigator.Nodes.Add(hub);
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
