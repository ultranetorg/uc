using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Numerics;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Web3;

namespace Uccs.Net
{
	public class Nas : INas
	{
		public const string						ContractAddress = "0xA755eB16Eb31873dCCAB4135E5659b50e9Addf57";
		Settings								Settings;
		public Web3								Web3;

		ContractHandler							_Contract;
		Nethereum.Web3.Accounts.Account			_Account;
		static Dictionary<Zone, IPAddress[]>	Zones = new ();
		static string							Creator;

		public Nethereum.Web3.Accounts.Account Account
		{
			get
			{
				if(_Account == null)
				{
					_Account = new Nethereum.Web3.Accounts.Account(EthECKey.GenerateKey().GetPrivateKeyAsBytes());
				}

				return _Account;
			}
		}

		ContractHandler Contract
		{
			get
			{
				if(_Contract == null)
				{
					Web3 = new Web3(Account, Settings.Nas.Provider);
					_Contract = Web3.Eth.GetContractHandler(ContractAddress);
				}

				return _Contract;
			}
		}

		public bool IsAdministrator
		{
			get
			{
				if(Creator == null)
				{
					var input = new CreatorFunction();
					Creator = Contract.QueryAsync<CreatorFunction, string>(input).Result;
				}
				return string.Compare(Account.Address, Creator, true) == 0;
			}
		}

		public Nas(Settings s)
		{
			Settings = s;
		}

		public string[] ReportEthereumJsonAPIWarning(string message, bool aserror)
		{
			return [message,
					$"But it is not set or incorrect.",
		 			$"It's located in {Path.Join(Settings.Profile, Settings.FileName)} -> Nas -> Provider",
		 			$"This can be instance of some Ethereum client or third-party services like infura.io or alchemy.com"];
		}

		public EmitFunction EstimateEmission(Nethereum.Web3.Accounts.Account from, BigInteger amount, Workflow workflow)
		{
			var w3 = new Web3(from, Settings.Nas.Provider);
			var c = w3.Eth.GetContractHandler(ContractAddress);

			var rt = new EmitFunction{AmountToSend = amount,
					 				  Secret = Emission.Serialize(AccountKey.Create(), 0)};

			var g = c.EstimateGasAsync(rt);
			var gp = w3.Eth.GasPrice.SendRequestAsync();

			g.Wait(workflow.Cancellation);
			gp.Wait(workflow.Cancellation);

			rt.Gas		= g.Result;
			rt.GasPrice = gp.Result;

			return rt;
		}

		public TransactionReceipt Emit(Nethereum.Web3.Accounts.Account from, AccountAddress to, BigInteger wei, int eid, BigInteger gas, BigInteger gasprice, Workflow workflow)
		{
			var w3 = new Web3(from, Settings.Nas.Provider);
			var c = w3.Eth.GetContractHandler(ContractAddress);

			var rt = new EmitFunction{AmountToSend = wei,
					 				  Secret = Emission.Serialize(to, eid)};

			rt.Gas = gas;
			rt.GasPrice = gasprice;

			workflow.Log?.Report(this, "Ethereum", "Sending and waiting for a confirmation...");

			var receipt = c.SendRequestAndWaitForReceiptAsync(rt, workflow.Cancellation).Result;

			if(receipt.Status != new HexBigInteger(0))
				workflow.Log?.Report(this, "Ethereum", $"Transaction succeeded. Hash: {receipt.TransactionHash}");
			else
			{
				workflow.Log?.Report(this, "Ethereum", $"Transaction FAILED. Hash: {receipt.TransactionHash}");
				throw new EntityException(EntityError.EmissionFailed);
			}
				
			return receipt;
		}

		public BigInteger FindEmission(AccountAddress account, int eid, Workflow workflow)
		{
			var f = new FindEmissionFunction {Secret = Emission.Serialize(account, eid)};

			var c =  Contract.QueryAsync<FindEmissionFunction, BigInteger>(f);
			c.Wait(workflow.Cancellation);
			
			return c.Result;
		}

