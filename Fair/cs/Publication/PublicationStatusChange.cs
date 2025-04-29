namespace Uccs.Fair;

public class PublicationStatusChange : VotableOperation
{
	public AutoId				Publication { get; set; }
	public PublicationStatus	Status { get; set; }

	public override bool		IsValid(McvNet net) => Publication != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Explanation => $"{Publication}, {Status}";

	public PublicationStatusChange()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication	= reader.Read<AutoId>();
		Status		= reader.Read<PublicationStatus>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Status);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as PublicationStatusChange).Publication == Publication;
	}

	public override bool ValidProposal(FairExecution execution)
	{
		if(!RequirePublication(execution, Publication, out var p))
			return false;

		return p.Status == PublicationStatus.Pending && (Status == PublicationStatus.Approved || Status == PublicationStatus.Rejected);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!ValidProposal(execution))
			return;

		if(!dispute)
	 	{
			if(!RequirePublicationModeratorAccess(execution, Publication, Signer, out var _, out var s))
				return;

	 		if(s.ChangePolicies[FairOperationClass.PublicationStatusChange] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
		} 	

		var p =	execution.AffectPublication(Publication);

 		if(Status == PublicationStatus.Approved)
 		{
			p.Status = Status;

			if(p.Flags.HasFlag(PublicationFlags.CreatedByAuthor))
			{
				var a = execution.AffectAuthor(execution.FindProduct(p.Product).Author);

				PayForModeration(execution, p, a);
			}

			var tr = p.Fields.FirstOrDefault(i => i.Name == ProductField.Title);
			
			if(tr != null)
				execution.PublicationTitles.Index(execution.FindCategory(p.Category).Site, p.Id, execution.FindProduct(p.Product).Get(tr).AsUtf8);
		}
		else if(Status == PublicationStatus.Rejected)
		{
			p.Deleted = true;
		}
	}
}
