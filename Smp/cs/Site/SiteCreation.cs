namespace Uccs.Smp;

public class SiteCreation : SmpOperation
{
	public SiteType				Type { get; set; }
	public string				Title { get; set; }

	public override bool		IsValid(Mcv mcv) => true; // !Changes.HasFlag(SiteChanges.Description) || (Data.Length <= Site.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}";

	public SiteCreation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Type	= reader.ReadEnum<SiteType>();
		Title	= reader.ReadUtf8();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.WriteEnum(Type);
		writer.Write(Title);
	}

	public override void Execute(SmpMcv mcv, SmpRound round)
	{
		var s = round.CreateSite(Signer);

		s.Type = Type;
		s.Title = Title;
		s.Moderators = [Signer.Id];

		Signer.Sites = Signer.Sites == null ? [s.Id] : [..Signer.Sites, s.Id];
	}
}
