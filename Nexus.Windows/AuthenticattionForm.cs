using System.Windows.Forms;
using Uccs.Net;
using Uccs.Vault;

namespace Uccs.Nexus.Windows;

public partial class AuthenticattionForm : Form
{
	public AccountAddress	Account { get => AccountAddress.Parse(Accounts.Text); set => Accounts.SelectedItem = value; }
	public Trust			Trust { get; protected set; }

	Vault.Vault				Vault;
	public void				SetApplication(string applicaiton) => Application.Text = applicaiton;
	public void				SetNet(string net) => Net.Text = net;

	public AuthenticattionForm(Vault.Vault vault)
	{
		Vault = vault;

		InitializeComponent();

		Page.BindWallets(vault, Wallets);
		Wallets_SelectedValueChanged(null, EventArgs.Empty);
	}
	
	public void SetLogo(byte[] image)
	{ 
		var i = Logo.Image;

		try
		{
			Logo.Image = Image.FromStream(new MemoryStream(image));
		}
		catch(Exception ex)
		{
			Logo.Image = i;
		}
	}

	private void cancel_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private void Wallets_SelectedValueChanged(object sender, EventArgs e)
	{
		if(Wallets.SelectedItem is string w)
			Page.BindAccounts(Vault, Accounts, Vault.FindWallet(w).Accounts);
		else
			Accounts.Items.Clear();
	}

	private void Allow_Click(object sender, EventArgs e)
	{
		Trust = Trust.Complete;
		DialogResult = DialogResult.OK;
		Close();
	}

	private void Ask_Click(object sender, EventArgs e)
	{
		Trust = Trust.AskEveryTime;
		DialogResult = DialogResult.OK;
		Close();
	}
}
