using System.Net;
using RocksDbSharp;

namespace Uccs.Rdn;

public enum RdnTable : byte
{
	Meta = McvTable.Meta, 
	User = McvTable.User,
	Subnet = McvTable.Subnet,
	Domain,
	Resource, 
}

public class RdnMcv : Mcv
{
	public DomainTable				Domains;
	public ResourceTable			Resources;
	Endpoint[]						GraphIPs;
	Endpoint[]						SeedHubIPs;

	public RdnMcv()
	{
  	}

	public RdnMcv(Rdn sun, McvSettings settings, string datapath, string databasepath, Endpoint[] baseips, Endpoint[] seedhubips, IClock clock) : base(sun, settings, datapath, databasepath, new RdnGenesis(), clock)
	{
		GraphIPs = baseips;
		SeedHubIPs = seedhubips;
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

		//dbo.SetEnv(RocksDbSharp.Native.Instance.rocksdb_create_mem_env());

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
		Users = new (this);
		Domains = new (this);
		Resources = new (this);
		Friends = new (this);

		Tables = [Metas, Users, Friends, Domains, Resources];
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
		return new RdnCandidacyDeclaration {GraphEPs		= GraphIPs,
											SeedHubRdcIPs	= SeedHubIPs};

	}

	public IEnumerable<Resource> SearchResources(AutoId domain, string query)
	{
		var d = Domains.Latest(domain);
		
		if(d == null)
			yield break;
		
		var b = Resources.FindBucket(d.Id.B);
		
		if(b == null)
			yield break;
		
		foreach(var i in b.Entries.Where(i => i.Domain == domain && (query == null || i.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase))))
			yield return i;
	}
}
