﻿using Nethereum.Contracts;
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

		public bool Ask(Core core, AccountKey account, Operation operation)
		{
			from.Text = account.ToString();
			
			var t = new Transaction(Zone);
			t.AddOperation(operation);
			t.Sign(account, new AccountAddress(Nethereum.Signer.EthECKey.GenerateKey()), 0);

			var f = core.EstimateFee(t.Operations);

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