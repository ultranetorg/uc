﻿using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Signer;

namespace Uccs.Net
{
	public interface INas
	{
		Nethereum.Web3.Accounts.Account Account { get; }
		bool							IsAdministrator { get; }

		bool							CheckEmission(Emission e);
		void							Emit(Nethereum.Web3.Accounts.Account source, BigInteger wei, AccountKey signer, IGasAsker gasAsker, int eid, Workflow flowcontrol = null);
		BigInteger						FinishEmission(AccountAddress account, int eid);
		void							ReportEthereumJsonAPIWarning(string message, bool aserror);
	}
}