using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Signer;

namespace UC.Net
{
	public class MockNas : INas
	{
		public Nethereum.Web3.Accounts.Account	Account { get => null; }
		public Chain							Chain { get => Chain.Ropsten; }
		public bool								IsAdministrator { get => throw new NotImplementedException(); }

		public async Task Emit(Nethereum.Web3.Accounts.Account source, BigInteger wei, PrivateAccount signer, IGasAsker gasAsker, int eid, IFlowControl flowcontrol = null, CancellationTokenSource cts = null)
		{
			await Task.CompletedTask;
		}

		public bool CheckEmission(Emission e)
		{
			return true;
		}

		public BigInteger FinishEmission(Account account, int eid)
		{
			throw new NotImplementedException();
		}

		public List<IPAddress> GetInitials(string zone)
		{
			if(zone == Zone.Localnet)
			{
				return Enumerable.Range(100, 16).Select(i => new IPAddress(new byte[] { 192, 168, 1, (byte)i })).ToList();
			}
		
			throw new NotImplementedException();
		}

		public string GetZone(string name)
		{
			throw new NotImplementedException();
		}

		public Task RemoveZone(string name, IGasAsker asker)
		{
			throw new NotImplementedException();
		}

		public Task SetZone(string name, string nodes, IGasAsker asker)
		{
			throw new NotImplementedException();
		}
	}
}
