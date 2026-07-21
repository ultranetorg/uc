using System.Text;

namespace Uccs.Fair;

public class ReviewEdit : VotableOperation
{
	public AutoId				Review { get; set; }
	public string				Text { get; set; }

	public override bool		IsValid(McvNet net) => Text.Length <= Fair.PostLengthMaximum;
	public override string		Explanation => $"{Review}, {Text}";

	public ReviewEdit()
	{
	}

	public override void Read(Reader reader)
	{
		Review	= reader.Read<AutoId>();
		Text	= reader.ReadUtf8();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Review);
		writer.WriteUtf8(Text);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as ReviewEdit;

		return Review == o.Review && o.User.Id == User.Id;
	}
	
	 public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(!IsReviewOwner(execution, Review, User, out var r, out error))
		{
			error = "Not a review owner";
			return false;
		}

		var p = execution.Publications.Find(r.Publication);

		if(!p.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
		{
			error = NotApproved;
			return false;
		}

		return true;
	}

	public override void Execute(FairExecution execution)
	{
		var v = execution.Reviews.Affect(Review);
		var p = execution.Publications.Find(v.Publication);
		var r = execution.Products.Find(p.Product);
		var a = execution.Authors.Affect(r.Author);

		execution.Free(a, a, Encoding.UTF8.GetByteCount(v.Text));
		v.Text = Text;
		execution.Allocate(Store, a, Encoding.UTF8.GetByteCount(Text), out Error);
		execution.RewardForModeration(Store, a, out Error);
	}
}
