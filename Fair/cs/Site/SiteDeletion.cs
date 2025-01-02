namespace Uccs.Fair;

public class SiteDeletion : FairOperation
{
	public EntityId				Site { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

	public SiteDeletion()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Site = reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Site);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(RequireSiteAccess(round, Site, out var c) == false)
			return;

		round.AffectSite(Site).Deleted = true;
		
		foreach(var i in c.Roots)
		{
			round.AffectPage(i).Deleted = true;
		}

		foreach(var i in c.Owners)
		{
			var a = round.AffectAccount(i);
			a.Sites = a.Sites.Where(i => i != Site).ToArray();
		}
		//Free(d, r.Length);
	}
}