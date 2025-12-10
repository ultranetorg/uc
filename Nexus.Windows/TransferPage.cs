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
		if(first)
		{
			FromNet.Items.Insert(0, "rdn");
			FromNet.SelectedIndex = 0;
			RefreshClasses(FromNet.Text, FromClass);

			ToNet.Items.Insert(0, "rdn");
			ToNet.SelectedIndex = 0;
			RefreshClasses(ToNet.Text, ToClass);
//
//			Wallets.Items.Insert(0, "All");
//			Wallets.SelectedIndex = 0;
		}

		Program.NexusSystem.BindWallets(this, Nexus.Vault, Wallets, Accounts);
	}

	private void Open_DropDown(object sender, EventArgs e)
	{
		try
		{
			if(sender == FromNet) RefreshNets(FromNet);
			if(sender == FromClass) RefreshClasses(FromNet.Text, FromClass);
			//if(sender == FromAccount) BindAccounts(Nexus.Vault, FromAccount, Nexus.Vault.Wallets.SelectMany(i => i.Accounts));

			if(sender == ToNet) RefreshNets(ToNet);
			if(sender == ToClass) RefreshClasses(ToNet.Text, ToClass);

			if(sender == Asset) RefreshAssets();
		}
		catch(Exception ex)
		{
		}
	}

	private void FromClass_SelectionChangeCommitted(object sender, EventArgs e)
	{
		RefreshAssets();
	}

	private void FromId_TextChanged(object sender, EventArgs e)
	{
		try
		{
			Asset.Items.Clear();
			RefreshAssets();
		}
		catch(Exception ex)
		{

		}
	}

	private void Asset_SelectionChangeCommitted(object sender, EventArgs e)
	{
		try
		{
			RefreshBalance();
		}
		catch(Exception ex)
		{
		}
	}

	void RefreshNets(ComboBox combobox)
	{
		combobox.Items.Clear();

		foreach(var i in Nexus.NnpIppServer.Locals.Select(i => i.Net))
		{
			combobox.Items.Add(i);
		}

	}

	void RefreshClasses(string net, ComboBox combobox)
	{
		if(combobox.Items.Count > 0)
			return;

		foreach(var i in Nexus.NnpConnection.Call(new Nnc<HolderClassesNna, HolderClassesNnr>(new()
		{
			Net = net
		}),
																							 new Flow(5000)).Classes)
		{
			combobox.Items.Add(i);
		}
	}

	void RefreshAssets()
	{
		if(Asset.Items.Count > 0)
			return;

		if(string.IsNullOrWhiteSpace(FromNet.Text) || string.IsNullOrWhiteSpace(FromClass.Text) || string.IsNullOrWhiteSpace(FromId.Text))
			return;

		Asset.Items.Clear();

		foreach(var a in Nexus.NnpConnection.Call(new Nnc<HolderAssetsNna, HolderAssetsNnr>(new()
		{
			Net = FromNet.Text,
			HolderClass = FromClass.Text,
			HolderId = FromId.Text
		}),
																							new Flow(5000)).Assets)
		{
			Asset.Items.Add(a);
		}
	}

	void RefreshBalance()
	{
		Balance.Text = "Balance: ";
		Balance.Text += Nexus.NnpConnection.Call(new Nnc<AssetBalanceNna, AssetBalanceNnr>(new()
		{
			Net = FromNet.Text,
			HolderClass = FromClass.Text,
			HolderId = FromId.Text,
			Name = (Asset.SelectedItem as Asset).Name
		}),
																							new Flow(5000)).Balance.ToString();
	}

	private void FromNet_TextUpdate(object sender, EventArgs e)
	{
		FromClass.Items.Clear();
		Asset.Items.Clear();
	}

	private void ToNet_TextUpdate(object sender, EventArgs e)
	{
		ToClass.Items.Clear();
	}

	private void Any_Changed(object sender, EventArgs e)
	{
		Transfer.Enabled = !string.IsNullOrEmpty(FromNet.Text) &&
							!string.IsNullOrEmpty(FromClass.Text) &&
							!string.IsNullOrEmpty(FromId.Text) &&
							!string.IsNullOrEmpty(Wallets.Text) &&
							!string.IsNullOrEmpty(ToNet.Text) &&
							!string.IsNullOrEmpty(ToClass.Text) &&
							!string.IsNullOrEmpty(ToId.Text) &&
							!string.IsNullOrEmpty(Asset.Text) &&
							!string.IsNullOrEmpty(Amount.Text);
	}

	private void Transfer_Click(object sender, EventArgs e)
	{
		try
		{
			Nexus.NnpConnection.Call(new Nnc<AssetTransferNna, AssetTransferNnr>(new()
																				{
																					Net = FromNet.Text,
																					ToNet = ToNet.Text,
																					FromClass = FromClass.Text,
																					FromId = FromId.Text,
																					ToClass = ToClass.Text,
																					ToId = ToId.Text,
																					Name = (Asset.SelectedItem as Asset).Name,
																					Amount = Amount.Text,
																					Signer = Accounts.SelectedItem as AccountAddress,
																				}),
																				new Flow(5000));
		}
		catch(Exception ex)
		{
			ShowError(ex.Message);
		}
	}
}
