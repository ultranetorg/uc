namespace Uccs.Fair;

public class PublicationCategoryChange : VotableOperation
{
	public AutoId				Publication { get; set; }
	public AutoId				Category { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"{Publication}, [{Category}]";

	public PublicationCategoryChange()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication	= reader.Read<AutoId>();
		Category	= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Category);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as PublicationCategoryChange).Publication == Publication;
	}
	
	public override bool ValidProposal(FairExecution execution)
	{
		if(!RequirePublication(execution, Publication, out var p))
			return false;

		if(!RequireCategory(execution, Category, out var c))
			return false;

		return p.Category != Category;
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!ValidProposal(execution))
			return;

		if(!dispute)
	 	{
			if(!RequirePublicationModeratorAccess(execution, Publication, Signer, out var _, out var s))
				return;

	 		if(s.ChangePolicies[FairOperationClass.PublicationCategoryChange] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
		}


		var p = execution.Publications.Affect(Publication);
		p.Category = Category;

		var c = execution.Categories.Find(p.Category);
 		
		if(c.Publications.Contains(p.Id)) /// published
		{
			c = execution.Categories.Affect(p.Category);
			c.Publications = c.Publications.Remove(p.Id);

			c = execution.Categories.Affect(Category);
			c.Publications = [..c.Publications, p.Id];
		}
	}
}
