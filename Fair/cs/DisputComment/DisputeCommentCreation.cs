using System.Text;

namespace Uccs.Fair;

public class DisputeCommentCreation : FairOperation
{
	public AutoId				Dispute { get; set; }
	public AutoId				Author { get; set; }
	public string				Text { get; set; }

	public override bool		NonExistingSignerAllowed => true;
	public override string		Explanation => $"{GetType().Name} Author={Author}";

	public override bool		IsValid(McvNet net) => Text.Length <= (net as Fair).PostLengthMaximum;

	public override void Read(BinaryReader reader)
	{
		Dispute		= reader.Read<AutoId>();
		Text		= reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Dispute);
		writer.WriteUtf8(Text);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireDispute(execution, Dispute, out var d))
			return;

		if(!RequireAuthorMembership(execution, d.Site, Author, out var s, out var a))
			return;

		var c = execution.DisputeComments.Create(d);

		c.Dispute		= d.Id;
		c.Author		= Author;
		c.Text			= Text;
		c.Created		= execution.Time;

		d = execution.Disputes.Affect(d.Id);

		d.Comments = [..d.Comments, c.Id];

		PayEnergyByAuthor(execution, a.Id);
	}
}