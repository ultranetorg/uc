using System.Numerics;
using Uccs.Net;
using Uccs.Rdn;

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
		Nets.Items.Insert(0, "rdn");
		Nets.SelectedIndex = 0;

		BindAccounts(Accounts, Nexus.Vault.Wallets.SelectMany(i => i.Accounts));
		Accounts.Items.Insert(0, "All");
		Accounts.SelectedIndex = 0;

		if(first)
		{
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
			var api = Nexus.GetNetToNetnNodeApi(Nets.Text);

			foreach(var acc in account == "All" ? Nexus.Vault.Wallets.SelectMany(i => i.Accounts).Select(i => i.Address) : [AccountAddress.Parse(account)])
			{
				foreach(var h in api.Request<AssetHolder[]>(new NnHoldersApc { Address = acc.Bytes }, f))
				{
					foreach(var a in api.Request<Asset[]>(new NnHolderAssetsApc { HolderClass = h.Class, HolderId = h.Id }, f))
					{
						var b = api.Request<BigInteger>(new NnAssetBalanceApc { HolderClass = h.Class, HolderId = h.Id, Name = a.Name }, f);

						var li = new ListViewItem(h.Class);
						li.SubItems.Add(h.Id);
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
