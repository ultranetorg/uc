namespace Uccs.Fair;

public class PublicationStatusChange : VotableOperation
{
	public EntityId				Publication { get; set; }
	public PublicationStatus	Status { get; set; }

	public override bool		IsValid(Mcv mcv) => Publication != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{Publication}, {Status}";

	public PublicationStatusChange()
	{
	}

	public override bool ValidProposal(FairExecution execution, SiteEntry site)
	{
		if(!RequirePublication(execution, Publication, out var p))
			return false;

		return p.Status == PublicationStatus.Pending && (Status == PublicationStatus.Approved || Status == PublicationStatus.Rejected);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as PublicationStatusChange).Publication == Publication;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Publication	= reader.Read<EntityId>();
		Status		= reader.Read<PublicationStatus>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Status);
	}

	public override void Execute(FairExecution execution)
	{
		if(!RequirePublicationAccess(execution, Publication, Signer, out var p, out var s))
			return;

 		if(s.ChangePolicies[FairOperationClass.PublicationStatusChange] != ChangePolicy.AnyModerator)
 		{
 			Error = Denied;
 			return;
 		}
 
		Execute(execution, s);
	}

	public override void Execute(FairExecution execution, SiteEntry site)
	{
		var p =	execution.AffectPublication(Publication);

 		if(Status == PublicationStatus.Approved)
 		{
			p.Status = Status;

			if(p.Flags.HasFlag(PublicationFlags.CreatedByAuthor))
			{
				var a = execution.AffectAuthor(execution.FindProduct(p.Product).Author);

				PayForModeration(execution, p, a);
			}
		}
		else if(Status == PublicationStatus.Rejected)
		{
			p.Deleted = true;
		}
	}
}
