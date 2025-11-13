using System.Numerics;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus.Windows;

public partial class TransferPage : Page
{
	public TransferPage()
	{
	}

	public TransferPage(Nexus nexus) : base(nexus)
	{
		InitializeComponent();
	}

	public override void Open(bool first)
	{
		FromNet.Items.Insert(0, "rdn");
		FromNet.SelectedIndex = 0;

		BindAccounts(FromId, Nexus.Vault.Wallets.SelectMany(i => i.Accounts));
		FromId.Items.Insert(0, "All");
		FromId.SelectedIndex = 0;

		if(first)
		{
		}
	}

	private void Nets_Changed(object sender, EventArgs e)
	{
		Search(FromId.SelectedItem.ToString());
	}

	private void Accounts_KeyDown(object sender, KeyEventArgs e)
	{
		if(e.KeyCode == Keys.Enter)
		{
			Search(FromId.Text);
		}
	}

	void Search(string query)
	{
	}
}
