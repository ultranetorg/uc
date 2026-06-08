using System.Diagnostics;
using System.Numerics;
using System.Text.Json;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus.Windows;

public partial class TransferPage : Page
{
	public TransferPage()
	{
	}

	public TransferPage(Nexus nexus, IccpLcpClientConnection nnp) : base(nexus, nnp)
	{
		InitializeComponent();
	}

	public override void Open(bool first)
	{
		if(first)
		{
			FromNet.Items.Insert(0, Rdn.Rdn.Test.Name);
			FromNet.SelectedIndex = 0;
			RefreshClasses(FromNet.Text, FromEntity);

			ToNet.Items.Insert(0, Rdn.Rdn.Test.Name);
			ToNet.SelectedIndex = 0;
			RefreshClasses(ToNet.Text, ToEntity);
//
//			Wallets.Items.Insert(0, "All");
//			Wallets.SelectedIndex = 0;

			if(Nexus.Settings.Zone == Zone.Simulation)
			{
				FromEntity.Text = $"{McvTable.User}/{Rdn.Rdn.Test.Father0Name}";
				ToEntity.Text = $"{McvTable.User}/{Rdn.Rdn.Test.Father0Name}";
			}
		}

		//Program.NexusSystem.BindWallets(this, Nexus.Vault, Wallets, Accounts, null);
	}

	private void Open_DropDown(object sender, EventArgs e)
	{
		try
		{
			if(sender == FromNet) RefreshNets(FromNet);
			if(sender == FromEntity) RefreshClasses(FromNet.Text, FromEntity);
			//if(sender == FromAccount) BindAccounts(Nexus.Vault, FromAccount, Nexus.Vault.Wallets.SelectMany(i => i.Accounts));

			if(sender == ToNet) RefreshNets(ToNet);
			if(sender == ToEntity) RefreshClasses(ToNet.Text, ToEntity);

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
		
		if(Nexus.IccpLcpServer != null)
			foreach(var i in Nexus.IccpLcpServer.Locals.Select(i => i.Net))
			{
				combobox.Items.Add(i);
			}
	}

	void RefreshClasses(string net, ComboBox combobox)
	{
		if(combobox.Items.Count > 0)
			return;

		try
		{
			foreach(var i in (Iccp.Call(null, net, new HolderClassesIcca{}, new Flow(5000)) as HolderClassesIccr).Classes)
				combobox.Items.Add(i);
		}
		catch(Exception)
		{
		}
	}

	void RefreshAssets()
	{
		if(Asset.Items.Count > 0)
			return;

		if(string.IsNullOrWhiteSpace(FromNet.Text) || string.IsNullOrWhiteSpace(FromEntity.Text))
			return;

		Asset.Items.Clear();

		var e = Iccp.Call(null, FromNet.Text, new AddressTextToUniversalIcca {Text = FromEntity.Text}, new Flow(5000)) as AddressTextToUniversalIccr;

		foreach(var a in (Iccp.Call(null, FromNet.Text, new HolderAssetsIcca {Entity = e.Universal}, new Flow(5000)) as HolderAssetsIccr).Assets)
		{
			Asset.Items.Add(a);
		}
	}

	void RefreshBalance()
	{
		var e = Iccp.Call(null, FromNet.Text, new AddressTextToUniversalIcca {Text = FromEntity.Text}, new Flow(5000)) as AddressTextToUniversalIccr;

		BalanceLabel.Text = "Balance: ";
		BalanceLabel.Text += (Iccp.Call(	null,
									FromNet.Text,	
									new AssetBalanceIcca
									{
										Entity = e.Universal,
										Asset = (Asset.SelectedItem as Asset).Id
									},
									new Flow(5000)) as AssetBalanceIccr).Balance.ToString();
	}

	private void FromNet_TextUpdate(object sender, EventArgs e)
	{
		FromEntity.Items.Clear();
		Asset.Items.Clear();
	}

	private void ToNet_TextUpdate(object sender, EventArgs e)
	{
		ToEntity.Items.Clear();
	}

	private void Any_Changed(object sender, EventArgs e)
	{
		Transfer.Enabled =	!string.IsNullOrEmpty(FromNet.Text) &&
							!string.IsNullOrEmpty(FromEntity.Text) &&
							!string.IsNullOrEmpty(ToNet.Text) &&
							!string.IsNullOrEmpty(ToEntity.Text) &&
							!string.IsNullOrEmpty(Asset.Text) &&
							!string.IsNullOrEmpty(Amount.Text);
	}

	private void Transfer_Click(object sender, EventArgs e)
	{
		try
		{
			var f = new Flow();

			var t = new AssetTransfer
					{
						FromNet		= FromNet.Text,
						FromEntity	= Iccp.AddressTextToUniversal(null, FromNet.Text, FromEntity.Text, f),
						Asset		= (Asset.SelectedItem as Asset).Id,
						Amount		= BigInteger.Parse(Amount.Text),
						ToNet		= ToNet.Text,
						ToEntity	= Iccp.AddressTextToUniversal(null, ToNet.Text, ToEntity.Text, f),
					};

			t.Signature = Nexus.Vault.Authorize(CryptographyType.Iccp,
												FromNet.Text,
												JsonSerializer.Serialize(t),
												FromEntity.Text,
												Nexus.GetApplicationSession(FromNet.Text, f),
												t.Hashify(),
												f);

			Iccp.Call(null, FromNet.Text, new TransactIcca {Transactions = [t]}, f);
		}
		catch(Exception ex) when(!Debugger.IsAttached)
		{
			ShowError(ex.Message);
		}
	}
}
