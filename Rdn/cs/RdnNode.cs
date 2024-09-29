using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using DnsClient;
using RocksDbSharp;

namespace Uccs.Rdn
{
	public enum RdnPeerCallClass : byte
	{
		None = 0, 
		Domain = McvPeerCallClass._Last + 1, 
		RdnMembers,
		QueryResource, Resource, DeclareRelease, LocateRelease, FileInfo, DownloadRelease, Cost
	}

	public class RdnNode : McvNode
	{
		public override long					Roles => Mcv == null ? 0 : Mcv.Settings.Roles;
		public new Rdn					Net => base.Net as Rdn;
		public new RdnMcv						Mcv => base.Mcv as RdnMcv;
		public new RdnSettings					Settings => base.Settings as RdnSettings;

		LookupClient							Dns = new LookupClient(new LookupClientOptions {Timeout = TimeSpan.FromSeconds(5)});
		HttpClient								Http = new HttpClient();

		public ResourceHub						ResourceHub;
		public PackageHub						PackageHub;
		public SeedHub							SeedHub;

		public RdnNode(string name, Guid id, string profile, RdnSettings settings, string packagespath, Vault vault, IClock clock, Flow flow) : base(name, Rdn.ById(id), settings ?? new RdnSettings(Path.Join(profile, id.ToString())), vault, flow)
		{
			Flow.Log?.Report(this, $"Net: {Net.Name}");
		
			if(Settings.Api != null)
			{
				ApiServer = new RdnApiServer(this, Flow);
			}

			if(Settings.Seed != null)
			{
				ResourceHub = new ResourceHub(this, Net, Settings.Seed);
				PackageHub = new PackageHub(this, Settings.Seed){DeploymentPath = packagespath};
			}

			if(Settings.Base != null)
			{
				base.Mcv = new RdnMcv(	this, 
										Settings,
										Path.Join(Settings.Profile, "Mcv"),
										flow,
										clock ?? new RealClock());

				Mcv.Commited += r => {
										if(Mcv.LastConfirmedRound.Members.Any(i => Settings.Generators.Contains(i.Address)))
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
	
																			lock(Lock)
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

				if(Settings.Generators.Any())
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

			RunPeer();

		}

		public override string ToString()
		{
			return string.Join(", ", new string[]{	GetType().Name,
													Name,
													(Settings.Api != null ? "A" : null) +
													(Settings.Base != null ? "B" : null) +
													(Settings.Base?.Chain != null  ? "C" : null) +
													(Settings is RdnSettings x && x.Seed != null  ? "S" : null),
													Connections.Count() < Settings.Peering.PermanentMin ? "Low Peers" : null,
													Mcv != null ? $"{Synchronization}/{Mcv.LastConfirmedRound?.Id}/{Mcv.LastConfirmedRound?.Hash.ToHexPrefix()}" : null,
													$"T(i/o)={IncomingTransactions.Count}/{OutgoingTransactions.Count}"}
						.Where(i => !string.IsNullOrWhiteSpace(i)));
		}

		protected override void CreateTables(ColumnFamilies columns)
		{
			columns.Add(new (ResourceHub.ReleaseFamilyName,	new ()));
			columns.Add(new (ResourceHub.ResourceFamilyName,new ()));
		}

		public override void RunPeer()
		{
			base.RunPeer();

			if(Settings.Seed != null)
			{
				ResourceHub.RunDeclaring();
			}
		}

		public override object Constract(Type t, byte b)
		{
			if(t == typeof(VersionManifest))		return new VersionManifest();

			return base.Constract(t, b);
		}

		private bool IsDnsValid(DomainMigration am)
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
					using(var m = new HttpRequestMessage(HttpMethod.Get, $"https://www.googleapis.com/customsearch/v1?key={Mcv.Settings.GoogleApiKey}&cx={Mcv.Settings.GoogleSearchEngineID}&q={am.Name}&start=10"))
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

		public override bool ProcessIncomingOperation(Operation o)
		{
			#if ETHEREUM
			if(o is Immission e && !Ethereum.IsEmissionValid(e))
				return false;
			#endif

			if(o is DomainMigration m && !IsDnsValid(m))
				return false;

			return true;
		}

		public override void OnRequestException(Peer peer, NodeException ex)
		{
			base.OnRequestException(peer, ex);

			if(ex.Error == NodeError.NotSeed)	peer.Roles  &= ~(long)RdnRole.Seed;
		}

	}
}
