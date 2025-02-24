namespace Uccs.Fair;

public class DisputeVoting : FairOperation
{
	public EntityId				Dispute { get; set; }
	public bool					Pros { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

	public DisputeVoting()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Dispute	= reader.Read<EntityId>();
		Pros	= reader.ReadBoolean();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Dispute);
		writer.Write(Pros);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireDispute(round, Dispute, out var d))
			return;

		if(!d.Flags.HasFlag(DisputeFlags.Referendum))
		{
			if(!RequireSiteAccess(round, d.Site, out var s))
				return;
		}
		else
		{
			if(!Signer.Sites.Contains(d.Site))
			{
				Error = Denied;
				return;
			}
		}

		if(d.Pros.Contains(Signer.Id) || d.Cons.Contains(Signer.Id))
		{
			Error = AlreadyExists;
			return;
		}

// 		s = round.AffectSite(Site);
// 		s.Disputes = [..s.Disputes];

		d = round.AffectDispute(Dispute);

		if(Pros)
			d.Pros = [..d.Pros, Signer.Id];
		else
			d.Cons = [..d.Cons, Signer.Id];
	}
}