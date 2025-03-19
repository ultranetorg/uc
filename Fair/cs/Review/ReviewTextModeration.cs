using System.Text;

namespace Uccs.Fair;

public class ReviewTextModeration : VotableOperation
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

	public override bool ValidProposal(FairMcv mcv, FairRound round, Site site)
	{
		if(!RequireReviewAccess(round, Review, Signer, out var r))
			return false;

		if(!RequirePublication(round, r.Publication, out var p))
			return false;

		if(!p.ReviewChanges.Contains(r.Id))
		{	
			Error = NotFound;
			return false;
		}

		if(!Cryptography.Hash(TextHashLength, Encoding.UTF8.GetBytes(r.TextNew)).SequenceEqual(Hash))
		{	
			Error = Mismatch;
			return false;
		}

		return true;
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as ReviewTextModeration).Review == Review;
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
		if(!ValidProposal(mcv, round, null))
			return;

		if(round.FindSite(round.FindCategory(round.FindPublication(round.FindReview(Review).Publication).Category).Site).ChangePolicies[FairOperationClass.ReviewTextModeration] != ChangePolicy.AnyModerator)
 		{
 			Error = Denied;
 			return;
 		}

		Execute(mcv, round, null);
	}

	public override void Execute(FairMcv mcv, FairRound round, SiteEntry site)
	{
		if(!ValidProposal(mcv, round, null))
		{	
			Error = null;
			return;
		}

		var r = round.AffectReview(Review);

		var a = round.AffectAuthor(round.FindProduct(round.FindPublication(r.Publication).Product).Author);
		EnergySpenders = [a];
		
		var p = round.AffectPublication(r.Publication);

		if(Resolution == true)
		{
			Free(round, a, a, Encoding.UTF8.GetByteCount(r.Text));

			r.Text = r.TextNew;
			r.TextNew = "";
			p.ReviewChanges = [..p.ReviewChanges.Where(i => i != Review)];

			var c = round.AffectAccount(r.Creator);
			c.Approvals++;
		}
		else
		{
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

		PayForModeration(round, p, a);
	}
}
