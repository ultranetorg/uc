namespace Uccs.Fair;

public class DisputeCreation : FairOperation
{
	public AutoId				    Site { get; set; }
	public AutoId				    Creator { get; set; }
	public string				    Text { get; set; }
	public VotableOperation	        Proposal { get; set; }
	
	public override bool		    IsValid(McvNet net) => Proposal.IsValid(net) && Text.Length < 8192;
	public override string		    Explanation => $"{Id}";

	public DisputeCreation()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Site		= reader.Read<AutoId>();
		Creator		= reader.Read<AutoId>();
 		Text		= reader.ReadUtf8();

 		Proposal = GetType().Assembly.GetType(GetType().Namespace + "." + reader.Read<FairOperationClass>()).GetConstructor([]).Invoke(null) as VotableOperation;
 		Proposal.Read(reader); 
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(Creator);
 		writer.WriteUtf8(Text);

		writer.Write(Enum.Parse<FairOperationClass>(Proposal.GetType().Name));
		Proposal.Write(writer);
	}

	public override void Execute(FairExecution execution, bool _)
	{
 		if(!Proposal.ValidProposal(execution))
 		{
 			Error = InvalidProposal;
 			return;
 		}

        if(!RequireSite(execution, Site, out var s))
            return;
 
        var t = Enum.Parse<FairOperationClass>(Proposal.GetType().Name);

 		if(!s.ChangePolicies.TryGetValue(t, out var p) || p == ChangePolicy.AnyModerator)
 		{
 			Error = InvalidProposal;
 			return;
 		}
 
 		if(s.Disputes.Any(i =>  {
                                    var p = execution.FindDispute(i).Proposal;

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
 			
 				if(!RequireAccountAccess(execution, Creator, out var _))
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
 
 				if(!RequireAuthorAccess(execution, Creator, out var _))
 					return;
 
 				break;
 			}
 
 			default:
 			{
 				Error = Denied;
 				return;
 			}
 		}
 
 		var d = execution.CreateDispute(s);
 
 		d.Site       = Site;
        d.Text       = Text;
 		d.Proposal   = Proposal;
 		d.Expirtaion = execution.Time + Time.FromDays(30);
 
 		AllocateEntity(Signer);
 
 		s = execution.AffectSite(s.Id);
 		s.Disputes = [..s.Disputes, d.Id];
	}
}