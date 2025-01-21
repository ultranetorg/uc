using System.Net;
using RocksDbSharp;

namespace Uccs.Smp;

public class SmpMcv : Mcv
{
	public AuthorTable		Authors;
	public ProductTable		Products;
	public SiteTable		Sites;
	public CategoryTable	Categories;
	public PublicationTable	Publications;
	IPAddress[]				BaseIPs;

	public SmpMcv()
	{
  	}

	public SmpMcv(Smp net, McvSettings settings, string databasepath, bool skipinitload = false) : base(net, settings, databasepath, skipinitload)
	{
	}

	public SmpMcv(Smp sun, McvSettings settings, string databasepath, IPAddress[] baseips, IClock clock) : base(sun, settings, databasepath, clock)
	{
		BaseIPs = baseips;
	}

	public string CreateGenesis(AccountKey god, AccountKey f0)
	{
		return CreateGenesis(god, f0, new CandidacyDeclaration {BaseRdcIPs = [Net.Father0IP]});
	}

	public override string CreateGenesis(AccountKey god, AccountKey f0, CandidacyDeclaration candidacydeclaration)
	{
		return base.CreateGenesis(god, f0, candidacydeclaration);
	}

	protected override void GenesisInitilize(Round round)
	{
		#if IMMISSION
		if(round.Id == 1)
			(round as SmpRound).ConsensusEmissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
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

		Database = RocksDb.Open(dbo, databasepath, cfs);

		Accounts = new SmpAccountTable(this);
		Authors = new (this);
		Products = new (this);
		Sites = new (this);
		Categories = new (this);
		Publications = new (this);

		Tables = [Accounts, Authors, Products, Sites, Categories, Publications];
	}

	public override Round CreateRound()
	{
		return new SmpRound(this);
	}

	public override Vote CreateVote()
	{
		return new Vote(this);
	}

	public override Generator CreateGenerator()
	{
		return new Generator();
	}

	public override CandidacyDeclaration CreateCandidacyDeclaration()
	{
		return new CandidacyDeclaration {BaseRdcIPs	= BaseIPs};
	}

	public override void FillVote(Vote vote)
	{
	}
}
