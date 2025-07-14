using System.Text;

namespace Uccs.Fair;

public class ReviewEditModeration : VotableOperation
{
	const int							TextHashLength = 16;

	public AutoId						Review { get; set; }
	public byte[]						Hash { get; set; }
	public bool							Resolution { get; set; }

	public override bool				IsValid(McvNet net) => Hash.Length == TextHashLength;
	public override string				Explanation => $"{Hash.ToHex()}, {Resolution}";

	public ReviewEditModeration()
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
		return (other as ReviewEditModeration).Review == Review;
	}

	 public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(!ReviewExists(execution, Review, out var r, out error))
			return false;

		var p = execution.Publications.Find(r.Publication);

		if(!Site.ChangedReviews.Contains(r.Id))
		{	
			error = NotFound;
			return false;
		}

		if(!Cryptography.Hash(TextHashLength, Encoding.UTF8.GetBytes(r.TextNew)).SequenceEqual(Hash))
		{	
			error = Mismatch;
			return false;
		}

		return true;
	}

	public override void Execute(FairExecution execution)
	{
		var v = execution.Reviews.Affect(Review);
		var p = execution.Publications.Affect(v.Publication);
		var s = execution.Sites.Affect(p.Site);

		if(Resolution == true)
		{
			execution.Free(s, s, Encoding.UTF8.GetByteCount(v.Text));

			v.Text = v.TextNew;
			v.TextNew = "";
			s.ChangedReviews = [..s.ChangedReviews.Where(i => i != Review)];

			var c = execution.AffectAccount(v.Creator);
			c.Approvals++;
		}
		else
		{
			execution.Free(s, s, Encoding.UTF8.GetByteCount(v.TextNew));

			v.TextNew = "";
			s.ChangedReviews = s.ChangedReviews.Remove(Review);

			var c = execution.AffectAccount(v.Creator);
			c.Rejections++;

			if(c.Rejections > c.Approvals/3)
			{
				execution.DeleteAccount(c); /// TODO RESEARCH MORE
			}
		}

		if(p.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
		{ 
			var r = execution.Products.Find(p.Product);
			var a = execution.Authors.Affect(r.Author);

			RewardForModeration(execution, a, s);
		}
	}
}
