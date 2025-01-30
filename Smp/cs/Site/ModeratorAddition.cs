namespace Uccs.Smp;

public class ModeratorAddition : SmpOperation
{
	public EntityId				Site { get; set; }
	public EntityId				Candidate { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

	public ModeratorAddition()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Site = reader.Read<EntityId>();
		Candidate = reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(Candidate);
	}

	public override void Execute(SmpMcv mcv, SmpRound round)
	{
		if(!RequireSiteAccess(round, Site, out var s))
			return;

		s = round.AffectSite(Site);

		s.Moderators = [..s.Moderators, Candidate];
	}
}