		public bool IsEmissionValid(Emission e)
		{
	 		try
	 		{
				var f = new FindEmissionFunction {Secret = Emission.Serialize(e.Signer, e.Eid)};

				var wei = Contract.QueryAsync<FindEmissionFunction, BigInteger>(f).Result;

				return wei == e.Wei;
	 		}
	 		catch(Exception)
	 		{
	 		}

			return false;
		}

// 		public IPAddress[] GetInitials(Zone zone, Workflow workflow)
// 		{
// 			lock(Zones)
// 			{
// 				var ips = new List<IPAddress>();
// 
// 				if(!Zones.ContainsKey(zone))
// 				{
// 					try
// 					{
// 						var input = new GetZoneFunction{Name = zone.Name};
// 						var z = Contract.QueryAsync<GetZoneFunction, string>(input).Result;
// 	
// 						foreach(var i in z.Split(new char[]{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries))
// 						{
// 							if(!IPAddress.TryParse(i, out var ip))
// 							{
// 								try
// 								{
// 									var r = Dns.GetHostEntry(i);
// 									ip = r.AddressList.First();
// 								}
// 								catch(SocketException ex)
// 								{
// 									workflow?.Log.ReportError(this, $"Can't DNS resolve - {i}", ex);
// 									continue;
// 								}
// 							}
// 	
// 							ips.Add(ip);
// 						}
// 
// 						Zones[zone] = ips.ToArray();
// 					}
// 					catch(Exception ex) when (ex is not RequirementException)
// 					{
// 		  				try
// 		  				{
// 		 					new Uri(Settings.Nas.Provider);
// 		  				}
// 		  				catch(Exception)
// 		  				{
// 							ReportEthereumJsonAPIWarning($"Ethereum Json-API provider required to get intial nodes.", false);
// 		  				}
// 
// 					}
// 
// 					if(!ips.Any())
// 					{
// 						workflow?.Log.ReportWarning(this, "Can't retrieve initial peers from Ethereum. Predefined ones are used.");
// 						Zones[zone] = zone.Initials;
// 					}
// 				}
// 
// 				return Zones[zone];
// 			}
// 		}

// 		public async Task SetZone(Zone zone, string nodes, IGasAsker asker)
// 		{
// 			var f = new SetZoneFunction
// 			{
// 				Name = zone.Name,
// 				Nodes = nodes
// 			};
// 
// 			try
// 			{
// 				Log.Report(this, "Setting zone", "Initiated");
// 
// 				if(asker.Ask(Web3, Contract, Account.Address, f, null))
// 				{
// 					f.Gas = asker.Gas;
// 					f.GasPrice = asker.GasPrice;
// 
// 					var r = await Contract.SendRequestAndWaitForReceiptAsync(f);
// 
// 					Log.Report(this, "Setting zone", $"Succeeded, Hash={r.TransactionHash}, Gas={r.CumulativeGasUsed}");
// 				}
// 			}
// 			catch(Exception ex)
// 			{
// 				Log.ReportError(this, "Registering a new Node: Failed", ex);
// 				throw ex;
// 			}
// 		}
// 
// 		public async Task RemoveZone(Zone zone, IGasAsker asker)
// 		{
// 			var f = new RemoveZoneFunction
// 			{
// 				Name = zone.Name
// 			};
// 
// 			if(asker.Ask(Web3, Contract, Account.Address, f, null))
// 			{
// 				Log.Report(this, "Removing zone", "Initiated");
// 
// 				try
// 				{
// 					f.Gas = asker.Gas;
// 					f.GasPrice = asker.GasPrice;
// 
// 					var r = await Contract.SendRequestAndWaitForReceiptAsync(f);
// 
// 					Log.Report(this, "Removing zone", $"Succeeded, Hash={r.TransactionHash}, Gas={r.CumulativeGasUsed}");
// 				}
// 				catch(Exception ex)
// 				{
// 					Log.ReportError(this, "Removing zone: Failed; ", ex);
// 					throw ex;
// 				}
// 			}
// 		}

	}
}