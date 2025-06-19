using System.Net;
using RocksDbSharp;

namespace Uccs.Rdn;

public class RdnMcv : Mcv
{
	public DomainTable				Domains;
	public ResourceTable			Resources;
	//public List<ForeignResult>	ApprovedEmissions = new();
	public List<ForeignResult>		ApprovedMigrations = new();
	IPAddress[]						GraphIPs;
	IPAddress[]						SeedHubIPs;

	public RdnMcv()
	{
  	}

	public RdnMcv(Rdn net, McvSettings settings, string databasepath, bool skipinitload = false) : base(net, settings, databasepath, skipinitload)
	{
	}

	public RdnMcv(Rdn sun, McvSettings settings, string databasepath, IPAddress[] baseips, IPAddress[] seedhubips, IClock clock) : base(sun, settings, databasepath, clock)
	{
		GraphIPs = baseips;
		SeedHubIPs = seedhubips;
	}

	public string CreateGenesis(AccountKey god, AccountKey f0)
	{
		return CreateGenesis(god, f0, new RdnCandidacyDeclaration {GraphIPs = [Net.Father0IP], SeedHubRdcIPs = [Net.Father0IP]});
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
		
		if(RocksDb.TryListColumnFamilies(dbo, databasepath, out var cfn))
		{	
			foreach(var i in cfn)
			{
				cfs.Add(i, new ());
			}
		}
		else
			cfs.Add(ChainFamilyName, new ());

		Rocks = RocksDb.Open(dbo, databasepath, cfs);

		Metas = new (this);
		Accounts = new (this);
		Domains = new (this);
		Resources = new (this);

		Tables = [Metas, Accounts, Domains, Resources];
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
		return new RdnCandidacyDeclaration {GraphIPs		= GraphIPs,
											SeedHubRdcIPs	= SeedHubIPs};

	}

	public override void FillVote(Vote vote)
	{
		var v = vote as RdnVote;

  			//v.Emissions	= ApprovedEmissions.ToArray();
		v.Migrations	= ApprovedMigrations.ToArray();
	}

	public IEnumerable<Resource> SearchResources(string query)
	{
		var r = Ura.Parse(query);
	
		var d = Domains.Find(r.Domain, LastConfirmedRound.Id);
		
		if(d == null)
			yield break;
		
		var c = Resources.FindBucket(d.Id.B);
		
		if(c == null)
			yield break;

		foreach(var i in c.Entries.Where(i => i.Id.E == d.Id.E && i.Address.Resource.StartsWith(r.Resource)))
			yield return i;
	}
}
