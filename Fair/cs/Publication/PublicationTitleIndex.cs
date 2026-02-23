using System.Collections;
using System.Collections.Generic;
using System.Text;

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
