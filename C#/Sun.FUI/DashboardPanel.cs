using Nethereum.KeyStore.Crypto;
using Nethereum.Web3;
using Org.BouncyCastle.Crypto;
using System;
using System.Windows.Forms;

namespace UC.Sun.FUI
{
	public partial class DashboardPanel : MainPanel
	{

		public DashboardPanel()
		{
			InitializeComponent();
		}

		public DashboardPanel(Core d, Vault vault) : base(d, vault)
		{
			InitializeComponent();

			monitor.Core	= d;

			d.Started += c =>	{
									if(Core.Database != null && !Core.Database.BlockAdded.GetInvocationList().Any(i => i == monitor.OnBlockAdded))
										Core.Database.BlockAdded +=  monitor.OnBlockAdded;
								};
		}

		public override void Open(bool first)
		{
			if(Core.Database != null && !Core.Database.BlockAdded.GetInvocationList().Any(i => i == monitor.OnBlockAdded))
				Core.Database.BlockAdded +=  monitor.OnBlockAdded;

			if(first)
			{
				logbox.Log = Core.Workflow.Log;

				BindAccounts(source);
				BindAccounts(destination);

				if(destination.Items.Count > 1)
					destination.SelectedIndex = 1;
			}
		}

		public override void Close()
		{
			base.Close();

			if(Core.Database != null)
				Core.Database.BlockAdded -= monitor.OnBlockAdded;
		}

		public override void PeriodicalRefresh()
		{
			string[][] i;

			lock(Core.Lock)
			{
				i = Core.Info;
			}

			fields.Text = string.Join('\n', i[0]);
			values.Text = string.Join('\n', i[1]);
		}

		private void all_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if(source.SelectedItem is Account a)
			{
				amount.Coins = Core.Database.Accounts.Find(a, Core.Database.LastConfirmedRound.Id).Balance;
			}
		}

		private void source_SelectionChangeCommitted(object sender, EventArgs e)
		{
			//amount.Coins = Core.Database.GetConfirmedBalance(source.SelectedItem as Account);
		}

		private void send_Click(object sender, EventArgs e)
		{
			var signer = GetPrivate(source.SelectedItem as Account);

			if(signer != null)
			{
				try
				{
					Core.Enqueue(new UntTransfer(signer, Account.Parse(destination.Text), amount.Coins), PlacingStage.Null, new Workflow());
				}
				catch(RequirementException ex)
				{
					MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
	}
}
