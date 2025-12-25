using System.Text;

namespace Uccs.Fair;

public class ReviewCreation : VotableOperation
{
	public AutoId				Publication { get; set; }
	public string				Text { get; set; }
	public byte					Rating { get; set; }

	public override bool		IsValid(McvNet net) => Text.Length <= Fair.PostLengthMaximum && Rating <= 100;
	public override string		Explanation => $"Publication={Publication} Rating={Rating} Text={Text}";

	public override void Read(BinaryReader reader)
	{
		Publication = reader.Read<AutoId>();
		Rating		= reader.ReadByte();
		Text		= reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Rating);
		writer.WriteUtf8(Text);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as ReviewCreation;

		return Publication == o.Publication && o.User.Id == User.Id;
	}
	
	 public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(!PublicationExists(execution, Publication, out var p, out error))
			return false;

		return true;
	}

	public override void Execute(FairExecution execution)
	{
		var v = execution.Reviews.Create(Publication);

		v.Publication	= Publication;
		v.Creator		= User.Id;
		v.Status		= ReviewStatus.Accepted;
		v.Rating		= Rating;
		v.Text			= Text;
		v.Created		= execution.Time;

		var p = execution.Publications.Affect(Publication);
		p.Reviews = [..p.Reviews, v.Id];
		p.Rating = (byte)((p.Rating + v.Rating)/2);

		User.Reviews = [..User.Reviews, v.Id];

		if(p.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
		{ 
			var x = execution.Products.Find(p.Product);
			var a = execution.Authors.Affect(x.Author);
			var pb = Site.Publishers.First(i => i.Author == a.Id);
			
			RewardForModeration(execution, a, Site);
			execution.Allocate(a, pb, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text), out Error);
		}
		else
			execution.Allocate(Site, Site, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));
	}
}