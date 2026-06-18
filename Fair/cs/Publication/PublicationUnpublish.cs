namespace Uccs.Fair;

public class PublicationUnpublish : VotableOperation
{
	public AutoId				Publication { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"{Publication}]";

	public PublicationUnpublish()
	{
	}

	public override void Read(Reader reader)
	{
		Publication	= reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Publication);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as PublicationUnpublish).Publication == Publication;
	}
	
	 public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(!PublicationExists(execution, Publication, out var p, out error))
			return false;

		return true;
	}

	public override void Execute(FairExecution execution)
	{
		execution.Unpublish(Site, Publication, out Error);	
	}
}
