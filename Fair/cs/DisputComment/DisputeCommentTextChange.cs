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
		if(!RequireDisputeCommentOwnerAccess(execution, Comment, out var s, out var a, out var d, out var c))
			return;

		c = execution.DisputeComments.Affect(Comment);
		a = execution.Authors.Affect(a.Id);

		Free(execution, a, a, Encoding.UTF8.GetByteCount(c.Text));
		
		c.Text = Text;
		
		Allocate(execution, a, a, Encoding.UTF8.GetByteCount(Text));

		PayEnergyByAuthor(execution, a.Id);
	}
}
