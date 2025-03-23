using System.Net;
using RocksDbSharp;

namespace Uccs.Fair;

public class FairMcv : Mcv
{
	public AuthorTable		Authors;
	public ProductTable		Products;
	public SiteTable		Sites;
	public CategoryTable	Categories;
	public PublicationTable	Publications;
	public ReviewTable		Reviews;
	public DisputeTable		Disputes;
	public NicknameTable	Nicknames;
	IPAddress[]				BaseIPs;

	public FairMcv()
	{
  	}

	public FairMcv(Fair net, McvSettings settings, string databasepath, bool skipinitload = false) : base(net, settings, databasepath, skipinitload)
	{
	}

	public FairMcv(Fair sun, McvSettings settings, string databasepath, IPAddress[] baseips, IClock clock) : base(sun, settings, databasepath, clock)
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
			(round as FairRound).ConsensusEmissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
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

		Accounts = new FairAccountTable(this);
		Authors = new (this);
		Products = new (this);
		Sites = new (this);
		Categories = new (this);
		Publications = new (this);
		Reviews = new (this);
		Disputes = new (this);
		Nicknames = new (this);

		Tables = [Accounts, Authors, Products, Sites, Categories, Publications, Reviews, Disputes, Nicknames];
	}

	public override Round CreateRound()
	{
		return new FairRound(this);
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
