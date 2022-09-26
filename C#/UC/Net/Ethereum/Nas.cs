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
using System.Threading;
using System.Threading.Tasks;
using UC;

namespace UC.Net
{
	public class Nas : INas
	{
		public const string ContractAddress = "0x72329af958b5679e0354ff12fb27ddbf34d37aca";
		Settings Settings;
		public Web3 Web3;
		Log Log;

		ContractHandler сontract;
		Nethereum.Web3.Accounts.Account account;
		static Dictionary<Zone, List<IPAddress>> Zones = new Dictionary<Zone, List<IPAddress>>();
		static string creator;

		public Nethereum.Signer.Chain Chain => (Chain)Enum.Parse(typeof(Chain), Settings.Nas.Chain);

		public Nethereum.Web3.Accounts.Account Account
		{
			get
			{
				if(account == null)
				{
					if(Settings.Secret?.NasWallet != null && Settings.Secret?.NasPassword != null)
					{
						account = Nethereum.Web3.Accounts.Account.LoadFromKeyStore(File.ReadAllText(Settings.Secret.NasWallet), Settings.Secret.NasPassword);
					}
					else
					{
						account = new Nethereum.Web3.Accounts.Account(EthECKey.GenerateKey().GetPrivateKeyAsBytes());
					}
				}

				return account;
			}
		}

		ContractHandler Contract
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

		public List<IPAddress> GetInitials(Zone zone)
		{
			if(zone == Zone.Localnet)
			{
				return Enumerable.Range(100, 16).Select(i => new IPAddress(new byte[] { 192, 168, 1, (byte)i })).ToList();
			}

			lock(Zones)
			{
				var ips = new List<IPAddress>();

				if(!Zones.ContainsKey(zone))
				{
					var input = new GetZoneFunction { Name = zone.Name };
					var z = Contract.QueryAsync<GetZoneFunction, string>(input).Result;

					foreach(var i in z.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
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

		public async Task SetZone(Zone zone, string nodes, IGasAsker asker)
		{
			var f = new SetZoneFunction
			{
				Name = zone.Name,
				Nodes = nodes
			};

			try
			{
				Log.Report(this, "Setting zone", "Initiated");

				if(asker.Ask(Web3, Contract, Account.Address, f, null))
				{
					f.Gas = asker.Gas;
					f.GasPrice = asker.GasPrice;

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

		public async Task RemoveZone(Zone zone, IGasAsker asker)
		{
			var f = new RemoveZoneFunction
			{
				Name = zone.Name
			};

			if(asker.Ask(Web3, Contract, Account.Address, f, null))
			{
				Log.Report(this, "Removing zone", "Initiated");

				try
				{
					f.Gas = asker.Gas;
					f.GasPrice = asker.GasPrice;

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

		public void Emit(Nethereum.Web3.Accounts.Account source, BigInteger wei, PrivateAccount signer, IGasAsker gasAsker, int eid, Workflow vizor)
		{
			var args = Emission.Serialize(signer, eid);

			var w3 = new Web3(source, Settings.Nas.Provider);
			var c = w3.Eth.GetContractHandler(ContractAddress);

			var rt = new RequestTransferFunction
					 {
					 	AmountToSend = wei,
					 	Secret = args
					 };

			if(gasAsker.Ask(w3, c, source.Address, rt, vizor?.Log))
			{
				rt.Gas = gasAsker.Gas;
				rt.GasPrice = gasAsker.GasPrice;

				vizor?.Log?.Report(this, "Ethereum", "Sending and waiting for a confirmation...");

				var receipt = c.SendRequestAndWaitForReceiptAsync(rt, vizor.Cancellation).Result;

				vizor?.Log?.Report(this, "Ethereum", $"Transaction succeeded. Hash: {receipt.TransactionHash}. Gas: {receipt.CumulativeGasUsed}");
			}
		}

		public BigInteger FinishEmission(Account account, int eid)
		{
			var f = new FindTransferFunction { Secret = Emission.Serialize(account, eid) };

			return Contract.QueryAsync<FindTransferFunction, BigInteger>(f).Result;
		}

		public bool CheckEmission(Emission e)
		{
			var f = new FindTransferFunction { Secret = Emission.Serialize(e.Signer, e.Eid) };

			var wei = Contract.QueryAsync<FindTransferFunction, BigInteger>(f).Result;

			return wei == e.Wei;
		}
	}
}