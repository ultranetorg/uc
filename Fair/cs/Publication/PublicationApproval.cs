namespace Uccs.Fair;

public enum PublicationStatus : byte
{
	None,
	Approve,
	Reject,
}

public class PublicationApproval : VotableOperation
{
	public AutoId				Publication { get; set; }
	//public PublicationStatus	Status { get; set; }

	public override bool		IsValid(McvNet net) => Publication != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Explanation => $"{Publication}";

	public PublicationApproval()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication	= reader.Read<AutoId>();
		//Status		= reader.Read<PublicationStatus>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
		//writer.Write(Status);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as PublicationApproval).Publication == Publication;
	}

	public override bool ValidProposal(FairExecution execution)
	{
		if(!RequirePublication(execution, Publication, out var p))
			return false;

		return true;
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!ValidProposal(execution))
			return;

		if(!dispute)
	 	{
			if(!RequirePublicationModeratorAccess(execution, Publication, Signer, out var _, out var ss))
				return;

	 		if(ss.ChangePolicies[FairOperationClass.PublicationApproval] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
		} 	

		var p =	execution.Publications.Affect(Publication);
		var c = execution.Categories.Find(p.Category);
		var s = execution.Sites.Affect(c.Site);


		if(!c.Publications.Contains(p.Id))
		{
			c = execution.Categories.Affect(c.Id);
			c.Publications = [..c.Publications, p.Id];

			s.PublicationsCount++;
		}

		if(s.PendingPublications.Contains(p.Id))
		{
			s.PendingPublications = s.PendingPublications.Remove(p.Id);
		}

		if(p.Flags.HasFlag(PublicationFlags.CreatedByAuthor))
		{
			var a = execution.Authors.Affect(execution.Products.Find(p.Product).Author);

			PayForModeration(execution, p, a);
		}

		var tr = p.Fields.FirstOrDefault(i => i.Name == ProductField.Title);
			
		if(tr != null)
			execution.PublicationTitles.Index(execution.Categories.Find(p.Category).Site, p.Id, execution.Products.Find(p.Product).Get(tr).AsUtf8);
		
		//else if(Status == PublicationStatus.Reject)
		//{
		//	if(c.Publications.Contains(p.Id))
		//	{
		//		c = execution.Categories.Affect(c.Id);
		//		c.Publications = c.Publications.Remove(p.Id);
		//	}
		//
		//	if(s.PendingPublications.Contains(p.Id))
		//	{
		//		s = execution.Sites.Affect(s.Id);
		//		s.PendingPublications = s.PendingPublications.Remove(p.Id);
		//	}
		//
		//	var tr = p.Fields.FirstOrDefault(i => i.Name == ProductField.Title);
		//
		//	if(tr != null)
		//		execution.PublicationTitles.Deindex(execution.Categories.Find(p.Category).Site, execution.Products.Find(p.Product).Get(tr).AsUtf8);
		//
		//	p.Deleted = true;
		//}
	}
}
