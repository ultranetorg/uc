namespace Uccs.Smp;

public class ReviewCreation : SmpOperation
{
	public EntityId				Publication { get; set; }
	public string				Text { get; set; }

	public override bool		IsValid(Mcv mcv) => Text.Length <= 4096;
	public override string		Description => $"{GetType().Name} Publication={Publication}";

	public override void ReadConfirmed(BinaryReader reader)
	{
		Publication = reader.Read<EntityId>();
		Text = reader.ReadString();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Text);
	}

	public override void Execute(SmpMcv mcv, SmpRound round)
	{
		if(!RequirePublication(round, Publication, out var p))
			return;
					
		var r = round.CreateReview(p);

		r.Publication	= p.Id;
		r.User			= Signer.Id;
		r.Status		= ReviewStatus.Active;
		r.Text			= Text;
		r.Created		= round.ConsensusTime;

		p = round.AffectPublication(p.Id);

		p.Reviews = [..p.Reviews, r.Id];
	}
}