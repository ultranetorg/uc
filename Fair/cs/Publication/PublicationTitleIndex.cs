using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Uccs.Fair;

public class PublicationTitleIndex : HnswTable<string, StringToDictionaryHnswEntity>
{
	public override string			Name => FairTable._PublicationTitle.ToString();

	public PublicationTitleIndex(Mcv mcv, int maxLevel = 5, int maxConnections = 5, int efConstruction = 64, int threshold = 100, int minDiversity = 100) : base(mcv, new NeedlemanWunsch(), maxLevel, maxConnections, efConstruction, threshold, minDiversity)
	{
	}

	public override StringToDictionaryHnswEntity Create()
	{
		return new StringToDictionaryHnswEntity() {References = []};
	}

	public PublicationTitleExecution CreateExecuting(Execution execution)
	{
		return new PublicationTitleExecution(execution as FairExecution);
	}
}

public class PublicationTitleExecution : StringHnswTableExecution<StringToDictionaryHnswEntity>
{
	public PublicationTitleExecution(FairExecution execution) : base(execution, execution.Mcv.PublicationTitles)
	{
	}

  	public void Index(AutoId site, AutoId entity, string text)
  	{
 		var e = Index(entity, text);
 
  		if(!e.References.ContainsKey(site))
  		{	
  			e.References = new (e.References);
  			e.References[site] = entity;
  		}
  	}
 
  	public void Deindex(AutoId site, string text)
  	{
 		var e = Affect(text);
 	
 		e.References = new (e.References);
 		e.References.Remove(site);
  	}
}
