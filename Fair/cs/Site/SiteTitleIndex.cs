using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Uccs.Fair;

public class SiteTitleIndex : HnswTable<string, StringToOneHnswEntity>
{
	public override string			Name => FairTable._SiteTitle.ToString();
	
	public SiteTitleIndex(Mcv mcv, int maxLevel = 5, int maxConnections = 5, int efConstruction = 64, int threshold = 100, int minDiversity = 100) : base(mcv, new NeedlemanWunsch(), maxLevel, maxConnections, efConstruction, threshold, minDiversity)
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
