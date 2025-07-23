namespace Uccs.Fair;

public class AuthorLinksChange : FairOperation
{
	public AutoId					Author { get; set; }
	public string[]					Additions { get; set; }
	public string[]					Removals { get; set; }

	public override bool			IsValid(McvNet net) => Additions.Any() || Removals.Any();
	public override string			Explanation => $"Author={Author} Additions={Additions.Length} Removals={Removals.Length}";
	
	public override void Read(BinaryReader reader)
	{
		Author		= reader.Read<AutoId>();
		Additions	= reader.ReadStrings();
		Removals	= reader.ReadStrings();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Author);
		writer.Write(Additions);
		writer.Write(Removals);
	}
//
//	public override bool Overlaps(VotableOperation other)
//	{
//		var o = other as AuthorLinksChange;
//		
//		if(o.Author != Author)
//			return true;
//
//		foreach(var i in Additions)
//			if(o.Additions.Contains(i) || o.Removals.Contains(i))
//				return true;
//	
//		foreach(var i in Removals)
//			if(o.Additions.Contains(i) || o.Removals.Contains(i))
//				return true;
//				
//		return false;
//	}
//
// 	public override bool ValidateProposal(FairExecution execution, out string error)
// 	{
//		error = null;
//		return true;
// 	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAuthor(execution, Author, out var a, out Error))
			return;

		a = execution.Authors.Affect(Author);
		 
 		a.Links = [..a.Links, ..Additions];
 
 		foreach(var i in Removals)
 			a.Links = a.Links.Remove(i);

		execution.PayCycleEnergy(a);
	}
}