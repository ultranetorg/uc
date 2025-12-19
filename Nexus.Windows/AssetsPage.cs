using System.Numerics;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus.Windows;

public partial class AssetsPage : Page
{

	public AssetsPage()
	{
	}

	public AssetsPage(Nexus nexus, NnpIppClientConnection nnp) : base(nexus, nnp)
	{
		InitializeComponent();
	}

	public override void Open(bool first)
	{
		if(first)
		{
			Nets.Items.Insert(0, "rdn");
			Nets.SelectedIndex = 0;

			Program.NexusSystem.BindAccounts(Nexus.Vault, Accounts, Nexus.Vault.Wallets.SelectMany(i => i.Accounts), null);
			Accounts.Items.Insert(0, "All");
			Accounts.SelectedIndex = 0;

			Message.Visible = false;
		}
	}

	private void Nets_Changed(object sender, EventArgs e)
	{
		Search(Accounts.SelectedItem.ToString());
	}

	private void Accounts_KeyDown(object sender, KeyEventArgs e)
	{
		if(e.KeyCode == Keys.Enter)
		{
			Search(Accounts.Text);
		}
	}

	void Search(string account)
	{
		var f = new Flow("");

		Assets.Items.Clear();
		Message.Visible = true;
		Message.Text = "Loading...";

		try
		{
			foreach(var acc in account == "All" ? Nexus.Vault.Wallets.SelectMany(i => i.Accounts).Select(i => i.Address) : [AccountAddress.Parse(account)])
			{
				foreach(var h in Nnp.Call(new Nnc<HoldersByAccountNna, HoldersByAccountNnr>(new () {Net = Nets.Text, Address = acc.Bytes}), f).Holders)
				{
					foreach(var a in Nnp.Call(new Nnc<HolderAssetsNna, HolderAssetsNnr>(new () {Net = Nets.Text, Entity = h}), f).Assets)
					{
						var b = Nnp.Call(new Nnc<AssetBalanceNna, AssetBalanceNnr>(new () {Net = Nets.Text, Entity = h, Name = a.Name}), f).Balance;
			
						var li = new ListViewItem(h);
						li.SubItems.Add(a.Name);
						li.SubItems.Add(a.Units);
						li.SubItems.Add(b.ToString());
			
						Assets.Items.Add(li);
			
					}
				}
			}
			
			Message.Visible = false;
		}
		catch(CodeException ex)
		{
			Message.Text = ex.Message;
		}
		catch(NexusException ex)
		{
			Message.Text = ex.Message;
		}
		catch(FormatException ex)
		{
			Message.Visible = false;

			ShowError(ex.Message);
		}
	}

	private void Start_Click(object sender, EventArgs e)
	{
		Search(Accounts.Text);
	}
}
