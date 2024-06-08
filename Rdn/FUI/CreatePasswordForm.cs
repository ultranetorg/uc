using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using Uccs.Net;

namespace Uccs.Rdn.FUI
{
	public partial class CreatePasswordForm : Form
	{
		public string Password => password.Text;
		
		public CreatePasswordForm()
		{
			InitializeComponent();

			suggestions.Text = string.Join(Environment.NewLine + Environment.NewLine, Vault.PasswordWarning.Select(i => "      " + i));
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
}
