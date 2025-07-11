using System.Text;

namespace Uccs.Fair;

public class ReviewEdit : FairOperation
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

	public override void Execute(FairExecution execution)
	{
		if(!IsReviewOwner(execution, Review, Signer, out var r, out Error))
			return;

		r = execution.Reviews.Affect(Review);
		var p = execution.Publications.Affect(r.Publication);
		var s = execution.Sites.Affect(p.Site);

		execution.Free(s, s, Encoding.UTF8.GetByteCount(r.TextNew));
		execution.Allocate(s, s, Encoding.UTF8.GetByteCount(Text));

		r.TextNew = Text;

		if(!s.ChangedReviews.Contains(r.Id))
			s.ChangedReviews = [..s.ChangedReviews, r.Id];
	}
}
