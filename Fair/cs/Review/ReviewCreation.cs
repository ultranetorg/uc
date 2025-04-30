using System.Text;

namespace Uccs.Fair;

public class ReviewCreation : FairOperation
{
	public AutoId				Publication { get; set; }
	public string				Text { get; set; }
	public byte					Rate { get; set; }

	public override bool		NonExistingSignerAllowed => true;
	public override string		Explanation => $"{GetType().Name} Publication={Publication}";

	public override bool		IsValid(McvNet net) => Text.Length <= (net as Fair).ReviewLengthMaximum;

	public override void Read(BinaryReader reader)
	{
		Publication = reader.Read<AutoId>();
		Rate		= reader.ReadByte();
		Text		= reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Rate);
		writer.WriteUtf8(Text);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequirePublication(execution, Publication, out var p))
			return;

		var r = execution.Reviews.Create(p);

		r.Publication	= p.Id;
		r.Creator		= Signer.Id;
		r.Status		= ReviewStatus.Pending;
		r.Rate			= Rate;
		r.Text			= "";
		r.TextNew		= Text;
		r.Created		= execution.Time;

		p = execution.Publications.Affect(p.Id);

		p.Reviews = [..p.Reviews, r.Id];
		p.ReviewChanges = [..p.ReviewChanges, r.Id];

		var a = execution.Authors.Affect(execution.Products.Find(p.Product).Author);

		Allocate(execution, a, a, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));

		if(Signer.Id == execution.LastCreatedId)
		{
			Signer.AllocationSponsor		= a.Id;
			Signer.AllocationSponsorClass	= execution.Mcv.Authors.Id;
		}

		Signer.Reviews = [..Signer.Reviews, r.Id];

		EnergyFeePayer = a;
		EnergySpenders.Add(a);
	}
}