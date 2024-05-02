using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;

namespace Uccs.Net
{
	public class MockNas : INas
	{
		public Nethereum.Web3.Accounts.Account	Account { get => null; }
		public Chain							Chain { get => Chain.Ropsten; }
		public bool								IsAdministrator => true;

		string									Workpath;

		public MockNas(string workpath)
		{
			Workpath = workpath;
		}

		public EmitFunction EstimateEmission(Nethereum.Web3.Accounts.Account from, BigInteger amount, Flow workflow)
		{
			return new EmitFunction {Gas = 0, GasPrice = 0};
		}

		public TransactionReceipt Emit(Nethereum.Web3.Accounts.Account from, AccountAddress to, BigInteger wei, int eid, BigInteger gas, BigInteger gasprice, Flow workflow)
		{
			if(FindEmission(to, eid, workflow) != 0)
				throw new EntityException(EntityError.EmissionFailed);

			File.WriteAllText(Path.Join(Workpath, $"{to}.{eid}"), wei.ToString());
			return new TransactionReceipt {};
		}

		public bool IsEmissionValid(Emission e)
		{
			return true;
		}

		public BigInteger FindEmission(AccountAddress to, int eid, Flow workflow)
		{
			var p = Path.Join(Workpath, $"{to}.{eid}");

			if(!File.Exists(p))
			{
				return 0;
			}

			return BigInteger.Parse(File.ReadAllText(p));
		}

		public string GetZone(Zone zone)
		{
			throw new NotImplementedException();
		}

		public Task RemoveZone(Zone zone, IGasAsker asker)
		{
			throw new NotImplementedException();
		}

		public Task SetZone(Zone zone, string nodes, IGasAsker asker)
		{
			throw new NotImplementedException();
		}

		public string[] ReportEthereumJsonAPIWarning(string message, bool aserror)
		{
			return null;
		}
	}
}
