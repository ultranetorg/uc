namespace Uccs.Fair;

public class DisputeCreation : FairOperation
{
	public EntityId				Site { get; set; }
	public Proposal				Proposal { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

	public DisputeCreation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Site = reader.Read<EntityId>();
		Proposal = reader.Read<Proposal>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(Proposal);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireSite(round, Site, out var s))
			return;

		if(s.Disputes.Any(i => round.FindDispute(i).Proposal == Proposal))
		{
			Error = AlreadyExists;
			return;
		}

		s = round.AffectSite(s.Id);

		var d = round.CreateDispute(Signer);

		AllocateEntity(Signer);

		s.Disputes = [..s.Disputes, d.Id];
	}
}