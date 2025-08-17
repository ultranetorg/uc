namespace Uccs.Fair;

public class SiteAuthorsChange : VotableOperation
{
	public AutoId[]				Additions { get; set; }
	public AutoId[]				Removals { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Site={Site}, Additions={Additions.Length}, Removals={Removals.Length}";
	
	public override void Read(BinaryReader reader)
	{
		Additions = reader.ReadArray<AutoId>();
		Removals = reader.ReadArray<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Additions);
		writer.Write(Removals);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as SiteAuthorsChange;
		
		foreach(var i in Additions)
			if(o.Additions.Contains(i) || o.Removals.Contains(i))
				return true;
	
		foreach(var i in Removals)
			if(o.Additions.Contains(i) || o.Removals.Contains(i))
				return true;
				
		return false;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		foreach(var i in Additions)
		{
			if(Site.Publishers.Any(a => a.Author == i))
			{	
				error = AlreadyExists;
				return false;
			}

			if(!AuthorExists(execution, i, out _, out error))
				return false;
		}
	
		foreach(var i in Removals)
			if(!Site.Publishers.Any(a => a.Author == i))
			{	
				error = NotFound;
				return false;
			}
	
		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
 		var s = Site;
 
 		s.Publishers = [..s.Publishers, ..Additions.Select(i => new Publisher {Author = i})];
 
 		foreach(var i in Removals)
 			s.Publishers = s.Publishers.Remove(s.Publishers.First(m => m.Author == i));
	}
}