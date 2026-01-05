using System.Security.Cryptography;
using System.Windows.Forms;
using Uccs.Net;
using Uccs.Vault;

namespace Uccs.Nexus.Windows;

public partial class AuthenticattionForm : Form
{
	Vault.Vault Vault;
	public AccountAddress Account { get => AccountAddress.Parse(Accounts.Text); }
	public Trust Trust { get; protected set; }

	public AuthenticattionForm(Vault.Vault vault, string applicaiton, string net, string user, AccountAddress account)
	{
		Vault = vault;

		InitializeComponent();

		Net.Text = net;
		User.Text = user;
		Application.Text = applicaiton;
		Allow.Enabled = Ask.Enabled = false;

		Program.NexusSystem.BindWallets(this, vault, Wallets, Accounts, account);
	}

	public void SetLogo(byte[] image)
	{
		Logo.BackColor = Color.Transparent;

		try
		{
			Logo.Image = Image.FromStream(new MemoryStream(image));
			Shield.Width = Shield.Width / 2;
			Shield.Height = Shield.Height / 2;
			Shield.Left = Logo.Width - Shield.Width;
			Shield.Top = Logo.Height - Shield.Height;
			Shield.BackColor = Color.Transparent;

			Shield.Parent = Logo;
		}
		catch
		{
		}
	}

	private void cancel_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
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

	private void Accounts_TextChanged(object sender, EventArgs e)
	{
		Allow.Enabled = Ask.Enabled = Accounts.SelectedItem is AccountAddress;
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);

		FlashWindow.Flash(this);
	}
}
