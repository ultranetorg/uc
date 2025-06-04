using System.Text;

namespace Uccs.Fair;

public class DisputeCommentTextChange : FairOperation
{
	public AutoId				Comment { get; set; }
	public string				Text { get; set; }

	public override bool		IsValid(McvNet net) => Text.Length <= (net as Fair).PostLengthMaximum;
	public override string		Explanation => $"{Comment}, {Text}";

	public DisputeCommentTextChange()
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

	public override void Execute(FairExecution execution, bool dispute)
	{
		var c = execution.DisputeComments.Affect(Comment);
		var d = execution.Disputes.Find(c.Dispute);

		if(!execution.IsReferendum(d))
 		{
 			if(!RequireModeratorAccess(execution, d.Site, out var s))
 				return;
 
			s = execution.Sites.Affect(s.Id);

			Free(execution, s, s, Encoding.UTF8.GetByteCount(c.Text));
 			Allocate(execution, s, s, Encoding.UTF8.GetByteCount(Text));
		
			PayEnergyBySite(execution, s.Id);
 		}
 		else
 		{
			if(!RequireReferendumCommentAuthorAccess(execution, Comment, out var s, out var a, out var _, out var _))
				return;

			a = execution.Authors.Affect(a.Id);

			Free(execution, a, a, Encoding.UTF8.GetByteCount(c.Text));
 			Allocate(execution, a, a, Encoding.UTF8.GetByteCount(Text));

			PayEnergyByAuthor(execution, a.Id);
 		}

		c.Text = Text; /// after all above
	}
}
