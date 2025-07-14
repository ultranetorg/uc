namespace Uccs.Fair;

public enum ProposalVote : byte
{
	Abstained, Yes, No, Punish
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
 		if(!ProposalExists(execution, Proposal, out var d, out Error))
 			return;
 
 		if(d.Yes.Contains(Voter) || d.No.Contains(Voter) || d.Abs.Contains(Voter))
 		{
 			Error = AlreadyExists;
 			return;
 		}

// 		if(d.Flags.HasFlag(ProposalFlags.Succeeded))
// 		{
// 			Error = Ended;
// 			return;
// 		}

 
		var s = execution.Sites.Find(d.Site);
        var policy = s.ChangePolicies[Enum.Parse<FairOperationClass>(d.Operation.GetType().Name)];

 		switch(policy)
 		{
 			case ChangePolicy.AnyModerator :
 			case ChangePolicy.ElectedByModeratorsMajority :
 			case ChangePolicy.ElectedByModeratorsUnanimously :
 			{
 				if(!IsModerator(execution, s.Id, out var _, out Error))
 					return;
 				break;
 			}
 			
 			case ChangePolicy.ElectedByAuthorsMajority :
 			{
 				if(!IsMember(execution, s.Id, Voter, out var _, out var a, out var Error))
 					return;
 				break;
 			}
 		}
 
 		d = execution.Proposals.Affect(Proposal);
 
 		switch(Vote)
 		{
 			case ProposalVote.Yes:			d.Yes	= [..d.Yes, Voter];	break;
 			case ProposalVote.No:			d.No	= [..d.No,  Voter];	break;
 			case ProposalVote.Abstained:	d.Abs	= [..d.Abs, Voter];	break;
 		}
 
 		var success = policy switch
 							 {
 								ChangePolicy.AnyModerator					=> d.Yes.Length == 1,
 								ChangePolicy.ElectedByModeratorsMajority	=> d.Yes.Length > s.Moderators.Length/2,
 								ChangePolicy.ElectedByModeratorsUnanimously	=> d.Yes.Length == s.Moderators.Length,
 								ChangePolicy.ElectedByAuthorsMajority		=> d.Yes.Length > s.Authors.Length/2,
 								_ => throw new IntegrityException()
 							 };
 
 		var	fail = policy switch
 						  {
							 ChangePolicy.AnyModerator						=> d.No.Length == 1,
 							 ChangePolicy.ElectedByModeratorsMajority		=> d.No.Length > s.Moderators.Length/2,
 							 ChangePolicy.ElectedByModeratorsUnanimously	=> d.No.Length == s.Moderators.Length,
 							 ChangePolicy.ElectedByAuthorsMajority			=> d.No.Length > s.Authors.Length/2,
 							 _ => throw new IntegrityException()
 						  };
 
 		if(success)
 		{
			d.Operation.Site = s;

			var e = execution.CreateChild();
	 			
			if(d.Operation.ValidateProposal(e, out _))
			{
				d.Operation.Execute(e);
	
				if(d.Operation.Error == null)
				{
					execution.Absorb(e);
				}
			}

			d.Deleted = true;

			foreach(var i in d.Comments)
				execution.ProposalComments.Affect(i).Deleted = true;

 			s = execution.Sites.Affect(d.Site);
 			s.Proposals = s.Proposals.Remove(d.Id);
		}
 
 		if(fail || d.Expiration < execution.Time)
 		{
 			d.Deleted = true;

 			s = execution.Sites.Affect(d.Site);
 			s.Proposals = s.Proposals.Remove(d.Id);
 		}

 		switch(policy)
 		{
 			case ChangePolicy.ElectedByModeratorsMajority :
 			case ChangePolicy.ElectedByModeratorsUnanimously :
 			{
 				if(!IsModerator(execution, s.Id, out var _, out Error))
 					return;

				execution.PayCycleEnergy(execution.Sites.Affect(s.Id));
 				break;
 			}
 			
 			case ChangePolicy.ElectedByAuthorsMajority :
 			{
 				if(!IsMember(execution, s.Id, Voter, out var _, out var a, out var _))
 					return;
 
				execution.PayCycleEnergy(execution.Authors.Affect(a.Id));
 				break;
 			}
 		}
	}
}