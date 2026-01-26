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

	public override void Read(BinaryReader reader)
	{
		Comment	= reader.Read<AutoId>();
		Text	= reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Comment);
		writer.WriteUtf8(Text);
	}

	public override void Execute(FairExecution execution)
	{
		var c = execution.ProposalComments.Affect(Comment);
		var d = execution.Proposals.Find(c.Proposal);
		var s = execution.Sites.Find(d.Site);

		if(s.IsDiscussion(d.OptionClass))
 		{
 			if(!IsModerator(execution, d.Site, out _, out Error))
 				return;
 
			s = execution.Sites.Affect(s.Id);

			execution.Free(s, s, Encoding.UTF8.GetByteCount(c.Text));
 			execution.Allocate(s, s, Encoding.UTF8.GetByteCount(Text));
		
			execution.PayOperationEnergy(s);
 		}
 		else
 		{
			if(!IsReferendumCommentOwner(execution, s, Comment, out var m, out var _, out var _, out Error))
				return;

			var a = execution.Authors.Affect(m.Author);

			execution.Free(a, a, Encoding.UTF8.GetByteCount(c.Text));
 			execution.Allocate(a, a, Encoding.UTF8.GetByteCount(Text));

			execution.PayOperationEnergy(a);
 		}

		c.Text = Text; /// after all above
	}
}
