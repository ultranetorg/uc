namespace Uccs.Fair;

public class ProposalVoting : FairOperation
{
	public AutoId				Proposal { get; set; }
	public AutoId				Voter { get; set; }
	public sbyte				Choice { get; set; }

	public override bool		IsValid(McvNet net) => Choice >= (sbyte)SpecialChoice._First;
	public override string		Explanation => $"Proposal={Proposal}, Voter={Voter}, Choice={Choice}";

	public ProposalVoting()
	{
	}

	public ProposalVoting(AutoId proposal, AutoId voter, sbyte choice)
	{
		Proposal = proposal;
		Voter = voter;
		Choice = choice;
	}

	public override void Read(BinaryReader reader)
	{
		Proposal	= reader.Read<AutoId>();
		Voter		= reader.ReadNullable<AutoId>();
		Choice		= reader.ReadSByte();
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

		if(Choice >= p.Options.Length && Choice != (sbyte)SpecialChoice.Neither && Choice != (sbyte)SpecialChoice.Any && Choice != (sbyte)SpecialChoice.Ban && Choice != (sbyte)SpecialChoice.Banish)
		{
 			Error = OutOfBounds;
 			return;
		}
 
 		if(p.Options.Any(i => i.Yes.Contains(Voter)) || p.Neither.Contains(Voter) || p.Any.Contains(Voter) || p.Ban.Contains(Voter) || p.Banish.Contains(Voter))
 		{
 			Error = AlreadyExists;
 			return;
 		}
 
		var s = execution.Sites.Affect(p.Site);
        var c = p.OptionClass;

		if(s.IsReferendum(c) && c != FairOperationClass.SiteApprovalPolicyChange)
 		{
			if(!IsPublisher(execution, s, Voter, out var x, out Error))
				return;

			var a = execution.Authors.Affect(Voter);
			execution.PayOperationEnergy(a);
 		}
		else if(s.IsDiscussion(c))
 		{
			if(!IsModerator(execution, s, Voter, out var _, out Error))
				return;

			execution.PayOperationEnergy(s);
 		}
		else
		{
			Error = Denied;
			return;
		}
		 
 		p = execution.Proposals.Affect(Proposal);

		var policy = s.Policies.FirstOrDefault(i => i.OperationClass == c);

		bool won(AutoId[] votes)
		{
    		return policy.Approval	switch
 									{
										ApprovalRequirement.AnyModerator		=> votes.Length + p.Any.Length == 1,
 										ApprovalRequirement.ModeratorsMajority	=> votes.Length + p.Any.Length >= s.Moderators.Length/2 + (s.Moderators.Length & 1),
 										ApprovalRequirement.AllModerators		=> votes.Length + p.Any.Length == s.Moderators.Length,
 										ApprovalRequirement.PublishersMajority	=> votes.Length + p.Any.Length >= s.Publishers.Length/2 + (s.Publishers.Length & 1),
 										_ => throw new IntegrityException()
 									};
		}

		var result = (sbyte)SpecialChoice.None;

 		switch((SpecialChoice)Choice)
 		{
			case SpecialChoice.Any:
				p.Any = [..p.Any, Voter];

				foreach((var i, var j) in p.Options.Index())
					if(won(j.Yes))
					{
						result = (sbyte)i;
						break;
					}

				//if(result == (sbyte)SpecialChoice.None)
				//{
				//	if(won(p.Neither))		result = (sbyte)SpecialChoice.Neither; else
				//	//if(approved(p.Abstained))	result = (byte)SpecialChoice.Abstained; else
				//	if(won(p.Ban))			result = (sbyte)SpecialChoice.Ban; else
				//	if(won(p.Banish))		result = (sbyte)SpecialChoice.Banish;
				//}

				break;
 				
 			case SpecialChoice.Neither:
				p.Neither = [..p.Neither, Voter];	

				if(won(p.Neither))
				{
					result = (sbyte)SpecialChoice.Neither;
				}
				else if(policy.Approval == ApprovalRequirement.AllModerators)
				{
					result = (sbyte)SpecialChoice.Neither;
				}
				break;
 				
			case SpecialChoice.Ban:
				p.Ban = [..p.Ban, Voter];

				if(won(p.Ban))
				{
					result = (sbyte)SpecialChoice.Ban;
				}
				break;
 				
			case SpecialChoice.Banish:
				p.Banish = [..p.Banish, Voter];	

				if(won(p.Banish))
				{
					result = (sbyte)SpecialChoice.Banish;
				}
				break;
				
			default:
			{
				var o = p.Options[Choice];

				p.Options = [..p.Options];
				p.Options[Choice] = new ProposalOption {Title = o.Title, Operation = o.Operation, Yes = [..o.Yes, Voter]};

 				if(won(p.Options[Choice].Yes))
 				{
					result = Choice;
				}

				break;
			}
 		}
 
		if(result != (sbyte)SpecialChoice.None)
		{
			execution.Proposals.Delete(s, p);

 			switch(result)
 			{
				case (sbyte)SpecialChoice.Ban:
					if(p.As == Role.Publisher)
 					{
						var i = Array.FindIndex(s.Publishers, i => i.Author == p.By);
						s.Publishers = [..s.Publishers];
						s.Publishers[i] = new Publisher {Author = p.By, BannedTill = execution.Time + Time.FromDays(30)};
 					}
					else if(p.As == Role.Moderator)
 					{
						var i = Array.FindIndex(s.Moderators, i => i.User == p.By);
						s.Moderators = [..s.Moderators];
						s.Moderators[i] = new Moderator {User = p.By, BannedTill = execution.Time + Time.FromDays(30)};
 					}
					break;
 				
				case (sbyte)SpecialChoice.Banish:
					/// TODO
					break;


				case (sbyte)SpecialChoice.Neither:
					break;

				default:
				{
					var o = p.Options[result];

					o.Operation.Site	= s;
					o.Operation.As		= p.As;
					o.Operation.By		= p.By;
					o.Operation.User	= p.As == Role.User ? execution.AffectUser(p.By) : null;

					var e = execution.CreateChild();
	 		
					e.SkipPowCheck = true;

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