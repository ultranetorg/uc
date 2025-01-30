namespace Uccs.Smp;

public class SiteDeletion : SmpOperation
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

	public override void Execute(SmpMcv mcv, SmpRound round)
	{
		if(RequireSiteAccess(round, Site, out var s) == false)
			return;

		round.AffectSite(Site).Deleted = true;
		
		foreach(var i in s.Categories)
		{
			var c = round.AffectCategory(i);
			c.Deleted = true;

			foreach(var j in c.Publications)
			{
				var p = round.AffectPublication(j);
				p.Deleted = true;
			}
		}

		foreach(var i in s.Moderators)
		{
			var a = round.AffectAccount(i);
			a.Sites = a.Sites.Where(i => i != Site).ToArray();
		}
		//Free(d, r.Length);
	}
}