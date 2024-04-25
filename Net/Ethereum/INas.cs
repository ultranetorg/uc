using System.Collections.Generic;
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
		BigInteger						FindEmission(AccountAddress account, int eid, Workflow workflow);
		string[]						ReportEthereumJsonAPIWarning(string message, bool aserror);
	}
}