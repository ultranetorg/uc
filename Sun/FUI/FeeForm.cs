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
using Uccs.Net;

namespace Uccs.Sun.FUI
{
	public partial class FeeForm : Form, IFeeAsker
	{
		Zone Zone;

		public FeeForm(Zone zone)
		{
			Zone = zone;

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

		public bool Ask(Net.Sun sun, AccountKey account, Operation operation)
		{
			from.Text = account.ToString();
			
			var t = new Transaction(Zone){Generator = AccountKey.Create(), Expiration = 0};
			t.AddOperation(operation);
			t.Sign(account, Zone.Cryptography.ZeroHash);

			var f = sun.EstimateFee(t.Operations);

			fee.Text = f.ToString();

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
