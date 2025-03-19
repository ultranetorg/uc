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

	public override bool ValidProposal(FairExecution execution, SiteEntry site)
	{
		if(!RequireReviewAccess(execution, Review, Signer, out var r))
			return false;

		if(!RequirePublication(execution, r.Publication, out var p))
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

	public override void Execute(FairExecution execution)
	{
		if(!ValidProposal(execution, null))
			return;

		if(execution.FindSite(execution.FindCategory(execution.FindPublication(execution.FindReview(Review).Publication).Category).Site).ChangePolicies[FairOperationClass.ReviewTextModeration] != ChangePolicy.AnyModerator)
 		{
 			Error = Denied;
 			return;
 		}

		Execute(execution, null);
	}

	public override void Execute(FairExecution execution, SiteEntry site)
	{
		if(!ValidProposal(execution, null))
		{	
			Error = null;
			return;
		}

		var r = execution.AffectReview(Review);

		var a = execution.AffectAuthor(execution.FindProduct(execution.FindPublication(r.Publication).Product).Author);
		EnergySpenders = [a];
		
		var p = execution.AffectPublication(r.Publication);

		if(Resolution == true)
		{
			Free(execution, a, a, Encoding.UTF8.GetByteCount(r.Text));

			r.Text = r.TextNew;
			r.TextNew = "";
			p.ReviewChanges = [..p.ReviewChanges.Where(i => i != Review)];

			var c = execution.AffectAccount(r.Creator);
			c.Approvals++;
		}
		else
		{
			Free(execution, a, a, Encoding.UTF8.GetByteCount(r.TextNew));

			r.TextNew = "";
			p.ReviewChanges = [..p.ReviewChanges.Where(i => i != Review)];

			var c = execution.AffectAccount(r.Creator);
			c.Rejections++;

			if(c.Rejections > c.Approvals/3)
			{
				execution.DeleteAccount(c);
			}
		}

		PayForModeration(execution, p, a);
	}
}
