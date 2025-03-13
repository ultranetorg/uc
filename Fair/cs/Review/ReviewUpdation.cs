using System.Text;

namespace Uccs.Fair;

public class ReviewStatusUpdation : FairOperation
{
	public EntityId				Review { get; set; }
	public ReviewStatus			Status { get; set; }

	public override bool		IsValid(Mcv mcv) => Review != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{Status}";

	public ReviewStatusUpdation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Review	= reader.Read<EntityId>();
		Status	= reader.ReadEnum<ReviewStatus>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Review);
		writer.WriteEnum(Status);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireReviewAccess(round, Review, Signer, out var r))
			return;

		r = round.AffectReview(Review);
		r.Status = Status;

		var a = round.AffectAuthor(round.FindProduct(round.FindPublication(r.Publication).Product).Author);
		EnergySpenders = [a];
	}
}

public class ReviewTextUpdation : FairOperation
{
	public EntityId				Review { get; set; }
	public string				Text { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Review}, {Text}";

	public ReviewTextUpdation()
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

public class ReviewTextModeration : FairOperation
{
	const int							TextHashLength = 16;

	public EntityId						Review { get; set; }
	public byte[]						Hash { get; set; }
	public bool							Resolution { get; set; }

	public override bool				IsValid(Mcv mcv) => Hash.Length == TextHashLength;
	public override string				Description => $"{Hash.ToHex()}, {Resolution}";

	public ReviewTextModeration()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Review		= reader.Read<EntityId>();
		Hash		= reader.ReadBytes(TextHashLength);
		Resolution	= reader.ReadBoolean();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Review);
		writer.Write(Hash);
		writer.Write(Resolution);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireReviewAccess(round, Review, Signer, out var r))
			return;

		r = round.AffectReview(Review);

		var a = round.AffectAuthor(round.FindProduct(round.FindPublication(r.Publication).Product).Author);
		EnergySpenders = [a];

		if(Resolution == true)
		{
			var p = round.AffectPublication(r.Publication);

			if(p.ReviewChanges.Contains(r.Id))
			{
				Error = NotFound;
				return;
			}

			if(!Cryptography.Hash(TextHashLength, Encoding.UTF8.GetBytes(r.TextNew)).SequenceEqual(Hash))
			{
				Error = Mismatch;
				return;
			}

			Free(round, a, a, Encoding.UTF8.GetByteCount(r.Text));

			r.Text = r.TextNew;
			r.TextNew = "";
			p.ReviewChanges = [..p.ReviewChanges.Where(i => i != Review)];

			var c = round.AffectAccount(r.Creator);
			c.Approvals++;
		}
		else
		{
			var p = round.AffectPublication(r.Publication);

			if(p.ReviewChanges.Contains(r.Id))
			{
				Error = NotFound;
				return;
			}

			if(!Cryptography.Hash(TextHashLength, Encoding.UTF8.GetBytes(r.TextNew)).SequenceEqual(Hash))
			{
				Error = Mismatch;
				return;
			}

			Free(round, a, a, Encoding.UTF8.GetByteCount(r.TextNew));

			r.TextNew = "";
			p.ReviewChanges = [..p.ReviewChanges.Where(i => i != Review)];

			var c = round.AffectAccount(r.Creator);
			c.Rejections++;

			if(c.Rejections > c.Approvals/3)
			{
				round.DeleteAccount(c);
			}
		}
	}
}
