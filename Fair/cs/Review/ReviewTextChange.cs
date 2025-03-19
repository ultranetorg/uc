using System.Text;

namespace Uccs.Fair;

public class ReviewTextChange : FairOperation
{
	public EntityId				Review { get; set; }
	public string				Text { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Review}, {Text}";

	public ReviewTextChange()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Review	= reader.Read<EntityId>();
		Text	= reader.ReadUtf8();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Review);
		writer.WriteUtf8(Text);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireReviewAccess(round, Review, Signer, out var r))
			return;

		r = round.AffectReview(Review);
		var a = round.AffectAuthor(round.FindProduct(round.FindPublication(r.Publication).Product).Author);

		EnergySpenders = [a];

		Free(round, a, a, Encoding.UTF8.GetByteCount(r.TextNew));
		Allocate(round, a, a, Encoding.UTF8.GetByteCount(Text));

		r.TextNew = Text;

		var p = round.AffectPublication(r.Publication);

		if(!p.ReviewChanges.Contains(r.Id))
			p.ReviewChanges = [..p.ReviewChanges, Review];
	}
}
