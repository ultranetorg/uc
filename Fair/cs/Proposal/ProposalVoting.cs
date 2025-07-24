namespace Uccs.Fair;

public enum SpecialVote : byte
{
	Neither = 100, Abstained = 101, NoAndBan = 102, NoAndBanish = 103
}

public class ProposalVoting : FairOperation
{
	public AutoId				Proposal { get; set; }
	public AutoId				Voter { get; set; }
	public byte					Choice { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Proposal={Proposal}, Voter={Voter}, Choice={Choice}";

	public ProposalVoting()
	{
	}

	public ProposalVoting(AutoId proposal, AutoId voter, byte options)
	{
		Proposal = proposal;
		Voter = voter;
		Choice = options;
	}

	public override void Read(BinaryReader reader)
	{
		Proposal	= reader.Read<AutoId>();
		Voter		= reader.ReadNullable<AutoId>();
		Choice		= reader.ReadByte();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Proposal);
		writer.WriteNullable(Voter);
		writer.Write(Choice);
	}

	public override void Execute(FairExecution execution)
	{
 		if(!ProposalExists(execution, Proposal, out var z, out Error))
 			return;

		if(Choice >= z.Options.Length)
		{
 			Error = OutOfBounds;
 			return;
		}
 
 		if(z.Options.Any(i => i.Yes.Contains(Voter)) || z.Abs.Contains(Voter) || z.Ban.Contains(Voter) || z.Banish.Contains(Voter))
 		{
 			Error = AlreadyExists;
 			return;
 		}
 
		var s = execution.Sites.Affect(z.Site);
        var c = z.OptionClass;

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

		bool approved(AutoId[] voters)
		{
    		return s.ApprovalPolicies[c] switch
 										 {
											ApprovalPolicy.AnyModerator						=> voters.Length + z.Abs.Length == 1,
 											ApprovalPolicy.ElectedByModeratorsMajority		=> voters.Length + z.Abs.Length > s.Moderators.Length/2,
 											ApprovalPolicy.ElectedByModeratorsUnanimously	=> voters.Length + z.Abs.Length == s.Moderators.Length,
 											ApprovalPolicy.ElectedByAuthorsMajority			=> voters.Length + z.Abs.Length > s.Authors.Length/2,
 											_ => throw new IntegrityException()
 										 };
		}

 		if(Choice >= (byte)SpecialVote.Neither)
 		{
 			switch((SpecialVote)Choice)
 			{
 				case SpecialVote.Neither:
					z.Neither = [..z.Neither, Voter];	

					if(approved(z.Neither))
					{
						execution.Proposals.Delete(s, z);
					}
					break;
 				
				case SpecialVote.Abstained:
					z.Abs = [..z.Abs, Voter];

					//if(approved(z.Abs))
					//{
					//	execution.Proposals.Delete(s, z);
					//}
					break;
 				
				case SpecialVote.NoAndBan:
					z.Ban = [..z.Ban, Voter];

					if(approved(z.Ban))
					{
						execution.Proposals.Delete(s, z);

						/// TODO
					}
					break;
 				
				case SpecialVote.NoAndBanish:
					z.Banish = [..z.Banish, Voter];	

					if(approved(z.Banish))
					{
						execution.Proposals.Delete(s, z);

						/// TODO
					}
					break;
 			}
 
 			//if(fail || z.Expiration < execution.Time)
 			//{
 			//	z.Deleted = true;
 			//	s.Proposals = s.Proposals.Remove(z.Id);
 			//}
 		}
		else
		{
			var o = z.Options[Choice];

			z.Options = z.Options.Remove(o);
			o = new ProposalOption {Text = o.Text, Operation = o.Operation, Yes = [..o.Yes, Voter]};
			z.Options = [..z.Options, o];

 			if(approved(o.Yes))
 			{
				o.Operation.Site	= s;
				o.Operation.As		= z.As;
				o.Operation.By		= z.By;
				o.Operation.Signer	= z.As == Role.User ? execution.AffectAccount(z.By) : null;

				var e = execution.CreateChild();
	 		
				e.LongYesVoted = true;

				if(o.Operation.ValidateProposal(e, out _))
				{
					o.Operation.Execute(e);
	
					if(o.Operation.Error == null)
					{
						execution.Absorb(e);
					}
				}

				execution.Proposals.Delete(s, z);
			}
		}
	}
}