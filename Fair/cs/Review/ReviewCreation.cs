using System.Text;

namespace Uccs.Fair;

public class ReviewCreation : FairOperation
{
	public EntityId				Publication { get; set; }
	public string				Text { get; set; }
	public byte					Rate { get; set; }
	public override bool		NonExistingSignerAllowed => true;

	public override string		Description => $"{GetType().Name} Publication={Publication}";

	public override bool		IsValid(Mcv mcv) => Text.Length <= 4096;

	public override void ReadConfirmed(BinaryReader reader)
	{
		Publication = reader.Read<EntityId>();
		Rate		= reader.ReadByte();
		Text		= reader.ReadString();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Rate);
		writer.Write(Text);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequirePublication(round, Publication, out var p))
			return;

		var r = round.CreateReview(p);

		r.Publication	= p.Id;
		r.Creator		= Signer.Id;
		r.Status		= ReviewStatus.Pending;
		r.Rate			= Rate;
		r.Text			= "";
		r.TextNew		= Text;
		r.Created		= round.ConsensusTime;

		p = round.AffectPublication(p.Id);

		p.Reviews = [..p.Reviews, r.Id];
		p.ReviewChanges = [..p.ReviewChanges, r.Id];

		var a = round.AffectAuthor(round.FindProduct(p.Product).Author);

		Allocate(round, a, a, mcv.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));

		if(Signer.Id == round.LastCreatedId)
		{
			Signer.AllocationSponsor		= a.Id;
			Signer.AllocationSponsorClass	= mcv.Authors.Id;
		}

		Signer.Reviews = [..Signer.Reviews, r.Id];

		EnergyFeePayer = a;
		EnergySpenders.Add(a);
	}
}