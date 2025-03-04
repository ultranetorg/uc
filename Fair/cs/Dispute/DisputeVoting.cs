namespace Uccs.Fair;

public class DisputeVoting : FairOperation
{
	public EntityId				Dispute { get; set; }
	public EntityId				Voter { get; set; }
	public bool					Pro { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

	public DisputeVoting()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Dispute	= reader.Read<EntityId>();
		Voter	= reader.Read<EntityId>();
		Pro		= reader.ReadBoolean();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Dispute);
		writer.Write(Voter);
		writer.Write(Pro);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireDispute(round, Dispute, out var d))
			return;

		if(!RequireSite(round, d.Site, out var s))
			return;

		if(!d.Flags.HasFlag(DisputeFlags.Referendum))
		{
			if(d.Pros.Contains(Signer.Id) || d.Cons.Contains(Signer.Id))
			{
				Error = AlreadyExists;
				return;
			}

			if(!RequireSiteAccess(round, d.Site, out s))
				return;
		}
		else
		{
			if(d.Pros.Contains(Voter) || d.Cons.Contains(Voter))
			{
				Error = AlreadyExists;
				return;
			}

			if(!s.Authors.Contains(Voter))
			{
				Error = AlreadyExists;
				return;
			}

			if(!RequireAuthorAccess(round, Voter, out var a))
				return;
		}

		d = round.AffectDispute(Dispute);

		if(Pro)
			d.Pros = [..d.Pros, Voter];
		else
			d.Cons = [..d.Cons, Voter];

		var success = s.ModeratorElectionPolicy	switch
												{
													ElectionPolicy.AnyModerator				=> d.Pros.Length > 0,
													ElectionPolicy.ModeratorsMajority		=> d.Pros.Length > s.Moderators.Length/2,
													ElectionPolicy.ModeratorsUnanimously	=> d.Pros.Length == s.Moderators.Length,
													ElectionPolicy.AuthorsMajority			=> d.Pros.Length > s.Authors.Length/2,
													_ => throw new IntegrityException()
												};

		if(success)
		{
			d.Pros = [];
			d.Cons = [];
			d.Flags |= DisputeFlags.Resolved;

			d.Proposal.Execute(d.Site, round);
		}

		var fail = s.ModeratorElectionPolicy switch
											 {
												ElectionPolicy.AnyModerator				=> d.Cons.Length > 0,
												ElectionPolicy.ModeratorsMajority		=> d.Cons.Length > s.Moderators.Length/2,
												ElectionPolicy.ModeratorsUnanimously	=> d.Cons.Length == s.Moderators.Length,
												ElectionPolicy.AuthorsMajority			=> d.Cons.Length > s.Authors.Length/2,
												_ => throw new IntegrityException()
											 };

		if(d.Cons.Length > s.Authors.Length/2)
		{
			s = round.AffectSite(d.Site);
			s.Disputes = s.Disputes.Where(i => i != d.Id).ToArray();

			d.Deleted = true;
		}

		if(d.Expirtaion < round.ConsensusTime)
		{
			s = round.AffectSite(d.Site);
			s.Disputes = s.Disputes.Remove(d.Id);

			d.Deleted = true;
		}
	}
}