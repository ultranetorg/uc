using System.Text;

namespace Uccs.Fair;

public class ReviewTextChange : FairOperation
{
	public AutoId				Review { get; set; }
	public string				Text { get; set; }

	public override bool		IsValid(McvNet net) => Text.Length <= (net as Fair).PostLengthMaximum;
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
		var p = execution.Publications.Affect(r.Publication);
		var a = execution.Authors.Affect(execution.Products.Find(execution.Publications.Find(r.Publication).Product).Author);

		ISpacetimeHolder h;
		ISpaceConsumer c;

		if(p.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
		{
			h = a; 
			c = a;
		}
		else
		{
			var s = execution.Sites.Affect(p.Site);
			h = s; 
			c = s;
		}

		Free(execution, h, c, Encoding.UTF8.GetByteCount(r.TextNew));
		Allocate(execution, h, c, Encoding.UTF8.GetByteCount(Text));

		r.TextNew = Text;

		if(!p.ReviewChanges.Contains(r.Id))
			p.ReviewChanges = [..p.ReviewChanges, r.Id];
		
		PayEnergyBySiteOrAuthor(execution, p, a);
	}
}
