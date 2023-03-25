using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using Org.BouncyCastle.Math;
using System.Threading;

namespace Uccs.Sun.FUI
{
	public partial class EmissionPanel : MainPanel
	{
		public EmissionPanel(Core d, Vault vault) : base(d, vault)
		{
			InitializeComponent();

			if(Core.Settings.Secret?.EmissionWallet != null)
				browse.Text = Core.Settings.Secret.EmissionWallet;

			walletChoice.Checked = true;

			DestLabel.Text += $"\n({Core.Settings.Zone.EtheterumNetwork} Network)";
		}

		public override void Open(bool first)
		{
			if(first)
			{
				BindAccounts(destination);

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
			if(walletChoice.Checked)
			{
				try
				{
					var json = JsonValue.Parse(File.ReadAllText(browse.Text));
					source.Text = json["address"];
				}
				catch(Exception)
				{
					source.Text = string.IsNullOrWhiteSpace(browse.Text) || browse.Text == "Browse ..." ? "" : "<Can't read or incorrect wallet file>";
				}
			}

			if(privatekeyChoice.Checked)
			{
				try
				{
					source.Text = new EthECKey(privatekey.Text).GetPublicAddress();
				}
				catch(Exception)
				{
					source.Text = string.IsNullOrWhiteSpace(privatekey.Text) ? "" : "<Incorrect private key>";
				}
			}
		}

		private void UpdateReadiness()
		{
			var v = Nethereum.Util.AddressUtil.Current.IsValidAddressLength(source.Text) && 
					source.Text.IsHex() && 
					eth.Wei > 0 && 
					destination.SelectedItem != null && Vault.Accounts.Any(i => i == destination.SelectedItem as AccountAddress);

			transfer.Enabled = v;
		}

		private void source_TextChanged(object sender, EventArgs e)
		{
			UpdateReadiness();
		}

		private void eth_TextChanged(object sender, EventArgs e)
		{
			UpdateReadiness();

			lock(Core.Lock)
			{
				if(Core.Database != null)
				{
					estimated.Text = Emission.Calculate(Core.Database.LastConfirmedRound.WeiSpent, Core.Database.LastConfirmedRound.Factor, eth.Wei).Amount.ToHumanString() + " UNT";
				}
			}
		}

	 	private void transfer_Click(object sender, EventArgs e)
		{
			transfergroup.Enabled = false;
			finishgroup.Enabled = false;

			Log log = new Log();
			log.Report(this, "Emission initiated");

			Nethereum.Web3.Accounts.Account a = null;

			try
			{
				if(walletChoice.Checked)
				{
					string password = Core.Settings.Secret?.EmissionPassword;
	
					if(password == null)
					{
						var f = new PasswordForm(Core.Settings.Secret?.Password);
					
						if(f.Ask(browse.Text))
							password = f.Password;
					}
	
					if(password != null)
					{
						a = Nethereum.Web3.Accounts.Account.LoadFromKeyStore(File.ReadAllText(browse.Text), password, new System.Numerics.BigInteger((int)Core.Settings.Zone.EtheterumNetwork));
					}
				}
				else if(privatekeyChoice.Checked)
				{
					a = new Nethereum.Web3.Accounts.Account(new EthECKey(privatekey.Text), Core.Settings.Zone.EtheterumNetwork);
				}

				if(a != null)
				{
					var k = GetPrivate(destination.SelectedItem as AccountAddress);
						
					if(k != null)
					{
						var v = new Workflow(log);

						var f = new FlowControlForm(Core, v);
						f.StartPosition = FormStartPosition.CenterParent;
						f.Show(ParentForm);

						Core.Emit(a, eth.Wei, k, PlacingStage.Null, v);
					}
				}
			}
			catch(TaskCanceledException)
			{
			}
			catch(Exception ex)
			{
				ShowException("Emission failed", ex);
			}

			transfergroup.Enabled = true;
			finishgroup.Enabled = true;
		}

		private void finish_Click(object sender, EventArgs e)
		{
			transfergroup.Enabled = false;
			finishgroup.Enabled = false;

			try
			{
				Core.FinishTransfer(GetPrivate(Unfinished.SelectedItems[0].Tag as AccountAddress));

				transfergroup.Enabled = true;
				finishgroup.Enabled = true;
			}
			catch(Exception ex)
			{
				ShowException("Emission finishing failed", ex);
			}
			
			transfergroup.Enabled = true;
			finishgroup.Enabled = true;
		}
	}
}
