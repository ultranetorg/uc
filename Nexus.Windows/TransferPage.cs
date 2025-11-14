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

			FromAccount.Items.Insert(0, "All");
			FromAccount.SelectedIndex = 0;
		}

		BindAccounts(FromAccount, Nexus.Vault.Wallets.SelectMany(i => i.Accounts));
	}

	private void Open_DropDown(object sender, EventArgs e)
	{
		try
		{
			if(sender == FromNet) RefreshNets(FromNet);
			if(sender == FromClass) RefreshClasses(FromNet.Text, FromClass);
			if(sender == FromAccount) BindAccounts(FromAccount, Nexus.Vault.Wallets.SelectMany(i => i.Accounts));

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

		foreach(var i in Nexus.Nodes.Select(i => i.Net))
		{
			combobox.Items.Add(i);
		}

	}

	void RefreshClasses(string net, ComboBox combobox)
	{
		if(combobox.Items.Count > 0)
			return;

		foreach(var i in Nexus.GetNetToNetPeering(net).Call(net, () => new HolderClassesNnc {}, new Flow("")).Classes)
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

		foreach(var a in Nexus.GetNetToNetPeering(FromNet.Text).Call(FromNet.Text,	() => new HolderAssetsNnc
																					{
																						HolderClass = FromClass.Text,
																						HolderId = FromId.Text
																					},
																					new Flow("")).Assets)
		{
			Asset.Items.Add(a);
		}
	}

	void RefreshBalance()
	{
		Balance.Text = "Balance: ";
		Balance.Text += Nexus.GetNetToNetPeering(FromNet.Text).Call(FromNet.Text, () => new AssetBalanceNnc
																						{
																							HolderClass = FromClass.Text,
																							HolderId = FromId.Text,
																							Name = (Asset.SelectedItem as Asset).Name
																						},
																						new Flow("")).ToString();
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
}
