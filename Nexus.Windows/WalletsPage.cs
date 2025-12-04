using Uccs.Net;
using Uccs.Net.FUI;
using Uccs.Vault;

namespace Uccs.Nexus.Windows;

public partial class WalletsPage : Page
{
	WalletAccount CurrentAccout => Accounts.SelectedItems[0].Tag as WalletAccount;
	Wallet CurrentWallet => Wallets.SelectedItems[0].Tag as Wallet;

	public WalletsPage()
	{
	}

	public WalletsPage(Nexus nexus) : base(nexus)
	{
		InitializeComponent();
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		Wallets.SmallImageList = imageList1;
		AccountsPanel.Enabled = false;


		Task.Run(() =>
				{
					LoadWallets();

					Invoke(() => Wallets_ItemSelectionChanged(this, new ListViewItemSelectionChangedEventArgs(null, 0, false)));
				});
	}

	void LoadWallets()
	{
		Wallets.Items.Clear();

		foreach(var i in Nexus.Vault.Wallets)
		{
			var li = new ListViewItem(i.Name, i.Locked ? "lock" : null);
			li.Tag = i;

			Invoke(() => Wallets.Items.Add(li));
		}

		Invoke(() => Wallets_ItemSelectionChanged(this, new ListViewItemSelectionChangedEventArgs(null, 0, false)));
	}

	void LoadAccounts(Wallet w)
	{
		Invoke(() => Accounts.Items.Clear());

		foreach(var i in w.Accounts)
		{
			var li = new ListViewItem(i.Name);
			li.Tag = i;
			li.SubItems.Add(i.Address.ToString());

			Invoke(() => Accounts.Items.Add(li));
		}

		Invoke(() =>{
						Accounts_ItemSelectionChanged(this, new ListViewItemSelectionChangedEventArgs(null, 0, false));

						CreateAccount.Enabled = !w.Locked;
						ImportAccount.Enabled = !w.Locked;
					});
	}

	private void Wallets_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		var w = e.Item?.Tag as Wallet;

