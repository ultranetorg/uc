using System.Text;

namespace Uccs.Fair;

public class ProposalCreation : FairOperation
{
	public AutoId				Site { get; set; }
	public AutoId				By { get; set; } /// Account Id for Moderators, Author Id for Author
	public Role					As { get; set; }
	public string				Text { get; set; }
	public VotableOperation	    Option { get; set; }

	public override string		Explanation => $"Site={Site}, Creator={By}, Proposal={{{Option}}}, Text={Text}";

	public ProposalCreation()
	{
	}

	public ProposalCreation(AutoId site, AutoId creator, Role creatorrole, VotableOperation proposal, string text = "")
	{
		Site = site;
		By = creator;
		As = creatorrole;
		Option = proposal;
		Text = text;
	}
	
	public override bool IsValid(McvNet net)
	{
		return Option.IsValid(net) && Text.Length < Fair.PostLengthMaximum;
	}

	public override void Read(BinaryReader reader)
	{
		Site		= reader.Read<AutoId>();
		By		= reader.Read<AutoId>();
		As			= reader.Read<Role>();
 		Text		= reader.ReadUtf8();

 		Option = GetType().Assembly.GetType(GetType().Namespace + "." + reader.Read<FairOperationClass>()).GetConstructor([]).Invoke(null) as VotableOperation;
 		Option.Read(reader); 
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(By);
		writer.Write(As);
 		writer.WriteUtf8(Text);

		writer.Write(Enum.Parse<FairOperationClass>(Option.GetType().Name));
		Option.Write(writer);
	}

	public override void Execute(FairExecution execution)
	{
        if(!SiteExists(execution, Site, out var s, out Error))
            return;

		Option.Site = s;
		Option.Signer = Signer;

 		if(!Option.ValidateProposal(execution, out Error))
 			return;
 
        var t = Enum.Parse<FairOperationClass>(Option.GetType().Name);

 		if(!s.ApprovalPolicies.TryGetValue(t, out var p))
 			throw new IntegrityException();
 
 		if(s.Proposals.Any(i =>  {
                                    var d = execution.Proposals.Find(i);

                                    if(p.GetType() != d.Option.GetType())
                                        return false;
                                    
                                    return d.Option.Overlaps(Option);
                                }))
 		{
 			Error = AlreadyExists;
 			return;
 		}

		s = execution.Sites.Affect(s.Id);

		if(As == Role.Publisher)
 		{
			if(!CanAccessAuthor(execution, By, out _, out Error))
				return;

			if(!IsPublisher(execution, s.Id, By, out var _, out var _, out Error))
				return;

			var a = execution.Authors.Affect(By);
			execution.PayCycleEnergy(a);
 		}
		else if(As == Role.Moderator && s.CreationPolicies[t].Contains(Role.Moderator))
 		{
			if(!CanAccessAccount(execution, By, out _, out Error))
				return;
			
			if(!IsModerator(execution, s.Id, By, out var _, out Error))
				return;

			execution.PayCycleEnergy(s);
 		}
 		else if(As == Role.Author && s.CreationPolicies[t].Contains(Role.Author))
 		{
			if(!CanAccessAuthor(execution, By, out var _, out Error))
				return;

			var a = execution.Authors.Affect(By);

			a.Energy -= s.AuthorRequestFee;
			s.Energy += s.AuthorRequestFee;

			execution.PayCycleEnergy(a);
 		}
		else if(As == Role.User && s.CreationPolicies[t].Contains(Role.User))
		{
			if(!CanAccessAccount(execution, By, out var _, out Error))
				return;
		
			execution.PayCycleEnergy(s);
		}
		else
		{
			Error = Denied;
			return;
		}

		if(	s.ApprovalPolicies[t] == ApprovalPolicy.AnyModerator	&& IsModerator(execution, By, out _, out _) ||
			s.IsDiscussion(t)										&& IsModerator(execution, By, out _, out _) && s.Moderators.Length == 1 ||
			s.IsReferendum(t)										&& IsPublisher(execution, s.Id, By, out _, out _, out _) && s.Authors.Length == 1)
		{
			Option.Site		= s;
			Option.As		= As;
			Option.By		= As == Role.User ? Signer.Id : By;

			Option.Execute(execution);
		}
		else
		{
 			var z = execution.Proposals.Create(s);
 
 			z.Site			= Site;
			z.By			= As == Role.User ? Signer.Id : By;
			z.As			= As;
			z.Text			= Text;
 			z.Option		= Option;
 			z.Expiration	= execution.Time + Time.FromDays(30);
  
			s.Proposals = [..s.Proposals, z.Id];

			if(As == Role.Moderator || As == Role.User)
 			{
 				execution.Allocate(s, s, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));
 			}
			else if(As == Role.Publisher || As == Role.Author)
 			{
				var a = execution.Authors.Affect(By);
 				execution.Allocate(a, s, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));
 			}
		}
	}
}