using System.Net;
using RocksDbSharp;

namespace Uccs.Fair;

public enum FairMetaEntityType : int
{
	AuthorsCount = MetaEntityType._Last + 1,
	ProductsCount,
	SitesCount,
	CategoriesCount,
	PublicationsCount,
	ReviewsCount,
	ProposalCount,
	ProposalCommentsCount,
	WordsCount,

	SiteTitleEntryPoint,
	PublicationTitleEntryPoint,
}

public enum FairTable : byte
{
	Meta = McvTable.Meta, Account = McvTable.Account,
	Author, Product, Site, Category, Publication, Review, Proposal, ProposalComment, File, _Word, _PublicationTitle, _SiteTitle
}

public class FairMcv : Mcv
{
	public AuthorTable					Authors;
	public ProductTable					Products;
	public SiteTable					Sites;
	public CategoryTable				Categories;
	public PublicationTable				Publications;
	public ReviewTable					Reviews;
	public ProposalTable				Proposals;
	public ProposalCommentTable			ProposalComments;
	public FileTable					Files;
	public WordTable					Words;
	public PublicationTitleIndex		PublicationTitles;
	public SiteTitleIndex				SiteTitles;

	IPAddress[]							GraphIPs;
	public new IEnumerable<FairRound>	Tail => base.Tail.Cast<FairRound>();

	public FairMcv()
	{
  	}

	public FairMcv(Fair net, McvSettings settings, string databasepath, bool skipinitload = false) : base(net, settings, databasepath, skipinitload)
	{
	}

	public FairMcv(Fair net, McvSettings settings, string databasepath, IPAddress[] baseips, IClock clock) : base(net, settings, databasepath, clock)
	{
		GraphIPs = baseips;
	}

	public string CreateGenesis(AccountKey f0)
	{
		return CreateGenesis(f0, new Genesis());
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

		Rocks = RocksDb.Open(dbo, databasepath, cfs);

		Metas = new (this);
		Accounts = new FairAccountTable(this);
		Authors = new (this);
		Products = new (this);
		Sites = new (this);
		Categories = new (this);
		Publications = new (this);
		Reviews = new (this);
		Proposals = new (this);
		ProposalComments = new (this);
		Files = new (this);
		Words = new (this);
		PublicationTitles = new (this);
		SiteTitles = new (this);

		Tables = [Metas, Accounts, Authors, Products, Sites, Categories, Publications, Reviews, Proposals, ProposalComments, Files, Words, PublicationTitles, SiteTitles];
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
		return new CandidacyDeclaration {GraphIPs	= GraphIPs};
	}

	public override void FillVote(Vote vote)
	{
	}
}
