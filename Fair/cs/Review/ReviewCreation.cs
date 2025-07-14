using System.Text;

namespace Uccs.Fair;

public class ReviewCreation : FairOperation
{
	public AutoId				Publication { get; set; }
	public string				Text { get; set; }
	public byte					Rating { get; set; }

	public override bool		Sponsored => true;
	public override string		Explanation => $"{GetType().Name} Publication={Publication}";

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

	public override void Execute(FairExecution execution)
	{
		if(!PublicationExists(execution, Publication, out var p, out Error))
			return;

		var v = execution.Reviews.Create(p);

		v.Publication	= p.Id;
		v.Creator		= Signer.Id;
		v.Status		= ReviewStatus.Pending;
		v.Rating		= Rating;
		v.Text			= "";
		v.TextNew		= Text;
		v.Created		= execution.Time;

		p = execution.Publications.Affect(p.Id);
		p.Reviews = [..p.Reviews, v.Id];

		var s = execution.Sites.Affect(p.Site);
		s.ChangedReviews = [..s.ChangedReviews, v.Id];

		Signer.Reviews = [..Signer.Reviews, v.Id];

		if(Signer.Id == execution.LastCreatedId)
		{
			Signer.AllocationSponsor = new (s.Id, FairTable.Site);
		}

		execution.Allocate(s, s, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));
	}
}