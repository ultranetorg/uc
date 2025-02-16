using System.Text;

namespace Uccs.Fair;

public enum ReviewChange : byte
{
	None,
	Status,
	Delete,
	Text,
	Approve,
	Reject
}

public class ReviewUpdation : UpdateOperation
{
	public EntityId				Review { get; set; }
	public ReviewChange			Change { get; set; }

	public override bool		IsValid(Mcv mcv) => Review != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}, [{Change}]";

	const int					TextHsshLength = 16;

	public ReviewUpdation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Review	= reader.Read<EntityId>();
		Change	= reader.ReadEnum<ReviewChange>();
		
		Value = Change switch
					   {
							ReviewChange.Status		=> reader.ReadEnum<ReviewStatus>(),
							ReviewChange.Text		=> reader.ReadString(),
							ReviewChange.Approve	=> reader.ReadBytes(TextHsshLength),
							ReviewChange.Reject		=> reader.ReadBytes(TextHsshLength),
							_ => throw new IntegrityException()
					   };
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Review);
		writer.WriteEnum(Change);

		switch(Change)
		{
			case ReviewChange.Status	: writer.WriteEnum((ReviewStatus)Value); break;
			case ReviewChange.Text		: writer.Write(String); break;
			case ReviewChange.Approve	: writer.Write(Bytes); break;
			case ReviewChange.Reject	: writer.Write(Bytes); break;
			default						: throw new IntegrityException();
		}
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireReviewAccess(round, Review, Signer, out var r))
			return;

		r = round.AffectReview(Review);

		switch(Change)
		{
			case ReviewChange.Status:
			{
				var s = (ReviewStatus)Value;
				
				//if(r.Reward.Length > 0 && r.Status == ReviewStatus.Pending && s != ReviewStatus.Pending)
				//{
				//	Signer.ECBalanceAdd(r.Reward);
				//	r.Reward = [];
				//}

				r.Status = s;
				break;
			}

			case ReviewChange.Text:
			{
				var a = round.AffectAuthor(round.FindProduct(round.FindPublication(r.Publication).Product).Author);

				Free(round, a, a, Encoding.UTF8.GetByteCount(r.TextNew));
				Allocate(round, a, a, Encoding.UTF8.GetByteCount(Value as string));

				r.TextNew = Value as string;

				var p = round.AffectPublication(r.Publication);

				if(!p.ReviewChanges.Contains(r.Id))
					p.ReviewChanges = [..p.ReviewChanges, Review];

				break;
			}

			case ReviewChange.Approve:
			{
				var p = round.AffectPublication(r.Publication);

				if(p.ReviewChanges.Contains(r.Id))
				{
					Error = NotFound;
					return;
				}

				if(!Cryptography.Hash(TextHsshLength, Encoding.UTF8.GetBytes(r.TextNew)).SequenceEqual(Bytes))
				{
					Error = Mismatch;
					return;
				}

				var a = round.AffectAuthor(mcv.Products.Find(mcv.Publications.Find(r.Publication, round.Id).Product, round.Id).Author);

				Free(round, a, a, Encoding.UTF8.GetByteCount(r.Text));

				r.Text = r.TextNew;
				r.TextNew = "";
				p.ReviewChanges = [..p.ReviewChanges.Where(i => i != Review)];

				break;
			}

			case ReviewChange.Reject:
			{
				var p = round.AffectPublication(r.Publication);

				if(p.ReviewChanges.Contains(r.Id))
				{
					Error = NotFound;
					return;
				}

				if(!Cryptography.Hash(TextHsshLength, Encoding.UTF8.GetBytes(r.TextNew)).SequenceEqual(Bytes))
				{
					Error = Mismatch;
					return;
				}

				var a = round.AffectAuthor(mcv.Products.Find(mcv.Publications.Find(r.Publication, round.Id).Product, round.Id).Author);

				Free(round, a, a, Encoding.UTF8.GetByteCount(r.TextNew));

				r.TextNew = "";
				p.ReviewChanges = [..p.ReviewChanges.Where(i => i != Review)];

				break;
			}
		}
	}
}