namespace Uccs.Fair;

public enum DisputeVote : byte
{
	Abstained, Yes, No, 
}

public class DisputeVoting : FairOperation
{
	public EntityId				Dispute { get; set; }
	public EntityId				Voter { get; set; }
	public DisputeVote			Vote { get; set; }

	public override bool		IsValid(Mcv mcv) => Enum.IsDefined<DisputeVote>(Vote);
	public override string		Description => $"{Id}, {Voter}, {Vote}";

	public DisputeVoting()
	{
	}

	public DisputeVoting(EntityId dispute, EntityId voter, DisputeVote pro)
	{
		Dispute = dispute;
		Voter = voter;
		Vote = pro;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Dispute	= reader.Read<EntityId>();
		Voter	= reader.Read<EntityId>();
		Vote	= reader.Read<DisputeVote>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Dispute);
		writer.Write(Voter);
		writer.Write(Vote);
	}

	public override void Execute(FairExecution execution)
	{
 		if(!RequireDispute(execution, Dispute, out var d))
 			return;
 
 		if(!RequireSite(execution, d.Site, out var s))
 			return;
 
 		if(d.Yes.Contains(Voter) || d.No.Contains(Voter) || d.Abs.Contains(Voter))
 		{
 			Error = AlreadyExists;
 			return;
 		}
 
        var policy = s.ChangePolicies[Enum.Parse<FairOperationClass>(d.Proposal.GetType().Name)];

 		switch(policy)
 		{
 			case ChangePolicy.ElectedByModeratorsMajority :
 			case ChangePolicy.ElectedByModeratorsUnanimously :
 			{
 				if(!s.Moderators.Contains(Voter)) /// Voter is Account
 				{
 					Error = Denied;
 					return;
 				}
 
 				if(!RequireAccountAccess(execution, Voter, out var _))
 					return;
 
 				break;
 			}
 			
 			case ChangePolicy.ElectedByAuthorsMajority :
 			{
 				if(!s.Authors.Contains(Voter)) /// Voter is Author
 				{
 					Error = Denied;
 					return;
 				}
 
 				if(!RequireAuthorAccess(execution, Voter, out var _))
 					return;
 
 				break;
 			}
 		}
 
 		d = execution.AffectDispute(Dispute);
 
 		switch(Vote)
 		{
 			case DisputeVote.Yes:		d.Yes	= [..d.Yes, Voter];	break;
 			case DisputeVote.No:		d.No	= [..d.No, Voter];	break;
 			case DisputeVote.Abstained:	d.Abs	= [..d.Abs, Voter];	break;
 		}
 
 		var success = policy switch
 							 {
 								ChangePolicy.ElectedByModeratorsMajority	=> d.Yes.Length > s.Moderators.Length/2,
 								ChangePolicy.ElectedByModeratorsUnanimously	=> d.Yes.Length == s.Moderators.Length,
 								ChangePolicy.ElectedByAuthorsMajority		=> d.Yes.Length > s.Authors.Length/2,
 								_ => throw new IntegrityException()
 							 };
 
 		var	fail = policy	switch
 						    {
 							    ChangePolicy.ElectedByModeratorsMajority	=> d.No.Length > s.Moderators.Length/2,
 							    ChangePolicy.ElectedByModeratorsUnanimously	=> d.No.Length == s.Moderators.Length,
 							    ChangePolicy.ElectedByAuthorsMajority		=> d.No.Length > s.Authors.Length/2,
 							    _ => throw new IntegrityException()
 						    };
 
 		if(success)
 		{
 			d.Yes = [];
 			d.No = [];
 			d.Abs = [];
 			d.Flags |= DisputeFlags.Resolved;
 
 			d.Proposal.Execute(execution, s);
 		}
 
 		if(fail || d.Expirtaion < execution.Time)
 		{
 			s = execution.AffectSite(d.Site);
 			s.Disputes = s.Disputes.Remove(d.Id);
 
 			d.Deleted = true;
 		}
	}
}