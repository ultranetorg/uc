using System.Text;

namespace Uccs.Fair;

public class ProposalCreation : FairOperation
{
	public AutoId				Site { get; set; }
	public AutoId				Creator { get; set; } /// Account Id for Moderators, Author Id for Author
	public Role					As { get; set; }
	public string				Text { get; set; }
	public VotableOperation	    Proposal { get; set; }
	
	public override string		Explanation => $"Site={Site}, Creator={Creator}, Proposal={{{Proposal}}}, Text={Text}";

	public ProposalCreation()
	{
	}

	public ProposalCreation(AutoId site, AutoId creator, Role creatorrole, VotableOperation proposal, string text = "")
	{
		Site = site;
		Creator = creator;
		As = creatorrole;
		Proposal = proposal;
		Text = text;
	}
	
	public override bool IsValid(McvNet net)
	{
		return Proposal.IsValid(net) && Text.Length < Fair.PostLengthMaximum;
	}

	public override void Read(BinaryReader reader)
	{
		Site		= reader.Read<AutoId>();
		Creator		= reader.Read<AutoId>();
		As			= reader.Read<Role>();
 		Text		= reader.ReadUtf8();

 		Proposal = GetType().Assembly.GetType(GetType().Namespace + "." + reader.Read<FairOperationClass>()).GetConstructor([]).Invoke(null) as VotableOperation;
 		Proposal.Read(reader); 
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(Creator);
		writer.Write(As);
 		writer.WriteUtf8(Text);

		writer.Write(Enum.Parse<FairOperationClass>(Proposal.GetType().Name));
		Proposal.Write(writer);
	}

	public override void Execute(FairExecution execution)
	{
        if(!SiteExists(execution, Site, out var s, out Error))
            return;

		Proposal.Site = s;
		Proposal.Signer = Signer;

 		if(!Proposal.ValidateProposal(execution, out Error))
 			return;
 
        var t = Enum.Parse<FairOperationClass>(Proposal.GetType().Name);

 		if(!s.ChangePolicies.TryGetValue(t, out var p))
 			throw new IntegrityException();
 
 		if(s.Proposals.Any(i =>  {
                                    var d = execution.Proposals.Find(i);

									//if(d.Flags.HasFlag(ProposalFlags.Succeeded))
									//	return false;

                                    if(p.GetType() != d.Operation.GetType())
                                        return false;
                                    
                                    return d.Operation.Overlaps(Proposal);
                                }))
 		{
 			Error = AlreadyExists;
 			return;
 		}

		s = execution.Sites.Affect(s.Id);

		if(As == Role.Member)
 		{
			if(!CanAccessAuthor(execution, Creator, out _, out Error))
				return;

			if(!IsMember(execution, s.Id, Creator, out var _, out var _, out Error))
				return;

			var a = execution.Authors.Affect(Creator);
			execution.PayCycleEnergy(a);
 		}
		else if(As == Role.Moderator && s.CreationPolicies[t].Contains(Role.Moderator))
 		{
			if(!CanAccessAccount(execution, Creator, out _, out Error))
				return;
			
			if(!IsModerator(execution, s.Id, Creator, out var _, out Error))
				return;

			execution.PayCycleEnergy(s);
 		}
 		else if(As == Role.Author && s.CreationPolicies[t].Contains(Role.Author))
 		{
			if(!CanAccessAuthor(execution, Creator, out var _, out Error))
				return;

			var a = execution.Authors.Affect(Creator);

			a.Energy -= s.AuthorRequestFee;
			s.Energy += s.AuthorRequestFee;

			execution.PayCycleEnergy(a);
 		}
		else
		{
			Error = Denied;
			return;
		}

		if(	s.ChangePolicies[t] == ChangePolicy.AnyModerator	&& IsModerator(execution, Creator, out _, out _) ||
			execution.IsDiscussion(s.ChangePolicies[t])			&& IsModerator(execution, Creator, out _, out _) && s.Moderators.Length == 1 ||
			execution.IsReferendum(s.ChangePolicies[t])			&& IsMember(execution, s.Id, Creator, out _, out _, out _) && s.Authors.Length == 1)
		{
			Proposal.Site = s;
			Proposal.Execute(execution);
		}
		else
		{
 			var d = execution.Proposals.Create(s);
 
 			d.Site			= Site;
			d.Creator		= Creator;
			d.As			= As;
			d.Text			= Text;
 			d.Operation		= Proposal;
 			d.Expiration	= execution.Time + Time.FromDays(30);
  
 			s = execution.Sites.Affect(s.Id);
			s.Proposals = [..s.Proposals, d.Id];

			if(As == Role.Moderator)
 			{
 				execution.Allocate(s, s, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));
 			}
			else if(As == Role.Member)
 			{
				var a = execution.Authors.Affect(Creator);
 				execution.Allocate(a, s, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));
 			}
 			else if(As == Role.Author)
 			{
				var a = execution.Authors.Affect(Creator);
 				execution.Allocate(a, s, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));
 			}
		}
	}
}