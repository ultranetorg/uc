using System.Text;

namespace Uccs.Fair;

public class PublicationRemoveFromChanged : VotableOperation
{
	public AutoId						Publication { get; set; }

	public override bool				IsValid(McvNet net) => true;
	public override string				Explanation => $"{Publication}";

	public PublicationRemoveFromChanged()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication	= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as PublicationUpdation;

		return o.Publication == Publication;
	}
	
	 public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(!PublicationExists(execution, Publication, out var p, out error))
			return false;

		return true;
	}

	public override void Execute(FairExecution execution)
	{
		Site.ChangedPublications = [..Site.ChangedPublications, Publication];
	}
}
