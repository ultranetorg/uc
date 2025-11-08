using Uccs.Net;
using Uccs.Net.FUI;
using Uccs.Vault;

namespace Uccs.Iam.FUI;

public partial class WalletsPage : Page
{
	Iam Iam;

	public WalletsPage()
	{
	}

	public WalletsPage(Iam iam)
	{
		InitializeComponent();

		Iam = iam;
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

		foreach(var i in Iam.Vault.Request<WalletsApc.Wallet[]>(new WalletsApc {AdminKey = Iam.Settings.VaultAdminKey }, new Flow(null)))
		{
			var li = new ListViewItem("", i.Locked ? "lock" : null);

			li.SubItems.Add(i.Name);
			li.Tag = i;

			Invoke(() => Wallets.Items.Add(li));
		}
	}

	private void Wallets_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		var w = e.Item?.Tag as WalletsApc.Wallet;

		if(w != null && e.IsSelected)
		{

			LockUnlock.Text = w.Locked ? "Unlock" : "Lock";
			LockUnlock.Enabled = true;
			RenameWallet.Enabled = true;
			DeleteWallet.Enabled = true;

			AccountsPanel.Enabled = true;

			Invoke(() => Accounts.Items.Clear());

			foreach(var i in Iam.Vault.Request<WalletAccount[]>(new WalletApc {Name = w.Name, AdminKey = Iam.Settings.VaultAdminKey}, new Flow(null)))
			{
				var li = new ListViewItem(i.Name);
				li.Tag = i;
				li.SubItems.Add(i.Address.ToString());

				Invoke(() =>
						{
							Accounts.Items.Add(li);
						});
			}

			Invoke(() =>{
							Accounts_ItemSelectionChanged(this, new ListViewItemSelectionChangedEventArgs(null, 0, false));

							CreateAccount.Enabled = !w.Locked;
							ImportAccount.Enabled = !w.Locked;
						
							RenameWallet.Enabled = !w.Locked;
						});
		}
		else
		{
			LockUnlock.Enabled = false;
			RenameWallet.Enabled = false;
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
		RenameAccount.Enabled = e.IsSelected;
		ShowSecret.Enabled = e.IsSelected;
		DeleteAccount.Enabled = e.IsSelected;
	}

	private void CreateWallet_Click(object sender, EventArgs e)
	{

	}

	private void ImportWallet_Click(object sender, EventArgs e)
	{

	}

	private void LockUnlock_Click(object sender, EventArgs e)
	{
		var i = Wallets.SelectedItems[0].Index;

		if(Wallets.SelectedItems[0].Tag is WalletsApc.Wallet w && w.Locked)
		{
			var f = new EnterPasswordForm();
			
			if(f.Ask("A password required to unlock this wallet"))
			{
				Iam.Vault.Request<WalletsApc.Wallet[]>(new UnlockWalletApc {Name = w.Name, Password = f.Password, AdminKey = Iam.Settings.VaultAdminKey}, new Flow(null));
			} 
			else
				return;
		}
		else
			Iam.Vault.Request<WalletsApc.Wallet[]>(new LockWalletApc {AdminKey = Iam.Settings.VaultAdminKey}, new Flow(null));

		LoadWallets();

		Wallets.Items[i].Selected = true;
		//Wallets_ItemSelectionChanged(this, new ListViewItemSelectionChangedEventArgs([i.Index], i.Index, true));
	}

	private void RenameWallet_Click(object sender, EventArgs e)
	{

	}

	private void DeleteWallet_Click(object sender, EventArgs e)
	{

	}

	private void CreateAccount_Click(object sender, EventArgs e)
	{

	}

	private void ImportAccount_Click(object sender, EventArgs e)
	{

	}

	private void CopyAddress_Click(object sender, EventArgs e)
	{

	}

	private void RenameAccount_Click(object sender, EventArgs e)
	{

	}

	private void ShowSecret_Click(object sender, EventArgs e)
	{

	}

	private void DeleteAccount_Click(object sender, EventArgs e)
	{

	}
}
