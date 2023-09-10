using Nethereum.Contracts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Web3;
using System.Collections.Generic;
using System.Numerics;

namespace Uccs.Net
{
	public interface IFeeAsker
	{
		bool Ask(Sun sun, AccountAddress account, Operation operation);
	}

	public class SilentFeeAsker : IFeeAsker
	{
		public SilentFeeAsker()
		{
		}

		public bool Ask(Sun sun, AccountAddress account, Operation operation)
		{
			return true;
		}
	}
}
