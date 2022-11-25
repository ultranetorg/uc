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

namespace UC.Sun.FUI
{
	public partial class MainForm : Form
	{
		public readonly Core		Core;
		readonly Timer					Timer = new Timer();

		public MainForm(Core core)
		{
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;

			InitializeComponent();

			MinimumSize = Size;
			Core = core;

			var dashboard = new TreeNode("Dashboard"){Tag = new DashboardPanel(Core, core.Vault)};
			navigator.Nodes.Add(dashboard);

			var accs = new TreeNode("Accounts"){ Tag = new AccountsPanel(Core, core.Vault)};
			navigator.Nodes.Add(accs);

			if(core.Database != null)
			{
				var txs = new TreeNode("Transactions"){ Tag = new TransactionsPanel(Core, core.Vault) };
				navigator.Nodes.Add(txs);
	
				var memb = new TreeNode("Membership"){ Tag = new MembershipPanel(Core, core.Vault) };
				navigator.Nodes.Add(memb);
	
				var auth = new TreeNode("Authors"){ Tag = new AuthorPanel(Core, core.Vault) };
				navigator.Nodes.Add(auth);
	
				var prod = new TreeNode("Products"){ Tag = new ProductPanel(Core, core.Vault) };
				navigator.Nodes.Add(prod);
	
				var rel = new TreeNode("Releases"){ Tag = new ReleasePanel(Core, core.Vault) };				
				navigator.Nodes.Add(rel);
	
				var pub = new TreeNode("Publish"){ Tag = new PublishPanel(Core, core.Vault) };				
				navigator.Nodes.Add(pub);

				var exp = new TreeNode("Chain"){ Tag = new ChainPanel(Core, core.Vault) };
				navigator.Nodes.Add(exp);
			}

			var transfer = new TreeNode("Emission"){ Tag = new EmissionPanel(Core, core.Vault) };
			navigator.Nodes.Add(transfer);

			var nodes = new TreeNode("Network"){ Tag = new NetworkPanel(Core, core.Vault) };
			nodes.Expand();
			navigator.Nodes.Add(nodes);

			{
				//if(core.Chain != null)
				//{
				//	var members = new TreeNode("Members"){ Tag = new NetworkPanel(Core, core.Vault)};
				//	nodes.Nodes.Add(members);
				//}

				if(Settings.Dev.UI)
				{
					var initials = new TreeNode("Initials"){ Tag = new InitialsPanel(Core, core.Vault)};
					nodes.Nodes.Add(initials);
				}
 
// 				var ipfs = new TreeNode("IPFS"){  };
// 				nodes.Nodes.Add(ipfs);
			}

			if(Core.Hub != null)
			{
				var hub = new TreeNode("Hub"){ Tag = new HubPanel(Core, core.Vault) };
				navigator.Nodes.Add(hub);
			}

			var apps = new TreeNode("Applications"){ Tag = new ApplicationsPanel(Core, core.Vault) };
			navigator.Nodes.Add(apps);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			Timer.Tick += RefreshInfo;
			Timer.Interval = 500;
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
			lock(Core.Lock)
			{
				Text = "Ultranet Node"; //System.Reflection.Assembly.GetAssembly(GetType()).ManifestModule.Assembly.CustomAttributes.FirstOrDefault(i => i.AttributeType == typeof(AssemblyProductAttribute)).ConstructorArguments[0].Value.ToString();
				Text += $"{(Core.Networking && Core.Connections.Count() < Core.Settings.PeersMin ? " - Low Peers" : "")}{(Core.Networking && Core.IP != IPAddress.None ? " - " + Core.IP : "")} - {Core.Synchronization}{(Core.Generator != null && Core.Database.Members.Any(i => i.Generator == Core.Generator) ? $" - {Core.Generator}" : "")}";
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
