using System.Text.RegularExpressions;
using DnsClient;
using Uccs.Nexus;

namespace Uccs.Rdn;

[Flags]
public enum RdnRole : uint
{
	None,
	Seed = 0b00000100,
}

public class RdnNode : McvNode
{
	new public RdnTcpPeering		Peering => base.Peering as RdnTcpPeering;
	new public RdnMcv				Mcv => base.Mcv as RdnMcv;
	public new Rdn					Net => base.Net as Rdn;
	public new RdnNodeSettings		Settings => base.Settings as RdnNodeSettings;

	LookupClient					Dns = new LookupClient(new LookupClientOptions {Timeout = TimeSpan.FromSeconds(5)});

	public ResourceHub				ResourceHub;
	public SeedHub					SeedHub;
	public JsonServer				ApiServer;
	//public RdnNnTcpPeering			NnPeering;
	IppConnection					NnConnection;

	public RdnNode(string name, Zone zone, string profile, NexusSettings nexussettings, RdnNodeSettings settings, IClock clock, Flow flow) : base(name, Rdn.ByZone(zone), profile, nexussettings, flow)
	{
		base.Settings = settings ?? new RdnNodeSettings(profile);

		if(Flow.Log != null)
			new FileLog(Flow.Log, GetType().Name, Settings.Profile);

		if(NodeGlobals.Any)
			Flow.Log?.ReportWarning(this, $"Dev: {NodeGlobals.AsString}");

		InitializeVaultApi(NexusSettings.Host);

		if(Settings.Mcv != null)
		{
			base.Mcv = new RdnMcv(Net, Settings.Mcv, Settings.DataPath ?? ExeDirectory, Path.Join(profile, "Mcv"), [Settings.Peering.EP], [Settings.Peering.EP], clock ?? new RealClock());

			Mcv.Confirmed += r =>	{
										if(Mcv.LastConfirmedRound.Members.Any(i => Settings.Mcv.Generators.Any(g => g.Signer == i.Address)))
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

			//if(Settings.NnPeering != null)
			{
				//NnPeering = new RdnNnTcpPeering(this, Settings.NnPeering, 0, flow);
			}
		}
		
		NnConnection = new RdnNnpIppConnection(this, flow);

		base.Peering = new RdnTcpPeering(this, Settings.Peering, Settings.Roles, VaultApi, flow, clock);

		if(Settings.Seed != null)
		{
			ResourceHub = new ResourceHub(this, Net, Settings.Seed);
			ResourceHub.RunDeclaring();
		}
		
		ApiServer = new RdnApiServer(this, (Settings.Api ?? new ()).ToApiSettings(Net), Flow);
	}

	public override string ToString()
	{
		string f()=> string.Join(", ", new string[]{GetType().Name,
													Name,
													(ApiServer != null ? "A" : null) +
													(Settings.Mcv != null ? "G" : null) +
													(Settings.Mcv?.Chain != null  ? "C" : null) +
													(Peering.Synchronization == Synchronization.Synchronized && Mcv.NextVotingRound.VotersRound.Members.Any(i => Settings.Mcv.Generators.Any(g => g.Signer == i.Address)) ? "M" : null) +
													(Settings.Seed != null  ? "S" : null),
													Peering.Connections.Count() < Settings.Peering.PermanentMin ? "Low Peers" : null,
													Mcv != null ? $"{Peering.Synchronization}{(Peering.SynchronizationInfo != null ? $"-{Peering.SynchronizationInfo}" : null)}/{Mcv.LastConfirmedRound?.Id}/{Mcv.LastConfirmedRound?.Hash.ToHexPrefix()}" : null,
													$"T(i/o)={Peering.CandidateTransactions.Count}/{Peering.OutgoingTransactions.Count}"}
						.Where(i => !string.IsNullOrWhiteSpace(i)));

		if(Mcv != null)
		{
			lock(Mcv.Lock)
				return f();
		} 
		else
			return f();
	}

	public override void Stop()
	{
		Flow.Abort();

		ApiServer?.Stop();
		Peering.Stop();
		NnConnection?.Disconnect();
		//NnPeering?.Stop();
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

			//if(am.RankCheck)
			//{
			//	using(var m = new HttpRequestMessage(HttpMethod.Get, $"https://www.googleapis.com/customsearch/v1?key={Settings.GoogleApiKey}&cx={Settings.GoogleSearchEngineID}&q={am.Name}&start=10"))
			//	{
			//		var cr = Http.Send(m, Flow.Cancellation);
			//
			//		if(cr.StatusCode == HttpStatusCode.OK)
			//		{
			//			JsonElement j = JsonSerializer.Deserialize<dynamic>(cr.Content.ReadAsStringAsync().Result);
			//
			//			var domains = j.GetProperty("items").EnumerateArray().Select(i => new Uri(i.GetProperty("link").GetString()).Host.Split('.').TakeLast(2));
			//
			//			return domains.FirstOrDefault(i => i.First() == am.Name)?.Last() == am.Tld;
			//		}
			//	}
			//}
		}
		catch(Exception)
		{
		}

		return false;
	}

}
