using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DnsClient;
using RocksDbSharp;

namespace Uccs.Net
{
	public abstract class RdsCall<R> : PeerCall<R> where R : PeerResponse
	{
		public Rds	Rds => Mcv as Rds;
	}

	public class Rds : Mcv
	{
		public DomainTable				Domains;
		public readonly static Guid		Id = new Guid("A8B619CB-8A8C-4C71-847A-4A182ABDE2B9");
		public override Guid			Guid => Id;
		public IEthereum				Nas;
		LookupClient					Dns = new LookupClient(new LookupClientOptions {Timeout = TimeSpan.FromSeconds(5)});
		HttpClient						Http = new HttpClient();

		public new RdsSettings			Settings;
		public ResourceHub				ResourceHub;
		public PackageHub				PackageHub;
		public SeedHub					SeedHub;

		public List<ForeignResult>		ApprovedEmissions = new();
		public List<ForeignResult>		ApprovedMigrations = new();

		public Rds(Zone zone, RdsSettings settings, string databasepath, bool skipinitload = false) : base(zone, settings, databasepath, skipinitload)
		{
			Settings = settings;
		}

		public Rds(Sun sun, RdsSettings settings, string databasepath, Flow flow, IEthereum nas, IClock clock) : base(sun, settings, databasepath, clock, flow)
		{
			Settings = settings;
			Nas = nas ?? new Ethereum(settings);
			Clock = clock ?? new RealClock();

			if(settings.Roles.HasFlag(Role.Seed))
			{
				ResourceHub = new ResourceHub(this, Zone, settings.Releases);
				PackageHub = new PackageHub(this, settings.Releases, settings.Packages);
			}

			if(Settings.Generators.Any())
			{
		  		try
		  		{
		 			new Uri(Settings.Ethereum.Provider);
		  		}
		  		catch(Exception)
		  		{
		  			Nas.ReportEthereumJsonAPIWarning($"Ethereum Json-API provider required to run the node as a generator.", true);
					return;
		  		}

				SeedHub = new SeedHub(this);
			}

			Commited += r => {
								if(LastConfirmedRound.Members.Any(i => Settings.Generators.Contains(i.Account)))
								{
									var ops = r.ConsensusTransactions.SelectMany(t => t.Operations).ToArray();
												
									foreach(var o in ops)
									{
										if(o is DomainMigration am)
										{
	 										if(!SunGlobals.SkipMigrationVerification)
	 										{
												Task.Run(() =>	{
																	var approved = IsDnsValid(am);

																	lock(Sun.Lock)
																		ApprovedMigrations.Add(new ForeignResult {OperationId = am.Id, Approved = approved});
																});
	 										}
											else
												ApprovedMigrations.Add(new ForeignResult {OperationId = am.Id, Approved = true});
										}
	
										if(o is Emission e)
										{
											Task.Run(() =>	{
																var v = Nas.IsEmissionValid(e);

																lock(Sun.Lock)
																	ApprovedEmissions.Add(new ForeignResult {OperationId = e.Id, Approved = v});
															});
										}
									}
								}

								ApprovedEmissions.RemoveAll(i => (r as RdsRound).ConsensusEmissions.Any(j => j.OperationId == i.OperationId) || r.Id > i.OperationId.Ri + Zone.ExternalVerificationDurationLimit);
								ApprovedMigrations.RemoveAll(i => (r as RdsRound).ConsensusMigrations.Any(j => j.OperationId == i.OperationId) || r.Id > i.OperationId.Ri + Zone.ExternalVerificationDurationLimit);
							};

		}

		protected override void GenesisCreate(Vote vote)
		{
			(vote as RdsVote).Emissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
		}

		protected override void GenesisInitilize(Round round)
		{
			if(round.Id == 1)
				(round as RdsRound).ConsensusEmissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
		}

		protected override void CreateTables(string databasepath)
		{
			var dbo	= new DbOptions().SetCreateIfMissing(true)
									 .SetCreateMissingColumnFamilies(true);

			var cfs = new ColumnFamilies();
			
			foreach(var i in new ColumnFamilies.Descriptor[]{	new (AccountTable.MetaColumnName,	new ()),
																new (AccountTable.MainColumnName,	new ()),
																new (AccountTable.MoreColumnName,	new ()),
																new (DomainTable.MetaColumnName,	new ()),
																new (DomainTable.MainColumnName,	new ()),
																new (DomainTable.MoreColumnName,	new ()),
																new (ChainFamilyName,				new ()),
																new (ResourceHub.ReleaseFamilyName,	new ()),
																new (ResourceHub.ResourceFamilyName,new ()) 
																})
				cfs.Add(i);

			Database = RocksDb.Open(dbo, databasepath, cfs);

			Accounts = new (this);
			Domains = new (this);

			Tables = [Accounts, Domains];
		}

		public override Round CreateRound()
		{
			return new RdsRound(this);
		}

		public override Vote CreateVote()
		{
			return new RdsVote(this);
		}

		public override void ClearTables()
		{
			Domains.Clear();
		}

		public IEnumerable<Resource> QueryResource(string query)
		{
			var r = Ura.Parse(query);
		
			var a = Domains.Find(r.Domain, LastConfirmedRound.Id);

			if(a == null)
				yield break;

			foreach(var i in a.Resources.Where(i => i.Address.Resource.StartsWith(r.Resource)))
				yield return i;
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

		public override bool ProcessIncomingOperation(Operation o)
		{
			if(o is Emission e && !Nas.IsEmissionValid(e))
				return false;

			if(o is DomainMigration m && !IsDnsValid(m))
				return false;

			return true;
		}

		public override void FillVote(Vote vote)
		{
			var v = vote as RdsVote;

  			v.Emissions		= ApprovedEmissions.ToArray();
			v.Migrations	= ApprovedMigrations.ToArray();
		}

	}
}
