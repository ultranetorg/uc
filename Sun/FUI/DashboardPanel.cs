using Nethereum.Web3;
using Org.BouncyCastle.Crypto;
using System;
using System.Windows.Forms;

namespace Uccs.Sun.FUI
{
	public partial class DashboardPanel : MainPanel
	{

		public DashboardPanel()
		{
			InitializeComponent();
		}

		public DashboardPanel(Net.Sun d, Vault vault) : base(d, vault)
		{
			InitializeComponent();

			monitor.Sun	= d;

			//d.MainStarted += c =>	{
			//							if(Core.Database != null && !Core.Database.BlockAdded.GetInvocationList().Any(i => i == monitor.OnBlockAdded))
			//								Core.Database.BlockAdded +=  monitor.OnBlockAdded;
			//						};
		}

		public override void Open(bool first)
		{
			//if(Core.Database != null && !Core.Database.BlockAdded.GetInvocationList().Any(i => i == monitor.OnBlockAdded))
			//	Core.Database.BlockAdded +=  monitor.OnBlockAdded;

			if(first)
			{
				logbox.Log = Sun.Workflow.Log;

				BindAccounts(source);
				BindAccounts(destination);

				if(destination.Items.Count > 1)
					destination.SelectedIndex = 1;

				Sun.Mcv.BlockAdded += (b) =>{
												BeginInvoke((MethodInvoker)delegate
															{
																monitor.Invalidate();
															});
											};

				Sun.Mcv.JoinAdded += (b) =>	{
												BeginInvoke((MethodInvoker)delegate
															{
																monitor.Invalidate();
															});
											};
			}
		}

		public override void Close()
		{
			base.Close();

			//if(Core.Database != null)
			//	Core.Database.BlockAdded -= monitor.OnBlockAdded;
		}

		public override void PeriodicalRefresh()
		{
			List<KeyValuePair<string, string>> i;

			lock(Sun.Lock)
			{
				i = Sun.Summary;
			}

			fields.Text = string.Join('\n', i.Select(j => j.Key));
			values.Text = string.Join('\n', i.Select(j => j.Value));
		
			 //monitor.OnBlockAdded(null);
		}

		private void all_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if(source.SelectedItem is AccountAddress a)
			{
				amount.Coins = Sun.Mcv.Accounts.Find(a, Sun.Mcv.LastConfirmedRound.Id).Balance;
			}
		}

		private void source_SelectionChangeCommitted(object sender, EventArgs e)
		{
			//amount.Coins = Core.Database.GetConfirmedBalance(source.SelectedItem as Account);
		}

		private void send_Click(object sender, EventArgs e)
		{
			var signer = GetPrivate(source.SelectedItem as AccountAddress);

			if(signer != null)
			{
				try
				{
					Sun.Enqueue(new UntTransfer(signer, AccountAddress.Parse(destination.Text), amount.Coins), PlacingStage.Null, new Workflow());
				}
				catch(RequirementException ex)
				{
					MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
	}
}
