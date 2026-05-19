using System.ComponentModel;
using System.Windows.Forms;

namespace Uccs.Nexus.Windows;

public partial class RdnRecipientForm : Form
{
	public RdnRecipientForm()
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
		DialogResult = DialogResult.OK;
		Close();
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);
	}

	private void Asset_SelectionChangeCommitted(object sender, EventArgs e)
	{

	}
}
