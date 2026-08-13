using System.Net;
using RocksDbSharp;

namespace Uccs.Fair;

public enum FairMetaEntityType : uint
{
	AuthorsIdCounter = MetaEntityType._Last + 1,
	ProductsIdCounter,
	StoreIdCounter,
	CategoriesIdCounter,
	PublicationsIdCounter,
	ReviewsIdCounter,
	ProposalIdCounter,
	ProposalCommentsIdCounter,
	NameIdCounter,
	FileIdCounter,

	AuthorsCount,
	ProductsCount,
	StoreCount,
	CategoriesCount,
	PublicationsCount,
	ReviewsCount,
	ProposalCount,
	ProposalCommentsCount,
	NamesCount,
}

public enum FairTable : byte
{
	Meta = McvTable.Meta, 
	User = McvTable.User,
	Subnet = McvTable.Friend,
	Author, Product, Store, Category, Publication, Review, Proposal, ProposalComment, File, 
	Name, StoreTitle, ProductTitle, PublicationTitle
}

public enum EntityTextField : byte
{
	UserName, 
	AuthorName, 
	ProductName, 
	StoreName, 
}

public class FairMcv : Mcv
{
	public AuthorTable					Authors;
	public ProductTable					Products;
	public StoreTable					Stores;
	public CategoryTable				Categories;
	public PublicationTable				Publications;
	public ReviewTable					Reviews;
	public ProposalTable				Proposals;
	public ProposalCommentTable			ProposalComments;
	public FileTable					Files;
	public NameTable					Names;
	public StoreTitleNgramIndex			StoreTitles;
	public ProductTitleNgramIndex		ProductTitles;
	public PublicationTitleNgramIndex	PublicationTitles;

	Net.Endpoint[]						GraphIPs;
	public new IEnumerable<FairRound>	Tail => base.Tail.Cast<FairRound>();

	public FairMcv()
	{
  	}

	public FairMcv(Fair net, McvSettings settings, string datapath, string databasepath, Net.Endpoint[] grpaheps, IClock clock) : base(net, settings, datapath, databasepath, new Genesis(), clock)
	{
		GraphIPs = grpaheps;
	}

	protected override void GenesisInitilize(Round round)
	{
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
		Users = new FairUserTable(this);
		Friends = new (this);
		Authors = new (this);
		Products = new (this);
		Stores = new (this);
		Categories = new (this);
		Publications = new (this);
		Reviews = new (this);
		Proposals = new (this);
		ProposalComments = new (this);
		Files = new (this);
		Names = new (this);
		StoreTitles = new (this);
		ProductTitles = new (this);
		PublicationTitles = new (this);

		Tables = [Metas, Users, Friends, Authors, Products, Stores, Categories, Publications, Reviews, Proposals, ProposalComments, Files, Names, StoreTitles, ProductTitles, PublicationTitles];
	}

	public override Round CreateRound()
	{
		return new FairRound(this);
	}

	public override Vote CreateVote()
	{
		return new Vote(this);
	}

	public override Member CreateGenerator()
	{
		return new Member();
	}

	public override CandidacyDeclaration CreateCandidacyDeclaration(AutoId beneficiary)
	{
		return	new CandidacyDeclaration
				{
					Beneficiary	= beneficiary,
					GraphEndpoints	= GraphIPs
				};
	}

	public override void FillVote(Vote vote)
	{
	}
}
