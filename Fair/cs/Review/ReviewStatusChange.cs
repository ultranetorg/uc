namespace Uccs.Fair;

public class ReviewStatusChange : VotableOperation
{
	public EntityId				Review { get; set; }
	public ReviewStatus			Status { get; set; }

	public override bool		IsValid(McvNet net) => Review != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{Status}";

	public ReviewStatusChange()
	{
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

	public override bool Overlaps(VotableOperation other)
	{
		return (other as ReviewTextModeration).Review == Review;
	}

	public override bool ValidProposal(FairExecution execution)
	{
		if(!RequireReview(execution, Review, out var r))
			return false;

		return r.Status != Status;
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!ValidProposal(execution))
			return;

		if(!dispute)
	 	{
			if(!RequireReviewModertorAccess(execution, Review, Signer, out var _, out var s))
				return;

	 		if(s.ChangePolicies[FairOperationClass.ReviewStatusChange] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
		}	 	

		var r = execution.AffectReview(Review);
		r.Status = Status;

		var a = execution.AffectAuthor(execution.FindProduct(execution.FindPublication(r.Publication).Product).Author);
		EnergySpenders = [a];
	}
}
