using System.ComponentModel;
using System.Windows.Forms;

namespace Uccs.Nexus.Windows;

public partial class CreateWalletForm : Form
{
	public string Password => password.Text;
	public string WalletName => string.IsNullOrWhiteSpace(walletname.Text) ? null : walletname.Text;
	
	public CreateWalletForm()
	{
		InitializeComponent();

		//suggestions.Text = string.Join(Environment.NewLine + Environment.NewLine, Vault.PasswordWarning.Select(i => "      " + i));
	}

	private void cancel_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private void ok_Click(object sender, EventArgs e)
	{
		if(string.IsNullOrWhiteSpace(password.Text) || string.IsNullOrWhiteSpace(passwordConfirm.Text))
		{
			MessageBox.Show(this, "Please fill out both fields", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		else if(password.Text != passwordConfirm.Text)
		{
			MessageBox.Show(this, "Password mismatch", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		} 
		else
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);
	}
}
