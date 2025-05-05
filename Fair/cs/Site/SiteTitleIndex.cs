using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Uccs.Fair;

public class SiteTitleIndex : HnswTable<string, StringHnswEntity>
{
	public SiteTitleIndex(Mcv mcv, int maxLevel = 5, int maxConnections = 5, int efConstruction = 64, int threshold = 100, int minDiversity = 100) : base(mcv, new NeedlemanWunsch(), maxLevel, maxConnections, efConstruction, threshold, minDiversity)
	{
	}

	public override StringHnswEntity Create()
	{
		return new StringHnswEntity() {References = []};
	}

	public SiteTitleExecution CreateExecuting(Execution execution)
	{
		return new SiteTitleExecution(execution as FairExecution);
	}
}

// public class SiteTitleState : HnswTableState<string, StringHnswEntity>
// {
// 	public SiteTitleState(HnswTable<string, StringHnswEntity> table) : base(table)
// 	{
// 	}
// }

public class SiteTitleExecution : StringHnswTableExecution<StringHnswEntity>
{
	public SiteTitleExecution(FairExecution execution) : base(execution, execution.Mcv.SiteTitles)
	{
		EntryPoints = execution.Round.SiteTitles?.EntryPoints ?? Table.EntryPoints;
	}

  	public void Index(AutoId site, string text)
  	{
 		text = text.ToLowerInvariant();
 
 		var e =	Find(text);
 
  		if(e == null)
  		{
			var b = DataToBucket(text);
			
  			var id = new HnswId(b, Execution.GetNextEid(Table, b));
  	
  			e = Affect(id);
  	
  			e.Text = text;
 			//e.Hash = Metric.Hashify(text);
  			
  			Add(e);
  		}
  		else
 			e = Affect(e.Id);
 
  		if(!e.References.Contains(site))
  		{	
  			e.References = new (e.References);
  			e.References.Add(site);
  		}
  	}
 
  	public void Deindex(AutoId site, string text)
  	{
 		text = text.ToLowerInvariant();
 
  		var e =	Find(text);
 	
 		e = Affect(e.Id);
 	
 		e.References = new (e.References);
 		e.References.Remove(site);
  	}
}
