using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Signer;

namespace UC.Net
{
	public interface INas
	{
		Nethereum.Web3.Accounts.Account Account { get; }
		Chain							Chain { get; }
		bool							IsAdministrator { get; }

		bool							CheckEmission(Emission e);
		Task							Emit(Nethereum.Web3.Accounts.Account source, BigInteger wei, PrivateAccount signer, IGasAsker gasAsker, int eid, IFlowControl flowcontrol = null, CancellationTokenSource cts = null);
		BigInteger						FinishEmission(Account account, int eid);
		List<IPAddress>					GetInitials(string zone);
		string							GetZone(string name);
		Task							RemoveZone(string name, IGasAsker asker);
		Task							SetZone(string name, string nodes, IGasAsker asker);
	}
}