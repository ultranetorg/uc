using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using DnsClient;
using RocksDbSharp;

namespace Uccs.Rdn
{
	public class RdnNode : McvNode
	{
		new public RdnTcpPeering		Peering => base.Peering as RdnTcpPeering;
		new public RdnMcv				Mcv => base.Mcv as RdnMcv;
		public RdnNodeSettings			Settings;

		LookupClient					Dns = new LookupClient(new LookupClientOptions {Timeout = TimeSpan.FromSeconds(5)});
		HttpClient						Http = new HttpClient();

		public ResourceHub				ResourceHub;
		public PackageHub				PackageHub;
		public SeedHub					SeedHub;
		public JsonServer				ApiServer;

		public RdnNode(string name, Rdn net, string profile, RdnNodeSettings settings, string deploymentpath, Vault vault, IClock clock, Flow flow) : base(name, net, profile, flow, vault)
		{
			Settings = settings ?? new RdnNodeSettings(Path.Join(profile, net.Address));

			if(Flow.Log != null)
			{
				new FileLog(Flow.Log, net.Address, Settings.Profile);
			}

			if(NodeGlobals.Any)
				Flow.Log?.ReportWarning(this, $"Dev: {NodeGlobals.AsString}");

			if(Settings.Api != null)
			{
				ApiServer = new RdnApiServer(this, Settings.Api, Flow);
			}


			if(Settings.Mcv != null)
			{
				base.Mcv = new RdnMcv(net, Settings.Mcv, Path.Join(Settings.Profile, "Mcv"), [Settings.Peering.IP], [Settings.Peering.IP], clock ?? new RealClock());

				Mcv.Commited += r => {
										if(Mcv.LastConfirmedRound.Members.Any(i => Settings.Mcv.Generators.Contains(i.Address)))
										{
											var ops = r.ConsensusTransactions.SelectMany(t => t.Operations).ToArray();
													
											foreach(var o in ops)
											{
												if(o is DomainMigration am)
												{
		 											if(!NodeGlobals.SkipMigrationVerification)
		 											{
														Task.Run(() =>	{
																			var approved = IsDnsValid(am);
	
																			lock(Mcv.Lock)
																				Mcv.ApprovedMigrations.Add(new ForeignResult {OperationId = am.Id, Approved = approved});
																		});
		 											}
													else
														Mcv.ApprovedMigrations.Add(new ForeignResult {OperationId = am.Id, Approved = true});
												}
		
												#if IMMISION
												if(o is Immission e)
												{
													Task.Run(() =>	{
																		var v = Ethereum.IsEmissionValid(e);
	
																		lock(Lock)
																			Mcv.ApprovedEmissions.Add(new ForeignResult {OperationId = e.Id, Approved = v});
																	});
												}
												#endif
											}
										}
	
										//Mcv.ApprovedEmissions.RemoveAll(i => (r as RdnRound).ConsensusEmissions.Any(j => j.OperationId == i.OperationId) || r.Id > i.OperationId.Ri + Net.ExternalVerificationRoundDurationLimit);
										Mcv.ApprovedMigrations.RemoveAll(i => (r as RdnRound).ConsensusMigrations.Any(j => j.OperationId == i.OperationId) || r.Id > i.OperationId.Ri + Net.ExternalVerificationRoundDurationLimit);
									};


				if(Settings.Mcv.Generators.Any())
				{
					#if ETHEREUM
		  			try
		  			{
		 				new Uri(Settings.Ethereum.Provider);
		  			}
		  			catch(Exception)
		  			{
		  				Ethereum.ReportEthereumJsonAPIWarning($"Ethereum Json-API provider required to run the node as a generator.", true);
						return;
		  			}
					#endif

					SeedHub = new SeedHub(Mcv);
				}
			}

			base.Peering = new RdnTcpPeering(this, Settings.Peering, Settings.Roles, vault, flow, clock);

			if(Settings.Seed != null)
			{
				if(!Database.TryGetColumnFamily(ResourceHub.ReleaseFamilyName, out var cf))
					Database.CreateColumnFamily(new (), ResourceHub.ReleaseFamilyName);

				if(!Database.TryGetColumnFamily(ResourceHub.ResourceFamilyName, out cf))
					Database.CreateColumnFamily(new (), ResourceHub.ResourceFamilyName);

				ResourceHub = new ResourceHub(this, Net, Settings.Seed);
				PackageHub = new PackageHub(this, Settings.Seed, deploymentpath);

				ResourceHub.RunDeclaring();
			}
		}

		public override string ToString()
		{
			return string.Join(", ", new string[]{	GetType().Name,
													Name,
													(Settings.Api != null ? "A" : null) +
													(Settings.Mcv != null ? "B" : null) +
													(Settings.Mcv?.Chain != null  ? "C" : null) +
													(Settings.Seed != null  ? "S" : null),
													Peering.Connections.Count() < Settings.Peering.PermanentMin ? "Low Peers" : null,
													Mcv != null ? $"{Peering.Synchronization}/{Mcv.LastConfirmedRound?.Id}/{Mcv.LastConfirmedRound?.Hash.ToHexPrefix()}" : null,
													$"T(i/o)={Peering.IncomingTransactions.Count}/{Peering.OutgoingTransactions.Count}"}
						.Where(i => !string.IsNullOrWhiteSpace(i)));
		}

		public override void Stop()
		{
			Flow.Abort();

			ApiServer?.Stop();
			Peering.Stop();
			Mcv?.Stop();

			base.Stop();
		}

//		protected override void CreateTables(ColumnFamilies columns)
//		{
//			base.CreateTables(columns);
//
//			columns.Add(new (ResourceHub.ReleaseFamilyName,	new ()));
//			columns.Add(new (ResourceHub.ResourceFamilyName,new ()));
//		}
//
		public bool IsDnsValid(DomainMigration am)
		{
			try
			{
				var result = Dns.QueryAsync(am.Name + '.' + am.Tld, QueryType.TXT, QueryClass.IN, Flow.Cancellation);

				var txt = result.Result.Answers.TxtRecords().FirstOrDefault(r => r.DomainName == am.Name + '.' + am.Tld + '.');

				if(txt != null && txt.Text.Any(i => Regex.Match(i, "0[xX][0-9a-fA-F]{40}").Success && AccountAddress.Parse(i) == am.Transaction.Signer))
				{
					return true;
				}

				if(am.RankCheck)
				{
					using(var m = new HttpRequestMessage(HttpMethod.Get, $"https://www.googleapis.com/customsearch/v1?key={Settings.GoogleApiKey}&cx={Settings.GoogleSearchEngineID}&q={am.Name}&start=10"))
					{
						var cr = Http.Send(m, Flow.Cancellation);

						if(cr.StatusCode == HttpStatusCode.OK)
						{
							JsonElement j = JsonSerializer.Deserialize<dynamic>(cr.Content.ReadAsStringAsync().Result);

							var domains = j.GetProperty("items").EnumerateArray().Select(i => new Uri(i.GetProperty("link").GetString()).Host.Split('.').TakeLast(2));

							return domains.FirstOrDefault(i => i.First() == am.Name)?.Last() == am.Tld;
						}
					}
				}
			}
			catch(Exception)
			{
			}

			return false;
		}

	}
}
