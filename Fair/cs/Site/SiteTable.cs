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

			w.References = [..w.References, new EntityFieldAddress {Entity = i.Id, Field = EntityTextField.SiteNickname}];
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

	public Site Create(Account signer)
	{
		Execution.IncrementCount((int)FairMetaEntityType.SitesCount);

		var b = Execution.Mcv.Accounts.KeyToBucket(signer.Address);
		
		int e = Execution.GetNextEid(Table, b);

		var a = Table.Create();
		
		a.Id = LastCreatedId = new AutoId(b, e);
		a.Categories = [];
		a.Moderators = [];
		a.Authors = [];
		a.Proposals = [];
		a.CreationPolicies = [];
		a.ApprovalPolicies = [];
		a.UnpublishedPublications = [];
		a.ChangedPublications = [];
		a.ChangedReviews = [];
		a.Nickname = "";
		a.Files = [];
		a.Description = "";
		
		return Affected[a.Id] = a;
	}

	public override Site Affect(AutoId id)
	{
		var e = base.Affect(id);

		Execution.TransferEnergyIfNeeded(e);

		return e;
	}

}
