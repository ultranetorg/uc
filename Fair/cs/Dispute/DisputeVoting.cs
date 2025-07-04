namespace Uccs.Fair;

public enum DisputeVote : byte
{
	Abstained, Yes, No, 
}

public class DisputeVoting : FairOperation
{
	public AutoId				Dispute { get; set; }
	public AutoId				Voter { get; set; }
	public DisputeVote			Vote { get; set; }

	public override bool		IsValid(McvNet net) => Enum.IsDefined<DisputeVote>(Vote);
	public override string		Explanation => $"Dispute={Dispute}, Voter={Voter}, Vote={Vote}";

	public DisputeVoting()
	{
	}

	public DisputeVoting(AutoId dispute, AutoId voter, DisputeVote pro)
	{
		Dispute = dispute;
		Voter = voter;
		Vote = pro;
	}

	public override void Read(BinaryReader reader)
	{
		Dispute	= reader.Read<AutoId>();
		Voter	= reader.ReadNullable<AutoId>();
		Vote	= reader.Read<DisputeVote>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Dispute);
		writer.WriteNullable(Voter);
		writer.Write(Vote);
	}

	public override void Execute(FairExecution execution, bool _)
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

 		if(d.Flags.HasFlag(DisputeFlags.Succeeded))
 		{
 			Error = Ended;
 			return;
 		}
 
        var policy = s.ChangePolicies[Enum.Parse<FairOperationClass>(d.Proposal.GetType().Name)];

 		switch(policy)
 		{
 			case ChangePolicy.ElectedByModeratorsMajority :
 			case ChangePolicy.ElectedByModeratorsUnanimously :
 			{
 				if(!RequireModeratorAccess(execution, s.Id, out var _))
 					return;

				PayEnergyBySite(execution, s.Id);
 				break;
 			}
 			
 			case ChangePolicy.ElectedByAuthorsMajority :
 			{
 				if(!RequireAuthorMembership(execution, s.Id, Voter, out var _, out var a))
 					return;
 
				PayEnergyByAuthor(execution, a.Id);
 				break;
 			}
 		}
 
 		d = execution.Disputes.Affect(Dispute);
 
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
 
 		var	fail = policy switch
 						  {
 							 ChangePolicy.ElectedByModeratorsMajority		=> d.No.Length > s.Moderators.Length/2,
 							 ChangePolicy.ElectedByModeratorsUnanimously	=> d.No.Length == s.Moderators.Length,
 							 ChangePolicy.ElectedByAuthorsMajority			=> d.No.Length > s.Authors.Length/2,
 							 _ => throw new IntegrityException()
 						  };
 
 		if(success)
 		{
 			d.Yes = [];
 			d.No = [];
 			d.Abs = [];
 			d.Flags |= DisputeFlags.Succeeded;
 
			//d.Proposal.EnergyFeePayer	 = EnergyFeePayer;
			//d.Proposal.EnergySpenders	 = EnergySpenders;
			//d.Proposal.EnergyConsumed	 = execution.Round.ConsensusECEnergyCost;
			d.Proposal.SpacetimeSpenders = [];

 			s = execution.Sites.Affect(d.Site);

			//SpacetimeSpenders			= d.Proposal.SpacetimeSpenders;
			d.Proposal.EnergyConsumed	+= d.Proposal.EnergyConsumed;

 			d.Proposal.Execute(execution, true);
 		}
 
 		if(fail || d.Expirtaion < execution.Time)
 		{
 			s = execution.Sites.Affect(d.Site);
 			s.Disputes = s.Disputes.Remove(d.Id);
 
 			d.Deleted = true;
 		}
	}
}