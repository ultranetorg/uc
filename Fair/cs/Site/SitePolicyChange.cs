namespace Uccs.Fair;

public class SitePolicyChange : VotableOperation
{
	public EntityId				Site { get; set; }
	public FairOperationClass	Change { get; set; }
	public ChangePolicy			Policy { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

 	public override bool ValidProposal(FairExecution execution, SiteEntry site)
 	{
		return site.ChangePolicies.TryGetValue(Change, out var p) && p != Policy;
 	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as SitePolicyChange;
		
		return o.Change == Change;
	}
	
	public override void ReadConfirmed(BinaryReader reader)
	{
		Site	= reader.Read<EntityId>();
		Change	= reader.Read<FairOperationClass>();
		Policy	= reader.Read<ChangePolicy>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(Change);
		writer.Write(Policy);
	}

	public override void Execute(FairExecution execution)
	{
		Error = Denied;
	}

	public override void Execute(FairExecution execution, SiteEntry site)
	{
 		site = execution.AffectSite(Site);
 
		site.ChangePolicies = new(site.ChangePolicies);
		site.ChangePolicies[Change] = Policy;
	}
}