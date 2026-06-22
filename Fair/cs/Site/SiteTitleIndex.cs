using System.Collections;
using System.Collections.Generic;
using System.Text;
using RocksDbSharp;

namespace Uccs.Fair;

public class SiteTitleIndex : HnswTable<string, StringToOneHnswEntity>
{
	public override string			Name => FairTable._SiteTitle.ToString();
	
	public SiteTitleIndex(Mcv mcv) : base(mcv, new NeedlemanWunsch())
	{
	}

	public override StringToOneHnswEntity Create()
	{
		return new StringToOneHnswEntity() {References = [], Text = string.Empty};
	}

	public SiteTitleExecution CreateExecuting(Execution execution)
	{
		return new SiteTitleExecution(execution as FairExecution);
	}

	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in Mcv.Sites.GraphEntities.Where(i => i.Name != null))
		{
			var w = e.Words.Affect(Word.GetId(i.Name));

			w.Reference = new EntityFieldAddress {Entity = i.Id, Field = EntityTextField.SiteName};
		}

		Mcv.Words.Commit(batch, e.Words.Affected.Values, e.Words, null);

		Mcv.SiteTitles.Clear();

		e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in Mcv.Sites.GraphEntities)
		{
			e.SiteTitles.Index(i.Id, i.Title);
		}
	
		Mcv.SiteTitles.Commit(batch, e.SiteTitles.Affected.Values, e.SiteTitles, lastincommit);
		//(lastincommit as FairRound).SiteTitles = new (Mcv.SiteTitles)
		//										 {
		//											EntryPoints = e.SiteTitles.EntryPoints
		//										 };
	}

	public SearchResult[] Search(string query, int skip, int take)
	{
		var result = Mcv.SiteTitles.Search(	query.ToLowerInvariant(), 
											skip, 
											take, 
											null,
											Mcv.SiteTitles.Latest);

		return result.SelectMany(i =>	{
											return i.References.Select(j => new SearchResult {Entity = j, Text = i.Text});
										}).ToArray();
	}

}

public class SiteTitleExecution : StringHnswTableExecution<StringToOneHnswEntity>
{
	public SiteTitleExecution(FairExecution execution) : base(execution, execution.Mcv.SiteTitles)
	{
	}

  	public override StringToOneHnswEntity Index(AutoId site, string text)
  	{
		var e = base.Index(site, text);
 
  		if(!e.References.Contains(site))
  		{	
  			e.References = new (e.References);
  			e.References.Add(site);
  		}

		return e;
  	}
 
  	public void Deindex(AutoId site, string text)
  	{
 		var e = Affect(text);
 	
 		e.References = new (e.References);
 		e.References.Remove(site);
  	}
}
