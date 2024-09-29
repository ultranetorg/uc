#if ETHEREUM
using System.Net;
using System.Numerics;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Web3;

namespace Uccs.Rdn
{
	public class Ethereum : IEthereum
	{
		public const string						ContractAddress = "0xA755eB16Eb31873dCCAB4135E5659b50e9Addf57";
		RdnSettings								Settings;
		public Web3								Web3;

		ContractHandler							_Contract;
		Nethereum.Web3.Accounts.Account			_Account;
		static Dictionary<McvNet, IPAddress[]>	Nets = new ();
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
					Web3 = new Web3(Account, Settings.Ethereum.Provider);
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

		public Ethereum(RdnSettings s)
		{
			Settings = s;
		}

		public string[] ReportEthereumJsonAPIWarning(string message, bool aserror)
		{
			return [message,
					$"But it is not set or incorrect.",
		 			$"It's located in {Path.Join(Settings.Profile, new RdnSettings().FileName)} -> Ethereum -> Provider",
		 			$"This can be instance of some Ethereum client or third-party services like infura.io or alchemy.com"];
		}

		public EmitFunction EstimateEmission(Nethereum.Web3.Accounts.Account from, BigInteger amount, Flow workflow)
		{
			var w3 = new Web3(from, Settings.Ethereum.Provider);
			var c = w3.Eth.GetContractHandler(ContractAddress);

			var rt = new EmitFunction{AmountToSend = amount,
					 				  Secret = Immission.Serialize(AccountKey.Create(), 0)};

			var g = c.EstimateGasAsync(rt);
			var gp = w3.Eth.GasPrice.SendRequestAsync();

			g.Wait(workflow.Cancellation);
			gp.Wait(workflow.Cancellation);

			rt.Gas		= g.Result;
			rt.GasPrice = gp.Result;

			return rt;
		}

		public TransactionReceipt Emit(Nethereum.Web3.Accounts.Account from, AccountAddress to, BigInteger wei, int eid, BigInteger gas, BigInteger gasprice, Flow workflow)
		{
			var w3 = new Web3(from, Settings.Ethereum.Provider);
			var c = w3.Eth.GetContractHandler(ContractAddress);

			var rt = new EmitFunction{AmountToSend = wei,
					 				  Secret = Immission.Serialize(to, eid)};

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

		public BigInteger FindEmission(AccountAddress account, int eid, Flow workflow)
		{
			var f = new FindEmissionFunction {Secret = Immission.Serialize(account, eid)};

			var c =  Contract.QueryAsync<FindEmissionFunction, BigInteger>(f);
			c.Wait(workflow.Cancellation);
			
			return c.Result;
		}

		public bool IsEmissionValid(Immission e)
		{
	 		try
	 		{
				var f = new FindEmissionFunction {Secret = Immission.Serialize(e.Signer, e.Eid)};

				var wei = Contract.QueryAsync<FindEmissionFunction, BigInteger>(f).Result;

				return wei == e.Wei;
	 		}
	 		catch(Exception)
	 		{
	 		}

			return false;
		}

// 		public IPAddress[] GetInitials(Net net, Workflow workflow)
// 		{
// 			lock(Nets)
// 			{
// 				var ips = new List<IPAddress>();
// 
// 				if(!Nets.ContainsKey(net))
// 				{
// 					try
// 					{
// 						var input = new GetNetFunction{Name = net.Name};
// 						var z = Contract.QueryAsync<GetNetFunction, string>(input).Result;
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
// 						Nets[net] = ips.ToArray();
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
// 						Nets[net] = net.Initials;
// 					}
// 				}
// 
// 				return Nets[net];
// 			}
// 		}

// 		public async Task SetNet(Net net, string nodes, IGasAsker asker)
// 		{
// 			var f = new SetNetFunction
// 			{
// 				Name = net.Name,
// 				Nodes = nodes
// 			};
// 
// 			try
// 			{
// 				Log.Report(this, "Setting net", "Initiated");
// 
// 				if(asker.Ask(Web3, Contract, Account.Address, f, null))
// 				{
// 					f.Gas = asker.Gas;
// 					f.GasPrice = asker.GasPrice;
// 
// 					var r = await Contract.SendRequestAndWaitForReceiptAsync(f);
// 
// 					Log.Report(this, "Setting net", $"Succeeded, Hash={r.TransactionHash}, Gas={r.CumulativeGasUsed}");
// 				}
// 			}
// 			catch(Exception ex)
// 			{
// 				Log.ReportError(this, "Registering a new Node: Failed", ex);
// 				throw ex;
// 			}
// 		}
// 
// 		public async Task RemoveNet(Net net, IGasAsker asker)
// 		{
// 			var f = new RemoveNetFunction
// 			{
// 				Name = net.Name
// 			};
// 
// 			if(asker.Ask(Web3, Contract, Account.Address, f, null))
// 			{
// 				Log.Report(this, "Removing net", "Initiated");
// 
// 				try
// 				{
// 					f.Gas = asker.Gas;
// 					f.GasPrice = asker.GasPrice;
// 
// 					var r = await Contract.SendRequestAndWaitForReceiptAsync(f);
// 
// 					Log.Report(this, "Removing net", $"Succeeded, Hash={r.TransactionHash}, Gas={r.CumulativeGasUsed}");
// 				}
// 				catch(Exception ex)
// 				{
// 					Log.ReportError(this, "Removing net: Failed; ", ex);
// 					throw ex;
// 				}
// 			}
// 		}

	}

	public class HexBigIntegerJsonConverter : JsonConverter<HexBigInteger>
	{
		public override HexBigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return new HexBigInteger(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, HexBigInteger value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.HexValue);
		}
	}

}
#endif