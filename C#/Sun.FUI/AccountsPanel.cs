using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nethereum.KeyStore.Crypto;

namespace UC.Sun.FUI
{
	public partial class AccountsPanel : MainPanel
	{
		public Account CurrentAccout => accounts.SelectedItems[0]?.Tag as Account;

		public AccountsPanel(Core d, Vault vault) : base(d, vault)
		{
			InitializeComponent();

		}

		public override void Open(bool first)
		{
			if(first)
			{
				accounts.Items.Clear();

				foreach(var i in Vault.Accounts)
				{
					AddRow(i);
				}
			}
		}
		
		public override void PeriodicalRefresh()
		{
			foreach(ListViewItem i in accounts.Items)
			{
				if(!(bool)i.SubItems[1].Tag)
				{
					i.SubItems[1].Tag = true;
	
					Task.Run(	() =>
								{
									string t;
	
									try
									{
										t = Core.Connect(Role.Chain, null, new Workflow(5 * 1000)).GetAccountInfo(i.Tag as Account, true).Info?.Balance.ToHumanString(); 
									}
									catch(ApiCallException)
									{
										t = "...";
									}
	
									Invoke(	(MethodInvoker) delegate
											{
												i.SubItems[1].Text = t; 
												i.SubItems[1].Tag = false;
											});
								});
				}
			}
	
			foreach(ListViewItem i in accounts.Items)
			{
				if(!(bool)i.SubItems[2].Tag)
				{
					i.SubItems[2].Tag = true;
	
					Task.Run(	() =>
								{
									string t;
									var c = new CancellationTokenSource(30 * 1000);
	
									try
									{
										t = Core.Connect(Role.Chain, null, new Workflow(5 * 1000)).GetAccountInfo(i.Tag as Account, false).Info?.Balance.ToHumanString(); 
									}
									catch(ApiCallException)
									{
										t = "...";
									}
									finally
									{
										c.Dispose();
									}
	
									Invoke(	(MethodInvoker) delegate
											{
												i.SubItems[2].Text = t; 
												i.SubItems[2].Tag = false;
											});
								});
				}
			}
		}

		void AddRow(Account a)
		{
			var r = new ListViewItem(a.ToString());
			r.Tag = a;

			r.SubItems.Add("...").Tag = false;
			r.SubItems.Add("...").Tag = false;

			accounts.Items.Add(r);
		}

		private void add_Click(object sender, EventArgs e)
		{
			var f = new CreatePasswordForm();
			
			if(f.ShowDialog() == DialogResult.OK)
			{
				var acc = PrivateAccount.Create();
				Vault.SaveAccount(acc, f.Password);
				
				AddRow(acc);
			}
		}

		private void remove_Click(object sender, EventArgs e)
		{
			if(MessageBox.Show(this, $"Are you sure you want to delete {CurrentAccout} account?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				Vault.DeleteAccount(CurrentAccout);
				accounts.Items.Remove(accounts.SelectedItems[0]);
			}
		}

		private void showprivate_Click(object sender, EventArgs e)
		{
			try
			{
				var a = GetPrivate(CurrentAccout);

				if(a != null)
				{
					TextForm.ShowDialog("Private Key", $"Private Key for {CurrentAccout}", a.Key.GetPrivateKey());
				}
			}
			catch(Exception ex) when (ex is RequirementException || ex is DecryptionException)
			{
				ShowException("Can't access private key", ex);
			}
		}

		private void backup_Click(object sender, EventArgs e)
		{
			var f = new SaveFileDialog();

			f.FileName = CurrentAccout.ToString();
			f.DefaultExt = Vault.WalletExt;

			if(f.ShowDialog(this) == DialogResult.OK)
			{
				File.WriteAllBytes(f.FileName, Vault.Wallets[CurrentAccout]);
			}
		}

		private void accounts_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			Remove.Enabled = e.IsSelected;
			Showprivate.Enabled  = e.IsSelected;
			Backup.Enabled  = e.IsSelected;
			CopyAddress.Enabled  = e.IsSelected;
		}

		private void CopyAddress_Click(object sender, EventArgs e)
		{
			System.Windows.Forms.Clipboard.SetText(CurrentAccout.ToString());
		}
	}

}