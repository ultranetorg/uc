using RocksDbSharp;

namespace Uccs.Fair;

public class SiteTable : Table<AutoId, Site>
{
	public override string			Name => FairTable.Site.ToString();
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public SiteTable(FairMcv mcv) : base(mcv)
	{
	}
	
	public override Site Create()
	{
		return new Site(Mcv);
	}

	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in GraphEntities.Where(i => i.Nickname != ""))
		{
			var w = e.Words.Affect(Word.GetId(i.Nickname));

			w.Reference = new EntityFieldAddress {Entity = i.Id, Field = EntityTextField.SiteNickname};
		}

		Mcv.Words.Commit(batch, e.Words.Affected.Values, e.Words, null);

		e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in GraphEntities)
		{
			e.SiteTitles.Index(i.Id, i.Title);
		}
	
		Mcv.SiteTitles.Commit(batch, e.SiteTitles.Affected.Values, e.SiteTitles, lastincommit);
		(lastincommit as FairRound).SiteTitles = new (Mcv.SiteTitles){
																		EntryPoints = e.SiteTitles.EntryPoints
																	 };
	}

	public SearchResult[] Search(string query, int skip, int take)
	{
		var result = Mcv.SiteTitles.Search(	query.ToLowerInvariant(), 
											skip, 
											take, 
											null,
											Mcv.SiteTitles.Latest, 
											(Mcv.LastConfirmedRound as FairRound).SiteTitles.EntryPoints);

		return result.SelectMany(i =>	{
											return i.References.Select(j => new SearchResult {Entity = j, Text = i.Text});
										}).ToArray();
	}
}

public class SiteExecution : TableExecution<AutoId, Site>
{
	public SiteExecution(FairExecution execution) : base(execution.Mcv.Sites, execution)
	{
	}

	public Site Create(User signer)
	{
		Execution.IncrementCount((int)FairMetaEntityType.SitesCount);

		var b = Execution.Mcv.Users.KeyToBucket(signer.Name);
		int e = Execution.GetNextEid(Table, b);

		var s = Table.Create();
		
		s.Id = LastCreatedId = new AutoId(b, e);
		s.Categories = [];
		s.Moderators = [];
		s.Publishers = [];
		s.Users = [];
		s.Proposals = [];
		s.UnpublishedPublications = [];
		s.ChangedPublications = [];
		s.Nickname = "";
		s.Files = [];
		s.Description = "";
		
		return Affected[s.Id] = s;
	}

	public override Site Affect(AutoId id)
	{
		var e = base.Affect(id);

		Execution.TransferEnergyIfNeeded(e);

		return e;
	}

}
