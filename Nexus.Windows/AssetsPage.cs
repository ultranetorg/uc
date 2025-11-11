namespace Uccs.Nexus.Windows;

public partial class AssetsPage : Page
{
	public AssetsPage()
	{
	}

	public AssetsPage(Nexus nexus) : base(nexus)
	{
		InitializeComponent();
	}

	public override void Open(bool first)
	{
//		BindWallets(Wallets);
//		Wallets.Items.Insert(0, "Any");
//		Wallets.SelectedIndex = 0;

		BindAccounts(Accounts, Nexus.Vault.Wallets.SelectMany(i => i.Accounts));
		Accounts.Items.Insert(0, "Any");
		Accounts.SelectedIndex = 0;
	}
}
