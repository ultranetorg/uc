using System.Text;

namespace Uccs.Fair;

public class Option : IBinarySerializable
{
	public string				Title { get; set; }
	public VotableOperation	    Operation { get; set; }

	public Option()
	{
	}

	public Option(VotableOperation operation, string text = "")
	{
		Operation = operation;
		Title = text;
	}

	public virtual void Read(BinaryReader reader)
	{
 		Title = reader.ReadUtf8();

		Operation = Fair.OContructors[typeof(Operation)][reader.ReadUInt32()].Invoke(null) as VotableOperation;
 		Operation.Read(reader); 
	}

	public virtual void Write(BinaryWriter writer)
	{
 		writer.WriteUtf8(Title);

		writer.Write(Fair.OCodes[Operation.GetType()]);
		Operation.Write(writer);
	}
}

public class ProposalCreation : FairOperation
{
	public AutoId				Site { get; set; }
	public AutoId				By { get; set; } /// Account Id for Moderators, Author Id for Author
	public Role					As { get; set; }
	public string				Title { get; set; }
	public string				Text { get; set; }
	public Option[]				Options { get; set; }

	public override string		Explanation => $"Site={Site}, By={By}, As={As}, Option={{{Options}}}, Text={Text}";

	public ProposalCreation()
	{
	}

	public ProposalCreation(AutoId site, AutoId creator, Role creatorrole, VotableOperation operation, string title = "", string text = "")
	{
		Site = site;
		By = creator;
		As = creatorrole;
		Options = [new Option(operation)];
		Title = title;
		Text = text;
	}

	public ProposalCreation(AutoId site, AutoId creator, Role creatorrole, Option[] options, string title = "", string text = "")
	{
		Site = site;
		By = creator;
		As = creatorrole;
		Options = options;
		Title = title;
		Text = text;
	}
	
	public override bool IsValid(McvNet net)
	{
		var e =	Title.Length <= Fair.TitleLengthMaximum &&
				(Text == null || Text.Length <= Fair.PostLengthMaximum) &&
				Options.Length > 0 &&
				Enum.IsDefined<Role>(As) &&
				Options.All(i => i.Operation.GetType() == Options[0].Operation.GetType() && i.Operation.IsValid(net) && i.Title.Length <= Fair.TitleLengthMaximum);

		return e;
	}

	public override void PreTransact(McvNode node, Flow flow)
	{
		foreach(var i in Options)
			i.Operation.PreTransact(node, flow);

		base.PreTransact(node, flow);
	}

	public override void Read(BinaryReader reader)
	{
		Site		= reader.Read<AutoId>();
		By			= reader.Read<AutoId>();
		As			= reader.Read<Role>();
		Title		= reader.ReadUtf8();
 		Text		= reader.ReadUtf8();
		Options		= reader.ReadArray<Option>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(By);
		writer.Write(As);
		writer.WriteUtf8(Title);
 		writer.WriteUtf8(Text);
		writer.Write(Options);
	}

