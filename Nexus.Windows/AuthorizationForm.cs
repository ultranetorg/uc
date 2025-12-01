using System.Windows.Forms;
using Uccs.Net;

namespace Uccs.Nexus.Windows;

public partial class AuthorizationForm : Form
{
	public void SetApplication(string applicaiton) => Application.Text = applicaiton;
	public void SetNet(string net) => Net.Text = net;
	public void SetSigner(AccountAddress account) => Signer.Text = account.ToString();
	public void SetOperation(string operation) => Operation.Text = operation;

	public AuthorizationForm()
	{
		InitializeComponent();
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
