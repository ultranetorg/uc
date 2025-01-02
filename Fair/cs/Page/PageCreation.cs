namespace Uccs.Fair;

public class PageCreation : FairOperation
{
	public EntityId				Site { get; set; }

	public override bool		IsValid(Mcv mcv) => true; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}";

	public PageCreation()
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
		if(RequireSiteAccess(round, Site, out var s) == false)
			return;

		var p = round.AffectPage(s);

		p.Site = Site;

		s = round.AffectSite(s.Id);

		s.Roots = s.Roots == null ? [p.Id] : [..s.Roots, p.Id];
	}
}