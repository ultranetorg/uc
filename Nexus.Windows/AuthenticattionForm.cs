using System.Security.Cryptography;
using System.Windows.Forms;
using Uccs.Net;

namespace Uccs.Nexus.Windows;

public partial class AuthenticattionForm : Form
{
	Vault				Vault;
	public PublicKey	Key { get => PublicKey.Parse(Keys.Text); }
	public Trust		Trust { get; protected set; }

	public AuthenticattionForm(Vault vault, string applicaiton, string net, string user, PublicKey account)
	{
		Vault = vault;

		InitializeComponent();

		Net.Text = net;
		User.Text = user;
		Application.Text = applicaiton;
		Allow.Enabled = Ask.Enabled = false;

		Program.NexusWindows.BindWallets(this, vault, Wallets, Keys, account);
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
		Trust = Trust.AlwaysAllow;
		DialogResult = DialogResult.OK;
		Close();
	}

	private void Ask_Click(object sender, EventArgs e)
	{
		Trust = Trust.AskEveryTime;
		DialogResult = DialogResult.OK;
		Close();
	}

	private void Keys_TextChanged(object sender, EventArgs e)
	{
		Allow.Enabled = Ask.Enabled = Keys.SelectedItem is PublicKey;
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);

		FlashWindow.Flash(this);
	}
}
