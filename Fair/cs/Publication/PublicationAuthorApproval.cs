namespace Uccs.Fair;

public class PublicationAuthorApproval : FairOperation
{
	public AutoId				Publication { get; set; }
	public bool					Approved { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"{Publication}, [{Approved}]";

	public PublicationAuthorApproval()
	{
	}

	public override void Read(Reader reader)
	{
		Publication	= reader.Read<AutoId>();
		Approved	= reader.ReadBoolean();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Publication);
		writer.Write(Approved);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessPublication(execution, Publication, out var p, out var a, out var r, out Error))
			return;

		p = execution.Publications.Affect(p.Id);

		if(Approved)
			p.Flags |= PublicationFlags.ApprovedByAuthor;
		else
		{	
			p.Flags &= ~PublicationFlags.ApprovedByAuthor;
			
			var s = execution.Sites.Affect(p.Site);
			execution.Unpublish(s, Publication, out Error);
		}

		a = execution.Authors.Affect(a.Id);
		execution.PayOperationEnergy(a);
	}
}
