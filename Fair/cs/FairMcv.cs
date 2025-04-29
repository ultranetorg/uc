using System.Net;
using RocksDbSharp;

namespace Uccs.Fair;

public class FairMcv : Mcv
{
	IPAddress[]							GraphIPs;
	public AuthorTable					Authors;
	public ProductTable					Products;
	public SiteTable					Sites;
	public CategoryTable				Categories;
	public PublicationTable				Publications;
	public ReviewTable					Reviews;
	public DisputeTable					Disputes;
	public WordTable					Words;
	public PublicationTitleIndex		PublicationTitles;

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

/*
		var luceneVersion = LuceneVersion.LUCENE_48; 

		var indexDir = FSDirectory.Open(Path.Join(databasepath, "Lucene"));

		LuceneAnalyzer = new WhitespaceAnalyzer(luceneVersion);

		var indexConfig = new IndexWriterConfig(luceneVersion, LuceneAnalyzer);
		indexConfig.OpenMode = OpenMode.CREATE_OR_APPEND;
		LuceneWriter = new IndexWriter(indexDir, indexConfig);

/ *
  		var a = new Document();
  		a.Add(new StringField("t", "The great application", Field.Store.YES));
  		a.Add(new TextField("s", "111-11 567-22\n222-22 567-44", Field.Store.YES));
  		LuceneWriter.AddDocument(a);
  		var b = new Document();
  		b.Add(new StringField("t", "The great", Field.Store.YES));
  		b.Add(new TextField("s", "1111-11 5678-22\n333-33 9999-66", Field.Store.YES));
  		LuceneWriter.AddDocument(b);
  		
  		LuceneWriter.Commit();
 
  		var reader = LuceneWriter.GetReader(applyAllDeletes: true);
  		var LuceneSearcher = new IndexSearcher(reader);
  
  		var q = new BooleanQuery();
  		q.Add(new TermQuery(new Term("s", "111-11")), Occur.MUST);

		//var phq = new PhraseQuery();
		//phq.Add(new Term("id", "The"));
		//phq.Add(new Term("id", "great"));
  
  		var docs = LuceneSearcher.Search(q, 10);
  
  		var d = LuceneSearcher.Doc(docs.ScoreDocs[0].Doc);* /

		Commited += r => { 
							using var rd = LuceneWriter.GetReader(applyAllDeletes: true);
							var s = new IndexSearcher(rd);

							foreach(var i in (r as FairRound).AffectedTexts.Values)
							{
								if(!i.Deleted)
								{
									var t = new Term("e", $"s{i.Site} {i.Address.Entity}");

									var docs = s.Search(new TermQuery(t), 1);

									if(docs.TotalHits == 0)
									{	
										var d = new Document();
										d.Add(new TextField("t", i.Text, Field.Store.YES));
										d.Add(new TextField("e", t.Text, Field.Store.YES));

										LuceneWriter.AddDocument(d);
									}
									else
									{	
										var d = s.Doc(docs.ScoreDocs[0].Doc);

										d.Add(new TextField("e", $"{d.Get("e")}\n{t.Text}", Field.Store.YES));

										LuceneWriter.UpdateDocument(t, d);
									}
								}
								else
								{
								}
							}
						
							LuceneWriter.Commit();
						 };*/
	}

	public string CreateGenesis(AccountKey god, AccountKey f0)
	{
		return CreateGenesis(god, f0, new CandidacyDeclaration {GraphIPs = [Net.Father0IP]});
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

		Rocks = RocksDb.Open(dbo, databasepath, cfs);

		Accounts = new FairAccountTable(this);
		Authors = new (this);
		Products = new (this);
		Sites = new (this);
		Categories = new (this);
		Publications = new (this);
		Reviews = new (this);
		Disputes = new (this);
		Words = new (this);
		PublicationTitles = new (this);

		Tables = [Accounts, Authors, Products, Sites, Categories, Publications, Reviews, Disputes, Words, PublicationTitles];
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
