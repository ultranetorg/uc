using Uccs.Net;
using Uccs.Nexus;
using Uccs.Vault;

namespace Uccs.Nexus.Windows;

public partial class SessionsPage : Page
{
	Nexus Nexus;

	public SessionsPage()
	{
		InitializeComponent();
	}

	public SessionsPage(Nexus nexus) : this()
	{
		Nexus = nexus;
	}

	public override void Open(bool first)
	{
		BindWallets(Wallets);
	}

	protected void BindWallets(ComboBox control)
	{
		control.Items.Clear();

		IEnumerable<string> keys;

		lock(Nexus.Vault)
			keys = Nexus.Vault.Wallets.Select(i => i.Name);

		foreach(var i in keys)
			control.Items.Add(i);

		if(control.Items.Count > 0)
			control.SelectedIndex = 0;

		Wallets_SelectionChangeCommitted(null, EventArgs.Empty);
	}

	protected void BindAccunts(ComboBox control, Wallet wallet)
	{
		control.Items.Clear();

		IEnumerable<AccountAddress> keys;

		lock(Nexus.Vault)
			keys = wallet.Accounts.Select(i => i.Address);

		foreach(var i in keys)
			control.Items.Add(i);

		if(control.Items.Count > 0)
			control.SelectedIndex = 0;

		Accounts_SelectionChangeCommitted(null, EventArgs.Empty);
	}

	protected void BindSessions(WalletAccount account)
	{
		Sessions.Items.Clear();

		foreach(var i in account.Authentications)
		{
			var li = new ListViewItem(i.Application);
			li.Tag = i;
			li.SubItems.Add(i.Net);
			li.SubItems.Add(i.Session.ToHex());

			Sessions.Items.Add(li);
		}

		Revoke.Enabled = false;
	}

	private void Wallets_SelectionChangeCommitted(object sender, EventArgs e)
	{
		Wallet w;

		lock(Nexus.Vault)
			w = Nexus.Vault.Wallets.Find(i => i.Name == Wallets.SelectedItem as string);

		BindAccunts(Accounts, w);
	}

	private void Accounts_SelectionChangeCommitted(object sender, EventArgs e)
	{
		WalletAccount a;

		lock(Nexus.Vault)
		{
			var w = Nexus.Vault.Wallets.Find(i => i.Name == Wallets.SelectedItem as string);
			a = w.Accounts.Find(i => i.Address == Accounts.SelectedItem as AccountAddress);
		}

		BindSessions(a);
	}

	private void Revoke_Click(object sender, EventArgs e)
	{
		WalletAccount a;

		lock(Nexus.Vault)
		{
			var w = Nexus.Vault.Wallets.Find(i => i.Name == Wallets.SelectedItem as string);
			a = w.Accounts.Find(i => i.Address == Accounts.SelectedItem as AccountAddress);

			a.Authentications.Remove(Sessions.SelectedItems[0].Tag as Authentication);

			BindSessions(a);
		}
	}

	private void Sessions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		Revoke.Enabled = e.IsSelected;
	}
}