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

	public override bool ValidProposal(FairExecution execution, SiteEntry site)
	{
		if(!RequireReviewAccess(execution, Review, Signer, out var r))
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

	public override void Execute(FairExecution execution)
	{
		if(!ValidProposal(execution, null))
			return;

		if(execution.FindSite(execution.FindCategory(execution.FindPublication(execution.FindReview(Review).Publication).Category).Site).ChangePolicies[FairOperationClass.ReviewStatusChange] != ChangePolicy.AnyModerator)
 		{
 			Error = Denied;
 			return;
 		}

		Execute(execution, null);
	}

	public override void Execute(FairExecution execution, SiteEntry site)
	{
		if(!ValidProposal(execution, null))
		{	
			Error = null;
			return;
		}

		var r = execution.AffectReview(Review);
		r.Status = Status;

		var a = execution.AffectAuthor(execution.FindProduct(execution.FindPublication(r.Publication).Product).Author);
		EnergySpenders = [a];
	}
}
