using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
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
			Sun.MainStarted += c =>{ 	
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

			if(sun.Mcv != null)
			{
				if(sun.Settings.Roles.HasFlag(Role.Chain))
				{
					var txs = new TreeNode("Transactions"){ Tag = new TransactionsPanel(Sun, sun.Vault) };
					navigator.Nodes.Add(txs);

					var exp = new TreeNode("Chain"){ Tag = new ChainPanel(Sun, sun.Vault) };
					navigator.Nodes.Add(exp);
				}

				///var memb = new TreeNode("Membership"){ Tag = new MembershipPanel(Core, sun.Vault) };
				///navigator.Nodes.Add(memb);
	
				var auth = new TreeNode("Authors"){ Tag = new AuthorPanel(Sun, sun.Vault) };
				navigator.Nodes.Add(auth);
	
				var prod = new TreeNode("Products"){ Tag = new ProductPanel(Sun, sun.Vault) };
				navigator.Nodes.Add(prod);
	
				var rel = new TreeNode("Releases"){ Tag = new ReleasePanel(Sun, sun.Vault) };				
				navigator.Nodes.Add(rel);
			}

			var transfer = new TreeNode("Emission"){ Tag = new EmissionPanel(Sun, sun.Vault) };
			navigator.Nodes.Add(transfer);

			var net = new TreeNode("Network"){ Tag = new NetworkPanel(Sun, sun.Vault) };
			net.Expand();
			navigator.Nodes.Add(net);
			
			var gens = new TreeNode("Generators"){ Tag = new GeneratorsPanel(Sun, sun.Vault) };				
			net.Nodes.Add(gens);

			if(Sun.Hub != null)
			{
				var hub = new TreeNode("Hub"){ Tag = new HubPanel(Sun, sun.Vault) };
				navigator.Nodes.Add(hub);
			}

			navigator.SelectedNode = dashboard;
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
