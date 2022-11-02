using Nethereum.Contracts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using System;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using UC.Net;
using System.Linq;

namespace UC.Sun.FUI
{
	public partial class PasswordForm : Form, IPasswordAsker
	{
		public string Password => password.Text;

		public PasswordForm(string defaultpassword)
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

		public bool Ask(string information)
		{
			info.Text = information;

			if(ShowDialog() == DialogResult.OK)
			{
				return true;
			} 
			else
			{
				return false;
			}
		}

		public void	ShowError(string message)
		{
			MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}
