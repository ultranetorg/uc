namespace Uccs.Fair;

public class DisputeCreation : FairOperation
{
	public AutoId				Site { get; set; }
	public AutoId				Creator { get; set; } /// Account Id for Moderators, Author Id for Author
	public string				Text { get; set; }
	public VotableOperation	    Proposal { get; set; }
	
	public override bool		IsValid(McvNet net) => Proposal.IsValid(net) && Text.Length < 8192;
	public override string		Explanation => $"Site={Site}, Creator={Creator}, Text={Text}, Proposal={{{Proposal}}}";

	public DisputeCreation()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Site		= reader.Read<AutoId>();
		Creator		= reader.ReadNullable<AutoId>();
 		Text		= reader.ReadUtf8();

 		Proposal = GetType().Assembly.GetType(GetType().Namespace + "." + reader.Read<FairOperationClass>()).GetConstructor([]).Invoke(null) as VotableOperation;
 		Proposal.Read(reader); 
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.WriteNullable(Creator);
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
                                    var d = execution.Disputes.Find(i);

									if(d.Flags.HasFlag(DisputeFlags.Succeeded))
										return false;

                                    if(p.GetType() != d.Proposal.GetType())
                                        return false;
                                    
                                    return d.Proposal.Overlaps(Proposal);
                                }))
 		{
 			Error = AlreadyExists;
 			return;
 		}
 
		if(!execution.IsReferendum(p))
 		{
 			if(!RequireModeratorAccess(execution, s.Id, out var _))
 				return;
 
			s = execution.Sites.Affect(s.Id);

 			AllocateEntity(s);
			PayEnergyBySite(execution, s.Id);
 		}
 		else
 		{
 			if(!RequireAuthorMembership(execution, s.Id, Creator, out var _, out var a))
 				return;
 
			a = execution.Authors.Affect(a.Id);

 			AllocateEntity(a);
			PayEnergyByAuthor(execution, a.Id);
 		}
 
 		var d = execution.Disputes.Create(s);
 
 		d.Site       = Site;
        d.Text       = Text;
 		d.Proposal   = Proposal;
 		d.Expirtaion = execution.Time + Time.FromDays(30);
  
 		s = execution.Sites.Affect(s.Id);
 		s.Disputes = [..s.Disputes, d.Id];

	}
}