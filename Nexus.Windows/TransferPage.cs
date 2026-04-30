using System.Numerics;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus.Windows;

public partial class TransferPage : Page
{
	public TransferPage()
	{
	}

	public TransferPage(Nexus nexus, NnpLcpClientConnection nnp) : base(nexus, nnp)
	{
		InitializeComponent();
	}

	public override void Open(bool first)
	{
		if(first)
		{
			FromNet.Items.Insert(0, "rdn");
			FromNet.SelectedIndex = 0;
			RefreshClasses(FromNet.Text, FromEntity);

			ToNet.Items.Insert(0, "rdn");
			ToNet.SelectedIndex = 0;
			RefreshClasses(ToNet.Text, ToEntity);
//
//			Wallets.Items.Insert(0, "All");
//			Wallets.SelectedIndex = 0;

			if(Nexus.Settings.Zone == Zone.Simulation)
			{
				FromEntity.Text = "user/father0000";
				ToEntity.Text = "user/father0000";
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
		
		if(Nexus.NnpIppServer != null)
			foreach(var i in Nexus.NnpIppServer.Locals.Select(i => i.Net))
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
			foreach(var i in (Nnp.Call(net, new HolderClassesNna{}, new Flow(5000)) as HolderClassesNnr).Classes)
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

		foreach(var a in (Nnp.Call(FromNet.Text, new HolderAssetsNna {Entity = FromEntity.Text}, new Flow(5000)) as HolderAssetsNnr).Assets)
		{
			Asset.Items.Add(a);
		}
	}

	void RefreshBalance()
	{
		Balance.Text = "Balance: ";
		Balance.Text += (Nnp.Call(FromNet.Text,	new AssetBalanceNna
												{
													Entity = FromEntity.Text,
													Name = (Asset.SelectedItem as Asset).Name
												},
												new Flow(5000)) as AssetBalanceNnr).Balance.ToString();
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
		///try
		///{
		///	Nnp.Call(ToNet.Text, new AssetTransferNna
		///						 {
		///							Net = FromNet.Text,
		///							ToNet = ,
		///							FromEntity = FromEntity.Text,
		///							ToEntity = ToEntity.Text,
		///							Name = (Asset.SelectedItem as Asset).Name,
		///							Amount = Amount.Text,
		///							///Signer = Accounts.SelectedItem as AccountAddress,
		///						 },
		///						 new Flow(5000));
		///}
		///catch(Exception ex)
		///{
		///	ShowError(ex.Message);
		///}
	}
}
