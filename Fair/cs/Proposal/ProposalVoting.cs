namespace Uccs.Fair;

public enum SpecialChoice : byte
{
	Neither = 100, 
	Abstained = 101, 
	Ban = 102, 
	Banish = 103, 
	None = 255
}

public class ProposalVoting : FairOperation
{
	public AutoId				Proposal { get; set; }
	public AutoId				Voter { get; set; }
	public byte					Choice { get; set; }

	public override bool		IsValid(McvNet net) => Choice < (byte)SpecialChoice.None;
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

		if(Choice >= z.Options.Length && Choice != (byte)SpecialChoice.Neither && Choice != (byte)SpecialChoice.Abstained && Choice != (byte)SpecialChoice.Ban && Choice != (byte)SpecialChoice.Banish)
		{
 			Error = OutOfBounds;
 			return;
		}
 
 		if(z.Options.Any(i => i.Yes.Contains(Voter)) || z.Neither.Contains(Voter) || z.Abs.Contains(Voter) || z.Ban.Contains(Voter) || z.Banish.Contains(Voter))
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

		bool approved(AutoId[] votes)
		{
    		return s.ApprovalPolicies[c] switch
 										 {
											ApprovalPolicy.AnyModerator						=> votes.Length + z.Abs.Length == 1,
 											ApprovalPolicy.ElectedByModeratorsMajority		=> votes.Length + z.Abs.Length > s.Moderators.Length/2,
 											ApprovalPolicy.ElectedByModeratorsUnanimously	=> votes.Length + z.Abs.Length == s.Moderators.Length,
 											ApprovalPolicy.ElectedByAuthorsMajority			=> votes.Length + z.Abs.Length > s.Authors.Length/2,
 											_ => throw new IntegrityException()
 										 };
		}

		byte result = (byte)SpecialChoice.None;

 			switch((SpecialChoice)Choice)
 			{
 				case SpecialChoice.Neither:
					z.Neither = [..z.Neither, Voter];	

					if(approved(z.Neither))
					{
						result = (byte)SpecialChoice.Neither;
					}
					break;
 				
				case SpecialChoice.Abstained:
					z.Abs = [..z.Abs, Voter];

					foreach((var i, var j) in z.Options.Index())
						if(approved(j.Yes))
						{
							result = (byte)i;
						}

					if(result != (byte)SpecialChoice.None)
					{
						if(approved(z.Neither))	result = (byte)SpecialChoice.Neither; else
						if(approved(z.Abs))		result = (byte)SpecialChoice.Abstained; else
						if(approved(z.Ban))		result = (byte)SpecialChoice.Ban; else
						if(approved(z.Banish))	result = (byte)SpecialChoice.Banish;
					}

					break;
 				
				case SpecialChoice.Ban:
					z.Ban = [..z.Ban, Voter];

					if(approved(z.Ban))
					{
						result = (byte)SpecialChoice.Ban;
					}
					break;
 				
				case SpecialChoice.Banish:
					z.Banish = [..z.Banish, Voter];	

					if(approved(z.Banish))
					{
						result = (byte)SpecialChoice.Banish;
					}
					break;
				
				default:
				{
					var o = z.Options[Choice];

					z.Options = z.Options.Remove(o);
					o = new ProposalOption {Title = o.Title, Operation = o.Operation, Yes = [..o.Yes, Voter]};
					z.Options = [..z.Options, o];

 					if(approved(o.Yes))
 					{
						result = Choice;
					}

					break;
				}
 			}
 

		if(result != (byte)SpecialChoice.None)
		{
			execution.Proposals.Delete(s, z);

 			switch(result)
 			{
				case (byte)SpecialChoice.Ban:
					if(s.IsReferendum(c))
 					{
						var a = execution.Authors.Affect(Voter);
						/// TODO a.BannedTill 
 					}
					else if(s.IsDiscussion(c))
 					{
						/// TODO s.Moderators
 					}
					break;
 				
				case (byte)SpecialChoice.Banish:
					/// TODO
					break;

				default:
				{
					var o = z.Options[result];

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

					break;
				}
			}
		}
	}
}