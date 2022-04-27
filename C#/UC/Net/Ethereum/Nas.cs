using Nethereum.Contracts.ContractHandlers;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading.Tasks;
using UC;

namespace UC.Net
{
	public class Nas
	{
		public const string							ContractAddress = "0x72329af958b5679e0354ff12fb27ddbf34d37aca";
		Settings									Settings;
		public Web3									Web3;
		Log											Log;

		ContractHandler								сontract;
		Nethereum.Web3.Accounts.Account				account;
		static Dictionary<string, List<IPAddress>>	Zones = new Dictionary<string, List<IPAddress>>();
		static string								creator;

		public Nethereum.Signer.Chain				Chain => (Chain)Enum.Parse(typeof(Chain), Settings.Nas.Chain);

		public Nethereum.Web3.Accounts.Account Account
		{
			get
			{
				if(account == null)
				{
					if(Settings.Secret?.NasWallet != null && Settings.Secret?.NasPassword != null)
					{
						account = Nethereum.Web3.Accounts.Account.LoadFromKeyStore(File.ReadAllText(Settings.Secret.NasWallet),  Settings.Secret.NasPassword);
					} 
					else
					{
						account = new Nethereum.Web3.Accounts.Account(EthECKey.GenerateKey().GetPrivateKeyAsBytes());
					}
				}

				return account;
			}
		}

		public ContractHandler Contract
		{
			get
			{
				if(сontract == null)
				{
					Web3 = new Web3(Account, Settings.Nas.Provider);
					сontract = Web3.Eth.GetContractHandler(ContractAddress);
				}

				return сontract;
			}
		}
			
		public bool IsAdministrator
		{
			get
			{
				if(creator == null)
				{
					var input = new CreatorFunction();
					creator = Contract.QueryAsync<CreatorFunction, string>(input).Result;
				}
				return string.Compare(Account.Address, creator, true) == 0;
			}
		}

		public Nas(Settings s, Log log)
		{
			Log = log;
			Settings = s;
		}

		public List<IPAddress> GetInitials(string zone)
		{
			if(zone == Zone.Localnet)
			{
				return Enumerable.Range(100, 16).Select(i => new IPAddress(new byte[] {192, 168, 1, (byte)i})).ToList();
			}

			lock(Zones)
			{
				var ips = new List<IPAddress>();
	
				if(!Zones.ContainsKey(zone))
				{
					var z = GetZone(Settings.Zone);

					foreach(var i in z.Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries))
					{
						if(!IPAddress.TryParse(i, out var ip))
						{
							try
							{
								var r = Dns.GetHostEntry(i);
								ip = r.AddressList.First();
							}
							catch(SocketException ex)
							{
								Log.ReportError(this, $"Peer: {i} - {ex.Message}");
								continue;
							}
						}

						ips.Add(ip);
					}

					if(ips.Any())
					{
						Zones[zone] = ips;
					}
						
					return ips;
				}
				else
				{
					return Zones[zone];
				}
			}
		}

		public string GetZone(string name)
		{
			var input = new GetZoneFunction {Name = name};
			return Contract.QueryAsync<GetZoneFunction, string>(input).Result;
		}

/*
		public List<string> LoadNodes()
		{
			var r = new List<string>();

			try
			{
				var input = new GetNodesFunction();
				var ids = ContractHandler.QueryDeserializingToObjectAsync<GetNodesFunction, GetNodesOutputDTO>(input).Result;
								

				for(BigInteger i=0; i<ids.Indexes.Count; i++)
				{
					var fi = new GetNodeFunction(){ Index = i };
					var node = ContractHandler.QueryDeserializingToObjectAsync<GetNodeFunction, GetNodeOutputDTO>(fi).Result;

					r.Add(node.Name);
				}

			}
			catch(Exception ex)
			{
				Log.ReportError(ex);
			}

			return r;
		}*/

		public async Task SetZone(string name, string nodes, IGasAsker asker)
		{
			var f = new SetZoneFunction
					{
					 	Name = name, Nodes = nodes
					};

			try
			{
				Log.Report(this, "Setting zone", "Initiated");

				if(asker.Ask(Web3, Contract, Account.Address, f, null))
				{
					f.Gas		= asker.Gas;
					f.GasPrice	= asker.GasPrice;

					var r = await Contract.SendRequestAndWaitForReceiptAsync(f);

					Log.Report(this, "Setting zone", $"Succeeded, Hash={r.TransactionHash}, Gas={r.CumulativeGasUsed}");
				}
			}
			catch(Exception ex)
			{
				Log.ReportError(this, "Registering a new Node: Failed", ex);
				throw ex;
			}
		}

		public async Task RemoveZone(string name, IGasAsker asker)
		{
			var f = new RemoveZoneFunction
					{
						Name = name
					};

			if(asker.Ask(Web3, Contract, Account.Address, f, null))
			{
				Log.Report(this, "Removing zone", "Initiated");

				try
				{
					f.Gas		= asker.Gas;
					f.GasPrice	= asker.GasPrice;

					var r = await Contract.SendRequestAndWaitForReceiptAsync(f);

					Log.Report(this, "Removing zone", $"Succeeded, Hash={r.TransactionHash}, Gas={r.CumulativeGasUsed}");
				}
				catch(Exception ex)
				{
					Log.ReportError(this, "Removing zone: Failed; ", ex);
					throw ex;
				}
			}
		}

		public BigInteger FindTransfer(Account account, int eid)
		{
			var f = new FindTransferFunction {Secret = Emission.Serialize(account, eid)};
			
			return Contract.QueryAsync<FindTransferFunction, BigInteger>(f).Result;
					
		}
	}
}