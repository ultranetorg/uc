using Uccs.Net;
using Uccs.Vault;

namespace Uccs.Nexus.Windows;

public class Page : UserControl
{
	public bool First = true;
	public Nexus Nexus;

	public virtual void Open(bool first){ }
	public virtual void Close(){ }
	public virtual void PeriodicalRefresh(){ }

	public Page()
	{
	}

	public Page(Nexus nexus) : this()
	{
		Nexus = nexus;
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

	}

	protected void BindAccounts(ComboBox control, IEnumerable<WalletAccount> accounts)
	{
		control.Items.Clear();

		IEnumerable<AccountAddress> keys;

		lock(Nexus.Vault)
			keys = accounts.Select(i => i.Address);

		foreach(var i in keys)
			control.Items.Add(i);

		if(control.Items.Count > 0)
			control.SelectedIndex = 0;

	}


	public void ShowException(string message, Exception ex)
	{
		MessageBox.Show(this, message + " (" + ex.Message + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}

	public void ShowError(string message)
	{
		MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
}
