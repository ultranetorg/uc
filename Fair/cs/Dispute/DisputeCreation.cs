namespace Uccs.Fair;

public class DisputeCreation : FairOperation
{
	public EntityId				    Site { get; set; }
	public EntityId				    Creator { get; set; }
	public string				    Text { get; set; }
	public VotableOperation	Proposal { get; set; }
	
	public override bool		    IsValid(Mcv mcv) => true;
	public override string		    Description => $"{Id}";

	public DisputeCreation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Site		= reader.Read<EntityId>();
		Creator		= reader.Read<EntityId>();
 		Text		= reader.ReadUtf8();

 		Proposal = GetType().Assembly.GetType(GetType().Namespace + "." + reader.Read<FairOperationClass>()).GetConstructor([]).Invoke(null) as VotableOperation;
 		Proposal.Read(reader); 
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(Creator);
 		writer.WriteUtf8(Text);

		writer.Write(Enum.Parse<FairOperationClass>(Proposal.GetType().Name));
		Proposal.Write(writer);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
 		if(!RequireSite(round, Site, out var s))
 			return;
 
        var t = Enum.Parse<FairOperationClass>(Proposal.GetType().Name);

 		if(!s.ChangePolicies.TryGetValue(t, out var p) || p == ChangePolicy.AnyModerator)
 		{
 			Error = InvalidProposal;
 			return;
 		}
 
 		if(!Proposal.ValidProposal(mcv, round, s))
 		{
 			Error = InvalidProposal;
 			return;
 		}
 
 		if(s.Disputes.Any(i =>  {
                                    var p = round.FindDispute(i).Proposal;

                                    if(p.GetType() != Proposal.GetType())
                                        return false;
                                    
                                    return p.Overlaps(Proposal);
                                }))
 		{
 			Error = AlreadyExists;
 			return;
 		}
 
 		switch(s.ChangePolicies[t])
 		{
 			case ChangePolicy.ElectedByModeratorsMajority :
 			case ChangePolicy.ElectedByModeratorsUnanimously :
 			{
 				if(!s.Moderators.Contains(Creator))
 				{
 					Error = Denied;
 					return;
 				}
 			
 				if(!RequireAccountAccess(round, Creator, out var _))
 					return;
 
 				break;
 			}
 			
 			case ChangePolicy.ElectedByAuthorsMajority :
 			{
 				if(!s.Authors.Contains(Creator))
 				{
 					Error = Denied;
 					return;
 				}
 
 				if(!RequireAuthorAccess(round, Creator, out var _))
 					return;
 
 				break;
 			}
 
 			default:
 			{
 				Error = Denied;
 				return;
 			}
 		}
 
 		var d = round.CreateDispute(s);
 
 		d.Site       = Site;
        d.Text       = Text;
 		d.Proposal   = Proposal;
 		d.Expirtaion = round.ConsensusTime + Time.FromDays(30);
 
 		AllocateEntity(Signer);
 
 		s = round.AffectSite(s.Id);
 		s.Disputes = [..s.Disputes, d.Id];
	}
}