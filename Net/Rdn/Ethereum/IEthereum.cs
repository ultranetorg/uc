﻿using System.Numerics;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;

namespace Uccs.Net
{
	public interface IEthereum
	{
		Nethereum.Web3.Accounts.Account			Account { get; }
		bool									IsAdministrator { get; }

		bool									IsEmissionValid(Emission e);
		
		public EmitFunction						EstimateEmission(Nethereum.Web3.Accounts.Account from, BigInteger amount, Flow workflow);
		public TransactionReceipt				Emit(Nethereum.Web3.Accounts.Account from, AccountAddress to, BigInteger wei, int eid, BigInteger gas, BigInteger gasprice, Flow workflow);
		
		BigInteger								FindEmission(AccountAddress account, int eid, Flow workflow);
		string[]								ReportEthereumJsonAPIWarning(string message, bool aserror);
	}
}