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

		if(p.Category == null)
		{
			Error = CategoryNotSet;
			return;
		}

		var s = execution.Sites.Affect(p.Site);
		
		if(!s.PendingPublications.Contains(p.Id))
		{
			Error = NotAvailable;
			return;
		}

		s.PendingPublications = s.PendingPublications.Remove(p.Id);
		
		var c = execution.Categories.Find(p.Category);
		var r = execution.Products.Find(p.Product);
		var a = execution.Authors.Find(r.Author);

		PayEnergy(execution, p, a);

		if(Error != null)
			return;

		if(!s.Authors.Contains(a.Id))
		{
			a = execution.Authors.Affect(a.Id);
			s = execution.Sites.Affect(s.Id);

			s.Authors = [..s.Authors, a.Id];
			a.Sites = [.. a.Sites, s.Id];
		}

		if(!c.Publications.Contains(p.Id))
		{
			c = execution.Categories.Affect(c.Id);
			c.Publications = [..c.Publications, p.Id];

			s.PublicationsCount++;
		}

		var tr = p.Fields.FirstOrDefault(i => i.Name == ProductField.Title);
			
		if(tr != null)
			execution.PublicationTitles.Index(s.Id, p.Id, r.Get(tr).AsUtf8);
	}
}
