using System.Text;

namespace Uccs.Fair;

public class ReviewCreation : FairOperation
{
	public AutoId				Publication { get; set; }
	public string				Text { get; set; }
	public byte					Rate { get; set; }

	public override bool		NonExistingSignerAllowed => true;
	public override string		Explanation => $"{GetType().Name} Publication={Publication}";

	public override bool		IsValid(McvNet net) => Text.Length <= (net as Fair).PostLengthMaximum && Rate <= 100;

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
		r.Rating			= Rate;
		r.Text			= "";
		r.TextNew		= Text;
		r.Created		= execution.Time;

		p = execution.Publications.Affect(p.Id);

		p.Reviews		= [..p.Reviews, r.Id];
		p.ReviewChanges = [..p.ReviewChanges, r.Id];

		Signer.Reviews = [..Signer.Reviews, r.Id];

		var a = execution.Authors.Affect(execution.Products.Find(p.Product).Author);

		ISpacetimeHolder sh;
		ISpaceConsumer sc;

		if(p.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
		{
			sh = a; 
			sc = a;

			if(Signer.Id == execution.LastCreatedId)
			{
				Signer.AllocationSponsor		= a.Id;
				Signer.AllocationSponsorClass	= execution.Mcv.Authors.Id;
			}
		}
		else
		{
			var s = execution.Sites.Affect(p.Site);
			sh = s; 
			sc = s;

			if(Signer.Id == execution.LastCreatedId)
			{
				Signer.AllocationSponsor		= s.Id;
				Signer.AllocationSponsorClass	= execution.Mcv.Sites.Id;
			}
		}

		Allocate(execution, sh, sc, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));

		PayEnergyBySiteOrAuthor(execution, p, a);
	}
}