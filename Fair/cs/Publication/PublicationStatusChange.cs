namespace Uccs.Fair;

public class PublicationStatusChange : PublicationUpdation
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

		return p.Status == PublicationStatus.Pending && Status == PublicationStatus.Approved;
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
 
 		if(p.Status == PublicationStatus.Pending && Status == PublicationStatus.Approved) /// When Author asked to publish
 		{
			Execute(mcv, round, s);
 		}
		else
		{
			Error = NotAvailable;
			return;
		}
	}

	public override void Execute(FairMcv mcv, FairRound round, SiteEntry site)
	{
		var p =	round.AffectPublication(Publication);
		var a = round.AffectAuthor(round.FindProduct(p.Product).Author);

		Pay(round, p, a);

		p.Status = PublicationStatus.Approved;
	}
}
