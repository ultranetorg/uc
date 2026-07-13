using System.Collections;
using System.Collections.Generic;
using System.Text;
using RocksDbSharp;

namespace Uccs.Fair;

public class StoreSearchResult
{
	public string		Text { get; set; }
	public AutoId		Entity { get; set; }

	public override string ToString()
	{
		return $"{Text}, {Entity}";
	}
}

public class StoreTitleIndex : HnswTable<string, StringToOneHnswEntity>
{
	public override string			Name => FairTable._StoreTitle.ToString();
	
	public StoreTitleIndex(Mcv mcv) : base(mcv, new NeedlemanWunsch())
	{
	}

	public override StringToOneHnswEntity Create()
	{
		return new StringToOneHnswEntity() {References = [], Text = string.Empty};
	}

	public StoreTitleExecution CreateExecuting(Execution execution)
	{
		return new StoreTitleExecution(execution as FairExecution);
	}

	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in Mcv.Stores.GraphEntities.Where(i => i.Name != null))
		{
			var w = e.Words.Affect(Word.GetId(i.Name));

			w.Reference = new EntityFieldAddress {Entity = i.Id, Field = EntityTextField.StoreName};
		}

		Mcv.Words.Commit(batch, e.Words.Affected.Values, e.Words, null);

		Mcv.StoreTitles.Clear();

		e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in Mcv.Stores.GraphEntities)
		{
			e.StoreTitles.Index(i.Id, i.Title);
		}
	
		Mcv.StoreTitles.Commit(batch, e.StoreTitles.Affected.Values, e.StoreTitles, lastincommit);
		//(lastincommit as FairRound).StoreTitles = new (Mcv.StoreTitles)
		//										 {
		//											EntryPoints = e.StoreTitles.EntryPoints
		//										 };
	}

	public StoreSearchResult[] Search(string query, int skip, int take)
	{
		var result = Mcv.StoreTitles.Search(	query.ToLowerInvariant(), 
											skip, 
											take, 
											null,
											Mcv.StoreTitles.Latest);

		return result.SelectMany(i =>	{
											return i.References.Select(j => new StoreSearchResult {Entity = j, Text = i.Text});
										}).ToArray();
	}

}

public class StoreTitleExecution : StringHnswTableExecution<StringToOneHnswEntity>
{
	public StoreTitleExecution(FairExecution execution) : base(execution, execution.Mcv.StoreTitles)
	{
	}

  	public StringToOneHnswEntity Index(AutoId store, string text)
  	{
		var e = Index(text);
 
  		if(!e.References.Contains(store))
  		{	
  			e.References = new (e.References);
  			e.References.Add(store);
  		}

		return e;
  	}
 
  	public void Deindex(AutoId store, string text)
  	{
 		var e = Affect(text);
 	
 		e.References = new (e.References);
 		e.References.Remove(store);
  	}
}
