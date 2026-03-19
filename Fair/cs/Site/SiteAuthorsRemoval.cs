namespace Uccs.Fair;

public class SiteAuthorsRemoval : VotableOperation
{
	public AutoId[]				Authors { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Site={Site}, Authors={Authors.Length}";
	
	public override void Read(BinaryReader reader)
	{
		Authors = reader.ReadArray<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Authors);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as SiteAuthorsRemoval;
		
		foreach(var i in Authors)
			if(o.Authors.Contains(i))
				return true;
				
		return false;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		foreach(var i in Authors)
		{
			if(!Site.Publishers.Any(a => a.Author == i))
			{	
				error = NotFound;
				return false;
			}

			var a = execution.Authors.Find(i);

			if(a.Products.Select(i => execution.Products.Find(i)).Any(i => i.Publications.Any(p => execution.Publications.Find(p).Site == Site.Id)))
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
 		var s = Site;
 
 		foreach(var i in Authors)
 		{	
			s.Publishers = s.Publishers.Remove(s.Publishers.First(m => m.Author == i));
			
			var a = execution.Authors.Affect(i);
			a.Sites = a.Sites.Remove(s.Id);
		}

	}
}