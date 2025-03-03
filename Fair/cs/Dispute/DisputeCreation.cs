namespace Uccs.Fair;

public class DisputeCreation : FairOperation
{
	public EntityId				Site { get; set; }
	public Proposal				Proposal { get; set; }
	public short				Days { get; set; }
	public bool					Referendum { get; set; }
	
	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

	public DisputeCreation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Site		= reader.Read<EntityId>();
		Proposal	= reader.Read<Proposal>();
		Days		= reader.ReadInt16();
		Referendum	= reader.ReadBoolean();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(Proposal);
		writer.Write(Days);
		writer.Write(Referendum);
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

		if(!Referendum && !s.ModerationPermissions.HasFlag(ModerationPermissions.ElectModerators))
		{
			Error = Denied;
			return;
		}

		var d = round.CreateDispute(s);

		d.Site = Site;
		d.Flags = (Referendum ? DisputeFlags.Referendum : 0);
		d.Proposal = Proposal;
		d.Expirtaion = round.ConsensusTime + Time.FromDays(Days);

		AllocateEntity(Signer);

		s = round.AffectSite(s.Id);
		s.Disputes = [..s.Disputes, d.Id];
	}
}