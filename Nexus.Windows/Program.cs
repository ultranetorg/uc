using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Uccs.Net;
using Uccs.Nexus;
using Uccs.Rdn;
using Uccs.Vault;

namespace Uccs.Nexus.Windows;

public class Program: ApplicationContext
{
	[STAThread]
	static void Main()
	{
		ApplicationConfiguration.Initialize();
		System.Windows.Forms.Application.Run(new NexusSystem());
	}

	public class NexusSystem : ApplicationContext
	{
		public static string		ExeDirectory;
		Nexus						Nexus;

		public NexusSystem()
		{
			ExeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		
			var b = new NetBoot(ExeDirectory);
			var ns = new NexusSettings(b.Zone, b.Profile) {Name = Guid.NewGuid().ToString()};
			var vs = new VaultSettings(b.Profile, b.Zone);
		
			Nexus = new Nexus(b, ns, vs, new RealClock(), new Flow(nameof(Nexus), new Log()));

			InitializeAuthUI(Nexus);

			Nexus.RunRdn(null);

		}

		public static void InitializeAuthUI(Nexus nexus)
		{
			nexus.Vault.AuthenticationRequested =	(appplication, logo, net, suggested) =>
													{
														var f = new AuthenticattionForm(nexus.Vault);

														f.Account = suggested;
														f.SetApplication(appplication);
														f.SetNet(net);
														f.SetLogo(logo);
													
														if(f.ShowDialog() == DialogResult.OK)
														{
															return new AuthenticationChoice {Account = f.Account, Trust = f.Trust};
														}
														else
														{
															return null;
														}
													};

			nexus.Vault.AuthorizationRequested  =	(signer, authenticateon, operation) =>
													{
														var f = new AuthorizationForm(signer, authenticateon, operation);
													
														if(f.ShowDialog() == DialogResult.OK)
														{
															return true;
														}
														else
														{
															return false;
														}
													};

			nexus.Vault.UnlockRequested	=	(uiparent, wallet) =>
											{
												var f = new EnterPasswordForm();
											
												while(true)
												{
													try
													{
														if(f.Ask($"A password required to unlock '{wallet}' wallet", uiparent as IWin32Window))
														{
															nexus.Vault.FindWallet(wallet).Unlock(f.Password);
														}

														break;
													}
													catch(CryptographicException ex)
													{
														MessageBox.Show(f, "Access Denied", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
													}
												}
											};
		}

		static void BindWallets(Vault.Vault vault,ComboBox control)
		{
			//control.Items.Clear();

			//foreach(var i in vault.Wallets)
			//	control.Items.Add(i);

			control.DataSource = vault.Wallets;
			control.DisplayMember = "Name";

			control.SelectedItem = control.Items.OfType<Wallet>().FirstOrDefault(i => !i.Locked);
		}

		public static void BindAccounts(Vault.Vault vault, ComboBox control, IEnumerable<WalletAccount> accounts)
		{
			control.Items.Clear();

			IEnumerable<AccountAddress> keys;

			lock(vault)
				keys = accounts.Select(i => i.Address);

			foreach(var i in keys)
				control.Items.Add(i);

			if(control.Items.Count > 0)
				control.SelectedIndex = 0;

		}

		public static void BindWallets(IWin32Window parent, Vault.Vault vault, ComboBox wallets, ComboBox accounts)
		{
			void f(object s, EventArgs e)
			{
				accounts.Items.Clear();

				if(wallets.SelectedItem is Wallet w)
				{
					if(w.Locked)
						vault.UnlockRequested(parent, w.Name);
	
					if(!w.Locked)
						BindAccounts(vault, accounts, w.Accounts);
					else
						wallets.SelectedItem = null;
				}
			};

			wallets.SelectionChangeCommitted += f;

			BindWallets(vault, wallets);

			f(null, null);
		}
	}
}
