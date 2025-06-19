namespace Uccs.Fair;

public class SiteDescriptionChange : VotableOperation
{
	public AutoId				Site { get; set; }
	public string				Description { get; set; }

	public override bool		IsValid(McvNet net) => Description.Length < 1024;
	public override string		Explanation => $"{Site}, {Description}";
	
	public override void Read(BinaryReader reader)
	{
		Site		= reader.Read<AutoId>();
		Description	= reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.WriteUtf8(Description);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return false;
	}

 	public override bool ValidProposal(FairExecution execution)
 	{
 		if(!RequireSite(execution, Site, out var s))
 			return false;

		return true;
 	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!ValidProposal(execution))
			return;

		if(!dispute)
	 	{
	 		if(!RequireModeratorAccess(execution, Site, out var x))
 				return;

	 		if(x.ChangePolicies[FairOperationClass.SiteDescriptionChange] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
		}

 		var s = execution.Sites.Affect(Site);
 
		s.Description = Description;

		PayEnergyBySite(execution, s.Id);
	}
}