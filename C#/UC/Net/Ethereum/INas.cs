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
		bool							IsAdministrator { get; }

		bool							CheckEmission(Emission e);
		void							Emit(Nethereum.Web3.Accounts.Account source, BigInteger wei, PrivateAccount signer, IGasAsker gasAsker, int eid, Workflow flowcontrol = null);
		BigInteger						FinishEmission(Account account, int eid);
		IPAddress[]						GetInitials(Zone zone);
		Task							RemoveZone(Zone zone, IGasAsker asker);
		Task							SetZone(Zone zone, string nodes, IGasAsker asker);
		void							ReportEthereumJsonAPIWarning(string message, bool aserror);
	}
}