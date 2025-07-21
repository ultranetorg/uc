using System.Text;

namespace Uccs.Fair;

public class PublicationDeletion : VotableOperation
{
	public AutoId				Publication { get; set; }

	public override bool		IsValid(McvNet net) => Publication != null;
	public override string		Explanation => $"Publication={Publication}";

	public PublicationDeletion()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication = reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as PublicationDeletion;

		return o.Publication == Publication;
	}
	
	public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(!PublicationExists(execution, Publication, out _, out error))
			return false;

		return true;
	}

	public override void Execute(FairExecution execution)
	{
		execution.Publications.Delete(Publication);
	}
}