using Nethereum.Contracts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Web3;
using System.Collections.Generic;
using System.Numerics;

namespace UC.Net
{
	public interface IFeeAsker
	{
		bool Ask(Dispatcher dispatcher, PrivateAccount account, Operation operation);
	}

	public class SilentFeeAsker : IFeeAsker
	{
		public SilentFeeAsker()
		{
		}

		public bool Ask(Dispatcher dispatcher, PrivateAccount account, Operation operation)
		{
			return true;
		}
	}
}
