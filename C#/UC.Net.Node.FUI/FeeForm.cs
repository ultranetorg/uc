using Nethereum.Contracts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using UC.Net;

namespace UC.Net.Node
{
	public partial class FeeForm : Form, IFeeAsker
	{
		Settings Settings;

		public FeeForm(Settings d)
		{
			Settings = d;

			InitializeComponent();
		}

		private void send_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		public bool Ask(Core core, PrivateAccount account, Operation operation)
		{
			from.Text = account.ToString();
			
			var t = new Transaction(core.Settings, account, 0);
			t.Operations.Add(operation);
			t.Sign(new Account(Nethereum.Signer.EthECKey.GenerateKey()), 0);

			var f = core.EstimateFee(t);

			fee.Text = f > 0 ? f.ToHumanString() : "unavailavle"; 

			if(ShowDialog() == DialogResult.OK)
			{
				return true;
			} 
			else
			{
				return false;
			}
		}
	}
}
