using System.Collections;
using System.Collections.Generic;
using System.Text;
using RocksDbSharp;

namespace Uccs.Fair;

public class PublicationTitleIndex : HnswTable<string, StringToDictionaryHnswEntity>
{
	public override string			Name => FairTable._PublicationTitle.ToString();

	public PublicationTitleIndex(Mcv mcv) : base(mcv, new NeedlemanWunsch())
	{
	}

	public override StringToDictionaryHnswEntity Create()
	{
		return new StringToDictionaryHnswEntity() {References = [], Text = string.Empty};
	}

	public PublicationTitleExecution CreateExecuting(Execution execution)
	{
		return new PublicationTitleExecution(execution as FairExecution);
	}


	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in Mcv.Publications.GraphEntities)
		{
			//var c = e.Categories.Find(i.Category);
			var r = Mcv.Products.Find(i.Product);
			var f = r.Versions[i.ProductVersion].Fields.FirstOrDefault(f => f.Name == Token.Title);

			if(f != null)
			{
				e.PublicationTitles.Index(i.Site, i.Id, f.AsUtf8);
			}
		}
			
		Commit(batch, e.PublicationTitles.Affected.Values, e.PublicationTitles, null);
		//(lastincommit as FairRound).PublicationTitles = new (Mcv.PublicationTitles) { EntryPoints = e.PublicationTitles.EntryPoints};
	}

	public SearchResult[] Search(AutoId site, string query, int skip, int take)
	{
		var result = Search(query.ToLowerInvariant(), 
							skip, 
							take, 
							i => i.References.ContainsKey(site),
							Mcv.PublicationTitles.Latest);

		return result.Select(i =>	new SearchResult
									{
										Entity = i.References[site], 
										Text = i.Text
									})
									.ToArray();
	}

	public PublicationGlobalSearchResult[] Search(string query, int skip, int take)
	{
		var result = Search(query.ToLowerInvariant(), 
							skip, 
							take, 
							null,
							Mcv.PublicationTitles.Latest);

		return result.Select(i =>	new PublicationGlobalSearchResult
									{
										Publications = i.References.Values.ToArray(), 
										Title = i.Text
									})
									.ToArray();
	}

}

public class PublicationTitleExecution : StringHnswTableExecution<StringToDictionaryHnswEntity>
{
	public PublicationTitleExecution(FairExecution execution) : base(execution, execution.Mcv.PublicationTitles)
	{
	}

  	public void Index(AutoId site, AutoId publication, string text)
  	{
 		var e = Index(publication, text);
 
  		if(!e.References.ContainsKey(site))
  		{	
  			e.References = new (e.References);
  			e.References[site] = publication;
  		}
  	}
 
  	public void Deindex(AutoId site, string text)
  	{
 		var e = Affect(text);
 	
 		e.References = new (e.References);
 		e.References.Remove(site);
  	}
}
