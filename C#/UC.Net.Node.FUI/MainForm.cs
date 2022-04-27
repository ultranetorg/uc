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

namespace UC.Net.Node.FUI
{
	public partial class MainForm : Form
	{
		public readonly Dispatcher		Dispatcher;
		readonly Timer					Timer = new Timer();

		public MainForm(Dispatcher dispatcher)
		{
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;

			InitializeComponent();

			MinimumSize = Size;
			Dispatcher = dispatcher;

			var dashboard = new TreeNode("Dashboard"){Tag = new DashboardPanel(Dispatcher, dispatcher.Vault)};
			navigator.Nodes.Add(dashboard);

			var accs = new TreeNode("Accounts"){ Tag = new AccountsPanel(Dispatcher, dispatcher.Vault)};
			navigator.Nodes.Add(accs);

			if(dispatcher.Chain != null)
			{
				var txs = new TreeNode("Transactions"){ Tag = new TransactionsPanel(Dispatcher, dispatcher.Vault) };
				navigator.Nodes.Add(txs);
	
				var memb = new TreeNode("Membership"){ Tag = new MembershipPanel(Dispatcher, dispatcher.Vault) };
				navigator.Nodes.Add(memb);
	
				var auth = new TreeNode("Authors"){ Tag = new AuthorPanel(Dispatcher, dispatcher.Vault) };
				navigator.Nodes.Add(auth);
	
				var prod = new TreeNode("Products"){ Tag = new ProductPanel(Dispatcher, dispatcher.Vault) };
				navigator.Nodes.Add(prod);
	
				var rel = new TreeNode("Releases"){ Tag = new ReleasePanel(Dispatcher, dispatcher.Vault) };				
				navigator.Nodes.Add(rel);
	
				var pub = new TreeNode("Publish"){ Tag = new PublishPanel(Dispatcher, dispatcher.Vault) };				
				navigator.Nodes.Add(pub);

				var exp = new TreeNode("Explorer"){ Tag = new ExplorerPanel(Dispatcher, dispatcher.Vault) };
				navigator.Nodes.Add(exp);
			}

			var transfer = new TreeNode("Emission"){ Tag = new EmissionPanel(Dispatcher, dispatcher.Vault) };
			navigator.Nodes.Add(transfer);

			var nodes = new TreeNode("Network");
			nodes.Expand();
			navigator.Nodes.Add(nodes);

			{
				var peers = new TreeNode("Peers"){ Tag = new PeersPanel(Dispatcher, dispatcher.Vault) };
				nodes.Nodes.Add(peers);

				if(dispatcher.Chain != null)
				{
					var members = new TreeNode("Members"){ Tag = new MembersPanel(Dispatcher, dispatcher.Vault)};
					nodes.Nodes.Add(members);
				}

				if(Dispatcher.Settings.Dev.UI)
				{
					var initials = new TreeNode("Initials"){ Tag = new InitialsPanel(Dispatcher, dispatcher.Vault)};
					nodes.Nodes.Add(initials);
				}

				var ipfs = new TreeNode("IPFS"){  };
				nodes.Nodes.Add(ipfs);
			}

			var apps = new TreeNode("Applications"){ Tag = new ApplicationsPanel(Dispatcher, dispatcher.Vault) };
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
			lock(Dispatcher.Lock)
			{
				Text = "Ultranet " + (Dispatcher.IsNode ? "Node" : "Client"); //System.Reflection.Assembly.GetAssembly(GetType()).ManifestModule.Assembly.CustomAttributes.FirstOrDefault(i => i.AttributeType == typeof(AssemblyProductAttribute)).ConstructorArguments[0].Value.ToString();
				Text += $"{(Dispatcher.IsNode && Dispatcher.Connections.Count() < Dispatcher.Settings.PeersMin ? " - Low Peers" : "")}{(Dispatcher.IsNode && Dispatcher.IP != IPAddress.None ? " - " + Dispatcher.IP : "")} - {Dispatcher.Synchronization}{(Dispatcher.Generator != null && Dispatcher.Chain.Members.Any(i => i.Generator == Dispatcher.Generator) ? $" - {Dispatcher.Generator}" : "")}";
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
