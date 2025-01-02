namespace Uccs.Fair;

public class PageDeletion : FairOperation
{
	public EntityId				Page { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

	public PageDeletion()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Page = reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Page);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(RequirePageAccess(round, Page, out var s, out var p) == false)
			return;

		round.AffectPage(Page).Deleted = true;
		
		s = round.AffectSite(s.Id);
		s.Roots = s.Roots.Where(i => i != Page).ToArray();

		//Free(d, r.Length);
	}
}