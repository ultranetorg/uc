namespace Uccs.Fair;

public class SiteDeletion : FairOperation
{
	public EntityId				Site { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"{Id}";

	public SiteDeletion()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Site = reader.Read<EntityId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Site);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(RequireSiteModeratorAccess(execution, Site, out var s) == false)
			return;

		s = execution.AffectSite(Site);
		s.Deleted = true;
		
		foreach(var i in s.Categories)
		{
			var c = execution.AffectCategory(i);
			c.Deleted = true;

			foreach(var j in c.Publications)
			{
				var p = execution.AffectPublication(j);
				p.Deleted = true;
			}
		}

		foreach(var i in s.Moderators)
		{
			var a = execution.AffectAccount(i);
			a.Sites = a.Sites.Where(i => i != Site).ToArray();
		}
		
		///TODO: Free(round, Signer, s, s.Space);
	}
}