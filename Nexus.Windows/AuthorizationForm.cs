using System.Windows.Forms;
using Uccs.Net;
using Uccs.Vault;

namespace Uccs.Nexus.Windows;

public partial class AuthorizationForm : Form
{

	public AuthorizationForm(AccountAddress signer,  Authentication authentication, string operation)
	{
		InitializeComponent();

		Application.Text = authentication.Application;
		Net.Text = authentication.Net;
		Signer.Text = signer.ToString();
		Operation.Text = operation;
		
		if(authentication.Logo != null)
		{
			Logo.Image = Image.FromStream(new MemoryStream(authentication.Logo));
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
}
