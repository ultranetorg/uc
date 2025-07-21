using System.Text;

namespace Uccs.Fair;

public class ReviewCreation : VotableOperation
{
	public AutoId				Publication { get; set; }
	public string				Text { get; set; }
	public byte					Rating { get; set; }

	public override string		Explanation => $"Publication={Publication} Rating={Rating} Text={Text}";

	public override bool		IsValid(McvNet net) => Text.Length <= Fair.PostLengthMaximum && Rating <= 100;

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

		return Publication == o.Publication && o.Signer.Id == Signer.Id;
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
		v.Creator		= Signer.Id;
		v.Status		= ReviewStatus.Pending;
		v.Rating		= Rating;
		v.Text			= "";
		v.TextNew		= Text;
		v.Created		= execution.Time;

		var p = execution.Publications.Affect(Publication);
		p.Reviews = [..p.Reviews, v.Id];

		Site.ChangedReviews = [..Site.ChangedReviews, v.Id];

		Signer.Reviews = [..Signer.Reviews, v.Id];

		execution.Allocate(Site, Site, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));
	}
}