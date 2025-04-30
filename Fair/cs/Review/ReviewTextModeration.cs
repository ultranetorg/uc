using System.Text;

namespace Uccs.Fair;

public class ReviewTextModeration : VotableOperation
{
	const int							TextHashLength = 16;

	public AutoId						Review { get; set; }
	public byte[]						Hash { get; set; }
	public bool							Resolution { get; set; }

	public override bool				IsValid(McvNet net) => Hash.Length == TextHashLength;
	public override string				Explanation => $"{Hash.ToHex()}, {Resolution}";

	public ReviewTextModeration()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Review		= reader.Read<AutoId>();
		Hash		= reader.ReadBytes(TextHashLength);
		Resolution	= reader.ReadBoolean();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Review);
		writer.Write(Hash);
		writer.Write(Resolution);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as ReviewTextModeration).Review == Review;
	}

	public override bool ValidProposal(FairExecution execution)
	{
		if(!RequireReview(execution, Review, out var r))
			return false;

		var p = execution.Publications.Find(r.Publication);

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

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!ValidProposal(execution))
			return;

		if(!dispute)
	 	{
			if(!RequireReviewModertorAccess(execution, Review, Signer, out var _, out var s))
				return;

	 		if(s.ChangePolicies[FairOperationClass.ReviewTextModeration] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
		}	 	

		var r = execution.Reviews.Affect(Review);

		var a = execution.Authors.Affect(execution.Products.Find(execution.Publications.Find(r.Publication).Product).Author);
		EnergySpenders = [a];
		
		var p = execution.Publications.Affect(r.Publication);

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
