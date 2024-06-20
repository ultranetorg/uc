using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DnsClient;

namespace Uccs.Net
{
	public class Rdn : McvNode
	{
		public override long					Roles => Mcv.Settings.Roles;
		public new RdnZone						Zone => base.Zone as RdnZone;
		public new RdnMcv						Mcv => base.Mcv as RdnMcv;
		public new RdnSettings					Settings => base.Settings as RdnSettings;

		public IEthereum						Ethereum;
		LookupClient							Dns = new LookupClient(new LookupClientOptions {Timeout = TimeSpan.FromSeconds(5)});
		HttpClient								Http = new HttpClient();

		public ResourceHub						ResourceHub;
		public PackageHub						PackageHub;
		public SeedHub							SeedHub;


		public Rdn(string name, Guid zoneid, string profile, RdnSettings settings, Vault vault, IEthereum ethereum, IClock clock, Flow flow) : base(name, settings ?? new RdnSettings(Path.Join(profile, zoneid.ToString())), vault, flow)
		{
			base.Zone = RdnZone.ById(zoneid);
			Ethereum = ethereum ?? new Ethereum(Settings);

			Flow.Log?.Report(this, $"Zone: {Zone.Name}");
		
			if(Settings.Api != null)
			{
				ApiServer = new RdnApiServer(this, Flow);
			}

			base.Mcv = new RdnMcv(	this, 
									Settings,
									Path.Join(Settings.Profile, "Mcv"),
									flow,
									Ethereum,
									clock ?? new RealClock());

			if(Settings.Seed != null)
			{
				ResourceHub = new ResourceHub(this, Zone, Settings.Seed);
				PackageHub = new PackageHub(this, Settings.Seed);
			}

			if(Settings.Generators.Any())
			{
		  		try
		  		{
		 			new Uri(Settings.Ethereum.Provider);
		  		}
		  		catch(Exception)
		  		{
		  			Ethereum.ReportEthereumJsonAPIWarning($"Ethereum Json-API provider required to run the node as a generator.", true);
					return;
		  		}

				SeedHub = new SeedHub(Mcv);
			}

			Mcv.Commited += r => {
									if(Mcv.LastConfirmedRound.Members.Any(i => Settings.Generators.Contains(i.Account)))
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
	
											if(o is Emission e)
											{
												Task.Run(() =>	{
																	var v = Ethereum.IsEmissionValid(e);

																	lock(Lock)
																		Mcv.ApprovedEmissions.Add(new ForeignResult {OperationId = e.Id, Approved = v});
																});
											}
										}
									}

									Mcv.ApprovedEmissions.RemoveAll(i => (r as RdnRound).ConsensusEmissions.Any(j => j.OperationId == i.OperationId) || r.Id > i.OperationId.Ri + Zone.ExternalVerificationDurationLimit);
									Mcv.ApprovedMigrations.RemoveAll(i => (r as RdnRound).ConsensusMigrations.Any(j => j.OperationId == i.OperationId) || r.Id > i.OperationId.Ri + Zone.ExternalVerificationDurationLimit);
								};

			RunPeer();

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
			if(t == typeof(Transaction))	return new Transaction {Zone = Zone};
			if(t == typeof(Manifest))		return new Manifest();
			if(t == typeof(Urr))			return Urr.FromType(b); 

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
			if(o is Emission e && !Ethereum.IsEmissionValid(e))
				return false;

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