		if(w != null && e.IsSelected)
		{

			LockUnlock.Text = w.Locked ? "Unlock" : "Lock";

			LockUnlock.Enabled = true;
			ExportWallet.Enabled = true;
			DeleteWallet.Enabled = true;

			AccountsPanel.Enabled = !w.Locked;

			LoadAccounts(w);
		}
		else
		{
			LockUnlock.Enabled = false;
			ExportWallet.Enabled = false;
			DeleteWallet.Enabled = false;

			Accounts.Items.Clear();
			AccountsPanel.Enabled = false;
		}

	}

	private void LockUnlock_EnabledChanged(object sender, EventArgs e)
	{

	}

	private void Accounts_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		if(e.IsSelected)
		{
			var a = e.Item.Tag as WalletAccount;

		}
		else
		{
		}

		CopyAddress.Enabled = e.IsSelected;
		ShowSecret.Enabled = e.IsSelected;
		DeleteAccount.Enabled = e.IsSelected;
	}

	private void CreateWallet_Click(object sender, EventArgs e)
	{
		try
		{
			var f = new CreateWalletForm();

			if(f.ShowDialog(this) == DialogResult.OK)
			{
				var acc = AccountKey.Create();
				var w = Nexus.Vault.AddWallet(f.WalletName, [acc], f.Password);

				LoadWallets();
			}
		}
		catch(Exception ex)
		{
			ShowException("Wallet creation failed", ex);
		}
	}

	private void ImportWallet_Click(object sender, EventArgs e)
	{
		var f = new OpenFileDialog();

		using(var ofd = new OpenFileDialog())
		{
			ofd.Title = "Import a wallet ...";
			ofd.Filter = "All Files (*.*)|*.*";
			ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

			if(ofd.ShowDialog() == DialogResult.OK)
			{
				Nexus.Vault.AddWallet(Path.GetFileNameWithoutExtension(ofd.SafeFileName), File.ReadAllBytes(ofd.FileName));

				LoadWallets();
			}
		}
	}

	private void LockUnlock_Click(object sender, EventArgs e)
	{
		var i = Wallets.SelectedItems[0].Index;
		var w = Wallets.SelectedItems[0].Tag as Wallet;

		if(w.Locked)
		{
			var f = new EnterPasswordForm();

			if(f.Ask("A password required to unlock this wallet"))
			{
				w.Unlock(w.Password);
			}
			else
				return;
		}
		else
			w.Lock();

		LoadWallets();

		Wallets.Items[i].Selected = true;
		//Wallets_ItemSelectionChanged(this, new ListViewItemSelectionChangedEventArgs([i.Index], i.Index, true));
	}

	private void RenameWallet_Click(object sender, EventArgs e)
	{

	}

	private void ExportWallet_Click(object sender, EventArgs e)
	{
		var f = new SaveFileDialog();

		using(var fd = new SaveFileDialog())
		{
			fd.FileName = CurrentWallet.Name;
			fd.Title = "Export wallet ...";
			fd.Filter = "All Files (*.*)|*.*";
			fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

			if(fd.ShowDialog() == DialogResult.OK)
			{
				File.WriteAllBytes(fd.FileName, CurrentWallet.ToRaw());
			}
		}
	}

	private void DeleteWallet_Click(object sender, EventArgs e)
	{
		Nexus.Vault.DeleteWallet(CurrentWallet.Name);

		LoadWallets();
	}

	private void CreateAccount_Click(object sender, EventArgs e)
	{
		CurrentWallet.AddAccount(AccountKey.Create().PrivateKey);
		LoadAccounts(CurrentWallet);
	}

	private void ImportAccount_Click(object sender, EventArgs e)
	{
		try
		{
			if(TextForm.ShowDialog(this, "Enter private key",	"Your private or secret key is a 32-byte (256-bit) hexadecimal string.\r\n" +
																"It usually contains 64 characters, using numbers 0–9 and letters A–F (or a–f).\r\n\r\n" +
																"Example format:\r\n\r\n" +
																"4f3c9a27d8b0e6f2c3a91be547d0f8c2a3b1e7d4568c9f0a1b2c3d4e5f6a7b8c\r\n\r\n" +
																"Important Notes:\r\n\r\n" +
																"It should not contain spaces, symbols, or the “0x” prefix.\r\n" +
																"Always copy it exactly — even a single wrong character will make it invalid.\r\n" +
																"Keep it secret and offline — anyone with this key can access your funds and data.", out var k) == DialogResult.OK)
			{
				CurrentWallet.AddAccount(k.FromHex());
				LoadAccounts(CurrentWallet);
			}
		}
		catch(Exception ex)
		{
			ShowException("Account import failed", ex);
		}
	}

	private void CopyAddress_Click(object sender, EventArgs e)
	{
		Clipboard.SetText(CurrentAccout.Address.ToString());
	}

	private void ShowSecret_Click(object sender, EventArgs e)
	{
		TextForm.ShowDialog(this,
							"Secret(Private) Key", 
							"Your private key is the most sensitive part of your account.\r\n" +
							"It provides full access to your assets and personal data.\r\n" +
							"If someone obtains your private key, they can permanently steal your funds or impersonate you.\r\n" +
							"There is no way to recover lost or stolen assets caused by sharing your key.\r\n\r\n" +
							"Important Guidelines:\r\n\r\n" +
							"Never share your private key or recovery phrase with anyone — not even support staff.\r\n" +
							"Do not upload or store your key in cloud storage, email, or messaging apps.\r\n" +
							"Keep it offline, encrypted, and backed up in a secure location.\r\n" +
							"Only use your key in trusted applications and official websites.\r\n\r\n" +
							"Remember:\r\n\r\n" +
							"Once exposed, a private key cannot be made safe again. Always keep it secret and secure.", 
							CurrentAccout.Key.PrivateKey.ToHex());
	}

	private void DeleteAccount_Click(object sender, EventArgs e)
	{
		if(MessageBox.Show(this,"You are about to delete your crypto account from this wallet.\r\n" +
								"This action is irreversible — once deleted, your account and all associated data will be removed.\r\n\r\n" +
								"Before you continue:\r\n\r\n" +
								"Make sure you have securely backed up your private key or recovery phrase.\r\nn" +
								"Without it, you will lose permanent access to your funds and data.\r\n" +
								"We cannot recover your account or restore your assets after deletion.\r\n\r\n" +
								"Are you sure you want to continue?", 
								"Warning – Deleting Your Account is Permanent", 
								MessageBoxButtons.YesNo, 
								MessageBoxIcon.Warning, 
								MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
		{
			CurrentWallet.DeleteAccount(CurrentAccout);
			LoadAccounts(CurrentWallet);
		}
	}

	private void Wallets_AfterLabelEdit(object sender, LabelEditEventArgs e)
	{
		if(e.Label == null)
		{
			e.CancelEdit = true;
			return;
		}

		if(string.IsNullOrWhiteSpace(e.Label))
		{
			MessageBox.Show(this, "Can not be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			e.CancelEdit = true;
			return;
		}

		CurrentWallet.Rename(e.Label);
	}
}