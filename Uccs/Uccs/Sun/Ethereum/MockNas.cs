﻿using System;
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
		public bool								IsAdministrator => true;

		public void Emit(Nethereum.Web3.Accounts.Account source, BigInteger wei, AccountKey signer, IGasAsker gasAsker, int eid, Workflow vizor)
		{
		}

		public bool CheckEmission(Emission e)
		{
			return true;
		}

		public BigInteger FinishEmission(AccountAddress account, int eid)
		{
			throw new NotImplementedException();
		}

		public IPAddress[] GetInitials(Zone zone)
		{
			return zone.Initials ?? throw new NotSupportedException();
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

		public void ReportEthereumJsonAPIWarning(string message, bool aserror)
		{
		}
	}
}