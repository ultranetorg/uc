using System.Windows.Forms;

namespace Uccs.Nexus.Windows;

public partial class EnterPasswordForm : Form
{
	public string Password => password.Text;

	public EnterPasswordForm(string defaultpassword = null)
	{
		InitializeComponent();

		password.Text = defaultpassword;
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

	public static string Ask(string information, IWin32Window owner, string defaultpassword = null)
	{
		var f = new EnterPasswordForm(defaultpassword);

		f.info.Text = information;

		if(f.ShowDialog(owner) == DialogResult.OK)
		{
			return f.password.Text;
		}
		else
		{
			return null;
		}
	}
}
