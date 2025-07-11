namespace Uccs.Fair;

public class ReviewStatusChange : VotableOperation
{
	public AutoId				Review { get; set; }
	public ReviewStatus			Status { get; set; }

	public override bool		IsValid(McvNet net) => Review != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Explanation => $"{Status}";

	public ReviewStatusChange()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Review	= reader.Read<AutoId>();
		Status	= reader.Read<ReviewStatus>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Review);
		writer.Write(Status);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as ReviewStatusChange).Review == Review;
	}

	public override bool ValidateProposal(FairExecution execution)
	{
		if(!ReviewExists(execution, Review, out var r, out _))
			return false;

		return r.Status != Status;
	}

	public override void Execute(FairExecution execution)
	{
		var v = execution.Reviews.Affect(Review);

		Publication p;

		if(Status == ReviewStatus.Accepted)
		{
			p = execution.Publications.Affect(v.Publication);
			p.Rating = (byte)((p.Rating + v.Rating)/2);
		}
		else
			p = execution.Publications.Find(v.Publication);
	
		//PayEnergyForModeration(execution, a, execution.Sites.Affect(Site.Id));

		if(v.Status == ReviewStatus.Pending)
		{
			if(p.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
			{ 
				var s = execution.Sites.Affect(p.Site);
				var r = execution.Products.Find(p.Product);
				var a = execution.Authors.Affect(r.Author);
			
				RewardForModeration(execution, a, s);
			}
		}

		v.Status = Status;
	}
}
