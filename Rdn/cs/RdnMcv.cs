using System.Net;
using RocksDbSharp;

namespace Uccs.Rdn
{
	public class RdnMcv : Mcv
	{
		public DomainTable				Domains;
		public SiteTable				Sites;
		//public List<ForeignResult>	ApprovedEmissions = new();
		public List<ForeignResult>		ApprovedMigrations = new();
		IPAddress[]						BaseIPs;
		IPAddress[]						SeedHubIPs;

		public RdnMcv()
		{
  		}

		public RdnMcv(Rdn net, McvSettings settings, string databasepath, bool skipinitload = false) : base(net, settings, databasepath, skipinitload)
		{
		}

		public RdnMcv(Rdn sun, McvSettings settings, string databasepath, IPAddress[] baseips, IPAddress[] seedhubips, IClock clock) : base(sun, settings, databasepath, clock)
		{
			BaseIPs = baseips;
			SeedHubIPs = seedhubips;
		}

		public string CreateGenesis(AccountKey god, AccountKey f0)
		{
			return CreateGenesis(god, f0, new RdnCandidacyDeclaration {BaseRdcIPs = [Net.Father0IP], SeedHubRdcIPs = [Net.Father0IP]});
		}

		protected override void GenesisCreate(Vote vote)
		{
			//(vote as RdnVote).Emissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
		}

		protected override void GenesisInitilize(Round round)
		{
			#if IMMISSION
			if(round.Id == 1)
				(round as RdnRound).ConsensusEmissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
			#endif
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
																new (SiteTable.MetaColumnName,		new ()),
																new (SiteTable.MainColumnName,		new ()),
																new (SiteTable.MoreColumnName,		new ()),
																new (ChainFamilyName,				new ())})
				cfs.Add(i);

			Database = RocksDb.Open(dbo, databasepath, cfs);

			Accounts = new (this);
			Domains = new (this);
			Sites = new (this);

			Tables = [Accounts, Domains, Sites];
		}

		public override Round CreateRound()
		{
			return new RdnRound(this);
		}

		public override Vote CreateVote()
		{
			return new RdnVote(this);
		}

		public override Generator CreateGenerator()
		{
			return new RdnGenerator();
		}

		public override CandidacyDeclaration CreateCandidacyDeclaration()
		{
			return new RdnCandidacyDeclaration {BaseRdcIPs		= BaseIPs,
												SeedHubRdcIPs	= SeedHubIPs};

		}

		public override void FillVote(Vote vote)
		{
			var v = vote as RdnVote;

  			//v.Emissions		= ApprovedEmissions.ToArray();
			v.Migrations	= ApprovedMigrations.ToArray();
		}

		public IEnumerable<Resource> SearchResources(string query)
		{
			var r = Ura.Parse(query);
		
			var d = Domains.Find(r.Domain, LastConfirmedRound.Id);

			if(d == null)
				yield break;

			var s = Sites.Find(d.Id, LastConfirmedRound.Id);

			foreach(var i in s.Resources.Where(i => i.Address.Resource.StartsWith(r.Resource)))
				yield return i;
		}
	}
}
