using System.Windows.Forms;

namespace Uccs.Net.FUI
{
	public partial class EmissionPanel : MainPanel
	{
		public EmissionPanel(McvNode mcv)
		{
			InitializeComponent();

			//if(Sun.Settings.Secrets?.EthereumWallet != null)
			//	browse.Text = Sun.Settings.Secrets.EthereumWallet;

			walletChoice.Checked = true;

			//DestLabel.Text += $"\n({RdnNode.Net.EthereumNetwork} Network)";
		}

		public override void Open(bool first)
		{
			if(first)
			{
				//BindAccounts(destination);

				eth_TextChanged(this, null);
			}

			UpdateReadiness();
		}

		private void transferBrowse_Click(object sender, EventArgs e)
		{
			var d = new OpenFileDialog();

			if(d.ShowDialog() == DialogResult.OK)
			{
				browse.Text = d.FileName;
				UpdateSource();
			}
		}

		private void sourceChoice_CheckedChanged(object sender, EventArgs e)
		{
			browse.Enabled = walletChoice.Checked;
			privatekey.Enabled = privatekeyChoice.Checked;

			UpdateSource();
		}

		private void walletORprivatekey_TextChanged(object sender, EventArgs e)
		{
			UpdateSource();
		}

		private void UpdateSource()
		{
// 			if(walletChoice.Checked)
// 			{
// 				try
// 				{
// 					var json = JsonSerializer.Deserialize<dynamic>(File.ReadAllText(browse.Text));
// 					source.Text = json["address"];
// 				}
// 				catch(Exception)
// 				{
// 					source.Text = string.IsNullOrWhiteSpace(browse.Text) || browse.Text == "Browse ..." ? "" : "<Can't read or incorrect wallet file>";
// 				}
// 			}
// 
// 			if(privatekeyChoice.Checked)
// 			{
// 				try
// 				{
// 					source.Text = new EthECKey(privatekey.Text).GetPublicAddress();
// 				}
// 				catch(Exception)
// 				{
// 					source.Text = string.IsNullOrWhiteSpace(privatekey.Text) ? "" : "<Incorrect private key>";
// 				}
// 			}
		}

		private void UpdateReadiness()
		{
// 			var v = Nethereum.Util.AddressUtil.Current.IsValidAddressLength(source.Text) && 
// 					eth.Wei > 0 && 
// 					destination.SelectedItem != null && McvNode.Vault.Wallets.Keys.Any(i => i == destination.SelectedItem as AccountAddress);
// 
// 			transfer.Enabled = v;
		}

		private void source_TextChanged(object sender, EventArgs e)
		{
			UpdateReadiness();
		}

		private void eth_TextChanged(object sender, EventArgs e)
 		{
// 			UpdateReadiness();
// 
// 			lock(Node.Lock)
// 			{
// 				if(Mcv?.LastConfirmedRound != null)
// 				{
// 					estimated.Text = Immission.Calculate(eth.Wei).ToDecimalString() + " UNT";
// 				}
// 			}
		}

	 	private void transfer_Click(object sender, EventArgs e)
		{
// 			transfergroup.Enabled = false;
// 			finishgroup.Enabled = false;
// 
// 			Log log = new Log();
// 			log.Report(this, "Emission initiated");
// 
// 			Nethereum.Web3.Accounts.Account a = null;
// 
// 			try
// 			{
// 				if(walletChoice.Checked)
// 				{
// 					//string password = Sun.Settings.Secrets?.EthereumPassword;
// 					//
// 					//if(password == null)
// 					//{
// 					//	var f = new EnterPasswordForm(Sun.Settings.Secrets?.Password);
// 					//
// 					//	if(f.Ask(browse.Text))
// 					//		password = f.Password;
// 					//}
// 					//
// 					//if(password != null)
// 					//{
// 					//	a = Nethereum.Web3.Accounts.Account.LoadFromKeyStore(File.ReadAllText(browse.Text), password, new System.Numerics.BigInteger((int)Sun.Net.EthereumNetwork));
// 					//}
// 				}
// 				else if(privatekeyChoice.Checked)
// 				{
// 					a = new Nethereum.Web3.Accounts.Account(new EthECKey(privatekey.Text), RdnNode.Net.EthereumNetwork);
// 				}
// 
// 				if(a != null)
// 				{
// 					var k = GetPrivate(destination.SelectedItem as AccountAddress);
// 						
// 					if(k != null)
// 					{
// 						var v = new Flow("Emission", log);
// 
// 						var f = new FlowControlForm(Node, v);
// 						f.StartPosition = FormStartPosition.CenterParent;
// 						f.Show(ParentForm);
// 
// 						///Sun.Emit(a, eth.Wei, k, TransactionStatus.None, v);
// 					}
// 				}
// 			}
// 			catch(TaskCanceledException)
// 			{
// 			}
// 			catch(Exception ex)
// 			{
// 				ShowException("Emission failed", ex);
// 			}
// 
// 			transfergroup.Enabled = true;
// 			finishgroup.Enabled = true;
		}

		private void finish_Click(object sender, EventArgs e)
		{
			//transfergroup.Enabled = false;
			//finishgroup.Enabled = false;
			//
			//try
			//{
			//	Sun.FinishEmission(GetPrivate(Unfinished.SelectedItems[0].Tag as AccountAddress), new Flow(MethodBase.GetCurrentMethod().Name));
			//
			//	transfergroup.Enabled = true;
			//	finishgroup.Enabled = true;
			//}
			//catch(Exception ex)
			//{
			//	ShowException("Emission finishing failed", ex);
			//}
			//
			//transfergroup.Enabled = true;
			//finishgroup.Enabled = true;
		}
	}
}

