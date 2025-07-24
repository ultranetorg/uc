namespace Uccs.Fair;

public enum ProposalVote : byte
{
	Abstained, Yes, No, NoAndBan, NoAndBanish
}

public class ProposalVoting : FairOperation
{
	public AutoId				Proposal { get; set; }
	public AutoId				Voter { get; set; }
	public ProposalVote			Vote { get; set; }

	public override bool		IsValid(McvNet net) => Enum.IsDefined<ProposalVote>(Vote);
	public override string		Explanation => $"Proposal={Proposal}, Voter={Voter}, Vote={Vote}";

	public ProposalVoting()
	{
	}

	public ProposalVoting(AutoId proposal, AutoId voter, ProposalVote pro)
	{
		Proposal = proposal;
		Voter = voter;
		Vote = pro;
	}

	public override void Read(BinaryReader reader)
	{
		Proposal	= reader.Read<AutoId>();
		Voter		= reader.ReadNullable<AutoId>();
		Vote		= reader.Read<ProposalVote>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Proposal);
		writer.WriteNullable(Voter);
		writer.Write(Vote);
	}

	public override void Execute(FairExecution execution)
	{
 		if(!ProposalExists(execution, Proposal, out var z, out Error))
 			return;
 
 		if(z.Yes.Contains(Voter) || z.No.Contains(Voter) || z.NoAndBan.Contains(Voter) || z.NoAndBanish.Contains(Voter) || z.Abs.Contains(Voter))
 		{
 			Error = AlreadyExists;
 			return;
 		}
 
		var s = execution.Sites.Affect(z.Site);
        var c = Enum.Parse<FairOperationClass>(z.Option.GetType().Name);

		if(s.IsReferendum(c))
 		{
			if(!IsPublisher(execution, s.Id, Voter, out var _, out var _, out Error))
				return;

			var a = execution.Authors.Affect(Voter);
			execution.PayCycleEnergy(a);
 		}
		else if(s.IsDiscussion(c))
 		{
			if(!IsModerator(execution, s.Id, Voter, out var _, out Error))
				return;

			execution.PayCycleEnergy(s);
 		}
		else
		{
			Error = Denied;
			return;
		}

 
 		z = execution.Proposals.Affect(Proposal);
 
 		switch(Vote)
 		{
 			case ProposalVote.Yes:			z.Yes	= [..z.Yes, Voter];	break;
 			case ProposalVote.No:			z.No	= [..z.No,  Voter];	break;
 			case ProposalVote.Abstained:	z.Abs	= [..z.Abs, Voter];	break;
 		}
 
 		var success = s.ApprovalPolicies[c]	switch
 											{
 												ApprovalPolicy.AnyModerator						=> z.Yes.Length == 1,
 												ApprovalPolicy.ElectedByModeratorsMajority		=> z.Yes.Length > s.Moderators.Length/2,
 												ApprovalPolicy.ElectedByModeratorsUnanimously	=> z.Yes.Length == s.Moderators.Length,
 												ApprovalPolicy.ElectedByAuthorsMajority			=> z.Yes.Length > s.Authors.Length/2,
 												_ => throw new IntegrityException()
 											};
 
 		var	fail = s.ApprovalPolicies[c] switch
 										 {
											 ApprovalPolicy.AnyModerator					=> z.No.Length == 1,
 											 ApprovalPolicy.ElectedByModeratorsMajority		=> z.No.Length > s.Moderators.Length/2,
 											 ApprovalPolicy.ElectedByModeratorsUnanimously	=> z.No.Length == s.Moderators.Length,
 											 ApprovalPolicy.ElectedByAuthorsMajority		=> z.No.Length > s.Authors.Length/2,
 											 _ => throw new IntegrityException()
 										 };
 
 		if(success)
 		{
			z.Option.Site	= s;
			z.Option.As		= z.As;
			z.Option.By		= z.By;
			z.Option.Signer	= z.As == Role.User ? execution.AffectAccount(z.By) : null;

			var e = execution.CreateChild();
	 		
			e.LongYesVoted = true;

			if(z.Option.ValidateProposal(e, out _))
			{
				z.Option.Execute(e);
	
				if(z.Option.Error == null)
				{
					execution.Absorb(e);
				}
			}

			foreach(var i in z.Comments)
				execution.ProposalComments.Affect(i).Deleted = true;

			z.Deleted = true;
 			s.Proposals = s.Proposals.Remove(z.Id);
		}
 
 		if(fail || z.Expiration < execution.Time)
 		{
 			z.Deleted = true;
 			s.Proposals = s.Proposals.Remove(z.Id);
 		}
	}
}