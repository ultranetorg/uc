using System.Text;

namespace Uccs.Fair;

public class ReviewDeletion : VotableOperation
{
	public AutoId				Review { get; set; }

	public override bool		IsValid(McvNet net) => true; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Explanation => $"{Review}";

	public ReviewDeletion()
	{
	}

	public override void Read(Reader reader)
	{
		Review	= reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Review);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as ReviewDeletion).Review == Review;
	}

	public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(!ReviewExists(execution, Review, out var r, out error))
			return false;

		return true;
	}

	public override void Execute(FairExecution execution)
	{
		var v = execution.Reviews.Affect(Review);
		v.Deleted = true; 

		var p = execution.Publications.Affect(v.Publication);
		p.Reviews = p.Reviews.Remove(v.Id);

		var u = execution.AffectUser(v.Creator);
		u.Reviews = u.Reviews.Remove(v.Id);

		var r = execution.Products.Find(p.Product);
		var a = execution.Authors.Affect(r.Author);
		
		execution.Free(Store, a, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(v.Text));
		execution.RewardForModeration(Store, a, out Error);
	}
}
