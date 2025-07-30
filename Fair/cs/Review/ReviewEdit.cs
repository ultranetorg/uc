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

	public override void Read(BinaryReader reader)
	{
		Review	= reader.Read<AutoId>();
		Text	= reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Review);
		writer.WriteUtf8(Text);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as ReviewEdit;

		return Review == o.Review && o.Signer.Id == Signer.Id;
	}
	
	 public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(!IsReviewOwner(execution, Review, Signer, out var _, out error))
			return false;

		return true;
	}

	public override void Execute(FairExecution execution)
	{
		var r = execution.Reviews.Affect(Review);
		var p = execution.Publications.Affect(r.Publication);
		var s = execution.Sites.Affect(p.Site);

		execution.Free(s, s, Encoding.UTF8.GetByteCount(r.Text));
		execution.Allocate(s, s, Encoding.UTF8.GetByteCount(Text));

		r.Text = Text;

		if(p.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
		{ 
			var x = execution.Products.Find(p.Product);
			var a = execution.Authors.Affect(x.Author);
			
			RewardForModeration(execution, a, Site);
		}
	}
}
