namespace Uccs.Fair;

public class StoreAuthorsRemoval : VotableOperation
{
	public AutoId[]				Authors { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Store={Store}, Authors={Authors.Length}";
	
	public override void Read(Reader reader)
	{
		Authors = reader.ReadArray<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Authors);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as StoreAuthorsRemoval;
		
		foreach(var i in Authors)
			if(o.Authors.Contains(i))
				return true;
				
		return false;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		foreach(var i in Authors)
		{
			if(!Store.Publishers.Any(a => a.Author == i))
			{	
				error = NotFound;
				return false;
			}

			var a = execution.Authors.Find(i);

			if(a.Products.Select(i => execution.Products.Find(i)).Any(i => i.Publications.Any(p => execution.Publications.Find(p)?.Store == Store.Id)))
			{	
				error = PublicationsExist;
				return false;
			}
		}

	
		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
 		var s = Store;
 
 		foreach(var i in Authors)
 		{	
			s.Publishers = s.Publishers.Remove(s.Publishers.First(m => m.Author == i));
			
			var a = execution.Authors.Affect(i);
			a.Stores = a.Stores.Remove(s.Id);
		}

	}
}