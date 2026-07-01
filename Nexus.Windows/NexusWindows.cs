using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Win32;
using Uccs.Net;
using Uccs.Nexus.Windows.Properties;
using Uccs.Vault;

namespace Uccs.Nexus.Windows;

public class Program: ApplicationContext
{
	[STAThread]
	static void Main()
	{
		ApplicationConfiguration.Initialize();
		System.Windows.Forms.Application.Run(new NexusWindows());
	}

	public class NexusWindows : ApplicationContext
	{
		public static string		ExeDirectory;
		Nexus						Nexus;
		NotifyIcon					TrayIcon;

		public NexusWindows()
		{
			ExeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		
			var b = new NetBoot(ExeDirectory);
			var ns = new NexusSettings(b.Zone, b.Profile);
			var vs = new VaultSettings(b.Profile);
		
			var f = new Flow(nameof(Program), new Log());

			Nexus = new Nexus(b, ns, vs, f);

			Nexus.Stopped += n =>	{
										TrayIcon.Visible = false;
										TrayIcon.Dispose();
										System.Windows.Forms.Application.Exit();
									};

			InitializeAuthUI(Nexus);

			var traymenu = new ContextMenuStrip();

			traymenu.Items.Add("-");
			traymenu.Items.Add("Identity and Activity", null, (s, e) =>	{
																			var f = new IamForm(Nexus);
																			f.Show();
																		});
			traymenu.Items.Add("-");
			traymenu.Items.Add("Exit", null, (s, e) => Nexus.Stop());

			Nexus.IccpLcpServer.ConnectionEstablished += c =>	{
														 			if(c is IccpLcpConnection nc && nc.Type == IccpLcpConnectionType.Node)
														 			{
															 			if(nc.Net == Iccn.Root)
															 			{
																			var i = new ToolStripMenuItem(nc.Net, null, (s, e) =>	{
																 																		var f = new RdnForm(Nexus.RdnNode);
																 																		f.Show();
																																	}
																																	){Tag = c};
																			traymenu.Items.Insert(0, i);
															 			} 
															 			else
															 			{
																			var i = new ToolStripMenuItem(nc.Net, null, (s, e) =>	{
																																		nc.Call(null, nc.Net, new ShowGuiIcca(), new Flow(5000));
																																	}
																																	){Tag = c};
																			
																			traymenu.Invoke(() => traymenu.Items.Insert(0, i));
															 			}
														 			}
																};

			Nexus.IccpLcpServer.ConnectionLost += c =>	{
															foreach(var i in traymenu.Items)
															{
																if(i is ToolStripMenuItem l && l.Tag == c)
																{
																	traymenu.Invoke(() => traymenu.Items.Remove(l));
																	break;
																}
															}
														};

			#pragma warning disable WFO5001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
			TrayIcon =	new NotifyIcon()
						{
							Icon = Resources.IconWhite,
							ContextMenuStrip = traymenu,
							Visible = true,
							Text = "UOS"
						};
			#pragma warning restore WFO5001

			TrayIcon.ShowBalloonTip(3000, $"Welcome to the Ultranet {b.Zone} Zone", "Click the tray icon to manage your identity and activity", ToolTipIcon.Info);
			
			TrayIcon.BalloonTipClicked += (o, a) => new IamForm(Nexus).Show();
			TrayIcon.DoubleClick += (s, e) => {};

			if(!IsProtocolRegistered())
				RegisterProtocol();

			Nexus.RunRdn(null, new RealClock());

			 f.Log.Report($"CreateFirstAccountIfEmpty={Nexus.Vault.Settings.CreateFirstAccountIfEmpty} && Nexus.Vault.Wallets={Nexus.Vault.Wallets.Count}");

			if(Nexus.Vault.Settings.CreateFirstAccountIfEmpty && !Nexus.Vault.Wallets.Any())
			{
				var iam = new IamForm(Nexus);
				iam.CreateFirstWallet();
				iam.Show();
			}
		}

		public static void InitializeAuthUI(Nexus nexus)
		{
			nexus.Vault.AuthenticationRequested =	(appplication, logo, net, user, account) =>
													{
														var f = new AuthenticattionForm(nexus.Vault, appplication, net, user, account);
														
														if(logo != null)
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

		public static void BindAccounts(Vault.Vault vault, ComboBox control, IEnumerable<WalletAccount> accounts, AccountAddress preselected)
		{
			control.Items.Clear();

			IEnumerable<AccountAddress> keys;

			lock(vault)
				keys = accounts.Select(i => i.Address);

			foreach(var i in keys)
				control.Items.Add(i);

			if(control.Items.Count > 0)
				if(preselected != null)
					control.SelectedIndex = Array.IndexOf(keys.ToArray(), preselected);
				else	
					control.SelectedIndex = 0;
		}

		public static void BindWallets(IWin32Window parent, Vault.Vault vault, ComboBox wallets, ComboBox accounts, AccountAddress preselected)
		{
			void f(object s, EventArgs e)
			{
				accounts.Items.Clear();

				if(wallets.SelectedItem is Wallet w)
				{
					if(w.Locked)
						vault.UnlockRequested(parent, w.Name);
	
					if(!w.Locked)
						BindAccounts(vault, accounts, w.Accounts, preselected);
					else
						wallets.SelectedItem = null;
				}
			};

			wallets.SelectionChangeCommitted += f;

			BindWallets(vault, wallets);

			f(null, null);
		}

		string OpenCmd
		{
			get
			{
				var c = Assembly.GetExecutingAssembly().Location;
				c = c.Remove(c.Length - 3) + "exe";
				return $"\"{c}\" open \"%1\"";
			}
		}

		public void RegisterProtocol()
		{
			using var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{Iccp.Scheme}");
			key.SetValue("", $"{Iccp.Scheme} Protocol");
			key.SetValue("URL Protocol", "");

			var shell = key.CreateSubKey(@"shell\open\command");
			shell.SetValue("", OpenCmd);
		}

		public bool IsProtocolRegistered()
		{
			var p = Assembly.GetExecutingAssembly().Location;
			p = p.Remove(p.Length - 3) + "exe";

			using var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{Iccp.Scheme}");

			if(key == null)
				return false;

			if(key.GetValue("URL Protocol") == null)
				return false;

			using var commandKey = key.OpenSubKey(@"shell\open\command");
			var commandValue = commandKey?.GetValue("")?.ToString();

			if(string.IsNullOrEmpty(commandValue)) 
				return false;

			return commandValue.Equals(OpenCmd, StringComparison.OrdinalIgnoreCase);
		}
	}
}
