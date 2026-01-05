using System.Windows.Forms;
using Uccs.Net;
using Uccs.Vault;

namespace Uccs.Nexus.Windows;

public partial class AuthorizationForm : Form
{

	public AuthorizationForm(AccountAddress signer, Authentication authentication, string operation)
	{
		InitializeComponent();

		Application.Text = authentication.Application;
		Net.Text = authentication.Net;
		User.Text = authentication.User;
		Signer.Text = signer.ToString();
		Operation.Text = operation;

		if(authentication.Logo != null)
		{
			Logo.BackColor = Color.Transparent;

			try
			{
				Logo.Image = Image.FromStream(new MemoryStream(authentication.Logo));
				Shield.Width = Shield.Width / 2;
				Shield.Height = Shield.Height / 2;
				Shield.Left = Logo.Width - Shield.Width;
				Shield.Top = Logo.Height - Shield.Height;
				Shield.BackColor = Color.Transparent;

				Shield.Parent = Logo;
			}
			catch(Exception ex)
			{
			}
		}
	}

	private void Reject_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private void Allow_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.OK;
		Close();
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);

		FlashWindow.Flash(this);
	}

	private void Net_Click(object sender, EventArgs e)
	{

	}
}
