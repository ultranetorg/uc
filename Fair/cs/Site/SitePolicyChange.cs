namespace Uccs.Fair;

public class SitePolicyChange : FairOperation
{
	public EntityId				Site { get; set; }
	public FairOperationClass	Change { get; set; }
	public ChangePolicy			Policy { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

 	public override bool ValidProposal(FairMcv mcv, FairRound round, Site site)
 	{
		return site.ChangePolicies.TryGetValue(Change, out var p) && p != Policy;
 	}

	public bool Overlaps(Operation other)
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

	public override void Execute(FairMcv mcv, FairRound round)
	{
 		if(!RequireSiteAccess(round, Site, out var s))
 			return;

 		if(s.ChangePolicies[FairOperationClass.SitePolicyChange] != ChangePolicy.AnyModerator)
 		{
 			Error = Denied;
 			return;
 		}

 		Execute(mcv, round, s);
	}

	public override void Execute(FairMcv mcv, FairRound round, SiteEntry site)
	{
 		site = round.AffectSite(Site);
 
		site.ChangePolicies = new(site.ChangePolicies);
		site.ChangePolicies[Change] = Policy;
	}
}