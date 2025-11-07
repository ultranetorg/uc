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
		
		Wallets.SmallImageList = imageList1;

		Task.Run(() =>	{
							foreach(var i in iam.Vault.Request<WalletsApc.Wallet[]>(new WalletsApc {}, new Flow(null)))
							{
								var li = new ListViewItem("", i.Locked ? "lock" : null);
								
								li.SubItems.Add(i.Name);
								li.Tag = i;

								Invoke(() =>{ 
												Wallets.Items.Add(li);

												//LockUnlock.Text = i.Locked ? "Unlock" : "Lock";
												//RenameWallet.Enabled = !i.Locked;
												//DeleteWallet.Enabled = !i.Locked;	
											});
							}
						});
	}
}
