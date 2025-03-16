namespace Uccs.Fair;

public class SiteAuthorsChange : FairOperation
{
	public EntityId				Site { get; set; }
	public EntityId[]			Additions { get; set; }
	public EntityId[]			Removals { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

 	public override bool ValidProposal(FairMcv mcv, FairRound round, Site site)
 	{
		foreach(var i in Additions)
			if(site.Authors.Contains(i))
				return false;
	
		foreach(var i in Removals)
			if(!site.Authors.Contains(i))
				return false;
	
		return true;
 	}

	public bool Overlaps(Operation other)
	{
		var o = other as SiteAuthorsChange;
		
		foreach(var i in Additions)
			if(o.Additions.Contains(i) || o.Removals.Contains(i))
				return true;
	
		foreach(var i in Removals)
			if(o.Additions.Contains(i) || o.Removals.Contains(i))
				return true;
				
		return false;
	}
	
	public override void ReadConfirmed(BinaryReader reader)
	{
		Site = reader.Read<EntityId>();
		Additions = reader.ReadArray<EntityId>();
		Removals = reader.ReadArray<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(Additions);
		writer.Write(Removals);
	}

	//public bool Valid(SiteEntry site, FairRound round)
	//{
	//	foreach(var i in Additions)
	//		if(site.Authors.Contains(i))
	//			return false;
	//
	//	foreach(var i in Removals)
	//		if(!site.Authors.Contains(i))
	//			return false;
	//
	//	return true;
	//}

	public override void Execute(FairMcv mcv, FairRound round)
	{
 		if(!RequireSiteAccess(round, Site, out var s))
 			return;

 		if(s.ChangePolicies[FairOperationClass.SiteAuthorsChange] != ChangePolicy.AnyModerator)
 		{
 			Error = Denied;
 			return;
 		}

 		Execute(mcv, round, s);
	}

	public override void Execute(FairMcv mcv, FairRound round, SiteEntry site)
	{
 		var s = round.AffectSite(Site);
 
 		s.Authors = [..s.Authors, ..Additions];
 
 		foreach(var i in Removals)
 			s.Authors = s.Authors.Remove(i);
	}
}