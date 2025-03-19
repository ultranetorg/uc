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

	public override bool ValidProposal(FairMcv mcv, FairRound round, Site site)
	{
		if(!RequirePublication(round, Publication, out var p))
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

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequirePublicationAccess(round, Publication, Signer, out var p, out var s))
			return;

 		if(s.ChangePolicies[FairOperationClass.PublicationStatusChange] != ChangePolicy.AnyModerator)
 		{
 			Error = Denied;
 			return;
 		}
 
		Execute(mcv, round, s);
	}

	public override void Execute(FairMcv mcv, FairRound round, SiteEntry site)
	{
		var p =	round.AffectPublication(Publication);

 		if(Status == PublicationStatus.Approved)
 		{
			p.Status = Status;

			if(p.Flags.HasFlag(PublicationFlags.CreatedByAuthor))
			{
				var a = round.AffectAuthor(round.FindProduct(p.Product).Author);

				PayForModeration(round, p, a);
			}
		}
		else if(Status == PublicationStatus.Rejected)
		{
			p.Deleted = true;
		}
	}
}
