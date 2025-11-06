using Uccs.Net;

namespace Uccs.Iam.FUI;

public partial class WalletsPage : Page
{
	public WalletsPage()
	{
	}

	public WalletsPage(Iam iam)
	{
		InitializeComponent();

		Task.Run(() =>	{
							foreach(var i in iam.Vault.Request<WalletsApc.Wallet[]>(new WalletsApc {}, new Flow(null)))
							{
								var li = new ListViewItem();
								
								li.Tag = i;		

								Wallets.Items.Add(i.Name);
							}
						});
	}
}
