using System.Text;

namespace Uccs.Fair;

public class ProposalCommentEdit : FairOperation
{
	public AutoId				Comment { get; set; }
	public string				Text { get; set; }

	public override bool		IsValid(McvNet net) => Text.Length <= Fair.PostLengthMaximum;
	public override string		Explanation => $"{Comment}, {Text}";

	public ProposalCommentEdit()
	{
	}

	public override void Read(Reader reader)
	{
		Comment	= reader.Read<AutoId>();
		Text	= reader.ReadUtf8();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Comment);
		writer.WriteUtf8(Text);
	}

	public override void Execute(FairExecution execution)
	{
		var c = execution.ProposalComments.Affect(Comment);
		var d = execution.Proposals.Find(c.Proposal);
		var s = execution.Stores.Find(d.Store);

		if(c.As == Role.Moderator)
 		{
 			if(!IsModerator(execution, s.Id, out _, out Error))
 				return;

 			if(c.Creator != User.Id)
 			{	
				Error = Denied;
				return;
			}
			 
			s = execution.Stores.Affect(s.Id);

			execution.Free(s, s, Encoding.UTF8.GetByteCount(c.Text));
 			execution.Allocate(s, s, Encoding.UTF8.GetByteCount(Text));
			execution.PayOperationEnergy(s);
 		}
 		else if(c.As == Role.Publisher)
 		{
			if(!IsReferendumCommentOwner(execution, s, Comment, out var p, out var _, out var _, out Error))
				return;

			var a = execution.Authors.Affect(p.Author);

			execution.Free(a, a, Encoding.UTF8.GetByteCount(c.Text));
 			execution.Allocate(a, a, Encoding.UTF8.GetByteCount(Text));
			execution.PayOperationEnergy(a);
 		}
		else
			throw new IntegrityException();

		c.Text = Text; /// after all above
	}
}
