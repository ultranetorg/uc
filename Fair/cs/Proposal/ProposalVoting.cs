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
 		if(!ProposalExists(execution, Proposal, out var p, out Error))
 			return;

		if(Choice >= p.Options.Length && Choice != (byte)SpecialChoice.Neither && Choice != (byte)SpecialChoice.Abstained && Choice != (byte)SpecialChoice.Ban && Choice != (byte)SpecialChoice.Banish)
		{
 			Error = OutOfBounds;
 			return;
		}
 
 		if(p.Options.Any(i => i.Yes.Contains(Voter)) || p.Neither.Contains(Voter) || p.Abs.Contains(Voter) || p.Ban.Contains(Voter) || p.Banish.Contains(Voter))
 		{
 			Error = AlreadyExists;
 			return;
 		}
 
		var s = execution.Sites.Affect(p.Site);
        var c = p.OptionClass;

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
		 
 		p = execution.Proposals.Affect(Proposal);

		bool approved(AutoId[] votes)
		{
    		return s.ApprovalPolicies[c] switch
 										 {
											ApprovalPolicy.AnyModerator						=> votes.Length + p.Abs.Length == 1,
 											ApprovalPolicy.ElectedByModeratorsMajority		=> votes.Length + p.Abs.Length > s.Moderators.Length/2,
 											ApprovalPolicy.ElectedByModeratorsUnanimously	=> votes.Length + p.Abs.Length == s.Moderators.Length,
 											ApprovalPolicy.ElectedByAuthorsMajority			=> votes.Length + p.Abs.Length > s.Authors.Length/2,
 											_ => throw new IntegrityException()
 										 };
		}

		byte result = (byte)SpecialChoice.None;

 		switch((SpecialChoice)Choice)
 		{
 			case SpecialChoice.Neither:
				p.Neither = [..p.Neither, Voter];	

				if(approved(p.Neither))
				{
					result = (byte)SpecialChoice.Neither;
				}
				break;
 				
			case SpecialChoice.Abstained:
				p.Abs = [..p.Abs, Voter];

				foreach((var i, var j) in p.Options.Index())
					if(approved(j.Yes))
					{
						result = (byte)i;
					}

				if(result != (byte)SpecialChoice.None)
				{
					if(approved(p.Neither))	result = (byte)SpecialChoice.Neither; else
					if(approved(p.Abs))		result = (byte)SpecialChoice.Abstained; else
					if(approved(p.Ban))		result = (byte)SpecialChoice.Ban; else
					if(approved(p.Banish))	result = (byte)SpecialChoice.Banish;
				}

				break;
 				
			case SpecialChoice.Ban:
				p.Ban = [..p.Ban, Voter];

				if(approved(p.Ban))
				{
					result = (byte)SpecialChoice.Ban;
				}
				break;
 				
			case SpecialChoice.Banish:
				p.Banish = [..p.Banish, Voter];	

				if(approved(p.Banish))
				{
					result = (byte)SpecialChoice.Banish;
				}
				break;
				
			default:
			{
				var o = p.Options[Choice];

				p.Options = p.Options.Remove(o);
				o = new ProposalOption {Title = o.Title, Operation = o.Operation, Yes = [..o.Yes, Voter]};
				p.Options = [..p.Options, o];

 				if(approved(o.Yes))
 				{
					result = Choice;
				}

				break;
			}
 		}
 
		if(result != (byte)SpecialChoice.None)
		{
			execution.Proposals.Delete(s, p);

 			switch(result)
 			{
				case (byte)SpecialChoice.Ban:
					if(s.IsReferendum(c))
 					{
						var a = execution.Authors.Affect(Voter);
						s.Authors = [..s.Authors.Where(i => i.Author != p.By), new Citizen {Author = p.By, BannedTill = execution.Time + Time.FromDays(30)}];
 					}
					else if(s.IsDiscussion(c))
 					{
						s.Moderators = [..s.Moderators.Where(i => i.Account != p.By), new Moderator {Account = p.By, BannedTill = execution.Time + Time.FromDays(30)}];
 					}
					break;
 				
				case (byte)SpecialChoice.Banish:
					/// TODO
					break;

				default:
				{
					var o = p.Options[result];

					o.Operation.Site	= s;
					o.Operation.As		= p.As;
					o.Operation.By		= p.By;
					o.Operation.Signer	= p.As == Role.User ? execution.AffectAccount(p.By) : null;

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