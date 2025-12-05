using Uccs.Net;
using Uccs.Nexus;
using Uccs.Vault;

namespace Uccs.Nexus.Windows;

public partial class SessionsPage : Page
{
	public SessionsPage()
	{
		InitializeComponent();
	}

	public SessionsPage(Nexus nexus) : base(nexus)
	{
		InitializeComponent();
	}

	public override void Open(bool first)
	{
		Program.NexusSystem.BindWallets(this, Nexus.Vault, Wallets, Accounts);
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

			a.RemoveAuthentication(Sessions.SelectedItems[0].Tag as Authentication);

			BindSessions(a);
		}
	}

	private void Sessions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		Revoke.Enabled = e.IsSelected;
	}
}