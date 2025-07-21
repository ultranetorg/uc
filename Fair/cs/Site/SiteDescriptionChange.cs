using System.Text;

namespace Uccs.Fair;

public class SiteDescriptionChange : VotableOperation
{
	public string				Description { get; set; }

	public override bool		IsValid(McvNet net) => Description.Length <= Fair.PostLengthMaximum;
	public override string		Explanation => $"{Site}, {Description}";
	
	public override void Read(BinaryReader reader)
	{
		Description	= reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(Description);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return false;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
		Site.Description = Description;

		execution.Allocate(Site, Site, Encoding.UTF8.GetByteCount(Description));
	}
}