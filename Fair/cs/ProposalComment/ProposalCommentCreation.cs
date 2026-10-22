using System.Text;

namespace Uccs.Fair;

public class ProposalCommentCreation : FairOperation
{
	public AutoId				Proposal { get; set; }
	public AutoId				Author { get; set; }
	public string				Text { get; set; }

	public override string		Explanation => $"{GetType().Name} Author={Author}";

	public override bool		IsValid(McvNet net) => Text.Length <= Fair.PostLengthMaximum;

	public override void Read(Reader reader)
	{
		Proposal	= reader.Read<AutoId>();
		Author		= reader.ReadNullable<AutoId>();
		Text		= reader.ReadUtf8();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Proposal);
		writer.WriteNullable(Author);
		writer.WriteUtf8(Text);
	}

	public override void Execute(FairExecution execution)
	{
		if(!ProposalExists(execution, Proposal, out var d, out Error))
			return;

		var c = execution.ProposalComments.Create(d);

		c.Proposal		= d.Id;
		c.Text			= Text;
		c.Created		= execution.Time;

		d = execution.Proposals.Affect(d.Id);
		d.Comments = [..d.Comments, c.Id];

		var s = execution.Stores.Affect(d.Store);

		if(s.IsDiscussion(d.OptionClass))
 		{
 			if(!IsModerator(execution, s.Id, out var _, out Error))
 				return;

			c.Creator	= User.Id;
			c.As		= Role.Moderator;

 			execution.Allocate(s, s, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));
			execution.PayOperationEnergy(s);
		
			return;
 		}
 		else if(s.IsReferendum(d.OptionClass))
 		{
 			if(IsPublisher(execution, s, Author, out var p, out Error))
 			{	
				c.Creator	= Author;
				c.As		= Role.Publisher;

				var a = execution.Authors.Affect(p.Author);

 				execution.Allocate(a, a, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));
				execution.PayOperationEnergy(a);
			
				return;
			}
			else if(IsModerator(execution, s.Id, out var _, out Error))
			{	
				c.Creator	= User.Id;
				c.As		= Role.Moderator;

 				execution.Allocate(s, s, execution.Net.EntityLength + Encoding.UTF8.GetByteCount(Text));
				execution.PayOperationEnergy(s);
			
				return;
			}
 
 		}

		Error = Denied;
	}
}