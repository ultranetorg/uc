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
		Author		= reader.ReadNullable<AutoId>();
		Text		= reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Dispute);
		writer.WriteNullable(Author);
		writer.WriteUtf8(Text);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireDispute(execution, Dispute, out var d))
			return;

		var c = execution.DisputeComments.Create(d);

		c.Dispute		= d.Id;
		c.Text			= Text;
		c.Created		= execution.Time;

		d = execution.Disputes.Affect(d.Id);
		d.Comments = [..d.Comments, c.Id];

		var s = execution.Sites.Find(d.Site);

		if(!execution.IsReferendum(d))
 		{
 			if(!RequireModeratorAccess(execution, s.Id, out var _))
 				return;

			c.Creator = Signer.Id;

			s = execution.Sites.Affect(s.Id);

 			AllocateEntity(s);
 			Allocate(execution, s, s, Encoding.UTF8.GetByteCount(Text));
			PayEnergyBySite(execution, s.Id);
 		}
 		else
 		{
 			if(!RequireAuthorMembership(execution, s.Id, Author, out var _, out var a))
 				return;
 
			c.Creator = Author;

			a = execution.Authors.Affect(a.Id);

 			AllocateEntity(a);
			Allocate(execution, a, a, Encoding.UTF8.GetByteCount(Text));
			PayEnergyByAuthor(execution, a.Id);
 		}
	}
}