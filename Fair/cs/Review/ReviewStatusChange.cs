namespace Uccs.Fair;

public class ReviewStatusChange : VotableOperation
{
	public EntityId				Review { get; set; }
	public ReviewStatus			Status { get; set; }

	public override bool		IsValid(Mcv mcv) => Review != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{Status}";

	public ReviewStatusChange()
	{
	}

	public override bool ValidProposal(FairMcv mcv, FairRound round, Site site)
	{
		if(!RequireReviewAccess(round, Review, Signer, out var r))
			return false;

		return true;
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as ReviewTextModeration).Review == Review;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Review	= reader.Read<EntityId>();
		Status	= reader.Read<ReviewStatus>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Review);
		writer.Write(Status);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!ValidProposal(mcv, round, null))
			return;

		if(round.FindSite(round.FindCategory(round.FindPublication(round.FindReview(Review).Publication).Category).Site).ChangePolicies[FairOperationClass.ReviewStatusChange] != ChangePolicy.AnyModerator)
 		{
 			Error = Denied;
 			return;
 		}

		Execute(mcv, round, null);
	}

	public override void Execute(FairMcv mcv, FairRound round, SiteEntry site)
	{
		if(!ValidProposal(mcv, round, null))
		{	
			Error = null;
			return;
		}

		var r = round.AffectReview(Review);
		r.Status = Status;

		var a = round.AffectAuthor(round.FindProduct(round.FindPublication(r.Publication).Product).Author);
		EnergySpenders = [a];
	}
}
