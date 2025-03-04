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

		if(!Proposal.Valid(s, round))
		{
			Error = InvalidProposal;
			return;
		}

		if(s.Disputes.Any(i => round.FindDispute(i).Proposal.Overlaps(Proposal)))
		{
			Error = AlreadyExists;
			return;
		}

		if(Referendum)
		{
			if(Proposal.Change == ProposalChange.Authors)
			{
				Error = Denied;
				return;
			}
		}
		else
		{
			if(Proposal.Change == ProposalChange.Moderators && !(s.ModeratorElectionPolicy == ElectionPolicy.AnyModerator ||
																 s.ModeratorElectionPolicy == ElectionPolicy.ModeratorsMajority ||
																 s.ModeratorElectionPolicy == ElectionPolicy.ModeratorsUnanimously))
			{
				Error = Denied;
				return;
			}
	
			if(Proposal.Change == ProposalChange.ModeratorElectionPolicy)
			{
				Error = Denied;
				return;
			}
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