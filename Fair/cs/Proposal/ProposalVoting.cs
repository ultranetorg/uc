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

	public ProposalVoting(AutoId proposal, AutoId voter, byte choice)
	{
		Proposal = proposal;
		Voter = voter;
		Choice = choice;
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
 
 		if(p.Options.Any(i => i.Yes.Contains(Voter)) || p.Neither.Contains(Voter) || p.Abstained.Contains(Voter) || p.Ban.Contains(Voter) || p.Banish.Contains(Voter))
 		{
 			Error = AlreadyExists;
 			return;
 		}
 
		var s = execution.Sites.Affect(p.Site);
        var c = p.OptionClass;

		if(s.IsReferendum(c))
 		{
			if(!IsPublisher(execution, s, Voter, out var x, out Error))
				return;

			var a = execution.Authors.Affect(Voter);
			execution.PayCycleEnergy(a);
 		}
		else if(s.IsDiscussion(c))
 		{
			if(!IsModerator(execution, s, Voter, out var _, out Error))
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
			var odd = p.Abstained.Length % 2 != 0;

    		return s.ApprovalPolicies[c] switch
 										 {
											ApprovalPolicy.AnyModerator			=> votes.Length + p.Abstained.Length == 1,
 											ApprovalPolicy.ModeratorsMajority	=> votes.Length + p.Abstained.Length >= s.Moderators.Length/2 + (odd ? 0 : 1),
 											ApprovalPolicy.AllModerators		=> votes.Length + p.Abstained.Length == s.Moderators.Length,
 											ApprovalPolicy.AuthorsMajority		=> votes.Length + p.Abstained.Length >= s.Publishers.Length/2 + (odd ? 0 : 1),
 											_ => throw new IntegrityException()
 										 };
		}

		byte result = (byte)SpecialChoice.None;

 		switch((SpecialChoice)Choice)
 		{
			case SpecialChoice.Abstained:
				p.Abstained = [..p.Abstained, Voter];

				foreach((var i, var j) in p.Options.Index())
					if(approved(j.Yes))
					{
						result = (byte)i;
						break;
					}

				if(result == (byte)SpecialChoice.None)
				{
					if(approved(p.Neither))		result = (byte)SpecialChoice.Neither; else
					//if(approved(p.Abstained))	result = (byte)SpecialChoice.Abstained; else
					if(approved(p.Ban))			result = (byte)SpecialChoice.Ban; else
					if(approved(p.Banish))		result = (byte)SpecialChoice.Banish;
				}

				break;
 				
 			case SpecialChoice.Neither:
				p.Neither = [..p.Neither, Voter];	

				if(approved(p.Neither))
				{
					result = (byte)SpecialChoice.Neither;
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

				p.Options = [..p.Options];
				p.Options[Choice] = new ProposalOption {Title = o.Title, Operation = o.Operation, Yes = [..o.Yes, Voter]};

 				if(approved(p.Options[Choice].Yes))
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
					if(p.As == Role.Publisher)
 					{
						var i = Array.FindIndex(s.Publishers, i => i.Author != p.By);
						s.Publishers = [..s.Publishers];
						s.Publishers[i] = new Publisher {Author = p.By, BannedTill = execution.Time + Time.FromDays(30)};
 					}
					else if(p.As == Role.Moderator)
 					{
						var i = Array.FindIndex(s.Moderators, i => i.Account != p.By);
						s.Moderators = [..s.Moderators];
						s.Moderators[i] = new Moderator {Account = p.By, BannedTill = execution.Time + Time.FromDays(30)};
 					}
					break;
 				
				case (byte)SpecialChoice.Banish:
					/// TODO
					break;


				case (byte)SpecialChoice.Neither:
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