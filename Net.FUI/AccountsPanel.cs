using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Uccs.Net.FUI;

public partial class AccountsPanel : MainPanel
{
	public AccountAddress CurrentAccout => Accounts.SelectedItems[0]?.Tag as AccountAddress;

	McvNode Node;

	public AccountsPanel(McvNode d)
	{
		InitializeComponent();

		Node = d;
	}

	public override void Open(bool first)
	{
		if(first)
		{
			Accounts.Items.Clear();
		}
	}
	
	public override void PeriodicalRefresh()
	{
		foreach(ListViewItem i in Accounts.Items)
		{
			if(!(bool)i.SubItems[1].Tag)
			{
				i.SubItems[1].Tag = true;

				Task.Run(() =>	{
									Account e = null;

									try
									{
										e = Node.Peering.Call(new AccountPpc(i.Tag as AccountAddress), Node.Flow).Account; 
									}
									catch(Exception)
									{
									}

									Invoke(() => {
													i.SubItems[1].Text = e?.Spacetime.ToString() ?? "..."; 
													i.SubItems[2].Text = (e?.Energy.ToString() ?? "...") + " / " + (e?.EnergyNext.ToString() ?? "..."); 
													i.SubItems[3].Text = null; 
													i.SubItems[1].Tag = false;
												 });
								});
			}
		}
	}

	void AddRow(AccountAddress a)
	{
		var r = new ListViewItem(a.ToString());
		r.Tag = a;

		r.SubItems.Add("...").Tag = false;

		Accounts.Items.Add(r);
	}

	private void add_Click(object sender, EventArgs args)
	{
		//var f = new CreatePasswordForm();
		//
		//if(f.ShowDialog() == DialogResult.OK)
		//{
		//	var acc = AccountKey.Create();
		//	Uos.Vault.AddWallet(acc, f.Password);
		//	Uos.Vault.SaveWallet(acc);
		//	
		//	AddRow(acc);
		//}
	}

	private void remove_Click(object sender, EventArgs e)
	{
		//if(MessageBox.Show(this, $"Are you sure you want to delete {CurrentAccout} account?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
		//{
		//	Uos.Vault.DeleteWallet(CurrentAccout);
		//	Accounts.Items.Remove(Accounts.SelectedItems[0]);
		//}
	}

	private void showprivate_Click(object sender, EventArgs e)
	{
		//try
		//{
		//	var a = GetPrivate(Node, CurrentAccout);
		//
		//	if(a != null)
		//	{
		//		TextForm.ShowDialog("Private Key", $"Private Key for {CurrentAccout}", a.PrivateKey.ToHex());
		//	}
		//}
		//catch(Exception ex)
		//{
		//	ShowException("Can't access private key", ex);
		//}
	}

	private void backup_Click(object sender, EventArgs e)
	{
		var f = new SaveFileDialog();

		f.FileName = CurrentAccout.ToString();
		//f.DefaultExt = Vault.WalletExt(Node.Net.Cryptography);

		if(f.ShowDialog(this) == DialogResult.OK)
		{
			//File.WriteAllBytes(f.FileName, Uos.Vault.Wallets.Find(i => i.Address == CurrentAccout).Raw);
		}
	}

	private void accounts_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		Remove.Enabled = e.IsSelected;
		Showprivate.Enabled  = e.IsSelected;
		Backup.Enabled  = e.IsSelected;
		CopyAddress.Enabled  = e.IsSelected;
	}

	private void CopyAddress_Click(object sender, EventArgs e)
	{
		Clipboard.SetText(CurrentAccout.ToString());
	}
}