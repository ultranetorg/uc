using System.Text;

namespace Uccs.Fair;

public class ReviewTextChange : FairOperation
{
	public AutoId				Review { get; set; }
	public string				Text { get; set; }

	public override bool		IsValid(McvNet net) => Text.Length <= (net as Fair).ReviewLengthMaximum;
	public override string		Explanation => $"{Review}, {Text}";

	public ReviewTextChange()
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

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireReviewOwnerAccess(execution, Review, Signer, out var r))
			return;

		r = execution.Reviews.Affect(Review);
		var a = execution.Authors.Affect(execution.Products.Find(execution.Publications.Find(r.Publication).Product).Author);

		EnergySpenders = [a];

		Free(execution, a, a, Encoding.UTF8.GetByteCount(r.TextNew));
		Allocate(execution, a, a, Encoding.UTF8.GetByteCount(Text));

		r.TextNew = Text;

		var p = execution.Publications.Affect(r.Publication);

		if(!p.ReviewChanges.Contains(r.Id))
			p.ReviewChanges = [..p.ReviewChanges, Review];
	}
}
