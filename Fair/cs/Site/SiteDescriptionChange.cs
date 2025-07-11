using System.Text;

namespace Uccs.Fair;

public class SiteDescriptionChange : VotableOperation
{
	public string				Description { get; set; }

	public override bool		IsValid(McvNet net) => Description.Length < 1024;
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

 	public override bool ValidateProposal(FairExecution execution)
 	{
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
 		var s = execution.Sites.Affect(Site.Id);
 
		s.Description = Description;

		execution.Allocate(s, s, Encoding.UTF8.GetByteCount(Description));
	}
}