	public override void Execute(FairExecution execution)
	{
        if(!SiteExists(execution, Site, out var s, out Error))
            return;

		foreach(var i in Options)
		{
			i.Operation.Site = s;
			i.Operation.User = User;
		}

 		if(!Options.All(i => i.Operation.ValidateProposal(execution, out Error)))
 			return;
 
        var c = (FairOperationClass)Fair.OCodes[Options[0].Operation.GetType()];

		var p = s.Policies.First(i => i.OperationClass == c);
 		///if(p == null)
 		///	throw new IntegrityException();
 
 		if(s.Proposals.Any(i =>  {
									var d = execution.Proposals.Find(i);

									if(d.OptionClass != c)
										return false;

									foreach(var a in d.Options)
										foreach(var b in Options)
											if(a.Operation.Overlaps(b.Operation))
												return true;

									return false;
								}))
 		{
 			Error = AlreadyExists;
 			return;
 		}

		s = execution.Sites.Affect(s.Id);

		if(As == Role.Publisher && p.Creators.HasFlag(Role.Publisher))
 		{
			if(!CanAccessAuthor(execution, By, out var a, out Error))
				return;

			if(!IsPublisher(execution, s, By, out var x, out Error))
				return;

			if(x.BannedTill > execution.Time)
			{
				Error = Denied;
				return;
			}

			a = execution.Authors.Affect(By);
			execution.PayOperationEnergy(a);
 		}
		else if(As == Role.Moderator && p.Creators.HasFlag(Role.Moderator))
 		{
			if(!CanAccessAccount(execution, By, out _, out Error))
				return;
			
			if(!IsModerator(execution, s, By, out var m, out Error))
				return;

			if(m.BannedTill > execution.Time)
			{
				Error = Denied;
				return;
			}

			execution.PayOperationEnergy(s);
 		}
 		else if(As == Role.Candidate && p.Creators.HasFlag(Role.Candidate))
 		{
			if(!CanAccessAuthor(execution, By, out var _, out Error))
				return;

			var a = execution.Authors.Affect(By);

			a.Energy -= s.CandidateRequestFee;
			s.Energy += s.CandidateRequestFee;

			execution.PayOperationEnergy(a);
 		}
		else if(As == Role.User && p.Creators.HasFlag(Role.User))
		{
			if(!CanAccessAccount(execution, By, out var _, out Error))
				return;
		
			execution.PayOperationEnergy(s);
		}
		else
		{
			Error = Denied;
			return;
		}

		if(Options.Length == 1 &&  (p.Approval == ApprovalRequirement.AnyModerator	&& IsModerator(execution, Site, out _, out _) ||
									s.IsDiscussion(c)								&& IsModerator(execution, Site, out _, out _) && s.Moderators.Length == 1 ||
									s.IsReferendum(c)								&& IsPublisher(execution, s, By, out _, out _) && s.Publishers.Length == 1))
		{
			Options[0].Operation.Site	= s;
			Options[0].Operation.As		= As;
			Options[0].Operation.By		= As == Role.User ? User.Id : By;

			Options[0].Operation.Execute(execution);
		}
		else
		{
 			var z = execution.Proposals.Create(s);
 
 			z.Site			= Site;
			z.By			= As == Role.User ? User.Id : By;
			z.As			= As;
			z.Title			= Title;
			z.Text			= Text;
			z.Neither		= [];
			z.Any			= [];
			z.Ban			= [];
			z.Banish		= [];
 			z.Options		= Options.Select(i => new ProposalOption(i)).ToArray();
 			z.CreationTime	= execution.Time;
  
			s.Proposals = [..s.Proposals, z.Id];

			var l = execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text) + Options.Sum(i => Encoding.UTF8.GetByteCount(i.Title));

			if(As == Role.Moderator || As == Role.User)
 			{
 				execution.Allocate(s, s, l);
 			}
			else if(As == Role.Publisher)
 			{
				var a = execution.Authors.Affect(By);
				var pb = s.Publishers.First(i => i.Author == By);
 				execution.Allocate(a, pb, l, out Error);
 			}
			else if(As == Role.Candidate)
 			{
				var a = execution.Authors.Affect(By);
 				execution.Allocate(a, a, l);
 			}
		}

		foreach(var i in s.Proposals.Select(i => execution.Proposals.Find(i)).Where(i => {
																							var oc = (FairOperationClass)Fair.OCodes[i.Options[0].Operation.GetType()];
																							
																							return	s.Policies.First(i => i.OperationClass == oc).Approval == ApprovalRequirement.PublishersMajority && 
																									!Uccs.Fair.Site.Restrictions.First(i => i.OperationClass == oc).Flags.HasFlag(PolicyFlag.Infinite) && 
																									execution.Time - i.CreationTime > Time.FromDays(30);
																						}))
		{
			execution.Proposals.Delete(s, i);
		}
	}
}